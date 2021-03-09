using ASPSecurityKit;
using Microsoft.Data.SqlClient;
using SuperFinance.DataModels;
using SuperFinance.Security;
using SuperFinance.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASKSource;
using ASKSource.Infrastructure;
using ASKSource.Managers;
using ASKSource.Models;
using ASKSource.Repositories;
using ASKSource.Security;
using Microsoft.EntityFrameworkCore;
using SuperFinance.Models;

namespace SuperFinance.Managers
{
	public interface IBankManager
	{
		Task<LoginResult> RegisterBankAsync(RegisterBankModel model, string verificationUrl, string contactUrl);
		Task<List<Bank>> ListBanksAsync();
		Task<Bank> GetBankAsync(Guid? bankId);
		Task SetFirewallStatusAsync(Guid? bankId, bool enabled);
		Task SetMFAPolicyAsync(Guid? bankId, bool enforce, bool skipMFAInsideNetwork);
		Task SetPasswordExpirationPolicyAsync(Guid? bankId, int? passwordExpiresInDays);
	}

	public class BankManager : IBankManager
	{
		private readonly DemoDbContext dbContext;
		private readonly ILogger logger;
		private readonly ISFUserManager userManager;
		private readonly ISFUserService userService;
		private readonly IUserPermitRepository permitRepository;
		private readonly IFirewallManager firewallManager;

		public BankManager(DemoDbContext dbContext, ILogger logger, ISFUserManager userManager,
			IFirewallManager firewallManager, ISFUserService userService, IUserPermitRepository permitRepository)
		{
			this.dbContext = dbContext;
			this.logger = logger;
			this.userManager = userManager;
			this.firewallManager = firewallManager;
			this.userService = userService;
			this.permitRepository = permitRepository;
		}

		public async Task<List<Bank>> ListBanksAsync()
		{
			return await this.dbContext.Banks.Select(x => new Bank { Id = x.Id, Name = x.Name, Address = x.Address })
				.ToListAsync().ConfigureAwait(false);
		}

		public async Task<Bank> GetBankAsync(Guid? bankId)
		{
			var bank = await this.dbContext.Banks.Where(x => x.Id == bankId)
				.Select(x => new Bank
				{
					Id = x.Id,
					Name = x.Name,
					Address = x.Address,
					FirewallEnabled = x.FirewallEnabled,
					EnforceMFA = x.EnforceMFA,
					SkipMFAInsideNetwork = x.SkipMFAInsideNetwork,
					PasswordExpiresInDays = x.PasswordExpiresInDays
				})
				.SingleOrDefaultAsync().ConfigureAwait(false);

			return bank;
		}

		public async Task SetFirewallStatusAsync(Guid? bankId, bool enabled)
		{
			var dbBank = await this.dbContext.Banks
				.Where(m => m.Id == bankId)
				.SingleOrDefaultAsync()
				.ConfigureAwait(false);

			if (dbBank == null)
			{
				await this.logger.WarnAsync("{0} doesn't exist", bankId).ConfigureAwait(false);
				throw new OpException(OpResult.DoNotExist);
			}

			if (enabled)
			{
				if (!await this.dbContext.FirewallRules
					.AnyAsync(x => x.EntityUrn == EntityUrn.MakeUrn(SFEntityTypes.Bank, dbBank.Id))
					.ConfigureAwait(false))
				{
					await this.logger.WarnAsync("No IP ranges has been found for bank {0}", bankId).ConfigureAwait(false);
					throw new OpException(OpResult.Failed, Messages.CannotEnableFirewall);
				}
			}

			dbBank.FirewallEnabled = enabled;
			await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task SetMFAPolicyAsync(Guid? bankId, bool enforce, bool skipMFAInsideNetwork)
		{
			var dbBank = await this.dbContext.Banks
				.Where(m => m.Id == bankId)
				.SingleOrDefaultAsync()
				.ConfigureAwait(false);

			if (dbBank == null)
			{
				await this.logger.WarnAsync("{0} doesn't exist", bankId).ConfigureAwait(false);
				throw new OpException(OpResult.DoNotExist);
			}

			dbBank.EnforceMFA = enforce;
			dbBank.SkipMFAInsideNetwork = skipMFAInsideNetwork;
			await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task SetPasswordExpirationPolicyAsync(Guid? bankId, int? passwordExpiresInDays)
		{
			var dbBank = await this.dbContext.Banks
				.Where(m => m.Id == bankId)
				.SingleOrDefaultAsync()
				.ConfigureAwait(false);

			if (dbBank == null)
			{
				await this.logger.WarnAsync("{0} doesn't exist", bankId).ConfigureAwait(false);
				throw new OpException(OpResult.DoNotExist);
			}

			dbBank.PasswordExpiresInDays = passwordExpiresInDays;
			await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task<LoginResult> RegisterBankAsync(RegisterBankModel model, string verificationUrl, string contactUrl)
		{
			try
			{
				var result = await this.userManager.RegisterStaffUserAsync(
					new AppUser
					{
						Name = "Administrator",
						Username = model.Username,
						Password = model.Password
					},
					verificationUrl, contactUrl, true);

				if (result != null && result.IsSuccess)
				{
					var dbBank = new DbBank
					{
						Id = Guid.NewGuid(),
						Name = model.Name,
						Address = model.Address,
						OwningUserId = this.userService.CurrentUserId
					};
					this.dbContext.Banks.Add(dbBank);

					await this.permitRepository
						.AddPermitAsync(dbBank.OwningUserId, SFPermissionCodes.BankOwner, dbBank.Id)
						.ConfigureAwait(false);

					await this.permitRepository
						.AddPermitAsync(dbBank.OwningUserId, PermissionCodes.AddUser, null)
						.ConfigureAwait(false);

					await this.dbContext.SaveChangesAsync().ConfigureAwait(false);

					// reload permissions to include permit granted on the new bank entity to the owner user.
					await this.userService.RefreshPermissionsAsync().ConfigureAwait(false);

					// set bankId in session store
					this.userService.BankId = dbBank.Id;
				}

				return result;
			}
			catch (OpException)
			{
				throw;
			}
			catch (Exception ex)
			{
				await this.logger.ErrorAsync(ex).ConfigureAwait(false);

				if (ex.GetBaseException() is SqlException sqlEx &&
					sqlEx.Number.In((int)SqlErrors.KeyViolation, (int)SqlErrors.UniqueIndex))
				{
					throw new OpException(OpResult.AlreadyExists,
						string.Format(SFMessages.CannotAddDuplicateBank, model.Name));
				}

				throw;
			}
		}
	}
}

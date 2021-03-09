using ASPSecurityKit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SuperFinance.DataModels;
using SuperFinance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASKSource.Models;
using ASKSource.Security;
using SuperFinance.Security;

namespace SuperFinance.Managers
{
	public interface IAccountTypeManager
	{
		Task<PagedResult<AccountType>> GetAccountTypesAsync(int startIndex, int pageSize);

		Task<List<AccountType>> GetAccountTypesAsync(Guid? bankId, AccountKind? kind);

		Task<AccountType> AddAccountTypeAsync(AccountType accountType);

		Task<AccountType> EditAccountTypeAsync(AccountType accountType);

		Task DeleteAccountTypeAsync(Guid id);
	}

	public class AccountTypeManager : IAccountTypeManager
	{
		private readonly DemoDbContext dbContext;
		private readonly ILogger logger;
		private readonly ISFUserService userService;

		public AccountTypeManager(DemoDbContext dbContext, ISFUserService userService, ILogger logger)
		{
			this.dbContext = dbContext;
			this.userService = userService;
			this.logger = logger;
		}

		public async Task<AccountType> AddAccountTypeAsync(AccountType accountType)
		{
			try
			{
				var dbAccountType = new DbAccountType
				{
					Name = accountType.Name,
					InterestRate = accountType.InterestRate,
					Kind = accountType.Kind,
					BankId = this.userService.BankId.GetValueOrDefault()
				};

				this.dbContext.AccountTypes.Add(dbAccountType);
				await this.dbContext.SaveChangesAsync().ConfigureAwait(false);

				return new AccountType
				{
					Id = dbAccountType.Id,
					Name = dbAccountType.Name,
					InterestRate = dbAccountType.InterestRate,
					Kind = dbAccountType.Kind
				};
			}
			catch (Exception ex)
			{
				await this.logger.ErrorAsync(ex).ConfigureAwait(false);

				if (ex.GetBaseException() is SqlException sqlEx &&
					sqlEx.Number.In((int)SqlErrors.KeyViolation, (int)SqlErrors.UniqueIndex))
				{
					throw new OpException(OpResult.AlreadyExists,
						string.Format(SFMessages.CannotAddDuplicateAccountType, accountType.Name));
				}

				throw;
			}
		}

		public async Task DeleteAccountTypeAsync(Guid id)
		{
			var dbAccountType = await this.dbContext.AccountTypes
				.Where(m => m.Id == id)
				.SingleOrDefaultAsync()
				.ConfigureAwait(false);

			if (dbAccountType != null)
			{
				try
				{
					this.dbContext.AccountTypes.Remove(dbAccountType);
					await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					await this.logger.ErrorAsync(ex).ConfigureAwait(false);
					throw new OpException(OpResult.DBDeletionDenied, SFMessages.CannotDeleteAccountType);
				}
			}
			else
			{
				throw new OpException(OpResult.DoNotExist);
			}
		}

		public async Task<AccountType> EditAccountTypeAsync(AccountType accountType)
		{
			var dbAccountType = await this.dbContext.AccountTypes
				.Where(m => m.Id == accountType.Id)
				.SingleOrDefaultAsync()
				.ConfigureAwait(false);

			if (dbAccountType == null)
			{
				await this.logger.WarnAsync("{0} doesn't exist", accountType.Name).ConfigureAwait(false);
				throw new OpException(OpResult.DoNotExist);
			}

			try
			{
				dbAccountType.Name = accountType.Name;
				dbAccountType.InterestRate = accountType.InterestRate;
				dbAccountType.Kind = accountType.Kind;

				await this.dbContext.SaveChangesAsync().ConfigureAwait(false);

				return new AccountType
				{
					Id = dbAccountType.Id,
					Name = dbAccountType.Name,
					InterestRate = dbAccountType.InterestRate,
					Kind = dbAccountType.Kind
				};
			}
			catch (Exception ex)
			{
				await this.logger.ErrorAsync(ex).ConfigureAwait(false);

				if (ex.GetBaseException() is SqlException sqlEx &&
					sqlEx.Number.In((int)SqlErrors.KeyViolation, (int)SqlErrors.UniqueIndex))
				{
					throw new OpException(OpResult.AlreadyExists,
						string.Format(SFMessages.CannotAddDuplicateAccountType, accountType.Name));
				}

				throw;
			}
		}

		public async Task<PagedResult<AccountType>> GetAccountTypesAsync(int startIndex, int pageSize)
		{
			var bankId = this.userService.BankId;

			var list = await this.dbContext.AccountTypes.AsNoTracking()
				.Select(dummy => new
				{
					Total = this.dbContext.AccountTypes.Count(p => p.BankId == bankId),
					ThisPage = this.dbContext.AccountTypes.Where(p => p.BankId == bankId)
						.OrderBy(p => p.Name).Skip(startIndex).Take(pageSize)
						.Select(x => new AccountType
						{
							Id = x.Id,
							Name = x.Name,
							InterestRate = x.InterestRate,
							Kind = x.Kind
						})
						.ToList()
				}).FirstOrDefaultAsync().ConfigureAwait(false);

			return new PagedResult<AccountType>(list?.ThisPage.ToList(), startIndex, pageSize, list?.Total ?? 0);
		}

		public async Task<List<AccountType>> GetAccountTypesAsync(Guid? bankId, AccountKind? kind)
		{
			var dbAccountTypes = this.dbContext.AccountTypes.Where(x => x.BankId == bankId);

			if (kind != null)
			{
				dbAccountTypes = dbAccountTypes.Where(x => x.Kind == kind);
			}

			return await dbAccountTypes.OrderBy(p => p.Name)
						.Select(x => new AccountType
						{
							Id = x.Id,
							Name = x.Name,
							InterestRate = x.InterestRate,
							Kind = x.Kind
						})
						.ToListAsync().ConfigureAwait(false);
		}
	}
}

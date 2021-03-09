using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASKSource;
using ASKSource.Infrastructure;
using ASKSource.Managers;
using ASKSource.Models;
using ASKSource.Repositories;
using ASPSecurityKit;
using Microsoft.EntityFrameworkCore;
using SuperFinance.DataModels;
using SuperFinance.Security;

namespace SuperFinance.Managers
{
	public interface ISFUserManager : IUserManager
	{
		Task<AppUser> AddCustomerUserAsync(AppUser user, string verificationUrl, string contactUrl);

		Task<AppUser> AddStaffUserAsync(AppUser user, string verificationUrl, string contactUrl);

		Task<LoginResult> RegisterCustomerUserAsync(AppUser user, string verificationUrl, string contactUrl,
			bool createAuthCookie);

		Task<LoginResult> RegisterStaffUserAsync(AppUser user, string verificationUrl, string contactUrl,
			bool createAuthCookie);
	}

	public class SFUserManager : UserManager, ISFUserManager
	{
		private readonly DemoDbContext dbContext;
		private readonly IAppContext appContext;
		private readonly ISFUserService userService;
		private readonly ILogger logger;
		private readonly IMailSender mailer;
		private readonly IUserPermitRepository permitRepository;

		public SFUserManager(ISFUserService userService, DemoDbContext dbContext,
			IAppContext appContext, ISecuritySettings securitySettings,
			ILocaleManager localeManager, ILogger logger, IAuthSessionProvider authSessionProvider, IMailSender mailer,
			IUserPermitRepository permitRepository) : base(userService, dbContext, appContext, securitySettings,
			localeManager,  mailer, logger, authSessionProvider, permitRepository)
		{
			this.userService = userService;
			this.dbContext = dbContext;
			this.appContext = appContext;
			this.logger = logger;
			this.mailer = mailer;
			this.permitRepository = permitRepository;
		}

		public async Task<AppUser> AddCustomerUserAsync(AppUser user, string verificationUrl, string contactUrl)
		{
			var result = await AddUserAsync(user, verificationUrl, contactUrl).ConfigureAwait(false);
			if (result != null)
			{
				await AddCustomerPermitsAsync(user.Username).ConfigureAwait(false);
				await SendCustomerWelcomeMailAsync(result, user.Password, contactUrl).ConfigureAwait(false);

				// reload permissions to include permission on newly added user entity.
				await this.userService.RefreshPermissionsAsync().ConfigureAwait(false);
			}

			return result;
		}

		public async Task<AppUser> AddStaffUserAsync(AppUser user, string verificationUrl, string contactUrl)
		{
			var result = await AddUserAsync(user, verificationUrl, contactUrl).ConfigureAwait(false);
			if (result != null)
			{
				await SetMFAIfEnforcedAsync(user.Username).ConfigureAwait(false);

				if (this.userService.IsAuthenticated)
				{
					// reload permissions to include permission on newly added user entity.
					await this.userService.RefreshPermissionsAsync().ConfigureAwait(false);
				}
			}

			return result;
		}

		public async Task<LoginResult> RegisterCustomerUserAsync(AppUser user, string verificationUrl, string contactUrl, bool createAuthCookie)
		{
			var result = await RegisterUserAsync(user, verificationUrl, contactUrl, createAuthCookie).ConfigureAwait(false);
			if (result != null)
			{
				await AddCustomerPermitsAsync(user.Username).ConfigureAwait(false);

				// reload permissions added as part of above call.
				await this.userService.RefreshPermissionsAsync().ConfigureAwait(false);
			}

			return result;
		}

		public async Task<LoginResult> RegisterStaffUserAsync(AppUser user, string verificationUrl, string contactUrl, bool createAuthCookie)
		{
			var result = await RegisterUserAsync(user, verificationUrl, contactUrl, createAuthCookie).ConfigureAwait(false);
			if (result != null)
			{
				var dbUser = await this.dbContext.Users
					.SingleOrDefaultAsync(x => x.Username == user.Username)
					.ConfigureAwait(false);
				dbUser.UserType = UserType.Staff;
				this.userService.CurrentUser.UserType = UserType.Staff;
				await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
			}

			return result;
		}

		private async Task SendCustomerWelcomeMailAsync(AppUser user, string password, string contactUrl)
		{
			var subject = $"Welcome - {this.appContext.Title}";

			var body = EmailTemplate.BuildHtmlReply(new Dictionary<string, object>
				{
					{"user", user.Name},
					{"username", user.Username},
					{"password", password},
					{"subject", subject},
					{"sign", this.appContext.Title},
					{"product", this.appContext.Title},
					{"contact", contactUrl}
				},
				Template.CustomerWelcome);

			await this.mailer.SendAsync(user.Username, subject, body, true).ConfigureAwait(false);
		}

		private async Task AddCustomerPermitsAsync(string username)
		{
			var dbUser = await this.dbContext.Users
				.Where(x => x.Username == username)
				.Include(x => x.MultiFactors)
				.SingleOrDefaultAsync()
				.ConfigureAwait(false);

			dbUser.UserType = UserType.Customer;
			dbUser.ParentId = null;
			dbUser.MultiFactors.First().Enabled = false;

			var dbUserInvites = await this.dbContext.UserInvitations
				.Where(x => x.EmailAddress == dbUser.Username)
				.ToListAsync()
				.ConfigureAwait(false);

			foreach (var invite in dbUserInvites)
			{
				var dbNominee = new DbAccountNominee
				{
					Id = Guid.NewGuid(),
					AccountId = invite.AccountId,
					NomineeUserId = dbUser.Id
				};
				this.dbContext.AccountNominees.Add(dbNominee);

				invite.UserId = dbUser.Id;

				await this.permitRepository
					.AddPermitAsync(dbUser.Id, SFPermissionCodes.AccountNominee, invite.AccountId)
					.ConfigureAwait(false);
			}

			await this.permitRepository
				.AddPermitAsync(dbUser.Id, SFPermissionCodes.Customer, null)
				.ConfigureAwait(false);

			await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
		}

		private async Task SetMFAIfEnforcedAsync(string username)
		{
			var dbUser = await this.dbContext.Users
				.Where(x => x.Username == username)
				.Include(x => x.MultiFactors)
				.SingleOrDefaultAsync()
				.ConfigureAwait(false);

			var dbBank = await this.dbContext.Banks.Where(x => x.Id == this.userService.BankId)
				.SingleOrDefaultAsync().ConfigureAwait(false);

			dbUser.UserType = UserType.Staff;

			if (dbBank != null)
			{
				dbUser.MultiFactors.First().Enabled = dbBank.EnforceMFA;
			}

			await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
		}
	}
}
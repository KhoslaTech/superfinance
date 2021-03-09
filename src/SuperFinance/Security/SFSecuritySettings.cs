using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using Microsoft.EntityFrameworkCore;
using SuperFinance.DataModels;

namespace SuperFinance.Security
{
	public class SFSecuritySettings : NetSecuritySettings
	{
		private readonly IConfig config;
		private readonly DemoDbContext dbContext;
		private readonly ISecurityContext securityContext;

		public SFSecuritySettings(IConfig config, DemoDbContext dbContext, ISecurityContext securityContext) : base(config)
		{
			this.config = config;
			this.dbContext = dbContext;
			this.securityContext = securityContext;

			Initialize(ASPSecurityKitConfiguration.IsDevelopmentEnvironment);
		}

		public override int? GetPasswordExpirationIntervalInDays(IUser user)
		{
			throw new NotImplementedException("Use async");
		}

		public override async Task<int?> GetPasswordExpirationIntervalInDaysAsync(IUser user, CancellationToken cancellationToken)
		{
			if (this.securityContext.AuthDetails is SFIdentityAuthDetails authDetails)
			{
				return await this.dbContext.Banks.Where(x => x.Id == authDetails.BankId)
					.Select(x => x.PasswordExpiresInDays)
					.SingleOrDefaultAsync(cancellationToken)
					.ConfigureAwait(false);
			}

			return null;
		}

		private void Initialize(bool isDevelopmentEnvironment)
		{
			this.ThrowSecurityFailureAsException = true;
			this.MustHaveBeenVerified = config.GetSettingOrDefault("app:VerificationRequired", true);
			this.LoginUrl = "/User/SignIn?returnUrl={0}";
			this.RegisterUrl = "/User/SignUp";
			this.VerificationUrl = "/User/Verify";
			this.RedirectIfLoggedInUrl = "/User";
			this.RedirectIfVerifiedUrl = "/User";
			this.ChangePasswordUrl = "/User/ChangePassword?expired";
			this.MFAPromptUrl = "/User/TwoFactorAuthentication";
			this.MFASettingUrl = "/User/ChangeTwoFactorAuthSettings?enforced";
			this.IsDevelopmentEnvironment = isDevelopmentEnvironment;
		}
	}
}
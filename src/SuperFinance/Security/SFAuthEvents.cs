using System.Threading;
using System.Threading.Tasks;
using ASKSource.Security;
using ASPSecurityKit;
using ASPSecurityKit.Net;

namespace SuperFinance.Security
{
	public class SFAuthEvents : AuthEvents
	{
		private readonly ISFUserService userService;
		private readonly ISecurityContext securityContext;

		public SFAuthEvents(INetSecuritySettings securitySettings,
			ISFUserService userService, ILocaleService localeService,
			ISecurityContext securityContext) : base(securitySettings, userService, localeService)
		{
			this.userService = userService;
			this.securityContext = securityContext;
		}

		public override async Task OnAuthenticatedAsync(IRequestService req, CancellationToken cancellationToken)
		{
			await base.OnAuthenticatedAsync(req, cancellationToken);

			if (this.securityContext.AuthDetails is SFIdentityAuthDetails authDetails)
			{
				this.userService.BankId = authDetails.BankId;
			}
		}
	}
}
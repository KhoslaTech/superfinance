using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using ASKSource.Controllers;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using SuperFinance.Managers;

namespace SuperFinance.Controllers
{
	[AllowAnonymous]
	public class ErrorController : ErrorControllerBase
	{
		private readonly IAccountManager accountManager;

		public ErrorController(ISecurityContext securityContext, ISecurityUtility securityUtility, ILogger logger,
			IAccountManager accountManager) : base(securityContext, securityUtility, logger)
		{
			this.accountManager = accountManager;
		}

		public override async Task<ActionResult> Index(Guid? verify = null, int code = 500)
		{
			return await base.Index(verify, code);
		}

		protected override async Task LoadSuspensionData(AuthFailedException authFailedException)
		{
			if (Guid.TryParse(authFailedException.Errors.First().Meta.Keys.First().Split(':')[1], out Guid accountId))
			{
				var account = await this.accountManager.GetAccountAsync(accountId);
				if (account != null)
				{
					ViewBag.AccountNumber = account.AccountNumber;
					ViewBag.AccountStatus = account.Status.ToString();
				}
			}
		}
	}
}

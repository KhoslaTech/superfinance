using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ASKSource;
using ASKSource.Controllers;
using ASKSource.Managers;
using ASKSource.Models;
using ASKSource.Security;
using ASPSecurityKit;
using ASPSecurityKit.Authorization;
using ASPSecurityKit.Net;
using SuperFinance.Security;

namespace SuperFinance.Controllers
{
	[AuthPermission(PermissionCodes.ManageFirewall)]
	public class FirewallController : ServiceControllerBase
	{
		private readonly IFirewallManager firewallManager;

		public FirewallController(ISFUserService userService, IAppContext appContext,
			INetSecuritySettings securitySettings, ISecurityUtility securityUtility, ILogger logger, IFirewallManager firewallManager) :
			base(userService, appContext, securitySettings, securityUtility, logger)
		{
			this.firewallManager = firewallManager;
		}

		public ActionResult Index(string entityUrn)
		{
			return View("Index", entityUrn);
		}

		[HttpPost]
		public async Task<ActionResult> List(string entityUrn, int jtStartIndex, int jtPageSize)
		{
			return await SecureJsonAction(async () =>
			{
				var result = await this.firewallManager.GetIpRangesAsync(entityUrn, jtStartIndex, jtPageSize);
				return JsonResponse(ApiResponse.List(result.Records, result.TotalCount));
			});
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Add(FirewallIpRange model)
		{
			return await SecureJsonAction(async () =>
			{
				if (ModelState.IsValid)
				{
					return JsonResponse(ApiResponse.Single(await this.firewallManager.AddIpRangeAsync(model)));
				}

				throw new OpException(OpResult.InvalidInput);
			});
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit(FirewallIpRange model)
		{
			return await SecureJsonAction(async () =>
			{
				if (ModelState.IsValid)
				{
					return JsonResponse(ApiResponse.Single(await this.firewallManager.EditIpRangeAsync(model)));
				}

				throw new OpException(OpResult.InvalidInput);
			});
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Delete([Authorize("FirewallId")] Guid id)
		{
			return await SecureJsonAction(async () =>
			{
				await this.firewallManager.DeleteIpRangeAsync(id);
				return Json(ApiResponse.Success());
			});
		}
	}
}

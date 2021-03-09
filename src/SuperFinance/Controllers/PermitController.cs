using System;
using System.Threading.Tasks;
using ASKSource;
using ASKSource.Controllers;
using ASKSource.DataModels;
using ASKSource.Managers;
using ASKSource.Models;
using ASPSecurityKit;
using ASPSecurityKit.Authorization;
using ASPSecurityKit.Net;
using Microsoft.AspNetCore.Mvc;

namespace SuperFinance.Controllers
{
	// We are using 'EditUser' permission code for granting/revoking permissions flow; you can create more permission codes if you want to authorize these actions separately.
	[AuthPermission("EditUser")]
	public class PermitController : ServiceControllerBase
	{
		private readonly IPermitManager permitManager;

		public PermitController(IUserService<Guid, Guid, DbUser> userService, IAppContext appContext,
			INetSecuritySettings securitySettings, ISecurityUtility securityUtility, ILogger logger, IPermitManager permitManager) :
			base(userService, appContext, securitySettings, securityUtility, logger)
		{
			this.permitManager = permitManager;
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Revoke([Authorize("UserPermitId")]Guid id)
		{
			return await SecureJsonAction(async () =>
			{
				await this.permitManager.RevokePermitAsync(id);
				return Json(ApiResponse.Success());
			});
		}
	}
}

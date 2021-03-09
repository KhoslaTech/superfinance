using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ASKSource;
using ASKSource.Controllers;
using ASKSource.DataModels;
using ASKSource.Models;
using ASPSecurityKit;
using ASPSecurityKit.Authorization;
using ASPSecurityKit.Net;
using SuperFinance.Managers;
using SuperFinance.Models;

namespace SuperFinance.Controllers
{
	[AuthPermission("ManageAccountType")]
	public class AccountTypeController : ServiceControllerBase
	{
		private readonly IAccountTypeManager accountTypeManager;

		public AccountTypeController(IUserService<Guid, Guid, DbUser> userService, IAppContext appContext,
			INetSecuritySettings securitySettings, ISecurityUtility securityUtility, ILogger logger, IAccountTypeManager accountTypeManager) :
			base(userService, appContext, securitySettings, securityUtility, logger)
		{
			this.accountTypeManager = accountTypeManager;
		}

		[PossessesPermissionCode]
		public ActionResult Index()
		{
			return View();
		}

		[PossessesPermissionCode]
		[HttpPost]
		public async Task<ActionResult> List(int jtStartIndex, int jtPageSize)
		{
			return await SecureJsonAction(async () =>
			{
				var result = await this.accountTypeManager.GetAccountTypesAsync(jtStartIndex, jtPageSize);
				return Json(ApiResponse.List(result.Records, result.TotalCount));
			});
		}

		[HttpPost, ValidateAntiForgeryToken]
		[PossessesPermissionCode]
		public async Task<ActionResult> Add(AccountType model)
		{
			return await SecureJsonAction(async () =>
			{
				if (ModelState.IsValid)
				{
					return Json(ApiResponse.Single(await this.accountTypeManager.AddAccountTypeAsync(model)));
				}

				throw new OpException(OpResult.InvalidInput);
			});
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Edit(AccountType model)
		{
			return await SecureJsonAction(async () =>
			{
				if (ModelState.IsValid)
				{
					return Json(ApiResponse.Single(await this.accountTypeManager.EditAccountTypeAsync(model)));
				}

				throw new OpException(OpResult.InvalidInput);
			});
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Delete([Authorize("AccountTypeId")] Guid id)
		{
			return await SecureJsonAction(async () =>
			{
				await this.accountTypeManager.DeleteAccountTypeAsync(id);
				return Json(ApiResponse.Success());
			});
		}
	}
}

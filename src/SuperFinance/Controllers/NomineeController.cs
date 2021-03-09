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
using SuperFinance.ViewModels;

namespace SuperFinance.Controllers
{
	public class NomineeController : ServiceControllerBase
	{
		private readonly IAccountManager accountManager;

		public NomineeController(IUserService<Guid, Guid, DbUser> userService, IAppContext appContext,
			INetSecuritySettings securitySettings, ISecurityUtility securityUtility, ILogger logger,
			IAccountManager accountManager) :
			base(userService, appContext, securitySettings, securityUtility, logger)
		{
			this.accountManager = accountManager;
		}

		[AuthAction("Index")]
		[HttpPost]
		public async Task<ActionResult> List(Guid accountId)
		{
			return await SecureJsonAction(async () =>
			{
				var result = await this.accountManager.GetNomineesAsync(accountId);
				return Json(ApiResponse.List(result, result.Count));
			});
		}

		public ActionResult Add(Guid accountId)
		{
			var model = new AddNomineeModel
			{
				AccountId = accountId
			};

			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Add(AddNomineeModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var nominee = await this.accountManager.AddNomineeAsync(model.AccountId.GetValueOrDefault(), model.Username, RegisterUrl(), LoginUrl());
					return RedirectWithMessage("Index", "Account", null,
						nominee == null ? SFMessages.NomineeInvited : SFMessages.NomineeCreated, OpResult.Success);
				}
				catch (OpException ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}

			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Delete([Authorize("NomineeId")] Guid id)
		{
			return await SecureJsonAction(async () =>
			{
				await this.accountManager.DeleteNomineeAsync(id);
				return Json(ApiResponse.Success());
			});
		}
	}
}

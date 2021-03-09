using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ASKSource;
using ASKSource.Controllers;
using ASKSource.DataModels;
using ASKSource.Models;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using SuperFinance.DataModels;
using SuperFinance.Managers;
using SuperFinance.ViewModels;

namespace SuperFinance.Controllers
{
	public class AccountController : ServiceControllerBase
	{
		private readonly IAccountManager accountManager;
		private readonly IBranchManager branchManager;
		private readonly IBankManager bankManager;
		private readonly IAccountTypeManager accountTypeManager;

		public AccountController(IUserService<Guid, Guid, DbUser> userService, IAppContext appContext,
			INetSecuritySettings securitySettings, ISecurityUtility securityUtility, ILogger logger,
			IAccountManager accountManager, IBranchManager branchManager, IBankManager bankManager,
			IAccountTypeManager accountTypeManager) :
			base(userService, appContext, securitySettings, securityUtility, logger)
		{
			this.accountManager = accountManager;
			this.branchManager = branchManager;
			this.bankManager = bankManager;
			this.accountTypeManager = accountTypeManager;
		}

		[PossessesPermissionCode]
		public ActionResult Index()
		{
			return View();
		}

		public async Task<ActionResult> Open()
		{
			var model = new OpenAccountModel
			{
				Banks = await this.bankManager.ListBanksAsync()
			};

			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Open(OpenAccountModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var result = await this.accountManager.OpenAccountAsync(model, RegisterUrl(), LoginUrl());
					if (result != null)
					{
						return RedirectToAction("Index");
					}
				}
				catch (OpException ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}

			model.Banks = await this.bankManager.ListBanksAsync();

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		[AuthAction("Index")]
		[HttpPost]
		[PossessesPermissionCode]
		public async Task<ActionResult> List()
		{
			return await SecureJsonAction(async () =>
			{
				var accounts = await this.accountManager.GetAccountsAsync(this.userService.CurrentUserId);
				return Json(ApiResponse.List(accounts, accounts.Count));
			});
		}

		[AuthAction("Open")]
		[HttpGet]
		public async Task<ActionResult> ListBranches(Guid bankId)
		{
			return await SecureJsonAction(async () =>
			{
				var branches = await this.branchManager.GetBranchesAsync(bankId, null);
				return Json(ApiResponse.List(branches, branches.Count));
			});
		}

		[AuthAction("Open")]
		[HttpGet]
		public async Task<ActionResult> ListAccountTypes(Guid bankId)
		{
			return await SecureJsonAction(async () =>
			{
				var accountTypes = await this.accountTypeManager.GetAccountTypesAsync(bankId, AccountKind.Investment);
				return Json(ApiResponse.List(accountTypes, accountTypes.Count));
			});
		}
	}
}

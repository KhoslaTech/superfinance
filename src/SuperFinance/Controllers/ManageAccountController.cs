using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ASKSource;
using ASKSource.Controllers;
using ASKSource.Models;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using SuperFinance.DataModels;
using SuperFinance.Infrastructure;
using SuperFinance.Managers;
using SuperFinance.ViewModels;
using SuperFinance.Security;

namespace SuperFinance.Controllers
{
	[AuthEntity("CustomerAccount")]
	public class ManageAccountController : ServiceControllerBase
	{
		private readonly IAccountManager accountManager;
		private readonly IAccountTypeManager accountTypeManager;
		private readonly IBranchManager branchManager;
		private new readonly ISFUserService userService;

		public ManageAccountController(ISFUserService userService, IAppContext appContext,
			INetSecuritySettings securitySettings, ISecurityUtility securityUtility, ILogger logger,
			IAccountManager accountManager, IAccountTypeManager accountTypeManager, IBranchManager branchManager) :
			base(userService, appContext, securitySettings, securityUtility, logger)
		{
			this.accountManager = accountManager;
			this.accountTypeManager = accountTypeManager;
			this.branchManager = branchManager;
			this.userService = userService;
		}

		[PossessesPermissionCode]
		public ActionResult Index()
		{
			return View();
		}

		[PossessesPermissionCode]
		public async Task<ActionResult> Create()
		{
			var model = new CreateAccountModel();
			await PopulateCreateAccountModel(model);
			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Create(CreateAccountModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var result = await this.accountManager.CreateAccountAsync(model, VerificationUrl(), ContactUrl(), RegisterUrl(), LoginUrl());
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

			await PopulateCreateAccountModel(model);
			return View(model);
		}

		[AuthPermission("ChangeStatus")]
		public async Task<ActionResult> ChangeStatus(Guid accountId)
		{
			var model = new ChangeAccountStatusModel
			{
				Statuses = await this.accountManager.GetAccountStatusesAsync(accountId)
			};

			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		[AuthPermission("ChangeStatus")]
		public async Task<ActionResult> ChangeStatus(Guid accountId, ChangeAccountStatusModel model)
		{
			if (ModelState.IsValid)
			{
				if (Enum.TryParse(model.Status, out AccountStatus status))
				{
					await this.accountManager.ChangeAccountStatusAsync(accountId, status, model.Reason);
					return RedirectToAction("Index");
				}
				else
				{
					ModelState.AddModelError(nameof(model.Status), "Please select valid account status");
				}
			}

			model.Statuses = await this.accountManager.GetAccountStatusesAsync(accountId);

			return View(model);
		}

		[AuthAction("Index")]
		[PossessesPermissionCode]
		[HttpPost]
		public async Task<ActionResult> List(int jtStartIndex, int jtPageSize)
		{
			return await SecureJsonAction(async () =>
			{
				var accounts = await this.accountManager.GetAccountsAsync(jtStartIndex, jtPageSize);
				return Json(ApiResponse.List(accounts.Records, accounts.TotalCount));
			});
		}

		private async Task PopulateCreateAccountModel(CreateAccountModel model)
		{
			var branchIds = this.userService.PossessesPermission(SFPermissionCodes.BankOwner)
				? null
				: this.userService.GetEmployeeBranchIds();

			model.Branches = await this.branchManager.GetBranchesAsync(this.userService.BankId, branchIds);
			model.AccountTypes = await this.accountTypeManager.GetAccountTypesAsync(this.userService.BankId, null);
		}
	}
}

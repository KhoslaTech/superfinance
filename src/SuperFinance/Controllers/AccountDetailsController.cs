using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ASKSource;
using ASKSource.Controllers;
using ASKSource.DataModels;
using ASKSource.Models;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using SuperFinance.Managers;
using SuperFinance.ViewModels;

namespace SuperFinance.Controllers
{
	public class AccountDetailsController : ServiceControllerBase
	{
		private readonly ITransactionManager transactionManager;
		private readonly IAccountManager accountManager;

		public AccountDetailsController(IUserService<Guid, Guid, DbUser> userService, IAppContext appContext,
			INetSecuritySettings securitySettings, ISecurityUtility securityUtility, ILogger logger,
			ITransactionManager transactionManager, IAccountManager accountManager) :
			base(userService, appContext, securitySettings, securityUtility, logger)
		{
			this.transactionManager = transactionManager;
			this.accountManager = accountManager;
		}

		[AuthPermission("CreateDeposit")]
		public ActionResult Deposit(Guid accountId)
		{
			var model = new TransactionModel { AccountId = accountId };
			return View(model);
		}

		[AuthPermission("CreateDeposit")]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Deposit(TransactionModel model)
		{
			if (ModelState.IsValid)
			{
				await this.transactionManager.CreateDepositAsync(model);
				return RedirectWithMessage("Index", "AccountDetails", new { accountId = model.AccountId }, SFMessages.DepositCreated, OpResult.Success);
			}

			return View(model);
		}

		[AuthPermission("CreateTransfer")]
		public ActionResult Transfer(Guid accountId)
		{
			var model = new TransferModel { FromAccountId = accountId };
			return View(model);
		}

		[AuthPermission("CreateTransfer")]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Transfer(TransferModel model)
		{
			if (ModelState.IsValid)
			{
				await this.transactionManager.CreateTransferAsync(model);
				return RedirectWithMessage("Index", "AccountDetails", new { accountId = model.FromAccountId }, SFMessages.TransferCreated, OpResult.Success);
			}

			return View(model);
		}

		[AuthPermission("CreateWithdrawal")]
		public ActionResult Withdrawal(Guid accountId)
		{
			var model = new TransactionModel { AccountId = accountId };
			return View(model);
		}

		[AuthPermission("CreateWithdrawal")]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Withdrawal(TransactionModel model)
		{
			if (ModelState.IsValid)
			{
				await this.transactionManager.CreateWithdrawalAsync(model);
				return RedirectWithMessage("Index", "AccountDetails", new { accountId = model.AccountId }, SFMessages.WithdrawalCreated, OpResult.Success);
			}

			return View(model);
		}

		[AuthAction("Index")]
		[HttpPost]
		public async Task<ActionResult> List(Guid accountId, int jtStartIndex, int jtPageSize)
		{
			return await SecureJsonAction(async () =>
			{
				var transactions = await this.transactionManager.GetTransactionsAsync(accountId, jtStartIndex, jtPageSize);
				return JsonResponse(ApiResponse.List(transactions.Records, transactions.TotalCount));
			});
		}

		public async Task<ActionResult> Index(Guid accountId)
		{
			var model = new AccountDetailsModel
			{
				Account = await this.accountManager.GetAccountAsync(accountId)
			};

			return View(model);
		}
	}
}

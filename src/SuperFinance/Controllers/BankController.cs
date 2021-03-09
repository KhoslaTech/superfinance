using ASPSecurityKit;
using ASPSecurityKit.Net;
using Microsoft.AspNetCore.Mvc;
using SuperFinance.Managers;
using SuperFinance.ViewModels;
using System.Threading.Tasks;
using ASKSource;
using ASKSource.Controllers;
using ASKSource.Infrastructure;
using ASKSource.Models;
using SuperFinance.Security;

namespace SuperFinance.Controllers
{
	[AuthPermission("ManageBank")]
	public class BankController : ServiceControllerBase
	{
		private readonly IBankManager bankManager;
		private new readonly ISFUserService userService;

		public BankController(ISFUserService userService, IAppContext appContext,
			INetSecuritySettings securitySettings, ISecurityUtility securityUtility, ILogger logger,
			IBankManager bankManager) : base(userService, appContext, securitySettings, securityUtility, logger)
		{
			this.bankManager = bankManager;
			this.userService = userService;
		}

		[PossessesPermissionCode]
		public async Task<ActionResult> Index(ManageBankActionId id = ManageBankActionId.None)
		{
			return await ManageView(id);
		}

		[PossessesPermissionCode]
		public ActionResult FirewallStatus()
		{
			return RedirectToAction("Index", new { id = ManageBankActionId.SetFirewallStatus });
		}

		[PossessesPermissionCode]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> FirewallStatus(bool enabled)
		{
			await this.bankManager.SetFirewallStatusAsync(this.userService.BankId, enabled);
			return RedirectWithMessage("Index", SFMessages.FirewallStatusIsChanged, OpResult.Success);
		}

		[PossessesPermissionCode]
		public ActionResult MFAPolicy()
		{
			return RedirectToAction("Index", new { id = ManageBankActionId.SetMFAPolicy });
		}

		[PossessesPermissionCode]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> MFAPolicy(SetMFAPolicyModel model)
		{
			if (this.ModelState.IsValid)
			{
				await this.bankManager.SetMFAPolicyAsync(this.userService.BankId, model.EnforceMFA, model.SkipMFAInsideNetwork);
				return RedirectWithMessage("Index", SFMessages.MFAPolicyIsChanged, OpResult.Success);
			}

			return await ManageView(ManageBankActionId.SetMFAPolicy, mModel: model);
		}

		[PossessesPermissionCode]
		public ActionResult PasswordExpirationPolicy()
		{
			return RedirectToAction("Index", new { id = ManageBankActionId.SetPasswordExpirationPolicy });
		}

		[PossessesPermissionCode]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> PasswordExpirationPolicy(SetPasswordExpirationPolicy model)
		{
			if (this.ModelState.IsValid)
			{
				await this.bankManager.SetPasswordExpirationPolicyAsync(this.userService.BankId, model.ExpirePasswordInDays);
				return RedirectWithMessage("Index", SFMessages.PasswordExpirationPolicyIsChanged, OpResult.Success);
			}

			return await ManageView(ManageBankActionId.SetPasswordExpirationPolicy, null, p: model);
		}

		[AllowAnonymous]
		public ActionResult SignUp()
		{
			return View();
		}

		[AllowAnonymous]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> SignUp(RegisterBankModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var result = await this.bankManager.RegisterBankAsync(
						model, VerificationUrl(), ContactUrl());

					if (result != null && result.IsSuccess)
					{
						if (this.securitySettings.MustHaveBeenVerified)
						{
							return RedirectWithMessage("Verify", "User",
								string.Format(Messages.VerificationMailSent, this.userService.CurrentUsername), OpResult.Success);
						}

						return RedirectToAction("Index");
					}
				}
				catch (OpException ex)
				{
					if (ex.Reason == AppOpResult.EmailServiceFailed)
					{
						return RedirectWithMessage("Index", 
											ASPSecurityKitConfiguration.IsDevelopmentEnvironment
												? $"Mail service failed with {ex.Message}."
												: Messages.MailServiceFailed, AppOpResult.EmailServiceFailed);
					}

						ModelState.AddModelError(string.Empty, ex.Message);
				}
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		[AllowAnonymous]
		public async Task<ActionResult> List()
		{
			return await SecureJsonAction(async () =>
			{
				var banks = await this.bankManager.ListBanksAsync();
				return Json(ApiResponse.List(banks, banks.Count));
			});
		}

		private async Task<ViewResult> ManageView(ManageBankActionId act, SetMFAPolicyModel mModel = null, SetPasswordExpirationPolicy p = null)
		{
			var bank = await this.bankManager.GetBankAsync(this.userService.BankId);

			ViewBag.fModel = new SetFirewallStatusModel
			{
				EntityUrn = EntityUrn.MakeUrn(SFEntityTypes.Bank, bank.Id),
				FirewallEnabled = bank.FirewallEnabled
			};

			ViewBag.mModel = mModel ?? new SetMFAPolicyModel
			{
				EnforceMFA = bank.EnforceMFA,
				SkipMFAInsideNetwork = bank.SkipMFAInsideNetwork
			};

			ViewBag.pModel = p ?? new SetPasswordExpirationPolicy
			{
				ExpirePasswordInDays = bank.PasswordExpiresInDays
			};

			ViewBag.Name = bank.Name;
			ViewBag.Address = bank.Address;
			ViewBag.ManageId = act;
			return await Task.FromResult(View("Index"));
		}
	}
}

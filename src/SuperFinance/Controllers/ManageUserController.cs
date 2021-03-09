using System;
using System.Threading.Tasks;
using ASKSource;
using ASKSource.Controllers;
using ASKSource.DataModels;
using ASKSource.Models;
using ASKSource.ViewModels;
using SuperFinance.Managers;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using Microsoft.AspNetCore.Mvc;

namespace SuperFinance.Controllers
{
	[AuthEntity("User")]
	public class ManageUserController : ServiceControllerBase
	{
		private readonly ISFUserManager userManager;

		public ManageUserController(IUserService<Guid, Guid, DbUser> userService, IAppContext appContext,
			INetSecuritySettings securitySettings, ISecurityUtility securityUtility, ILogger logger, ISFUserManager userManager) :
			base(userService, appContext, securitySettings, securityUtility, logger)
		{
			this.userManager = userManager;
		}

		// Listing is authorized via user hierarchy in the database, only children/descendants of a user are editable by that user.
		// Following AuthAction("Add") is just to protect against malicious attempt to execute index without having access to user page. Removing this will not make system vulnerable to data leaking.
		[AuthAction("Add")]
		public ActionResult Index()
		{
			return View();
		}

		// Listing is authorized based on user hierarchy in the database, only children/descendants of a user are editable by that user.
		// Following AuthAction("Add") is just to protect against malicious attempt to execute list without having access to user page. Removing this will not make system vulnerable to data leaking.
		[HttpPost]
		[AuthAction("Add")]
		public async Task<ActionResult> ListChildren(int jtStartIndex, int jtPageSize)
		{
			return await SecureJsonAction(async () =>
					JsonResponse(ApiResponse.List(await this.userManager.GetChildUsersAsync(jtStartIndex, jtPageSize))));
		}

		// Listing is authorized based on user hierarchy in the database, only children/descendants of a user are editable by that user.
		// Following AuthAction("Add") is just to protect against malicious attempt to execute list without having access to user page. Removing this will not make system vulnerable to data leaking.
		[HttpPost]
		[AuthAction("Add")]
		public async Task<ActionResult> ListDescendants(int jtStartIndex, int jtPageSize)
		{
			return await SecureJsonAction(async () =>
					JsonResponse(ApiResponse.List(await this.userManager.GetDescendantUsersAsync(jtStartIndex, jtPageSize))));
		}

		public ActionResult Add()
		{
			return View();
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Add(RegisterModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await this.userManager.AddStaffUserAsync(new AppUser
					{
						Name = model.Name,
						Username = model.Username,
						Password = model.Password
					}, VerificationUrl(), ContactUrl());

					return RedirectToAction("Index");
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

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Delete(Guid id)
		{
			return await SecureJsonAction(async () =>
			{
				await this.userManager.DeleteUserAsync(id);
				return Json(ApiResponse.Success());
			});
		}

		[AuthAction("Edit")]
		public ActionResult ChangePassword(Guid id)
		{
			return View();
		}

		[HttpPost, ValidateAntiForgeryToken]
		[AuthAction("Edit")]
		public async Task<ActionResult> ChangePassword(SetNewPasswordModel model)
		{
			if (ModelState.IsValid)
			{
				if (!await this.userService.ChangePasswordAsync(model.Id, model.NewPassword))
				{
					SetRedirectMessage(Messages.UserDoesNotExist, OpResult.SomeError);
				}
				else
				{
					SetRedirectMessage(Messages.PasswordIsChanged, OpResult.Success);
				}

				return RedirectToAction("Index");
			}

			return View(model);
		}

		[AuthAction("Edit")]
		public ActionResult Suspend(Guid id)
		{
			return View();
		}

		[AuthAction("Edit")]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Suspend(SuspendUserModel model)
		{
			if (this.ModelState.IsValid)
			{
				if (!await this.userService.SetSuspensionStatusAsync(model.Id, true, model.Reason))
				{
					SetRedirectMessage(Messages.UserDoesNotExist, OpResult.SomeError);
				}
				else
				{
					SetRedirectMessage(Messages.UserSuspended, OpResult.Success);
				}

				return RedirectToAction("Index");
			}

			return View(model);
		}

		[AuthAction("Edit")]
		public async Task<ActionResult> Activate(Guid id)
		{
			if (!await this.userService.SetSuspensionStatusAsync(id, false))
			{
				SetRedirectMessage(Messages.UserDoesNotExist, OpResult.SomeError);
			}
			else
			{
				SetRedirectMessage(Messages.UserActivated, OpResult.Success);
			}

			return RedirectToAction("Index");
		}

		[AuthAction("Edit")]
		public ActionResult BlockPassword(Guid id)
		{
			return View();
		}

		[HttpPost, ValidateAntiForgeryToken]
		[AuthAction("Edit")]
		public async Task<ActionResult> BlockPassword(BlockPasswordModel model)
		{
			if (ModelState.IsValid)
			{
				if (!await this.userService.SetPasswordBlockedAsync(model.Id, true, model.Reason))
				{
					SetRedirectMessage(Messages.UserDoesNotExist, OpResult.SomeError);
				}
				else
				{
					SetRedirectMessage(Messages.PasswordIsBlocked, OpResult.Success);
				}

				return RedirectToAction("Index");
			}

			return View(model);
		}
	}
}

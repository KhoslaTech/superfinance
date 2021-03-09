using System;
using System.Linq;
using System.Threading.Tasks;
using ASKSource;
using ASKSource.Controllers;
using ASKSource.Infrastructure;
using ASKSource.Managers;
using ASKSource.Models;
using ASKSource.Security;
using ASKSource.ViewModels;
using SuperFinance.Managers;
using ASPSecurityKit;
using ASPSecurityKit.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuperFinance.Infrastructure;
using SuperFinance.Security;

namespace SuperFinance.Controllers
{
	[VerificationNotRequired] // To allow verification actions and user to change profile related things without needing to verify first. If you want to add any action that must need verification, move this attribute onto actions instead.
	[SkipActivityAuthorization] // To allow profile related actions like index/personal/password/username change. If you want to add any action that must need activity-based authorization, move this attribute onto actions instead.
	public class UserController : ServiceControllerBase
	{
		private readonly IAuthSessionProvider authSessionProvider;
		private readonly ILocaleManager localeManager;
		private readonly ISFUserManager userManager;
		private new readonly ISFUserService userService;

		public UserController(ISFUserService userService, IAppContext appContext,
			INetSecuritySettings securitySettings, ISecurityUtility securityUtility, ILogger logger,
			IAuthSessionProvider authSessionProvider, ILocaleManager localeManager, ISFUserManager userManager) :
			base(userService, appContext, securitySettings, securityUtility, logger)
		{
			this.authSessionProvider = authSessionProvider;
			this.userManager = userManager;
			this.localeManager = localeManager;
			this.userService = userService;
		}

		[AllowAnonymous]
		public ActionResult SignIn(string returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		[AllowAnonymous]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> SignIn(LoginModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				var result = await this.authSessionProvider.LoginAsync(model.Username, model.Password, model.RememberMe,
					this.securitySettings.LetSuspendedAuthenticate, true);
				switch (result.Result)
				{
					case OpResult.Success:
						if (this.securitySettings.MustHaveBeenVerified && !this.userService.IsVerified)
						{
							return RedirectWithMessage("Verify", Messages.VerifyYourEmailNow, OpResult.SomeError);
						}

						return RedirectToLocal(returnUrl);
					case OpResult.Suspended:
						ModelState.AddModelError("", Messages.AccountSuspended);
						break;
					case OpResult.PasswordBlocked:
						ModelState.AddModelError("", Messages.PasswordBlocked);
						break;
					default:
						ModelState.AddModelError("", Messages.IncorrectLogin);
						break;
				}
			}

			// If we got this far, something failed, redisplay form

			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		[Feature(RequestFeature.AuthorizationNotRequired, RequestFeature.MFANotRequired)]
		public async Task<ActionResult> SignOut()
		{
			await this.authSessionProvider.LogoutAsync();
			return RedirectToAction("Index", "Home");
		}

		[AllowAnonymous]
		public ActionResult SignUp()
		{
			return View();
		}

		[AllowAnonymous]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> SignUp(RegisterModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var result = await this.userManager.RegisterCustomerUserAsync(
						new AppUser
						{
							Name = model.Name,
							Username = model.Username,
							Password = model.Password
						},
						VerificationUrl(), ContactUrl(), true);

					if (result != null && result.IsSuccess)
					{
						if (this.securitySettings.MustHaveBeenVerified)
						{
							return RedirectWithMessage("Verify",
								string.Format(Messages.VerificationMailSent, this.userService.CurrentUsername), OpResult.Success);
						}

						return RedirectToAction("Open", "Account");
					}
				}
				catch (OpException ex)
				{
					if (GetResultForEmailServiceError(ex, false, "Index", "Home") is var result && result != null)
						return result;

					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		[MFANotRequired]
		public ActionResult Verify()
		{
			return View();
		}

		[MFANotRequired]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> ResendVerificationEmail()
		{
			if (this.userService.IsAuthenticated && this.userService.IsVerified)
			{
				return RedirectWithMessage("Index", "Home", Messages.AccountAlreadyVerified, OpResult.Success);
			}

			try
			{
				await this.userManager.SendVerificationMailAsync(this.userService.CurrentUser, VerificationUrl(), ContactUrl());
				return RedirectWithMessage("Verify",
					string.Format(Messages.VerificationMailSent, this.userService.CurrentUsername), OpResult.Success);
			}
			catch (OpException ex)
			{
				return GetResultForEmailServiceError(ex, false, nameof(Verify))
					   ?? throw ex;
			}
		}

		[AllowAnonymous]
		public async Task<ActionResult> Verification(string id)
		{
			if (this.userService.IsAuthenticated && this.userService.IsVerified)
			{
				return RedirectWithMessage("Index", "Home", Messages.AccountAlreadyVerified, OpResult.Success);
			}

			if (await this.userService.VerifyAccountAsync(id))
			{
				if (this.userService.IsAuthenticated)
				{
					return RedirectWithMessage("Index", "Home", Messages.AccountVerifiedSuccessfully, OpResult.Success);
				}

				return RedirectWithMessage("SignIn", Messages.AccountVerifiedSuccessfully, OpResult.Success);
			}

			return RedirectWithMessage("Verify", Messages.AccountVerificationFailed, OpResult.SomeError);
		}

		[AllowAnonymous]
		public ActionResult ForgotPassword()
		{
			return View();
		}

		[AllowAnonymous]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> ForgotPassword(ForgotPasswordModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await this.userManager.SendPasswordResetMailAsync(model.Username, PasswordResetUrl());
					return RedirectWithMessage("ForgotPassword", Messages.PasswordResetMailSent, OpResult.Success);
				}
			}
			catch (OpException ex)
			{
				if (ex.Reason == AppOpResult.EmailServiceFailed)
				{
					// only on dev env we should notify the problem; on prod env, notifying about it also leaks that the provided username was indeed correct.
					return ASPSecurityKitConfiguration.IsDevelopmentEnvironment
						? RedirectWithMessage(nameof(ForgotPassword), $"Mail service failed with {ex.Message}.", AppOpResult.EmailServiceFailed)
						: RedirectWithMessage(nameof(ForgotPassword), Messages.PasswordResetMailSent, OpResult.Success);
				}

				throw;
			}

			// Failed validation
			return View();
		}

		[AllowAnonymous]
		public async Task<ActionResult> ResetPassword(string token)
		{
			if (await this.userService.IsPasswordResetTokenValidAsync(token))
			{
				return View();
			}

			return RedirectWithMessage("ForgotPassword", Messages.PasswordResetTokenExpired, OpResult.SomeError);
		}

		[AllowAnonymous]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> ResetPassword(string token, ResetPasswordModel model)
		{
			if (ModelState.IsValid)
			{
				if (!await this.userService.ResetPasswordAsync(token, model.NewPassword))
				{
					return RedirectWithMessage("ForgotPassword", Messages.PasswordResetTokenExpired, OpResult.SomeError);
				}
				else
				{
					return RedirectWithMessage("SignIn", Messages.PasswordIsChanged, OpResult.Success);
				}
			}

			return View(model);
		}

		public async Task<ActionResult> Index(ManageActionId id = ManageActionId.None)
		{
			return await ManageView(id);
		}

		[AllowPasswordExpired]
		public async Task<ActionResult> ChangePassword()
		{
			if (Request.QueryString.Value.Contains("expired"))
			{
				SetMessage(Messages.PasswordExpired, OpResult.PasswordExpired);
			}

			return await ManageView(ManageActionId.ChangePassword);
		}

		[HttpPost, ValidateAntiForgeryToken]
		[AllowPasswordExpired]
		public async Task<ActionResult> ChangePassword(ChangePasswordModel model)
		{
			if (ModelState.IsValid)
			{
				if (await this.userService
					.ChangePasswordAsync(this.userService.CurrentUserId, model.OldPassword, model.NewPassword))
				{
					return RedirectWithMessage("Index", Messages.PasswordIsChanged, OpResult.Success);
				}
				else
					ModelState.AddModelError("", Messages.InvalidCurrentPassword);
			}

			// If we got this far, something failed, redisplay form
			return await ManageView(ManageActionId.ChangePassword);
		}

		public ActionResult ChangeUsername()
		{
			return RedirectToAction("Index", new { id = ManageActionId.ChangeUsername });
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangeUsername(ChangeUsernameModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await this.userManager.ChangeUsernameAsync(
						new AppUser
						{
							Id = this.userService.CurrentUserId,
							Username = model.Username,
							Password = model.Password
						}, VerificationUrl(), ContactUrl());

					if (this.securitySettings.MustHaveBeenVerified)
					{
						return RedirectWithMessage("Verify", Messages.EmailIsChanged, OpResult.Success);
					}

					return RedirectWithMessage("Index", Messages.EmailIsChanged, OpResult.Success);
				}
				catch (OpException ex)
				{
					if (GetResultForEmailServiceError(ex, false, nameof(Index)) is var result && result != null)
						return result;

					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}

			// If we got this far, something failed, redisplay form
			return await ManageView(ManageActionId.ChangeUsername, eModel: model);
		}

		public ActionResult Personal()
		{
			return RedirectToAction("Index", new { id = ManageActionId.ChangePersonal });
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Personal(PersonalModel model)
		{
			if (ModelState.IsValid)
			{
				await this.userManager.UpdateUserAsync(new AppUser
				{
					Id = this.userService.CurrentUserId,
					Name = model.Name,
					CultureCode = model.CultureCode,
					TimeZoneId = model.TimeZoneId
				});

				return RedirectWithMessage("Index", Messages.ProfileIsChanged, OpResult.Success);
			}

			// If we got this far, something failed, redisplay form
			return await ManageView(ManageActionId.ChangePersonal, pModel: model);
		}

		[MultiFactorSetting]
		public async Task<ActionResult> ChangeTwoFactorAuthSettings()
		{
			if (Request.QueryString.Value.Contains("enforced"))
			{
				SetMessage(SFMessages.MFAEnforced, OpResult.SomeError);
			}

			return await ManageView(ManageActionId.ChangeTwoFactorAuthSettings);
		}

		[MultiFactorSetting]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> ChangeTwoFactorAuthSettings(ChangeTwoFactorAuthSettingsModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					await this.userManager.SetUserMultiFactorAsync(this.userService.CurrentUserId, model.Enabled);

					if (model.Enabled)
					{
						await this.userService.SetMFAVerifiedAsync(true);
					}

					return RedirectWithMessage("Index", Messages.TwoFactorAuthenticationSettingChanged, OpResult.Success);
				}
				catch (OpException ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}

			// If we got this far, something failed, redisplay form
			return await ManageView(ManageActionId.ChangeTwoFactorAuthSettings, tModel: model);
		}

		[Feature(RequestFeature.AuthorizationNotRequired, RequestFeature.MFANotRequired)]
		public async Task<ActionResult> TwoFactorAuthentication(bool resendToken = false, string returnUrl = null)
		{
			try
			{
				await this.userManager.SendTwoFactorAuthenticationTokenAsync(resendToken);
				ViewBag.ReturnUrl = returnUrl;
				return View();
			}
			catch (OpException ex)
			{
				return GetResultForEmailServiceError(ex, true)
				       ?? throw ex;
			}
		}

		[HttpPost, ValidateAntiForgeryToken]
		[Feature(RequestFeature.AuthorizationNotRequired, RequestFeature.MFANotRequired)]
		public async Task<ActionResult> TwoFactorAuthentication(TwoFactorAuthenticationModel model, string returnUrl)
		{
			if (this.ModelState.IsValid)
			{
				if (await this.userManager.VerifyTwoFactorAuthenticationTokenAsync(model.Token))
				{
					return RedirectToLocal(returnUrl);
				}

				ModelState.AddModelError(string.Empty, string.Format(Messages.InvalidTwoFactorAuthenticationToken, model.Token));
			}

			return View(model);
		}

		public ActionResult FirewallStatus()
		{
			return RedirectToAction("Index", new { id = ManageActionId.ChangeFirewallStatus });
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> FirewallStatus(bool enabled)
		{
			try
			{
				await this.userManager.SetFirewallStatusAsync(this.userService.CurrentUserId, enabled);
				return RedirectWithMessage("Index", Messages.FirewallStatusIsChanged, OpResult.Success);
			}
			catch (OpException ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
			}

			// If we got this far, something failed, redisplay form	
			return await ManageView(ManageActionId.ChangeFirewallStatus);
		}

		private async Task<ViewResult> ManageView(ManageActionId act, PersonalModel pModel = null,
			ChangeUsernameModel eModel = null, ChangeTwoFactorAuthSettingsModel tModel = null)
		{
			ViewBag.pModel = pModel ?? new PersonalModel
			{
				Name = this.userService.CurrentUserFullName,
				CultureCode = this.userService.CurrentUser.CultureCode,
				TimeZoneId = this.userService.CurrentUser.TimeZoneId
			};

			ViewBag.eModel = eModel ?? new ChangeUsernameModel { Username = this.userService.CurrentUsername };

			ViewBag.tModel = tModel ?? new ChangeTwoFactorAuthSettingsModel
			{
				Enabled = this.userService.CurrentUser.MultiFactor.Enabled
			};

			ViewBag.fModel = new FirewallStatusModel
			{
				EntityUrn = EntityUrn.MakeUrn(EntityTypes.User, this.userService.CurrentUserId),
				FirewallEnabled = this.userService.CurrentUser.FirewallEnabled
			};

			ViewData["cultures"] = (await this.localeManager.GetCulturesAsync())
				.Select(m => new SelectListItem
				{
					Value = m.CultureCode,
					Text = m.DisplayName,
					Selected = this.userService.CurrentUser.CultureCode.Equals(m.CultureCode,
						StringComparison.OrdinalIgnoreCase)
				})
				.ToList();

			ViewData["timeZones"] = (await this.localeManager.GetTimeZonesAsync())
				.Select(m => new SelectListItem
				{
					Value = m.Id.ToString(),
					Text = m.DisplayName,
					Selected = this.userService.CurrentUser.TimeZoneId == m.Id
				})
				.ToList();

			ViewBag.ManageId = act;

			return await Task.FromResult(View("Index"));
		}

		private ActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}

			if (this.userService.CurrentUser.UserType == UserType.Customer &&
				(this.userService.PossessesPermission(SFPermissionCodes.AccountHolder) ||
				 this.userService.PossessesPermission(SFPermissionCodes.AccountNominee)))
			{
				return RedirectToAction("Index", "Account");
			}

			if (this.userService.CurrentUser.UserType == UserType.Staff)
			{
				if (this.userService.IsBankOwner())
				{
					return RedirectToAction("Index", "Bank");
				}

				return RedirectToAction("Index", "ManageAccount");
			}

			return RedirectToAction("Index", "User");
		}

		private string PasswordResetUrl() => Url.Action("ResetPassword", "User", new { token = "{0}" }, Request.Scheme);

		private ActionResult GetResultForEmailServiceError(OpException ex, bool currentView, string actionName = null, string controllerName = null)
		{
			if (ex.Reason == AppOpResult.EmailServiceFailed)
			{
				if (currentView)
				{
					// Can't do redirection as it'd then cause infinite recursion if email service continues to throw error.
					SetMessage(ASPSecurityKitConfiguration.IsDevelopmentEnvironment
						? $"Mail service failed with {ex.Message}."
						: Messages.MailServiceFailed, AppOpResult.EmailServiceFailed);

					return View();
				}

				return RedirectWithMessage(actionName, controllerName ?? "User",
					ASPSecurityKitConfiguration.IsDevelopmentEnvironment
						? $"Mail service failed with {ex.Message}."
						: Messages.MailServiceFailed, AppOpResult.EmailServiceFailed);
			}

			return null;
		}
	}
}
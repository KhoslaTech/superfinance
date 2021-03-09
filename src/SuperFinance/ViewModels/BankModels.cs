using System;
using System.ComponentModel.DataAnnotations;
using ASKSource;
using ASKSource.Infrastructure;
using ASPSecurityKit.Authorization;

namespace SuperFinance.ViewModels
{
	public enum ManageBankActionId
	{
		None,
		SetFirewallStatus,
		SetMFAPolicy,
		SetPasswordExpirationPolicy
	}

	public class RegisterBankModel
	{
		[MaxLength(60)]
		[Required]
		[Display(Name = "Bank Name")]
		public string Name { get; set; }

		[Display(Name = "Address")]
		public string Address { get; set; }

		[MaxLength(100)]
		[Required]
		[Display(Name = "Admin Email")]
		[RegularExpression(RegexPatterns.EmailAddress, ErrorMessageResourceName = "InvalidEmailAddress", ErrorMessageResourceType = typeof(Messages))]
		[DoNotAuthorize]
		public string Username { get; set; }

		[Required]
		[StringLength(512, MinimumLength = 6, ErrorMessageResourceName = "InvalidPasswordLength", ErrorMessageResourceType = typeof(Messages))]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
	}

	public class SetFirewallStatusModel
	{
		public string EntityUrn { get; set; }

		public bool FirewallEnabled { get; set; }
	}

	public class SetMFAPolicyModel
	{
		[Display(Name = "Enforce Two-Factor Authentication ?")]
		[Required]
		public bool EnforceMFA { get; set; }

		[Display(Name = "Skip Two-Factor Authentication check inside bank network?")]
		public bool SkipMFAInsideNetwork { get; set; }
	}

	public class SetPasswordExpirationPolicy
	{
		[Range(30, Int32.MaxValue, ErrorMessageResourceName = "InvalidPasswordExpirationDays", ErrorMessageResourceType = typeof(SFMessages))]
		[Display(Name = "Staff users are required to change password in")]
		public int? ExpirePasswordInDays { get; set; }
	}
}

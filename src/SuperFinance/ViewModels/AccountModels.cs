using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ASKSource;
using ASKSource.Infrastructure;
using ASPSecurityKit.Authorization;
using SuperFinance.Models;

namespace SuperFinance.ViewModels
{
	public class CreateAccountModel
	{
		[MaxLength(60)]
		[Required]
		[Display(Name = "Name")]
		public string Name { get; set; }

		[MaxLength(100)]
		[Required]
		[Display(Name = "Email")]
		[RegularExpression(RegexPatterns.EmailAddress, ErrorMessageResourceName = "InvalidEmailAddress", ErrorMessageResourceType = typeof(Messages))]
		[DoNotAuthorize]
		public string Username { get; set; }

		[MaxLength(24)]
		[Required]
		[Display(Name = "Identity Number")]
		public string IdentityNumber { get; set; }

		[Required]
		[Display(Name = "Amount")]
		public double Amount { get; set; }
		
		public Guid? BranchId { get; set; }

		[Required]
		[Display(Name = "Account Type")]
		[DoNotAuthorize]
		public Guid AccountTypeId { get; set; }

		[Display(Name = "Nominee Email")]
		[RegularExpression(RegexPatterns.EmailAddress, ErrorMessageResourceName = "InvalidEmailAddress", ErrorMessageResourceType = typeof(Messages))]
		[DoNotAuthorize]
		public string NomineeUsername { get; set; }

		public IList<Branch> Branches { get; set; }

		public IList<AccountType> AccountTypes { get; set; }
	}

	public class OpenAccountModel
	{
		[Required]
		[Display(Name = "Bank")]
		public Guid BankId { get; set; }

		public IList<Bank> Banks { get; set; }

		[Required]
		[Display(Name = "Branch")]
		public Guid BranchId { get; set; }

		[Required]
		[Display(Name = "Account Type")]
		public Guid AccountTypeId { get; set; }

		[MaxLength(24)]
		[Required]
		[Display(Name = "Identity Number")]
		public string IdentityNumber { get; set; }

		[Display(Name = "Nominee Email")]
		[RegularExpression(RegexPatterns.EmailAddress, ErrorMessageResourceName = "InvalidEmailAddress", ErrorMessageResourceType = typeof(Messages))]
		public string NomineeUsername { get; set; }
	}

	public class AccountDetailsModel
	{
		public Account Account { get; set; }
	}

	public class ChangeAccountStatusModel
	{
		[Required]
		[Display(Name = "Status")]
		public string Status { get; set; }

		[Required]
		[Display(Name = "Reason")]
		public string Reason { get; set; }

		public IList<string> Statuses { get; set; }
	}

	public class AddNomineeModel
	{
		[MaxLength(100)]
		[Required]
		[Display(Name = "Email")]
		[RegularExpression(RegexPatterns.EmailAddress, ErrorMessageResourceName = "InvalidEmailAddress", ErrorMessageResourceType = typeof(Messages))]
		[DoNotAuthorize]
		public string Username { get; set; }

		[Required]
		public Guid? AccountId { get; set; }
	}
}

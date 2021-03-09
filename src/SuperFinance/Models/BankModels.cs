using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ASPSecurityKit.Authorization;
using SuperFinance.DataModels;

namespace SuperFinance.Models
{
	public class Bank
	{
		[Authorize("BankId")]
		public Guid Id { get; set; }

		public string Name { get; set; }

		public string Address { get; set; }

		public bool FirewallEnabled { get; set; }

		public bool EnforceMFA { get; set; }

		public bool SkipMFAInsideNetwork { get; set; }

		public int? PasswordExpiresInDays { get; set; }
	}

	public class Branch
	{
		[Authorize("BranchId")]
		public Guid? Id { get; set; }

		[Required]
		[MaxLength(60)]
		public string Name { get; set; }

		[Required]
		[MaxLength(16)]
		public string Code { get; set; }

		public string Address { get; set; }

		public IList<BranchStaff> Staff { get; set; }
	}

	public class BranchStaff
	{
		public Guid? Id { get; set; }

		public string Name { get; set; }

		public string Username { get; set; }

		public string Role { get; set; }

		public Guid? BranchId { get; set; }
	}

	public class AccountType
	{
		[Authorize("AccountTypeId")]
		public Guid? Id { get; set; }

		[Required]
		[MaxLength(30)]
		public string Name { get; set; }

		[Required]
		public double InterestRate { get; set; }

		[Required]
		public AccountKind Kind { get; set; }
	}

	public class Account
	{
		public Guid? Id { get; set; }

		public string AccountNumber { get; set; }

		public string IdentityNumber { get; set; }

		public AccountStatus Status { get; set; }

		public string Reason { get; set; }

		public string AccountType { get; set; }

		public AccountKind AccountKind { get; set; }

		public string Branch { get; set; }

		public string Name { get; set; }

		public bool IsOwnAccount { get; set; }

		public double Balance { get; set; }

		public DateTime CreatedDate { get; set; }
	}

	public class AccountNominee
	{
		public Guid? Id { get; set; }

		public string Name { get; set; }

		public string Username { get; set; }

		public Guid? AccountId { get; set; }
	}

	public class Transaction
	{
		public Guid Id { get; set; }

		public DateTime Date { get; set; }

		public double Amount { get; set; }

		public TransactionType TransactionType { get; set; }

		public string Remarks { get; set; }
	}
}
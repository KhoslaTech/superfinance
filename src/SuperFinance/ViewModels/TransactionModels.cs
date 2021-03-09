using System;
using System.ComponentModel.DataAnnotations;
using ASPSecurityKit.Authorization;

namespace SuperFinance.ViewModels
{
	public class TransactionModel
	{
		[Required]
		[Display(Name = "Account")]
		public Guid AccountId { get; set; }

		[Required]
		[Display(Name = "Amount")]
		public double Amount { get; set; }

		[Display(Name = "Remarks")]
		public string Remarks { get; set; }
	}

	public class TransferModel
	{
		[Required]
		[Display(Name = "From Account")]
		[Authorize("AccountId")]
		public Guid FromAccountId { get; set; }

		[Required]
		[Display(Name = "To Account")]
		public string ToAccountNumber { get; set; }

		[Required]
		[Display(Name = "Amount")]
		public double Amount { get; set; }

		[Display(Name = "Remarks")]
		public string Remarks { get; set; }
	}
}
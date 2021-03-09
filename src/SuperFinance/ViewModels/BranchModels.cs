using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ASKSource;
using ASKSource.Infrastructure;

namespace SuperFinance.ViewModels
{
	public class AddStaffModel
	{
		[MaxLength(100)]
		[Required]
		[Display(Name = "Email")]
		[RegularExpression(RegexPatterns.EmailAddress, ErrorMessageResourceName = "InvalidEmailAddress", ErrorMessageResourceType = typeof(Messages))]
		public string Username { get; set; }

		[Required]
		[Display(Name = "Role")]
		public string Role { get; set; }

		public Guid? BranchId { get; set; }

		public IList<string> Roles { get; set; }
	}
}
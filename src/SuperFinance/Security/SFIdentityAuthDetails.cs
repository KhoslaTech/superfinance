using System;
using ASKSource.Security;

namespace SuperFinance.Security
{
	public class SFIdentityAuthDetails : IdentityAuthDetails
	{
		public Guid? BankId { get; set; }
	}
}
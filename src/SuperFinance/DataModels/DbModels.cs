using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using ASKSource.DataModels;
using Microsoft.EntityFrameworkCore;

namespace SuperFinance.DataModels
{
	public class DemoDbContext : AppDbContext
	{
		public DemoDbContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<DbBank> Banks { get; set; }
		public DbSet<DbBranch> Branches { get; set; }
		public DbSet<DbAccountType> AccountTypes { get; set; }
		public DbSet<DbAccount> Accounts { get; set; }
		public DbSet<DbAccountNominee> AccountNominees { get; set; }
		public DbSet<DbTransaction> Transactions { get; set; }
		public DbSet<DbTransfer> Transfers { get; set; }
		public DbSet<DbUserInvitation> UserInvitations { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<DbBank>()
				.HasIndex(x => x.Name)
				.IsUnique();

			modelBuilder.Entity<DbBranch>()
				.HasIndex(x => x.Code)
				.IsUnique();

			// Set cascade deletion to do nothing (consequently deletion fails if there's a dependent record)
			// because we should delete the dependent stuff explicitly to avoid unexpected deletion.
			foreach (var relationship in modelBuilder.Model.GetEntityTypes()
				.SelectMany(e => e.GetForeignKeys()))
			{
				relationship.DeleteBehavior = DeleteBehavior.NoAction;
			}
		}
	}

	public enum AccountStatus
	{
		PendingApproval,
		KYCRequired,
		Active,
		Dormant,
		Closed,
		Freezed
	}

	public enum TransactionType
	{
		Credit,
		Debit
	}

	public enum AccountKind
	{
		Investment,
		Loan
	}

	[Table("Bank")]
	public class DbBank
	{
		[Key]
		public Guid Id { get; set; }

		[MaxLength(60)]
		[Required]
		public string Name { get; set; }

		public string Address { get; set; }

		public bool FirewallEnabled { get; set; }

		public bool EnforceMFA { get; set; }

		public bool SkipMFAInsideNetwork { get; set; }

		public int? PasswordExpiresInDays { get; set; }

		[ForeignKey("User")]
		public Guid OwningUserId { get; set; }
		public DbUser OwningUser { get; set; }

		public IList<DbBranch> Branches { get; set; }

		public IList<DbAccountType> AccountTypes { get; set; }
	}

	[Table("Branch")]
	public class DbBranch
	{
		[Key]
		public Guid Id { get; set; }

		[MaxLength(60)]
		[Required]
		public string Name { get; set; }

		[MaxLength(16)]
		[Required]
		public string Code { get; set; }

		public string Address { get; set; }

		[ForeignKey("Bank")]
		public Guid BankId { get; set; }
		public DbBank Bank { get; set; }
	}

	[Table("AccountType")]
	public class DbAccountType
	{
		[Key]
		public Guid Id { get; set; }

		[MaxLength(30)]
		[Required]
		public string Name { get; set; }

		[Required]
		public double InterestRate { get; set; }

		[Required]
		public AccountKind Kind { get; set; }

		[ForeignKey("Bank")]
		public Guid BankId { get; set; }
		public DbBank Bank { get; set; }
	}

	[Table("Account")]
	public class DbAccount
	{
		[Key]
		public Guid Id { get; set; }

		[MaxLength(30)]
		[Required]
		public string Number { get; set; }

		[MaxLength(24)]
		[Required]
		public string IdentityNumber { get; set; }

		public AccountStatus Status { get; set; }

		public string Reason { get; set; }

		[ForeignKey("AccountType")]
		public Guid AccountTypeId { get; set; }
		public DbAccountType AccountType { get; set; }

		[ForeignKey("Branch")]
		public Guid BranchId { get; set; }
		public DbBranch Branch { get; set; }

		[ForeignKey("User")]
		public Guid OwningUserId { get; set; }
		public DbUser OwningUser { get; set; }

		public DateTime CreatedDate { get; set; }

		public IList<DbAccountNominee> Nominees { get; set; }
	}

	[Table("AccountNominee")]
	public class DbAccountNominee
	{
		[Key]
		public Guid Id { get; set; }

		[ForeignKey("Account")]
		public Guid AccountId { get; set; }
		public DbAccount Account { get; set; }

		[ForeignKey("User")]
		public Guid NomineeUserId { get; set; }
		public DbUser NomineeUser { get; set; }
	}

	[Table("Transaction")]
	public class DbTransaction
	{
		[Key]
		public Guid Id { get; set; }

		public DateTime Date { get; set; }

		public double Amount { get; set; }

		public TransactionType TransactionType { get; set; }

		public string Remarks { get; set; }

		[ForeignKey("Account")]
		public Guid AccountId { get; set; }
		public DbAccount Account { get; set; }
	}

	[Table("Transfer")]
	public class DbTransfer
	{
		[Key]
		public Guid Id { get; set; }

		[ForeignKey("Transaction")]
		public Guid DebitTransactionId { get; set; }
		public DbTransaction DebitTransaction { get; set; }

		[ForeignKey("Transaction")]
		public Guid CreditTransactionId { get; set; }
		public DbTransaction CreditTransaction { get; set; }

		public DateTime CreatedDate { get; set; }
	}

	[Table("UserInvitation")]
	public class DbUserInvitation
	{
		[Key]
		public Guid Id { get; set; }

		[MaxLength(100)]
		[Required]
		public string EmailAddress { get; set; }

		public DateTime Date { get; set; }

		[ForeignKey("Account")]
		public Guid AccountId { get; set; }
		public DbAccount Account { get; set; }

		[ForeignKey("User")]
		public Guid? UserId { get; set; }
		public DbUser User { get; set; }
	}
}
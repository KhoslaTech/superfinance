using ASPSecurityKit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SuperFinance.DataModels;
using SuperFinance.Models;
using SuperFinance.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASKSource;
using ASKSource.Managers;
using ASKSource.Models;
using ASKSource.Security;
using SuperFinance.ViewModels;
using SuperFinance.Infrastructure;
using ASKSource.Infrastructure;
using ASKSource.Repositories;

namespace SuperFinance.Managers
{
	public interface IAccountManager
	{
		Task<PagedResult<Account>> GetAccountsAsync(int startIndex, int pageSize);

		Task<List<Account>> GetAccountsAsync(Guid? userId);

		Task<Account> GetAccountAsync(Guid accountId);

		Task<IList<string>> GetAccountStatusesAsync(Guid accountId);

		Task<Account> OpenAccountAsync(OpenAccountModel model, string registerUrl, string loginUrl);

		Task<Account> CreateAccountAsync(CreateAccountModel model, string verificationUrl, string contactUrl, string registerUrl, string loginUrl);

		Task ChangeAccountStatusAsync(Guid id, AccountStatus status, string reason);

		Task<AccountNominee> AddNomineeAsync(Guid accountId, string nomineeEmail, string registerUrl, string loginUrl);

		Task DeleteNomineeAsync(Guid id);

		Task<List<AccountNominee>> GetNomineesAsync(Guid accountId);
	}

	public class AccountManager : IAccountManager
	{
		private readonly DemoDbContext dbContext;
		private readonly ILogger logger;
		private readonly ISFUserService userService;
		private readonly ISFUserManager userManager;
		private readonly ISuspensionManager suspensionManager;
		private readonly ITransactionManager transactionManager;
		private readonly IMailSender mailer;
		private readonly IAppContext appContext;
		private readonly IUserPermitRepository permitRepository;

		private readonly AccountStatus[] suspendedAccountStatuses = new[]
		{
			AccountStatus.PendingApproval, AccountStatus.KYCRequired, AccountStatus.Dormant, AccountStatus.Freezed,
			AccountStatus.Closed
		};

		public AccountManager(DemoDbContext dbContext, ISFUserService userService,
			ISFUserManager userManager, ISuspensionManager suspensionManager, ILogger logger, IMailSender mailer,
			IAppContext appContext, IUserPermitRepository permitRepository, ITransactionManager transactionManager)
		{
			this.dbContext = dbContext;
			this.userService = userService;
			this.userManager = userManager;
			this.suspensionManager = suspensionManager;
			this.transactionManager = transactionManager;
			this.logger = logger;
			this.mailer = mailer;
			this.appContext = appContext;
			this.permitRepository = permitRepository;
		}

		public async Task ChangeAccountStatusAsync(Guid id, AccountStatus status, string reason)
		{
			var dbAccount = await this.dbContext.Accounts.FindAsync(id).ConfigureAwait(false);
			if (dbAccount != null)
			{
				if (status != dbAccount.Status)
				{
					if (suspendedAccountStatuses.Contains(dbAccount.Status))
					{
						await this.suspensionManager.DeleteEntityAsync(id);
					}

					if (suspendedAccountStatuses.Contains(status))
					{
						await this.suspensionManager.AddEntityAsync(new SuspendedEntity(id, "Account", status.ToString()));
					}
				}

				dbAccount.Status = status;
				dbAccount.Reason = reason;
				await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
			}
			else
			{
				throw new OpException(OpResult.DoNotExist);
			}
		}

		public async Task<Account> OpenAccountAsync(OpenAccountModel model, string registerUrl, string loginUrl)
		{
			var dbAccount = new DbAccount
			{
				Id = Guid.NewGuid(),
				Number = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
				Status = AccountStatus.PendingApproval,
				IdentityNumber = model.IdentityNumber,
				AccountTypeId = model.AccountTypeId,
				BranchId = model.BranchId,
				OwningUserId = this.userService.CurrentUserId,
				CreatedDate = DateTime.UtcNow
			};

			var account = await AddAccountAsync(dbAccount, model.NomineeUsername, registerUrl, loginUrl, 0)
				.ConfigureAwait(false);

			await this.suspensionManager.AddEntityAsync(new SuspendedEntity(account.Id.GetValueOrDefault(), "Account", account.Status.ToString()));

			return account;
		}

		public async Task<Account> CreateAccountAsync(CreateAccountModel model, string verificationUrl, string contactUrl, string registerUrl, string loginUrl)
		{
			var dbUser = await this.dbContext.Users
				.SingleOrDefaultAsync(x => x.Username == model.Username).ConfigureAwait(false);
			var userId = dbUser?.Id;
			if (dbUser == null)
			{
				// leaving password empty so a random password is generated and marked as expired so user is forced to change it upon login
				var result = await this.userManager.AddCustomerUserAsync(
					new AppUser
					{
						Name = model.Name,
						Username = model.Username
					},
					verificationUrl, contactUrl);

				userId = result.Id;
			}
			else if (dbUser.UserType != UserType.Customer)
			{
				throw new OpException(OpResult.InvalidInput, string.Format(SFMessages.NotACustomerUser, model.Username));
			}

			var dbAccount = new DbAccount
			{
				Id = Guid.NewGuid(),
				Number = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
				Status = AccountStatus.Active,
				IdentityNumber = model.IdentityNumber,
				AccountTypeId = model.AccountTypeId,
				BranchId = model.BranchId.GetValueOrDefault(),
				OwningUserId = userId.GetValueOrDefault(),
				CreatedDate = DateTime.UtcNow
			};

			return await AddAccountAsync(dbAccount, model.NomineeUsername, registerUrl, loginUrl, model.Amount).ConfigureAwait(false);
		}

		public async Task<IList<string>> GetAccountStatusesAsync(Guid accountId)
		{
			var accountStatus = await this.dbContext.Accounts
				.Where(p => p.Id == accountId)
				.Select(x => x.Status)
				.SingleOrDefaultAsync()
				.ConfigureAwait(false);

			return Enum.GetNames(typeof(AccountStatus)).Where(x => !x.Equals(accountStatus.ToString())).ToList();
		}

		public async Task<Account> GetAccountAsync(Guid accountId)
		{
			var account = await this.dbContext.Accounts.Where(p => p.Id == accountId)
				.Include(x => x.OwningUser)
				.Include(x => x.AccountType)
				.Include(x => x.Branch)
				.OrderBy(p => p.Number)
				.Select(x => new Account
				{
					Id = x.Id,
					AccountNumber = x.Number,
					Status = x.Status,
					Branch = x.Branch.Name,
					AccountType = x.AccountType.Name,
					AccountKind = x.AccountType.Kind,
					Name = x.OwningUser.Name,
					IdentityNumber = x.IdentityNumber,
					Reason = x.Reason,
					CreatedDate = x.CreatedDate
				})
				.FirstOrDefaultAsync().ConfigureAwait(false);

			if (account != null)
			{
				var credit = await this.dbContext.Transactions
					.Where(x => x.AccountId == accountId && x.TransactionType == TransactionType.Credit)
					.SumAsync(x => x.Amount)
					.ConfigureAwait(false);

				var debit = await this.dbContext.Transactions
					.Where(x => x.AccountId == accountId && x.TransactionType == TransactionType.Debit)
					.SumAsync(x => x.Amount)
					.ConfigureAwait(false);

				account.Balance = credit - debit;
			}

			return account;
		}

		public async Task<List<Account>> GetAccountsAsync(Guid? userId)
		{
			var accounts = await this.dbContext.Accounts
				.Include(x => x.OwningUser)
				.Include(x => x.AccountType)
				.Include(x => x.Branch)
				.Include(x => x.Nominees)
				.Where(x => x.OwningUserId == userId || x.Nominees.Any(x => x.NomineeUserId == userId))
				.OrderBy(p => p.Number)
				.Select(x => new Account
				{
					Id = x.Id,
					AccountNumber = x.Number,
					IdentityNumber = x.IdentityNumber,
					Status = x.Status,
					Branch = x.Branch.Name,
					AccountType = x.AccountType.Name,
					AccountKind = x.AccountType.Kind,
					CreatedDate = x.CreatedDate,
					IsOwnAccount = x.Nominees.All(y => y.NomineeUserId != userId)
				})
				.ToListAsync().ConfigureAwait(false);

			return accounts;
		}

		public async Task<PagedResult<Account>> GetAccountsAsync(int startIndex, int pageSize)
		{
			var dbAccounts = this.dbContext.Accounts.AsNoTracking();

			if (this.userService.IsBankOwner())
			{
				dbAccounts = dbAccounts.Where(x => x.Branch.BankId == this.userService.BankId);
			}
			else
			{
				var branchIds = this.userService.GetEmployeeBranchIds();
				dbAccounts = dbAccounts.Where(x => branchIds.Contains(x.BranchId));
			}

			var list = await dbAccounts
				.Select(dummy => new
				{
					Total = dbAccounts.Count(),
					ThisPage = dbAccounts
						.Include(x => x.OwningUser)
						.Include(x => x.Branch)
						.Include(x => x.AccountType)
						.Include(x => x.Nominees)
						.OrderBy(p => p.Number)
						.Skip(startIndex).Take(pageSize)
						.Select(x => new Account
						{
							Id = x.Id,
							AccountNumber = x.Number,
							IdentityNumber = x.IdentityNumber,
							Status = x.Status,
							Branch = x.Branch.Name,
							AccountType = x.AccountType.Name,
							AccountKind = x.AccountType.Kind,
							Name = x.OwningUser.Name,
							CreatedDate = x.CreatedDate
						})
						.ToList()
				}).FirstOrDefaultAsync().ConfigureAwait(false);

			return new PagedResult<Account>(list?.ThisPage.ToList(), startIndex, pageSize, list?.Total ?? 0);
		}

		public async Task<AccountNominee> AddNomineeAsync(Guid accountId, string nomineeEmail, string registerUrl,
			string loginUrl)
		{
			var dbNomineeUser = await this.dbContext.Users.Include(x => x.PermitGroups)
				.SingleOrDefaultAsync(x => x.Username == nomineeEmail).ConfigureAwait(false);
			if (dbNomineeUser != null)
			{
				if (dbNomineeUser.UserType != UserType.Customer)
				{
					throw new OpException(OpResult.InvalidInput, string.Format(SFMessages.NotACustomerUser, nomineeEmail));
				}

				var dbAccountNominee = new DbAccountNominee
				{
					Id = Guid.NewGuid(),
					AccountId = accountId,
					NomineeUserId = dbNomineeUser.Id
				};

				this.dbContext.AccountNominees.Add(dbAccountNominee);
				await this.dbContext.SaveChangesAsync().ConfigureAwait(false);

				await this.permitRepository
					.AddPermitAsync(dbNomineeUser.Id, SFPermissionCodes.AccountNominee, accountId)
					.ConfigureAwait(false);

				await SendNomineeNotificationMailAsync(nomineeEmail, loginUrl).ConfigureAwait(false);

				return new AccountNominee
				{
					Id = dbAccountNominee.Id,
					Name = dbNomineeUser.Name,
					Username = dbNomineeUser.Username,
					AccountId = accountId
				};
			}
			else
			{
				var dbNomineeInvitation = new DbUserInvitation
				{
					Id = Guid.NewGuid(),
					Date = DateTime.UtcNow,
					AccountId = accountId,
					EmailAddress = nomineeEmail
				};

				this.dbContext.UserInvitations.Add(dbNomineeInvitation);
				await this.dbContext.SaveChangesAsync().ConfigureAwait(false);

				await SendNomineeInviteMailAsync(nomineeEmail, registerUrl).ConfigureAwait(false);

				return null;
			}
		}

		public async Task DeleteNomineeAsync(Guid id)
		{
			var dbAccountNominee = await this.dbContext.AccountNominees
				.Where(m => m.Id == id)
				.SingleOrDefaultAsync()
				.ConfigureAwait(false);

			if (dbAccountNominee != null)
			{
				await this.permitRepository
					.RemovePermitAsync(dbAccountNominee.NomineeUserId, SFPermissionCodes.AccountNominee, dbAccountNominee.AccountId)
					.ConfigureAwait(false);

				this.dbContext.AccountNominees.Remove(dbAccountNominee);
				await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
			}
			else
			{
				throw new OpException(OpResult.DoNotExist);
			}
		}

		public async Task<List<AccountNominee>> GetNomineesAsync(Guid accountId)
		{
			return await this.dbContext.AccountNominees.Where(x => x.AccountId == accountId)
				.OrderBy(x => x.NomineeUser.Name)
				.Select(x => new AccountNominee
				{
					Id = x.Id,
					Name = x.NomineeUser.Name,
					Username = x.NomineeUser.Username,
					AccountId = x.AccountId
				})
				.ToListAsync().ConfigureAwait(false);
		}

		public async Task SendNomineeNotificationMailAsync(string email, string loginUrl)
		{
			var subject = "You're added as a nominee!";

			var body = EmailTemplate.BuildHtmlReply(new Dictionary<string, object>
				{
					{"link", loginUrl},
					{"subject", subject},
					{"sign", this.appContext.Title}
				},
				Template.NomineeNotification);

			await this.mailer.SendAsync(email, subject, body, true).ConfigureAwait(false);
		}

		public async Task SendNomineeInviteMailAsync(string email, string registerUrl)
		{
			var subject = "You're invited!";

			var body = EmailTemplate.BuildHtmlReply(new Dictionary<string, object>
				{
					{"link", registerUrl},
					{"subject", subject},
					{"sign", this.appContext.Title},
					{"product", this.appContext.Title}
				},
				Template.NomineeInvite);

			await this.mailer.SendAsync(email, subject, body, true).ConfigureAwait(false);
		}

		private async Task<Account> AddAccountAsync(DbAccount dbAccount, string nomineeEmail, string registerUrl, string loginUrl, double openingAmount)
		{
			try
			{
				this.dbContext.Accounts.Add(dbAccount);

				await this.permitRepository
					.AddPermitAsync(dbAccount.OwningUserId, SFPermissionCodes.AccountHolder, dbAccount.Id)
					.ConfigureAwait(false);

				if (!string.IsNullOrWhiteSpace(nomineeEmail))
				{
					await AddNomineeAsync(dbAccount.Id, nomineeEmail, registerUrl, loginUrl).ConfigureAwait(false);
				}

				await this.dbContext.SaveChangesAsync().ConfigureAwait(false);

				dbAccount = await this.dbContext.Accounts.Where(x => x.Id == dbAccount.Id)
					.Include(x => x.AccountType)
					.Include(x => x.Branch)
					.Include(x => x.Nominees)
					.FirstOrDefaultAsync().ConfigureAwait(false);

				await this.userService.RefreshPermissionsAsync().ConfigureAwait(false);

				if (openingAmount > 0)
				{
					var transactionModel = new TransactionModel
					{
						AccountId = dbAccount.Id,
						Amount = openingAmount
					};

					if (dbAccount.AccountType.Kind == AccountKind.Investment)
					{
						await this.transactionManager.CreateDepositAsync(transactionModel).ConfigureAwait(false);
					}
					else
					{
						await this.transactionManager.CreateWithdrawalAsync(transactionModel).ConfigureAwait(false);
					}
				}

				return new Account
				{
					Id = dbAccount.Id,
					AccountNumber = dbAccount.Number,
					IdentityNumber = dbAccount.IdentityNumber,
					Status = dbAccount.Status,
					Branch = dbAccount.Branch.Name,
					AccountType = dbAccount.AccountType.Name,
					AccountKind = dbAccount.AccountType.Kind
				};
			}
			catch (Exception ex)
			{
				await this.logger.ErrorAsync(ex).ConfigureAwait(false);

				if (ex.GetBaseException() is SqlException sqlEx &&
					sqlEx.Number.In((int)SqlErrors.KeyViolation, (int)SqlErrors.UniqueIndex))
				{
					throw new OpException(OpResult.AlreadyExists,
						string.Format(SFMessages.CannotAddDuplicateAccount, dbAccount.Number));
				}

				throw;
			}
		}
	}
}

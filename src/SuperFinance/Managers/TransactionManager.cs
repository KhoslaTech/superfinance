using System;
using System.Linq;
using System.Threading.Tasks;
using ASKSource.Models;
using ASPSecurityKit;
using Microsoft.EntityFrameworkCore;
using SuperFinance.DataModels;
using SuperFinance.Models;
using SuperFinance.ViewModels;

namespace SuperFinance.Managers
{
	public interface ITransactionManager
	{
		Task CreateDepositAsync(TransactionModel transaction);

		Task CreateWithdrawalAsync(TransactionModel transaction);

		Task CreateTransferAsync(TransferModel transfer);

		Task<PagedResult<Transaction>> GetTransactionsAsync(Guid accountId, int startIndex, int pageSize);
	}

	public class TransactionManager : ITransactionManager
	{
		private readonly DemoDbContext dbContext;
		private readonly ILogger logger;

		public TransactionManager(DemoDbContext dbContext, ILogger logger)
		{
			this.dbContext = dbContext;
			this.logger = logger;
		}

		public async Task CreateDepositAsync(TransactionModel transaction)
		{
			var dbTransaction = new DbTransaction
			{
				Id = Guid.NewGuid(),
				Date = DateTime.UtcNow,
				AccountId = transaction.AccountId,
				Amount = transaction.Amount,
				TransactionType = TransactionType.Credit,
				Remarks = transaction.Remarks
			};

			this.dbContext.Transactions.Add(dbTransaction);
			await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task CreateTransferAsync(TransferModel transfer)
		{
			var toAccount = await this.dbContext.Accounts.FirstOrDefaultAsync(x => x.Number == transfer.ToAccountNumber)
				.ConfigureAwait(false);
			if (toAccount == null)
			{
				throw new OpException(OpResult.DoNotExist, "Account does not exist");
			}

			var dbDebitTransaction = new DbTransaction
			{
				Id = Guid.NewGuid(),
				Date = DateTime.UtcNow,
				AccountId = transfer.FromAccountId,
				Amount = transfer.Amount,
				TransactionType = TransactionType.Debit,
				Remarks = transfer.Remarks
			};

			var dbCreditTransaction = new DbTransaction
			{
				Id = Guid.NewGuid(),
				Date = DateTime.UtcNow,
				AccountId = toAccount.Id,
				Amount = transfer.Amount,
				TransactionType = TransactionType.Credit,
				Remarks = transfer.Remarks
			};

			var dbTransfer = new DbTransfer
			{
				Id = Guid.NewGuid(),
				CreditTransactionId = dbCreditTransaction.Id,
				DebitTransactionId = dbDebitTransaction.Id,
				CreatedDate = DateTime.UtcNow
			};

			this.dbContext.Transactions.AddRange(dbDebitTransaction, dbCreditTransaction);
			this.dbContext.Transfers.Add(dbTransfer);

			await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task CreateWithdrawalAsync(TransactionModel transaction)
		{
			var dbTransaction = new DbTransaction
			{
				Id = Guid.NewGuid(),
				Date = DateTime.UtcNow,
				AccountId = transaction.AccountId,
				Amount = transaction.Amount,
				TransactionType = TransactionType.Debit,
				Remarks = transaction.Remarks
			};

			this.dbContext.Transactions.Add(dbTransaction);
			await this.dbContext.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task<PagedResult<Transaction>> GetTransactionsAsync(Guid accountId, int startIndex, int pageSize)
		{
			var list = await this.dbContext.Transactions.AsNoTracking()
				.Select(dummy => new
				{
					Total = this.dbContext.Transactions.Count(p => p.AccountId == accountId),
					ThisPage = this.dbContext.Transactions.Where(p => p.AccountId == accountId)
						.OrderBy(p => p.Date).Skip(startIndex).Take(pageSize)
						.Select(x => new Transaction
						{
							Id = x.Id,
							Date = x.Date,
							TransactionType = x.TransactionType,
							Amount = x.Amount,
							Remarks = x.Remarks
						})
						.ToList()
				}).FirstOrDefaultAsync().ConfigureAwait(false);

			return new PagedResult<Transaction>(list?.ThisPage.ToList(), startIndex, pageSize, list?.Total ?? 0);
		}
	}
}
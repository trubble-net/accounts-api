﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Picassi.Core.Accounts.Reports;
using Picassi.Core.Accounts.ViewModels.Transactions;
using Picassi.Data.Accounts.Database;
using Picassi.Data.Accounts.Models;

namespace Picassi.Core.Accounts.Services.Transactions
{
    public interface ITransactionUploadService
    {
        void AddTransactionsToAccount(int accountId, ICollection<TransactionUploadModel> transactions);
        void ConfirmTransactions(int accountId, IEnumerable<int> transactionids);
    }

    public class TransactionUploadService : ITransactionUploadService
    {
        private readonly IAccountsDataContext _dbContext;
        private readonly IAccountBalanceService _balanceService;

        public TransactionUploadService(IAccountsDataContext dbContext, IAccountBalanceService balanceService)
        {
            _dbContext = dbContext;
            _balanceService = balanceService;
        }

        public void AddTransactionsToAccount(int accountId, ICollection<TransactionUploadModel> transactions)
        {
            var maxOrdinal = _dbContext.Transactions.Any()
                ? _dbContext.Transactions.Max(transaction => transaction.Ordinal)
                : 0;

            var transactionModels = transactions
                .OrderBy(x => x.Ordinal)
                .Select(transaction => CreateTransaction(accountId, transaction, ++maxOrdinal))
                .ToList();

            _dbContext.Transactions.AddRange(transactionModels);
            _dbContext.SaveChanges();
            _balanceService.SetTransactionBalances(accountId, transactionModels.Min(transaction => transaction.Date));
        }

        public void ConfirmTransactions(int accountId, IEnumerable<int> transactionids)
        {
            var firstDate = _dbContext.Transactions.Where(x => transactionids.Contains(x.Id)).Min(x => x.Date);
            foreach (var transaction in _dbContext.Transactions.Where(x => transactionids.Contains(x.Id)))
            {
                transaction.Status = TransactionStatus.Confirmed;
            }
            _balanceService.SetTransactionBalances(accountId, firstDate);
            _dbContext.SaveChanges();

        }

        private Transaction CreateTransaction(int baseAccountId, TransactionUploadModel transaction, int ordinal)
        {
            return new Transaction
            {
                Amount = GetTransactionAmount(transaction),
                Date = DateTime.ParseExact(transaction.Date, "dd/MM/yyyy", CultureInfo.CurrentCulture),
                Description = transaction.Description,
                CategoryId = _dbContext.Categories.SingleOrDefault(x => x.Name == transaction.CategoryName)?.Id,
                FromId = (transaction.Amount < 0 || transaction.Amount == 0 && transaction.Debit > 0) ? baseAccountId : (int?)null,
                ToId = (transaction.Amount > 0 || transaction.Amount == 0 && transaction.Credit > 0) ? baseAccountId : (int?)null,
                Status = TransactionStatus.Provisional,
                Ordinal = ordinal
            };
        }

        private static decimal GetTransactionAmount(TransactionUploadModel transaction)
        {
            return transaction.Amount == 0
                ? transaction.Credit + transaction.Debit
                : (transaction.Amount > 0 ? transaction.Amount : -transaction.Amount);
        }
    }
}
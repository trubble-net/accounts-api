﻿using System.Collections.Generic;
using System.Linq;
using Picassi.Core.Accounts.ViewModels.Statements;
using Picassi.Core.Accounts.ViewModels.Transactions;

namespace Picassi.Core.Accounts.Reports
{
    public interface IStatementViewModelFactory
    {
        StatementViewModel CompileStatement(int accountId, decimal startBalance, List<TransactionViewModel> transactions, int? pageNumber, int? pageSize);
    }

    public class StatementViewModelFactory : IStatementViewModelFactory
    {
        public StatementViewModel CompileStatement(int accountId, decimal startBalance, List<TransactionViewModel> transactions, int? pageNumber, int? pageSize)
        {
            return new StatementViewModel
            {
                AccountId = accountId,
                StartBalance = startBalance,
                TotalLines = transactions.Count,
                Lines = GetStatementLines(accountId, startBalance, transactions, pageNumber, pageSize)
            };
        }

        private IEnumerable<StatementLineViewModel> GetStatementLines(int accountId, decimal startBalance, IEnumerable<TransactionViewModel> transactions,
            int? pageNumber, int? pageSize)
        {
            var cumulativeBalance = startBalance;
            var ret = (IEnumerable<TransactionViewModel>)transactions.OrderBy(x => x.Date);
            if (pageSize != null) ret = ret.Skip((int)pageSize * ((pageNumber ?? 1) - 1)).Take((int)pageSize);                
            return ret.Select(transaction => GetStatementLine(transaction, accountId))
                .ToList();
        }

        private static StatementLineViewModel GetStatementLine(TransactionViewModel transaction, int accountId)
        {
            var transactionsValue = transaction.FromId == accountId ? -transaction.Amount : transaction.Amount;
            var amountIn = transactionsValue > 0 ? transactionsValue : 0;
            var amountOut = transactionsValue < 0 ? -transactionsValue : 0;
            var linkedAccount = transaction.FromId == accountId ? transaction.ToId : transaction.FromId;

            return new StatementLineViewModel
            {
                TransactionId = transaction.Id,
                LinkedAccountId = linkedAccount,
                Balance = transaction.Balance,
                Date = transaction.Date,
                Description = transaction.Description,
                Amount = transactionsValue,
                Credit = amountIn,
                Debit = amountOut,
                CategoryId = transaction.CategoryId
            };
        }

    }
}

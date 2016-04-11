﻿using System;
using System.Collections.Generic;
using System.Linq;
using Picassi.Core.Accounts.Time;
using Picassi.Data.Accounts.Database;
using Picassi.Data.Accounts.Models;

namespace Picassi.Core.Accounts.ViewModels.Categories
{
    public interface ICategorySummaryViewModelFactory
    {
        CategorySummaryViewModel GetSummary(CategorySummaryQueryModel query);
    }

    public class CategorySummaryViewModelFactory : ICategorySummaryViewModelFactory
    {
        private readonly IAccountsDataContext _dataContext;
        private readonly IPeriodCalculator _periodCalculator;

        public CategorySummaryViewModelFactory(IAccountsDataContext dataContext, IPeriodCalculator periodCalculator)
        {
            _dataContext = dataContext;
            _periodCalculator = periodCalculator;
        }

        public CategorySummaryViewModel GetSummary(CategorySummaryQueryModel query)
        {
            var transactionsForCategory = query.DateRange != null ? GetFilteredTransactions(query) : null;

            var averageSpend = transactionsForCategory != null ? GetAverageSpend(query, transactionsForCategory.ToList()) : (decimal?)null;

            return new CategorySummaryViewModel
            {
                Id = query.Category?.Id,
                Name = query.Category?.Name ?? "Uncategorised",
                AverageSpend = averageSpend
            };
        }

        private IEnumerable<Transaction> GetFilteredTransactions(CategorySummaryQueryModel query)
        {
            var transactions = _dataContext.Transactions.AsQueryable();
            transactions = FilterTransactionsByCategory(transactions, query.Category?.Id);
            transactions = FilterTransactionsByDateRange(transactions, query.DateRange);
            return transactions.ToList();
        }

        private static IQueryable<Transaction> FilterTransactionsByCategory(IQueryable<Transaction> transactions, int? categoryId)
        {
            return categoryId == null ? transactions.Where(x => x.CategoryId == null) : transactions.Where(x => x.CategoryId == categoryId);
        }

        private static IQueryable<Transaction> FilterTransactionsByDateRange(IQueryable<Transaction> transactions, DateRange range)
        {
            return transactions.Where(x => x.Date >= range.Start && x.Date < range.End);
        }

        private decimal GetAverageSpend(CategorySummaryQueryModel query, IReadOnlyCollection<Transaction> transactions)
        {
            var totalIn = transactions.Where(x => x.ToId != null && query.AccountIds.Contains((int)x.ToId)).Sum(x => x.Amount);
            var totalOut = transactions.Where(x => x.FromId != null && query.AccountIds.Contains((int)x.FromId)).Sum(x => x.Amount);
            var numberOfPeriods = _periodCalculator.GetNumberOfPeriods(query.AverageSpendPeriod, query.DateRange);
            var total = (totalIn - totalOut) / numberOfPeriods;
            return Math.Round(total, 2);
        }
    }
}

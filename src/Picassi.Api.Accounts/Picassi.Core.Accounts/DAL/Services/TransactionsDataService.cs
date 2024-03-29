using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Picassi.Api.Accounts.Contract.Enums;
using Picassi.Api.Accounts.Contract.Transactions;
using Picassi.Core.Accounts.DAL.Entities;
using Picassi.Core.Accounts.Events;
using Picassi.Core.Accounts.Events.Messages;
using Picassi.Core.Accounts.Exceptions;
using Picassi.Core.Accounts.Models.Transactions;
using Picassi.Core.Accounts.Services;
using Picassi.Core.Accounts.Extensions;

namespace Picassi.Core.Accounts.DAL.Services
{
    public interface ITransactionsDataService : IGenericDataService<TransactionModel>
    {
        IEnumerable<TransactionModel> Query(
            string text = null,
            int[] accounts = null,
            int[] categories = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            bool? showUncategorised = true,
            bool? showAllCategories = null,
            bool? includeSubcategories = true,
            int? pageSize = null,
            int? pageNumber = null,
            string sortBy = null,
            bool? sortAscending = true);

        TransactionsResultsViewModel QueryWithCount(
            string text = null, 
            int[] accounts = null, 
            int[] categories = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            bool? showUncategorised = null,
            bool? showAllCategories = null,
            bool? includeSubcategories = true,
            int? pageSize = null,
            int? pageNumber = null,
            string sortBy = null,
            bool? sortAscending = null);

        IEnumerable<TransactionModel> SetTransactionRecurrences(IList<int> transactionIds, PeriodType? recurrence);

        void MoveTransactionUp(int accountId, int transactionId);

        void MoveTransactionDown(int accountId, int transactionId);
    }

    public class TransactionsDataService : GenericDataService<TransactionModel, Transaction>, ITransactionsDataService
    {
        public TransactionsDataService(ITransactionModelMapper modelMapper, IAccountsDatabaseProvider dbProvider) 
            : base(modelMapper, dbProvider)
        {
        }

        public override TransactionModel Create(TransactionModel model)
        {
            var dataModel = ModelMapper.CreateEntity(model);
            DbProvider.GetDataContext().Transactions.Add(dataModel);
            DbProvider.GetDataContext().SaveChanges();
            return Get(dataModel.Id);
        }

        public override TransactionModel Get(int id)
        {
            var entity = DbProvider.GetDataContext().Transactions
                .Include("Account")
                .Include("Category")
                .SingleOrDefault(x => x.Id == id);
            if (entity == null) throw new EntityNotFoundException<Transaction>(id);

            return ModelMapper.Map(entity);
        }

        public IEnumerable<TransactionModel> Query(
            string text = null,
            int[] accounts = null,
            int[] categories = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            bool? showUncategorised = null,
            bool? showAllCategories = null,
            bool? includeSubcategories = true,
            int? pageSize = null,
            int? pageNumber = null,
            string sortBy = null,
            bool? sortAscending = null)
        {
            var transactions = DbProvider.GetDataContext().Transactions.Include(x => x.Category);
            var results = FilterTransactions(text, accounts, categories, dateFrom, dateTo, showUncategorised, 
                showAllCategories, includeSubcategories, pageSize, pageNumber, sortBy, sortAscending == true, transactions).ToList();

            return ModelMapper.MapList(results);
        }

        public TransactionsResultsViewModel QueryWithCount(
            string text = null,
            int[] accounts = null,
            int[] categories = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            bool? showUncategorised = null,
            bool? showAllCategories = null,
            bool? includeSubcategories = true,
            int? pageSize = null,
            int? pageNumber = null,
            string sortBy = null,
            bool? sortAscending = null)
        {
            var transactions = DbProvider.GetDataContext().Transactions.Include(x => x.Category).Include(x => x.Account);
            var results = FilterTransactions(text, accounts, categories, dateFrom, dateTo, 
                showUncategorised ?? false, showAllCategories, includeSubcategories != false, 
                pageSize, pageNumber, sortBy, sortAscending != false, transactions).ToList();
            var count = FilterTransactionsWithoutPaging(text, accounts, categories, dateFrom, dateTo, 
                showUncategorised ?? false, showAllCategories ?? false, includeSubcategories != false, transactions).Count();

            return new TransactionsResultsViewModel
            {
                TotalLines = count,
                Total = results.Sum(x => x.Amount),
                Lines = ModelMapper.MapList(results)
            };
        }

        public IEnumerable<TransactionModel> SetTransactionRecurrences(IList<int> transactionIds, PeriodType? recurrence)
        {
            var transactions = DbProvider.GetDataContext().Transactions
                .Include(t => t.ScheduledTransaction)
                .Where(t => transactionIds.Contains(t.Id)).ToList();

            foreach (var transaction in transactions)
            {                
                if (recurrence != null)
                {
                    if (transaction.ScheduledTransactionId == null)
                    {
                        var scheduledTransaction = new ScheduledTransaction
                        {
                            AccountId = transaction.AccountId,
                            Description = transaction.Description,
                            ToId = transaction.ToId,
                            Amount = transaction.Amount,
                            CategoryId = transaction.CategoryId,
                            Recurrence = recurrence,
                            RecurrenceDayOfMonth = recurrence == PeriodType.Month ? transaction.Date.Day : (int?)null,
                            RecurrenceDayOfWeek = recurrence == PeriodType.Week ? transaction.Date.DayOfWeek : (DayOfWeek?)null
                        };
                        DbProvider.GetDataContext().ScheduledTransactions.Add(scheduledTransaction);
                        transaction.ScheduledTransaction = scheduledTransaction;
                    }
                    else
                    {
                        transaction.ScheduledTransaction.Recurrence = recurrence;
                        transaction.ScheduledTransaction.RecurrenceDayOfMonth =
                            recurrence == PeriodType.Month ? transaction.Date.Day : (int?) null;
                        transaction.ScheduledTransaction.RecurrenceDayOfWeek =
                            recurrence == PeriodType.Week ? transaction.Date.DayOfWeek : (DayOfWeek?) null;
                    }
                }
                else
                {
                    transaction.ScheduledTransactionId = null;
                    DbProvider.GetDataContext().ScheduledTransactions.Remove(transaction.ScheduledTransaction);
                }
            }

            DbProvider.GetDataContext().SaveChanges();

            var results = DbProvider.GetDataContext().Transactions
                .Include(t => t.Category).Include(t => t.Account).Include(t => t.To)
                .Where(t => transactionIds.Contains(t.Id)).ToList();
            return ModelMapper.MapList(results);
        }

        private static IEnumerable<Transaction> FilterTransactions(string text, int[] accounts, int[] categories,
            DateTime? dateFrom, DateTime? dateTo, bool? showUncategorised, bool? showAllCategories, bool? includeSubcategories,
            int? pageSize, int? pageNumber, string sortBy, bool sortAscending, IQueryable <Transaction> transactions)
        {
            transactions = FilterTransactionsWithoutPaging(text, accounts, categories, dateFrom, dateTo, 
                ShouldShouldUncategorised(showUncategorised, categories), showAllCategories ?? false, includeSubcategories != false, transactions);
            transactions = OrderResults(transactions, sortBy, sortAscending);
            if (pageNumber != null && pageSize != null)
            {
                transactions = PageResults(transactions, (int)pageNumber, (int)pageSize);
            }
            return transactions;
        }

        private static IQueryable<Transaction> FilterTransactionsWithoutPaging(string text, int[] accounts, int[] categories,
            DateTime? dateFrom, DateTime? dateTo, bool showUncategorised, bool showAllCategories, bool includeSubcategories, IQueryable<Transaction> transactions)
        {
            transactions = FilterText(transactions, text);
            transactions = FilterAccounts(transactions, accounts);
            transactions = FilterCategories(transactions, categories, showUncategorised, showAllCategories, includeSubcategories);
            transactions = FilterDate(transactions, dateFrom, dateTo);
            return transactions;
        }

        private static IQueryable<Transaction> FilterText(IQueryable<Transaction> transactions, string text)
        {
            return text == null ? transactions : transactions.Where(x => x.Description != null && x.Description.Contains(text));
        }

        private static IQueryable<Transaction> FilterDate(IQueryable<Transaction> transactions, DateTime? start, DateTime? end)
        {
            return transactions.Where(x => (start == null || x.Date >= start) && (end == null || x.Date < end));
        }

        private static IQueryable<Transaction> FilterAccounts(IQueryable<Transaction> transactions, int[] accountIds)
        {
            return accountIds == null
                ? transactions
                : transactions.Where(x => accountIds.Contains((int) x.AccountId));
        }

        private static IQueryable<Transaction> FilterCategories(IQueryable<Transaction> transactions, 
            int[] categoryIds, bool showUncategorised, bool showAllCategories, bool includeSubcategories)
        {
            if (categoryIds == null) categoryIds = new int[0];
            return transactions.Where(x =>
                (showUncategorised && x.CategoryId == null) ||
                (showAllCategories && x.CategoryId != null) ||
                (x.CategoryId != null && categoryIds.Contains((int)x.CategoryId)) ||
                (includeSubcategories && x.Category != null && x.Category.ParentId != null && categoryIds.Contains((int)x.Category.ParentId)));

        }

        private static IQueryable<Transaction> PageResults(IQueryable<Transaction> transactions, int pageNumber, int pageSize)
        {

            return pageSize < 0 ? transactions : transactions.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        private static IQueryable<Transaction> OrderResults(IQueryable<Transaction> transactions, string field, bool ascending)
        {
            if (field == null)
            {
                field = "Date";
                ascending = false;
            }

            return transactions.OrderBy(field, @ascending).ThenBy("Ordinal", @ascending);
        }

        private static bool ShouldShouldUncategorised(bool? showUncategorised, int[] categories)
        {
            return (showUncategorised == null && categories == null) || showUncategorised == true;
        }

        public void InsertTransactionAt(int id, int position)
        {
            DbProvider.GetDataContext().Database.ExecuteSqlCommand($"Update accounts.transactions set ordinal = ordinal + 1 where ordinal >= {position}");
            DbProvider.GetDataContext().Database.ExecuteSqlCommand($"Update accounts.transactions set ordinal = {position} where id = {id}");
        }

        public void MoveTransactionUp(int accountId, int transactionId)
        {
            var transaction = DbProvider.GetDataContext().Transactions.Find(transactionId);
            var targetTransaction = GetNextTransactionOnSameDay(accountId, transaction);
            MoveOrdinalFromAtoB(transactionId, transaction.Ordinal, targetTransaction?.Ordinal ?? transaction.Ordinal + 1);
            EventBus.Instance.Publish(new TransactionMoved(accountId, transaction.Date));
        }

        public void MoveTransactionDown(int accountId, int transactionId)
        {
            var transaction = DbProvider.GetDataContext().Transactions.Find(transactionId);
            var targetTransaction = GetPreviousTransactionOnSameDay(accountId, transaction);
            MoveOrdinalFromAtoB(transactionId, transaction.Ordinal, targetTransaction?.Ordinal ?? transaction.Ordinal + 1);
            EventBus.Instance.Publish(new TransactionMoved(accountId, transaction.Date));
        }

        private Transaction GetNextTransactionOnSameDay(int accountId, Transaction transaction)
        {
            var targetTransaction = DbProvider.GetDataContext().Transactions
                .Where(x => x.AccountId == accountId && x.Date == transaction.Date && x.Ordinal > transaction.Ordinal)
                .OrderBy(x => x.Ordinal)
                .FirstOrDefault();
            return targetTransaction;
        }

        private Transaction GetPreviousTransactionOnSameDay(int accountId, Transaction transaction)
        {
            var targetTransaction = DbProvider.GetDataContext().Transactions
                .Where(x => x.AccountId == accountId && x.Date == transaction.Date && x.Ordinal < transaction.Ordinal)
                .OrderByDescending(x => x.Ordinal).FirstOrDefault();
            return targetTransaction;
        }

        private void MoveOrdinalFromAtoB(int id, int start, int end)
        {
            var update = start > end
                ? $"Update accounts.transactions set ordinal = ordinal + 1 where ordinal < {start} and ordinal >= {end}"
                : $"Update accounts.transactions set ordinal = ordinal - 1 where ordinal > {start} and ordinal <= {end}";
            DbProvider.GetDataContext().Database.ExecuteSqlCommand(update);
            DbProvider.GetDataContext().Database.ExecuteSqlCommand($"Update accounts.transactions set ordinal = {end} where id = {id}");
        }
    }
}


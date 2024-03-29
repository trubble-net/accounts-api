﻿using System;
using System.Linq;
using Picassi.Core.Accounts.DAL;

namespace Picassi.Core.Accounts.Services.Accounts
{
    public interface IAccountStatusChecker
    {
        DateTime? LastUpdated(int? accountId);
        DateTime? LastUpdated();
        DateTime? MostOutOfDate();
    }

    public class AccountStatusChecker : IAccountStatusChecker
    {
        private readonly IAccountsDatabaseProvider _dataContext;

        public AccountStatusChecker(IAccountsDatabaseProvider dataContext)
        {
            _dataContext = dataContext;
        }

        public DateTime? LastUpdated(int? accountId)
        {
            return accountId == null 
                ? _dataContext.GetDataContext().Accounts.Where(x => x.Id == accountId).Select(x => x.LastUpdated).FirstOrDefault()
                : _dataContext.GetDataContext().Accounts.Select(x => x.LastUpdated).OrderByDescending(x => x).FirstOrDefault();
        }

        public DateTime? LastUpdated()
        {
            return _dataContext.GetDataContext().Accounts.Any() ? _dataContext.GetDataContext().Accounts.Max(account => account.LastUpdated) : (DateTime?)null;
        }

        public DateTime? MostOutOfDate()
        {
            return _dataContext.GetDataContext().Accounts.Any() ? _dataContext.GetDataContext().Accounts.Min(account => account.LastUpdated) : (DateTime?)null;
        }
    }
}

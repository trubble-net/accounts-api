﻿using System;
using System.Configuration;
using System.Linq;
using Microsoft.Owin;

namespace Picassi.Core.Accounts.DAL
{
    public interface IAccountsDatabaseProvider
    {
        IAccountsDataContext GetDataContext();
    }

    public class AccountsDatabaseProvider : IAccountsDatabaseProvider
    {
        private const string DbClaimType = "dbname";

        private readonly IOwinContext _context;
        private AccountsDataContext _dataContext;

        public AccountsDatabaseProvider(IOwinContext context)
        {
            _context = context;
        }

        public IAccountsDataContext GetDataContext()
        {
            return _dataContext ?? (_dataContext = BuildDataContext());
        }

        private AccountsDataContext BuildDataContext()
        {
            return new AccountsDataContext(BuildConnectionString(_context));
        }

        public static string BuildConnectionString(IOwinContext context)
        {
            if (ConfigurationManager.AppSettings["connection-type"] == "static")
            {
                return ConfigurationManager.ConnectionStrings["Accounts"].ConnectionString;
            }

            var principal = context.Authentication?.User;

            if (principal == null) throw new AccessViolationException("User has not been authenticated");

            var databaseName = principal.Claims.Single(x => x.Type == DbClaimType).Value;

            return BuildConnectionString(databaseName);
        }

        public static string BuildConnectionString(string databaseName)
        {
            var serverName = ConfigurationManager.AppSettings["db-server"] ?? "(local)";

            return $"Data Source={serverName};Initial Catalog={databaseName};Integrated Security=True;MultipleActiveResultSets=True;";
        }

    }
}
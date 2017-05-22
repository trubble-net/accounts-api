﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Reflection;
using Autofac;
using Picassi.Api.Accounts.Client;
using Picassi.Core.Accounts.DAL;
using Picassi.Utils.Api.Test;

namespace Picassi.Api.Accounts.Tests.Framework
{
    public class SandboxWrapper : IDisposable
    {
        private ApiSandbox _sandbox;

        public IPicassiAccountsApiClient ApiClient { get; }
        public IAccountsDataContext Database { get; set; }

        public SandboxWrapper()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            System.Data.Entity.Database.SetInitializer(new DropCreateDatabaseAlways<AccountsDataContext>());

            var builder = new ApiTestEnvironmentBuilder()
                .WithServicePort(TestPortProvider.GetFreeLocalhostBinding())
                .WithAuthPort(TestPortProvider.GetFreeLocalhostBinding())
                .WithWebApiAssembly(Assembly.GetAssembly(typeof(AccountsApiModule)))
                .WithAutofacModules(new AccountsApiModule())
                .WithScopes("accounts-user")
                .WithApplicationConfiguration("accounts-api", new[] { "accounts-user" })
                .WithMobileClientConfiguration("picassi-server", new[] { "accounts-user" });

            _sandbox = builder.BuildApiSandbox();

            var apiClient = builder.BuildClient("picassi-server");
            ApiClient = new PicassiAccountsApiClient(apiClient);

            Database = TestAutofacConfig.Container.Resolve<IAccountsDataContext>();
        }

        public void Dispose()
        {
            _sandbox?.Dispose();
            _sandbox = null;
        }
    }
}
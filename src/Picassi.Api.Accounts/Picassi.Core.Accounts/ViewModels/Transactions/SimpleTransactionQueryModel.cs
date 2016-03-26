﻿using System;

namespace Picassi.Core.Accounts.ViewModels.Transactions
{
    public class SimpleTransactionQueryModel
    {
        public int AccountId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}

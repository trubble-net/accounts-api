﻿using System;

namespace Picassi.Core.Accounts.Models.ModelledTransactions
{
    public class ModelledTransactionQueryModel
    {
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
        public string Text { get; set; }
        public int[] Categories { get; set; }
        public bool ShowAllCategorised { get; set; }
        public bool ShowUncategorised { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string SortBy { get; set; }
        public bool SortAscending { get; set; }
    }
}

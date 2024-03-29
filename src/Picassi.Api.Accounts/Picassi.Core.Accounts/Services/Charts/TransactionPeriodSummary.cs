using System;

namespace Picassi.Core.Accounts.Services.Charts
{
    public class TransactionPeriodSummary
    {
        public DateTime PeriodStart { get; set; }
        public decimal TotalSpending { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal EndBalance { get; set; }
        public decimal TotalChange => TotalIncome - TotalSpending;
    }
}
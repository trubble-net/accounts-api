﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using Picassi.Api.Accounts.Contract.Enums;
using Picassi.Core.Accounts.Time.Periods;

namespace Picassi.Core.Accounts.DAL.Entities
{
    public class ScheduledTransaction : IEntity
    {
        public int Id { get; set; }

        public string Description { get; set; }

        [ForeignKey("Account")]
        public int AccountId { get; set; }

        [ForeignKey("To")]
        public int? ToId { get; set; }

        public int? CategoryId { get; set; }

        public int? EventId { get; set; }

        public decimal Amount { get; set; }

        public DateTime? Date { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public int? DaysBefore { get; set; }

        public PeriodType? Recurrence { get; set; }

        public int? RecurrenceDayOfMonth { get; set; }

        public int? RecurrenceWeekOfMonth { get; set; }

        public DayOfWeek? RecurrenceDayOfWeek { get; set; }

        public virtual Account Account { get; set; }

        public virtual Account To { get; set; }

        public virtual Category Category { get; set; }

        public virtual Event Event { get; set; }
    }
}

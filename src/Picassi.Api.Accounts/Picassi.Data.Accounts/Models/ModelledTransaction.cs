﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Picassi.Data.Accounts.Models
{
    public class ModelledTransaction
    {
        public int Id { get; set; }

        public int Ordinal { get; set; }

        public string Description { get; set; }

        [ForeignKey("From")]
        public int? FromId { get; set; }

        [ForeignKey("To")]
        public int? ToId { get; set; }

        public int? CategoryId { get; set; }

        public int? EventId { get; set; }

        public int? ScheduledTransactionId { get; set; }

        public decimal Amount { get; set; }

        public decimal Balance { get; set; }

        public DateTime Date { get; set; }

        public virtual Account From { get; set; }

        public virtual Account To { get; set; }

        public virtual Category Category { get; set; }

        public virtual Event Event { get; set; }

        public virtual ScheduledTransaction ScheduledTransaction { get; set; }
    }
}

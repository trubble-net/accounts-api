﻿using System.Collections.Generic;

namespace Picassi.Data.Accounts.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? BudgetId { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }

        public virtual Budget Budget { get; set; }

    }
}
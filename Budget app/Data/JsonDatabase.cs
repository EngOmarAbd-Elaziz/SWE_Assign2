using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using BudgetApp.Interfaces;
using BudgetApp.Models;

namespace BudgetApp.Data
{
    internal class JsonDatabase
    {
        public List<BudgetCycle> BudgetCycles { get; set; } = new List<BudgetCycle>();
        public List<Expense> Expenses { get; set; } = new List<Expense>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public int NextCycleId { get; set; } = 1;
        public int NextExpenseId { get; set; } = 1;
        public int NextCategoryId { get; set; } = 1;

    }

}

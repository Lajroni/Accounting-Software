using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Data.Models.AccountingViewModel
{
    public class Filter
    {
        public string JournalID { get; set; }
        public string AccountName { get; set; }
        public string DebitValue { get; set; }
        public string CreditValue { get; set; }
        public string Status { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Data.Models.AccountingViewModel
{
    public class JoinedJournalTransaction
    {
        public bool isPosted { get; set; }

        public int JournalId { get; set; }

        public string AccountName { get; set; }

        public bool isDebit { get; set; }

        public double Value { get; set; }

        public DateTime AddedOn { get; set; }
    }
}

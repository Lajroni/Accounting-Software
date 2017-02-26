using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Data.Models.AccountingViewModel
{
    public class Transactions
    {
        [Key]
        public int Id { get; set; }

        public DateTime AddedOn { get; set; }

        [Required]
        public int JournalId { get; set; }

        [Required]
        public string AccountName { get; set; }

        [Required]
        public bool isDebit { get; set; }

        [Required]
        public double Value { get; set; }
    }
}

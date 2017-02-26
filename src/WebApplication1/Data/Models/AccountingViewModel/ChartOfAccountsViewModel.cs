using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Data.Models.AccountingViewModel
{
    public class ChartOfAccountsViewModel
    {
        [Required]
        [Key]
        public string AccountName { get; set; }

        [Required]
        public double Balance { get; set; }

        [Required]
        public double InitialBalance { get; set; }

        public bool isActive { get; set; }

        public DateTime AddedOn { get; set; }

        public string AddedBy { get; set; }

        public int Order { get; set; }
    }

    public class AccountPostModel
    {
        public string AccountName { get; set; }
    }
}

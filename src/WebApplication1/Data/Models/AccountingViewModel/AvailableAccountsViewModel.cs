using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Data.Models.AccountingViewModel
{
    public class AvailableAccountsViewModel
    {

        [Required]
        [Key]
        public string AccountName { get; set; }

        
        public string SubCategory { get; set; }


        public string Category { get; set; }

        [Required]
        public bool isLeftNormalSide { get; set; }

        [Required]
        public int AccountCode { get; set; }

       
        public string Description { get; set; }
    }   
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Data.Models.AccountingViewModel
{
    public class JournalizingViewModel
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime AddedOn { get; set; }

        [Required]
        public string AddedBy { get; set; }

        public string credits { get; set; }

        public string debits { get; set; }

        public string creditsval { get; set; }

        public string debitsval { get; set; }

        public bool isSubmited { get; set; }

        public bool isRejected { get; set; }

        public bool isApproved { get; set; }

        public bool isPosted { get; set; }
    }
}

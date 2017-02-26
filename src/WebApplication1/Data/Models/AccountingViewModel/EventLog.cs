using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Data.Models.AccountingViewModel
{
    public class EventLog
    {
        [Key]
        public int Id { get; set; }

        public string Description { get; set; }

        [Required]
        public string EditedBy { get; set; }

        [Required]
        public DateTime EditedOn { get; set; }
    }
}

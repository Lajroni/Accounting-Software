using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Data.Models.AccountingViewModel
{
    public class FileData
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string FileCode { get; set; }

        public string journalId { get; set; }
    }
}

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

        [Required]
        public SubCategory SubCategory { get; set; }

        [Required]
        public Category Category { get; set; }

        [Required]
        public bool isLeftNormalSide { get; set; }

        [Required]
        public int AccountCode { get; set; }

        [Required]
        public int Order { get; set; }

        [Required]
        public string Description { get; set; }

    }   
    public enum Category : byte
    {
        Asset = 1,
        Liability = 2,
        [Display(Name = "Owner's Equity")]
        OwnersEquity = 3,
        Revenue = 4,
        Expense = 5
    }

    public enum SubCategory : byte 
    {
        //Starts with 1: Asset. 2: Liability. 3: OE. 4: Revenue. 5: Expense
        [Display(Name = "Cash Related")]
        CashRelated = 10,
        Receivables = 12,
        Land = 13,
        Building = 14,
        Equipment = 15,
        [Display(Name = "Short-term Liability")]
        ShortTermPayable = 20,
        [Display(Name = "Employee Payable")]
        EmployeePayable = 21,
        [Display(Name = "Long-term Liability")]
        LongTermLiability = 22,
        [Display(Name = "Owner's Equity")]
        OwnersEquity = 30,
        [Display(Name = "Operating Revenues")]
        OperatingRevenues = 40,
        [Display(Name = "Other Revenues")]
        OtherRevenues = 41,
        Purchase = 50,
        [Display(Name = "Selling Expenses")]
        SellingExpense = 51,
        [Display(Name = "General Expenses")]
        GeneralExpense = 52,
        [Display(Name = "Other Expenses")]
        OtherExpense = 53,
        Inapplicable = 100,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using WebApplication1.Models;
using WebApplication1.Models.AccountViewModels;
using WebApplication1.Services;
using WebApplication1.Data.Models.AccountingViewModel;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ChartOfAccountsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ChartOfAccountsController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            DbContext = dbContext;
            _userManager = userManager;
        }

        public ApplicationDbContext DbContext { get; }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Views()
        {
            return View();
        }


        // GET: /ChartOfAccounts/Add
        [HttpGet]
        public ActionResult Add(string returnUrl = null)
        {
            //List<AvailableAccountsViewModel> accs2 = DbContext.AvailableAccounts.Where(c => !DbContext.ChartOfAccounts.Select(b => b.AccountName).Contains(c.AccountName)).ToList();
            //IEnumerable<string> accs = accs2 as IEnumerable<string>;
            //ViewData["AvailableAccounts"] = new SelectList(accs);
            List<string> accs2 = DbContext.AvailableAccounts.Where(c => !DbContext.ChartOfAccounts.Select(b => b.AccountName).Contains(c.AccountName)).Select(x => x.AccountName).ToList();
            IEnumerable<string> accs = accs2 as IEnumerable<string>;
            ViewData["AvailableAccounts"] = new SelectList(accs);
            return View();
        }

        // POST: /ChartOfAccounts/Add
        [HttpPost]
        public async Task<IActionResult> Add(ChartOfAccountsViewModel model, string returnurl = "")
        {
            var user = await _userManager.GetUserAsync(User);
            var email = user.Email;
            model.AddedBy = email;
            model.AddedOn = DateTime.Now;
            model.isActive = true;
            model.InitialBalance = model.Balance;
            model.Order = DbContext.AvailableAccounts.Where(x => x.AccountName == model.AccountName).Select(x => x.AccountCode).FirstOrDefault();
            if (ModelState.IsValid && model.AddedBy != null)
            {
                var addedAccount = DbContext.ChartOfAccounts.Add(model);
                await DbContext.SaveChangesAsync();
                EventLog log = new EventLog();
                log.EditedBy = user.Email;
                log.EditedOn = DateTime.Now;
                log.Description = "Account  " + model.AccountName + " added to chart of accounts with balance " + model.Balance;
                DbContext.EventLog.Add(log);
                DbContext.Entry(log).State = EntityState.Added;
                await DbContext.SaveChangesAsync();
            }
            List<string> accs2 = DbContext.ChartOfAccounts.Select(x => x.AccountName).ToList();
            ChartOfAccountsViewModel accs = await DbContext.ChartOfAccounts.FirstOrDefaultAsync();
            return Redirect("ViewAccounts");
        }

        [HttpGet]
        public async Task<IActionResult> AdminAddAccounts(string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdminAddAccounts(AvailableAccountsViewModel model, string returnurl = "")
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            if (ModelState.IsValid)
            {
                var addedAccount = DbContext.AvailableAccounts.Add(model);
                await DbContext.SaveChangesAsync();
                EventLog log = new EventLog();
                log.EditedBy = user.Email;
                log.Description = "Account " + model.AccountName + " created";
                log.EditedOn = DateTime.Now;
                DbContext.EventLog.Add(log);
                DbContext.Entry(log).State = EntityState.Added;
                await DbContext.SaveChangesAsync();
            }
            List<string> accs2 = DbContext.AvailableAccounts.Select(x => x.AccountName).ToList();
            AvailableAccountsViewModel accs = await DbContext.AvailableAccounts.FirstOrDefaultAsync();
            return Redirect("ViewAvailableAccounts");
        }

        [HttpGet]
        public async Task<IActionResult> ViewAccounts(string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            var accs2 = DbContext.ChartOfAccounts.OrderBy(c => c.Order).ToList();
            ViewData["Accounts"] = new SelectList(accs2);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ViewAvailableAccounts(string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            var accs2 = DbContext.AvailableAccounts.OrderBy(c => c.AccountCode).ToList();
            ViewData["Accounts"] = new SelectList(accs2);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditAccount(string id, string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            var acc = DbContext.AvailableAccounts.Where(x => x.AccountName == id).FirstOrDefault();
            var count = DbContext.ChartOfAccounts.Where(x => x.AccountName == id).Count();
            if (count > 0)
            {
                TempData["CannotEdit"] = "1";
                return RedirectToAction(nameof(ChartOfAccountsController.ViewAvailableAccounts), "ChartOfAccounts");
            }
            return View(acc);
        }

        [HttpPost]
        public async Task<IActionResult> EditAccount(AvailableAccountsViewModel model, string returnurl = "")
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            if (ModelState.IsValid)
            {
                DbContext.Entry(model).State = EntityState.Modified;
                await DbContext.SaveChangesAsync();
            }
            EventLog log = new EventLog();
            log.EditedBy = user.Email;
            log.EditedOn = DateTime.Now;
            log.Description = "Account in CoA " + model.AccountName + " edited";
            DbContext.EventLog.Add(log);
            DbContext.Entry(log).State = EntityState.Added;
            await DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(ChartOfAccountsController.ViewAvailableAccounts), "ChartOfAccounts");
        }

        [HttpGet]
        public IActionResult Journalizing([FromQuery]string journalid, [FromQuery]bool viewonly, string returnurl = "")
        {
            if (journalid != null)
            {
                var journal = DbContext.Journals.Where(x => x.Id == Convert.ToInt32(journalid)).FirstOrDefault();
                var transactions = DbContext.Transactions.Where(x => x.JournalId == journal.Id);
                ViewData["transactions"] = new List<Transactions>(transactions);
                ViewData["JournalId"] = journal.Id;
                ViewData["Date"] = journal.AddedOn;
                ViewData["hasID"] = true;
                if (viewonly == true || journal.isPosted)
                {
                    ViewData["ViewOnly"] = true;
                }
                else
                {
                    ViewData["ViewOnly"] = false;
                }
            }
            else
            {
                ViewData["ViewOnly"] = false;
                ViewData["transactions"] = "null";
            }


            List<string> accs2 = DbContext.AvailableAccounts.Where(c => DbContext.ChartOfAccounts.Where(d => d.isActive == true).Select(b => b.AccountName).Contains(c.AccountName)).Where(c => c.isLeftNormalSide == true).Select(x => x.AccountName).ToList();
            IEnumerable<string> accs = accs2 as IEnumerable<string>;
            ViewData["Debits"] = new SelectList(accs);
            List<string> accs4 = DbContext.AvailableAccounts.Where(c => DbContext.ChartOfAccounts.Where(d => d.isActive == true).Select(b => b.AccountName).Contains(c.AccountName)).Where(c => c.isLeftNormalSide == false).Select(x => x.AccountName).ToList();
            IEnumerable<string> accs3 = accs4 as IEnumerable<string>;
            ViewData["Credits"] = new SelectList(accs3);
            List<string> accs6 = DbContext.AvailableAccounts.Where(c => DbContext.ChartOfAccounts.Where(d => d.isActive == true).Select(b => b.AccountName).Contains(c.AccountName)).Select(x => x.AccountName).ToList();
            IEnumerable<string> accs5 = accs6 as IEnumerable<string>;
            ViewData["Accounts"] = new SelectList(accs5);
            ViewData["Files"] = DbContext.FileData.Where(x => x.journalId == journalid);
            if (DbContext.ChartOfAccounts.Where(d => d.isActive == false).Count() > 0)
            {
                ViewData["Inactive"] = "1";
            }
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Journalize([FromQuery]string journalid, ICollection<IFormFile> files)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            List<string> credits = Request.Query["credits"].ToString().Split(',').ToList();
            List<string> debits = Request.Query["debits"].ToString().Split(',').ToList();
            List<double> creditsval = Request.Query["creditsVal"].ToString().Split(',').ToList().Select(double.Parse).ToList();
            List<double> debitsval = Request.Query["debitsval"].ToString().Split(',').ToList().Select(double.Parse).ToList();

            double sum = Request.Query["creditsVal"].ToString().Split(',').ToList().Sum(x => double.Parse(x));
            double sum2 = Request.Query["debitsval"].ToString().Split(',').ToList().Sum(x => double.Parse(x));
            var success = "1";
            if (sum == sum2)
            {
                JournalizingViewModel model;
                if (Request.Query["isUpdate"].ToString().ToLower() == "true")
                {
                    var journalID = Request.Query["journalId"].ToString().ToLower();
                    journalid = journalID;
                    DbContext.Transactions.RemoveRange(DbContext.Transactions.Where(x => x.JournalId == Convert.ToInt32(journalID)));
                    model = DbContext.Journals.Where(x => x.Id == Convert.ToInt32(journalID)).FirstOrDefault();
                    model.isPosted = false;
                    model.isApproved = false;
                    model.isRejected = false;
                    model.isSubmited = true;
                    DbContext.Entry(model).State = EntityState.Modified;
                }
                else
                {
                    model = new JournalizingViewModel();
                    model.AddedBy = user.Email;
                    model.AddedOn = DateTime.Now;
                    model.isPosted = false;
                    model.isApproved = false;
                    model.isRejected = false;
                    model.isSubmited = true;
                    var addedAccount = DbContext.Journals.Add(model);
                    DbContext.Entry(model).State = EntityState.Added;
                }

                await DbContext.SaveChangesAsync();

                using (var e1 = debits.GetEnumerator())
                using (var e2 = debitsval.GetEnumerator())
                {
                    while (e1.MoveNext() && e2.MoveNext())
                    {
                        var item1 = e1.Current;
                        var item2 = e2.Current;
                        // use item1 and item2
                        Transactions transaction = new Transactions();
                        transaction.AccountName = item1;
                        transaction.isDebit = true;
                        transaction.JournalId = model.Id;
                        transaction.Value = item2;
                        transaction.AddedOn = DateTime.Now;
                        DbContext.Transactions.Add(transaction);
                        DbContext.Entry(transaction).State = EntityState.Added;

                        EventLog trans = new EventLog();
                        trans.EditedBy = user.Email;
                        trans.EditedOn = DateTime.Now;
                        trans.Description = "TransactionID " + transaction.Id + " created";
                        DbContext.EventLog.Add(trans);
                        DbContext.Entry(trans).State = EntityState.Added;

                        await DbContext.SaveChangesAsync();
                    }
                }
                using (var e1 = credits.GetEnumerator())
                using (var e2 = creditsval.GetEnumerator())
                {
                    while (e1.MoveNext() && e2.MoveNext())
                    {
                        var item1 = e1.Current;
                        var item2 = e2.Current;
                        // use item1 and item2
                        Transactions transaction = new Transactions();
                        transaction.AccountName = item1;
                        transaction.isDebit = false;
                        transaction.JournalId = model.Id;
                        transaction.Value = item2;
                        transaction.AddedOn = DateTime.Now;
                        DbContext.Transactions.Add(transaction);
                        DbContext.Entry(transaction).State = EntityState.Added;

                        EventLog trans = new EventLog();
                        trans.EditedBy = user.Email;
                        trans.EditedOn = DateTime.Now;
                        trans.Description = "TransactionID " + transaction.Id + " created";
                        DbContext.EventLog.Add(trans);
                        DbContext.Entry(trans).State = EntityState.Added;

                        await DbContext.SaveChangesAsync();
                    }
                }
                EventLog log = new EventLog();
                log.EditedBy = user.Email;
                log.EditedOn = DateTime.Now;
                log.Description = "JournalID " + model.Id + " created";
                DbContext.EventLog.Add(log);
                DbContext.Entry(log).State = EntityState.Added;
                await DbContext.SaveChangesAsync();
                success = "1";
            }
            else
            {
                success = "-1";
            }

            return Json(new { success = success });
        }

        [HttpGet]
        public async Task<IActionResult> PostJournals(string returnurl = "")
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            List<JournalizingViewModel> Journals = DbContext.Journals.Where(c => c.isPosted == false).ToList();
            using (var e1 = Journals.GetEnumerator())
            {
                while (e1.MoveNext())
                {
                    var item = e1.Current;
                    var id = item.Id;
                    List<Transactions> transactions = DbContext.Transactions.Where(c => c.JournalId == id).ToList();
                    using (var e2 = transactions.GetEnumerator())
                    {
                        while (e2.MoveNext())
                        {
                            var iteminside = e2.Current;
                            var orig = DbContext.ChartOfAccounts.Where(c => c.AccountName == iteminside.AccountName).FirstOrDefault();
                            if (orig != null)
                            {
                                if (iteminside.isDebit == DbContext.AvailableAccounts.Where(x => x.AccountName == orig.AccountName).FirstOrDefault().isLeftNormalSide)
                                { orig.Balance = orig.Balance + iteminside.Value; }
                                else
                                { orig.Balance = orig.Balance - iteminside.Value; }

                                await DbContext.SaveChangesAsync();
                            }
                        }
                    }
                    item.isPosted = true;

                    await DbContext.SaveChangesAsync();

                    EventLog log = new EventLog();
                    log.EditedBy = user.Email;
                    log.EditedOn = DateTime.Now;
                    log.Description = "JournallD " + item.Id + " posted";
                    DbContext.EventLog.Add(log);
                    DbContext.Entry(log).State = EntityState.Added;
                    await DbContext.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(ChartOfAccountsController.ViewAccounts), "ChartOfAccounts");
        }

        [HttpGet]
        public async Task<IActionResult> ViewJournals(Filter filter, [FromQuery]string page = "0", string sortOrder = "", [FromQuery]string posted = "false", [FromQuery]string JournalID = null, [FromQuery]string AccountName = null, [FromQuery]string DebitValue = null, [FromQuery]string CreditValue = null, [FromQuery]string Status = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            //var tranasctions = DbContext.Transactions.Where(x => x.isSubmited == false);
            //IEnumerable<string> accs5 = tranasctions as IEnumerable<string>;
            //ViewData["Transactinos"] = new SelectList(accs5);
            int pagen = Convert.ToInt32(page);
            if (Status != null) { filter.Status = Status; }
            if (JournalID != null) { filter.JournalID = JournalID; }
            if (DebitValue != null) { filter.DebitValue = DebitValue; }
            if (CreditValue != null) { filter.CreditValue = CreditValue; }
            if (Status != null) { filter.Status = Status; }
            if (filter.Status == "Posted")
            {
                posted = "true";
            }
            var journalsinit = DbContext.Journals.Where(x => posted == "true" ? x.isPosted == true : x.isPosted == false);
            if (filter.JournalID != null)
            {
                journalsinit = journalsinit.Where(y => y.Id == Convert.ToInt32(filter.JournalID));
            }
            if (filter.Status != null)
            {
                switch (filter.Status)
                {
                    case "Posted": journalsinit = journalsinit.Where(z => z.isPosted == true); break;
                    case "Rejected": journalsinit = journalsinit.Where(z => z.isRejected == true); break;
                    case "Submitted": journalsinit = journalsinit.Where(z => z.isSubmited == true && z.isPosted == false); break;
                }
            }
            if (filter.AccountName != null && filter.CreditValue != null && filter.DebitValue == null)
            {
                journalsinit = journalsinit.Where(k => DbContext.Transactions.Any(v => v.isDebit == false && v.AccountName == filter.AccountName && v.Value == Convert.ToDouble(filter.CreditValue) && v.JournalId == k.Id));
            }
            else if (filter.AccountName != null && filter.CreditValue == null && filter.DebitValue != null)
            {
                journalsinit = journalsinit.Where(k => DbContext.Transactions.Any(v => v.isDebit == true && v.AccountName == filter.AccountName && v.Value == Convert.ToDouble(filter.DebitValue) && v.JournalId == k.Id));

            }
            else
            {
                if (filter.AccountName != null)
                {
                    journalsinit = journalsinit.Where(k => DbContext.Transactions.Any(v => v.AccountName == filter.AccountName && v.JournalId == k.Id));
                }
                if (filter.CreditValue != null)
                {
                    journalsinit = journalsinit.Where(k => DbContext.Transactions.Any(v => v.isDebit == false && v.Value == Convert.ToDouble(filter.CreditValue) && v.JournalId == k.Id));
                }
                if (filter.DebitValue != null)
                {
                    journalsinit = journalsinit.Where(k => DbContext.Transactions.Any(v => v.isDebit == true && v.Value == Convert.ToDouble(filter.DebitValue) && v.JournalId == k.Id));
                }
            }
            var journals = journalsinit.OrderBy(y => y.AddedOn).Skip((10 * pagen)).Take(10);
            ViewBag.JournalIDSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            if (sortOrder == "name_desc")
            {
                journals = journals.OrderByDescending(x => x.Id);
            }
            switch (sortOrder)
            {
                case "name_desc":
                    journals = journals.OrderByDescending(x => x.Id);
                    break;
                case "Date":
                    journals = journals.OrderBy(x => x.AddedOn);
                    break;
                case "date_desc":
                    journals = journals.OrderByDescending(x => x.AddedOn);
                    break;
                default:
                    journals = journals.OrderBy(x => x.Id);
                    break;
            }
            var listval = new List<double>();
            var list = new List<SelectList>();
            var listdates = new List<string>();
            var listsubmitted = new List<int>();
            var listredgreen = new List<bool>();
            var listreasons = new List<string>();
            var listfiles = new List<int>();
            foreach (var item in journals)
            {
                listfiles.Add(DbContext.FileData.Where(x => x.journalId == item.Id + "").Count());
                listreasons.Add(item.reason);
                listdates.Add(item.AddedOn.ToString());
                var transactions = DbContext.Transactions.Where(x => x.JournalId == item.Id);
                if (item != null)
                {
                    list.Add(new SelectList(transactions));
                }
                double debit = 0;
                double credit = 0;
                foreach (var iteminside in transactions)
                {
                    if (iteminside.isDebit)
                    {
                        debit += iteminside.Value;
                    }
                    else
                    {
                        credit += iteminside.Value;
                    }
                    if (iteminside.isDebit == DbContext.AvailableAccounts.Where(z => z.AccountName == iteminside.AccountName).FirstOrDefault().isLeftNormalSide)
                    {
                        listredgreen.Add(true);
                    }
                    else
                    {
                        listredgreen.Add(false);
                    }
                }
                if (item.isApproved)
                {
                    listsubmitted.Add(4);
                }
                else if (item.isRejected)
                {
                    listsubmitted.Add(3);
                }
                else if (item.isSubmited)
                {
                    listsubmitted.Add(2);
                }
                else
                {
                    listsubmitted.Add(1);
                }
                listval.Add(debit);
                listval.Add(credit);
            }
            ViewBag.Files = DbContext.FileData.ToList();
            List<string> accs6 = DbContext.AvailableAccounts.Where(c => DbContext.ChartOfAccounts.Where(d => d.isActive == true).Select(b => b.AccountName).Contains(c.AccountName)).Select(x => x.AccountName).ToList();
            IEnumerable<string> accs5 = accs6 as IEnumerable<string>;
            ViewData["AccountName"] = new SelectList(accs5);
            ViewData["FilesCount"] = listfiles;
            ViewData["UserType"] = user.isManager ? 1 : (user.isAdmin ? 2 : 0);
            ViewData["Color"] = listredgreen;
            ViewData["Dates"] = listdates;
            ViewData["Submit"] = listsubmitted;
            ViewData["Totals"] = listval;
            ViewData["Transactions"] = list;
            ViewData["Query"] = "&Status=" + filter.Status + "&CreditValue=" + filter.CreditValue + "&DebitValue=" + filter.DebitValue + "&AccountName=" + filter.AccountName + "&JournalID=" + filter.JournalID + "&";
            ViewData["Reasons"] = listreasons;
            ViewData["PostedOnly"] = (posted.ToLower() == "true");
            if (journalsinit.Skip((10 * (pagen))).Count() > 10)
            {
                ViewData["NextPage"] = pagen + 1;
            }
            if (pagen > 0)
            {
                ViewData["PrevPage"] = pagen - 1;
            }
            return View();
        }
        [HttpGet]
        public IActionResult getdata([FromQuery]string accountname, string returnUrl = null)
        {
            var acc = DbContext.AvailableAccounts.Where(x => x.AccountName == accountname).FirstOrDefault();
            return Json(acc);
        }

        [HttpGet]
        public async Task<IActionResult> post([FromQuery]string journalid, [FromQuery]string all, string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            var journals = DbContext.Journals.Where(x => all == null ? x.Id == Convert.ToInt32(journalid) : x.isPosted == false);

            foreach (var journal in journals)
            {
                journal.isApproved = true;
                journal.isPosted = true;
                journal.isRejected = false;
                journal.isSubmited = true;
                EventLog log = new EventLog();
                log.EditedBy = user.Email;
                log.EditedOn = DateTime.Now;
                log.Description = "JournalID " + journal.Id + " posted";
                DbContext.Entry(log).State = EntityState.Added;
                foreach (var transaction in DbContext.Transactions.Where(x => x.JournalId == journal.Id))
                {
                    var acc = DbContext.ChartOfAccounts.Where(y => y.AccountName == transaction.AccountName).FirstOrDefault();
                    var isleft = DbContext.AvailableAccounts.Where(k => k.AccountName == transaction.AccountName).FirstOrDefault().isLeftNormalSide;
                    if (transaction.isDebit == isleft)
                    {
                        acc.Balance += transaction.Value;
                    }
                    else
                    {
                        acc.Balance -= transaction.Value;
                    }
                }
            }

            await DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(ChartOfAccountsController.ViewJournals), "ChartOfAccounts");
        }

        [HttpGet]
        public async Task<IActionResult> reject([FromQuery]string journalid, [FromQuery]string reason = "empty", string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            var journals = DbContext.Journals.Where(x => x.Id == Convert.ToInt32(journalid)).FirstOrDefault();
            journals.isApproved = false;
            journals.isPosted = false;
            journals.isRejected = true;
            journals.isSubmited = false;
            if (reason != "empty")
            {
                journals.reason = reason;
            }
            EventLog log = new EventLog();
            log.EditedBy = user.Email;
            log.EditedOn = DateTime.Now;
            log.Description = "JournalID " + journals.Id + " rejected";
            DbContext.EventLog.Add(log);
            DbContext.Entry(log).State = EntityState.Added;

            await DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(ChartOfAccountsController.ViewJournals), "ChartOfAccounts", new { Status = "Submitted"});
        }

        [HttpGet]
        public async Task<IActionResult> delete([FromQuery]string journalid, string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            DbContext.Journals.RemoveRange(DbContext.Journals.Where(x => x.Id == Convert.ToInt32(journalid)));
            EventLog log = new EventLog();
            log.EditedBy = user.Email;
            log.EditedOn = DateTime.Now;
            log.Description = "JournalID " + journalid + " deleted";
            DbContext.Entry(log).State = EntityState.Added;
            await DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(ChartOfAccountsController.ViewJournals), "ChartOfAccounts");
        }

        [HttpGet]
        public async Task<IActionResult> submitall([FromQuery]string journalid, string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }

            var journals = DbContext.Journals.Where(x => journalid == null ? x.isSubmited == false : x.Id == Convert.ToInt32(journalid));
            foreach (var item in journals)
            {
                item.isSubmited = true;
                item.isApproved = false;
                item.isPosted = false;
                item.reason = "empty";
                item.isRejected = false; ;
                EventLog log = new EventLog();
                log.EditedBy = user.Email;
                log.EditedOn = DateTime.Now;
                log.Description = "JournalID " + item.Id + " submitted";
                DbContext.Entry(log).State = EntityState.Added;
            }
            await DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(ChartOfAccountsController.ViewJournals), "ChartOfAccounts");
        }

        [HttpGet]
        public async Task<IActionResult> TrialBalance(string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            var listval = new List<double>();
            var listacc = new List<string>();
            var listside = new List<bool>();
            var listparenthesis = new List<bool>();
            var accounts = DbContext.ChartOfAccounts.OrderBy(x => x.Order);
            var debitval = 0.0;
            var creditval = 0.0;
            var listdebcred = new List<double>();
            foreach (var account in accounts)
            {
                var accdetails = DbContext.AvailableAccounts.OrderBy(x => x.AccountCode).Where(x => x.AccountName == account.AccountName).FirstOrDefault();
                if (account.Balance == 0)
                    continue;
                listacc.Add(account.AccountName);
                listval.Add(Math.Abs(account.Balance));
                if (accdetails.isLeftNormalSide)
                {
                    listside.Add(true);
                    debitval += account.Balance;
                }
                else
                {
                    listside.Add(false);
                    creditval += account.Balance;
                }
                if (account.Balance > 0)
                {
                    listparenthesis.Add(false);
                }
                else
                {
                    listparenthesis.Add(true);
                }
            }
            EventLog trans = new EventLog();
            trans.EditedBy = user.Email;
            trans.EditedOn = DateTime.Now;
            trans.Description = "Trial Balance computed for the period";
            DbContext.Entry(trans).State = EntityState.Added;
            listdebcred.Add(debitval);
            listdebcred.Add(creditval);
            ViewData["Accounts"] = listacc;
            ViewData["Values"] = listval;
            ViewData["Sides"] = listside;
            ViewData["Sum"] = listdebcred;
            ViewData["Parenthesis"] = listparenthesis;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> IncomeStatement(string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            var RevenueAccs = DbContext.ChartOfAccounts.Where(x => DbContext.AvailableAccounts.OrderBy(y => y.AccountCode).Any(v => v.Category == "Revenue" && v.AccountName == x.AccountName) && x.Balance != 0);
            var ExpenseAccs = DbContext.ChartOfAccounts.Where(x => DbContext.AvailableAccounts.OrderBy(y => y.AccountCode).Any(v => v.Category == "Expense" && v.AccountName == x.AccountName) && x.Balance != 0);
            var totalExp = 0.00;
            var totalRev = 0.00;
            foreach (var item in RevenueAccs)
            {
                totalRev += item.Balance;
            }
            foreach (var item in ExpenseAccs)
            {
                totalExp += item.Balance;
            }
            ViewData["TotalRev"] = totalRev;
            ViewData["TotalExp"] = totalExp;
            ViewData["NetProfit"] = (totalRev - totalExp);
            ViewData["RevenueAccs"] = RevenueAccs;
            ViewData["ExpenseAccs"] = ExpenseAccs;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> BalanceSheet(string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            var CurrentLiabilityAccs = DbContext.ChartOfAccounts.Where(x => DbContext.AvailableAccounts.OrderBy(y => y.AccountCode).Any(v => v.Category == "Liability" && v.AccountName == x.AccountName && (v.SubCategory == "ShortTermPayable" || v.SubCategory == "EmployeePayable" || v.SubCategory == "EmployerPayable" || v.SubCategory == "SalesTax")) && x.Balance != 0);
            var LongTermLiabilityAccs = DbContext.ChartOfAccounts.Where(x => DbContext.AvailableAccounts.OrderBy(y => y.AccountCode).Any(v => v.Category == "Liability" && v.AccountName == x.AccountName && (v.SubCategory == "DeferredRevenuesAndCurrentPortionLongTermDebt" || v.SubCategory == "LongTermLiabilities")) && x.Balance != 0);
            var CurrentAssetAccs = DbContext.ChartOfAccounts.Where(x => DbContext.AvailableAccounts.OrderBy(y => y.AccountCode).Any(v => v.Category == "Asset" && v.AccountName == x.AccountName && (v.SubCategory == "CashRelated" || v.SubCategory == "Receivables" || v.SubCategory == "Inventories" || v.SubCategory == "PrepaidItems")) && x.Balance != 0);
            var LongTermAssetAccs = DbContext.ChartOfAccounts.Where(x => DbContext.AvailableAccounts.OrderBy(y => y.AccountCode).Any(v => v.Category == "Asset" && v.AccountName == x.AccountName && (v.SubCategory != "CashRelated" && v.SubCategory != "Receivables" && v.SubCategory != "Inventories" && v.SubCategory != "PrepaidItems")) && x.Balance != 0);

            //            < option class="cat1 opt0 opt" value="CashRelated">Cash Related</option>
            //<option class="cat1 opt1 opt" value="Receivables">Receivables</option>
            //<option class="cat1 opt2 opt" value="Inventories">Inventories</option>
            //<option class="cat1 opt3 opt" value="PrepaidItems">PrepaidItems</option>
            //<option class="cat1 opt4 opt" value="LongTermInvestments">LongTermInvestments</option>
            //<option class="cat1 opt5 opt" value="Land">Land</option>
            //<option class="cat1 opt6 opt" value="Building">Building</option>
            //<option class="cat1 opt7 opt" value="Equipment">Equipment</option>
            //<option class="cat1 opt8 opt" value="Intangibles">Intangibles</option>
            var totalCurrentAsset = 0.00;
            var totalLongTermAsset = 0.00;
            var totalCurrentLiability = 0.00;
            var totalLongTermLiability = 0.00;
            foreach (var item in CurrentAssetAccs)
            {
                totalCurrentAsset += item.Balance;
            }
            foreach (var item in LongTermAssetAccs)
            {
                totalLongTermAsset += item.Balance;
            }
            foreach (var item in CurrentLiabilityAccs)
            {
                totalCurrentLiability += item.Balance;
            }
            foreach (var item in LongTermLiabilityAccs)
            {
                totalLongTermLiability += item.Balance;
            }
            ViewData["TotalCurrentAsset"] = totalCurrentAsset;
            ViewData["TotalLongTermAsset"] = totalLongTermAsset;
            ViewData["TotalCurrentLiablity"] = totalCurrentLiability;
            ViewData["TotalLongTermLiability"] = totalLongTermLiability;
            ViewData["TotalAsset"] = totalCurrentAsset + totalLongTermAsset;
            ViewData["TotalLiability"] = totalCurrentLiability + totalLongTermLiability;
            ViewData["CurrentAssetAccs"] = CurrentAssetAccs;
            ViewData["LongTermAssetAccs"] = LongTermAssetAccs;
            ViewData["CurrentLiabilityAccs"] = CurrentLiabilityAccs;
            ViewData["LongTermLiabilityAccs"] = LongTermLiabilityAccs;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> RetainedEarningsStatement(string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            var RevenueAccs = DbContext.ChartOfAccounts.Where(x => DbContext.AvailableAccounts.OrderBy(y => y.AccountCode).Any(v => v.Category == "Revenue" && v.AccountName == x.AccountName) && x.Balance != 0);
            var ExpenseAccs = DbContext.ChartOfAccounts.Where(x => DbContext.AvailableAccounts.OrderBy(y => y.AccountCode).Any(v => v.Category == "Expense" && v.AccountName == x.AccountName) && x.Balance != 0);
            var totalExp = 0.00;
            var totalRev = 0.00;
            foreach (var item in RevenueAccs)
            {
                totalRev += item.Balance;
            }
            foreach (var item in ExpenseAccs)
            {
                totalExp += item.Balance;
            }
            var NetIncome = (totalRev - totalExp);
            var OwnersEquity = DbContext.ChartOfAccounts.Where(x => x.AccountName == "Owner's Equity").FirstOrDefault();
            var OwnersEquityAccs = DbContext.ChartOfAccounts.Where(x => DbContext.AvailableAccounts.OrderBy(y => y.AccountCode).Any(v => v.Category == "OwnersEquity" && v.AccountName == x.AccountName) && x.Balance != 0);
            ViewData["OwnersEquityAcc"] = OwnersEquity;
            var listplusminus = new List<bool>();
            var OEFinal = OwnersEquity.InitialBalance;
            var listAdd = new List<ChartOfAccountsViewModel>();
            var listLess = new List<ChartOfAccountsViewModel>();
            foreach (var acc in OwnersEquityAccs)
            {
                var accdet = DbContext.AvailableAccounts.Where(x => x.AccountName == acc.AccountName).FirstOrDefault();
                if (accdet.isLeftNormalSide)
                {
                    listLess.Add(acc);
                    listplusminus.Add(false);
                    OEFinal -= acc.Balance;
                }
                else
                {
                    listAdd.Add(acc);
                    listplusminus.Add(true);
                    OEFinal += acc.Balance;
                }
            }
            OEFinal += NetIncome;
            ViewData["OwnersEquityAccs"] = OwnersEquityAccs;
            ViewData["AccsLess"] = listLess;
            ViewData["AccsAdd"] = listAdd;
            ViewData["side"] = listplusminus;
            ViewData["NetIncome"] = NetIncome;
            ViewData["OEFinal"] = OEFinal;
            return View();
        }
        [HttpGet]
        public IActionResult eventlog(string returnUrl = null)
        {
            var logs = DbContext.EventLog.OrderByDescending(x => x.EditedOn).ToList();
            ViewData["EventLogs"] = new SelectList(logs);
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ToggleActive(ChartOfAccountsViewModel model, string returnUrl = null)
        {
            var edit = Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(Request).Split('/')[Microsoft.AspNetCore.Http.Extensions.UriHelper.GetDisplayUrl(Request).Split('/').Length - 1];
            var acc = DbContext.ChartOfAccounts.Where(x => x.AccountName == edit).FirstOrDefault();
            var transactions = DbContext.Transactions.Where(x => x.AccountName == acc.AccountName).Count();
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            if (acc != null && transactions == 0)
            {
                acc.isActive = !acc.isActive;
                DbContext.Entry(acc).State = EntityState.Modified;
                DbContext.SaveChanges();
                EventLog log = new EventLog();
                log.EditedBy = user.Email;
                log.EditedOn = DateTime.Now;
                log.Description = "Account " + model.AccountName + " " + (acc.isActive ? "activated" : "deactivated");
                DbContext.EventLog.Add(log);
                DbContext.Entry(log).State = EntityState.Added;
                await DbContext.SaveChangesAsync();
            }
            else if (transactions > 0)
            {
                TempData["UnpostedJournals"] = "1";
                return RedirectToAction(nameof(ChartOfAccountsController.ViewAccounts), "ChartOfAccounts");
            }

            return RedirectToAction(nameof(ChartOfAccountsController.ViewAccounts), "ChartOfAccounts");
        }

        [HttpGet]
        public async Task<IActionResult> accountdetails([FromQuery]string accountname, string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            var account = DbContext.ChartOfAccounts.Where(x => x.AccountName == accountname).FirstOrDefault();
            var listJournals = new List<int>();
            var listDates = new List<DateTime>();
            var listCumulative = new List<double>();
            var currentbal = account.InitialBalance;
            listCumulative.Add(account.InitialBalance);
            var listredgreen = new List<bool>();
            var transactions = DbContext.Transactions.Where(x => x.AccountName == accountname);
            var transactions2 = DbContext.Transactions.Join(DbContext.Journals, c => c.JournalId, d => d.Id, (c, d) => new JoinedJournalTransaction { AccountName = c.AccountName, isPosted = d.isPosted, JournalId = c.JournalId, isDebit = c.isDebit, Value = c.Value, AddedOn = c.AddedOn }).Where(v => v.isPosted == true && v.AccountName == accountname);
            foreach (var item in transactions2)
            {
                listJournals.Add(item.JournalId);
                listDates.Add(item.AddedOn);
                var left = DbContext.AvailableAccounts.Where(z => z.AccountName == item.AccountName).FirstOrDefault().isLeftNormalSide;
                if (item.isDebit == DbContext.AvailableAccounts.Where(z => z.AccountName == item.AccountName).FirstOrDefault().isLeftNormalSide)
                {
                    listredgreen.Add(true);
                }
                else
                {
                    listredgreen.Add(false);
                }
                if (item.isDebit == DbContext.AvailableAccounts.Where(z => z.AccountName == item.AccountName).FirstOrDefault().isLeftNormalSide)
                {
                    currentbal += item.Value;
                    listCumulative.Add(currentbal);
                }
                else
                {
                    currentbal -= item.Value;
                    listCumulative.Add(currentbal);
                }
            }
            ViewData["Transactions"] = new SelectList(transactions2);
            ViewData["ListJournals"] = listJournals;
            ViewData["ListDates"] = listDates;
            ViewData["Account"] = account;
            ViewData["Colors"] = listredgreen;
            ViewData["Cumulative"] = listCumulative;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveFile(ICollection<IFormFile> files, [FromQuery]string journalId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            try
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Getting file into buffer.
                        byte[] buffer = null;
                        using (var stream = file.OpenReadStream())
                        {
                            buffer = new byte[stream.Length];
                            stream.Read(buffer, 0, (int)stream.Length);
                        }

                        // Converting buffer into base64 code.
                        string base64FileRepresentation = Convert.ToBase64String(buffer);

                        // Saving it into database.
                        DbContext.FileData.Add(new FileData()
                        {
                            Name = string.Format("{0}_{1}", DateTime.UtcNow.ToString("yyyyMMddHHmmss"), file.FileName),
                            FileCode = base64FileRepresentation,
                            journalId = journalId
                        });
                        await DbContext.SaveChangesAsync();
                        EventLog trans = new EventLog();
                        trans.EditedBy = user.Email;
                        trans.EditedOn = DateTime.Now;
                        trans.Description = "Document added to JournalID " + journalId;
                        DbContext.Entry(trans).State = EntityState.Added;
                    }
                }
                ViewBag.Files = DbContext.FileData.ToList();
                ViewBag.Message = "File uploaded successfully";
            }
            catch (Exception ex)
            {
                ViewBag.Message = string.Format("Error: {0}", ex.ToString());
            }

            return RedirectToAction(nameof(ChartOfAccountsController.ViewJournals), "ChartOfAccounts");
        }

        [HttpPost]
        public IActionResult getFiles(string journalId)
        {
            var files = DbContext.FileData.Where(x => x.journalId == journalId).ToList();
            return Json(files);
        }
        public FileStreamResult Download(string fileName)
        {
            // Fetching file encoded code from database.
            string code = DbContext.FileData.ToList().FirstOrDefault(x => x.Name.Equals(fileName)).FileCode;

            // Converting to code to byte array
            byte[] bytes = Convert.FromBase64String(code);

            // Converting byte array to memory stream.
            MemoryStream stream = new MemoryStream(bytes);

            // Create final file stream result.
            FileStreamResult fileStream = new FileStreamResult(stream, "*/*");

            // File name with file extension.
            fileStream.FileDownloadName = fileName;
            return fileStream;
        }

        [HttpPost]
        public string GetData(AccountPostModel model)
        {
            List<AvailableAccountsViewModel> accs2 = DbContext.AvailableAccounts.Where(x => x.AccountName == model.AccountName).ToList();
            return JsonConvert.SerializeObject(accs2);
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

    }
}
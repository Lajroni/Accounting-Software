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
                log.Description = "The account with account name " + model.AccountName + " has been initialized with balance " + model.Balance;
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
                log.Description = "The available account with account name " + model.AccountName + " has been created";
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
            var accs2 = DbContext.AvailableAccounts.OrderBy(c => c.Order).ToList();
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
            log.Description = "The available account with account name " + model.AccountName + " has been edited";
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
                if(viewonly == true)
                {
                    ViewData["ViewOnly"] = true;
                }else
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
            if (DbContext.ChartOfAccounts.Where(d => d.isActive == false).Count() > 0)
            {
                ViewData["Inactive"] = "1";
            }
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Journalize([FromQuery]string journalid)
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
                if(Request.Query["isUpdate"].ToString().ToLower() == "true")
                {
                    var journalID = Request.Query["journalId"].ToString().ToLower();
                    DbContext.Transactions.RemoveRange(DbContext.Transactions.Where(x => x.JournalId == Convert.ToInt32(journalID)));
                    DbContext.Journals.RemoveRange(DbContext.Journals.Where(x => x.Id == Convert.ToInt32(journalID)));
                }
                JournalizingViewModel model = new JournalizingViewModel();
                model.AddedBy = user.Email;
                model.AddedOn = DateTime.Now;
                model.isPosted = false;
                model.isApproved = false;
                model.isRejected = false;
                model.isSubmited = true;

                var addedAccount = DbContext.Journals.Add(model);
                DbContext.Entry(model).State = EntityState.Added;
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
                        DbContext.Transactions.Add(transaction);
                        DbContext.Entry(transaction).State = EntityState.Added;

                        EventLog trans = new EventLog();
                        trans.EditedBy = user.Email;
                        trans.EditedOn = DateTime.Now;
                        trans.Description = "The transaction with ID " + transaction.Id + " has been created";
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
                        DbContext.Transactions.Add(transaction);
                        DbContext.Entry(transaction).State = EntityState.Added;

                        EventLog trans = new EventLog();
                        trans.EditedBy = user.Email;
                        trans.EditedOn = DateTime.Now;
                        trans.Description = "The transaction with ID " + transaction.Id + " has been created";
                        DbContext.EventLog.Add(trans);
                        DbContext.Entry(trans).State = EntityState.Added;

                        await DbContext.SaveChangesAsync();
                    }
                }
                EventLog log = new EventLog();
                log.EditedBy = user.Email;
                log.EditedOn = DateTime.Now;
                log.Description = "The journal with ID " + model.Id + " has been created";
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
                        while (e2.MoveNext()) {
                            var iteminside = e2.Current;
                            var orig = DbContext.ChartOfAccounts.Where(c => c.AccountName == iteminside.AccountName).FirstOrDefault();
                            if (orig != null)
                            {
                                if (iteminside.isDebit == DbContext.AvailableAccounts.Where(x => x.AccountName == orig.AccountName).FirstOrDefault().isLeftNormalSide)
                                {orig.Balance = orig.Balance + iteminside.Value;}
                                else
                                {orig.Balance = orig.Balance - iteminside.Value;}
                                
                                await DbContext.SaveChangesAsync();
                            }
                        }
                    }
                    item.isPosted = true;

                    await DbContext.SaveChangesAsync();

                    EventLog log = new EventLog();
                    log.EditedBy = user.Email;
                    log.EditedOn = DateTime.Now;
                    log.Description = "The journal with ID " + item.Id  + " has been posted";
                    DbContext.EventLog.Add(log);
                    DbContext.Entry(log).State = EntityState.Added;
                    await DbContext.SaveChangesAsync();
                }
            }
                    return RedirectToAction(nameof(ChartOfAccountsController.ViewAccounts), "ChartOfAccounts");
        }

        [HttpGet]
        public async Task<IActionResult> ViewJournals([FromQuery]string page = "0", [FromQuery]string posted = "false")  
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
            var journals = DbContext.Journals.Where(x => posted == "true" ? x.isPosted == true : x.isPosted == false).OrderBy(y => y.AddedOn).Skip((10 * pagen)).Take(10);
            var listval = new List<double>();
            var list = new List<SelectList>();
            var listdates = new List<string>();
            var listsubmitted = new List<int>();
            var listredgreen = new List<bool>();
            foreach (var item in journals)
            {
                listdates.Add(item.AddedOn.ToString());
                var transactions = DbContext.Transactions.Where(x => x.JournalId == item.Id);
                if(item != null)
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
                    if(iteminside.isDebit == DbContext.AvailableAccounts.Where(z => z.AccountName == iteminside.AccountName).FirstOrDefault().isLeftNormalSide)
                    {
                        listredgreen.Add(true);
                    }
                    else
                    {
                        listredgreen.Add(false);
                    }
                }
                if (item.isApproved){
                    listsubmitted.Add(4);
                }
                else if(item.isRejected){
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
            ViewData["UserType"] = user.isManager ? 1 : (user.isAdmin ? 2 : 0);
            ViewData["Color"] = listredgreen;
            ViewData["Dates"] = listdates;
            ViewData["Submit"] = listsubmitted;
            ViewData["Totals"] = listval;
            ViewData["Transactions"] = list;
            ViewData["PostedOnly"] = (posted.ToLower() == "true");
            if (DbContext.Journals.Where(x => x.isPosted == false).Skip((10 * (pagen))).Count() > 10)
            {
                ViewData["NextPage"] = pagen + 1;
            }
            if(pagen > 0)
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
        public async Task <IActionResult> post([FromQuery]string journalid, [FromQuery]string all, string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            var journals = DbContext.Journals.Where(x => all == null ? x.Id == Convert.ToInt32(journalid) : x.isPosted == false);

            foreach(var journal in journals)
            {
                journal.isApproved = true;
                journal.isPosted = true;
                journal.isRejected = false;
                journal.isSubmited = true;
                EventLog log = new EventLog();
                log.EditedBy = user.Email;
                log.EditedOn = DateTime.Now;
                log.Description = "The journal with ID " + journal.Id + " has been posted";
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
        public async Task<IActionResult> reject([FromQuery]string journalid, string returnUrl = null)
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
            EventLog log = new EventLog();
            log.EditedBy = user.Email;
            log.EditedOn = DateTime.Now;
            log.Description = "The journal with ID " + journals.Id + " has been rejected";
            DbContext.EventLog.Add(log);
            DbContext.Entry(log).State = EntityState.Added;

            await DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(ChartOfAccountsController.ViewJournals), "ChartOfAccounts");
        }

        [HttpGet]
        public async Task<IActionResult> delete([FromQuery]string journalid, string returnUrl = null)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }
            var journals = DbContext.Journals.Where(x => x.Id == Convert.ToInt32(journalid)).FirstOrDefault();
            EventLog log = new EventLog();
            log.EditedBy = user.Email;
            log.EditedOn = DateTime.Now;
            log.Description = "The journal with ID " + journals.Id + " has been deleted";
            DbContext.Journals.Remove(journals);
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
            
            var journals = DbContext.Journals.Where(x => journalid == null ? x.isSubmited == false: x.Id == Convert.ToInt32(journalid));
            foreach (var item in journals)
            {
                item.isSubmited = true;
                item.isApproved = false;
                item.isPosted = false;
                item.isRejected = false; ;
                EventLog log = new EventLog();
                log.EditedBy = user.Email;
                log.EditedOn = DateTime.Now;
                log.Description = "The journal with ID " + item.Id + " has been submitted";
            }
            await DbContext.SaveChangesAsync();
            return RedirectToAction(nameof(ChartOfAccountsController.ViewJournals), "ChartOfAccounts");
        }

        [HttpGet]
        public IActionResult eventlog(string returnUrl = null) {
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
                log.Description = "The account with name " + model.AccountName + " has been " + (acc.isActive ? "activated": "deactivated");
                DbContext.EventLog.Add(log);
                DbContext.Entry(log).State = EntityState.Added;
                await DbContext.SaveChangesAsync();
            }    
            else if( transactions > 0)
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
            var transactions2 = DbContext.Transactions.Join(DbContext.Journals, c => c.JournalId, d => d.Id, (c, d) => new JoinedJournalTransaction { AccountName = c.AccountName, isPosted = d.isPosted, JournalId = c.JournalId, isDebit = c.isDebit, Value = c.Value, AddedOn = c.AddedOn}).Where(v => v.isPosted == true && v.AccountName == accountname);
            foreach(var item in transactions2)
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
                }else
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
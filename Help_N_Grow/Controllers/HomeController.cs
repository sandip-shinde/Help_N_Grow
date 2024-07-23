using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Help_N_Grow.Models;
using Help_N_Grow.Entity;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Pdf;
using PdfSharpCore;
using System.IO;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using Microsoft.AspNetCore.Http;

namespace Help_N_Grow.Controllers
{       //Main method
    public class HomeController : Controller
    {
        private readonly ILogin _loginUser;
        private readonly HelthPlan_Dbcontext _context;

        public HomeController(ILogin loguser, HelthPlan_Dbcontext context)
        {
            _context = context;
            _loginUser = loguser;
        }
        #region MainLogin

        public IActionResult Main()
        {
            return View();
        }
        public IActionResult Index()
        {
            Logout();
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string passcode, string role)
        {
            try
            {
                if (string.IsNullOrEmpty(passcode) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {


                    var issuccess = _loginUser.AuthenticateUser(username, passcode, role);

                    if (issuccess.Result?.Full_Name != null)
                    {
                        ViewBag.username = string.Format("Successfully logged-in", issuccess.Result?.Full_Name);
                        TempData["Package_ID"] = issuccess.Result.Package_ID;
                        TempData["Package_Name"] = issuccess.Result.Package_Name;
                        TempData["Level_Id"] = issuccess.Result.Level_Id;
                        TempData["Parent_Id"] = issuccess.Result.Reg_Id;
                        TempData["username"] = issuccess.Result.UserName;
                        //Create the identity for the user  
                        var identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, username)
                }, CookieAuthenticationDefaults.AuthenticationScheme);

                        var principal = new ClaimsPrincipal(identity);

                        var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        var level = _context.Level.Where(a => a.Level_Id == issuccess.Result.Level_Id && a.Package_ID == issuccess.Result.Package_ID).FirstOrDefault();

                        if (role == "User" || role == "Member")
                        {
                            ViewBag.registrationLevel = issuccess.Result.Level_Id;
                            ViewBag.MemberAmount = level.Amount;
                            Level_Upgrade(issuccess.Result.Reg_Id, issuccess.Result.Level_Id, issuccess.Result.Package_ID);
                            return View("Dashboard",level);
                        }
                        if (role == "Admin")
                            return RedirectToAction("AllMember");
                        if (role == "Super_Admin")
                            return RedirectToAction("TransactionApprovalIndex");
                        return View();
                    }
                    else
                    {
                        ViewBag.username = string.Format("Please check Username and Password ", username);
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.InnerException;

            }
        }
        [HttpGet]
        public IActionResult Logout()
        {
            var login = HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        // GET: Registration
        [Authorize]
        public IActionResult Dashboard()
        {

            int Package_ID = Convert.ToInt32(TempData.Peek("Package_ID"));
            string Package_Name = Convert.ToString(TempData.Peek("Package_Name"));
            int Level_Id = Convert.ToInt32(TempData.Peek("Level_Id"));
            int Parent_Id = Convert.ToInt32(TempData.Peek("Parent_Id"));
             ViewBag.registrationLevel = Level_Id;
            var level = _context.Level.Where(a => a.Level_Id == Level_Id && a.Package_ID == Package_ID).FirstOrDefault();

            ViewBag.MemberAmount = level.Amount;
            Level_Upgrade(Parent_Id, Level_Id, Package_ID);
            return View(level);
        }

        #endregion


        #region Level
        [Authorize]
        // GET: Levels
        public async Task<IActionResult> LevelIndex()
        {
            return View(await _context.Level.ToListAsync());
        }
        [Authorize]
        // GET: Levels/Details/5
        public async Task<IActionResult> LevelDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var level = await _context.Level
                .FirstOrDefaultAsync(m => m.Level_Id == id);
            if (level == null)
            {
                return NotFound();
            }

            return View(level);
        }

        // GET: Levels/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Levels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LevelCreate([Bind("Level_Id,Level_Name,Company_Percentage,Amount")] Level level)
        {
            if (ModelState.IsValid)
            {
                _context.Add(level);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(LevelIndex));
            }
            return View(level);
        }

        // GET: Levels/Edit/5
        [Authorize]
        public async Task<IActionResult> LevelEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var level = await _context.Level.FindAsync(id);
            if (level == null)
            {
                return NotFound();
            }
            return View(level);
        }

        // POST: Levels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> LevelEdit(int id, [Bind("Level_Id,Level_Name,Company_Percentage,Amount")] Level level)
        {
            if (id != level.Level_Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(level);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LevelExists(level.Level_Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(LevelIndex));
            }
            return View(level);
        }

        // GET: Levels/Delete/5
        [Authorize]
        public async Task<IActionResult> LevelDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var level = await _context.Level
                .FirstOrDefaultAsync(m => m.Level_Id == id);
            if (level == null)
            {
                return NotFound();
            }

            return View(level);
        }

        // POST: Levels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var level = await _context.Level.FindAsync(id);
            _context.Level.Remove(level);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(LevelIndex));
        }

        private bool LevelExists(int id)
        {
            return _context.Level.Any(e => e.Level_Id == id);
        }
        #endregion

        #region Registration

        // GET: Registration
        [Authorize]
        public async Task<IActionResult> RegistrationIndex()
        {
            ViewBag.UpgradedMessage = "";
            ViewBag.Error = "";
            int Package_ID = Convert.ToInt32(TempData.Peek("Package_ID"));

            int Parent_Id = Convert.ToInt32(TempData.Peek("Parent_Id"));
            ViewBag.Parent_Id = Parent_Id;
            int Level_Id = Convert.ToInt32(TempData.Peek("Level_Id"));

            var Parent = await _context.Registration.Where(e => e.Reg_Id == Parent_Id).FirstOrDefaultAsync();

            var registration = await _context.Registration.Where(e => e.Parent_Id == Parent_Id && e.Level_Id == Level_Id).ToListAsync();
            var transaction = await _context.TblTransaction.Where(e => e.Parent_Id == Parent_Id && e.Level_Id == Level_Id).ToListAsync();
            Level_Upgrade(Parent_Id, Level_Id, Package_ID);
            var registrationList = from e in registration
                                   join d in transaction on e.Reg_Id equals d.Reg_Id into table1
                                   from subgroup in table1.DefaultIfEmpty()
                                   select new { e, subgroup };
            RegistrationViewModel RegistrationViewObj = new RegistrationViewModel();
            List<RegistrationVM> RegistrationVM = new List<RegistrationVM>();
            RegistrationViewObj.showCreate = Registration_Level_Upgrade(Parent_Id, Level_Id);
            foreach (var item in registrationList)
            {
                Registration Registrationobj = new Registration();
                string unique_Id = item.e.Package_Name + "/" + item.e.Parent_Id + "/" + item.e.Reg_Id + "/" + item.e.Level_Id;
                item.e.UserName = unique_Id;
                RegistrationVM.Add(new RegistrationVM
                {
                    Registrationobj = item.e,
                    Is_Approved = item.subgroup == null ? "Not Approved" : item.subgroup.Is_Approved == false ? "Not Approved" : "Approved"
                });

            }
            if (Level_Id == 1 || Level_Id == 4 || Level_Id == 7)
            {

                RegistrationViewObj.oldMemberLst = new List<OldMember>();
            }
            else
            {
                var oldMemberLst = await _context.Registration.Where(a => a.Parent_Id == Parent_Id && a.Level_Id == (Level_Id - 1)).ToListAsync();
                List<OldMember> oldmemberList = new List<OldMember>();
                foreach (var item in oldMemberLst)
                {
                    string unique_Id = item.Package_Name + "/" + item.Parent_Id + "/" + item.Reg_Id + "/" + item.Level_Id;
                    OldMember oldmember = new OldMember();
                    oldmember.Unique_No = unique_Id;
                    oldmember.Member_Name = item.Full_Name;
                    oldmember.Member_Id = item.Reg_Id;
                    oldmemberList.Add(oldmember);
                }

                RegistrationViewObj.oldMemberLst = oldmemberList;
            }
            // ViewBag.IsTransactionComplete = IsTransactionComplete(Parent_Id, Level_Id);
            RegistrationViewObj.RegistrationoVMLst = RegistrationVM;
            RegistrationViewObj.registration = Parent;
            return View(RegistrationViewObj);
        }

        //// GET: Transaction/Edit/5
        [Authorize]
        public async Task<IActionResult> downloadmembershipSlipSelf(int? id)
        {
            try
            {
                var registration = await _context.Registration.Where(e => e.Reg_Id == id).FirstOrDefaultAsync();
                var anytransaction = await _context.TblTransaction.Where(e => e.Reg_Id == id && e.Level_Id == registration.Level_Id).AnyAsync();
                string Status = "Not Approved";
                if (anytransaction)
                {
                    var transaction = await _context.TblTransaction.Where(e => e.Reg_Id == id && e.Level_Id == registration.Level_Id).FirstOrDefaultAsync();
                    Status = transaction.Is_Approved == true ? "Approved" : "Not Approved";
                }

                // var registration =await _context.Registration.Where(e => e.Reg_Id == id).FirstOrDefaultAsync();


                var data = new PdfDocument();
                string htmlContent = "<div style = 'margin: 5px auto; heigth:500px; max-width: 300px; padding: 10px; border: 1px solid #ccc; background-color: #FFFFFF; font-family: Arial, sans-serif;' >";
                htmlContent += "<div style = 'margin-bottom: 5px; text-align: center;'>";
                htmlContent += "<img src = '.\\wwwroot\\Logo_Img\\Economic_Help_Logo.JPG' alt = 'Logo' style = 'max-width: 100px; margin-bottom: 10px;' > </div>";
                htmlContent += " <div style = 'margin-bottom: 5px; text-align: center;'>";
                htmlContent += " <h4> CERTIFICATE OF REGISTRATION </h4> </div> ";
                htmlContent += " <p style = 'margin: 0;' >Sr.No: " + registration.Package_Name + "/" + registration.Parent_Id + "/" + registration.Reg_Id + "/" + registration.Level_Id + "</p> ";
                htmlContent += " <p style = 'margin: 0;' >Date: " + System.DateTime.Now.ToShortDateString() + "</p> ";
                htmlContent += " <p style = 'margin: 0;' >---------------------------------------------------------</p>";
                htmlContent += " <p style = 'margin: 0;' > Dear " + registration.Full_Name + "</p> ";
                htmlContent += " <p style = 'margin: 0;' > Mobile: " + registration.Mobile_No + "</p>  <br>";
                htmlContent += "  <p style = 'margin: 5px;' >Salutation,</p> ";
                htmlContent += " <p style = 'margin: 10px;' > Thank you for your membership with us. your contribution is valuable and it helps our mission.</p>";
                htmlContent += " <p>your registration is as:  User Name: " + registration.UserName + " and Password: " + registration.Password + "</p> ";
                htmlContent += " <p style = 'margin: 0;' >--------------------------------------------------------</p>";
                htmlContent += " <p><i> Thank you again for your ongoing support of our mission.</i></p><p style = 'margin:5px;'><b>Sincerely.</b></p>";
                htmlContent += " <p><b>Economic Help and Growth Center</b></p>";
                htmlContent += " <p>Membership Status : <b>" + Status + "</b></p>";
                htmlContent += " <p>Note :  Membership is only considered after status approved</p>";
                htmlContent += "</div>";
                PdfGenerator.AddPdfPages(data, htmlContent, PageSize.A5);
                byte[] response = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    data.Save(ms);
                    response = ms.ToArray();
                }
                string fileName = "Member_" + registration.Parent_Id + "_" + registration.Reg_Id + "_ " + registration.Level_Id + ".pdf";
                return File(response, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //// GET: Transaction/Edit/5
        [Authorize]
        public async Task<IActionResult> downloadmembershipSlip(int? id)
        {
            try
            {
                var registration = await _context.Registration.Where(e => e.Reg_Id == id).FirstOrDefaultAsync();
                var anytransaction = await _context.TblTransaction.Where(e => e.Reg_Id == id && e.Level_Id == registration.Level_Id).AnyAsync();
                string Status = "Not Approved";
                if (anytransaction)
                {
                    var transaction = await _context.TblTransaction.Where(e => e.Reg_Id == id && e.Level_Id == registration.Level_Id).FirstOrDefaultAsync();
                    Status = transaction.Is_Approved == true ? "Approved" : "Not Approved";
                }

                // var registration =await _context.Registration.Where(e => e.Reg_Id == id).FirstOrDefaultAsync();


                var data = new PdfDocument();
                string htmlContent = "<div style = 'margin: 5px auto; heigth:300px; max-width: 200px; padding: 10px; border: 1px solid #ccc; background-color: #FFFFFF; font-family: Arial, sans-serif;' >";
                htmlContent += "<div style = 'margin-bottom: 5px; text-align: center;'>";
                htmlContent += "<img src = '.\\wwwroot\\Logo_Img\\Economic_Help_Logo.JPG' alt = 'Logo' style = 'max-width: 100px; margin-bottom: 10px;' > </div>";
                htmlContent += " <div style = 'margin-bottom: 5px; text-align: center;'>";
                htmlContent += " <h4> MEMBER REGISTRATION </h4> </div> ";
                htmlContent += " <p style = 'margin: 0;' >Sr.No: " + registration.Package_Name + "/" + registration.Parent_Id + "/" + registration.Reg_Id + "/" + registration.Level_Id + "                                      Date: " + System.DateTime.Now.ToShortDateString() + "</p> ";
                htmlContent += " <p style = 'margin: 0;' >****************************************************************************</p> ";
                htmlContent += " <p style = 'margin: 0;' >-------------------------Member Details------------------------------</p>";
                htmlContent += " <p style = 'margin: 0;' > Reference Transaction No: ---------------------------------------</p>   <br>";
                htmlContent += " <p style = 'margin: 0;' > Full_Name: ---------------------------------Mobile: ----------------------</p> ";
                htmlContent += " <p style = 'margin: 0;' > Your Bank A/c No: --------------------------- IFSC Code: ---------------</p> ";
                htmlContent += " <p style = 'margin: 0;' >Phone pay/G pay UPI ID: -------------------------------------</p> ";
                htmlContent += " <p style = 'margin: 0;' > Reason for help: ----------------------------------------------</p>  <br>";
                htmlContent += " <p style = 'margin: 0;' >--------------------------Reference Details----------------------------</p>";
                htmlContent += " <table><tr><td><div>  ";
                htmlContent += " <p style = 'margin: 0;' > Company Transaction No: --------------------------------------</p> ";
                htmlContent += " <p style = 'margin: 0;' >Bank Name: " + registration.Mobile_No + "---------------------</p>";
                htmlContent += " <p style = 'margin: 0;' >Bank A/c No: " + registration?.Bank_AC + " IFSC Code: " + registration?.IFSC_Code + "---------------------</p>";
                htmlContent += " <p style = 'margin: 0;' >Phone pay/G pay UPI ID: " + registration?.UPI_ID + "-----------------</p>  <br> ";
                htmlContent += " </div></td><td><div> ";
                htmlContent += " <p>Ref. Membership Status : <b>" + Status + "</b></p>";
                htmlContent += " <p style = 'margin: 0;' > Name: " + registration.Full_Name + "  Mobile: " + registration.Mobile_No + "---------------------</p>";
                htmlContent += " <p style = 'margin: 0;' > Your Bank A/c No: " + registration?.Bank_AC + " IFSC Code: " + registration?.IFSC_Code + "---------------------</p>";
                htmlContent += " <p style = 'margin: 0;' >Phone pay/G pay UPI ID: " + registration?.UPI_ID + "-----------------</p>  <br> ";
                htmlContent += " </div></td></tr><table>";
                htmlContent += " <p style = 'margin: 10px;' > Thank you for your membership with us. your contribution is valuable and it helps our mission.</p>";
                htmlContent += " <p style = 'margin: 0;' >--------------------------------------------------------------------</p>";
                htmlContent += " <p>Note : 1) Membership is only considered after status approved.</p>";
                htmlContent += " <p>       2) Registration amount is not refundable.</p>";
                htmlContent += "</div>";
                PdfGenerator.AddPdfPages(data, htmlContent, PageSize.A5);
                byte[] response = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    data.Save(ms);
                    response = ms.ToArray();
                }
                string fileName = "Member_" + registration.Parent_Id + "_" + registration.Reg_Id + "_ " + registration.Level_Id + ".pdf";
                return File(response, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //// GET: Transaction/Edit/5
        [Authorize]
        public async Task<IActionResult> downloadAllmembershipSlip(int? id)
        {
            try
            {
                var registration = await _context.Registration.Where(e => e.Reg_Id == id).FirstOrDefaultAsync();
                var anytransaction = await _context.TblTransaction.Where(e => e.Reg_Id == id && e.Level_Id == registration.Level_Id).AnyAsync();
                string Status = "Not Approved";
                if (anytransaction)
                {
                    var transaction = await _context.TblTransaction.Where(e => e.Reg_Id == id && e.Level_Id == registration.Level_Id).FirstOrDefaultAsync();
                    Status = transaction.Is_Approved == true ? "Approved" : "Not Approved";
                }

                var allMember = await _context.Registration.Where(e => e.Parent_Id == id && e.Level_Id == registration.Level_Id).ToListAsync();


                var data = new PdfDocument();
                string htmlContent = "<div style = 'margin: 5px auto; heigth:500px; max-width: 300px; padding: 10px; border: 1px solid #ccc; background-color: #FFFFFF; font-family: Arial, sans-serif;' >";
                htmlContent += "<div style = 'margin-bottom: 5px; text-align: center;'>";
                htmlContent += "<img src = '.\\wwwroot\\Logo_Img\\Economic_Help_Logo.JPG' alt = 'Logo' style = 'max-width: 100px; margin-bottom: 10px;' > </div>";
                htmlContent += " <div style = 'margin-bottom: 5px; text-align: center;'>";
                htmlContent += " <h4> CERTIFICATE OF GROUP MEMBER </h4> </div> ";
                htmlContent += " <p style = 'margin: 0;' >Sr.No: " + registration.Package_Name + "/" + registration.Parent_Id + "/" + registration.Reg_Id + "/" + registration.Level_Id + "</p> ";
                htmlContent += " <p style = 'margin: 0;' >Date: " + System.DateTime.Now.ToShortDateString() + "</p> ";
                htmlContent += " <p style = 'margin: 0;' >---------------------------------------------------------</p>";
                htmlContent += " <p style = 'margin: 0;' > Dear " + registration.Full_Name + "</p> ";
                htmlContent += " <p style = 'margin: 0;' > Mobile: " + registration.Mobile_No + "</p>  <br>";
                htmlContent += "  <p style = 'margin: 5px;' >Salutation,</p> ";
                htmlContent += " <p style = 'margin: 10px;' > Thank you for your membership with us. your contribution is valuable and it helps our mission.</p>";
                htmlContent += " <p>your registration is as:  User Name: " + registration.UserName + " and Password: " + registration.Password + "</p> ";
                htmlContent += " <p style = 'margin: 0;' >--------------------------------------------------------</p>";

                htmlContent += "<table><th><td>Full Name</td><td>Register ID</td><td></td></th>";
                foreach (var member in allMember)
                {
                    htmlContent += "<tr><td>" + member.Full_Name + "</td><td>" + member.Package_Name + "/" + member.Parent_Id + "/" + member.Reg_Id + "/" + member.Level_Id + "</td><td></td></tr>";
                }
                htmlContent += "</table>";
                htmlContent += " <p style = 'margin: 0;' >--------------------------------------------------------</p>";

                htmlContent += " <p><i> Thank you again for your ongoing support of our mission.</i></p><p style = 'margin:5px;'><b>Sincerely.</b></p>";
                htmlContent += " <p><b>Economic Help and Growth Center</b></p>";
                htmlContent += " <p>Membership Status : <b>" + Status + "</b></p>";
                htmlContent += " <p>Note :  Membership is only considered after status approved</p>";
                htmlContent += "</div>";
                PdfGenerator.AddPdfPages(data, htmlContent, PageSize.A4);
                byte[] response = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    data.Save(ms);
                    response = ms.ToArray();
                }
                string fileName = "Member_" + registration.Parent_Id + "_" + registration.Reg_Id + "_ " + registration.Level_Id + ".pdf";
                return File(response, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        // GET: Registration
        [Authorize]
        public async Task<IActionResult> Search(string UserName)
        {
            if (UserName == null)
            {
                return View("Index");
            }

            int Parent_Id = Convert.ToInt32(TempData.Peek("Parent_Id"));
            int Level_Id = Convert.ToInt32(TempData.Peek("Level_Id"));

            var registrationList = await _context.Registration.Where(e => e.Level_Id == Level_Id && e.UserName == UserName).ToListAsync();
            return View("Search", registrationList);
        }

        
        // GET: Term_Condition
        [Authorize]
        public IActionResult Term_Condition()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        public ActionResult Term_Conditions([Bind("Term_Condition_Accepted")]  Registration registration)
        {
            if (registration.Term_Condition_Accepted)
                return RedirectToAction("RegistrationIndex", "Registration");
            return RedirectToAction("Term_Condition", "Registration");
        }

        // GET: Registration/Details/5
        [Authorize]
        public async Task<IActionResult> RegistrationDetails(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            var registration = await _context.Registration
                .FirstOrDefaultAsync(m => m.Reg_Id == id);
            if (registration == null)
            {
                return NotFound();
            }

            TempData["NewMember_ID"] = id;
            int Parent_Id = Convert.ToInt32(TempData.Peek("Parent_Id"));
            Registration_Level_Upgrade(registration.Parent_Id, registration.Level_Id);

            return View(registration);
        }

        // GET: Registration/Create
        [Authorize]
        public IActionResult RegistrationCreate()
        {
            int Level_Id = Convert.ToInt32(TempData.Peek("Level_Id"));
            int Parent_Id = Convert.ToInt32(TempData.Peek("Parent_Id"));
            int Package_ID = Convert.ToInt32(TempData.Peek("Package_ID"));
            string Package_Name = Convert.ToString(TempData.Peek("Package_Name"));
            var level = _context.Level.Where(a => a.Level_Id == Level_Id).FirstOrDefault();
            var registration = _context.Registration.Where(a => a.Reg_Id == Parent_Id).FirstOrDefault();
            ViewBag.MemberAmount = level.Self_Amount;
            ViewBag.CompanyAmount = level.Company_Percentage;
            ViewBag.Full_Name = registration.Full_Name;
            ViewBag.Bank_AC = registration.Bank_AC;
            ViewBag.IFSC_Code = registration.IFSC_Code;
            ViewBag.UPI_ID = registration.UPI_ID;
            return View();
        }


        // GET: Registration/Create
        [Authorize]
        public IActionResult RegistrationOld(int MemberID)
        {
            ViewBag.Error = "";
            int Level_Id = Convert.ToInt32(TempData.Peek("Level_Id"));
            int Parent_Id = Convert.ToInt32(TempData.Peek("Parent_Id"));

            if (!_context.Registration.Where(e => e.Reg_Id == MemberID && e.Parent_Id == Parent_Id).Any())
            {
                ViewBag.Error = "Please Enter Valid Number";
                return RedirectToAction(nameof(RegistrationIndex));
            }
            else
            {
                var member = _context.Registration.Where(e => e.Reg_Id == MemberID && e.Parent_Id == Parent_Id).FirstOrDefault();
                member.Level_Id = member.Level_Id + 1;
                _context.Update(member);
                _context.SaveChangesAsync();
                return RedirectToAction(nameof(RegistrationIndex));
            }
        }


        // POST: Registration/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> RegistrationCreate([Bind("Reg_Id,UserName,Full_Name,Mobile_No,Bank_AC,IFSC_Code,UPI_ID,Password,Term_Condition_Accepted,Reg_Date,Level_Id,Parent_Id,Is_Active")] Registration registration)
        {
            ViewBag.Usernameexist = ""; ;
            var isExistRegistration = await _context.Registration.Where(a => a.UserName == registration.UserName).AnyAsync();
            if (isExistRegistration)
            {
                ViewBag.Usernameexist = "User Name is already exist.";
                return View(registration);
            }
            if (registration.Term_Condition_Accepted == true)
                if (ModelState.IsValid && registration.Term_Condition_Accepted == true)
                {
                    registration.Parent_Id = Convert.ToInt32(TempData.Peek("Parent_Id"));

                    var ParentUser = _context.Registration.Where(a => a.Reg_Id == registration.Parent_Id).FirstOrDefault();

                    registration.Package_ID = ParentUser.Package_ID;
                    registration.Package_Name = ParentUser.Package_Name;
                    registration.Level_Id = ParentUser.Level_Id;
                    registration.Is_Active = false;
                    registration.Reg_Date = System.DateTime.Now;
                    registration.Role = "Member";
                    _context.Add(registration);
                    int NewMember_ID = await _context.SaveChangesAsync();
                    TempData["NewMember_ID"] = NewMember_ID;
                    return RedirectToAction(nameof(RegistrationIndex));
                }
            ViewBag.Usernameexist = "User Not Accepted Term's & Condition.";
            return View(registration);
        }



        // POST: Registration/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddExisting([Bind("Reg_Id,UserName,Full_Name,Mobile_No,Bank_AC,IFSC_Code,UPI_ID,Password,Term_Condition_Accepted,Reg_Date,Level_Id,Parent_Id,Is_Active")] Registration registration)
        {
            if (registration.Term_Condition_Accepted == true)
                if (ModelState.IsValid && registration.Term_Condition_Accepted == true)
                {

                    registration.Level_Id = Convert.ToInt32(TempData.Peek("Level_Id"));
                    registration.Parent_Id = Convert.ToInt32(TempData.Peek("Parent_Id"));
                    registration.Is_Active = true;


                    try
                    {
                        _context.Update(registration);
                        var NewMember_ID = await _context.SaveChangesAsync();

                        TempData["NewMember_ID"] = NewMember_ID;

                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!RegistrationExists(registration.Reg_Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(RegistrationIndex));

                }

            return View("Index", registration);
        }

        // GET: Registration/Create
        [Authorize]
        public async Task<IActionResult> Select(int? NewMemberid)
        {
            if (NewMemberid == null)
            {
                return NotFound();
            }

            var registration = await _context.Registration.FindAsync(NewMemberid);
            if (registration == null)
            {
                return NotFound();
            }
            return View(registration);
        }

        //// GET: Registration/Edit/5
        //public async Task<IActionResult> RegistrationEdit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }
        //    TempData["NewMember_ID"] = id;
        //    var registration = await _context.Registration.FindAsync(id);
        //    if (registration == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(registration);
        //}

        //// POST: Registration/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> RegistrationEdit(int id, [Bind("Reg_Id,UserName,Full_Name,Mobile_No,Bank_AC,IFSC_Code,UPI_ID,Password,Term_Condition_Accepted,Reg_Date,Level_Id,Parent_Id,Is_Active")] Registration registration)
        //{
        //    if (id != registration.Reg_Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(registration);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!RegistrationExists(registration.Reg_Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(RegistrationIndex));
        //    }
        //    return View(registration);
        //}

        //// GET: Registration/Delete/5
        //public async Task<IActionResult> RegistrationDelete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var registration = await _context.Registration
        //        .FirstOrDefaultAsync(m => m.Reg_Id == id);
        //    if (registration == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(registration);
        //}

        //// POST: Registration/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var registration = await _context.Registration.FindAsync(id);
        //    _context.Registration.Remove(registration);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(RegistrationIndex));
        //}

        private bool RegistrationExists(int id)
        {
            return _context.Registration.Any(e => e.Reg_Id == id);
        }

        private string Registration_Level_Upgrade(int parent_id, int Level_Id)
        {
            var memberCount = _context.Level.Where(e => e.Level_Id == Level_Id).Select(a => a.MemberCount).FirstOrDefault();

            ViewBag.showCreate = "";
            bool registrationComplete = _context.Registration.Where(e => e.Parent_Id == parent_id && e.Level_Id == Level_Id).Count() < memberCount;

            if (registrationComplete == true)
            {
                return "true";
            }
            else
            {
                bool transactionApprove = _context.TblTransaction.Where(e => e.Parent_Id == parent_id && e.Is_Approved == true && e.Level_Id == Level_Id).Count() < memberCount;
                if (transactionApprove == true)
                {
                    return "Pending";
                }
                else
                {
                    return "false";
                }

            };
        }
        private bool IsTransactionComplete(int reg_id, int Level_Id)
        {
            bool transactionComplete = _context.TblTransaction.Where(e => e.Reg_Id == reg_id && e.Is_Approved == true && e.Level_Id == Level_Id).Count() == 1;

            return transactionComplete;
        }
        private void Level_Upgrade(int Parent_Id, int levelID, int packageId)
        {
            var cnt = _context.Registration.Where(e => e.Parent_Id == Parent_Id && e.Is_Active == true && e.Package_ID == packageId && e.Level_Id == levelID).Count() == 0;
            int level = levelID <= 3 ? levelID : levelID >= 4 && levelID <= 6 ? levelID - 3 : levelID - 6;
            if (cnt)
                ViewBag.UpgradedMessage = "Congratulations !!! you have Pramoted to Next Level " + level + ":)";
        }

        #endregion

        #region Transaction

        // GET: Transaction
        [Authorize]
        public async Task<IActionResult> TransactionIndex()
        {
            int Reg_Id = Convert.ToInt32(TempData.Peek("NewMember_ID"));
            int Level_Id = Convert.ToInt32(TempData.Peek("Level_Id"));
            int Parent_Id = Convert.ToInt32(TempData.Peek("Parent_Id"));
            ViewBag.Is_TransactionComplete = Is_TransactionComplete(Reg_Id, Level_Id);
            TransactionLevel_Upgrade(Parent_Id, Level_Id);
            return View(await _context.TblTransaction.Where(e => e.Reg_Id == Reg_Id && e.Parent_Id == Parent_Id && e.Level_Id == Level_Id).ToListAsync());
        }
        // GET: Transaction
        [Authorize]
        public async Task<IActionResult> TransactionNewIndex(int id)
        {
            int Reg_Id = id;
            int Level_Id = Convert.ToInt32(TempData.Peek("Level_Id"));
            int Parent_Id = Convert.ToInt32(TempData.Peek("Parent_Id"));
            ViewBag.Is_TransactionComplete = Is_TransactionComplete(Reg_Id, Level_Id);
            TransactionLevel_Upgrade(Parent_Id, Level_Id);
            return View("TransactionIndex", await _context.TblTransaction.Where(e => e.Reg_Id == Reg_Id && e.Parent_Id == Parent_Id && e.Level_Id == Level_Id).ToListAsync());
        }
        // GET: Transaction/Details/5
        [Authorize]
        public async Task<IActionResult> TransactionDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.TblTransaction
                .FirstOrDefaultAsync(m => m.Transaction_ID == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View("TransactionDetails", transaction);
        }

        // GET: Transaction/Create
        [Authorize]
        public IActionResult TransactionNCreate(int id)
        {
            // var registration =  _context.Registration.FirstOrDefault(m => m.Reg_Id == id);
            TempData["NewMember_ID"] = id;
            var Level_Id = Convert.ToInt32(TempData.Peek("Level_Id"));
            var LevelObj = _context.Level
               .FirstOrDefault(m => m.Level_Id == Level_Id);
            ViewBag.SelfAmount = LevelObj.Self_Amount;
            ViewBag.Company_Amount = LevelObj.Company_Percentage;
            bool transactionexist = TransactionExistsforThisMember(Level_Id, id);
            if (transactionexist)
                return RedirectToAction("TransactionNewIndex", new { id = id });
            return View("TransactionCreate");
        }
        // GET: Transaction/Create
        [Authorize]
        public IActionResult NewCreateForReplace(int id)
        {
            // var registration =  _context.Registration.FirstOrDefault(m => m.Reg_Id == id);
            TempData["NewMember_ID"] = id;
            var Level_Id = Convert.ToInt32(TempData.Peek("Level_Id"));
            var LevelObj = _context.Level
               .FirstOrDefault(m => m.Level_Id == Level_Id);
            ViewBag.SelfAmount = LevelObj.Self_Amount;
            ViewBag.Company_Amount = LevelObj.Company_Percentage;
            bool transactionexist = ApprovedTransactionExistsforThisMember(Level_Id, id);
            if (transactionexist)
                return RedirectToAction("NewIndex", new { id = id });
            return View("Create");
        }
        // GET: Transaction/Create
        [Authorize]
        public IActionResult TransactionCreate()
        {
            // var registration =  _context.Registration.FirstOrDefault(m => m.Reg_Id == id);
            var Level_Id = Convert.ToInt32(TempData.Peek("Level_Id"));
            var LevelObj = _context.Level
               .FirstOrDefault(m => m.Level_Id == Level_Id);
            ViewBag.SelfAmount = LevelObj.Self_Amount;
            ViewBag.Company_Amount = LevelObj.Company_Percentage;
            // bool transactionexist=  TransactionExistsforThisMember(registration.Level_Id, registration.Reg_Id);
            return View();
        }
        // POST: Transaction/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransactionCreate(Transaction_Upload transaction_Upload, IFormFile image)
        {
            if ((transaction_Upload == null || transaction_Upload.FileToUpload1 == null || transaction_Upload.FileToUpload1.Length == 0) && (transaction_Upload.FileToUpload2 == null || transaction_Upload.FileToUpload2.Length == 0))
            {
                ViewBag.Error = "Select company and Self Transaction Reciept file";
                return View("TransactionCreate", transaction_Upload);
            }
            if (transaction_Upload == null ||
        transaction_Upload.FileToUpload1 == null || transaction_Upload.FileToUpload1.Length == 0)
            {
                ViewBag.Error = "Select company Transaction Reciept file";
                return View("TransactionCreate", transaction_Upload);
            }
            if (transaction_Upload == null ||
        transaction_Upload.FileToUpload2 == null || transaction_Upload.FileToUpload2.Length == 0)
            {
                ViewBag.Error = "Select Self Transaction Reciept file";
                return View("TransactionCreate", transaction_Upload);
            }

            if ((transaction_Upload == null ||
                       transaction_Upload.FileToUpload1 == null || transaction_Upload.FileToUpload1.Length == 0)
                              || (transaction_Upload == null ||
                       transaction_Upload.FileToUpload2 == null || transaction_Upload.FileToUpload2.Length == 0))

                return View(transaction_Upload);
            if (ModelState.IsValid)
            {
                var isExistTransaction_No1 = await _context.TblTransaction.Where(a => a.Transaction_No.Trim() == transaction_Upload.Transaction_NoCompany.Trim() || a.Transaction_NoSelf.Trim() == transaction_Upload.Transaction_NoSelf.Trim()).AnyAsync();
                var isExistTransaction_No2 = await _context.TblTransaction.Where(a => a.Transaction_No.Trim() == transaction_Upload.Transaction_NoSelf.Trim() || a.Transaction_NoSelf.Trim() == transaction_Upload.Transaction_NoCompany.Trim()).AnyAsync();

                if (isExistTransaction_No1 || isExistTransaction_No2)
                {
                    ViewBag.Error = "Transaction No is already exist.";
                    return View(transaction_Upload);
                }
                //  if ((transaction_Upload == null ||
                //transaction_Upload.FileToUpload1 == null || transaction_Upload.FileToUpload1.Length == 0) && (transaction_Upload == null ||
                //transaction_Upload.FileToUpload2 == null || transaction_Upload.FileToUpload2.Length == 0))


                //var path = Path.Combine(
                //          Directory.GetCurrentDirectory(), "wwwroot/Transacton_Photo",
                //          transaction_Upload.Transaction_NoCompany + "_" + TempData.Peek("Parent_Id").ToString() + "_" + TempData.Peek("Level_Id").ToString() + "_" + transaction_Upload.FileToUpload1.FileName);
                //var path2 = Path.Combine(
                //                         Directory.GetCurrentDirectory(), "wwwroot/Transacton_Photo",
                //                         transaction_Upload.Transaction_NoSelf + "_" + TempData.Peek("Parent_Id").ToString() + "_" + TempData.Peek("Level_Id").ToString() + "_" + transaction_Upload.FileToUpload2.FileName);

                //using (var stream = new FileStream(path, FileMode.Create))
                //{
                //    await transaction_Upload.FileToUpload1.CopyToAsync(stream);
                //}
                //using (var stream = new FileStream(path2, FileMode.Create))
                //{
                //    await transaction_Upload.FileToUpload2.CopyToAsync(stream);
                //}

                TblTransaction transaction = new TblTransaction();
                transaction.Level_Id = Convert.ToInt32(TempData.Peek("Level_Id"));
                transaction.Reg_Id = Convert.ToInt32(TempData.Peek("NewMember_ID"));
                transaction.Parent_Id = Convert.ToInt32(TempData.Peek("Parent_Id"));

                var LeveL = await _context.Level
               .FirstOrDefaultAsync(m => m.Level_Id == transaction.Level_Id);
                transaction.Transaction_ID = 0;
                transaction.Upload_Path = "Company_" + transaction_Upload.Transaction_NoCompany + "_" + transaction.Parent_Id + "_" + TempData.Peek("NewMember_ID").ToString() + "_" + TempData.Peek("Level_Id").ToString() + "_" + transaction_Upload.FileToUpload1.GetFilename();
                transaction.Upload_PathSelf = "Member_" + transaction_Upload.Transaction_NoSelf + "_" + transaction.Parent_Id + "_" + TempData.Peek("NewMember_ID").ToString() + "_" + TempData.Peek("Level_Id").ToString() + "_" + transaction_Upload.FileToUpload2.GetFilename();

                using (var ms = new MemoryStream())
                {
                    transaction_Upload.FileToUpload2.CopyTo(ms);
                    transaction.MemberTransactionPhoto = ms.ToArray();
                    transaction.CompanyTransactionPhotoContentType = transaction_Upload.FileToUpload2.ContentType;
                }

                using (var ms = new MemoryStream())
                {
                    transaction_Upload.FileToUpload1.CopyTo(ms);
                    transaction.CompanyTransactionPhoto = ms.ToArray();
                    transaction.MemberTransactionPhotoContentType = transaction_Upload.FileToUpload1.ContentType;
                }

                var registration = await _context.Registration.Where(e => e.Reg_Id == transaction.Reg_Id).FirstOrDefaultAsync();
                transaction.Package_ID = registration.Package_ID;
                transaction.Package_Name = registration.Package_Name;
                transaction.Is_Approved = false;
                transaction.Transaction_No = transaction_Upload.Transaction_NoCompany;
                transaction.Transaction_NoSelf = transaction_Upload.Transaction_NoSelf;
                transaction.T_Date = System.DateTime.Now;
                transaction.Self_Amount = LeveL.Self_Amount;
                transaction.Company_Percentage = LeveL.Company_Percentage;
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(TransactionIndex));
            }
            ViewBag.Error = "Please Enter Both Transaction No";
            return View(transaction_Upload);
        }

        public string getextention(string path)
        {
            string myFilePath = @"C:\MyFile.txt";
            return Path.GetExtension(myFilePath);

        }

        [Authorize]
        public async Task<IActionResult> ViewCompanyPhoto(int Transactionid)
        {
            var fileToRetrieve = await _context.TblTransaction.Where(a => a.Transaction_ID == Transactionid).FirstOrDefaultAsync();
            return File(fileToRetrieve.CompanyTransactionPhoto, fileToRetrieve.CompanyTransactionPhotoContentType);
        }
        [Authorize]
        public async Task<IActionResult> ViewMemberPhoto(int Transactionid)
        {
            var fileToRetrieve = await _context.TblTransaction.Where(a => a.Transaction_ID == Transactionid).FirstOrDefaultAsync();
            return File(fileToRetrieve.MemberTransactionPhoto, fileToRetrieve.MemberTransactionPhotoContentType);
        }
        [Authorize]
        public async Task<IActionResult> DownloadCompanyPhoto(int Transactionid, string FileName)
        {
            var fileToRetrieve = await _context.TblTransaction.Where(a => a.Transaction_ID == Transactionid).FirstOrDefaultAsync();
            return File(fileToRetrieve.CompanyTransactionPhoto, fileToRetrieve.CompanyTransactionPhotoContentType, FileName);
        }
        [Authorize]
        public async Task<IActionResult> DownloadMemberPhoto(int Transactionid, string FileName)
        {
            var fileToRetrieve = await _context.TblTransaction.Where(a => a.Transaction_ID == Transactionid).FirstOrDefaultAsync();
            return File(fileToRetrieve.MemberTransactionPhoto, fileToRetrieve.MemberTransactionPhotoContentType, FileName);
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"}
            };
        }

        //// GET: Transaction/Edit/5
        [Authorize]
        public async Task<IActionResult> TransactionEdit(int? id)
        {
            var transaction = await _context.TblTransaction.Where(e => e.Transaction_ID == (id) && e.Is_Approved == true).FirstOrDefaultAsync();
            var registration = await _context.Registration.Where(e => e.Reg_Id == transaction.Reg_Id && e.Level_Id == transaction.Level_Id).FirstOrDefaultAsync();

            var data = new PdfDocument();
            string htmlContent = "<div style = 'margin: 20px auto; heigth:1000px; max-width: 600px; padding: 20px; border: 1px solid #ccc; background-color: #FFFFFF; font-family: Arial, sans-serif;' >";
            htmlContent += "<div style = 'margin-bottom: 20px; text-align: center;'>";
            htmlContent += "<img src = '.\\wwwroot\\Logo_Img\\Economic_Help_Logo.JPG' alt = 'School Logo' style = 'max-width: 100px; margin-bottom: 10px;' >";
            htmlContent += "</div>";
            htmlContent += "<div style = 'margin-bottom: 20px; text-align: center;'>";
            htmlContent += "<h1> RECEIPT </h1>";
            htmlContent += "</div>";
            htmlContent += "<p style = 'margin: 0;' >Receipt:</p>" + registration.Package_Name + "/" + transaction.Transaction_ID + "/" + transaction.Parent_Id + "/" + transaction.Reg_Id + "/" + transaction.Level_Id;
            htmlContent += "<p style = 'margin: 0;' >---------------------------</p>";
            htmlContent += "<p style = 'margin: 0;' >Date:</p>" + System.DateTime.Now.ToShortDateString();
            htmlContent += "<p style = 'margin: 0;' >User Name:</p>" + registration.UserName;
            htmlContent += "<p style = 'margin: 0;' >Password:</p>" + registration.Password;
            htmlContent += "<div style = 'text-align: center; margin-bottom: 20px;'>";
            htmlContent += "</div>";
            htmlContent += "<p style = 'margin: 0;' >----------------------------------------------------------------------------------------------------------------</p>";
            htmlContent += "<p> Donation Recieved from <i> ";
            htmlContent += " " + registration.Full_Name;
            htmlContent += ",</i> Level-<i>" + registration.Level_Id;
            htmlContent += "</i></p><p> of Rupees " + (transaction.Self_Amount + transaction.Company_Percentage) + "/- <b>for Donaton against the settlement of amount agreed by you.</b>" + "</p>";
            htmlContent += "<table style = 'width: 100%; border-collapse: collapse;'>";
            htmlContent += "<thead>";
            htmlContent += "<tr>";
            htmlContent += "<th style = 'padding: 8px; text-align: left; border-bottom: 1px solid #ddd;' > Fee Description </th>";
            htmlContent += "<th style = 'padding: 8px; text-align: left; border-bottom: 1px solid #ddd;' > Amount(INR) </th>";
            htmlContent += "</tr><hr/>";
            htmlContent += "</thead>";
            htmlContent += "<tbody>";
            htmlContent += "<tr>";
            htmlContent += "<td style = 'padding: 8px; text-align: left; border-bottom: 1px solid #ddd;' > Donation Amount </td>";
            htmlContent += "<td style = 'padding: 8px; text-align: left; border-bottom: 1px solid #ddd;' > " + transaction.Self_Amount + " </td>";
            htmlContent += "</tr><hr/>";
            htmlContent += "<tr>";
            htmlContent += "<td style = 'padding: 8px; text-align: left; border-bottom: 1px solid #ddd;' > Service Charges </td>";
            htmlContent += "<td style = 'padding: 8px; text-align: left; border-bottom: 1px solid #ddd;' > " + transaction.Company_Percentage + " </td>";
            htmlContent += "</tr><hr/>";
            htmlContent += "</tbody>";
            htmlContent += "<tbody>";
            //if (Parent_Id != null && levelID > 0)
            //{
            //    req.fees.ForEach(x =>
            //    {
            //        htmlContent += "<tr>";
            //        htmlContent += "<td style = 'padding: 8px; text-align: left; border-bottom: 1px solid #ddd;' >" + x.FeesDescription + " </td>";
            //        htmlContent += "<td style = 'padding: 8px; text-align: left; border-bottom: 1px solid #ddd;' >Rs " + x.Amount + "/- </td>";
            //        htmlContent += "</tr>";
            //        if (decimal.TryParse(x.Amount, out decimal feeAmount))
            //        {
            //            totalAmount += feeAmount;
            //        }
            //    });
            htmlContent += "</tbody>";
            htmlContent += "<tfoot>";
            htmlContent += "<tr>";
            htmlContent += "<td style = 'padding: 8px; text-align: right; font-weight: bold;'> Total:</td>";
            htmlContent += "<td style = 'padding: 8px; text-align: left; border-top: 1px solid #ddd;' >Rs" + (transaction.Company_Percentage + transaction.Self_Amount) + "/- </td>";
            htmlContent += "</tr>";
            htmlContent += "</tfoot>";
            //}
            htmlContent += "</table>";
            htmlContent += "</div>";
            PdfGenerator.AddPdfPages(data, htmlContent, PageSize.A4);
            byte[] response = null;
            using (MemoryStream ms = new MemoryStream())
            {
                data.Save(ms);
                response = ms.ToArray();
            }
            string fileName = "Transaction_" + registration.UserName + ".pdf";
            return File(response, "application/pdf", fileName);

        }

        //// POST: Transaction/Edit/5
        //[Authorize]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> TransactionEdit(int id, [Bind("Transaction_ID,Upload_Path,Transaction_NoCompany,Is_Approved,Reg_Id,Level_Id")] TblTransaction transaction)
        //{
        //    if (id != transaction.Transaction_ID)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(transaction);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!TransactionExists(transaction.Transaction_ID))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(transaction);
        //}
        [Authorize]
        public ActionResult DonationReciept(string transaction_ID)
        {
            return View();
        }

        //// GET: Transaction/Delete/5
        ///[Authorize]
        //public async Task<IActionResult> TransactionDelete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var transaction = await _context.TblTransaction
        //        .FirstOrDefaultAsync(m => m.Transaction_ID == id);
        //    if (transaction == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(transaction);
        //}

        //// POST: Transaction/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var transaction = await _context.TblTransaction.FindAsync(id);
        //    _context.TblTransaction.Remove(transaction);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool TransactionExists(int id)
        {
            return _context.TblTransaction.Any(e => e.Transaction_ID == id);
        }

        private bool TransactionExistsforThisMember(int Level_Id, int id)
        {
            return _context.TblTransaction.Any(e => e.Reg_Id == id && e.Level_Id == Level_Id);
        }
        private bool ApprovedTransactionExistsforThisMember(int Level_Id, int id)
        {
            return _context.TblTransaction.Any(e => e.Reg_Id == id && e.Level_Id == Level_Id && e.Is_Approved == true);
        }



        private bool Is_TransactionComplete(int Id, int levelID)
        {
            var Is_TransactionComplete = _context.TblTransaction.Where(e => e.Reg_Id == Id && e.Is_Approved == true && e.Level_Id == levelID).Count() > 0;

            return Is_TransactionComplete;
        }
        private bool TransactionRegistration_Level_Upgrade(int Parent_Id, int levelID)
        {
            bool registrationComplete = _context.Registration.Where(e => e.Parent_Id == Parent_Id && e.Is_Active == true && e.Level_Id == levelID).Count() == 2;
            int cnt = _context.TblTransaction.Where(e => e.Parent_Id == Parent_Id && e.Is_Approved == true && e.Level_Id == levelID).Count();

            bool transactionComplete = cnt <= 2 ? true : false;
            //if(cnt==2)
            // {
            //     try
            //     {
            //         var registration = _context.Registration.Where(e => e.Reg_Id == Parent_Id && e.Is_Active == true && e.Level_Id == levelID).FirstOrDefault();
            //         registration.Level_Id = levelID + 1;
            //         TempData["Level_Id"] = levelID + 1;
            //         _context.Update(registration);
            //         _context.SaveChanges();
            //         ViewBag.UpgradedMessage = "Congratulations !!! you have completed the level and Pramoted to Next Level :)";

            //     }
            //     catch (Exception ex)
            //     {

            //             throw;

            //     }
            // }
            return (registrationComplete == true && transactionComplete == true);
        }
        private void TransactionLevel_Upgrade(int Parent_Id, int levelID)
        {
            var cnt = _context.Registration.Where(e => e.Parent_Id == Parent_Id && e.Is_Active == true && e.Level_Id == levelID).Count() < 1;
            if (cnt)
                ViewBag.UpgradedMessage = "Congratulations !!! you have completed the level and Pramoted to Next Level :)";
        }
        #endregion
        #region TransactionApproval
        // GET: TransactionApproval
        [Authorize]
        public async Task<IActionResult> TransactionApprovalIndex()
        {
            // var transactionList=  await _context.TblTransaction.ToListAsync();
            var TransactionApproval = await (from rg in _context.Registration
                                             join tr in _context.TblTransaction on rg.Reg_Id equals tr.Reg_Id
                                             //where rg.Is_Active == true
                                             select new TblTransactionApproval
                                             {
                                                 Unique_No = rg.Package_Name + "/" + tr.Parent_Id + "/" + tr.Reg_Id + "/" + tr.Level_Id,
                                                 Reg_Id = rg.Reg_Id,
                                                 Full_Name = rg.Full_Name,
                                                 Mobile_No = rg.Mobile_No,
                                                 Is_Approved = tr.Is_Approved,
                                                 Parent_Id = tr.Parent_Id,
                                                 Level_Id = tr.Level_Id,
                                                 Transaction_NoSelf = tr.Transaction_NoSelf,//self
                                                 Transaction_No = tr.Transaction_No,//Company
                                                 Self_Amount = tr.Self_Amount,
                                                 Company_Percentage = tr.Company_Percentage,
                                                 Transaction_ID = tr.Transaction_ID,
                                                 Upload_Path = tr.Upload_Path,
                                                 Transaction_Date = tr.T_Date
                                             }).ToListAsync();


            return View(TransactionApproval);
        }


        // GET: TransactionApproval
        [Authorize]
        public async Task<IActionResult> AllMember()
        {
            // var transactionList=  await _context.TblTransaction.ToListAsync();
            var TransactionApproval = await (from rg in _context.Registration
                                             join tr in _context.TblTransaction on rg.Reg_Id equals tr.Reg_Id
                                             where rg.Is_Active == true
                                             select new TblTransactionApproval
                                             {
                                                 Unique_No = rg.Package_Name + "/" + rg.Parent_Id + "/" + rg.Reg_Id + "/" + rg.Level_Id,
                                                 Reg_Id = rg.Reg_Id,
                                                 Full_Name = rg.Full_Name,
                                                 Username = rg.UserName,
                                                 Password = rg.Password,
                                                 Mobile_No = rg.Mobile_No,
                                                 Parent_Id = rg.Parent_Id,
                                                 Level_Id = rg.Level_Id,
                                                 Transaction_ID = tr.Transaction_ID,
                                                 Transaction_Date = tr.T_Date
                                             }).ToListAsync();


            return View(TransactionApproval);
        }

        // GET: TransactionApproval
        [Authorize]
        public async Task<IActionResult> TransactionApprovalFilterBy(string IsApproved)
        {

            var TransactionApproval = await (from rg in _context.Registration
                                             join tr in _context.TblTransaction on rg.Reg_Id equals tr.Reg_Id
                                             //where rg.Is_Active == true
                                             select new TblTransactionApproval
                                             {
                                                 Unique_No = rg.Package_Name + "/" + tr.Parent_Id + "/" + tr.Reg_Id + "/" + tr.Level_Id,
                                                 Reg_Id = rg.Reg_Id,
                                                 Full_Name = rg.Full_Name,
                                                 Mobile_No = rg.Mobile_No,
                                                 Is_Approved = tr.Is_Approved,
                                                 Parent_Id = tr.Parent_Id,
                                                 Level_Id = tr.Level_Id,
                                                 Transaction_NoSelf = tr.Transaction_NoSelf,//self
                                                 Transaction_No = tr.Transaction_No,//Company
                                                 Self_Amount = tr.Self_Amount,
                                                 Company_Percentage = tr.Company_Percentage,
                                                 Transaction_ID = tr.Transaction_ID,
                                                 Upload_Path = tr.Upload_Path,
                                                 Transaction_Date = tr.T_Date
                                             }).ToListAsync();
            if (IsApproved != "All")
            {
                var isApproved = IsApproved == "Approved" ? true : false;


                return View("TransactionApprovalIndex", TransactionApproval.Where(a => a.Is_Approved == isApproved).ToList());
            }
            return View("TransactionApprovalIndex", TransactionApproval.ToList());
        }
        // GET: TransactionApproval
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> TransactionApprovalAllMemberFilterBy([Bind("FilterBy,fillValue")] vmSearch vmsearch)
        {
            var TransactionApproval = await (from rg in _context.Registration
                                             join tr in _context.TblTransaction on rg.Reg_Id equals tr.Reg_Id
                                             where rg.Is_Active == true
                                             select new TblTransactionApproval
                                             {
                                                 Unique_No = rg.Package_Name + "/" + rg.Parent_Id + "/" + rg.Reg_Id + "/" + rg.Level_Id,
                                                 Reg_Id = rg.Reg_Id,
                                                 Full_Name = rg.Full_Name,
                                                 Username = rg.UserName,
                                                 Password = rg.Password,
                                                 Mobile_No = rg.Mobile_No,
                                                 Parent_Id = rg.Parent_Id,
                                                 Level_Id = rg.Level_Id
                                             }).ToListAsync();

            if (!string.IsNullOrWhiteSpace(vmsearch.fillValue))
            {

                switch (vmsearch.FilterBy)
                {
                    case "1":

                        TransactionApproval = TransactionApproval.Where(a => a.Reg_Id == int.Parse(vmsearch.fillValue)).ToList();
                        break;
                    case "2":
                        TransactionApproval = TransactionApproval.Where(a => a.Parent_Id == int.Parse(vmsearch.fillValue)).ToList();
                        break;
                    case "3":
                        TransactionApproval = TransactionApproval.Where(a => a.Full_Name == vmsearch.fillValue).ToList();
                        break;
                    case "4":
                        TransactionApproval = TransactionApproval.Where(a => a.Mobile_No == int.Parse(vmsearch.fillValue)).ToList();
                        break;
                    case "5":
                        TransactionApproval = TransactionApproval.Where(a => a.Level_Id == int.Parse(vmsearch.fillValue)).ToList();
                        break;

                }


            }
            return View("AllMember", TransactionApproval);
        }
        [Authorize]
        public FileResult TransactionApprovalDownload(string ImageName)
        {
            var FileVirtualPath = ImageName;
            return File(FileVirtualPath, "application/force-download", Path.GetFileName(FileVirtualPath));
        }

        // GET: TransactionApproval/Edit/5
        [Authorize]
        public async Task<IActionResult> TransactionApprovalEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.TblTransaction.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return View(transaction);
        }

        // POST: TransactionApproval/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransactionApprovalEdit(int id, [Bind("Transaction_ID,Upload_Path,Transaction_No,Is_Approved,Reg_Id,Level_Id")] TblTransaction transaction)
        {
            bool transactionApproved = transaction.Is_Approved;
            string Note = transaction.Note;
            if (id != transaction.Transaction_ID)
            {
                return View(transaction);
            }
            else
            {
                try
                {
                    transaction = await _context.TblTransaction.FindAsync(id);
                    transaction.Is_Approved = transactionApproved;
                    transaction.Note = Note;
                    _context.Update(transaction);
                    var registration = await _context.Registration.FindAsync(transaction.Reg_Id);
                    registration.Is_Active = true;
                    _context.Update(registration);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionApprovalTransactionExists(transaction.Transaction_ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(TransactionApprovalIndex));
            }

        }

        private bool TransactionApprovalTransactionExists(int id)
        {
            return _context.TblTransaction.Any(e => e.Transaction_ID == id);
        }
        #endregion
        #region UpgradeToNextStep

        //Upgrade To Next Package
        public bool MemberUpgradeToNextPackage(int id)
        {
            var registration = _context.Registration.Find(id);
            registration.Package_ID = registration.Package_ID + 100;
            registration.Level_Id = registration.Level_Id + 1;

            _context.Update(registration);
            int result = _context.SaveChanges();
            return result > 1 ? true : false;
        }

        //Upgrade To Next Level
        public bool MemberUpgradeToNextLevel(int id)
        {
            var registration = _context.Registration.Where(a => a.Reg_Id == id).FirstOrDefault();
            registration.Level_Id = registration.Level_Id + 1;

            _context.Update(registration);
            int result = _context.SaveChanges();
            return result > 1 ? true : false;
        }

        #endregion

        #region Admin Reports
        [Authorize]
        public async Task<List<Registration>> AdminAllMemberChildList()
        {
            var AllMemberChildList = await _context.Registration.FromSql("SELECT s.Reg_Id, s.Full_Name, ((SELECT COUNT(*) FROM [Registration] c WHERE c.Parent_Id = s.Reg_Id ) + (SELECT COUNT(*) FROM [Registration] c JOIN [Registration] p  ON p.Parent_Id = c.Reg_Id  WHERE c.Parent_Id = s.Reg_Id ) ) as child_count FROM [Registration] s").ToListAsync();
            return AllMemberChildList;
        }

        [Authorize]
        public async Task<AdminReportsVM> AdminAllMemberCashhInHandLevel()
        {

            AdminReportsVM adminReportsVM = new AdminReportsVM();
            List<AdminReports> BypackageTotalReport = await _context.TblTransaction.Select(k => new { k.Package_Name, k.Company_Percentage }).GroupBy(x => new { x.Package_Name }, (key, group) => new
          AdminReports
            {
                package_Name = key.Package_Name,
                Amount = group.Sum(k => k.Company_Percentage)
            }).ToListAsync();

            List<AdminReports> BypackageandlevelReport = await _context.TblTransaction.Select(k => new { k.Package_Name, k.Level_Id, k.Company_Percentage }).GroupBy(x => new { x.Package_Name, x.Level_Id }, (key, group) => new
            AdminReports
            {
                package_Name = key.Package_Name,
                Level = key.Level_Id,
                Amount = group.Sum(k => k.Company_Percentage)
            }).ToListAsync();


            adminReportsVM.Bypackage_Report = BypackageTotalReport;
            adminReportsVM.BypackageAndLevel_Report = BypackageandlevelReport;
            return adminReportsVM;
        }
        #endregion
    }
}


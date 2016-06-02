using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using Questionnaire2.Models;
using Questionnaire2.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Questionnaire2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AppDataController : Controller
    {
        private readonly Entities _db = new Entities();
        private readonly ApplicationDbContext _udb = new ApplicationDbContext();

        public ActionResult Index()
        {
            return (ActionResult)this.View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(RegisterExternalLoginModel mReg, string Command, int id = 0)
        {
            if (Command == "MonthlyReport")
            {
                var list1 = this._db.Responses.Where<Respons>((Expression<Func<Respons, bool>>)(w => w.QuestionnaireId == 1)).Select(r => new
                {
                    userId = r.UserId,
                    questionText = r.QuestionText,
                    questionResponse = r.ResponseItem,
                    subordinal = r.SubOrdinal
                }).ToList();
                var list2 = this._db.UserLevels.Select(s => new
                {
                    userId = s.UserId,
                    finalStepLevel = s.FinalStepLevel,
                    finalStepLevelDate = s.FinalStepLevelDate
                }).ToList();
                IEnumerable<int> ints = list1.Select(s => s.userId).Distinct<int>();
                ExcelPackage excelPackage = new ExcelPackage();
                ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.Add("Users");
                excelWorksheet.Cells[1, 1].Value = (object)"First Name";
                excelWorksheet.Cells[1, 2].Value = (object)"Last Name";
                excelWorksheet.Cells[1, 3].Value = (object)"EMail";
                excelWorksheet.Cells[1, 4].Value = (object)"Phone";
                excelWorksheet.Cells[1, 5].Value = (object)"Home Address";
                excelWorksheet.Cells[1, 6].Value = (object)"City";
                excelWorksheet.Cells[1, 7].Value = (object)"State";
                excelWorksheet.Cells[1, 8].Value = (object)"Zip";
                excelWorksheet.Cells[1, 9].Value = (object)"Highest Edu";
                excelWorksheet.Cells[1, 10].Value = (object)"EC Provider";
                excelWorksheet.Cells[1, 11].Value = (object)"T/TA Provider";
                excelWorksheet.Cells[1, 12].Value = (object)"Credentials";
                excelWorksheet.Cells[1, 13].Value = (object)"Verified";
                excelWorksheet.Cells[1, 14].Value = (object)"Final Level";
                int index = 2;
                List<string> stringList = new List<string>()
        {
          "First Name",
          "Last Name",
          "EMail",
          "Phone",
          "Home Address",
          "City",
          "State",
          "Zip",
          "Highest Level of Education",
          "Career Pathways",
          "TA Provider"
        };
                List<string> list3 = this._udb.Users.Select<ApplicationUser, string>((Expression<Func<ApplicationUser, string>>)(x => x.Id)).Distinct<string>().ToList<string>();
                List<int> intList = new List<int>();
                foreach (string g in list3)
                {
                    int int32 = BitConverter.ToInt32(new Guid(g).ToByteArray(), 0);
                    intList.Add(int32);
                }
                foreach (int num1 in ints)
                {
                    int d_id = num1;
                    string userGuid = "";
                    foreach (string g in list3)
                    {
                        if (BitConverter.ToInt32(new Guid(g).ToByteArray(), 0) == d_id)
                            userGuid = g;
                    }
                    if (userGuid != "")
                    {
                        string firstName = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>)(x => x.Id == userGuid)).FirstName;
                        excelWorksheet.Cells[index, 1].Value = (object)firstName;
                        string lastName = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>)(x => x.Id == userGuid)).LastName;
                        excelWorksheet.Cells[index, 2].Value = (object)lastName;
                        string email = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>)(x => x.Id == userGuid)).Email;
                        excelWorksheet.Cells[index, 3].Value = (object)email;
                        string phoneNumber = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>)(x => x.Id == userGuid)).PhoneNumber;
                        excelWorksheet.Cells[index, 4].Value = (object)phoneNumber;
                        string address1 = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>)(x => x.Id == userGuid)).Address1;
                        excelWorksheet.Cells[index, 5].Value = (object)address1;
                        string city = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>)(x => x.Id == userGuid)).City;
                        excelWorksheet.Cells[index, 6].Value = (object)city;
                        string state = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>)(x => x.Id == userGuid)).State;
                        excelWorksheet.Cells[index, 7].Value = (object)state;
                        string zip = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>)(x => x.Id == userGuid)).ZIP;
                        excelWorksheet.Cells[index, 8].Value = (object)zip;
                        if (list1.Any(a =>
                        {
                            if (a.userId == d_id && a.questionText.ToLower().Contains("highest"))
                                return a.questionText.ToLower().Contains("education");
                            return false;
                        }))
                        {
                            string questionResponse = list1.FirstOrDefault(a =>
                            {
                                if (a.userId == d_id && a.questionText.ToLower().Contains("highest"))
                                    return a.questionText.ToLower().Contains("education");
                                return false;
                            }).questionResponse;
                            excelWorksheet.Cells[index, 9].Value = (object)questionResponse;
                        }
                        if (list1.Any(a =>
                        {
                            if (a.userId == d_id && a.questionText.ToLower().Contains("career"))
                                return a.questionText.ToLower().Contains("pathway");
                            return false;
                        }))
                        {
                            string questionResponse = list1.FirstOrDefault(a =>
                            {
                                if (a.userId == d_id && a.questionText.ToLower().Contains("career"))
                                    return a.questionText.ToLower().Contains("pathway");
                                return false;
                            }).questionResponse;
                            excelWorksheet.Cells[index, 10].Value = (object)questionResponse;
                        }
                        if (list1.Any(a =>
                        {
                            if (a.userId == d_id && a.questionText.ToLower().Contains("trainer"))
                                return a.questionText.ToLower().Contains("ta provider");
                            return false;
                        }))
                        {
                            string questionResponse = list1.FirstOrDefault(a =>
                            {
                                if (a.userId == d_id && a.questionText.ToLower().Contains("trainer"))
                                    return a.questionText.ToLower().Contains("ta provider");
                                return false;
                            }).questionResponse;
                            excelWorksheet.Cells[index, 11].Value = (object)questionResponse;
                        }
                        string str1 = "";
                        if (list1.Any(a =>
                        {
                            if (a.userId == d_id && a.questionText.ToLower().Contains("credential"))
                                return a.questionText.ToLower().Contains("type");
                            return false;
                        }))
                        {
                            List<Respons> responsList = new List<Respons>()
              {
                new Respons()
                {
                  ResponseItem = "Not Found"
                }
              };
                            var source = list1.Where(c =>
                            {
                                if (c.userId == d_id && c.questionText.ToLower().Contains("credential"))
                                    return c.questionText.ToLower().Contains("type");
                                return false;
                            });
                            int num2 = source.Select(x => x.subordinal).Max();
                            for (int i = 0; i < num2 + 1; ++i)
                            {
                                string str2 = "";
                                string str3 = "";
                                if (source.Any(x =>
                                {
                                    if (x.userId == d_id && x.questionText.ToLower().Contains("credential"))
                                        return x.questionText.ToLower().Contains("type");
                                    return false;
                                }))
                                    str2 = source.FirstOrDefault(x =>
                                    {
                                        if (x.userId == d_id && x.questionText.ToLower().Contains("credential") && x.questionText.ToLower().Contains("type"))
                                            return x.subordinal == i;
                                        return false;
                                    }).questionResponse;
                                if (source.Any(x =>
                                {
                                    if (x.userId == d_id && x.questionText.ToLower().Contains("credential"))
                                        return x.questionText.ToLower().Contains("name");
                                    return false;
                                }))
                                    str3 = source.FirstOrDefault(x =>
                                    {
                                        if (x.userId == d_id && x.questionText.ToLower().Contains("credential") && x.questionText.ToLower().Contains("name"))
                                            return x.subordinal == i;
                                        return false;
                                    }).questionResponse;
                                str1 = str1 + str2 + ":" + str3 + ",";
                            }
                            if (str1.Length > 1)
                                str1 = str1.Substring(0, str1.Length - 1);
                        }
                        excelWorksheet.Cells[index, 12].Value = (object)str1;
                        if (list2.Any(x => x.userId == d_id))
                        {
                            excelWorksheet.Cells[index, 13].Value = (object)"Yes";
                            excelWorksheet.Cells[index, 14].Value = (object)list2.Where(x => x.userId == d_id).FirstOrDefault().finalStepLevel;
                        }
                        else
                            excelWorksheet.Cells[index, 13].Value = (object)"No";
                        ++index;
                    }
                }
                MemoryStream memoryStream = new MemoryStream();
                excelPackage.SaveAs((Stream)memoryStream);
                string fileDownloadName = "Monthly_Report.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                memoryStream.Position = 0L;
                return (ActionResult)this.File((Stream)memoryStream, contentType, fileDownloadName);
            }
            if (Command == "ExportAllData")
            {
                var list1 = this._db.Responses.Select(r => new
                {
                    userId = r.UserId,
                    qCategoryId = r.QCategoryId,
                    qCategoryName = r.QCategoryName,
                    questionText = r.QuestionText,
                    questionResponse = r.ResponseItem
                }).ToList();
                var list2 = this._db.UserLevels.Select(r => new
                {
                    userId = r.UserId,
                    finalStepLevel = r.FinalStepLevel
                }).ToList();
                var list3 = this._db.Verifications.Select(r => new
                {
                    userId = r.UserId,
                    itemInfo = r.ItemInfo,
                    itemStepLevel = r.ItemStepLevel,
                    itemverified = r.ItemVerified
                }).ToList();
                ExcelPackage excelPackage = new ExcelPackage();
                ExcelWorksheet excelWorksheet1 = excelPackage.Workbook.Worksheets.Add("Responses");
                excelWorksheet1.Cells[1, 1].Value = (object)"UserId";
                excelWorksheet1.Cells[1, 2].Value = (object)"QCategoryId";
                excelWorksheet1.Cells[1, 3].Value = (object)"QCategoryName";
                excelWorksheet1.Cells[1, 4].Value = (object)"QuestionText";
                excelWorksheet1.Cells[1, 5].Value = (object)"QuestionResponse";
                for (int index = 0; index < list1.Count; ++index)
                {
                    excelWorksheet1.Cells[index + 2, 1].Value = (object)list1[index].userId;
                    excelWorksheet1.Cells[index + 2, 2].Value = (object)list1[index].qCategoryId;
                    excelWorksheet1.Cells[index + 2, 3].Value = (object)list1[index].qCategoryName;
                    excelWorksheet1.Cells[index + 2, 4].Value = (object)list1[index].questionText;
                    excelWorksheet1.Cells[index + 2, 5].Value = (object)list1[index].questionResponse;
                }
                ExcelWorksheet excelWorksheet2 = excelPackage.Workbook.Worksheets.Add("UserLevels");
                excelWorksheet2.Cells[1, 1].Value = (object)"UserId";
                excelWorksheet2.Cells[1, 2].Value = (object)"FinalStepLevel";
                for (int index = 0; index < list2.Count; ++index)
                {
                    excelWorksheet2.Cells[index + 2, 1].Value = (object)list2[index].userId;
                    excelWorksheet2.Cells[index + 2, 2].Value = (object)list2[index].finalStepLevel;
                }
                ExcelWorksheet excelWorksheet3 = excelPackage.Workbook.Worksheets.Add("Verifications");
                excelWorksheet3.Cells[1, 1].Value = (object)"UserId";
                excelWorksheet3.Cells[1, 2].Value = (object)"ItemInfo";
                excelWorksheet3.Cells[1, 3].Value = (object)"ItemStepLevel";
                excelWorksheet3.Cells[1, 4].Value = (object)"ItemVerified";
                for (int index = 0; index < list3.Count; ++index)
                {
                    excelWorksheet3.Cells[index + 2, 1].Value = (object)list3[index].userId;
                    excelWorksheet3.Cells[index + 2, 2].Value = (object)list3[index].itemInfo;
                    excelWorksheet3.Cells[index + 2, 3].Value = (object)list3[index].itemStepLevel;
                    excelWorksheet3.Cells[index + 2, 4].Value = (object)list3[index].itemverified;
                }
                MemoryStream memoryStream = new MemoryStream();
                excelPackage.SaveAs((Stream)memoryStream);
                string fileDownloadName = "myfilename.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                memoryStream.Position = 0L;
                return (ActionResult)this.File((Stream)memoryStream, contentType, fileDownloadName);
            }
            if (Command == "VerificationsReport")
            {
                List<UserInfo> source1 = new List<UserInfo>();
                List<int> list = this._db.Verifications.Select<Verification, int>((Expression<Func<Verification, int>>)(x => x.UserId)).Distinct<int>().ToList<int>();
                for (int index = 0; index < list.Count<int>(); ++index)
                {
                    UserInfo userInfo = new UserInfo()
                    {
                        UserId = list[index]
                    };
                    userInfo.VerifiedCount = this._db.Verifications.Count<Verification>((Expression<Func<Verification, bool>>)(x => x.UserId == userInfo.UserId && x.QuestionnaireId == 1 && x.ItemVerified));
                    userInfo.UnverifiedCount = this._db.Verifications.Count<Verification>((Expression<Func<Verification, bool>>)(x => x.UserId == userInfo.UserId && x.QuestionnaireId == 1 && x.ItemVerified == false));
                    userInfo.Editable = (!this._db.Verifications.Any<Verification>((Expression<Func<Verification, bool>>)(x => x.UserId == userInfo.UserId && x.QuestionnaireId == 1 && x.Editable == false)) ? 1 : 0) != 0;
                    ApplicationUser applicationUser = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>)(x => x.Id == userInfo.UserId.ToString()));
                    if (applicationUser != null)
                        userInfo.UserName = applicationUser.UserName;
                    IQueryable<Respons> source2 = this._db.Responses.Where<Respons>((Expression<Func<Respons, bool>>)(x => x.UserId == userInfo.UserId && x.QCategoryName.ToUpper().Contains("PERSONAL")));
                    Respons respons1 = source2.FirstOrDefault<Respons>((Expression<Func<Respons, bool>>)(x => x.QuestionText.ToUpper().Contains("FIRST")));
                    if (respons1 != null)
                        userInfo.FirstName = respons1.ResponseItem;
                    Respons respons2 = source2.FirstOrDefault<Respons>((Expression<Func<Respons, bool>>)(x => x.QuestionText.ToUpper().Contains("LAST")));
                    if (respons2 != null)
                        userInfo.LastName = respons2.ResponseItem;
                    source1.Add(userInfo);
                }
                IEnumerable<UserInfo> source3 = source1.Where<UserInfo>((Func<UserInfo, bool>)(x => x.UnverifiedCount == 0));
                IEnumerable<UserInfo> source4 = source1.Where<UserInfo>((Func<UserInfo, bool>)(x => x.UnverifiedCount != 0));
                UserVerifications userVerifications = new UserVerifications()
                {
                    UsersVerified = source3.ToList<UserInfo>(),
                    UsersUnverified = source4.ToList<UserInfo>()
                };
                string str1 = "<html><head></head><body><table>" + "<tr><td colspan=4><h1>User Verification Status</h1></td></tr>" + "<tr><td colspan=4><h2>Unverified Users</h2></td></tr>" + "<tr><th>First Name</th><th>Last Name</th><th>Username</th><th>Status</th></tr>";
                foreach (UserInfo userInfo in source4)
                {
                    str1 = str1 + "<tr><td>" + userInfo.FirstName + "</td>";
                    str1 = str1 + "<tr><td>" + userInfo.LastName + "</td>";
                    str1 = str1 + "<tr><td>" + userInfo.UserName + "</td>";
                    str1 = str1 + "<tr><td>" + (object)userInfo.VerifiedCount + "/" + (object)userInfo.UnverifiedCount + "</td>";
                }
                string str2 = str1 + "<tr><td colspan=4><h2>Verified Users</h2></td></tr>" + "<tr><th>First Name</th><th>Last Name</th><th>Username</th><th>Status</th></tr>";
                foreach (UserInfo userInfo in source3)
                {
                    str2 = str2 + "<tr><td>" + userInfo.FirstName + "</td>";
                    str2 = str2 + "<tr><td>" + userInfo.LastName + "</td>";
                    str2 = str2 + "<tr><td>" + userInfo.UserName + "</td>";
                    str2 = str2 + "<tr><td>" + (object)userInfo.VerifiedCount + "/" + (object)userInfo.UnverifiedCount + "</td>";
                }
                this.tableToPdf((object)this, new EventArgs(), str2 + "</table></body></html>");
            }
            return (ActionResult)this.View((object)this._db.AppSettings.ToList<AppSetting>());
        }

        public void tableToPdf(object sender, EventArgs e, string pageHtml)
        {
            Document document = new Document(PageSize.A4, 20f, 10f, 10f, 10f);
            try
            {
                MemoryStream memoryStream1 = new MemoryStream();
                PdfWriter.GetInstance(document, (Stream)memoryStream1);
                document.Open();
                foreach (IElement to in HTMLWorker.ParseToList((TextReader)new StringReader(pageHtml), (StyleSheet)null))
                    document.Add(to);
                document.Close();
                MemoryStream memoryStream2 = new MemoryStream(memoryStream1.ToArray());
                this.Response.Buffer = true;
                this.Response.ContentType = "application/pdf";
                this.Response.AddHeader("content-disposition", "attachment; filename=" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");
                this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                memoryStream2.WriteTo(this.Response.OutputStream);
                this.Response.End();
            }
            catch (Exception ex)
            {
                this.Response.Write(ex.ToString());
            }
        }

        public ActionResult Details(int id)
        {
            return (ActionResult)this.View();
        }

        public ActionResult Create()
        {
            return (ActionResult)this.View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                return (ActionResult)this.RedirectToAction("Index");
            }
            catch
            {
                return (ActionResult)this.View();
            }
        }

        public ActionResult Edit(int id)
        {
            return (ActionResult)this.View();
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                return (ActionResult)this.RedirectToAction("Index");
            }
            catch
            {
                return (ActionResult)this.View();
            }
        }

        public ActionResult Delete(int id)
        {
            return (ActionResult)this.View();
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                return (ActionResult)this.RedirectToAction("Index");
            }
            catch
            {
                return (ActionResult)this.View();
            }
        }
    }
}


//using iTextSharp.text;
//using iTextSharp.text.html.simpleparser;
//using iTextSharp.text.pdf;
//using OfficeOpenXml;
//using Questionnaire2.Models;
//using Questionnaire2.ViewModels;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Web;
//using System.Web.Mvc;

//namespace Questionnaire2.Controllers
//{
//    [Authorize(Roles = "Admin")]
//    public class AppDataController : Controller
//    {
//        private readonly Entities _db = new Entities();
//    private readonly ApplicationDbContext _udb = new ApplicationDbContext();

//    public ActionResult Index()
//    {
//      return (ActionResult) this.View();
//    }

//    [ValidateAntiForgeryToken]
//    [HttpPost]
//    public ActionResult Index(RegisterExternalLoginModel mReg, string Command, int id = 0)
//    {
//      if (Command == "MonthlyReport")
//      {
//        var list1 = this._db.Responses.Where<Respons>((Expression<Func<Respons, bool>>) (w => w.QuestionnaireId == 1)).Select(r => new
//        {
//          userId = r.UserId,
//          questionText = r.QuestionText,
//          questionResponse = r.ResponseItem,
//          subordinal = r.SubOrdinal
//        }).ToList();
//        var list2 = this._db.UserLevels.Select(s => new
//        {
//          userId = s.UserId,
//          finalStepLevel = s.FinalStepLevel,
//          finalStepLevelDate = s.FinalStepLevelDate
//        }).ToList();
//        IEnumerable<int> ints = list1.Select(s => s.userId).Distinct<int>();
//        ExcelPackage excelPackage = new ExcelPackage();
//        ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.Add("Users");
//        excelWorksheet.Cells[1, 1].Value = (object) "First Name";
//        excelWorksheet.Cells[1, 2].Value = (object) "Last Name";
//        excelWorksheet.Cells[1, 3].Value = (object) "EMail";
//        excelWorksheet.Cells[1, 4].Value = (object) "Phone";
//        excelWorksheet.Cells[1, 5].Value = (object) "Home Address";
//        excelWorksheet.Cells[1, 6].Value = (object) "City";
//        excelWorksheet.Cells[1, 7].Value = (object) "State";
//        excelWorksheet.Cells[1, 8].Value = (object) "Zip";
//        excelWorksheet.Cells[1, 9].Value = (object) "Highest Edu";
//        excelWorksheet.Cells[1, 10].Value = (object) "EC Provider";
//        excelWorksheet.Cells[1, 11].Value = (object) "T/TA Provider";
//        excelWorksheet.Cells[1, 12].Value = (object) "Credentials";
//        excelWorksheet.Cells[1, 13].Value = (object) "Verified";
//        excelWorksheet.Cells[1, 14].Value = (object) "Final Level";
//        int index = 2;
//        List<string> stringList = new List<string>()
//        {
//          "First Name",
//          "Last Name",
//          "EMail",
//          "Phone",
//          "Home Address",
//          "City",
//          "State",
//          "Zip",
//          "Highest Level of Education",
//          "Career Pathways",
//          "TA Provider"
//        };
//        List<string> list3 = this._udb.Users.Select<ApplicationUser, string>((Expression<Func<ApplicationUser, string>>) (x => x.Id)).Distinct<string>().ToList<string>();
//        List<int> intList = new List<int>();
//        foreach (string g in list3)
//        {
//          int int32 = BitConverter.ToInt32(new Guid(g).ToByteArray(), 0);
//          intList.Add(int32);
//        }
//        foreach (int num1 in ints)
//        {
//          int d_id = num1;
//          string userGuid = "";
//          foreach (string g in list3)
//          {
//            if (BitConverter.ToInt32(new Guid(g).ToByteArray(), 0) == d_id)
//              userGuid = g;
//          }
//          if (userGuid != "")
//          {
//            string firstName = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>) (x => x.Id == userGuid)).FirstName;
//            excelWorksheet.Cells[index, 1].Value = (object) firstName;
//            string lastName = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>) (x => x.Id == userGuid)).LastName;
//            excelWorksheet.Cells[index, 2].Value = (object) lastName;
//            string email = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>) (x => x.Id == userGuid)).Email;
//            excelWorksheet.Cells[index, 3].Value = (object) email;
//            string phoneNumber = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>) (x => x.Id == userGuid)).PhoneNumber;
//            excelWorksheet.Cells[index, 4].Value = (object) phoneNumber;
//            string address1 = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>) (x => x.Id == userGuid)).Address1;
//            excelWorksheet.Cells[index, 5].Value = (object) address1;
//            string city = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>) (x => x.Id == userGuid)).City;
//            excelWorksheet.Cells[index, 6].Value = (object) city;
//            string state = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>) (x => x.Id == userGuid)).State;
//            excelWorksheet.Cells[index, 7].Value = (object) state;
//            string zip = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>) (x => x.Id == userGuid)).ZIP;
//            excelWorksheet.Cells[index, 8].Value = (object) zip;
//            if (list1.Any(a =>
//            {
//              if (a.userId == d_id && a.questionText.ToLower().Contains("highest"))
//                return a.questionText.ToLower().Contains("education");
//              return false;
//            }))
//            {
//              string questionResponse = list1.FirstOrDefault(a =>
//              {
//                if (a.userId == d_id && a.questionText.ToLower().Contains("highest"))
//                  return a.questionText.ToLower().Contains("education");
//                return false;
//              }).questionResponse;
//              excelWorksheet.Cells[index, 9].Value = (object) questionResponse;
//            }
//            if (list1.Any(a =>
//            {
//              if (a.userId == d_id && a.questionText.ToLower().Contains("career"))
//                return a.questionText.ToLower().Contains("pathway");
//              return false;
//            }))
//            {
//              string questionResponse = list1.FirstOrDefault(a =>
//              {
//                if (a.userId == d_id && a.questionText.ToLower().Contains("career"))
//                  return a.questionText.ToLower().Contains("pathway");
//                return false;
//              }).questionResponse;
//              excelWorksheet.Cells[index, 10].Value = (object) questionResponse;
//            }
//            if (list1.Any(a =>
//            {
//              if (a.userId == d_id && a.questionText.ToLower().Contains("trainer"))
//                return a.questionText.ToLower().Contains("ta provider");
//              return false;
//            }))
//            {
//              string questionResponse = list1.FirstOrDefault(a =>
//              {
//                if (a.userId == d_id && a.questionText.ToLower().Contains("trainer"))
//                  return a.questionText.ToLower().Contains("ta provider");
//                return false;
//              }).questionResponse;
//              excelWorksheet.Cells[index, 11].Value = (object) questionResponse;
//            }
//            string str1 = "";
//            if (list1.Any(a =>
//            {
//              if (a.userId == d_id && a.questionText.ToLower().Contains("credential"))
//                return a.questionText.ToLower().Contains("type");
//              return false;
//            }))
//            {
//              List<Respons> responsList = new List<Respons>()
//              {
//                new Respons()
//                {
//                  ResponseItem = "Not Found"
//                }
//              };
//              var source = list1.Where(c =>
//              {
//                if (c.userId == d_id && c.questionText.ToLower().Contains("credential"))
//                  return c.questionText.ToLower().Contains("type");
//                return false;
//              });
//              int num2 = source.Select(x => x.subordinal).Max();
//              for (int i = 0; i < num2 + 1; ++i)
//              {
//                string str2 = "";
//                string str3 = "";
//                if (source.Any(x =>
//                {
//                  if (x.userId == d_id && x.questionText.ToLower().Contains("credential"))
//                    return x.questionText.ToLower().Contains("type");
//                  return false;
//                }))
//                  str2 = source.FirstOrDefault(x =>
//                  {
//                    if (x.userId == d_id && x.questionText.ToLower().Contains("credential") && x.questionText.ToLower().Contains("type"))
//                      return x.subordinal == i;
//                    return false;
//                  }).questionResponse;
//                if (source.Any(x =>
//                {
//                  if (x.userId == d_id && x.questionText.ToLower().Contains("credential"))
//                    return x.questionText.ToLower().Contains("name");
//                  return false;
//                }))
//                  str3 = source.FirstOrDefault(x =>
//                  {
//                    if (x.userId == d_id && x.questionText.ToLower().Contains("credential") && x.questionText.ToLower().Contains("name"))
//                      return x.subordinal == i;
//                    return false;
//                  }).questionResponse;
//                str1 = str1 + str2 + ":" + str3 + ",";
//              }
//              if (str1.Length > 1)
//                str1 = str1.Substring(0, str1.Length - 1);
//            }
//            excelWorksheet.Cells[index, 12].Value = (object) str1;
//            if (list2.Any(x => x.userId == d_id))
//            {
//              excelWorksheet.Cells[index, 13].Value = (object) "Yes";
//              excelWorksheet.Cells[index, 14].Value = (object) list2.Where(x => x.userId == d_id).FirstOrDefault().finalStepLevel;
//            }
//            else
//              excelWorksheet.Cells[index, 13].Value = (object) "No";
//            ++index;
//          }
//        }
//        MemoryStream memoryStream = new MemoryStream();
//        excelPackage.SaveAs((Stream) memoryStream);
//        string fileDownloadName = "Monthly_Report.xlsx";
//        string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
//        memoryStream.Position = 0L;
//        return (ActionResult) this.File((Stream) memoryStream, contentType, fileDownloadName);
//      }
//      if (Command == "ExportAllData")
//      {
//        var list1 = this._db.Responses.Select(r => new
//        {
//          userId = r.UserId,
//          qCategoryId = r.QCategoryId,
//          qCategoryName = r.QCategoryName,
//          questionText = r.QuestionText,
//          questionResponse = r.ResponseItem
//        }).ToList();
//        var list2 = this._db.UserLevels.Select(r => new
//        {
//          userId = r.UserId,
//          finalStepLevel = r.FinalStepLevel
//        }).ToList();
//        var list3 = this._db.Verifications.Select(r => new
//        {
//          userId = r.UserId,
//          itemInfo = r.ItemInfo,
//          itemStepLevel = r.ItemStepLevel,
//          itemverified = r.ItemVerified
//        }).ToList();
//        ExcelPackage excelPackage = new ExcelPackage();
//        ExcelWorksheet excelWorksheet1 = excelPackage.Workbook.Worksheets.Add("Responses");
//        excelWorksheet1.Cells[1, 1].Value = (object) "UserId";
//        excelWorksheet1.Cells[1, 2].Value = (object) "QCategoryId";
//        excelWorksheet1.Cells[1, 3].Value = (object) "QCategoryName";
//        excelWorksheet1.Cells[1, 4].Value = (object) "QuestionText";
//        excelWorksheet1.Cells[1, 5].Value = (object) "QuestionResponse";
//        for (int index = 0; index < list1.Count; ++index)
//        {
//          excelWorksheet1.Cells[index + 2, 1].Value = (object) list1[index].userId;
//          excelWorksheet1.Cells[index + 2, 2].Value = (object) list1[index].qCategoryId;
//          excelWorksheet1.Cells[index + 2, 3].Value = (object) list1[index].qCategoryName;
//          excelWorksheet1.Cells[index + 2, 4].Value = (object) list1[index].questionText;
//          excelWorksheet1.Cells[index + 2, 5].Value = (object) list1[index].questionResponse;
//        }
//        ExcelWorksheet excelWorksheet2 = excelPackage.Workbook.Worksheets.Add("UserLevels");
//        excelWorksheet2.Cells[1, 1].Value = (object) "UserId";
//        excelWorksheet2.Cells[1, 2].Value = (object) "FinalStepLevel";
//        for (int index = 0; index < list2.Count; ++index)
//        {
//          excelWorksheet2.Cells[index + 2, 1].Value = (object) list2[index].userId;
//          excelWorksheet2.Cells[index + 2, 2].Value = (object) list2[index].finalStepLevel;
//        }
//        ExcelWorksheet excelWorksheet3 = excelPackage.Workbook.Worksheets.Add("Verifications");
//        excelWorksheet3.Cells[1, 1].Value = (object) "UserId";
//        excelWorksheet3.Cells[1, 2].Value = (object) "ItemInfo";
//        excelWorksheet3.Cells[1, 3].Value = (object) "ItemStepLevel";
//        excelWorksheet3.Cells[1, 4].Value = (object) "ItemVerified";
//        for (int index = 0; index < list3.Count; ++index)
//        {
//          excelWorksheet3.Cells[index + 2, 1].Value = (object) list3[index].userId;
//          excelWorksheet3.Cells[index + 2, 2].Value = (object) list3[index].itemInfo;
//          excelWorksheet3.Cells[index + 2, 3].Value = (object) list3[index].itemStepLevel;
//          excelWorksheet3.Cells[index + 2, 4].Value = (object) list3[index].itemverified;
//        }
//        MemoryStream memoryStream = new MemoryStream();
//        excelPackage.SaveAs((Stream) memoryStream);
//        string fileDownloadName = "myfilename.xlsx";
//        string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
//        memoryStream.Position = 0L;
//        return (ActionResult) this.File((Stream) memoryStream, contentType, fileDownloadName);
//      }
//      if (Command == "VerificationsReport")
//      {
//        List<UserInfo> source1 = new List<UserInfo>();
//        List<int> list = this._db.Verifications.Select<Verification, int>((Expression<Func<Verification, int>>) (x => x.UserId)).Distinct<int>().ToList<int>();
//        for (int index = 0; index < list.Count<int>(); ++index)
//        {
//          UserInfo userInfo = new UserInfo()
//          {
//            UserId = list[index]
//          };
//          userInfo.VerifiedCount = this._db.Verifications.Count<Verification>((Expression<Func<Verification, bool>>) (x => x.UserId == userInfo.UserId && x.QuestionnaireId == 1 && x.ItemVerified));
//          userInfo.UnverifiedCount = this._db.Verifications.Count<Verification>((Expression<Func<Verification, bool>>) (x => x.UserId == userInfo.UserId && x.QuestionnaireId == 1 && x.ItemVerified == false));
//          userInfo.Editable = (!this._db.Verifications.Any<Verification>((Expression<Func<Verification, bool>>) (x => x.UserId == userInfo.UserId && x.QuestionnaireId == 1 && x.Editable == false)) ? 1 : 0) != 0;
//          ApplicationUser applicationUser = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>) (x => x.Id == userInfo.UserId.ToString()));
//          if (applicationUser != null)
//            userInfo.UserName = applicationUser.UserName;
//          IQueryable<Respons> source2 = this._db.Responses.Where<Respons>((Expression<Func<Respons, bool>>) (x => x.UserId == userInfo.UserId && x.QCategoryName.ToUpper().Contains("PERSONAL")));
//          Respons respons1 = source2.FirstOrDefault<Respons>((Expression<Func<Respons, bool>>) (x => x.QuestionText.ToUpper().Contains("FIRST")));
//          if (respons1 != null)
//            userInfo.FirstName = respons1.ResponseItem;
//          Respons respons2 = source2.FirstOrDefault<Respons>((Expression<Func<Respons, bool>>) (x => x.QuestionText.ToUpper().Contains("LAST")));
//          if (respons2 != null)
//            userInfo.LastName = respons2.ResponseItem;
//          source1.Add(userInfo);
//        }
//        IEnumerable<UserInfo> source3 = source1.Where<UserInfo>((Func<UserInfo, bool>) (x => x.UnverifiedCount == 0));
//        IEnumerable<UserInfo> source4 = source1.Where<UserInfo>((Func<UserInfo, bool>) (x => x.UnverifiedCount != 0));
//        UserVerifications userVerifications = new UserVerifications()
//        {
//          UsersVerified = source3.ToList<UserInfo>(),
//          UsersUnverified = source4.ToList<UserInfo>()
//        };
//        string str1 = "<html><head></head><body><table>" + "<tr><td colspan=4><h1>User Verification Status</h1></td></tr>" + "<tr><td colspan=4><h2>Unverified Users</h2></td></tr>" + "<tr><th>First Name</th><th>Last Name</th><th>Username</th><th>Status</th></tr>";
//        foreach (UserInfo userInfo in source4)
//        {
//          str1 = str1 + "<tr><td>" + userInfo.FirstName + "</td>";
//          str1 = str1 + "<tr><td>" + userInfo.LastName + "</td>";
//          str1 = str1 + "<tr><td>" + userInfo.UserName + "</td>";
//          str1 = str1 + "<tr><td>" + (object) userInfo.VerifiedCount + "/" + (object) userInfo.UnverifiedCount + "</td>";
//        }
//        string str2 = str1 + "<tr><td colspan=4><h2>Verified Users</h2></td></tr>" + "<tr><th>First Name</th><th>Last Name</th><th>Username</th><th>Status</th></tr>";
//        foreach (UserInfo userInfo in source3)
//        {
//          str2 = str2 + "<tr><td>" + userInfo.FirstName + "</td>";
//          str2 = str2 + "<tr><td>" + userInfo.LastName + "</td>";
//          str2 = str2 + "<tr><td>" + userInfo.UserName + "</td>";
//          str2 = str2 + "<tr><td>" + (object) userInfo.VerifiedCount + "/" + (object) userInfo.UnverifiedCount + "</td>";
//        }
//        this.tableToPdf((object) this, new EventArgs(), str2 + "</table></body></html>");
//      }
//      return (ActionResult) this.View((object) this._db.AppSettings.ToList<AppSetting>());
//    }

//    public void tableToPdf(object sender, EventArgs e, string pageHtml)
//    {
//      Document document = new Document(PageSize.A4, 20f, 10f, 10f, 10f);
//      try
//      {
//        MemoryStream memoryStream1 = new MemoryStream();
//        PdfWriter.GetInstance(document, (Stream) memoryStream1);
//        document.Open();
//        foreach (IElement to in HTMLWorker.ParseToList((TextReader) new StringReader(pageHtml), (StyleSheet) null))
//          document.Add(to);
//        document.Close();
//        MemoryStream memoryStream2 = new MemoryStream(memoryStream1.ToArray());
//        this.Response.Buffer = true;
//        this.Response.ContentType = "application/pdf";
//        this.Response.AddHeader("content-disposition", "attachment; filename=" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");
//        this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
//        memoryStream2.WriteTo(this.Response.OutputStream);
//        this.Response.End();
//      }
//      catch (Exception ex)
//      {
//        this.Response.Write(ex.ToString());
//      }
//    }

//        // GET: AppData/Details/5
//        public ActionResult Details(int id)
//        {
//            return View();
//        }

//        // GET: AppData/Create
//        public ActionResult Create()
//        {
//            return View();
//        }

//        // POST: AppData/Create
//        [HttpPost]
//        public ActionResult Create(FormCollection collection)
//        {
//            try
//            {
//                // TODO: Add insert logic here

//                return RedirectToAction("Index");
//            }
//            catch
//            {
//                return View();
//            }
//        }

//        // GET: AppData/Edit/5
//        public ActionResult Edit(int id)
//        {
//            return View();
//        }

//        // POST: AppData/Edit/5
//        [HttpPost]
//        public ActionResult Edit(int id, FormCollection collection)
//        {
//            try
//            {
//                // TODO: Add update logic here

//                return RedirectToAction("Index");
//            }
//            catch
//            {
//                return View();
//            }
//        }

//        // GET: AppData/Delete/5
//        public ActionResult Delete(int id)
//        {
//            return View();
//        }

//        // POST: AppData/Delete/5
//        [HttpPost]
//        public ActionResult Delete(int id, FormCollection collection)
//        {
//            try
//            {
//                // TODO: Add delete logic here

//                return RedirectToAction("Index");
//            }
//            catch
//            {
//                return View();
//            }
//        }
//    }
//}

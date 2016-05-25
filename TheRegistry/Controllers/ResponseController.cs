using Microsoft.AspNet.Identity;
using Microsoft.CSharp.RuntimeBinder;
using Questionnaire2.Helpers;
using Questionnaire2.Models;
using Questionnaire2.ViewModels;
using Spire.Doc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace Questionnaire2.Controllers
{
    [Authorize(Roles = "Admin, CareProvider")]
    public class ResponseController : Controller
    {
        private readonly Entities _db = new Entities();
        private readonly ApplicationDbContext _udb = new ApplicationDbContext();

        public ActionResult Index()
        {
            return (ActionResult)this.View((object)this._db.Responses.ToList<Respons>());
        }

        public ActionResult Details(int id = 0)
        {
            Respons respons = this._db.Responses.Find((object)id);
            if (respons == null)
                return (ActionResult)this.HttpNotFound();
            return (ActionResult)this.View((object)respons);
        }

        public ActionResult Download()
        {
            int userId = BitConverter.ToInt32(new Guid(this.User.Identity.GetUserId()).ToByteArray(), 0);
            return (ActionResult)this.View((object)this._db.UserLevels.Where<UserLevel>((Expression<Func<UserLevel, bool>>)(x => x.UserId == userId)).FirstOrDefault<UserLevel>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Download(RegisterExternalLoginModel mReg, string Command, int id = 0)
        {
            if (Command == "MS Word")
            {
                try
                {
                    int userId = BitConverter.ToInt32(new Guid(this.User.Identity.GetUserId()).ToByteArray(), 0);
                    List<Respons> list = this._db.Responses.Where<Respons>((Expression<Func<Respons, bool>>)(x => x.UserId == userId)).OrderBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.Ordinal)).ThenBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.SubOrdinal)).ThenBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.QQOrd)).ToList<Respons>();
                    List<string> categoryNames = new List<string>()
          {
            "Background Information",
            "Education",
            "Employment",
            "Coursework",
            "Credentials",
            "Training"
          };
                    List<string> stringList = new List<string>();
                    foreach (string str in categoryNames)
                    {
                        string c = str;
                        string qcategoryName = this._db.QCategories.Where<QCategory>((Expression<Func<QCategory, bool>>)(x => x.QCategoryName.Contains(c))).FirstOrDefault<QCategory>().QCategoryName;
                        if (qcategoryName != "")
                            stringList.Add(qcategoryName);
                    }
                    new Document((Stream)new MemoryStream(MakeWordFile.CreateDocument(new FormatUserInformation(list, categoryNames).Format()).ToArray())).SaveToFile("Portfolio.docx", FileFormat.Docx, System.Web.HttpContext.Current.Response, HttpContentType.Attachment);
                }
                catch (Exception ex)
                {
                    this.Response.Write(ex.Message);
                }
            }
            else if (Command == "Pdf")
            {
                try
                {
                    int userId = BitConverter.ToInt32(new Guid(this.User.Identity.GetUserId()).ToByteArray(), 0);
                    new Document((Stream)new MemoryStream(MakeWordFile.CreateDocument(new FormatUserInformation(this._db.Responses.Where<Respons>((Expression<Func<Respons, bool>>)(x => x.UserId == userId)).OrderBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.Ordinal)).ThenBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.SubOrdinal)).ThenBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.QQOrd)).ToList<Respons>(), new List<string>()
          {
            "Background Information",
            "Education",
            "Employment",
            "Coursework",
            "Credentials",
            "Training"
          }).Format()).ToArray())).SaveToFile("Portfolio.pdf", FileFormat.PDF, System.Web.HttpContext.Current.Response, HttpContentType.Attachment);
                }
                catch (Exception ex)
                {
                    this.Response.Write(ex.Message);
                }
            }
            else if (Command == "Certificate")
            {
                Guid guid = new Guid(this.User.Identity.GetUserId());
                string userIdStr = this.User.Identity.GetUserId();
                int userId = BitConverter.ToInt32(guid.ToByteArray(), 0);
                ApplicationUser applicationUser = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>)(s => s.Id == userIdStr));
                UserLevel userLevel = this._db.UserLevels.Where<UserLevel>((Expression<Func<UserLevel, bool>>)(q => q.UserId == userId)).First<UserLevel>();
                string @string = userLevel.FinalStepLevelDate.Value.ToString("d");
                string finalStepLevel = userLevel.FinalStepLevel;
                string newValue1 = "Zelda Boyd";
                string firstName = applicationUser.FirstName;
                string middleInitial = applicationUser.MiddleInitial;
                string lastName = applicationUser.LastName;
                string newValue2 = firstName + " " + (middleInitial != "" ? middleInitial + " " : "") + lastName;
                string physicalApplicationPath = this.Request.PhysicalApplicationPath;
                string fileName1 = physicalApplicationPath + "Content\\VPDR_Certificate_10.docx";
                //physicalApplicationPath + "Content\\VPDR_Certificate_" + lastName + "_" + firstName + ".docx";
                //physicalApplicationPath + "Content\\VPDR_Certificate_" + lastName + "_" + firstName + ".pdf";
                Document document = new Document();
                document.LoadFromFile(fileName1);
                document.Replace("PROVIDER", newValue2, true, true);
                document.Replace("LEVEL", finalStepLevel, true, true);
                document.Replace("DATE", @string, true, true);
                document.Replace("SIGNATURE", newValue1, true, true);
                string fileName2 = "VPDR_Certificate_" + lastName + "_" + firstName + ".pdf";
                document.SaveToFile(fileName2, FileFormat.PDF, System.Web.HttpContext.Current.Response, HttpContentType.Attachment);
            }
            if (this.ModelState.IsValid)
                return (ActionResult)this.RedirectToAction("Index");
            return (ActionResult)this.RedirectToAction("Download");
        }

        //
        // GET: /Response/Edit/5

        public ActionResult Edit(int id = 1)
        {
            ViewBag.Title = "Child Care Professional Registry Application";
            ViewBag.Message = "";
            var userGuid = new Guid(User.Identity.GetUserId()); //WebSecurity.GetUserId(User.Identity.Name);
            var userId = BitConverter.ToInt32(userGuid.ToByteArray(), 0);

            var lockedSections =
                _db.Verifications.Where(x => x.UserId == userId && x.Editable == false).Select(x => x.QQCategoryId).ToList();

            var viewModel = new QuestionnaireAppData();

            /* Add Fully Loaded (.Included) Questionnaire to the ViewModel */
            viewModel.Questionnaire =
                _db.Questionnaires
                    .Include(a => a.QuestionnaireQuestions
                        .Select(b => b.Question.QType).Select(c => c.Answers))
                    .Include(a => a.QuestionnaireQCategories
                        .Select(b => b.QCategory))
                    .Where(n => n.QuestionnaireId == id)
                    .Single();

            viewModel.Questionnaire.QuestionnaireQuestions = viewModel.Questionnaire.QuestionnaireQuestions.Where(x => x.UserId == userId || x.UserId == 0).ToList();

            var appQuestions = new List<Respons>();

            var qqList = viewModel.Questionnaire.QuestionnaireQuestions.ToList();

            var distinctQQCId = qqList.Select(x => x.QQCategoryId).Distinct();

            for (var i = 0; i < viewModel.Questionnaire.QuestionnaireQuestions.Count(); i++)
            {

                var qqId = qqList[i].Id;
                var qqCatId = qqList.Single(x => x.Id == qqId).QQCategoryId;
                if (qqCatId != null)
                {
                    var qqCId = (int)qqCatId;

                    if (lockedSections.Contains(qqCId)) continue;
                }
                var responseItem = _db.Responses.Any(a => a.UserId == userId && a.QuestionnaireQuestionId == qqId) ? _db.Responses.FirstOrDefault(a => a.UserId == userId && a.QuestionnaireQuestionId == qqId).ResponseItem : "";

                var answers = qqList[i].Question.QType.Answers;

                var qCategoryName = qqList[i].QuestionnaireQCategory.QCategory.QCategoryName;
                if (qqList[i].QuestionnaireQCategory.QCategory.QCategoryName != "Personal Information" && qqList[i].QuestionnaireQCategory.SubOrdinal > 0)
                    qCategoryName += " (" + (qqList[i].QuestionnaireQCategory.SubOrdinal + 1) + ")";

                appQuestions.Add(new Respons
                {
                    QuestionId = (int)qqList[i].QuestionId,
                    QuestionText = qqList[i].Question.QuestionText,
                    QTitle = qqList[i].Question.QTitle,
                    QTypeResponse = qqList[i].Question.QType.QTypeResponse,
                    QuestionnaireId = (int)qqList[i].QuestionnaireId,
                    QCategoryId = (int)qqList[i].QuestionnaireQCategory.QCategoryId,
                    QCategoryName = qCategoryName,
                    QuestionnaireQuestionId = qqList[i].Id,
                    QuestionnaireQCategoryId = (int)qqList[i].QQCategoryId,
                    QQOrd = qqList[i].Ordinal,
                    Ordinal = qqList[i].QuestionnaireQCategory.Ordinal,
                    SubOrdinal = qqList[i].QuestionnaireQCategory.SubOrdinal,
                    UserId = userId,
                    ResponseItem = responseItem,
                    Answers = answers
                });
            }

            var returnList = appQuestions.OrderBy(x => x.Ordinal).ThenBy(x => x.SubOrdinal).ThenBy(x => x.QQOrd).ToList();
            viewModel.Responses = returnList;

            var qCategories = _db.QCategories.ToList();
            viewModel.QCategories = qCategories;

            return View(viewModel);
        }

        //
        // POST: /Response/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IList<Respons> model, FormCollection formCollection)
        {
            var qqcIds = model.Select(x => x.QuestionnaireQCategoryId).Distinct().ToArray();

            var scope = new TransactionScope(
                // a new transaction will always be created
                TransactionScopeOption.RequiresNew,
                // we will allow volatile data to be read during transaction
                new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }
            );

            if (ModelState.IsValid)
            {
                using (scope)
                {
                    //var forDelete = new List<Respons>();

                    //for (int i = 1; i < qqcIds.Length; i++)
                    //{
                    //    var qqcId = qqcIds[i];
                    //    var query = (from r in _db.Responses
                    //                 select r).ToList().Where(y => y.UserId == model[0].UserId).Where(z => z.QuestionnaireQCategoryId == qqcId);

                    //    //var r = _db.Responses.Where(x => x.UserId == model[0].UserId && x.QuestionnaireQCategoryId == i).First();
                    //    forDelete.AddRange(query);
                    //}

                    var forDelete = (from r in _db.Responses
                                     select r)
                                 .ToList()
                                 .Where(y => y.UserId == model[0].UserId)
                                 .Where(z => qqcIds.Contains(z.QuestionnaireQCategoryId));

                    _db.Responses.RemoveRange(forDelete);
                    //_db.SaveChanges();
                    _db.Responses.AddRange(model);
                    _db.SaveChanges();

                    //var toDelete = _db.Responses.Where(x => x.UserId == model[0].UserId && qqcIds.Contains(x.QuestionnaireQCategoryId));

                    //foreach (var record in forDelete)
                    //{
                    //    _db.Responses.Remove(record);
                    //}


                    //foreach (var record in _db.Responses)
                    //{                          
                    //    if (record.UserId == model[0].UserId && qqcIds.Contains(record.QuestionnaireQCategoryId))
                    //    _db.Responses.Remove(record);                            
                    //}
                    //_db.SaveChanges();

                    //foreach (var r in model)
                    //{
                    //    Respons response = r;
                    //    _db.Responses.Add(response);
                    //    //_db.SaveChanges();
                    //    //var check = _db.Responses.Single(x => x.ResponseId == response.ResponseId);
                    //}                   

                    scope.Complete();
                    return RedirectToAction("Edit", "Response", new { area = "", id = 1 });


                }
                //db.Entry(Response).State = EntityState.Modified;                
            }
            return View();
        }

        //
        // GET: /Response/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Respons response = _db.Responses.Find(id);
            if (response == null)
            {
                return HttpNotFound();
            }
            return View(response);
        }

        //
        // POST: /Response/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Respons response = _db.Responses.Find(id);
            _db.Responses.Remove(response);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        //public void tableToPdf(object sender, EventArgs e, string pageHtml)
        //{
        //    //Set page size as A4
        //    iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 20, 10, 10, 10);

        //    try
        //    {
        //        var ms = new MemoryStream();
        //        PdfWriter.GetInstance(pdfDoc, ms);
        //        //Open PDF Document to write data
        //        pdfDoc.Open();
        //        //Read string contents using stream reader and convert html to parsed conent
        //        var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(pageHtml), null);
        //        //Get each array values from parsed elements and add to the PDF document
        //        foreach (var htmlElement in parsedHtmlElements)
        //            pdfDoc.Add(htmlElement as IElement);
        //        //Close your PDF
        //        pdfDoc.Close();
        //        var ms2 = new MemoryStream(ms.ToArray());
        //        Response.Buffer = true;
        //        Response.ContentType = "application/pdf";
        //        //Set default file Name as current datetime
        //        Response.AddHeader("content-disposition", "attachment; filename=" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");
        //        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //        ms2.WriteTo(Response.OutputStream);
        //        Response.End();
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Write(ex.ToString());
        //    }
        //}

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult AjaxTest()
        {
            var testString = "Hi there";
            return PartialView("_AjaxTest", testString);
        }
    }
}

//using Microsoft.AspNet.Identity;
//using Microsoft.CSharp.RuntimeBinder;
//using Questionnaire2.Helpers;
//using Questionnaire2.Models;
//using Questionnaire2.ViewModels;
//using Spire.Doc;
//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.IO;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Runtime.CompilerServices;
//using System.Transactions;
//using System.Web;
//using System.Web.Mvc;

//namespace Questionnaire2.Controllers
//{
//    [Authorize(Roles = "Admin, CareProvider")]
//    public class ResponseController : Controller
//    {
//        private readonly Entities _db = new Entities();
//        private readonly ApplicationDbContext _udb = new ApplicationDbContext();

//        public ActionResult Index()
//        {
//            return (ActionResult)this.View((object)this._db.Responses.ToList<Respons>());
//        }

//        public ActionResult Details(int id = 0)
//        {
//            Respons respons = this._db.Responses.Find((object)id);
//            if (respons == null)
//                return (ActionResult)this.HttpNotFound();
//            return (ActionResult)this.View((object)respons);
//        }

//        public ActionResult Download()
//        {
//            int userId = BitConverter.ToInt32(new Guid(this.User.Identity.GetUserId()).ToByteArray(), 0);
//            return (ActionResult)this.View((object)this._db.UserLevels.Where<UserLevel>((Expression<Func<UserLevel, bool>>)(x => x.UserId == userId)).FirstOrDefault<UserLevel>());
//        }

//        //
//        // POST: /Response/Create

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Download(RegisterExternalLoginModel mReg, string Command, int id = 0)
//        {
//            if (Command == "MS Word")
//            {
//                try
//                {
//                    int userId = BitConverter.ToInt32(new Guid(this.User.Identity.GetUserId()).ToByteArray(), 0);
//                    List<Respons> list = this._db.Responses.Where<Respons>((Expression<Func<Respons, bool>>)(x => x.UserId == userId)).OrderBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.Ordinal)).ThenBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.SubOrdinal)).ThenBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.QQOrd)).ToList<Respons>();
//                    List<string> categoryNames = new List<string>()
//          {
//            "Background Information",
//            "Education",
//            "Employment",
//            "Coursework",
//            "Credentials",
//            "Training"
//          };
//                    List<string> stringList = new List<string>();
//                    foreach (string str in categoryNames)
//                    {
//                        string c = str;
//                        string qcategoryName = this._db.QCategories.Where<QCategory>((Expression<Func<QCategory, bool>>)(x => x.QCategoryName.Contains(c))).FirstOrDefault<QCategory>().QCategoryName;
//                        if (qcategoryName != "")
//                            stringList.Add(qcategoryName);
//                    }
//                    var temp = new FormatUserInformation(list, categoryNames);
//                    var temp2 = temp.Format();

//                    new Document((Stream)new MemoryStream(MakeWordFile.CreateDocument(new FormatUserInformation(list, categoryNames).Format()).ToArray())).SaveToFile("Portfolio.docx", FileFormat.Docx, System.Web.HttpContext.Current.Response, HttpContentType.Attachment);
//                }
//                catch (Exception ex)
//                {
//                    this.Response.Write(ex.Message);
//                }
//            }
//            else if (Command == "Pdf")
//            {
//                try
//                {
//                    int userId = BitConverter.ToInt32(new Guid(this.User.Identity.GetUserId()).ToByteArray(), 0);

//                    var newdoc = new Document((Stream)new MemoryStream(MakeWordFile.CreateDocument(new FormatUserInformation(this._db.Responses.Where<Respons>((Expression<Func<Respons, bool>>)(x => x.UserId == userId)).OrderBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.Ordinal)).ThenBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.SubOrdinal)).ThenBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.QQOrd)).ToList<Respons>(), new List<string>()
//                      {
//                        "Background Information",
//                        "Education",
//                        "Employment",
//                        "Coursework",
//                        "Credentials",
//                        "Training"
//                      }).Format()).ToArray()));

//                    newdoc.SaveToFile("Portfolio.pdf", FileFormat.PDF, System.Web.HttpContext.Current.Response, HttpContentType.Attachment);

//                    //new Document((Stream)new MemoryStream(MakeWordFile.CreateDocument(new FormatUserInformation(this._db.Responses.Where<Respons>((Expression<Func<Respons, bool>>)(x => x.UserId == userId)).OrderBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.Ordinal)).ThenBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.SubOrdinal)).ThenBy<Respons, int>((Expression<Func<Respons, int>>)(x => x.QQOrd)).ToList<Respons>(), new List<string>()
//                    //  {
//                    //    "Personal Information",
//                    //    "Education",
//                    //    "Employment",
//                    //    "Coursework",
//                    //    "Credentials",
//                    //    "Training"
//                    //  }).Format()).ToArray())).SaveToFile("Portfolio.pdf", FileFormat.PDF, System.Web.HttpContext.Current.Response, HttpContentType.Attachment);
//                }
//                catch (Exception ex)
//                {
//                    this.Response.Write(ex.Message);
//                }
//            }
//            else if (Command == "Certificate")
//            {
//                Guid guid = new Guid(this.User.Identity.GetUserId());
//                string userIdStr = this.User.Identity.GetUserId();
//                int userId = BitConverter.ToInt32(guid.ToByteArray(), 0);
//                ApplicationUser applicationUser = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>)(s => s.Id == userIdStr));
//                UserLevel userLevel = this._db.UserLevels.Where<UserLevel>((Expression<Func<UserLevel, bool>>)(q => q.UserId == userId)).First<UserLevel>();
//                string @string = userLevel.FinalStepLevelDate.Value.ToString("d");
//                string finalStepLevel = userLevel.FinalStepLevel;
//                string newValue1 = "Zelda Boyd";
//                string firstName = applicationUser.FirstName;
//                string middleInitial = applicationUser.MiddleInitial;
//                string lastName = applicationUser.LastName;
//                string newValue2 = firstName + " " + (middleInitial != "" ? middleInitial + " " : "") + lastName;
//                string physicalApplicationPath = this.Request.PhysicalApplicationPath;
//                string fileName1 = physicalApplicationPath + "Content\\VPDR_Certificate_10.docx";
//                //physicalApplicationPath + "Content\\VPDR_Certificate_" + lastName + "_" + firstName + ".docx";
//                //physicalApplicationPath + "Content\\VPDR_Certificate_" + lastName + "_" + firstName + ".pdf";
//                Document document = new Document();
//                document.LoadFromFile(fileName1);
//                document.Replace("PROVIDER", newValue2, true, true);
//                document.Replace("LEVEL", finalStepLevel, true, true);
//                document.Replace("DATE", @string, true, true);
//                document.Replace("SIGNATURE", newValue1, true, true);
//                string fileName2 = "VPDR_Certificate_" + lastName + "_" + firstName + ".pdf";
//                document.SaveToFile(fileName2, FileFormat.PDF, System.Web.HttpContext.Current.Response, HttpContentType.Attachment);
//            }
//            if (this.ModelState.IsValid)
//                return (ActionResult)this.RedirectToAction("Index");
//            return (ActionResult)this.RedirectToAction("Download");
//        }

//        //
//        // GET: /Response/Edit/5

//        public ActionResult Edit(int id = 1)
//        {            
//            ViewBag.Title = "Child Care Professional Registry Application";
//            ViewBag.Message = "";
//            var userGuid = new Guid(User.Identity.GetUserId()); //WebSecurity.GetUserId(User.Identity.Name);
//            var userId = BitConverter.ToInt32(userGuid.ToByteArray(),0);

//            var lockedSections =
//                _db.Verifications.Where(x => x.UserId == userId && x.Editable == false).Select(x => x.QQCategoryId).ToList();     

//            var viewModel = new QuestionnaireAppData();

//            /* Add Fully Loaded (.Included) Questionnaire to the ViewModel */
//            viewModel.Questionnaire =
//                _db.Questionnaires
//                    .Include(a => a.QuestionnaireQuestions
//                        .Select(b => b.Question.QType).Select(c => c.Answers))
//                    .Include(a => a.QuestionnaireQCategories
//                        .Select(b => b.QCategory))
//                    .Where(n => n.QuestionnaireId == id)
//                    .Single();

//            viewModel.Questionnaire.QuestionnaireQuestions = viewModel.Questionnaire.QuestionnaireQuestions.Where(x => x.UserId == userId || x.UserId == 0).ToList();

//            var appQuestions = new List<Respons>();
           
//            var qqList = viewModel.Questionnaire.QuestionnaireQuestions.ToList();

//            var distinctQQCId = qqList.Select(x => x.QQCategoryId).Distinct();

//            for (var i = 0; i < viewModel.Questionnaire.QuestionnaireQuestions.Count(); i++)
//            {
                
//                var qqId = qqList[i].Id;
//                var qqCatId = qqList.Single(x => x.Id == qqId).QQCategoryId;
//                if (qqCatId != null)
//                {
//                    var qqCId = (int)qqCatId;

//                    if (lockedSections.Contains(qqCId)) continue;
//                }
//                var responseItem = _db.Responses.Any(a => a.UserId == userId && a.QuestionnaireQuestionId == qqId) ? _db.Responses.FirstOrDefault(a => a.UserId == userId && a.QuestionnaireQuestionId == qqId).ResponseItem : "";

//                var answers = qqList[i].Question.QType.Answers;

//                var qCategoryName = qqList[i].QuestionnaireQCategory.QCategory.QCategoryName;
//                if (qqList[i].QuestionnaireQCategory.QCategory.QCategoryName != "Personal Information" && qqList[i].QuestionnaireQCategory.SubOrdinal > 0)
//                    qCategoryName += " (" + (qqList[i].QuestionnaireQCategory.SubOrdinal + 1) + ")";

//                appQuestions.Add(new Respons
//                {
//                    QuestionId = (int)qqList[i].QuestionId,
//                    QuestionText = qqList[i].Question.QuestionText,
//                    QTitle = qqList[i].Question.QTitle,
//                    QTypeResponse = qqList[i].Question.QType.QTypeResponse,
//                    QuestionnaireId = (int)qqList[i].QuestionnaireId,
//                    QCategoryId = (int)qqList[i].QuestionnaireQCategory.QCategoryId,
//                    QCategoryName = qCategoryName,
//                    QuestionnaireQuestionId = qqList[i].Id,
//                    QuestionnaireQCategoryId = (int)qqList[i].QQCategoryId,
//                    QQOrd = qqList[i].Ordinal,
//                    Ordinal = qqList[i].QuestionnaireQCategory.Ordinal,
//                    SubOrdinal = qqList[i].QuestionnaireQCategory.SubOrdinal,
//                    UserId = userId,
//                    ResponseItem = responseItem,   
//                    Answers = answers
//                });
//            }

//            var returnList = appQuestions.OrderBy(x => x.Ordinal).ThenBy(x => x.SubOrdinal).ThenBy(x => x.QQOrd).ToList();
//            viewModel.Responses = returnList;

//            var qCategories = _db.QCategories.ToList();
//            viewModel.QCategories = qCategories;

//            return View(viewModel);
//        }

//        //
//        // POST: /Response/Edit/5

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult Edit(IList<Respons> model, FormCollection formCollection)
//        {
//            var qqcIds = model.Select(x => x.QuestionnaireQCategoryId).Distinct().ToArray();
            
//            var scope = new TransactionScope(
//                // a new transaction will always be created
//                TransactionScopeOption.RequiresNew,
//                // we will allow volatile data to be read during transaction
//                new TransactionOptions()
//                {
//                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
//                }
//            );

//            if (ModelState.IsValid)
//            {
//                using (scope)
//                {
//                    //var forDelete = new List<Respons>();
                    
//                    //for (int i = 1; i < qqcIds.Length; i++)
//                    //{
//                    //    var qqcId = qqcIds[i];
//                    //    var query = (from r in _db.Responses
//                    //                 select r).ToList().Where(y => y.UserId == model[0].UserId).Where(z => z.QuestionnaireQCategoryId == qqcId);

//                    //    //var r = _db.Responses.Where(x => x.UserId == model[0].UserId && x.QuestionnaireQCategoryId == i).First();
//                    //    forDelete.AddRange(query);
//                    //}

//                    var forDelete = (from r in _db.Responses
//                                 select r)
//                                 .ToList()
//                                 .Where(y => y.UserId == model[0].UserId)
//                                 .Where(z => qqcIds.Contains(z.QuestionnaireQCategoryId));

//                    _db.Responses.RemoveRange(forDelete);
//                    //_db.SaveChanges();
//                    _db.Responses.AddRange(model);
//                    _db.SaveChanges();

//                    //var toDelete = _db.Responses.Where(x => x.UserId == model[0].UserId && qqcIds.Contains(x.QuestionnaireQCategoryId));

//                    //foreach (var record in forDelete)
//                    //{
//                    //    _db.Responses.Remove(record);
//                    //}
                    

//                    //foreach (var record in _db.Responses)
//                    //{                          
//                    //    if (record.UserId == model[0].UserId && qqcIds.Contains(record.QuestionnaireQCategoryId))
//                    //    _db.Responses.Remove(record);                            
//                    //}
//                    //_db.SaveChanges();
                    
//                    //foreach (var r in model)
//                    //{
//                    //    Respons response = r;
//                    //    _db.Responses.Add(response);
//                    //    //_db.SaveChanges();
//                    //    //var check = _db.Responses.Single(x => x.ResponseId == response.ResponseId);
//                    //}                   

//                    scope.Complete();
//                    return RedirectToAction("Edit", "Response", new { area="", id = 1 });
                    

//                }
//                //db.Entry(Response).State = EntityState.Modified;                
//            }
//            return View();
//        }

//        //
//        // GET: /Response/Delete/5

//        public ActionResult Delete(int id = 0)
//        {
//            Respons response = _db.Responses.Find(id);
//            if (response == null)
//            {
//                return HttpNotFound();
//            }
//            return View(response);
//        }

//        //
//        // POST: /Response/Delete/5

//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public ActionResult DeleteConfirmed(int id)
//        {
//            Respons response = _db.Responses.Find(id);
//            _db.Responses.Remove(response);
//            _db.SaveChanges();
//            return RedirectToAction("Index");
//        }

//        //public void tableToPdf(object sender, EventArgs e, string pageHtml)
//        //{
//        //    //Set page size as A4
//        //    iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 20, 10, 10, 10);

//        //    try
//        //    {
//        //        var ms = new MemoryStream();
//        //        PdfWriter.GetInstance(pdfDoc, ms);
//        //        //Open PDF Document to write data
//        //        pdfDoc.Open();
//        //        //Read string contents using stream reader and convert html to parsed conent
//        //        var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(pageHtml), null);
//        //        //Get each array values from parsed elements and add to the PDF document
//        //        foreach (var htmlElement in parsedHtmlElements)
//        //            pdfDoc.Add(htmlElement as IElement);
//        //        //Close your PDF
//        //        pdfDoc.Close();
//        //        var ms2 = new MemoryStream(ms.ToArray());
//        //        Response.Buffer = true;
//        //        Response.ContentType = "application/pdf";
//        //        //Set default file Name as current datetime
//        //        Response.AddHeader("content-disposition", "attachment; filename=" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");
//        //        Response.Cache.SetCacheability(HttpCacheability.NoCache);
//        //        ms2.WriteTo(Response.OutputStream);
//        //        Response.End();
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Response.Write(ex.ToString());
//        //    }
//        //}

//        protected override void Dispose(bool disposing)
//        {
//            _db.Dispose();
//            base.Dispose(disposing);
//        }

//        public ActionResult AjaxTest()
//        {
//            var testString = "Hi there";
//            return PartialView("_AjaxTest", testString);
//        }
//    }
//}
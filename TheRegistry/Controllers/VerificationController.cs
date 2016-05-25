using Microsoft.AspNet.Identity;
using Questionnaire2.Models;
using Questionnaire2.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web.Mvc;

namespace Questionnaire2.Controllers
{
    public class VerificationController : Controller
    {
        private readonly Entities _db = new Entities();
        private readonly ApplicationDbContext _udb = new ApplicationDbContext();

        [Authorize(Roles = "Admin,Verifier")]
        public ActionResult Index()
        {
            List<UserInfo> source1 = new List<UserInfo>();
            List<int> list1 = this._db.Verifications.Select<Verification, int>((Expression<Func<Verification, int>>)(x => x.UserId)).Distinct<int>().ToList<int>();
            List<string> list2 = this._udb.Users.Select<ApplicationUser, string>((Expression<Func<ApplicationUser, string>>)(x => x.Id)).Distinct<string>().ToList<string>();
            if (list1 != null && list2 != null)
            {
                for (int index = 0; index < list1.Count<int>(); ++index)
                {
                    UserInfo userInfo = new UserInfo()
                    {
                        UserId = list1[index]
                    };
                    if (userInfo != null)
                    {
                        int num1 = this._db.Verifications.Count<Verification>((Expression<Func<Verification, bool>>)(x => x.UserId == userInfo.UserId && x.QuestionnaireId == 1 && x.ItemVerified));
                        int num2 = this._db.Verifications.Count<Verification>((Expression<Func<Verification, bool>>)(x => x.UserId == userInfo.UserId && x.QuestionnaireId == 1 && x.ItemVerified == false));
                        userInfo.VerifiedCount = num1;
                        userInfo.UnverifiedCount = num2;
                        string userGuid = "";
                        foreach (string g in list2)
                        {
                            if (BitConverter.ToInt32(new Guid(g).ToByteArray(), 0) == userInfo.UserId)
                                userGuid = g;
                        }
                        userInfo.Editable = (!this._db.Verifications.Any<Verification>((Expression<Func<Verification, bool>>)(x => x.UserId == userInfo.UserId && x.QuestionnaireId == 1 && x.Editable == false)) ? 1 : 0) != 0;
                        if (userGuid != "")
                        {
                            ApplicationUser applicationUser = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>)(x => x.Id == userGuid));
                            if (applicationUser != null)
                                userInfo.UserName = applicationUser.UserName;
                            this._db.Responses.Where<Respons>((Expression<Func<Respons, bool>>)(x => x.UserId == userInfo.UserId && x.QCategoryName.ToUpper().Contains("PERSONAL")));
                            userInfo.FirstName = applicationUser.FirstName != null ? applicationUser.FirstName : "";
                            userInfo.LastName = applicationUser.LastName != null ? applicationUser.LastName : "";
                            source1.Add(userInfo);
                        }
                    }
                }
            }
            IEnumerable<UserInfo> source2 = source1.Where<UserInfo>((Func<UserInfo, bool>)(x => x.UnverifiedCount == 0));
            IEnumerable<UserInfo> source3 = source1.Where<UserInfo>((Func<UserInfo, bool>)(x => x.UnverifiedCount != 0));
            UserVerifications userVerifications = new UserVerifications();
            if (source2 != null)
                userVerifications.UsersVerified = source2.ToList<UserInfo>();
            if (source3 != null)
                userVerifications.UsersUnverified = source3.ToList<UserInfo>();
            return (ActionResult)this.View((object)userVerifications);
        }

        public ActionResult Details(int id)
        {
            return (ActionResult)this.View();
        }

        [Authorize(Roles = "CareProvider")]
        public ActionResult Verification()
        {
            IDandBool idandBool = new IDandBool();
            int userId = BitConverter.ToInt32(new Guid(this.User.Identity.GetUserId()).ToByteArray(), 0);
            bool flag = this._db.Verifications.Any<Verification>((Expression<Func<Verification, bool>>)(x => x.UserId == userId));
            idandBool.UserId = userId;
            idandBool.HasVerifications = flag;
            return (ActionResult)this.View((object)idandBool);
        }

        [Authorize(Roles = "CareProvider")]
        public ActionResult SendToVerification(int id)
        {
            int userId = BitConverter.ToInt32(new Guid(this.User.Identity.GetUserId()).ToByteArray(), 0);
            List<int> lockedSections = this._db.Verifications.Where<Verification>((Expression<Func<Verification, bool>>)(x => x.UserId == userId && x.Editable == false)).Select<Verification, int>((Expression<Func<Verification, int>>)(x => x.QQCategoryId)).ToList<int>();
            List<Respons> list1 = this._db.Responses.Where<Respons>((Expression<Func<Respons, bool>>)(x => x.UserId == userId && x.QuestionnaireId == 1 && !lockedSections.Contains(x.QuestionnaireQCategoryId))).ToList<Respons>();
            foreach (int num in list1.Select<Respons, int>((Func<Respons, int>)(x => x.QCategoryId)).Distinct<int>())
            {
                int qCategoryId = num;
                List<int> list2 = list1.Where<Respons>((Func<Respons, bool>)(x => x.QCategoryId == qCategoryId)).Select<Respons, int>((Func<Respons, int>)(x => x.SubOrdinal)).Distinct<int>().ToList<int>();
                for (int index = 0; index < list2.Count<int>(); ++index)
                {
                    int subOrdinal = list2[index];
                    int questionnaireId = list1[index].QuestionnaireId;
                    int categoryId = qCategoryId;
                    string qcategoryName = this._db.QCategories.Single<QCategory>((Expression<Func<QCategory, bool>>)(x => x.QCategoryId == categoryId)).QCategoryName;
                    int qqCategoryId = list1.First<Respons>((Func<Respons, bool>)(x =>
                    {
                        if (x.QCategoryId == qCategoryId)
                            return x.SubOrdinal == subOrdinal;
                        return false;
                    })).QuestionnaireQCategoryId;
                    var source = list1.Where<Respons>((Func<Respons, bool>)(x =>
                    {
                        if (x.QCategoryId == categoryId)
                            return x.SubOrdinal == subOrdinal;
                        return false;
                    })).Select(x => new
                    {
                        QuestionText = x.QuestionText,
                        ResponseItem = x.ResponseItem
                    });
                    string str = "<b>" + qcategoryName.ToUpper() + "</b><br />" + source.Aggregate("", (current, item) => current + "<i>" + item.QuestionText + ":</i> " + item.ResponseItem + "<br />");
                    if (this._db.Verifications.Any<Verification>((Expression<Func<Verification, bool>>)(x => x.UserId == userId && x.QQCategoryId == qqCategoryId)))
                    {
                        Verification entity = this._db.Verifications.Single<Verification>((Expression<Func<Verification, bool>>)(x => x.UserId == userId && x.QQCategoryId == qqCategoryId));
                        entity.ItemInfo = str;
                        entity.Editable = false;
                        this._db.Entry<Verification>(entity).State = EntityState.Modified;
                    }
                    else
                        this._db.Verifications.Add(new Verification()
                        {
                            QuestionnaireId = questionnaireId,
                            UserId = userId,
                            QCategoryId = qCategoryId,
                            QQCategoryId = qqCategoryId,
                            SubOrdinal = subOrdinal,
                            ItemInfo = str,
                            ItemVerified = false,
                            ItemStepLevel = "",
                            Editable = false
                        });
                }
            }
            this._db.SaveChanges();
            return (ActionResult)this.View();
        }

        public ActionResult List(int id, int questionnaireId = 1)
        {
            VmVerificationItems verificationItems = new VmVerificationItems()
            {
                VerificationItems = (ICollection<VmVerificationItem>)new Collection<VmVerificationItem>()
            };
            List<Verification> list1 = this._db.Verifications.Where<Verification>((Expression<Func<Verification, bool>>)(x => x.UserId == id && x.QuestionnaireId == questionnaireId)).ToList<Verification>();
            this._db.UserLevels.Any<UserLevel>((Expression<Func<UserLevel, bool>>)(x => x.UserId == id));
            if (!this._db.UserLevels.Any<UserLevel>((Expression<Func<UserLevel, bool>>)(x => x.UserId == id)))
            {
                this._db.UserLevels.Add(new UserLevel()
                {
                    UserId = id,
                    FinalStepLevel = "none",
                    FinalStepLevelDate = new DateTime?(DateTime.Today)
                });
                this._db.SaveChanges();
            }
            UserLevel userLevel = this._db.UserLevels.Where<UserLevel>((Expression<Func<UserLevel, bool>>)(x => x.UserId == id)).SingleOrDefault<UserLevel>();
            verificationItems.UserLevel = userLevel;
            List<SelectListItem> list2 = this._db.LatticeItems.ToList<LatticeItem>().Select<LatticeItem, SelectListItem>((Func<LatticeItem, SelectListItem>)(latticeItem => new SelectListItem()
            {
                Text = latticeItem.DropdownText,
                Value = latticeItem.DropdownText
            })).ToList<SelectListItem>();
            verificationItems.LatticeItems = (IList<SelectListItem>)list2;
            foreach (Verification verification in list1)
            {
                Verification record = verification;
                VmVerificationItem verificationItem1 = new VmVerificationItem();
                verificationItem1.Verification = record;
                verificationItem1.Files = (ICollection<Questionnaire2.Models.File>)this._db.Files.Where<Questionnaire2.Models.File>((Expression<Func<Questionnaire2.Models.File, bool>>)(x => x.UserId == id && x.QuestionnaireId == questionnaireId && x.QuestionnaireQCategoryId == record.QCategoryId && x.QCategorySubOrdinal == record.SubOrdinal)).ToList<Questionnaire2.Models.File>();
                VmVerificationItem verificationItem2 = verificationItem1;
                verificationItems.VerificationItems.Add(verificationItem2);
            }
            return (ActionResult)this.View((object)verificationItems);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult List(VmVerificationItem item)
        {
            try
            {
                this._db.Entry<Verification>(item.Verification).State = EntityState.Modified;
                this._db.SaveChanges();
                return (ActionResult)this.RedirectToAction("List", (object)new
                {
                    id = item.Verification.UserId
                });
            }
            catch
            {
                return (ActionResult)this.View();
            }
        }

        [HttpPost]
        public ActionResult UpdateLevel(FormCollection collection, string stepLevel, int Id)
        {
            UserLevel entity = this._db.UserLevels.Where<UserLevel>((Expression<Func<UserLevel, bool>>)(x => x.Id == Id)).SingleOrDefault<UserLevel>();
            entity.FinalStepLevel = stepLevel;
            entity.FinalStepLevelDate = new DateTime?(DateTime.Now);
            this._db.Entry<UserLevel>(entity).State = EntityState.Modified;
            this._db.SaveChanges();
            return (ActionResult)this.RedirectToAction("List", (object)new { id = entity.UserId });
        }

        public ActionResult Edit(int id)
        {
            VmVerificationItems verificationItems = new VmVerificationItems()
            {
                VerificationItems = (ICollection<VmVerificationItem>)new Collection<VmVerificationItem>()
            };
            List<Verification> list1 = this._db.Verifications.Where<Verification>((Expression<Func<Verification, bool>>)(x => x.Id == id)).ToList<Verification>();
            int questionnaireId = list1.First<Verification>().QuestionnaireId;
            int userId = BitConverter.ToInt32(new Guid(this.User.Identity.GetUserId()).ToByteArray(), 0);
            List<LatticeItem> list2 = this._db.LatticeItems.ToList<LatticeItem>();
            List<SelectListItem> selectListItemList = new List<SelectListItem>();
            foreach (LatticeItem latticeItem in list2)
            {
                SelectListItem selectListItem = new SelectListItem()
                {
                    Text = latticeItem.DropdownText,
                    Value = latticeItem.DropdownText
                };
                selectListItemList.Add(selectListItem);
            }
            verificationItems.LatticeItems = (IList<SelectListItem>)selectListItemList;
            foreach (Verification verification in list1)
            {
                Verification record = verification;
                VmVerificationItem verificationItem1 = new VmVerificationItem();
                verificationItem1.Verification = record;
                verificationItem1.Files = (ICollection<Questionnaire2.Models.File>)this._db.Files.Where<Questionnaire2.Models.File>((Expression<Func<Questionnaire2.Models.File, bool>>)(x => x.UserId == userId && x.QuestionnaireId == questionnaireId && x.QuestionnaireQCategoryId == record.QCategoryId && x.QCategorySubOrdinal == record.SubOrdinal)).ToList<Questionnaire2.Models.File>();
                VmVerificationItem verificationItem2 = verificationItem1;
                verificationItems.VerificationItems.Add(verificationItem2);
            }
            return (ActionResult)this.View((object)verificationItems);
        }

        [HttpPost]
        public ActionResult Edit(VmVerificationItem item)
        {
            try
            {
                this._db.Entry<Verification>(item.Verification).State = EntityState.Modified;
                this._db.SaveChanges();
                return (ActionResult)this.RedirectToAction("List", (object)new
                {
                    id = item.Verification.UserId
                });
            }
            catch
            {
                return (ActionResult)this.View();
            }
        }

        public ActionResult DownloadFile(int id, int vId)
        {
            Questionnaire2.Models.File file = this._db.Files.First<Questionnaire2.Models.File>((Expression<Func<Questionnaire2.Models.File, bool>>)(p => p.FileId == id));
            byte[] fileBytes = file.FileBytes;
            string str = (string)null;
            this.Response.Clear();
            this.Response.ClearHeaders();
            this.Response.ClearContent();
            this.Response.ContentType = str;
            this.Response.AddHeader("Content-Disposition", string.Format("attachment; filename=" + file.FileName));
            this.Response.BinaryWrite(fileBytes);
            this.Response.End();
            return (ActionResult)this.RedirectToAction("Edit", (object)new { id = vId });
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

        public ActionResult LockUnlock(int id, int questionnaireId, bool editable)
        {
            IQueryable<Verification> queryable = this._db.Verifications.Where<Verification>((Expression<Func<Verification, bool>>)(x => x.UserId == id && x.QuestionnaireId == questionnaireId));
            if (editable)
            {
                foreach (Verification verification in (IEnumerable<Verification>)queryable)
                    verification.Editable = true;
            }
            else
            {
                foreach (Verification verification in (IEnumerable<Verification>)queryable)
                    verification.Editable = false;
            }
            this._db.SaveChanges();
            return (ActionResult)this.RedirectToAction("Index");
        }

        public string GetHtmlPage(string strURL)
        {
            string end;
            using (StreamReader streamReader = new StreamReader(WebRequest.Create(strURL).GetResponse().GetResponseStream()))
            {
                end = streamReader.ReadToEnd();
                streamReader.Close();
            }
            return end;
        }
    }
}


//using System.Collections.ObjectModel;
//using System.Data.Entity;
//using System.Web.Routing;
//using Questionnaire2.Models;
//using Questionnaire2.ViewModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using WebMatrix.WebData;
//using iTextSharp.text.pdf;
//using iTextSharp.text.html.simpleparser;
//using System.IO;
//using iTextSharp.text;
//using System.Net;
//using System.Text;
//using HtmlAgilityPack;
//using System.Net.Http;
//using Newtonsoft.Json;
//using Microsoft.AspNet.Identity;

//namespace Questionnaire2.Controllers
//{
//    public static class GuidConverter
//    {
//        public static int GuidToInt(Guid guid)
//        {
//            return BitConverter.ToInt32(guid.ToByteArray(), 0);
//        }
//    }

//    public class VerificationController : Controller
//    {
//        private readonly Entities _db = new Entities();
//        private readonly ApplicationDbContext _udb = new ApplicationDbContext();

//        [Authorize(Roles = "Admin,Verifier")]
//        public ActionResult Index()
//        {
//            var users = new List<UserInfo>();
//            var userIds = _db.Verifications.Select(x => x.UserId).Distinct().ToList();
//            var userGuids = _udb.Users.Select(x => x.Id).Distinct().ToList();

//            if (userIds != null && userGuids != null)
//            {
//                for (var i = 0; i < userIds.Count(); i++)
//                {
//                    var userInfo = new UserInfo { UserId = userIds[i] };
//                    if (userInfo != null)
//                    {
//                        var verCount = 0;
//                        var unverCount = 0;
//                        verCount = _db.Verifications.Count(x => x.UserId == userInfo.UserId && x.QuestionnaireId == 1 && x.ItemVerified);
//                        unverCount = _db.Verifications.Count(x => x.UserId == userInfo.UserId && x.QuestionnaireId == 1 && x.ItemVerified == false);
//                        userInfo.VerifiedCount = verCount;
//                        userInfo.UnverifiedCount = unverCount;

//                        var userGuid = "";
//                        foreach (var guid in userGuids)
//                        {
//                            var guid_int = BitConverter.ToInt32(new Guid(guid).ToByteArray(), 0);
//                            if (guid_int == userInfo.UserId)
//                                userGuid = guid;
//                        }

//                        userInfo.Editable = !_db.Verifications.Any(
//                            x => x.UserId == userInfo.UserId && x.QuestionnaireId == 1 && x.Editable == false);

//                        if (userGuid != "")
//                        {
//                            var firstOrDefault = _udb.Users.FirstOrDefault(x => x.Id == userGuid);
//                            if (firstOrDefault != null)
//                                userInfo.UserName = firstOrDefault.UserName;
//                            var responses = _db.Responses.Where(x => x.UserId == userInfo.UserId && x.QCategoryName.ToUpper().Contains("PERSONAL"));
                        
//                            userInfo.FirstName = firstOrDefault.FirstName != null ? firstOrDefault.FirstName : "";
//                            userInfo.LastName = firstOrDefault.LastName != null ? firstOrDefault.LastName: "";

//                            users.Add(userInfo);
//                        }
                        
//                    }                  
//                }
//            }

//            var usersVerified = users.Where(x => x.UnverifiedCount == 0);
//            var usersUnverified = users.Where(x => x.UnverifiedCount != 0);

//            var userVerifications = new UserVerifications();
//            if (usersVerified != null)
//            { 
//                userVerifications.UsersVerified = usersVerified.ToList();
//            }
//            if (usersUnverified != null)
//            {
//                userVerifications.UsersUnverified = usersUnverified.ToList();
//            }
//            return View(userVerifications);
//        }

//        public ActionResult Details(int id)
//        {
//            return View();
//        }

//        [Authorize(Roles = "CareProvider")]
//        public ActionResult Verification()
//        {
//            var idBool = new IDandBool();
            
//            var userGuid = new Guid(User.Identity.GetUserId());
//            var userId = BitConverter.ToInt32(userGuid.ToByteArray(), 0);

//            var hasVerifications = _db.Verifications.Any(x => x.UserId == userId);

//            idBool.UserId = userId;
//            idBool.HasVerifications = hasVerifications;
            
//            return View(idBool);
//        }

//        [Authorize(Roles = "CareProvider")]
//        public ActionResult SendToVerification(int id)
//        {
//            var userGuid = new Guid(User.Identity.GetUserId()); //WebSecurity.GetUserId(User.Identity.Name);
//            var userId = BitConverter.ToInt32(userGuid.ToByteArray(), 0);

//            // Delete existing verification items for this user
//            //var userItems = _db.Verifications.Where(x => x.UserId == userId);
//            //var removeRange = _db.Verifications.RemoveRange(userItems);
//            //var saveChanges = _db.SaveChanges();

//            var lockedSections =
//                _db.Verifications.Where(x => x.UserId == userId && x.Editable == false).Select(x => x.QQCategoryId).ToList();

//            var applicationDataToVerify = _db.Responses.Where(x => x.UserId == userId && x.QuestionnaireId == 1 && !lockedSections.Contains(x.QuestionnaireQCategoryId)).ToList();

//            var distinctQCategoryIds = applicationDataToVerify.Select(x => x.QCategoryId).Distinct();

//            foreach (var qCategoryId in distinctQCategoryIds)
//            {
//                var distinctSubOrdinals = applicationDataToVerify.Where(x => x.QCategoryId == qCategoryId).Select(x => x.SubOrdinal).Distinct().ToList();
//                for (var i=0; i < distinctSubOrdinals.Count(); i++)
//                {
//                    var subOrdinal = distinctSubOrdinals[i];
//                    var questionnaireId = applicationDataToVerify[i].QuestionnaireId;

//                    var categoryId = qCategoryId;
//                    var categoryName = _db.QCategories.Single(x => x.QCategoryId == categoryId).QCategoryName;
//                    var qqCategoryId =
//                        applicationDataToVerify.First(x => x.QCategoryId == qCategoryId && x.SubOrdinal == subOrdinal)
//                            .QuestionnaireQCategoryId;

//                    var subOrdinalQuestions = applicationDataToVerify.Where(x => x.QCategoryId == categoryId && x.SubOrdinal == subOrdinal).Select(x => new { x.QuestionText, x.ResponseItem });
//                    var itemInfo = "<b>" + categoryName.ToUpper() + "</b><br />";
//                    itemInfo += subOrdinalQuestions.Aggregate("", (current, item) => current + ("<i>" + item.QuestionText + ":</i> " + item.ResponseItem + "<br />"));

//                    if (_db.Verifications.Any(x => x.UserId == userId && x.QQCategoryId == qqCategoryId))
//                    {
//                        //update
//                        var verification = _db.Verifications.Single(x => x.UserId == userId && x.QQCategoryId == qqCategoryId);
//                        verification.ItemInfo = itemInfo;
//                        verification.Editable = false;
//                        _db.Entry(verification).State = (EntityState)System.Data.Entity.EntityState.Modified;
//                    }
//                    else
//                    {                      
//                        // make new
//                        var verifyQCategory = new Verification
//                        {
//                            QuestionnaireId = questionnaireId,
//                            UserId = userId,
//                            QCategoryId = qCategoryId,
//                            QQCategoryId = qqCategoryId,
//                            SubOrdinal = subOrdinal,
//                            ItemInfo = itemInfo,
//                            ItemVerified = false,
//                            ItemStepLevel = "",
//                            Editable = false
//                        };
//                        _db.Verifications.Add(verifyQCategory);
//                    }                                   
//                }
//            }
//            _db.SaveChanges();
//            return View();
//        }

//        public ActionResult List(int id, int questionnaireId = 1)
//        {            
//            var vmVerificationItems = new VmVerificationItems() {VerificationItems = new Collection<VmVerificationItem>()};

//            var userVerificationRecords = _db.Verifications.Where(x => x.UserId == id && x.QuestionnaireId == questionnaireId).ToList();

//            var levelCheck = _db.UserLevels.Any(x => x.UserId == id);

//            if (_db.UserLevels.Any(x => x.UserId == id) == false)
//            {
//                var uL = new UserLevel { UserId = id, FinalStepLevel = "none", FinalStepLevelDate = DateTime.Today };
//                _db.UserLevels.Add(uL);
//                _db.SaveChanges();
//            }

//            var userLevel = _db.UserLevels.Where(x => x.UserId == id).SingleOrDefault();
//            vmVerificationItems.UserLevel = userLevel;
            

//            var latticeItems = _db.LatticeItems.ToList();
//            var selectListItems = latticeItems.Select(latticeItem => new SelectListItem
//            {
//                Text = latticeItem.DropdownText, Value = latticeItem.DropdownText
//            }).ToList();
//            vmVerificationItems.LatticeItems = selectListItems;

//            foreach (var userVerificationRecord in userVerificationRecords)
//            {
//                var record = userVerificationRecord;
//                var vmVerificationItem = new VmVerificationItem
//                {
//                    Verification = record,
//                    Files = _db.Files.Where(
//                        x =>
//                            x.UserId == id && x.QuestionnaireId == questionnaireId &&
//                            x.QuestionnaireQCategoryId == record.QCategoryId &&
//                            x.QCategorySubOrdinal == record.SubOrdinal).ToList()
//                };
                
//                vmVerificationItems.VerificationItems.Add(vmVerificationItem);
//            }       

//            return View(vmVerificationItems);
//        }

//        [HttpPost]
//        [ValidateInput(false)]
//        public ActionResult List(VmVerificationItem item)
//        {
//            try
//            {
//                // TODO: Add update logic here
//                _db.Entry(item.Verification).State = (EntityState)System.Data.Entity.EntityState.Modified;
//                _db.SaveChanges();
//                return RedirectToAction("List", new { id = item.Verification.UserId });
//            }
//            catch
//            {
//                return View();
//            }
//        }

//        [HttpPost]
//        public ActionResult UpdateLevel(FormCollection collection, string stepLevel, int Id)
//        {
//            UserLevel userLevel = _db.UserLevels.Where(x => x.Id == Id).SingleOrDefault();
//            userLevel.FinalStepLevel = stepLevel;
//            userLevel.FinalStepLevelDate = DateTime.Now;
//            _db.Entry(userLevel).State = EntityState.Modified;
//            _db.SaveChanges();
//            return RedirectToAction("List", new { id = userLevel.UserId });
//        }

//        public ActionResult Edit(int id)
//        {
//            var vmVerificationItems = new VmVerificationItems { VerificationItems = new Collection<VmVerificationItem>() };

//            var userVerificationRecords = _db.Verifications.Where(x => x.Id == id).ToList();

//            var questionnaireId = userVerificationRecords.First().QuestionnaireId;

//            var userGuid = new Guid(User.Identity.GetUserId()); //WebSecurity.GetUserId(User.Identity.Name);
//            var userId = BitConverter.ToInt32(userGuid.ToByteArray(), 0);

//            var latticeItems = _db.LatticeItems.ToList();
//            var selectListItems = new List<SelectListItem>();
//            foreach (var latticeItem in latticeItems)
//            {
//                var selectListItem = new SelectListItem
//                {
//                    Text = latticeItem.DropdownText,
//                    Value = latticeItem.DropdownText
//                };
//                selectListItems.Add(selectListItem);
//            }
//            vmVerificationItems.LatticeItems = selectListItems;

//            foreach (var userVerificationRecord in userVerificationRecords)
//            {
//                var record = userVerificationRecord;
//                var vmVerificationItem = new VmVerificationItem
//                {
//                    Verification = record,
//                    Files = _db.Files.Where(
//                        x =>
//                            x.UserId == userId && x.QuestionnaireId == questionnaireId &&
//                            x.QuestionnaireQCategoryId == record.QCategoryId &&
//                            x.QCategorySubOrdinal == record.SubOrdinal).ToList()
//                };

//                vmVerificationItems.VerificationItems.Add(vmVerificationItem);
//            }

//            return View(vmVerificationItems);
//        }

//        [HttpPost]
//        public ActionResult Edit(VmVerificationItem item)
//        {
//            try
//            {
//                // TODO: Add update logic here
//                _db.Entry(item.Verification).State = (EntityState) System.Data.Entity.EntityState.Modified;
//                _db.SaveChanges();
//                return RedirectToAction("List", new { id = item.Verification.UserId });
//            }
//            catch
//            {
//                return View();
//            }
//        }

//        public ActionResult DownloadFile(int id, int vId)
//        {
//            var userGuid = new Guid(User.Identity.GetUserId()); //WebSecurity.GetUserId(User.Identity.Name);
//            var userId = BitConverter.ToInt32(userGuid.ToByteArray(), 0);
            
//            var fileRecord = _db.Files.First(p => p.FileId == id & p.UserId == userId);
//            byte[] fileData = fileRecord.FileBytes;

//            String mimeType = null;

//            Response.Clear();
//            Response.ClearHeaders();
//            Response.ClearContent();
//            Response.ContentType = mimeType;
//            Response.AddHeader("Content-Disposition", string.Format("attachment; filename=" + fileRecord.FileName));
//            Response.BinaryWrite(fileData);
//            Response.End();
//            return RedirectToAction("Edit", new { id = vId });
//        }

//        public ActionResult Delete(int id)
//        {
//            return View();
//        }

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

//        public ActionResult LockUnlock(int id, int questionnaireId, bool editable)
//        {
//            var verificationItems = _db.Verifications.Where(x => x.UserId == id && x.QuestionnaireId == questionnaireId);
//            if (editable == true)
//            {
//                foreach (var verificationItem in verificationItems)
//                {
//                    verificationItem.Editable = true;
//                }
//            }
//            else
//            {
//                foreach (var verificationItem in verificationItems)
//                {
//                    verificationItem.Editable = false;
//                }
//            }
//            _db.SaveChanges();
//            return RedirectToAction("Index");
//        }

//        //public void tableToPdf(object sender, EventArgs e, string pageHtml)
//        //{
//        //    //var table = document.GetElementbyId("verificationsTable");
            
//        //    //Set page size as A4
//        //    Document pdfDoc = new Document(PageSize.A4, 20, 10, 10, 10);

//        //    try
//        //    {
//        //        var ms = new MemoryStream();
//        //        PdfWriter.GetInstance(pdfDoc, ms);

//        //        //Open PDF Document to write data
//        //        pdfDoc.Open();

//        //        ////Assign Html content in a string to write in PDF
//        //        //string contents = "";

//        //        //StreamReader sr;
//        //        //try
//        //        //{
//        //        //    //Read file from server path
//        //        //    sr = System.IO.File.OpenText(Server.MapPath("~/sample.html"));
//        //        //    //store content in the variable
//        //        //    contents = sr.ReadToEnd();
//        //        //    sr.Close();
//        //        //}
//        //        //catch (Exception ex)
//        //        //{

//        //        //}

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

//        public string GetHtmlPage(string strURL)
//        {
//            String strResult;
//            WebRequest objRequest = WebRequest.Create(strURL);
//            WebResponse objResponse = objRequest.GetResponse();
//            using (var sr = new StreamReader(objResponse.GetResponseStream()))
//            {
//                strResult = sr.ReadToEnd();
//                sr.Close();
//            }
//            return strResult;
//        }     
//    }
//}

﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Questionnaire2.Models;
using Questionnaire2.ViewModels;
using WebMatrix.WebData;
using Microsoft.AspNet.Identity;

namespace Questionnaire2.Controllers
{
    [Authorize(Roles = "Administrator, CareProvider")]
    public class FileController : Controller
    {
        private readonly Entities _db = new Entities();

        public ActionResult Index()
        {
            var userGuid = new Guid(User.Identity.GetUserId()); //WebSecurity.GetUserId(User.Identity.Name);
            var userId = BitConverter.ToInt32(userGuid.ToByteArray(), 0);
            
            var model = new File2DB {UserId = userId, QuestionnaireId = 1};
            return View(model);
        }

        public ActionResult Details(int id = 0)
        {
            File file = _db.Files.Find(id);
            if (file == null)
            {
                return HttpNotFound();
            }
            return View(file);
        }

        public ActionResult Upload(int questionnaireQCategoryId = 1, int qCategorySubOrdinal = 0, int questionnaireId = 1, string qCategoryName = "General")
        {
            var userGuid = new Guid(User.Identity.GetUserId()); //WebSecurity.GetUserId(User.Identity.Name);
            var userId = BitConverter.ToInt32(userGuid.ToByteArray(), 0);

            var userFiles = _db.Files.Where(a => a.UserId == userId);

            var model = new File2DB
            {
                UserId = userId,
                QuestionnaireId = questionnaireId,
                QuestionnaireQCategoryId = questionnaireQCategoryId,
                QCategorySubOrdinal = qCategorySubOrdinal,
                QCategoryName = qCategoryName,
                UserFiles = userFiles.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(File2DB model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var uploadedFile = new byte[model.File.InputStream.Length];
            model.File.InputStream.Read(uploadedFile, 0, uploadedFile.Length);
            _db.Files.Add(new File() { UserId = model.UserId,
                                      QuestionnaireId = model.QuestionnaireId,
                                      QuestionnaireQCategoryId = model.QuestionnaireQCategoryId,
                                      QCategoryName = model.QCategoryName,
                                      FileName = model.File.FileName,
                                      Description = model.Description,
                                      FileBytes = uploadedFile });
            _db.SaveChanges();
            TempData["activePanel"] = Request.Form["activePanel"];
            return RedirectToAction("Upload", new { QuestionnaireId = model.QuestionnaireId, QuestionnaireQCategoryId = model.QuestionnaireQCategoryId, QCategoryName = model.QCategoryName });
        }

        public ActionResult Edit(int id = 0)
        {
            File file = _db.Files.Find(id);
            if (file == null)
            {
                return HttpNotFound();
            }
            return View(file);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(File file)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(file).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(file);
        }

        public ActionResult DownloadDelete(string fileName)
        {
            if (fileName.Substring(fileName.Length - 7, 7) == "_delete")
            {
                var userGuid = new Guid(User.Identity.GetUserId()); //WebSecurity.GetUserId(User.Identity.Name);
                var userId = BitConverter.ToInt32(userGuid.ToByteArray(), 0);
                
                var delName = fileName.Substring(0, fileName.Length - 7);
                var fileD = _db.Files.First(a => a.FileName == delName & a.UserId == userId);
                _db.Files.Remove(fileD);
                _db.SaveChanges();
            }
            else
            {
                var userGuid = new Guid(User.Identity.GetUserId()); //WebSecurity.GetUserId(User.Identity.Name);
                var userId = BitConverter.ToInt32(userGuid.ToByteArray(), 0);
                
                var fileRecord = _db.Files.First(p => p.FileName == fileName & p.UserId == userId);
                byte[] fileData = fileRecord.FileBytes;

                String mimeType = null;

                Response.Clear();
                Response.ClearHeaders();
                Response.ClearContent();
                Response.ContentType = mimeType;
                Response.AddHeader("Content-Disposition", string.Format("attachment; filename=" + fileRecord.FileName));
                Response.BinaryWrite(fileData);
                Response.End();
            }

            return RedirectToAction("Upload");
        }

        public ActionResult MakeWordFile()
        {
            Questionnaire2.Helpers.MakeWordFile.CreateSampleDocument();
            String mimeType = null;

            Response.Clear();
            Response.ClearHeaders();
            Response.ClearContent();
            Response.ContentType = mimeType;
            Response.AddHeader("Content-Disposition", string.Format("attachment; filename=DocXExample.docx"));
            Response.WriteFile(Helpers.Navigation.GetRoot() + "DocXExample.docx");
            Response.End();

            return View();
        }

        public ActionResult WordFile2Pdf()
        {
            Questionnaire2.Helpers.Word2Pdf.ConvertWord2Pdf();
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
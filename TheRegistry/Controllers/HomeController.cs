using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Questionnaire2.Controllers
{
    public class HomeController : Controller
    {
        private readonly Entities _db = new Questionnaire2.Entities();

        public ActionResult Index()
        {
            var appSettings = _db.AppSettings.ToList();
            if (appSettings.All(x => x.AppSettingName != "Title Message"))
                _db.AppSettings.Add(new Models.AppSetting { AppSettingName = "Title Message", AppSettingValue = "Title Message" });
            if (appSettings.All(x => x.AppSettingName != "Application Description"))
                _db.AppSettings.Add(new Models.AppSetting { AppSettingName = "Application Description", AppSettingValue = "Application Description" });
            _db.SaveChanges();
            return View(appSettings);
        }

        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            var appSettings = _db.AppSettings.ToList();
            if (appSettings.All(x => x.AppSettingName != "Contact Info"))
                _db.AppSettings.Add(new Models.AppSetting { AppSettingName = "Contact Info", AppSettingValue = "Contact Info" });
            _db.SaveChanges();
            return View(appSettings);
        }
    }
}

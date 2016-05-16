using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Questionnaire2.Controllers
{
    public class InfoController : Controller
    {
        private readonly Entities _db = new Questionnaire2.Entities();

        public ActionResult Index()
        {
            var appSettings = _db.AppSettings.ToList();
            if (appSettings.All(x => x.AppSettingName != "Useful Links"))
                _db.AppSettings.Add(new Models.AppSetting { AppSettingName = "Useful Links", AppSettingValue = "Useful Links" });
            if (appSettings.All(x => x.AppSettingName != "Useful Documents"))
                _db.AppSettings.Add(new Models.AppSetting { AppSettingName = "Useful Documents", AppSettingValue = "Useful Documents" });
            if (appSettings.All(x => x.AppSettingName != "Notices"))
                _db.AppSettings.Add(new Models.AppSetting { AppSettingName = "Notices", AppSettingValue = "Notices" });
            _db.SaveChanges();
            return View(appSettings);
        }
    }
}
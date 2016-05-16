using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Questionnaire2.Models
{
    public class AppSettingMetaData
    {
        [Display(Name = "Setting Name")]
        public string AppSettingName { get; set; }

        [UIHint("tinymce_jquery_full")]
        [AllowHtml]
        [Display(Name = "Setting Value")]
        public string AppSettingValue { get; set; }
    }
}
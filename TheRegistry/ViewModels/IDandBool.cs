using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Questionnaire2.Models;

namespace Questionnaire2.ViewModels
{
    public class IDandBool
    {
        public int UserId { get; set; }
        public bool HasVerifications { get; set; }
    }
}
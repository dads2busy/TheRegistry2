//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Questionnaire2.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class File
    {
        public int FileId { get; set; }
        public int UserId { get; set; }
        public int QuestionnaireId { get; set; }
        public int QuestionnaireQCategoryId { get; set; }
        public string QCategoryName { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public byte[] FileBytes { get; set; }
        public int QCategorySubOrdinal { get; set; }
    }
}

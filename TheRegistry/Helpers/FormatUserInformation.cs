using Microsoft.AspNet.Identity;
using Novacode;
using Questionnaire2.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Questionnaire2.Helpers
{
    public class FormatUserInformation
    {
        private readonly ApplicationDbContext _udb = new ApplicationDbContext();
        private readonly Formatting _title = new Formatting()
        {
            FontFamily = new FontFamily("Arial Black"),
            Size = new double?(16.0),
            Bold = true
        };
        private readonly Formatting _subtitle = new Formatting()
        {
            FontFamily = new FontFamily("Calibri"),
            Size = new double?(12.0),
            Bold = true
        };
        private readonly Formatting _normal = new Formatting()
        {
            FontFamily = new FontFamily("Calibri"),
            Size = new double?(10.0),
            Bold = false
        };
        private const Alignment Center = Alignment.center;
        private const Alignment Left = Alignment.left;

        private List<Respons> Items { get; set; }

        private List<string> CategoryNames { get; set; }

        public FormatUserInformation(List<Respons> items, List<string> categoryNames)
        {
            this.Items = items;
            this.CategoryNames = categoryNames;
        }

        public List<FormatUserInformation.Paragraph> Format()
        {
            List<FormatUserInformation.Paragraph> first = new List<FormatUserInformation.Paragraph>();
            foreach (string categoryName in this.CategoryNames)
            {
                string catName = categoryName;
                List<Respons> list = this.Items.Where<Respons>((Func<Respons, bool>)(x => x.QCategoryName.Contains(catName))).ToList<Respons>();
                switch (catName)
                {
                    case "Background Information":
                        first = first.Concat<FormatUserInformation.Paragraph>((IEnumerable<FormatUserInformation.Paragraph>)this.PersonalInformation(list)).ToList<FormatUserInformation.Paragraph>();
                        continue;
                    case "Employment":
                        first = first.Concat<FormatUserInformation.Paragraph>((IEnumerable<FormatUserInformation.Paragraph>)this.EmploymentInformation(list)).ToList<FormatUserInformation.Paragraph>();
                        continue;
                    case "Certifications":
                        first = first.Concat<FormatUserInformation.Paragraph>((IEnumerable<FormatUserInformation.Paragraph>)this.CertificationInformation(list)).ToList<FormatUserInformation.Paragraph>();
                        continue;
                    case "Licenses":
                        first = first.Concat<FormatUserInformation.Paragraph>((IEnumerable<FormatUserInformation.Paragraph>)this.LicenseInformation(list)).ToList<FormatUserInformation.Paragraph>();
                        continue;
                    case "Education":
                        first = first.Concat<FormatUserInformation.Paragraph>((IEnumerable<FormatUserInformation.Paragraph>)this.EducationInformation(list)).ToList<FormatUserInformation.Paragraph>();
                        continue;
                    case "Credentials":
                        first = first.Concat<FormatUserInformation.Paragraph>((IEnumerable<FormatUserInformation.Paragraph>)this.CredentialsInformation(list)).ToList<FormatUserInformation.Paragraph>();
                        continue;
                    case "Coursework":
                        first = first.Concat<FormatUserInformation.Paragraph>((IEnumerable<FormatUserInformation.Paragraph>)this.OtherCourseworkInformation(list)).ToList<FormatUserInformation.Paragraph>();
                        continue;
                    case "Training":
                        first = first.Concat<FormatUserInformation.Paragraph>((IEnumerable<FormatUserInformation.Paragraph>)this.ProfessionalTrainingInformation(list)).ToList<FormatUserInformation.Paragraph>();
                        continue;
                    default:
                        return (List<FormatUserInformation.Paragraph>)null;
                }
            }
            return first;
        }

        public List<FormatUserInformation.Paragraph> PersonalInformation(List<Respons> pi)
        {
            string userId = HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser applicationUser = this._udb.Users.FirstOrDefault<ApplicationUser>((Expression<Func<ApplicationUser, bool>>)(x => x.Id == userId));
            List<FormatUserInformation.Paragraph> paragraphList = new List<FormatUserInformation.Paragraph>();
            if (!pi.Any<Respons>())
                return paragraphList;
            List<Respons> responsList = new List<Respons>()
      {
        new Respons()
        {
          ResponseItem = "Not Found"
        }
      };
            string firstName = applicationUser.FirstName;
            string middleInitial = applicationUser.MiddleInitial;
            string lastName = applicationUser.LastName;
            paragraphList.Add(new FormatUserInformation.Paragraph()
            {
                Text = firstName + " " + middleInitial + " " + lastName,
                Format = this._title,
                Align = Alignment.center
            });
            string email = applicationUser.Email;
            paragraphList.Add(new FormatUserInformation.Paragraph()
            {
                Text = email,
                Format = this._subtitle,
                Align = Alignment.center
            });
            string phoneNumber = applicationUser.PhoneNumber;
            paragraphList.Add(new FormatUserInformation.Paragraph()
            {
                Text = phoneNumber,
                Format = this._subtitle,
                Align = Alignment.center
            });
            return paragraphList;
        }

        protected List<FormatUserInformation.Paragraph> EmploymentInformation(List<Respons> pi)
        {
            List<FormatUserInformation.Paragraph> paragraphList = new List<FormatUserInformation.Paragraph>();
            if (!pi.Any<Respons>())
                return paragraphList;
            List<Respons> responsList = new List<Respons>()
      {
        new Respons()
        {
          ResponseItem = "Not Found"
        }
      };
            int num = pi.Select<Respons, int>((Func<Respons, int>)(x => x.SubOrdinal)).Max();
            paragraphList.Add(new FormatUserInformation.Paragraph()
            {
                Text = "Employment History",
                Format = this._title,
                Align = Alignment.center
            });
            for (int i = 0; i < num + 1; ++i)
            {
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = " ",
                    Format = this._normal,
                    Align = Alignment.left
                });
                int i1 = i;
                IEnumerable<Respons> source1 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText == "Employer Name")
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText == "Employer Name")
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                string str1 = (source1 as IList<Respons> ?? (IList<Respons>)source1.ToList<Respons>()).Where<Respons>((Func<Respons, bool>)(x => x.ResponseItem != "")).First<Respons>().ResponseItem ?? "Employer Name";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str1,
                    Format = this._subtitle,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source2 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText == "Employer Address 1")
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText == "Employer Address 1")
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                string str2 = (source2 as IList<Respons> ?? (IList<Respons>)source2.ToList<Respons>()).FirstOrDefault<Respons>().ResponseItem ?? "Employer Address 1";
                IEnumerable<Respons> source3 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText == "Employer City" && x.SubOrdinal == i1)
                        return x.QCategoryName.Contains("Employment");
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText == "Employer City")
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source4 = source3 as IList<Respons> ?? (IList<Respons>)source3.ToList<Respons>();
                string str3 = str2 + (source4.FirstOrDefault<Respons>().ResponseItem != null ? ", " + source4.FirstOrDefault<Respons>().ResponseItem : ", Employer City");
                IEnumerable<Respons> source5 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText == "Employer State" && x.SubOrdinal == i1)
                        return x.QCategoryName.Contains("Employment");
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText == "Employer State")
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source6 = source5 as IList<Respons> ?? (IList<Respons>)source5.ToList<Respons>();
                string str4 = str3 + (source6.FirstOrDefault<Respons>().ResponseItem != null ? ", " + source6.FirstOrDefault<Respons>().ResponseItem : ", Employer State");
                IEnumerable<Respons> source7 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText == "Employer Zip Code" && x.SubOrdinal == i1)
                        return x.QCategoryName.Contains("Employment");
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText == "Employer Zip Code")
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source8 = source7 as IList<Respons> ?? (IList<Respons>)source7.ToList<Respons>();
                string str5 = str4 + (source8.FirstOrDefault<Respons>().ResponseItem != null ? " " + source8.FirstOrDefault<Respons>().ResponseItem : " Employer Zip Code");
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str5,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source9 = pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText == "Position" && x.SubOrdinal == i1)
                        return x.QCategoryName.Contains("Employment");
                    return false;
                }));
                string str6 = (source9 as IList<Respons> ?? (IList<Respons>)source9.ToList<Respons>()).FirstOrDefault<Respons>().ResponseItem ?? "Position Title";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str6,
                    Format = this._subtitle,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source10 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText == "Position Description")
                        return x.SubOrdinal == i;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText == "Position Description")
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                string str7 = (source10 as IList<Respons> ?? (IList<Respons>)source10.ToList<Respons>()).FirstOrDefault<Respons>().ResponseItem ?? "Position Description";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str7,
                    Format = this._normal,
                    Align = Alignment.left
                });
            }
            return paragraphList;
        }

        protected List<FormatUserInformation.Paragraph> CredentialsInformation(List<Respons> pi)
        {
            List<FormatUserInformation.Paragraph> paragraphList = new List<FormatUserInformation.Paragraph>();
            if (!pi.Any<Respons>())
                return paragraphList;
            List<Respons> responsList = new List<Respons>()
      {
        new Respons()
        {
          ResponseItem = "Not Found"
        }
      };
            int num = pi.Select<Respons, int>((Func<Respons, int>)(x => x.SubOrdinal)).Max();
            paragraphList.Add(new FormatUserInformation.Paragraph()
            {
                Text = "Professional Credentials",
                Format = this._title,
                Align = Alignment.center
            });
            for (int index = 0; index < num + 1; ++index)
            {
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = " ",
                    Format = this._normal,
                    Align = Alignment.left
                });
                int i1 = index;
                IEnumerable<Respons> source1 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("credential") && x.QuestionText.ToLower().Contains("name"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("credential") && x.QuestionText.ToLower().Contains("name"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source2 = source1 as IList<Respons> ?? (IList<Respons>)source1.ToList<Respons>();
                string str1 = source2.FirstOrDefault<Respons>().ResponseItem != null ? "Credential Name: " + source2.FirstOrDefault<Respons>().ResponseItem : "Credential Name";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str1,
                    Format = this._subtitle,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source3 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("credential") && x.QuestionText.ToLower().Contains("type"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("credential") && x.QuestionText.ToLower().Contains("type"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source4 = source3 as IList<Respons> ?? (IList<Respons>)source3.ToList<Respons>();
                string str2 = source4.FirstOrDefault<Respons>().ResponseItem != null ? "Credential Type: " + source4.FirstOrDefault<Respons>().ResponseItem : "Credential Type";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str2,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source5 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("awarding") && x.QuestionText.ToLower().Contains("institution"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("awarding") && x.QuestionText.ToLower().Contains("institution"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source6 = source5 as IList<Respons> ?? (IList<Respons>)source5.ToList<Respons>();
                string str3 = source6.FirstOrDefault<Respons>().ResponseItem != null ? "Awarding Institution: " + source6.FirstOrDefault<Respons>().ResponseItem : "Name of Awarding Institution";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str3,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source7 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("date") && x.QuestionText.ToLower().Contains("awarded"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("date") && x.QuestionText.ToLower().Contains("awarded"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source8 = source7 as IList<Respons> ?? (IList<Respons>)source7.ToList<Respons>();
                if (source8.Any<Respons>())
                {
                    string str4 = source8.FirstOrDefault<Respons>().ResponseItem != null ? "Date Awarded: " + source8.FirstOrDefault<Respons>().ResponseItem : "Date Awarded";
                    paragraphList.Add(new FormatUserInformation.Paragraph()
                    {
                        Text = str4,
                        Format = this._normal,
                        Align = Alignment.left
                    });
                }
                IEnumerable<Respons> source9 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("endorsement") && x.QuestionText.ToLower().Contains("type"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("endorsement") && x.QuestionText.ToLower().Contains("type"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source10 = source9 as IList<Respons> ?? (IList<Respons>)source9.ToList<Respons>();
                if (source10.Any<Respons>())
                {
                    string str4 = source10.FirstOrDefault<Respons>().ResponseItem != null ? "Endorsement Type: " + source10.FirstOrDefault<Respons>().ResponseItem : "Endorsement Type";
                    paragraphList.Add(new FormatUserInformation.Paragraph()
                    {
                        Text = str4,
                        Format = this._normal,
                        Align = Alignment.left
                    });
                }
                IEnumerable<Respons> source11 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("license") && x.QuestionText.ToLower().Contains("number"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("license") && x.QuestionText.ToLower().Contains("number"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source12 = source11 as IList<Respons> ?? (IList<Respons>)source11.ToList<Respons>();
                if (source12.Any<Respons>())
                {
                    string str4 = source12.FirstOrDefault<Respons>().ResponseItem != null ? "License Number: " + source12.FirstOrDefault<Respons>().ResponseItem : "License Number";
                    paragraphList.Add(new FormatUserInformation.Paragraph()
                    {
                        Text = str4,
                        Format = this._normal,
                        Align = Alignment.left
                    });
                }
            }
            return paragraphList;
        }

        protected List<FormatUserInformation.Paragraph> CertificationInformation(List<Respons> pi)
        {
            return new List<FormatUserInformation.Paragraph>();
        }

        protected List<FormatUserInformation.Paragraph> LicenseInformation(List<Respons> pi)
        {
            return new List<FormatUserInformation.Paragraph>();
        }

        protected List<FormatUserInformation.Paragraph> EducationInformation(List<Respons> pi)
        {
            List<FormatUserInformation.Paragraph> paragraphList = new List<FormatUserInformation.Paragraph>();
            if (!pi.Any<Respons>())
                return paragraphList;
            List<Respons> responsList = new List<Respons>()
      {
        new Respons()
        {
          ResponseItem = "Not Found"
        }
      };
            int num = pi.Select<Respons, int>((Func<Respons, int>)(x => x.SubOrdinal)).Max();
            paragraphList.Add(new FormatUserInformation.Paragraph()
            {
                Text = "Education Degree(s) Completed",
                Format = this._title,
                Align = Alignment.center
            });
            for (int index = 0; index < num + 1; ++index)
            {
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = " ",
                    Format = this._normal,
                    Align = Alignment.left
                });
                int i1 = index;
                IEnumerable<Respons> source1 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("degree") && x.QuestionText.ToLower().Contains("type"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("degree") && x.QuestionText.ToLower().Contains("type"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source2 = source1 as IList<Respons> ?? (IList<Respons>)source1.ToList<Respons>();
                string str1 = source2.FirstOrDefault<Respons>().ResponseItem != null ? "Degree Type: " + source2.FirstOrDefault<Respons>().ResponseItem : "Degree Type";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str1,
                    Format = this._subtitle,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source3 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("college") && x.QuestionText.ToLower().Contains("name"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("college") && x.QuestionText.ToLower().Contains("name"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source4 = source3 as IList<Respons> ?? (IList<Respons>)source3.ToList<Respons>();
                string str2 = source4.FirstOrDefault<Respons>().ResponseItem != null ? "Awarding Institution: " + source4.FirstOrDefault<Respons>().ResponseItem : "Name of Awarding Institution";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str2,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source5 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("date") && x.QuestionText.ToLower().Contains("degree"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("date") && x.QuestionText.ToLower().Contains("degree"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source6 = source5 as IList<Respons> ?? (IList<Respons>)source5.ToList<Respons>();
                if (source6.Any<Respons>())
                {
                    string str3 = source6.FirstOrDefault<Respons>().ResponseItem != null ? "Date Awarded: " + source6.FirstOrDefault<Respons>().ResponseItem : "Date Awarded";
                    paragraphList.Add(new FormatUserInformation.Paragraph()
                    {
                        Text = str3,
                        Format = this._normal,
                        Align = Alignment.left
                    });
                }
                IEnumerable<Respons> source7 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("major"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("major"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source8 = source7 as IList<Respons> ?? (IList<Respons>)source7.ToList<Respons>();
                string str4 = source8.FirstOrDefault<Respons>().ResponseItem != null ? "Major Area: " + source8.FirstOrDefault<Respons>().ResponseItem : "Major Area";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str4,
                    Format = this._normal,
                    Align = Alignment.left
                });
            }
            return paragraphList;
        }

        protected List<FormatUserInformation.Paragraph> CredentialEndorsementInformation(List<Respons> pi)
        {
            return new List<FormatUserInformation.Paragraph>();
        }

        protected List<FormatUserInformation.Paragraph> OtherCourseworkInformation(List<Respons> pi)
        {
            List<FormatUserInformation.Paragraph> paragraphList = new List<FormatUserInformation.Paragraph>();
            if (!pi.Any<Respons>())
                return paragraphList;
            List<Respons> responsList = new List<Respons>()
      {
        new Respons()
        {
          ResponseItem = "Not Found"
        }
      };
            int num = pi.Select<Respons, int>((Func<Respons, int>)(x => x.SubOrdinal)).Max();
            paragraphList.Add(new FormatUserInformation.Paragraph()
            {
                Text = "Other College Coursework Completed",
                Format = this._title,
                Align = Alignment.center
            });
            for (int index = 0; index < num + 1; ++index)
            {
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = " ",
                    Format = this._normal,
                    Align = Alignment.left
                });
                int i1 = index;
                IEnumerable<Respons> source1 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("course") && x.QuestionText.ToLower().Contains("name"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("course") && x.QuestionText.ToLower().Contains("name"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source2 = source1 as IList<Respons> ?? (IList<Respons>)source1.ToList<Respons>();
                string str1 = source2.FirstOrDefault<Respons>().ResponseItem != null ? "Name of Course: " + source2.FirstOrDefault<Respons>().ResponseItem : "Name of Course";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str1,
                    Format = this._subtitle,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source3 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("course") && x.QuestionText.ToLower().Contains("number"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("course") && x.QuestionText.ToLower().Contains("number"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source4 = source3 as IList<Respons> ?? (IList<Respons>)source3.ToList<Respons>();
                string str2 = source4.FirstOrDefault<Respons>().ResponseItem != null ? "Course Number: " + source4.FirstOrDefault<Respons>().ResponseItem : "Course Number";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str2,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source5 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("college") && x.QuestionText.ToLower().Contains("name"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("college") && x.QuestionText.ToLower().Contains("name"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source6 = source5 as IList<Respons> ?? (IList<Respons>)source5.ToList<Respons>();
                string str3 = source6.FirstOrDefault<Respons>().ResponseItem != null ? "Name of Awarding Institution: " + source6.FirstOrDefault<Respons>().ResponseItem : "Name of Awarding Institution";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str3,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source7 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("hours") && x.QuestionText.ToLower().Contains("earned"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("hours") && x.QuestionText.ToLower().Contains("earned"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source8 = source7 as IList<Respons> ?? (IList<Respons>)source7.ToList<Respons>();
                string str4 = source8.FirstOrDefault<Respons>().ResponseItem != null ? "No. of Credit Hours: " + source8.FirstOrDefault<Respons>().ResponseItem : "No. of Credit Hours, if applicable";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str4,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source9 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("hours") && x.QuestionText.ToLower().Contains("type"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("hours") && x.QuestionText.ToLower().Contains("type"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source10 = source9 as IList<Respons> ?? (IList<Respons>)source9.ToList<Respons>();
                string str5 = source10.FirstOrDefault<Respons>().ResponseItem != null ? "Type of Hours: " + source10.FirstOrDefault<Respons>().ResponseItem : "Type of Hours";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str5,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source11 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("date") && x.QuestionText.ToLower().Contains("completed"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("date") && x.QuestionText.ToLower().Contains("completed"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source12 = source11 as IList<Respons> ?? (IList<Respons>)source11.ToList<Respons>();
                if (source12.Any<Respons>())
                {
                    string str6 = source12.FirstOrDefault<Respons>().ResponseItem != null ? "Date Completed: " + source12.FirstOrDefault<Respons>().ResponseItem : "Date Completed";
                    paragraphList.Add(new FormatUserInformation.Paragraph()
                    {
                        Text = str6,
                        Format = this._normal,
                        Align = Alignment.left
                    });
                }
            }
            return paragraphList;
        }

        protected List<FormatUserInformation.Paragraph> ProfessionalTrainingInformation(List<Respons> pi)
        {
            List<FormatUserInformation.Paragraph> paragraphList = new List<FormatUserInformation.Paragraph>();
            if (!pi.Any<Respons>())
                return paragraphList;
            List<Respons> responsList = new List<Respons>()
      {
        new Respons()
        {
          ResponseItem = "Not Found"
        }
      };
            int num = pi.Select<Respons, int>((Func<Respons, int>)(x => x.SubOrdinal)).Max();
            paragraphList.Add(new FormatUserInformation.Paragraph()
            {
                Text = "Professional Training",
                Format = this._title,
                Align = Alignment.center
            });
            for (int index = 0; index < num + 1; ++index)
            {
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = " ",
                    Format = this._normal,
                    Align = Alignment.left
                });
                int i1 = index;
                IEnumerable<Respons> source1 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("training") && x.QuestionText.ToLower().Contains("title"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("training") && x.QuestionText.ToLower().Contains("title"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source2 = source1 as IList<Respons> ?? (IList<Respons>)source1.ToList<Respons>();
                string str1 = source2.FirstOrDefault<Respons>().ResponseItem != null ? "Training Title: " + source2.FirstOrDefault<Respons>().ResponseItem : "Training Title";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str1,
                    Format = this._subtitle,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source3 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("training") && x.QuestionText.ToLower().Contains("type"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("training") && x.QuestionText.ToLower().Contains("type"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source4 = source3 as IList<Respons> ?? (IList<Respons>)source3.ToList<Respons>();
                string str2 = source4.FirstOrDefault<Respons>().ResponseItem != null ? "Training Type: " + source4.FirstOrDefault<Respons>().ResponseItem : "Training Type";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str2,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source5 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("competency") && x.QuestionText.ToLower().Contains("area"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("competency") && x.QuestionText.ToLower().Contains("area"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source6 = source5 as IList<Respons> ?? (IList<Respons>)source5.ToList<Respons>();
                string str3 = source6.FirstOrDefault<Respons>().ResponseItem != null ? "Core Competency Areas: " + source6.FirstOrDefault<Respons>().ResponseItem : "Core Competency Areas";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str3,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source7 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("date") && x.QuestionText.ToLower().Contains("completed"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("date") && x.QuestionText.ToLower().Contains("completed"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source8 = source7 as IList<Respons> ?? (IList<Respons>)source7.ToList<Respons>();
                string str4 = source8.FirstOrDefault<Respons>().ResponseItem != null ? "Date Completed: " + source8.FirstOrDefault<Respons>().ResponseItem : "Date Completed";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str4,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source9 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("hours") && x.QuestionText.ToLower().Contains("earned"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("hours") && x.QuestionText.ToLower().Contains("earned"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source10 = source9 as IList<Respons> ?? (IList<Respons>)source9.ToList<Respons>();
                string str5 = source10.FirstOrDefault<Respons>().ResponseItem != null ? "No. of Hours: " + source10.FirstOrDefault<Respons>().ResponseItem : "No. of Hours, if applicable";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str5,
                    Format = this._normal,
                    Align = Alignment.left
                });
                string str6 = ((pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("hours") && x.QuestionText.ToLower().Contains("type"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("hours") && x.QuestionText.ToLower().Contains("type"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList) as IList<Respons> ?? (IList<Respons>)source9.ToList<Respons>()).FirstOrDefault<Respons>().ResponseItem != null ? "Type of Hours: " + source10.FirstOrDefault<Respons>().ResponseItem : "Type of Hours";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str6,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source11 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("trainer") && x.QuestionText.ToLower().Contains("name"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("trainer") && x.QuestionText.ToLower().Contains("name"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source12 = source11 as IList<Respons> ?? (IList<Respons>)source11.ToList<Respons>();
                string str7 = source12.FirstOrDefault<Respons>().ResponseItem != null ? "Trainer Name: " + source12.FirstOrDefault<Respons>().ResponseItem : "Trainer Name";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str7,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source13 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("training") && x.QuestionText.ToLower().Contains("location"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("training") && x.QuestionText.ToLower().Contains("location"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                string str8 = source13.FirstOrDefault<Respons>().ResponseItem != null ? "Training Location: " + source13.FirstOrDefault<Respons>().ResponseItem : "Training Location";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str8,
                    Format = this._normal,
                    Align = Alignment.left
                });
                IEnumerable<Respons> source14 = pi.Any<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("age") && x.QuestionText.ToLower().Contains("group"))
                        return x.SubOrdinal == i1;
                    return false;
                })) ? pi.Where<Respons>((Func<Respons, bool>)(x =>
                {
                    if (x.QuestionText.ToLower().Contains("age") && x.QuestionText.ToLower().Contains("group"))
                        return x.SubOrdinal == i1;
                    return false;
                })) : (IEnumerable<Respons>)responsList;
                IList<Respons> source15 = source14 as IList<Respons> ?? (IList<Respons>)source14.ToList<Respons>();
                string str9 = source15.FirstOrDefault<Respons>().ResponseItem != null ? "Age Group: " + source15.FirstOrDefault<Respons>().ResponseItem : "Age Group";
                paragraphList.Add(new FormatUserInformation.Paragraph()
                {
                    Text = str9,
                    Format = this._normal,
                    Align = Alignment.left
                });
            }
            return paragraphList;
        }

        public class Paragraph
        {
            public string Text { get; set; }

            public Formatting Format { get; set; }

            public Alignment Align { get; set; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ContosoUniversity.Models
{
    public class Student
    {
        public int ID { get; set; }

        [Required]
        [Display(Name ="Last Name")]
        [StringLength(50,MinimumLength =2, ErrorMessage ="Last Name cannot be less than 2 chars or more than 50 chars.")]
        [RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$",ErrorMessage ="Name must start by Capital letter and have only letters.")]
        public string LastName { get; set; }

        [Required]
        [Display(Name ="First Name")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First Name cannot be less than 2 chars or more than 50 chars.")]
        [RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$", ErrorMessage = "Name must start by Capital letter and have only letters.")]
        [Column("FirstName")]
        public string FirstMidName { get; set; }
        [Display(Name ="Full Name")]
        public string FullName
        {
            get
            {
              return  LastName + ", " + FirstMidName;
            }
        }

        [Display(Name ="Enrollment Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode =true)]
        public DateTime EnrollmentDate { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
    }
}

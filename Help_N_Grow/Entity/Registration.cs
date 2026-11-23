using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Help_N_Grow.Entity
{
    public class Registration
    {
        [Key]
        public int Reg_Id { get; set; }

        [Required(ErrorMessage = "Please Enter Name")]
        [Display(Name = "Member Name")]
        public string Full_Name { get; set; }

        [Required(ErrorMessage = "Please Enter Mobile_No")]
        [Display(Name = "Mobile No")]
        [DisplayFormat(DataFormatString ="{0:#}")]
        public decimal Mobile_No { get; set; }

        [Display(Name = "Bank Account Number")]
        [DisplayFormat(DataFormatString = "{0:#}")]
        public decimal Bank_AC { get; set; }

        [Display(Name = "IFSC_Code")]
        public string IFSC_Code { get; set; }

        [Display(Name = "UPI Number")]
        public string UPI_ID { get; set; }

        [Required(ErrorMessage = "Please Enter Password")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please accept Term_Condition")]
        [Display(Name = "Accept Terms and Condition")]
        public bool Term_Condition_Accepted { get; set; }
        public string Role { get; set; }
        [Required(ErrorMessage = "Please Enter User Name")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        //hiden ID
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Reg_Date { get; set; }
        public int Level_Id { get; set; }
        public int Parent_Id { get; set; }
        public bool Is_Active { get; set; }

        public int Package_ID { get; set; }
        [Display(Name = "Package Name")]
        public string Package_Name { get; set; }

    }

    
   public class RegistrationVM
    {
        
        public Registration Registrationobj { get; set; }
        public string Is_Approved { get; set; }
    }
    public class RegistrationViewModel
    {
        public string showCreate { get; set; }
        public List<RegistrationVM> RegistrationoVMLst { get; set; }
        public Registration registration { get; set; }
        public List<OldMember> oldMemberLst { get; set; }
    }

    public class OldMember
    {
        public int Member_Id { get; set; }
        [Display(Name = "Unique No")]
        public string Unique_No { get; set; }
        [Display(Name = "Member Name")]
        public string Member_Name { get; set; }
    }
}

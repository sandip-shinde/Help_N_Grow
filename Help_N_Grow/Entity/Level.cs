using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Help_N_Grow.Entity
{
    public class Level
    {
        [Key]
        public int Level_Id { get; set; }

        [Required(ErrorMessage = "Please Enter Level Name")]
        [Display(Name = "Level Name")]
        public string Level_Name { get; set; }
        [Required(ErrorMessage = "Please Enter Amount")]
        [Display(Name = "Amount")]
        public decimal? Amount { get; set; }
        [Required(ErrorMessage = "Please Enter Percentage")]
        [Display(Name = "Company Percentage")]
        public decimal? Company_Percentage { get; set; }
        [Required(ErrorMessage = "Please Enter Self Amount")]
        [Display(Name = "Self Amount")]
        public decimal ? Self_Amount { get; set; }

        [Required(ErrorMessage = "Please Enter Package ID")]
        public int Package_ID { get; set; }

        [Required(ErrorMessage = "Please Enter Package Name")]
        [Display(Name = "Package Name")]
        public string Package_Name { get; set; }

        public int MemberCount { get; set; }

        
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Help_N_Grow.Entity
{
    public class TblTransaction
    {
        [Key]
        public int Transaction_ID { get; set; }
        [Required(ErrorMessage = "Please upload Company Transaction Reciept")]
        [Display(Name = "Company Transaction Reciept")]
        public string Upload_Path { get; set; }
        [Required(ErrorMessage = "Please Enter Company Transaction No")]
        [Display(Name = "Company Transaction Number")]
        public string Transaction_No { get; set; }
        public bool Is_Approved { get; set; }

        [Display(Name = "Registration No")]
        public int Reg_Id { get; set; }
        [Display(Name = "Level No")]
        public int Level_Id { get; set; }
        public int Parent_Id { get; set; }

        [Required(ErrorMessage = "Please Enter Self Transaction No")]
        [Display(Name = "Self Transaction Number")]
        public string Transaction_NoSelf { get; set; }
        [Required(ErrorMessage = "Please upload Self Transaction Reciept")]
        [Display(Name = "Self Transaction Reciept")]
        public string Upload_PathSelf { get; set; }

        [Display(Name = "Self Transaction Amount")]
        public decimal? Self_Amount { get; set; }
        [Display(Name = "Company Transaction Amount")]
        public decimal? Company_Percentage { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? T_Date { get; set; }

        public byte[] MemberTransactionPhoto { get; set; }
        public byte[] CompanyTransactionPhoto { get; set; }
        public string CompanyTransactionPhotoContentType { get; set; }
        public string MemberTransactionPhotoContentType { get; set; }
        [DataType(DataType.MultilineText)]
        public string Note { get; set; }
        public int Package_ID { get; set; }
        [Display(Name = "Package Name")]
        public string Package_Name { get; set; }
    }

    public class Transaction_Upload
    {

        [Display(Name = "Company Transaction Recipet")]
        public IFormFile FileToUpload1 { get; set; }
        [Display(Name = "Self Transaction Recipet")]
        public IFormFile FileToUpload2 { get; set; }

        [Required(ErrorMessage = "Please Enter Company Transaction No")]
        [Display(Name = "Company Transaction Number")]
        public string Transaction_NoCompany { get; set; }

        [Required(ErrorMessage = "Please Enter Self Transaction No")]
        [Display(Name = "Self Transaction Number")]
        public string Transaction_NoSelf { get; set; }

    }

    public class TblTransactionApproval
    {
        public decimal Mobile_No { get; set; }
        public DateTime? Transaction_Date { get; set; }
        public string Full_Name { get; set; }
        public int Transaction_ID { get; set; }
        public string Unique_No { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        [Display(Name = "Trans. Number")]
        public string Transaction_No { get; set; }
        public string Upload_Path { get; set; }
        public bool Is_Approved { get; set; }

        public int Reg_Id { get; set; }
        public int Level_Id { get; set; }
        public int Parent_Id { get; set; }
        [Display(Name = "Self Trans. Number")]
        public string Transaction_NoSelf { get; set; }
        [Display(Name = "Self Transaction Amount")]
        public decimal? Self_Amount { get; set; }
        [Display(Name = "Trans. Amount")]
        public decimal? Company_Percentage { get; set; }
        public int Package_ID { get; set; }
        [Display(Name = "Package Name")]
        public string Package_Name { get; set; }

    }

    public class vmSearch{
      public  string FilterBy { get; set; }
      public  string fillValue { get; set; }
    }

    public class AdminReportsVM
    {
        public  List<Adminpackage_NameReports> Adminpackage_NameReports { get; set; }
        public List<Adminpackage_Name_Level_Reports> Adminpackage_Name_Level_Reports { get; set; }
        public List<Adminpackage_Name_Level_Name_Reports> Adminpackage_Name_Level_Name_Reports { get; set; }

    }
    
   public class Adminpackage_NameReports
    {
        [Key]
        public int ID { get; set; }
        public string package_Name { get; set; }
        public decimal ? Amount { get; set; }
    }

    public class Adminpackage_Name_Level_Reports
    {
        [Key]
        public int ID { get; set; }
        public string package_Name { get; set; }
        public int Level { get; set; }
        public decimal? Amount { get; set; }
    }

    public class Adminpackage_Name_Level_Name_Reports
    {
        [Key]
        public int ID { get; set; }
        public string package_Name { get; set; }
        public int Level { get; set; }
        public string Full_Name { get; set; }
        public decimal? Amount { get; set; }
    }
}

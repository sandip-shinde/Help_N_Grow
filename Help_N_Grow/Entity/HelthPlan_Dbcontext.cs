using Help_N_Grow.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Help_N_Grow.Entity;

namespace Help_N_Grow.Entity
{
    public class HelthPlan_Dbcontext : DbContext
    {
        public HelthPlan_Dbcontext(DbContextOptions<HelthPlan_Dbcontext> options)
            : base(options)
        {

        }

       public DbSet<UserLogin> UserLogin { get; set; }
        public DbSet<Registration> Registration { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Registration>(entity => {
                entity.HasKey(k => k.Reg_Id);
            });
        }
        public DbSet<Level> Level { get; set; }
        public DbSet<TblTransaction> TblTransaction { get; set; }
        //public DbSet<TblTransactionVM> TblTransactionVM { get; set; }
       // public DbSet<AdminReportsVM> AdminReportsVM { get; set; }
        public DbSet<Adminpackage_NameReports> Adminpackage_NameReports { get; set; }
        public DbSet<Adminpackage_Name_Level_Reports> Adminpackage_Name_Level_Reports { get; set; }
        public DbSet<Adminpackage_Name_Level_Name_Reports> Adminpackage_Name_Level_Name_Reports { get; set; }

    }
}

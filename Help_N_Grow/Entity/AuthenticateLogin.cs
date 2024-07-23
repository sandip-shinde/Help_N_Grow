using Help_N_Grow.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Help_N_Grow.Entity
{
    public class AuthenticateLogin : ILogin
    {
        private readonly HelthPlan_Dbcontext _context;

        public AuthenticateLogin(HelthPlan_Dbcontext context)
        {
            _context = context;
        }
        public async Task<Registration> AuthenticateUser(string username, string passcode,string role)
        {
          var succeeded = new Registration();
            if (await _context.Registration.Where(authUser => authUser.UserName.ToLower() == username.ToLower() && authUser.Password.ToLower() == passcode.ToLower() && authUser.Is_Active == true && authUser.Role == role).AnyAsync())
            {
                succeeded = await _context.Registration.Where(authUser => authUser.UserName.ToLower() == username.ToLower() && authUser.Password.ToLower() == passcode.ToLower() && authUser.Is_Active == true && authUser.Role==role).FirstOrDefaultAsync();
            }          
            return succeeded;
        }

        public async Task<IEnumerable<UserLogin>> Getuser()
        {
            return await _context.UserLogin.ToListAsync();
        }
    }
}

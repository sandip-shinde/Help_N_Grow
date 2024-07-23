using Help_N_Grow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Help_N_Grow.Entity
{
    public interface ILogin
    {
        Task<IEnumerable<UserLogin>> Getuser();
        Task<Registration> AuthenticateUser(string username, string passcode, string role);
    }
}

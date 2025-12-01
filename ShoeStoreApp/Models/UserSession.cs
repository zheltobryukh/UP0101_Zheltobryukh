using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStoreApp.Models
{
    public static class UserSession
    {
        public static User CurrentUser { get; set; }
        public static bool IsLoggedIn => CurrentUser != null;
        public static bool IsGuest => CurrentUser == null;
        public static void Logout() => CurrentUser = null;
    }
}

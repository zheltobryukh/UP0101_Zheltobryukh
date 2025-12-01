using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStoreApp
{
    public partial class User
    {
        public string FullName => $"{UserSurname} {UserName} {UserPatronymic}".Trim();
        public bool IsAdmin => UserRole?.RoleName == "Администратор";
        public bool IsManager => UserRole?.RoleName == "Менеджер";
        public bool IsClient => UserRole?.RoleName == "Клиент";
        public string RoleNameText => UserRole?.RoleName ?? "Неизвестно";
    }
}

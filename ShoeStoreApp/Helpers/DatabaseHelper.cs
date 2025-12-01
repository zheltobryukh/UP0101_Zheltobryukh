using System;
using System.Linq;
using System.Windows;

namespace ShoeStoreApp.Helpers
{
    public class DatabaseHelper
    {
        public static bool TestConnection()
        {
            try
            {
                using (var context = new ShoeStoreDBEntities())
                {
                    var testQuery = context.UserRoles.Take(1).ToList();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        public static ShoeStoreDBEntities GetContext()
        {
            return new ShoeStoreDBEntities();
        }
    }
}
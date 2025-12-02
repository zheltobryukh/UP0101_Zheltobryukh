using System.Linq;
using System.Windows;
using System.Windows.Input;
using ShoeStoreApp.Models;

namespace ShoeStoreApp.Views
{
    public partial class OrdersWindow : Window
    {
        public OrdersWindow()
        {
            InitializeComponent();
            LoadOrders();

            if (UserSession.CurrentUser.RoleID != 1)
            {
                BtnAddOrder.Visibility = Visibility.Collapsed;
                DgOrders.ContextMenu = null;
            }
        }

        private void LoadOrders()
        {
            using (var db = new ShoeStoreDBEntities())
            {
                DgOrders.ItemsSource = db.Orders.Include("OrderStatu").Include("User").ToList();
            }
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            var win = new OrderEditWindow(null);
            if (win.ShowDialog() == true) LoadOrders();
        }

        private void DgOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UserSession.CurrentUser.RoleID != 1) return;

            if (DgOrders.SelectedItem is Order selectedOrder)
            {
                var win = new OrderEditWindow(selectedOrder);
                if (win.ShowDialog() == true) LoadOrders();
            }
        }

        private void MenuItemDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            var order = DgOrders.SelectedItem as Order;
            if (order == null) return;

            if (MessageBox.Show($"Удалить заказ №{order.OrderID}?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var db = new ShoeStoreDBEntities())
                {
                    var o = db.Orders.Find(order.OrderID);
                    db.Orders.Remove(o);
                    db.SaveChanges();
                    LoadOrders();
                }
            }
        }
    }
}
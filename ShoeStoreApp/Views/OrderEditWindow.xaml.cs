using System;
using System.Linq;
using System.Windows;
using ShoeStoreApp.Models;

namespace ShoeStoreApp.Views
{
    public partial class OrderEditWindow : Window
    {
        private Order _currentOrder;
        private bool _isEdit;

        public OrderEditWindow(Order order)
        {
            InitializeComponent();
            using (var db = new ShoeStoreDBEntities())
            {
                CmbStatus.ItemsSource = db.OrderStatus.ToList();
                CmbPickupPoint.ItemsSource = db.PickupPoints.ToList();
            }

            if (order != null)
            {
                _isEdit = true;
                using (var db = new ShoeStoreDBEntities())
                {
                    _currentOrder = db.Orders.Find(order.OrderID);
                }
                CmbStatus.SelectedValue = _currentOrder.OrderStatusID;
                CmbPickupPoint.SelectedValue = _currentOrder.PickupPointID;
                DpDeliveryDate.SelectedDate = _currentOrder.OrderDeliveryDate;
            }
            else
            {
                _currentOrder = new Order();
                _currentOrder.OrderDate = DateTime.Now;
                _currentOrder.OrderGetCode = new Random().Next(100, 999);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new ShoeStoreDBEntities())
            {
                var orderToSave = _isEdit ? db.Orders.Find(_currentOrder.OrderID) : new Order();

                if (!_isEdit)
                {
                    orderToSave.OrderDate = DateTime.Now;
                    orderToSave.OrderGetCode = new Random().Next(100, 999);
                    orderToSave.UserID = null;
                }

                orderToSave.OrderStatusID = (int)CmbStatus.SelectedValue;
                orderToSave.PickupPointID = (int)CmbPickupPoint.SelectedValue;
                orderToSave.OrderDeliveryDate = DpDeliveryDate.SelectedDate ?? DateTime.Now.AddDays(3);

                if (!_isEdit) db.Orders.Add(orderToSave);

                db.SaveChanges();
            }
            DialogResult = true;
        }
    }
}
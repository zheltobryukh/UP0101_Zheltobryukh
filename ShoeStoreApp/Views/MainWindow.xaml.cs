using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ShoeStoreApp.Models;
using ShoeStoreApp.Helpers; 

namespace ShoeStoreApp.Views
{
    public partial class MainWindow : Window
    {
        private List<Product> _allProducts;

        public bool IsAdmin { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadData();
            SetupUserInterface();
        }

        private void SetupUserInterface()
        {
            var user = UserSession.CurrentUser;
            if (user != null)
            {
                TxtUserName.Text = $"{user.UserSurname} {user.UserName} ({user.UserRole.RoleName})";

                if (user.RoleID == 1)
                {
                    IsAdmin = true;
                    BtnAddProduct.Visibility = Visibility.Visible;
                    BtnOrders.Visibility = Visibility.Visible;
                }
                else if (user.RoleID == 2)
                {
                    BtnOrders.Visibility = Visibility.Visible;
                }
            }
            else
            {
                TxtUserName.Text = "Гость";
                PanelControls.Visibility = Visibility.Collapsed;

            }
        }

        private void LoadData()
        {
            try
            {
                using (var db = new ShoeStoreDBEntities())
                {
                    _allProducts = db.Products.Include("Manufacturer").Include("Category").ToList();

                    var manufacturers = db.Manufacturers.ToList();
                    manufacturers.Insert(0, new Manufacturer { ManufacturerName = "Все производители" });
                    CmbFilter.ItemsSource = manufacturers;
                    CmbFilter.DisplayMemberPath = "ManufacturerName";
                    CmbFilter.SelectedIndex = 0;
                }
                UpdateList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void UpdateList()
        {
            if (_allProducts == null) return;

            var currentList = _allProducts.AsEnumerable();

            var searchText = TxtSearch.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                currentList = currentList.Where(p =>
                    p.ProductName.ToLower().Contains(searchText) ||
                    (p.ProductDescription != null && p.ProductDescription.ToLower().Contains(searchText)));
            }

            if (CmbFilter.SelectedIndex > 0)
            {
                var selectedManuf = CmbFilter.SelectedItem as Manufacturer;
                if (selectedManuf != null)
                    currentList = currentList.Where(p => p.ManufacturerID == selectedManuf.ManufacturerID);
            }

            if (CmbSort.SelectedItem is ComboBoxItem selectedSort)
            {
                switch (selectedSort.Tag.ToString())
                {
                    case "PriceAsc":
                        currentList = currentList.OrderBy(p => p.ProductCost);
                        break;
                    case "PriceDesc":
                        currentList = currentList.OrderByDescending(p => p.ProductCost);
                        break;
                }
            }

            var result = currentList.ToList();
            LvProducts.ItemsSource = result;
            TxtStatusCount.Text = $"{result.Count} из {_allProducts.Count} товаров";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateList();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();
        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateList();

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            UserSession.CurrentUser = null;
            new LoginWindow().Show();
            Close();
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var win = new ProductWindow(null);
            if (win.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void LvProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsAdmin) return;

            if (LvProducts.SelectedItem is Product selectedProduct)
            {
                var win = new ProductWindow(selectedProduct);
                if (win.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            var product = LvProducts.SelectedItem as Product;
            if (product == null) return;

            if (MessageBox.Show("Вы уверены, что хотите удалить этот товар?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var db = new ShoeStoreDBEntities())
                {
                    if (db.OrderProducts.Any(op => op.ProductID == product.ProductID))
                    {
                        MessageBox.Show("Нельзя удалить товар, который есть в заказах.");
                        return;
                    }

                    var p = db.Products.Find(product.ProductID);
                    db.Products.Remove(p);
                    db.SaveChanges();
                    MessageBox.Show("Товар удален.");
                    LoadData();
                }
            }
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            new OrdersWindow().ShowDialog();
        }
    }
}
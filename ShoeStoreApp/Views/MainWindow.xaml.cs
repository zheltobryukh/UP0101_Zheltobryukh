using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ShoeStoreApp.Helpers;
using ShoeStoreApp.Models;

namespace ShoeStoreApp.Views
{
    public partial class MainWindow : Window
    {
        private List<Product> _allProducts;
        private List<Product> _filteredProducts;

        public Visibility IsAdminVisible { get; set; } = Visibility.Collapsed;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            SetupUserInterface();
            LoadProducts();
        }

        private void SetupUserInterface()
        {
            if (UserSession.IsGuest)
            {
                txtUserInfo.Text = "Гость";
                txtUserRole.Text = "Гостевой режим";
                pnlSearchPanel.Visibility = Visibility.Collapsed;
                btnAddProduct.Visibility = Visibility.Collapsed;
                btnOrders.Visibility = Visibility.Collapsed;
                IsAdminVisible = Visibility.Collapsed;
            }
            else if (UserSession.CurrentUser != null)
            {
                txtUserInfo.Text = UserSession.CurrentUser.FullName;
                txtUserRole.Text = UserSession.CurrentUser.RoleNameText;

                if (UserSession.CurrentUser.IsClient)
                {
                    pnlSearchPanel.Visibility = Visibility.Collapsed;
                    btnAddProduct.Visibility = Visibility.Collapsed;
                    btnOrders.Visibility = Visibility.Collapsed;
                    IsAdminVisible = Visibility.Collapsed;
                }
                else if (UserSession.CurrentUser.IsManager)
                {
                    pnlSearchPanel.Visibility = Visibility.Visible;
                    btnAddProduct.Visibility = Visibility.Collapsed;
                    btnOrders.Visibility = Visibility.Visible;
                    IsAdminVisible = Visibility.Collapsed;
                }
                else if (UserSession.CurrentUser.IsAdmin)
                {
                    pnlSearchPanel.Visibility = Visibility.Visible;
                    btnAddProduct.Visibility = Visibility.Visible;
                    btnOrders.Visibility = Visibility.Visible;
                    IsAdminVisible = Visibility.Visible;
                }
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (var context = DatabaseHelper.GetContext())
                {
                    _allProducts = context.Products
                        .Include("Category")
                        .Include("Manufacturer")
                        .Include("Supplier")
                        .Include("Unit")
                        .ToList();

                    ApplyFiltersAndSearch();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}");
            }
        }

        private void ApplyFiltersAndSearch()
        {
            if (_allProducts == null) return;

            var query = _allProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(txtSearch?.Text))
            {
                string text = txtSearch.Text.ToLower();
                query = query.Where(p =>
                    p.ProductName.ToLower().Contains(text) ||
                    (p.ProductDescription != null && p.ProductDescription.ToLower().Contains(text))
                );
            }

            if (cmbFilter?.SelectedItem is ComboBoxItem itemFilter)
            {
                switch (itemFilter.Tag.ToString())
                {
                    case "Discount": query = query.Where(p => p.HasDiscount); break;
                    case "InStock": query = query.Where(p => p.IsInStock); break;
                    case "OutOfStock": query = query.Where(p => !p.IsInStock); break;
                }
            }

            if (cmbSort?.SelectedItem is ComboBoxItem itemSort)
            {
                switch (itemSort.Tag.ToString())
                {
                    case "NameAsc": query = query.OrderBy(p => p.ProductName); break;
                    case "NameDesc": query = query.OrderByDescending(p => p.ProductName); break;
                    case "PriceAsc": query = query.OrderBy(p => p.FinalCost); break;
                    case "PriceDesc": query = query.OrderByDescending(p => p.FinalCost); break;
                    case "Stock": query = query.OrderByDescending(p => p.ProductQuantityInStock); break;
                }
            }

            _filteredProducts = query.ToList();
            itemsProducts.ItemsSource = _filteredProducts;
            txtStatus.Text = $"Показано {_filteredProducts.Count} из {_allProducts.Count} товаров";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFiltersAndSearch();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFiltersAndSearch();
        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFiltersAndSearch();

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            ProductWindow window = new ProductWindow(null);
            if (window.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button).Tag as Product;
            ProductWindow window = new ProductWindow(product);
            if (window.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var product = (sender as Button).Tag as Product;

            if (MessageBox.Show($"Вы уверены, что хотите удалить {product.ProductName}?",
                "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                using (var context = DatabaseHelper.GetContext())
                {
                    var pToDelete = context.Products.Find(product.ProductID);
                    if (pToDelete != null)
                    {
                        context.Products.Remove(pToDelete);
                        context.SaveChanges();
                        LoadProducts();
                        MessageBox.Show("Товар удален");
                    }
                }
            }
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Окно работы с заказами", "Заказы");
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            UserSession.Logout();
            new LoginWindow().Show();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupSearchAndFilters();
        }

        private void SetupSearchAndFilters()
        {
            if (cmbSort.Items.Count == 0)
            {
                cmbSort.Items.Add(new ComboBoxItem { Content = "Без сортировки", Tag = "None" });
                cmbSort.Items.Add(new ComboBoxItem { Content = "По названию (А-Я)", Tag = "NameAsc" });
                cmbSort.Items.Add(new ComboBoxItem { Content = "По названию (Я-А)", Tag = "NameDesc" });
                cmbSort.Items.Add(new ComboBoxItem { Content = "По цене (возрастание)", Tag = "PriceAsc" });
                cmbSort.Items.Add(new ComboBoxItem { Content = "По цене (убывание)", Tag = "PriceDesc" });
                cmbSort.Items.Add(new ComboBoxItem { Content = "По количеству на складе", Tag = "Stock" });
                cmbSort.SelectedIndex = 0;

                cmbFilter.Items.Add(new ComboBoxItem { Content = "Все товары", Tag = "All" });
                cmbFilter.Items.Add(new ComboBoxItem { Content = "Со скидкой", Tag = "Discount" });
                cmbFilter.Items.Add(new ComboBoxItem { Content = "В наличии", Tag = "InStock" });
                cmbFilter.Items.Add(new ComboBoxItem { Content = "Нет в наличии", Tag = "OutOfStock" });
                cmbFilter.SelectedIndex = 0;
            }
        }
    }
}
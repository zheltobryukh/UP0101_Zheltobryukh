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

        public MainWindow()
        {
            InitializeComponent();
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
                btnManageProducts.Visibility = Visibility.Collapsed;
            }
            else if (UserSession.CurrentUser != null)
            {
                txtUserInfo.Text = UserSession.CurrentUser.FullName;
                txtUserRole.Text = UserSession.CurrentUser.RoleNameText;

                if (UserSession.CurrentUser.IsClient)
                {
                    pnlSearchPanel.Visibility = Visibility.Collapsed;
                    btnManageProducts.Visibility = Visibility.Collapsed;
                }
                else if (UserSession.CurrentUser.IsManager)
                {
                    pnlSearchPanel.Visibility = Visibility.Visible;
                    btnManageProducts.Visibility = Visibility.Collapsed;
                    SetupSearchAndFilters();
                }
                else if (UserSession.CurrentUser.IsAdmin)
                {
                    pnlSearchPanel.Visibility = Visibility.Visible;
                    btnManageProducts.Visibility = Visibility.Visible;
                    SetupSearchAndFilters();
                }
            }
        }

        private void SetupSearchAndFilters()
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
                        .OrderBy(p => p.ProductName)
                        .ToList();

                    _filteredProducts = new List<Product>(_allProducts);
                    DisplayProducts();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DisplayProducts()
        {
            itemsProducts.ItemsSource = null;
            itemsProducts.ItemsSource = _filteredProducts;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFiltersAndSearch();
        }

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSearch();
        }

        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSearch();
        }

        private void ApplyFiltersAndSearch()
        {
            if (_allProducts == null)
                return;

            _filteredProducts = new List<Product>(_allProducts);

            if (!string.IsNullOrWhiteSpace(txtSearch?.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                _filteredProducts = _filteredProducts.Where(p =>
                    p.ProductName.ToLower().Contains(searchText) ||
                    (p.ProductDescription != null && p.ProductDescription.ToLower().Contains(searchText)) ||
                    p.ManufacturerNameText.ToLower().Contains(searchText) ||
                    p.CategoryNameText.ToLower().Contains(searchText)
                ).ToList();
            }

            if (cmbFilter?.SelectedItem != null)
            {
                string filterTag = ((ComboBoxItem)cmbFilter.SelectedItem).Tag.ToString();

                switch (filterTag)
                {
                    case "Discount":
                        _filteredProducts = _filteredProducts.Where(p => p.HasDiscount).ToList();
                        break;
                    case "InStock":
                        _filteredProducts = _filteredProducts.Where(p => p.IsInStock).ToList();
                        break;
                    case "OutOfStock":
                        _filteredProducts = _filteredProducts.Where(p => !p.IsInStock).ToList();
                        break;
                }
            }

            if (cmbSort?.SelectedItem != null)
            {
                string sortTag = ((ComboBoxItem)cmbSort.SelectedItem).Tag.ToString();

                switch (sortTag)
                {
                    case "NameAsc":
                        _filteredProducts = _filteredProducts.OrderBy(p => p.ProductName).ToList();
                        break;
                    case "NameDesc":
                        _filteredProducts = _filteredProducts.OrderByDescending(p => p.ProductName).ToList();
                        break;
                    case "PriceAsc":
                        _filteredProducts = _filteredProducts.OrderBy(p => p.FinalCost).ToList();
                        break;
                    case "PriceDesc":
                        _filteredProducts = _filteredProducts.OrderByDescending(p => p.FinalCost).ToList();
                        break;
                    case "Stock":
                        _filteredProducts = _filteredProducts.OrderByDescending(p => p.ProductQuantityInStock).ToList();
                        break;
                }
            }

            DisplayProducts();
        }

        private void BtnManageProducts_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функционал управления товарами будет реализован в следующем модуле",
                "Информация",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                UserSession.Logout();
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
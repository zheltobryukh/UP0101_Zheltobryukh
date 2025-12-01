using System;
using System.Linq;
using System.Windows;
using ShoeStoreApp.Helpers;
using ShoeStoreApp.Models;

namespace ShoeStoreApp.Views
{
    public partial class ProductWindow : Window
    {
        private Product _currentProduct;

        public ProductWindow(Product product)
        {
            InitializeComponent();
            _currentProduct = product;
            LoadData();
        }

        private void LoadData()
        {
            using (var context = DatabaseHelper.GetContext())
            {
                cmbCategory.ItemsSource = context.Categories.ToList();
            }

            if (_currentProduct != null)
            {
                txtArticle.Text = _currentProduct.ProductArticleNumber;
                txtArticle.IsEnabled = false;
                txtName.Text = _currentProduct.ProductName;
                txtCost.Text = _currentProduct.ProductCost.ToString();
                txtDiscount.Text = _currentProduct.ProductDiscountAmount?.ToString();
                txtStock.Text = _currentProduct.ProductQuantityInStock.ToString();
                txtDescription.Text = _currentProduct.ProductDescription;

                if (cmbCategory.ItemsSource != null)
                {
                    foreach (Category cat in cmbCategory.ItemsSource)
                    {
                        if (cat.CategoryID == _currentProduct.CategoryID)
                        {
                            cmbCategory.SelectedItem = cat;
                            break;
                        }
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = DatabaseHelper.GetContext())
                {
                    if (_currentProduct == null)
                    {
                        var newProduct = new Product
                        {
                            ProductArticleNumber = txtArticle.Text,
                            ProductName = txtName.Text,
                            ProductCost = decimal.Parse(txtCost.Text),
                            ProductDiscountAmount = string.IsNullOrEmpty(txtDiscount.Text) ? (decimal?)null : decimal.Parse(txtDiscount.Text),
                            ProductQuantityInStock = int.Parse(txtStock.Text),
                            ProductDescription = txtDescription.Text,
                            CategoryID = (cmbCategory.SelectedItem as Category)?.CategoryID ?? 1,
                            ManufacturerID = 1,
                            SupplierID = 1,
                            UnitID = 1,
                            ProductPhoto = null
                        };
                        context.Products.Add(newProduct);
                    }
                    else
                    {
                        var productToUpdate = context.Products.Find(_currentProduct.ProductID);
                        if (productToUpdate != null)
                        {
                            productToUpdate.ProductName = txtName.Text;
                            productToUpdate.ProductCost = decimal.Parse(txtCost.Text);
                            productToUpdate.ProductDiscountAmount = string.IsNullOrEmpty(txtDiscount.Text) ? (decimal?)null : decimal.Parse(txtDiscount.Text);
                            productToUpdate.ProductQuantityInStock = int.Parse(txtStock.Text);
                            productToUpdate.ProductDescription = txtDescription.Text;
                            productToUpdate.CategoryID = (cmbCategory.SelectedItem as Category)?.CategoryID ?? productToUpdate.CategoryID;
                        }
                    }
                    context.SaveChanges();
                    MessageBox.Show("Данные сохранены");
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
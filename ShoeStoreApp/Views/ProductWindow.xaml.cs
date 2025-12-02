using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ShoeStoreApp.Helpers;

namespace ShoeStoreApp.Views
{
    public partial class ProductWindow : Window
    {
        private Product _currentProduct = new Product();
        private bool _isEditMode = false;
        private string _selectedImagePath = null;

        public ProductWindow(Product product)
        {
            InitializeComponent();

            using (var db = new ShoeStoreDBEntities())
            {
                CmbCategory.ItemsSource = db.Categories.ToList();
                CmbManufacturer.ItemsSource = db.Manufacturers.ToList();
                CmbSupplier.ItemsSource = db.Suppliers.ToList();
                CmbUnit.ItemsSource = db.Units.ToList();
            }

            if (product != null)
            {
                _isEditMode = true;
                using (var db = new ShoeStoreDBEntities())
                {
                    _currentProduct = db.Products.Find(product.ProductID);
                }

                TxtArticle.Text = _currentProduct.ProductArticleNumber;
                TxtArticle.IsReadOnly = true;
                TxtName.Text = _currentProduct.ProductName;
                TxtCost.Text = _currentProduct.ProductCost.ToString("F2");
                TxtStock.Text = _currentProduct.ProductQuantityInStock.ToString();
                TxtDescription.Text = _currentProduct.ProductDescription;

                // ИСПРАВЛЕНО: Форматирование скидки без нулей после запятой
                TxtDiscount.Text = _currentProduct.ProductDiscountAmount.HasValue
                    ? _currentProduct.ProductDiscountAmount.Value.ToString("F0")
                    : "";

                CmbCategory.SelectedValue = _currentProduct.CategoryID;
                CmbManufacturer.SelectedValue = _currentProduct.ManufacturerID;
                CmbSupplier.SelectedValue = _currentProduct.SupplierID;
                CmbUnit.SelectedValue = _currentProduct.UnitID;

                ImgProduct.Source = _currentProduct.ImagePath;
            }

            DataContext = _currentProduct;
        }

        private void BtnChangeImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(openFileDialog.FileName);
                bitmap.EndInit();

                if (bitmap.PixelWidth > 300 || bitmap.PixelHeight > 200)
                {
                    MessageBox.Show("Размер изображения не должен превышать 300x200 пикселей!");
                    return;
                }

                _selectedImagePath = openFileDialog.FileName;
                ImgProduct.Source = bitmap;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text) || string.IsNullOrWhiteSpace(TxtArticle.Text))
            {
                MessageBox.Show("Заполните Артикул и Наименование!");
                return;
            }

            if (!decimal.TryParse(TxtCost.Text, out decimal cost) || cost < 0)
            {
                MessageBox.Show("Некорректная цена!");
                return;
            }

            if (!int.TryParse(TxtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Некорректное количество!");
                return;
            }

            byte? discount = null;
            if (!string.IsNullOrWhiteSpace(TxtDiscount.Text))
            {
                if (byte.TryParse(TxtDiscount.Text, out byte d) && d >= 0 && d <= 100)
                    discount = d;
                else
                {
                    MessageBox.Show("Скидка должна быть целым числом от 0 до 100!");
                    return;
                }
            }

            using (var db = new ShoeStoreDBEntities())
            {
                if (!_isEditMode && db.Products.Any(p => p.ProductArticleNumber == TxtArticle.Text))
                {
                    MessageBox.Show("Артикул уже существует!");
                    return;
                }

                string photoName = _currentProduct.ProductPhoto;

                if (_selectedImagePath != null)
                {
                    string resourcesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");

                    if (!Directory.Exists(resourcesFolder))
                        Directory.CreateDirectory(resourcesFolder);

                    string ext = Path.GetExtension(_selectedImagePath);
                    string newFileName = Guid.NewGuid().ToString() + ext;
                    string destPath = Path.Combine(resourcesFolder, newFileName);

                    try
                    {
                        File.Copy(_selectedImagePath, destPath, true);
                        photoName = newFileName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при копировании фото: " + ex.Message);
                        return;
                    }
                }

                Product productToSave = _isEditMode ? db.Products.Find(_currentProduct.ProductID) : new Product();

                productToSave.ProductArticleNumber = TxtArticle.Text;
                productToSave.ProductName = TxtName.Text;
                productToSave.ProductCost = cost;
                productToSave.ProductQuantityInStock = stock;
                productToSave.ProductDiscountAmount = discount;
                productToSave.ProductDescription = TxtDescription.Text;
                productToSave.ProductPhoto = photoName;

                productToSave.CategoryID = (int)CmbCategory.SelectedValue;
                productToSave.ManufacturerID = (int)CmbManufacturer.SelectedValue;
                productToSave.SupplierID = (int)CmbSupplier.SelectedValue;
                productToSave.UnitID = (int)CmbUnit.SelectedValue;

                if (!_isEditMode) db.Products.Add(productToSave);

                try
                {
                    db.SaveChanges();
                    MessageBox.Show("Товар успешно сохранен!");
                    DialogResult = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка БД: " + ex.Message);
                }
            }
        }
    }
}
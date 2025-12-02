using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShoeStoreApp
{
    public partial class Product
    {
        public ImageSource ImagePath
        {
            get
            {
                var imageName = ProductPhoto;

                if (string.IsNullOrEmpty(imageName))
                {
                    return LoadFromResource("picture.png");
                }

                string localFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");
                string localFilePath = Path.Combine(localFolder, imageName);

                if (File.Exists(localFilePath))
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(localFilePath);
                        bitmap.EndInit();
                        return bitmap;
                    }
                    catch
                    {
                    }
                }

                return LoadFromResource(imageName);
            }
        }

        private ImageSource LoadFromResource(string imageName)
        {
            try
            {
                return new BitmapImage(new Uri($"pack://application:,,,/resources/{imageName}"));
            }
            catch
            {
                return new BitmapImage(new Uri("pack://application:,,,/resources/picture.png"));
            }
        }

        public SolidColorBrush BackgroundColor
        {
            get
            {
                if (ProductQuantityInStock == 0)
                    return Brushes.LightBlue;

                if (ProductDiscountAmount > 15)
                    return (SolidColorBrush)new BrushConverter().ConvertFrom("#2E8B57");

                return Brushes.White;
            }
        }

        public decimal FinalCost
        {
            get
            {
                if (ProductDiscountAmount.HasValue && ProductDiscountAmount > 0)
                {
                    return ProductCost * (1.0m - (ProductDiscountAmount.Value / 100.0m));
                }
                return ProductCost;
            }
        }

        public bool HasDiscount => ProductDiscountAmount.HasValue && ProductDiscountAmount > 0;
        public bool HasLargeDiscount => ProductDiscountAmount.HasValue && ProductDiscountAmount > 15;
        public bool IsInStock => ProductQuantityInStock > 0;

        public string OriginalPriceText => ProductCost.ToString("N2");
        public string FinalPriceText => FinalCost.ToString("N2");

        public string CategoryNameText => Category?.CategoryName ?? "Без категории";
        public string ManufacturerNameText => Manufacturer?.ManufacturerName ?? "Неизвестно";
        public string SupplierNameText => Supplier?.SupplierName ?? "Неизвестно";
        public string UnitNameText => Unit?.UnitName ?? "шт.";
    }
}
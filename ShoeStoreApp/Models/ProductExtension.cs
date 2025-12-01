using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStoreApp
{
    public partial class Product
    {
        public decimal FinalCost
        {
            get
            {
                if (ProductDiscountAmount.HasValue && ProductDiscountAmount > 0)
                    return ProductCost * (1 - ProductDiscountAmount.Value / 100);
                return ProductCost;
            }
        }

        public bool HasDiscount => ProductDiscountAmount.HasValue && ProductDiscountAmount > 0;
        public bool HasLargeDiscount => ProductDiscountAmount.HasValue && ProductDiscountAmount > 15;
        public bool IsInStock => ProductQuantityInStock > 0;
        public string OriginalPriceText => ProductCost.ToString("N2") + " ₽";
        public string FinalPriceText => FinalCost.ToString("N2") + " ₽";

        public string CategoryNameText => Category?.CategoryName ?? "Без категории";
        public string ManufacturerNameText => Manufacturer?.ManufacturerName ?? "Неизвестно";
        public string SupplierNameText => Supplier?.SupplierName ?? "Неизвестно";
        public string UnitNameText => Unit?.UnitName ?? "шт.";
    }
}

using Production_Erp_Web_App.Domain.Common;

namespace Production_Erp_Web_App.Domain.Entities
{
    public class Item: BaseEntity
    {
        public string Sku { get; set; } = default!;
        public string Name { get; set; } = default!;
        public decimal Rate { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

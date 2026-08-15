using Production_Erp_Web_App.Domain.Common;

namespace Production_Erp_Web_App.Domain.Entities
{
    public class Supplier: BaseEntity
    {
        public string Name { get; set; } = default!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();

    }
}

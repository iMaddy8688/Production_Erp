using Production_Erp_Web_App.Domain.Common;

namespace Production_Erp_Web_App.Domain.Entities
{
    public class SalesInvoice: BaseEntity
    {
        public string InvoiceNumber { get; set; } = default!;
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;
    }
}

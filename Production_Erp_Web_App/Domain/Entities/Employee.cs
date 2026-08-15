using Production_Erp_Web_App.Domain.Common;

namespace Production_Erp_Web_App.Domain.Entities
{
    public class Employee: BaseEntity
    {
        public string FullName { get; set; } = default!;
        public string? Designation { get; set; }
        public DateTime DateOfJoining { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

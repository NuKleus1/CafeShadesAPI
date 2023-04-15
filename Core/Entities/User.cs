#nullable disable
namespace Core.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string BuildingName { get; set; }
        public string FloorNumber { get; set; }
        public string OfficeNumber { get; set; }
        public string Landmark { get; set; }
        public string MobileNumber { get; set; }
        public DateTime LastLoginAt { get; set; }
        public bool isLoggedIn { get; set; }
    }
}

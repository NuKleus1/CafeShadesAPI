#nullable disable
namespace Cafeshades.Models.Dtos.Request
{
    public class UserRequest
    {
        public string name { get; set; }
        public string buildingName { get; set; }
        public string floorNumber { get; set; }
        public string officeNumber { get; set; }
        public string landmark { get; set; }
        public string mobileNumber { get; set; }
        public string fcm { get; set; }
    }
}

namespace CafeShades.Models
{
    public class ApiResponse
    {
        public bool responseStatus { get; set; } = false;
        public ApiResponse(bool _responseStatus)
        {
            responseStatus = _responseStatus;
        }
        public ApiResponse()
        {
            
        }

    }
}

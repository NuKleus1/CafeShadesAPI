namespace CafeShades.Models
{
    public class ApiResponse
    {
        public bool responseStatus { get; set; } = false;
        public string responseMessage { get; set; }

        public ApiResponse(string responseMessage)
        {
            this.responseMessage = responseMessage;
        }

        public ApiResponse(bool responseStatus, string responseMessage)
        {
            this.responseMessage = responseMessage;
            this.responseStatus = responseStatus;

        }
        public ApiResponse()
        {
            
        }
    }
}

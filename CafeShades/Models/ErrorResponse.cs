namespace Cafeshades.Models
{
    public class ErrorResponse:ApiResponse
    {
        public string responseMessage { get; set; }

        public ErrorResponse(string responseMessage)
        {
            this.responseMessage = responseMessage;
        }
    }
}

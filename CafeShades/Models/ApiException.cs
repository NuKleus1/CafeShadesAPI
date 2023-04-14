namespace CafeShades.Models
{
    public class ApiException : ApiResponse
    {
        public int StatusCode {  get; set; }
        public string Details { get; set; }
        public string Message { get; set; }

        public ApiException(int statusCode, string message = null, string details = null)
        {
            Details = details;
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }

        private string GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "Uauthorized Access",
                404 => "Resource Not Found",
                500 => "Internal Server Error",
                _ => "Unkown Error"
            };
        }
    }
}

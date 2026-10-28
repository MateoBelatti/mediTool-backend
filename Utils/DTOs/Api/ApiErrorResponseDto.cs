namespace Utils.DTO.Api
{
    public class ApiErrorResponseDto
    {
        public string Status { get; set; } = "error";
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Details { get; set; } = new();
    }
}

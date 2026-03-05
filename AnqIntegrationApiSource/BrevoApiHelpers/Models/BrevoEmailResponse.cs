namespace BrevoApiHelpers.Models
{
    public class BrevoEmailResponse
    {
        public bool Success { get; set; }
        public string? MessageId { get; set; }
        public int StatusCode { get; set; }
        public string? Error { get; set; }
        public string? RawResponseBody { get; set; }
    }
}
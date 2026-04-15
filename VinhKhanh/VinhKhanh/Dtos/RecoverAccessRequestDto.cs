namespace VinhKhanh.Dtos
{
    public class RecoverAccessRequestDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string ClientDeviceId { get; set; } = string.Empty;
        public string QrRaw { get; set; } = string.Empty;
    }
}

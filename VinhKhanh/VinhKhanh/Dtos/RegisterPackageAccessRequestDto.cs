namespace VinhKhanh.Dtos
{
    public class RegisterPackageAccessRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public int IdGoi { get; set; }
        public bool BypassPayment { get; set; }
        public string ClientDeviceId { get; set; } = string.Empty;
    }
}

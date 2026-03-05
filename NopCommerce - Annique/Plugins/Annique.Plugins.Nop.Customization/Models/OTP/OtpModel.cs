namespace Annique.Plugins.Nop.Customization.Models.OTP
{
    public class OtpModel
    {
        public bool IsVerified { get; set; }

        public bool ShowSendOtpButtons { get; set; }
    }

    public class OtpApiErrorResponseModel
    {
        public bool isCallbackError { get; set; }
        public string message { get; set; }
    }
}

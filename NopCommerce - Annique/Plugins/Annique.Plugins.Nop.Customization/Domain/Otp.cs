using Nop.Core;
using System;

namespace Annique.Plugins.Nop.Customization.Domain
{
    public class Otp : BaseEntity
    {
        public int CustomerID { get; set; }

        public int OTP { get; set; }

        public DateTime Expiry { get; set; }

        public string Cell { get; set; }

        public string Email { get; set; }

        public bool Iverified { get; set; }
    }
}

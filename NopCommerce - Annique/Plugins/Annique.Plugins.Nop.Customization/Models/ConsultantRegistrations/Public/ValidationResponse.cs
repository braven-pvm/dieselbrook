using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Public
{
    public class ValidationResponse
    {
        public string Status { get; set; } 
        public List<ValidationError> Errors { get; set; }

        public string ccustno { get; set; }

        public SponsorInfo Sponsor { get; set; }
    }

    public class ValidationError
    {
        public string Rule { get; set; }
        public string Message { get; set; }
    }

    public class SponsorInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Cell { get; set; }
    }

    public class ProblemDetails
    {
        public string type { get; set; }
        public string title { get; set; }
        public int status { get; set; }
        public string detail { get; set; }
        public string instance { get; set; }
        public string additionalProp1 { get; set; }
        public string additionalProp2 { get; set; }
        public string additionalProp3 { get; set; }
    }
}



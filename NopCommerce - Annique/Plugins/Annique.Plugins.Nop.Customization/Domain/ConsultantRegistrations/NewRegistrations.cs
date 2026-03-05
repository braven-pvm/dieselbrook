using Nop.Core;
using System;

namespace Annique.Plugins.Nop.Customization.Domain.ConsultantRegistrations
{
    public class NewRegistrations : BaseEntity
    {
        public string csponsor { get; set; } 
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public string ccustno { get; set; }   
        public string cLname { get; set; }   
        public string cFname { get; set; }    
        public string cCompany { get; set; }  
        public string cTitle { get; set; }   

        public string cLanguage { get; set; }
        public string cEmail { get; set; }    
        public string cPhone1 { get; set; }  
        public string cPhone2 { get; set; }  
        public string cPhone3 { get; set; }  
        public string cFax { get; set; }    
        public string cZip { get; set; }  
        public string ccountry { get; set; } 
        public decimal? latitude { get; set; }
        public decimal? longitude { get; set; }
        public short laccept { get; set; }
        public DateTime? daccept { get; set; }
        public string besttocall { get; set; }  
        public string hearabout { get; set; }   
        public string interests { get; set; }  
        public string Status { get; set; }     
        public string Referredby { get; set; }  
        public bool? SMSactive { get; set; }
        public string CreatedBy { get; set; }
        public string LastUser { get; set; }    
        public string IPAddress { get; set; }  
        public string Browser { get; set; }
        public string ActivateLink { get; set; } 
    }
}

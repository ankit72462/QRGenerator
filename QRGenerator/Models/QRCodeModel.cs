using System.ComponentModel.DataAnnotations;

namespace QRGenerator.Models
{
    public class QRCodeModel
    {
        [Required]
        [Display(Name = "QR Code Type")]
        public int QRCodeType { get; set; }

        // URL QR code
        [Display(Name = "Website URL")]
        public string WebsiteURL { get; set; }

        // Bookmark QR code
        [Display(Name = "Bookmark URL")]
        public string BookmarkURL { get; set; }

        // SMS QR code
        [Display(Name = "SMS Phone Number")]
        public string SMSPhoneNumber { get; set; }

        [Display(Name = "SMS Body")]
        public string SMSBody { get; set; }

        // WhatsApp QR code
        [Display(Name = "WhatsApp Number")]
        public string WhatsAppNumber { get; set; }

        [Display(Name = "WhatsApp Message")]
        public string WhatsAppMessage { get; set; }

        // Email QR code
        [Display(Name = "Receiver Email Address")]
        public string ReceiverEmailAddress { get; set; }

        [Display(Name = "Email Subject")]
        public string EmailSubject { get; set; }

        [Display(Name = "Email Message")]
        public string EmailMessage { get; set; }

        // WiFi QR code
        [Display(Name = "WiFi Name")]
        public string WIFIName { get; set; }

        [Display(Name = "WiFi Password")]
        public string WIFIPassword { get; set; }
        public string QRImageFileName { get; set; }
        public string QRImageURL { get; set; }
    }
}

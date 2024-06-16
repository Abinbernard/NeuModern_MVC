using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace NeuModern.Models
{
    public class MultipleAddress
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        [DisplayName("Phone Number")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "The phone number must be 10 digits.")]
        public string PhoneNumber { get; set; }
        [Required]
        public string StreetAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]

        public string PostalCode { get; set; }
       
        public string? ApplicationUserId { get; set; }
        public int? Options { get; set; }
    }
}

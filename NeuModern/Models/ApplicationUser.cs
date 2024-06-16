using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NeuModern.Models
{
        public class ApplicationUser : IdentityUser
        {
      
            [Required]
            public string Name { get; set; }
            [Required]
            public string? StreetAddress { get; set; }
            [Required]
            public string? City { get; set; }
            public string? State { get; set; }
            public string? PostalCode { get; set; }
            public int? Wallet { get; set; }
            public bool IsBlocked { get; set; }

        
        }
}

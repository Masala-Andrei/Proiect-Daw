using Microsoft.AspNetCore.Identity;

namespace Proiect_DAW.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }

        public virtual ICollection<Product>? Products { get; set; }

        public virtual ICollection<Order>? Orders { get; set; }

        public virtual ICollection<Review>? Reviews { get; set; }

        public virtual ICollection<UserRating>? UserRatings { get; set; }
    }
}

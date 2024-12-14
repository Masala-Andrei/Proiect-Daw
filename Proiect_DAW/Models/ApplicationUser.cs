using Microsoft.AspNetCore.Identity;

namespace Proiect_DAW.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Product> Products { get; set; }
    }
}

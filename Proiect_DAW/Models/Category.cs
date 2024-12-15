using System.ComponentModel.DataAnnotations;

namespace Proiect_DAW.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
  
        [Required(ErrorMessage = "Numele este obligatoriu")]
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect_DAW.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Titlul este obligatoriu")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Descrierea este obligatorie")]
        public string Description { get; set; }

        //[Required(ErrorMessage = "Poza este obligatorie")]
        //public IFormFile Photo { get; set; }

        [Required(ErrorMessage = "Prețul este obligatoriu")]
        [Range(1, 1000000, ErrorMessage = "Prețul trebuie sa fie mai mare de 0 RON")]
        public int Price { get; set; }

        [Required(ErrorMessage = "Stocul este obligatoriu")]
        public int Stock { get; set; }

        //[Range(1, 5, ErrorMessage = "Rating-ul trebuie să fie între 1 și 5.")]
        public int? Rating { get; set; }



        [Required(ErrorMessage = "Categoria este obligatorie")]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public virtual ICollection<Review>? Reviews { get; set; }

        public virtual ICollection<ProductOrder>? ProductOrders { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }


        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }
    }
}

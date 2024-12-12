using System.ComponentModel.DataAnnotations;

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

        [Required(ErrorMessage = "Poza este obligatorie")]
        public string Photo { get; set; }

        [Required(ErrorMessage = "Prețul este obligatoriu")]
        public string Price { get; set; }

        [Required(ErrorMessage = "Stocul este obligatoriu")]
        public int Stock { get; set; }

        [Range(1, 5, ErrorMessage = "Rating-ul trebuie să fie între 1 și 5.")]
        public int? Rating { get; set; }

        [Required(ErrorMessage = "Categoria este obligatorie")]
        public int CategoryId { get; set; }
        public int UserId { get; set; }
    }
}

using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;


namespace Proiect_DAW.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public int ProductId { get; set; }
        public string? UserId {  get; set; }

        //Un review apartine unui singur user
        public virtual ApplicationUser? User { get; set; }

        //Un review apartine unui singur produs
        public virtual Product? Product { get; set; }
    }
}

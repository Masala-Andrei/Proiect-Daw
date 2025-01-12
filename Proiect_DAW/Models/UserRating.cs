using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect_DAW.Models
{
    public class UserRating
    {
        public int Id { get; set; }
        //public int? ChategoryId { get; set; }
        public string? UserId { get; set; }
        public int? ProductId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating-ul trebuie să fie între 1 și 5.")]
        public int? Number { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual Product? Product { get; set; }

    }
}

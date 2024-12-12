using System.ComponentModel.DataAnnotations;

namespace Proiect_DAW.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
    }
}

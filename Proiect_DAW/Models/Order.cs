using System.ComponentModel.DataAnnotations;

namespace Proiect_DAW.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<ProductOrder> ProductOrders { get; set; }
    }
}

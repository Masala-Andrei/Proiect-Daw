using Proiect_DAW.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proiect_DAW.Models
{
    public class ProductOrder
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int? OrderId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public virtual Product? Product { get; set; }
        public virtual Order? Order { get; set; }
    }
}

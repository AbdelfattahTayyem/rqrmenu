using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace rqrmenu.Areas.Dashboard.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public decimal TotalAmount { get; set; }

        [StringLength(100)]
        public string? CustomerName { get; set; }

        public Guid? TableId { get; set; }

        public Table? Table { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        
        public int? UserId { get; set; }

        public int Quantity { get; set; }
        public Guid ItemId { get; set; }
        public Item? Item { get; set; }


    }

}

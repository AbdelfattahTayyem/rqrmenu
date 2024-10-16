using Newtonsoft.Json;
using System.Collections.Generic;

namespace rqrmenu.Areas.Dashboard.Models
{
    public class OrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();      // Unique identifier
        public Guid ItemId { get; set; }    // Foreign key to Item
        public Guid OrderId { get; set; }   // Foreign key to Order
        public decimal Price { get; set; }  // Price of the item
        public int Quantity { get; set; }   // Quantity ordered

        // Navigation properties
        public virtual Item Item { get; set; } // Navigation property to Item

        [JsonIgnore]  // This prevents circular reference when serializing
        public virtual Order Order { get; set; } // Navigation property to Order

        public virtual ICollection<OrderItemExtra> OrderItemExtras { get; set; } = new List<OrderItemExtra>(); // Linking entity
    }
}

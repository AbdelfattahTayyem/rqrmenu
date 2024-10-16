using System.Collections.Generic;

namespace rqrmenu.Areas.Dashboard.Models
{
    public class Extra
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Guid ItemId { get; set; }
        public virtual Item Item { get; set; }

        public virtual ICollection<OrderItemExtra> OrderItemExtras { get; set; } = new List<OrderItemExtra>(); // Linking entity
    }
}

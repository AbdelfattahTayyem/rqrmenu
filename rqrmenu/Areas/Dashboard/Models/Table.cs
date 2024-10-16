using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rqrmenu.Areas.Dashboard.Models
{
    public class Table
    {
        public Guid Id { get; set; }

        [Required]
        public string TableNumber { get; set; }

        // Navigation property to Orders
        [NotMapped]
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

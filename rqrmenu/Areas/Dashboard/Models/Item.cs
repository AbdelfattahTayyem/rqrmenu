using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rqrmenu.Areas.Dashboard.Models
{
    public partial class Item
    {
        public Guid Id { get; set; } 

        [Required]
        public string Name { get; set; }

        public Guid CategoryId { get; set; }

       
        public Category? Category { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        [StringLength(3)]  // You might limit currency to a 3-letter ISO code
        public string Currency { get; set; }

        public string? Description { get; set; }

        public string? Image { get; set; }

        public ICollection<Extra> Extras { get; set; } = new List<Extra>();
        [NotMapped]
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        
    }

}

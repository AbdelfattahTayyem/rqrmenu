using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rqrmenu.Areas.Dashboard.Models
{
    public class Category
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Img { get; set; }

        // Navigation property to access items of this category
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }

}

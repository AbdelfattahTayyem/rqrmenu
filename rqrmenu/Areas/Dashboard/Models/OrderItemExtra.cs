using Newtonsoft.Json;
using System;

namespace rqrmenu.Areas.Dashboard.Models
{
    public class OrderItemExtra
    {
        public Guid OrderItemId { get; set; }

        [JsonIgnore] // This prevents circular reference when serializing
        public OrderItem OrderItem { get; set; }

        public Guid ExtraId { get; set; }
        public Extra Extra { get; set; }
    }
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QRCoder;
using rqrmenu.Areas.Dashboard.Data;
using rqrmenu.Areas.Dashboard.Models;
using rqrmenu.Models;
using System.Diagnostics;
using System.Drawing;

namespace rqrmenu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult GenerateQRCode(Guid tableId)
        {
            string tableUrl = $"https://localhost:44308/order?tableId={tableId}";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(tableUrl, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
            {
                using (var stream = new MemoryStream())
                {
                    qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return File(stream.ToArray(), "image/png");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index(string table)
        {
            if (Guid.TryParse(table, out Guid tableId))
            {
                HttpContext.Session.SetString("TableId", tableId.ToString());
            }
            else
            {
                ModelState.AddModelError("", "Invalid table ID.");
            }

            var categories = await _context.Category.Include(i=>i.Items).ToListAsync();
            return View(categories);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Cart()
        {
            var cartItems = CartHelper.GetCartItems(HttpContext.Session);
            return View(cartItems);
        }
        [HttpPost]
        public IActionResult AddToCart(int itemId, string itemName, string imageUrl, decimal price)
        {
            var item = new CartItem
            {
                ItemId = itemId,
                ItemName = itemName,
                ImageUrl = imageUrl,
                Price = price,
                Quantity = 1
            };

            // Add the item to the cart
            AddToCart(HttpContext.Session, item);

            // Return updated cart summary
            var cart = GetCart(HttpContext.Session);
            var totalItems = cart.Sum(i => i.Quantity); // Example: total items in cart

            return Json(new { success = true, message = "Item added to cart", totalItems = totalItems });
        }

        public static List<CartItem> GetCart(ISession session)
        {
            var cartJson = session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }
            return JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
        }

        public static void SaveCart(ISession session, List<CartItem> cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            session.SetString("Cart", cartJson);
        }

        public static void AddToCart(ISession session, CartItem item)
        {
            var cart = GetCart(session); // Retrieve the cart from the session
            var existingItem = cart.FirstOrDefault(i => i.ItemId == item.ItemId);

            if (existingItem != null)
            {
                existingItem.Quantity += 1; // Increment quantity if item exists
            }
            else
            {
                cart.Add(item); // Add new item if it doesn't exist
            }

            SaveCart(session, cart); // Save updated cart to session
        }






        [HttpPost]
        public IActionResult UpdateCart(int itemtId, int quantity)
        {
            CartHelper.UpdateQuantity(HttpContext.Session, itemtId, quantity);
            return RedirectToAction("Cart");
        }
        [HttpPost]
        public IActionResult RemoveFromCart(int itemtId)
        {
            CartHelper.RemoveFromCart(HttpContext.Session, itemtId);
            return RedirectToAction("Cart");
        }


        [HttpPost]
        public IActionResult ApplyCoupon(string couponCode, decimal totalAmount)
        {
            decimal discount = 0.00M;

            if (!string.IsNullOrEmpty(couponCode))
            {
                if (couponCode.Equals("Abdu0zd", StringComparison.OrdinalIgnoreCase))
                {
                    discount = totalAmount;
                }
                else if (couponCode.Equals("Farah", StringComparison.OrdinalIgnoreCase))
                {
                    discount = totalAmount * 0.10m;
                }
            }

            decimal finalAmount = totalAmount - discount;

            TempData["Discount"] = discount;

            if (finalAmount == 0.00M)
            {
                TempData["FinalAmount"] = "Free";  
            }
            else
            {
                TempData["FinalAmount"] = finalAmount.ToString("C");
            }

            return RedirectToAction("Checkout");
        }



        [HttpGet]
        public IActionResult Checkout(Guid tableId)
        {
            var orderJson = HttpContext.Session.GetString("Order");
            var orderStartTimeString = HttpContext.Session.GetString("OrderStartTime");

            if (string.IsNullOrEmpty(orderJson) || string.IsNullOrEmpty(orderStartTimeString))
            {
                TempData["CheckoutError"] = "No active order found or your session has expired.";
                return RedirectToAction("Shop");
            }

            DateTime orderStartTime = DateTime.Parse(orderStartTimeString);
            TimeSpan timeElapsed = DateTime.Now - orderStartTime;

            if (timeElapsed.TotalMinutes > 15)
            {
                HttpContext.Session.Remove("Order");
                HttpContext.Session.Remove("OrderStartTime");
                TempData["CheckoutError"] = "Your session expired. Please place your order again.";
                return RedirectToAction("Shop");
            }

            var order = JsonConvert.DeserializeObject<Order>(orderJson);

            if (order == null)
            {
                TempData["CheckoutError"] = "There was an error retrieving your order. Please try again.";
                return RedirectToAction("Shop");
            }

            return View(new List<Order> { order });
        }

        [HttpPost]
        public async Task<IActionResult> CheckoutConfirm(Guid tableId)
        {
            var orderJson = HttpContext.Session.GetString("Order");

            if (string.IsNullOrEmpty(orderJson))
            {
                TempData["CheckoutError"] = "No active order to confirm.";
                return RedirectToAction("Shop");
            }

            var order = JsonConvert.DeserializeObject<Order>(orderJson);

            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                HttpContext.Session.Remove("Order");
                HttpContext.Session.Remove("OrderStartTime");
                TempData["CheckoutSuccess"] = "Your order has been placed successfully!";
                return RedirectToAction("CheckoutSuccess");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming checkout.");
                TempData["CheckoutError"] = "There was an error confirming your order. Please try again.";
                return RedirectToAction("Checkout");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Shop_Details(Guid id)
        {
            var tableIdString = HttpContext.Session.GetString("TableId");
            Guid? tableId = string.IsNullOrEmpty(tableIdString) ? (Guid?)null : Guid.Parse(tableIdString);
            ViewBag.TableId = tableId;

            var item = await _context.Item
                .Include(i => i.Category)
                .Include(i => i.Extras)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Shop_Details(Guid id, List<Guid> selectedExtras, int quantity)
        {
            var item = await _context.Item
                .Include(i => i.Category)
                .Include(i => i.Extras)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            var tableIdString = HttpContext.Session.GetString("TableId");
            if (string.IsNullOrEmpty(tableIdString) || !Guid.TryParse(tableIdString, out Guid tableId))
            {
                ModelState.AddModelError("", "Table ID is missing. Please scan the QR code again.");
                return View(item);
            }

            decimal totalItemPrice = item.Price * quantity;
            decimal totalExtraPrice = 0;

            var orderItem = new OrderItem
            {
                ItemId = item.Id,
                Quantity = quantity,
                Price = totalItemPrice,
                OrderItemExtras = new List<OrderItemExtra>()
            };

            if (selectedExtras != null && selectedExtras.Any())
            {
                foreach (var extraId in selectedExtras)
                {
                    var extra = await _context.Extra.FindAsync(extraId);
                    if (extra != null)
                    {
                        var orderItemExtra = new OrderItemExtra
                        {
                            OrderItem = orderItem,
                            ExtraId = extra.Id
                        };
                        orderItem.OrderItemExtras.Add(orderItemExtra);
                        totalExtraPrice += extra.Price;
                    }
                }
            }

            decimal totalAmount = totalItemPrice + totalExtraPrice;

            var order = new Order
            {
                TableId = tableId,
                TotalAmount = totalAmount,
                OrderItems = new List<OrderItem> { orderItem }
            };

            HttpContext.Session.SetString("Order", JsonConvert.SerializeObject(order));
            HttpContext.Session.SetString("OrderStartTime", DateTime.Now.ToString());

            return RedirectToAction("Checkout", new { tableId });
        }

        public async Task<IActionResult> Shop()
        {
            return View(await _context.Item.ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult CheckoutSuccess()
        {
            return View();
        }
    }
}

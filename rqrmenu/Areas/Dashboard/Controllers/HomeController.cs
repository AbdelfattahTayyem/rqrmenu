using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Execution;
using Microsoft.EntityFrameworkCore;
using rqrmenu.Areas.Dashboard.Data;
using rqrmenu.Areas.Dashboard.Models;


namespace rqrmenu.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
   
    public class HomeController : Controller
    {

        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }



        public IActionResult Index()
        {
            return View();
        }
        #region Category
        public async Task<IActionResult> Category()
        {
            return View(await _context.Category.ToListAsync());
        }
        [HttpGet]
        public IActionResult add_category()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> add_category(Category category, IFormFile Img)
        {
            if (ModelState.IsValid)
            {
                if (Img != null && Img.Length > 0)
                {
                    var fileName = Path.GetFileName(Img.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Img.CopyToAsync(stream);
                    }

                    category.Img = fileName;
                }

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Category));
            }
            return View(category);
        }



        [HttpGet]
        public async Task<IActionResult> Edit_Cat(Guid id)
        {
            
            var item = await _context.Category.FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            // Prepare ViewModel if needed
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit_Cat(Category category, IFormFile? Img)
        {
            if (category == null || category.Id == Guid.Empty)
            {
                return NotFound();
            }
            
            var existingItem = await _context.Category.FirstOrDefaultAsync(i => i.Id == category.Id);
            if (existingItem == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Handle Image upload
                if (Img != null && Img.Length > 0)
                {
                    var fileName = Path.GetFileName(Img.FileName);
                    var uniqueFileName = Guid.NewGuid() + "_" + fileName;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Img.CopyToAsync(stream);
                    }

                    // Update the item's image with the new file name
                    existingItem.Img = uniqueFileName;
                }
                else
                {
                    category.Img = existingItem.Img;
                }


                existingItem.Name = category.Name;

                // Save changes to the database
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Category));
            }
            var categories = _context.Category.ToList();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
            return View(Category);
        }


        [HttpGet]
        public async Task<IActionResult> Delete_Cat(Guid id)
        {
            var cat = await _context.Category.FirstOrDefaultAsync(i => i.Id == id);
            if (cat == null)
            {
                return NotFound();
            }

            return View(cat);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmedCat(Guid id)
        {
            var cat = await _context.Category.FirstOrDefaultAsync(i => i.Id == id);
            if (cat == null)
            {
                return NotFound();
            }
            var items = _context.Item.Where(e => e.CategoryId == id).ToList();
            var itemIds = items.Select(i => i.Id).ToList();

            var extras = _context.Extra.Where(e => itemIds.Contains(e.ItemId)).ToList();

            _context.Extra.RemoveRange(extras);

           
            _context.Item.RemoveRange(items);

            _context.Category.Remove(cat);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Category));
        }


        #endregion





        public async Task<IActionResult> table()
        {
            return View(await _context.Table.ToListAsync());
        }
        [HttpGet]
        public IActionResult add_table()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> add_table(Table table)
        {
            if (ModelState.IsValid)
            {
                _context.Add(table);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(table));
            }
            return View(table);
        }

        public async Task<IActionResult> order()
        {
            var order = await _context.Orders.Include(i => i.Item).ToListAsync();

            return View(order);
        }

        public async Task<IActionResult> items()
        {
            var items = await _context.Item.Include(i => i.Category).Include(i => i.Extras).ToListAsync();

            return View(items);
        }

    
    [HttpGet]
        public IActionResult add_item()
        {
            var categories = _context.Category.ToList();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> add_item(Item item, IFormFile Image, string[] ExtraItemName, string[] ExtraItemPrice)
        {
            if (ModelState.IsValid)
            {
                if (Image != null && Image.Length > 0)
                {
                    var fileName = Path.GetFileName(Image.FileName);
                    var uniqueFileName = Guid.NewGuid() + "_" + fileName;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(stream);
                    }

                    item.Image = uniqueFileName;
                }

                _context.Add(item);
                await _context.SaveChangesAsync();

                for (int i = 0; i < ExtraItemName.Length; i++)
                {
                    var extraItem = new Extra
                    {
                        Name = ExtraItemName[i],
                        Price = decimal.Parse(ExtraItemPrice[i]),
                        ItemId = item.Id
                    };

                    _context.Extra.Add(extraItem);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(items));
            }
            var categories = _context.Category.ToList();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Edit_item(Guid id)
        {
            var categories = _context.Category.ToList();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
            var item = await _context.Item.Include(i => i.Extras).FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            // Prepare ViewModel if needed
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit_item(Item item, IFormFile? Image, string[] ExtraItemName, string[] ExtraItemPrice)
        {
            if (item == null || item.Id == Guid.Empty)
            {
                return NotFound();
            }

            var existingItem = await _context.Item.Include(i => i.Extras).FirstOrDefaultAsync(i => i.Id == item.Id);
            if (existingItem == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Handle Image upload
                if (Image != null && Image.Length > 0)
                {
                    var fileName = Path.GetFileName(Image.FileName);
                    var uniqueFileName = Guid.NewGuid() + "_" + fileName;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(stream);
                    }

                    // Update the item's image with the new file name
                    existingItem.Image = uniqueFileName;
                }
                else
                {
                    item.Image = existingItem.Image;
                }


                // Update other item properties
                existingItem.Name = item.Name;
                existingItem.CategoryId = item.CategoryId;
                existingItem.Price = item.Price;
                existingItem.Currency = item.Currency;

                // Clear existing extras and add new extras
                existingItem.Extras.Clear();

                for (int i = 0; i < ExtraItemName.Length; i++)
                {
                    if (!string.IsNullOrEmpty(ExtraItemName[i]) && !string.IsNullOrEmpty(ExtraItemPrice[i]))
                    {
                        // Validate and parse the price
                        if (decimal.TryParse(ExtraItemPrice[i], out decimal price))
                        {
                            var extraItem = new Extra
                            {
                                Name = ExtraItemName[i],
                                Price = price,
                                ItemId = existingItem.Id
                            };

                            existingItem.Extras.Add(extraItem);
                        }
                        else
                        {
                            ModelState.AddModelError("Price", "Invalid price format.");

                            return View(item);
                        }
                    }
                }

                // Save changes to the database
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(items));
            }
            var categories = _context.Category.ToList();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
            return View(item);
        }


        [HttpGet]
        public async Task<IActionResult> Delete_Item(Guid id)
        {
            var item = await _context.Item.Include(i => i.Category).FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var item = await _context.Item.Include(i => i.Extras).FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            var extras = _context.Extra.Where(e => e.ItemId == id);
            _context.Extra.RemoveRange(extras); 

            _context.Item.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(items));
        }


        public IActionResult contactus()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit_Order(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)  // Ensure OrderItems are included
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // If OrderItems is null, initialize it to an empty list
            order.OrderItems ??= new List<OrderItem>();

            ViewBag.Tables = new SelectList(await _context.Table.ToListAsync(), "Id", "Name");
            ViewBag.Items = new SelectList(await _context.Item.ToListAsync(), "Id", "Name");

            return View(order);
        }



        [HttpPost]
        public async Task<IActionResult> Edit_Order(Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Redirect to the index or another relevant action
            }

            // If we get here, something went wrong; re-display the form
            return View(order);
        }
    }








}


using Microsoft.AspNetCore.Mvc;
using Purely_Nuts.Models;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Purely_Nuts.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AdminController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<AdminController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            var products = _context.Product.ToList();
            return View(products);
        }


        public IActionResult Dashboard()
        {
            ViewBag.TotalProducts = _context.Product.Count();
            ViewBag.TotalSales = _context.Order.Sum(o => o.TotalPrice);
            ViewBag.TotalOrders = _context.Order.Count();
            ViewBag.LatestOrder = _context.Order.OrderByDescending(o => o.Date).FirstOrDefault();

            return View();
        }

        public IActionResult Add()
        {
            return View(new Product());
        }

        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile file)
        {
            _logger.LogInformation("Starting to process the Add method.");

            try
            {
                if (_context.Product.Any(u => u.ProductName == product.ProductName))
                {
                    ModelState.AddModelError("", "Product already exists.");
                    _logger.LogWarning("Product already exists with the name: {ProductName}", product.ProductName);
                    return View(product);
                }

                if (file != null && file.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        product.ImageData = memoryStream.ToArray();
                    }

                    product.ProductImageName = file.FileName;

                    // Log the description to verify it's not null
                    _logger.LogInformation("Product description: {ProductDescription}", product.ProductDescription);

                    _context.Product.Add(product);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Product added successfully with ID: {ProductId}", product.ProductId);
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ModelState.AddModelError("", "An image file must be uploaded.");
                    return View(product);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a new product.");
                ModelState.AddModelError("", "An error occurred while processing your request: " + ex.Message);
                return View(product);
            }
        }


        public IActionResult ViewProducts()
        {
            return View(_context.Product.ToList());
        }

        public IActionResult Edit()
        {
            var products = _context.Product.ToList();
            return View(products);
        }

        [HttpPost]
        public IActionResult Edit(int id)
        {
            var product = _context.Product.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }


        [HttpPost]
        public async Task<IActionResult> Update(Product product, IFormFile file)
        {

            var existingProduct = _context.Product.Find(product.ProductId);
            if (existingProduct == null)
            {
                return NotFound();
            }

            if (file != null && file.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    existingProduct.ImageData = memoryStream.ToArray();  // Update the image data
                }
                existingProduct.ProductImageName = file.FileName;
            }

            // Update other properties
            existingProduct.ProductName = product.ProductName;
            existingProduct.ProductPrice = product.ProductPrice;
            existingProduct.Quantity = product.Quantity;
            existingProduct.ProductDescription = product.ProductDescription;

            _context.Update(existingProduct);
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewProducts");
        }

        public IActionResult Delete()
        {
            var products = _context.Product.ToList();
            return View(products);
        }

        [HttpPost]
        public IActionResult Delete(int productId)
        {
            var product = _context.Product.Find(productId);
            if (product == null)
            {
                return NotFound();
            }

            _context.Product.Remove(product);
            _context.SaveChanges();

            return RedirectToAction(nameof(ViewProducts));
        }

        private async Task<bool> ProcessProductData(Product product, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    product.ImageData = memoryStream.ToArray();
                }
                product.ProductImageName = file.FileName;
                return true;
            }
            ModelState.AddModelError("", "An image file must be uploaded.");
            return false;
        }
    }
}

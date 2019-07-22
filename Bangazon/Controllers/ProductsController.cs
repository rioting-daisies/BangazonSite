using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Bangazon.Models.ProductViewModels;

namespace Bangazon.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // This method will be called every time we need to get the current user
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        // The product index view has been changed to show the 20 products needed for the homepage model. 
        // The index controller also adds a search bar, with a dropdown filter to allow searches by city,
        // product name, or both.
        // GET: Products
        public async Task<IActionResult> Index(string searchString, string SearchBar)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["SearchBar"] = SearchBar;

            var currentUser = GetCurrentUserAsync().Result;

            var applicationDbContext = _context.Product.Include(p => p.ProductType).Include(p => p.User);
            var products = applicationDbContext.OrderByDescending(p => p.DateCreated).Take(20);

            if (!String.IsNullOrEmpty(searchString))
            {
                switch (SearchBar)
                {
                    case "1":
                        products = products.Where(p => p.City.ToUpper().Contains(searchString.ToUpper()));
                        break;
                    case "2":
                        products = products.Where(p => p.Title.ToUpper().Contains(searchString.ToUpper()));
                        break;
                    default:
                        products = products.Where(p => p.Title.ToUpper().Contains(searchString.ToUpper())
                                       || p.City.ToUpper().Contains(searchString.ToUpper()));
                        break;
                }

            }

            return View(await products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductType)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        // user must be authorized to create a product that they want to sell
        [Authorize]
        public async Task<IActionResult> Create()
        {
            //ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label");
            //ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");

            var viewModel = new ProductCreateViewModel
            {
                AvailableProductTypes = await _context.ProductType.ToListAsync(),
            };
            return View(viewModel);
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ProductCreateViewModel viewModel)
        {
            //ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label", product.ProductTypeId);
            //ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", product.UserId);
            ModelState.Remove("Product.ProductType");
            ModelState.Remove("Product.User");
            ModelState.Remove("Product.UserId");

            if (ModelState.IsValid)
            {
                var product = viewModel.Product;
                var currUser = await GetCurrentUserAsync();
                product.UserId = currUser.Id;
                _context.Add(product);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            viewModel.AvailableProductTypes = await _context.ProductType.ToListAsync();
            return View(viewModel);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label", product.ProductTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", product.UserId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,DateCreated,Description,Title,Price,Quantity,UserId,City,ImagePath,Active,ProductTypeId")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label", product.ProductTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", product.UserId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductType)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Types()
        {
            var model = new ProductTypesViewModel
            {

                // Build list of Product instances for display in view
                // LINQ is awesome
                GroupedProducts = await (
                from t in _context.ProductType
                join p in _context.Product
                on t.ProductTypeId equals p.ProductTypeId
                group new { t, p } by new { t.ProductTypeId, t.Label } into grouped
                select new GroupedProducts
                {
                    TypeId = grouped.Key.ProductTypeId,
                    TypeName = grouped.Key.Label,
                    ProductCount = grouped.Select(x => x.p.ProductId).Count(),
                    Products = grouped.Select(x => x.p).Take(3)
                }).ToListAsync(),
                ProductTypes = await (_context.ProductType.ToListAsync())
            };

            return View(model);
        }
        [Authorize]
        public async Task<IActionResult> MyProducts()
        {
            var currentUser = GetCurrentUserAsync().Result;
            var applicationDbContext = _context.Product.Include(p => p.ProductType).Include(p => p.User);
            var products = applicationDbContext.Where(p => p.UserId == currentUser.Id).OrderByDescending(p => p.DateCreated);

            return View(await products.ToListAsync());
        }

        //this is the delete method that will delete a product from the user's shopping cart

        public async Task<IActionResult>DeleteShoppingCartItem(){

        }
        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}

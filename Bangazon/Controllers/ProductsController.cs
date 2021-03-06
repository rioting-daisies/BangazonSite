﻿using System;
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
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Bangazon.ViewComponents;

namespace Bangazon.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHostingEnvironment hostingEnvironment;

        public ProductsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _userManager = userManager;
            this.hostingEnvironment = hostingEnvironment;

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

            int count = await GetItemsAsync(product.ProductId);

            ViewBag.ProductCount = count;

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
                string uniqueFileName = null;

                // If the Photo property on the incoming model object is not null, then the user
                // has selected an image to upload.
                if (viewModel.Photo != null)
                {
                    // The image must be uploaded to the images folder in wwwroot
                    // To get the path of the wwwroot folder we are using the inject
                    // HostingEnvironment service provided by ASP.NET Core
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                    // To make sure the file name is unique we are appending a new
                    // GUID value and and an underscore to the file name
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    // Use CopyTo() method provided by IFormFile interface to
                    // copy the file to wwwroot/images folder
                    viewModel.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
                }

                var product = viewModel.Product;
                var currUser = await GetCurrentUserAsync();
                product.ImagePath = uniqueFileName;
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

            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,DateCreated,Description,Title,Price,Quantity,UserId,City,ImagePath,Active,ProductTypeId")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }
            ModelState.Remove("User");
            ModelState.Remove("ProductTypeId");
            ModelState.Remove("Title");
            ModelState.Remove("Description");
            ModelState.Remove("UserId");
            var productToUpdate = await _context.Product.FindAsync(id);
            productToUpdate.Quantity = product.Quantity;
            var currentUser = await GetCurrentUserAsync();

            if (ModelState.IsValid)
            {
                try
                {
                    productToUpdate.User = currentUser;
                    _context.Update(productToUpdate);
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
                return RedirectToAction("Details", "Products", new { id });
            }

            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize]
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
        [Authorize]
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
            var currentUser = await GetCurrentUserAsync();
            var products = await _context.Product.Include(p => p.ProductType)
                .Include(p => p.User)
                .Where(p => p.User == currentUser)
                .OrderByDescending(p => p.DateCreated).ToListAsync();
            var orderProducts = await _context.OrderProduct
                                        .Include(op => op.Order)
                                        .Where(op => op.Order.PaymentTypeId != null).ToListAsync();

            var model = new ProductListViewModel()
            {
                ProductsWithSales = (
                from op in orderProducts
                join p in products
                on op.ProductId equals p.ProductId
                group new { op, p } by new { op.ProductId, p } into grouped
                select new ProductDetailViewModel
                {
                    Product = grouped.Key.p,
                    UnitsSold = grouped.Select(x => x.p.ProductId).Count()
                }).ToList()
            };

            foreach(Product p in products)
            {
                if (!model.ProductsWithSales.Exists(ps => ps.Product.ProductId == p.ProductId))
                {
                    model.ProductsWithSales.Add(new ProductDetailViewModel { Product = p, UnitsSold = 0 }); 
                }
            }

            return View(model);
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }

        private async Task<int> GetItemsAsync(int productId)
        {
            var user = await GetCurrentUserAsync();

            // See if the user has an open order
            var openOrder = await _context.Order.SingleOrDefaultAsync(o => o.User == user && o.PaymentTypeId == null);

            var product = await _context.Product.Where(p => p.ProductId == productId).SingleOrDefaultAsync();

            if (openOrder == null)
            {
                return product.Quantity;
            }
            var cartList = await _context.OrderProduct.Where(op => op.OrderId == openOrder.OrderId && op.ProductId == product.ProductId).ToListAsync();
            int productCount = product.Quantity - cartList.Count();

            return productCount;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bangazon.Models;
using Microsoft.AspNetCore.Identity;
using Bangazon.Data;

namespace Bangazon.ViewComponents
{
    public class ProductCountViewModel
    {
        public int ProductCount { get; set; }
    }


    public class ProductCountViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ApplicationDbContext _context;

        public ProductCountViewComponent(ApplicationDbContext context,
                                UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        // This method will be called every time we need to get the current user
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public async Task<IViewComponentResult> InvokeAsync(int productId)
        {
            var quantityRemaining = await GetItemsAsync(productId);
            ProductCountViewModel model = new ProductCountViewModel
            {
                ProductCount = quantityRemaining
            };


            return View(model);
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
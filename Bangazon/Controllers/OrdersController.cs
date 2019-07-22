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
using Bangazon.Models.OrderViewModels;

namespace Bangazon.Controllers
{
    public class OrdersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context,
                                UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        // This method will be called every time we need to get the current user
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        // GET: Orders
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var currentUser = await GetCurrentUserAsync();
            var applicationDbContext = _context.Order.Include(o => o.PaymentType).Include(o => o.User).Where(o=> o.UserId == currentUser.Id);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Orders/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {

            var currentuser = await GetCurrentUserAsync();

            var order = await _context.Order
                .Include(o => o.PaymentType)
                .Include(o => o.User)
                .Include(o => o.OrderProducts)
                 .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(m => m.UserId == currentuser.Id.ToString() && m.PaymentTypeId == null);

            if (order == null || order.OrderProducts.Count() == 0)
            {
                return View("EmptyCart");
            }


            OrderDetailViewModel viewmodel = new OrderDetailViewModel
            {
                Order = order
            };

            OrderLineItem LineItem = new OrderLineItem();

            viewmodel.LineItems = order.OrderProducts
                 .GroupBy(op => op.Product)
                 .Select(p => new OrderLineItem
                 {
                     Product = p.Key,
                     Units = p.Select(l => l.Product).Count(),
                     Cost = p.Key.Price * p.Select(l => l.ProductId).Count()
                 }).ToList();

            if (order == null)
            {
                return NotFound();
            } 

            return View(viewmodel);
        }

        // GET: Orders/Create
        //public IActionResult Create()
        //{
        //    ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber");
        //    ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");
        //    return View();
        //}

        //// POST: Orders/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("OrderId,DateCreated,DateCompleted,UserId,PaymentTypeId")] Order order)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(order);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
        //    ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
        //    return View(order);
        //}

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            var paymentTypes = _context.PaymentType.Where(p => p.UserId == order.UserId);

            if (paymentTypes.Count() == 0)
            {
                return RedirectToAction("Create", "PaymentTypes");
            }

            ViewData["PaymentTypeId"] = new SelectList(paymentTypes, "PaymentTypeId", "Description", order.PaymentTypeId);

            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,DateCreated,DateCompleted,UserId,PaymentTypeId")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }
            var user = await GetCurrentUserAsync();
            ModelState.Remove("User");
            ModelState.Remove("UserId");
            ModelState.Remove("DateCompleted");

            if (ModelState.IsValid)
            {
                try
                {
                    order.DateCompleted = DateTime.Now;
                    order.UserId = user.Id;
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "Description", order.PaymentTypeId);

            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.PaymentType)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id);
            _context.Order.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [Authorize]
        public async Task<IActionResult> AddToCart([FromRoute] int id)
        {
            // Find the product requested
            Product productToAdd = await _context.Product.SingleOrDefaultAsync(p => p.ProductId == id);

            // Get the current user
            var user = await GetCurrentUserAsync();

            // See if the user has an open order
            var openOrder = await _context.Order.SingleOrDefaultAsync(o => o.User == user && o.PaymentTypeId == null);

            Order currentOrder = new Order();
            // If no order, create one, else add to existing order

            if (openOrder == null)
            {

                currentOrder.UserId = user.Id;
                currentOrder.PaymentType = null;
                _context.Add(currentOrder);
                await _context.SaveChangesAsync();



            }
            else
            {
                currentOrder = openOrder;
            }

            OrderProduct item = new OrderProduct();

            productToAdd.Quantity = productToAdd.Quantity - 1;
            item.OrderId = currentOrder.OrderId;
            item.ProductId = productToAdd.ProductId;


            /*currentOrder.OrderProducts.Add(item);*/
            _context.Add(item);

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Products", new { id = productToAdd.ProductId });
        }
        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderId == id);
        }

       
    }


/*
 * 
 *   Just in case we need later.  was in Order Detail.  -JH n JE
 if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.PaymentType)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }*/

}
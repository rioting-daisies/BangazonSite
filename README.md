# Welcome to Bangazon!

## Overview

This version of Bangazon implements the Identity framework, and extends the base User object with the `ApplicationUser` model.
It shows how to remove a model's property from the automatic model binding in a controller method by using `ModelState.Remove()`.

## Setup for Everyone

Once the initial setup is complete by the volunteer and the PR is approved by your team lead, the PR will get merged into master and now everyone else can pull the repository.

1. Open Visual Studio and load the solution file
1. Go to the Package Manager Console in Visual Studio.
1. Execute `Update-Database` to generate your tables.
1. Use the SQL Server Object Explorer to verify that everything worked as expected.

## References for Tickets

### Accessing the Authenticated User

In any of your controllers that need to access the currently authenticated user, for example the Order Summary screen or the New Product Form, you need to inject the `UserManager` into the controller. Here's the relevant code that you need.

Add a private field to your controller.

```cs
private readonly UserManager<ApplicationUser> _userManager;
```

In the constructor, inject the `UserManager` service.

```cs
public ProductsController(ApplicationDbContext ctx,
                          UserManager<ApplicationUser> userManager)
{
    _userManager = userManager;
    _context = ctx;
}
```

Then add the following private method to the controller.

```cs
private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
```

Once that is defined, any method that needs to see who the user is can invoke the method. Here's an example of when someone clicks the Purchase button for a product.

```cs
[Authorize]
public async Task<IActionResult> Purchase([FromRoute] int id)
{
    // Find the product requested
    Product productToAdd = await _context.Product.SingleOrDefaultAsync(p => p.ProductId == id);

    // Get the current user
    var user = await GetCurrentUserAsync();

    // See if the user has an open order
    var openOrder = await _context.Order.SingleOrDefaultAsync(o => o.User == user && o.PaymentTypeId == null);


    // If no order, create one, else add to existing order
}
```

### Grouped Products by Product Type

One of the features you need to implement is a view that displays all of the product types as headers, with the first three products in that type listed beneath it. We are providing you a LINQ statement that will get you started.

Whomever tackles that ticket, this is the method that you will need to add to your `ProductsController.cs`.

```cs
public async Task<IActionResult> Types()
{
    var model = new ProductTypesViewModel();

    // Build list of Product instances for display in view
    // LINQ is awesome
    model.GroupedProducts = await (
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
        }).ToListAsync();

    return View(model);
}
```

In addition to that, add the following custom route to the bottom of your `Startup.cs` file.

```cs
routes.MapRoute ("types", "types",
    defaults : new { controller = "Products", action = "Types" });
```

## Removing Items from Model Validation

One of the features you must implement is allowing customers to add products to sell. You'll need to remove the user from model validation to get it to work. Here's an example of something your team will need to do in `Create()` method in **`ProductsController`**.

```cs
// Remove the user from the model validation because it is
// not information posted in the form
ModelState.Remove("product.User");
```


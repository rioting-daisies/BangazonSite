using System.Collections.Generic;
using Bangazon.Models;
using Bangazon.Data;

namespace Bangazon.Models.ProductViewModels
{
  public class ProductListViewModel
  {
        public List<ProductDetailViewModel> ProductsWithSales { get; set; }

        public readonly Product Product;

  }
}
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon.Models.ProductViewModels
{
    public class ProductCreateViewModel
    {
        public Product Product { get; set; }

        public List<ProductType> AvailableProductTypes { get; set; }

        public List<SelectListItem> ProductTypeOptions => AvailableProductTypes?.Select(p => new SelectListItem(p.Label, p.ProductTypeId.ToString())).ToList();

    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bangazon.Models.ProductViewModels
{
    public class ProductCreateViewModel
    {
        public Product Product { get; set; }

        public IFormFile Photo { get; set; }

        public List<ProductType> AvailableProductTypes { get; set; }

        public List<SelectListItem> ProductTypeOptions
        {
            get
            {
                if(AvailableProductTypes == null)
                {
                    return null;
                }
                var pt = AvailableProductTypes?.Select(p => new SelectListItem(p.Label, p.ProductTypeId.ToString())).ToList();
                pt.Insert(0, new SelectListItem("Select Product Type", null));

                return pt;
            }
        }

        
    }
}

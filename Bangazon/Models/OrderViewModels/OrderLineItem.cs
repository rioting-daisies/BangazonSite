using System.ComponentModel.DataAnnotations;

namespace Bangazon.Models.OrderViewModels {
    public class OrderLineItem {
        public Product Product { get; set; }
        public int Units { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public double Cost { get; set; }
    }
}
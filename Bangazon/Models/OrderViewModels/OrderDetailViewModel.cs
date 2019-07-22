using System.Collections.Generic;

namespace Bangazon.Models.OrderViewModels
{
    public class OrderDetailViewModel
    {
        public Order Order { get; set; }

        public OrderProduct OrderProduct { get; set; }

        public List<OrderLineItem> LineItems { get; set; }

        public readonly Product Product;

    }
}
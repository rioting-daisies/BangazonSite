using System.Collections.Generic;

namespace Bangazon.Models.OrderViewModels
{
    public class OrderDetailViewModel
    {
        public Order Order { get; set; }

        public OrderProduct OrderProduct { get; set; }

        public IEnumerable<OrderLineItem> LineItems { get; set; }

    }
}
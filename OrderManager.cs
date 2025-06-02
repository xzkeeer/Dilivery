using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestDelivery;

namespace Dilivery
{
    public class OrderManager
    {
        private List<Order> _orders = new List<Order>();

        public IEnumerable<Order> Orders => _orders;

        public void LoadOrders(Order[] orders)
        {
            _orders = orders.ToList();
        }

        public void AddOrder(Point destination, double priority)
        {
            int newId = _orders.Count > 0 ? _orders.Max(o => o.ID) + 1 : 1;
            _orders.Add(new Order
            {
                ID = newId,
                Destination = destination,
                Priority = priority
            });
        }

        public string GetOrderInfo(Order order)
        {
            string priorityLabel = order.Priority >= 0.9 ? "Высокий" :
                                order.Priority >= 0.4 ? "Средний" :
                                "Низкий";

            return $"Заказ #{order.ID}: Координаты ({order.Destination.X:F4}, {order.Destination.Y:F4}), " +
                   $"Приоритет: {priorityLabel}";
        }
    }
}

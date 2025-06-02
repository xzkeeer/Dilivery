using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BestDelivery;

namespace Dilivery
{
    public class RouteManager
    {
        private readonly Point _depotLocation;

        public RouteManager(Point depotLocation)
        {
            _depotLocation = depotLocation;
        }

        public (List<int> route, double cost) CalculateRoute(IEnumerable<Order> orders)
        {
            var solver = new DijkstraDeliverySolver(_depotLocation, orders.ToList());
            var route = solver.FindOptimalRoute();

            bool isValid = RoutingTestLogic.TestRoutingSolution(
                _depotLocation,
                orders.ToArray(),
                route.ToArray(),
                out var cost);

            return (isValid ? route : new List<int>(), cost);
        }

        public string GetRouteInfo(IEnumerable<int> route, double cost, int ordersCount)
        {
            return $"ИНФОРМАЦИЯ О МАРШРУТЕ:\n" +
                   $"Количество заказов: {ordersCount}\n" +
                   $"Последовательность: {string.Join(" → ", route)}\n" +
                   $"Общая стоимость маршрута: {cost:F2} \n";
        }
    }
}

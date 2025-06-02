using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BestDelivery;
using Point = BestDelivery.Point;

namespace Dilivery
{
    public partial class MainWindow : Window
    {
        private readonly MapVisualizer _mapVisualizer;
        private readonly OrderManager _orderManager;
        private readonly RouteManager _routeManager;
        private readonly Point _depotLocation = new Point { X = 55.7558, Y = 37.6173 };

        public MainWindow()
        {
            InitializeComponent();
            _mapVisualizer = new MapVisualizer(PointsCanvas);
            _orderManager = new OrderManager();
            _routeManager = new RouteManager(_depotLocation);

            InitializeComboBox();
            LoadOrders(OrderArrays.GetOrderArray1());

            PointsCanvas.MouseLeftButtonDown += PointsCanvas_MouseLeftButtonDown;
        }


        private void InitializeComboBox()
        {
            OrderComboBox.Items.Add("Набор заказов 1 (3 точки)");
            OrderComboBox.Items.Add("Набор заказов 2 (4 точки)");
            OrderComboBox.Items.Add("Набор заказов 3 (4 точки)");
            OrderComboBox.Items.Add("Набор заказов 4 (5 точек)");
            OrderComboBox.Items.Add("Набор заказов 5 (4 точки)");
            OrderComboBox.Items.Add("Набор заказов 6 (10 точек)");
            OrderComboBox.Items.Add("Набор заказов 7 (1 точка)");

            OrderComboBox.SelectedIndex = 0;
        }

        private void OrderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (OrderComboBox.SelectedIndex)
            {
                case 0: LoadOrders(OrderArrays.GetOrderArray1()); break;
                case 1: LoadOrders(OrderArrays.GetOrderArray2()); break;
                case 2: LoadOrders(OrderArrays.GetOrderArray3()); break;
                case 3: LoadOrders(OrderArrays.GetOrderArray4()); break;
                case 4: LoadOrders(OrderArrays.GetOrderArray5()); break;
                case 5: LoadOrders(OrderArrays.GetOrderArray6()); break;
                case 6: LoadOrders(OrderArrays.GetOrderArray7()); break;
            }
        }

        private void LoadOrders(Order[] orders)
        {
            _orderManager.LoadOrders(orders);
            _mapVisualizer.DrawOrders(_orderManager.Orders);
        }

        private void GetOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_orderManager.Orders.Any())
            {
                MessageBox.Show("Нет доступных заказов");
                return;
            }

            var (route, cost) = _routeManager.CalculateRoute(_orderManager.Orders);

            if (route.Any())
            {
                _mapVisualizer.DrawRoute(route, _orderManager.Orders, _depotLocation);
                RouteInfoText.Text = _routeManager.GetRouteInfo(route, cost, _orderManager.Orders.Count());
            }
            else
            {
                MessageBox.Show("Ошибка при расчете маршрута");
            }
        }

        private void PointsCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickPoint = e.GetPosition(PointsCanvas);
            var geoPoint = _mapVisualizer.ConvertFromCanvasCoordinates(
                clickPoint.X, clickPoint.Y, _orderManager.Orders);

            _orderManager.AddOrder(geoPoint, PrioritySlider.Value);
            _mapVisualizer.DrawOrders(_orderManager.Orders);

            ListBox2.Visibility = Visibility.Visible;
            ListBox2.Items.Add($"Добавлен заказ\n{_orderManager.GetOrderInfo(_orderManager.Orders.Last())}");

            GetOrderButton_Click(null, null);
        }

        private void ClearListButton_Click(object sender, RoutedEventArgs e)
        {
            ListBox2.Items.Clear();
        }
    }
}

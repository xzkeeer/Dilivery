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
        private Point _depotLocation = new Point { X = 55.7558, Y = 37.6173 };
        private List<Order> _currentOrders = new List<Order>();
        private Random _random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            InitializeComboBox();
            LoadOrders(OrderArrays.GetOrderArray1());

            // Добавляем обработчики кликов мыши
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
            _currentOrders = orders.ToList();
            DrawGraph();
        }

        private void DrawGraph()
        {
            PointsCanvas.Children.Clear();

            if (_currentOrders == null || _currentOrders.Count == 0) return;

            // Рассчитываем масштабирование и центрирование
            var bounds = CalculateBounds();
            double scale = CalculateScale(bounds);
            Point center = CalculateCenter(bounds);

            // Рисуем депо (красная точка)
            DrawDepot(scale, center);

            // Рисуем все заказы
            foreach (var order in _currentOrders)
            {
                DrawOrder(order, scale, center);
            }
        }

        private (double minX, double maxX, double minY, double maxY) CalculateBounds()
        {
            double minX = _currentOrders.Min(o => o.Destination.X);
            double maxX = _currentOrders.Max(o => o.Destination.X);
            double minY = _currentOrders.Min(o => o.Destination.Y);
            double maxY = _currentOrders.Max(o => o.Destination.Y);

            // Добавляем депо в расчет границ
            minX = Math.Min(minX, _depotLocation.X);
            maxX = Math.Max(maxX, _depotLocation.X);
            minY = Math.Min(minY, _depotLocation.Y);
            maxY = Math.Max(maxY, _depotLocation.Y);

            // Добавляем отступы
            double padding = 0.01;
            return (minX - padding, maxX + padding, minY - padding, maxY + padding);
        }

        private double CalculateScale((double minX, double maxX, double minY, double maxY) bounds)
        {
            double canvasWidth = PointsCanvas.ActualWidth;
            double canvasHeight = PointsCanvas.ActualHeight;

            double scaleX = canvasWidth / (bounds.maxX - bounds.minX);
            double scaleY = canvasHeight / (bounds.maxY - bounds.minY);

            return Math.Min(scaleX, scaleY) * 0.9;
        }

        private Point CalculateCenter((double minX, double maxX, double minY, double maxY) bounds)
        {
            return new Point
            {
                X = (bounds.minX + bounds.maxX) / 2,
                Y = (bounds.minY + bounds.maxY) / 2
            };
        }

        private void DrawDepot(double scale, Point center)
        {
            var (x, y) = ConvertToCanvasCoordinates(_depotLocation, scale, center);

            Ellipse depot = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = Brushes.DarkCyan,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                ToolTip = "Склад"
            };

            Canvas.SetLeft(depot, x - depot.Width / 2);
            Canvas.SetTop(depot, y - depot.Height / 2);
            PointsCanvas.Children.Add(depot);
        }

        private void DrawOrder(Order order, double scale, Point center)
        {
            var (x, y) = ConvertToCanvasCoordinates(order.Destination, scale, center);

            // Точка заказа
            Ellipse point = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = Brushes.Black,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Tag = order.ID // Сохраняем ID заказа в Tag для последующего удаления
            };

            Canvas.SetLeft(point, x - point.Width / 2);
            Canvas.SetTop(point, y - point.Height / 2);
            PointsCanvas.Children.Add(point);

            // Подпись
            TextBlock label = new TextBlock
            {
                Text = order.ID.ToString() + " Заказ",
                FontSize = 10,
                Foreground = Brushes.Black,
                Margin = new Thickness(x + 6, y - 8, 0, 0)
            };
            PointsCanvas.Children.Add(label);

            Brush fillColor;
            if (order.Priority >= 0.9)
                fillColor = Brushes.Red;
            else if (order.Priority >= 0.4)
                fillColor = Brushes.Orange;
            else
                fillColor = Brushes.Green;

            Ellipse points = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = fillColor,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Tag = order.ID
            };
        }

        private (double x, double y) ConvertToCanvasCoordinates(Point point, double scale, Point center)
        {
            double canvasCenterX = PointsCanvas.ActualWidth / 2;
            double canvasCenterY = PointsCanvas.ActualHeight / 2;

            double x = canvasCenterX + (point.X - center.X) * scale;
            double y = canvasCenterY + (point.Y - center.Y) * scale;

            return (x, y);
        }

        private Point ConvertFromCanvasCoordinates(double canvasX, double canvasY, double scale, Point center)
        {
            double canvasCenterX = PointsCanvas.ActualWidth / 2;
            double canvasCenterY = PointsCanvas.ActualHeight / 2;

            double geoX = center.X + (canvasX - canvasCenterX) / scale;
            double geoY = center.Y + (canvasY - canvasCenterY) / scale;

            return new Point { X = geoX, Y = geoY };
        }

        private void GetOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentOrders.Count == 0)
            {
                MessageBox.Show("Нет доступных заказов");
                return;
            }

            // Рассчитываем оптимальный маршрут для всех заказов
            var solver = new DijkstraDeliverySolver(_depotLocation, _currentOrders);
            var route = solver.FindOptimalRoute();

            // Проверяем маршрут
            if (RoutingTestLogic.TestRoutingSolution(_depotLocation, _currentOrders.ToArray(), route.ToArray(), out var cost))
            {
                // Отображаем маршрут
                DrawRoute(route, _currentOrders);

                // Выводим информацию
                RouteInfoText.Text = $"ИНФОРМАЦИЯ О МАРШРУТЕ:\n" +
                                   $"Количество заказов: {_currentOrders.Count}\n" +
                                   $"Последовательность: {string.Join(" → ", route)}\n" +
                                   $"Общая стоимость маршрута: {cost:F2} \n";
            }
            else
            {
                MessageBox.Show("Ошибка при расчете маршрута");
            }
        }

        private void DrawRoute(List<int> route, List<Order> orders)
        {
            // Очищаем предыдущие линии маршрута и подписи
            var elementsToRemove = PointsCanvas.Children.OfType<FrameworkElement>()
                .Where(x => x is Line || x is TextBlock && ((string)x.Tag) == "distance").ToList();
            foreach (var element in elementsToRemove)
            {
                PointsCanvas.Children.Remove(element);
            }

            // Рассчитываем масштабирование и центрирование
            var bounds = CalculateBounds();
            double scale = CalculateScale(bounds);
            Point center = CalculateCenter(bounds);

            Point previousPoint = _depotLocation;

            for (int i = 1; i < route.Count; i++)
            {
                Point currentPoint;

                if (route[i] == -1) // Возврат в депо
                {
                    currentPoint = _depotLocation;
                }
                else
                {
                    var order = orders.FirstOrDefault(o => o.ID == route[i]);
                    if (order.Equals(default(Order))) continue;
                    currentPoint = order.Destination;
                }

                // Конвертируем координаты
                var (x1, y1) = ConvertToCanvasCoordinates(previousPoint, scale, center);
                var (x2, y2) = ConvertToCanvasCoordinates(currentPoint, scale, center);

                // Рисуем линию
                Line line = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = Brushes.DarkCyan,
                    StrokeThickness = 2
                };
                PointsCanvas.Children.Add(line);

                // Добавляем подпись расстояния
                double distance = RoutingTestLogic.CalculateDistance(previousPoint, currentPoint);
                TextBlock distanceLabel = new TextBlock
                {
                    Text = $"{distance:F2}",
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    Tag = "distance",
                    Margin = new Thickness((x1 + x2) / 2, (y1 + y2) / 2, 0, 0)
                };
                PointsCanvas.Children.Add(distanceLabel);
                previousPoint = currentPoint;
            }
        }

       

        private void PointsCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Явно указываем тип System.Windows.Point
            System.Windows.Point clickPoint = e.GetPosition(PointsCanvas);

            var bounds = CalculateBounds();
            double scale = CalculateScale(bounds);
            Point center = CalculateCenter(bounds);

            // Преобразуем координаты
            double canvasCenterX = PointsCanvas.ActualWidth / 2;
            double canvasCenterY = PointsCanvas.ActualHeight / 2;

            double geoX = center.X + (clickPoint.X - canvasCenterX) / scale;
            double geoY = center.Y + (clickPoint.Y - canvasCenterY) / scale;

            // Создаем новый заказ с явным указанием типа BestDelivery.Point
            var newOrder = new Order
            {
                ID = _currentOrders.Count > 0 ? _currentOrders.Max(o => o.ID) + 1 : 1,
                Destination = new Point { X = geoX, Y = geoY },
                Priority = GetPriorityValueFromComboBox()
            };

            _currentOrders.Add(newOrder);
            DrawGraph();

            ListBox2.Visibility = Visibility.Visible;
            string priorityLabel = newOrder.Priority >= 0.9 ? "Высокий" :
                        newOrder.Priority >= 0.4 ? "Средний" :
                        "Низкий";
         
            ListBox2.Items.Add(
                $"Добавлен заказ\n" +
                $"Заказ #{newOrder.ID}: Координаты ({newOrder.Destination.X:F4}, {newOrder.Destination.Y:F4}), " +
                $"Приоритет: {priorityLabel}");

            GetOrderButton_Click(null, null);
        }

        private void PointsCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Явно указываем тип System.Windows.Point
            System.Windows.Point clickPoint = e.GetPosition(PointsCanvas);

            foreach (var child in PointsCanvas.Children.OfType<Ellipse>())
            {
                if (child.Tag is int orderId &&
                    Math.Abs(Canvas.GetLeft(child) + child.Width / 2 - clickPoint.X) < 10 &&
                    Math.Abs(Canvas.GetTop(child) + child.Height / 2 - clickPoint.Y) < 10)
                {
                    _currentOrders.RemoveAll(o => o.ID == orderId);
                    DrawGraph();
                    GetOrderButton_Click(null, null);
                    return;
                }
            }
        }
        private double GetPriorityValueFromComboBox()
        {
            return PrioritySlider.Value;
        }

        private void ClearListButton_Click(object sender, RoutedEventArgs e)
        {
            ListBox2.Items.Clear();
        }
    }
}
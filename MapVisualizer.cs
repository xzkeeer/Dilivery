using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BestDelivery;
using Point = BestDelivery.Point;


namespace Dilivery
{
    public class MapVisualizer
    {
        private readonly Canvas _canvas;
        private BestDelivery.Point _depotLocation = new BestDelivery.Point { X = 55.7558, Y = 37.6173 };

        public MapVisualizer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void DrawOrders(IEnumerable<Order> orders)
        {
            _canvas.Children.Clear();
            if (orders == null || !orders.Any()) return;

            var bounds = CalculateBounds(orders);
            double scale = CalculateScale(bounds);
            Point center = CalculateCenter(bounds);

            DrawDepot(scale, center);

            foreach (var order in orders)
            {
                DrawOrder(order, scale, center);
            }
        }

        public void DrawRoute(IEnumerable<int> route, IEnumerable<Order> orders, Point depotLocation)
        {
            var bounds = CalculateBounds(orders);
            double scale = CalculateScale(bounds);
            Point center = CalculateCenter(bounds);

            Point previousPoint = depotLocation;

            for (int i = 1; i < route.Count(); i++)
            {
                Point currentPoint = route.ElementAt(i) == -1
                    ? depotLocation
                    : orders.First(o => o.ID == route.ElementAt(i)).Destination;

                var (x1, y1) = ConvertToCanvasCoordinates(previousPoint, scale, center);
                var (x2, y2) = ConvertToCanvasCoordinates(currentPoint, scale, center);

                DrawLine(x1, y1, x2, y2);
                DrawDistanceLabel((x1 + x2) / 2, (y1 + y2) / 2,
                    RoutingTestLogic.CalculateDistance(previousPoint, currentPoint));

                previousPoint = currentPoint;
            }
        }

        private void DrawDepot(double scale, Point center)
        {
            var (x, y) = ConvertToCanvasCoordinates(_depotLocation, scale, center);

            var depot = new Ellipse
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
            _canvas.Children.Add(depot);
        }

        private void DrawOrder(Order order, double scale, Point center)
        {
            var (x, y) = ConvertToCanvasCoordinates(order.Destination, scale, center);

            var point = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = Brushes.Black,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Tag = order.ID
            };

            Canvas.SetLeft(point, x - point.Width / 2);
            Canvas.SetTop(point, y - point.Height / 2);
            _canvas.Children.Add(point);

            DrawOrderLabel(x + 6, y - 8, order.ID);
        }

        

        private void DrawOrderLabel(double x, double y, int orderId)
        {
            var label = new TextBlock
            {
                Text = $"{orderId} Заказ",
                FontSize = 10,
                Foreground = Brushes.Black,
                Margin = new Thickness(x, y, 0, 0)
            };
            _canvas.Children.Add(label);
        }

        private void DrawLine(double x1, double y1, double x2, double y2)
        {
            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Brushes.DarkCyan,
                StrokeThickness = 2
            };
            _canvas.Children.Add(line);
        }

        private void DrawDistanceLabel(double x, double y, double distance)
        {
            var label = new TextBlock
            {
                Text = $"{distance:F2}",
                FontSize = 10,
                Foreground = Brushes.Black,
                Tag = "distance",
                Margin = new Thickness(x, y, 0, 0)
            };
            _canvas.Children.Add(label);
        }

        private (double minX, double maxX, double minY, double maxY) CalculateBounds(IEnumerable<Order> orders)
        {
            double minX = orders.Min(o => o.Destination.X);
            double maxX = orders.Max(o => o.Destination.X);
            double minY = orders.Min(o => o.Destination.Y);
            double maxY = orders.Max(o => o.Destination.Y);

            minX = Math.Min(minX, _depotLocation.X);
            maxX = Math.Max(maxX, _depotLocation.X);
            minY = Math.Min(minY, _depotLocation.Y);
            maxY = Math.Max(maxY, _depotLocation.Y);

            double padding = 0.01;
            return (minX - padding, maxX + padding, minY - padding, maxY + padding);
        }

        private double CalculateScale((double minX, double maxX, double minY, double maxY) bounds)
        {
            double canvasWidth = _canvas.ActualWidth;
            double canvasHeight = _canvas.ActualHeight;

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

        private (double x, double y) ConvertToCanvasCoordinates(Point point, double scale, Point center)
        {
            double canvasCenterX = _canvas.ActualWidth / 2;
            double canvasCenterY = _canvas.ActualHeight / 2;

            double x = canvasCenterX + (point.X - center.X) * scale;
            double y = canvasCenterY + (point.Y - center.Y) * scale;

            return (x, y);
        }

        public Point ConvertFromCanvasCoordinates(double canvasX, double canvasY, IEnumerable<Order> orders)
        {
            var bounds = CalculateBounds(orders);
            double scale = CalculateScale(bounds);
            Point center = CalculateCenter(bounds);

            double canvasCenterX = _canvas.ActualWidth / 2;
            double canvasCenterY = _canvas.ActualHeight / 2;

            double geoX = center.X + (canvasX - canvasCenterX) / scale;
            double geoY = center.Y + (canvasY - canvasCenterY) / scale;

            return new Point { X = geoX, Y = geoY };
        }
    }
}

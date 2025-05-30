using System;
using System.Collections.Generic;
using System.Linq;

namespace BestDelivery
{
    public class DijkstraDeliverySolver
    {
        private readonly Point _depot;
        private readonly List<Order> _orders;
        private readonly Dictionary<int, Order> _ordersDict;
        private readonly Dictionary<(int from, int to), double> _distanceCache;

        public DijkstraDeliverySolver(Point depot, IEnumerable<Order> orders)
        {
            _depot = depot;
            _orders = orders.ToList();
            _ordersDict = _orders.ToDictionary(o => o.ID);
            _distanceCache = new Dictionary<(int, int), double>();
        }

        private class RouteState : IComparable<RouteState>
        {
            public int CurrentLocationId { get; set; }
            public HashSet<int> VisitedOrders { get; set; }
            public double TotalCost { get; set; }
            public List<int> CurrentPath { get; set; }

            public int CompareTo(RouteState other) => TotalCost.CompareTo(other.TotalCost);
        }

        public List<int> FindOptimalRoute()
        {
            if (_orders.Count == 0) return new List<int> { -1, -1 };

            var priorityQueue = new PriorityQueue<RouteState>();
            var bestStates = new Dictionary<string, RouteState>();

            priorityQueue.Enqueue(new RouteState
            {
                CurrentLocationId = -1,
                VisitedOrders = new HashSet<int>(),
                TotalCost = 0,
                CurrentPath = new List<int> { -1 }
            });

            while (priorityQueue.Count > 0)
            {
                var currentState = priorityQueue.Dequeue();

                if (currentState.VisitedOrders.Count == _orders.Count &&
                    currentState.CurrentLocationId == -1)
                {
                    return currentState.CurrentPath;
                }

                foreach (var nextState in GetNextStates(currentState))
                {
                    var stateKey = GetStateKey(nextState);
                    if (!bestStates.TryGetValue(stateKey, out var existing) ||
                        nextState.TotalCost < existing.TotalCost)
                    {
                        bestStates[stateKey] = nextState;
                        priorityQueue.Enqueue(nextState);
                    }
                }
            }

            return new List<int> { -1, -1 };
        }

        private string GetStateKey(RouteState state)
        {
            int mask = 0;
            foreach (var id in state.VisitedOrders) mask |= 1 << id;
            return $"{state.CurrentLocationId}|{mask}";
        }

        private IEnumerable<RouteState> GetNextStates(RouteState currentState)
        {
            if (currentState.CurrentLocationId != -1 &&
                currentState.VisitedOrders.Count == _orders.Count)
            {
                yield return CreateNextState(currentState, -1, currentState.VisitedOrders,
                    GetCost(currentState.CurrentLocationId, -1));
            }

            foreach (var order in _orders)
            {
                if (!currentState.VisitedOrders.Contains(order.ID))
                {
                    var newVisited = new HashSet<int>(currentState.VisitedOrders) { order.ID };
                    yield return CreateNextState(currentState, order.ID, newVisited,
                        GetCost(currentState.CurrentLocationId, order.ID));
                }
            }
        }

        private RouteState CreateNextState(RouteState current, int nextId,
            HashSet<int> visited, double costDelta)
        {
            return new RouteState
            {
                CurrentLocationId = nextId,
                VisitedOrders = visited,
                TotalCost = current.TotalCost + costDelta,
                CurrentPath = new List<int>(current.CurrentPath) { nextId }
            };
        }

        private double GetCost(int fromId, int toId)
        {
            double distance = GetDistance(fromId, toId);
            if (toId == -1) return distance;

            var order = _ordersDict[toId];
            double priorityFactor = Math.Max(0.1, 1.0 - order.Priority);
            return distance * priorityFactor;
        }

        private double GetDistance(int fromId, int toId)
        {
            var key = (fromId, toId);
            if (!_distanceCache.TryGetValue(key, out var distance))
            {
                Point from = fromId == -1 ? _depot : _ordersDict[fromId].Destination;
                Point to = toId == -1 ? _depot : _ordersDict[toId].Destination;
                distance = RoutingTestLogic.CalculateDistance(from, to);
                _distanceCache[key] = distance;
            }
            return distance;
        }
    }

   
}
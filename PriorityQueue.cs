public class PriorityQueue<T> where T : IComparable<T>
{
    private readonly SortedSet<T> _elements;

    public PriorityQueue()
    {
        _elements = new SortedSet<T>(Comparer<T>.Create((x, y) => x.CompareTo(y)));
    }

    public int Count => _elements.Count;
    public void Enqueue(T item) => _elements.Add(item);

    public T Dequeue()
    {
        var item = _elements.Min;
        _elements.Remove(item);
        return item;
    }
}

using System.Collections.Generic;

public class PriorityQueue<T>
{
    private SortedList<float, Queue<T>> elements = new();
    private HashSet<T> lookup = new();
    public int Count { get; private set; }

    public void Enqueue(T item, float priority)
    {
        if (!elements.ContainsKey(priority))
            elements[priority] = new Queue<T>();

        elements[priority].Enqueue(item);
        lookup.Add(item);
        Count++;
    }

    public T Dequeue()
    {
        if (elements.Count == 0) return default;

        var firstKey = elements.Keys[0];
        var queue = elements[firstKey];

        T item = queue.Dequeue();
        if (queue.Count == 0) elements.Remove(firstKey);

        lookup.Remove(item);
        Count--;
        return item;
    }

    public bool Contains(T item)
    {
        return lookup.Contains(item);
    }
}

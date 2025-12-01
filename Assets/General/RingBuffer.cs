using System;

public class RingBuffer<T>
{
    protected readonly T[] buffer;
    protected int head;    // index of oldest element
    protected int count;   // how many valid elements we have

    public int Capacity => buffer.Length;
    public int Count => count;

    public RingBuffer(int capacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        buffer = new T[capacity];
        head   = 0;
        count  = 0;
    }

    public void Push(T item)
    {
        int tail = (head + count) % Capacity;
        buffer[tail] = item;

        if (count == Capacity)
        {
            // overwrite oldest
            head = (head + 1) % Capacity;
        }
        else
        {
            count++;
        }
    }

    // logical index: 0 = oldest, Count-1 = newest
    public T this[int logicalIndex]
    {
        get
        {
            if (logicalIndex < 0 || logicalIndex >= count)
                throw new ArgumentOutOfRangeException(nameof(logicalIndex));

            int idx = (head + logicalIndex) % Capacity;
            return buffer[idx];
        }
        set
        {
            if (logicalIndex < 0 || logicalIndex >= count)
                throw new ArgumentOutOfRangeException(nameof(logicalIndex));

            int idx = (head + logicalIndex) % Capacity;
            buffer[idx] = value;
        }
    }
}

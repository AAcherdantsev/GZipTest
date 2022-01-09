using System.Collections.Concurrent;

namespace GZipTest;

public class ObservedConcurrentDictionary<TValue>
{
    public event AvailabilityOfNecessaryBlockHandler? NextBlockIsInCollection;

    public delegate void AvailabilityOfNecessaryBlockHandler(int index);

    private readonly ConcurrentDictionary<int, TValue> dictionary;

    private readonly object lockObj;

    private volatile int lookingIndex;

    public int Count => this.dictionary.Count;

    public ObservedConcurrentDictionary()
    {
        this.dictionary = new();
        this.lockObj = new();
        this.lookingIndex = 0;
    }

    public bool TryRemove(int key, out TValue? value)
    {
        bool equality;

        lock (this.lockObj)
        {
            equality = key == this.lookingIndex;
        }

        if (equality)
        {
            if (this.dictionary.TryRemove(key, out value))
            {
                Interlocked.Increment(ref this.lookingIndex);
                return true;
            }

            return false;
        }
        else
        {
            value = default;
        }

        return false;
    }

    public bool TryAdd(int key, TValue value)
    {
        if (this.dictionary.TryAdd(key, value))
        {
            return true;
        }
        return false;
    }

    public void CheckingAvailabilityOfNecessaryBlock()
    {
        int key;

        lock (this.lockObj)
        {
            key = this.lookingIndex;
        }

        if (this.dictionary.ContainsKey(key))
        {
            this.NextBlockIsInCollection?.Invoke(key);
        }
    }
}
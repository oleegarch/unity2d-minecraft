using System;
using System.Collections.Generic;

public class ComputationCache<TKey, TValue>
{
    private readonly Dictionary<TKey, LinkedListNode<(TKey key, TValue value)>> _cacheMap;
    private readonly LinkedList<(TKey key, TValue value)> _lruList;
    private readonly int _capacity;
    private readonly int _cleanupBatchSize;

    public ComputationCache(int capacity = 1000, float cleanupFactor = 0.2f)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));

        if (cleanupFactor <= 0f || cleanupFactor > 1f)
            throw new ArgumentException("Cleanup factor must be between 0 and 1.", nameof(cleanupFactor));

        _capacity = capacity;
        _cleanupBatchSize = Math.Max(1, (int)(capacity * cleanupFactor));

        _cacheMap = new Dictionary<TKey, LinkedListNode<(TKey, TValue)>>(capacity + _cleanupBatchSize);
        _lruList = new LinkedList<(TKey, TValue)>();
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        if (_cacheMap.TryGetValue(key, out var node))
        {
            _lruList.Remove(node);
            _lruList.AddFirst(node);
            return node.Value.value;
        }

        TValue value = valueFactory(key);

        // Если превышен лимит — удаляем пачку старых элементов
        if (_cacheMap.Count >= _capacity + _cleanupBatchSize)
            CleanupOldEntries();

        var newNode = new LinkedListNode<(TKey, TValue)>((key, value));
        _lruList.AddFirst(newNode);
        _cacheMap[key] = newNode;

        return value;
    }

    private void CleanupOldEntries()
    {
        for (int i = 0; i < _cleanupBatchSize; i++)
        {
            if (_lruList.Last is not LinkedListNode<(TKey key, TValue value)> lastNode)
                break;

            _cacheMap.Remove(lastNode.Value.key);
            _lruList.RemoveLast();
        }
    }

    public void Clear()
    {
        _cacheMap.Clear();
        _lruList.Clear();
    }

    public int Count => _cacheMap.Count;
}
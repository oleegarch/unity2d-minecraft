using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
    {
        if (!dict.TryGetValue(key, out TValue value))
        {
            value = new TValue();
            dict.Add(key, value);
        }

        return value;
    }
}

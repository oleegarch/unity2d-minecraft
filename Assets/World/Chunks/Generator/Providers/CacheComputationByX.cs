using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.Chunks.Generator.Providers
{
    public class CacheComputationByX<T>
    {
        private readonly Dictionary<int, T> _cache = new();

        private readonly Func<int, int, int> _makeKey;
        private readonly Func<int, int, T> _compute;

        public CacheComputationByX(Func<int, int, int> makeKey, Func<int, int, T> compute)
        {
            _makeKey = makeKey;
            _compute = compute;
        }

        public void CacheComputation(RectInt rect, int seed)
        {
            // Сначала собираем набор ключей, которые должны остаться
            var neededKeys = new HashSet<int>();
            for (int x = rect.xMin; x <= rect.xMax; x++)
                neededKeys.Add(_makeKey(x, seed));

            // Удаляем все лишние ключи
            var keysToRemove = new List<int>();
            foreach (var key in _cache.Keys)
            {
                if (!neededKeys.Contains(key))
                    keysToRemove.Add(key);
            }
            foreach (var key in keysToRemove)
                _cache.Remove(key);

            // Генерим всё, что должно быть, и добавляем недостающее
            for (int x = rect.xMin; x <= rect.xMax; x++)
            {
                int key = _makeKey(x, seed);
                if (!_cache.ContainsKey(key))
                    _cache[key] = _compute(x, seed);
            }
        }

        public T GetValue(int worldX, int seed)
        {
            int key = _makeKey(worldX, seed);
            if (_cache.TryGetValue(key, out var value))
                return value;

            T result = _compute(worldX, seed);
            _cache[key] = result;
            return result;
        }
    }
}
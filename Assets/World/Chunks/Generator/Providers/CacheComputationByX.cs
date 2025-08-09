using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.Chunks.Generator.Providers
{
    public class CacheComputationByX<T>
    {
        private readonly Dictionary<int, T> _cache = new();

        private readonly Func<int, int, T> _compute;

        public CacheComputationByX(Func<int, int, T> compute)
        {
            _compute = compute;
        }

        public void CacheComputation(RectInt rect, int seed)
        {
            // Собираем все ключи, которые нужны для текущего окна
            int width = rect.width;
            var neededKeys = new HashSet<int>(width);
            for (int x = rect.xMin; x <= rect.xMax; x++)
                neededKeys.Add(MakeKey(x, seed));

            // Удаляем все лишние ключи
            var keysSnapshot = new int[_cache.Count];
            _cache.Keys.CopyTo(keysSnapshot, 0);
            foreach (var key in keysSnapshot)
            {
                if (!neededKeys.Contains(key))
                    _cache.Remove(key);
            }

            // Добавляем недостающие ключи
            for (int x = rect.xMin; x <= rect.xMax; x++)
            {
                int key = MakeKey(x, seed);
                if (!_cache.ContainsKey(key))
                    _cache[key] = _compute(x, seed);
            }
        }

        public T GetValue(int worldX, int seed)
        {
            int key = MakeKey(worldX, seed);
            if (_cache.TryGetValue(key, out var value))
                return value;

            T result = _compute(worldX, seed);
            _cache[key] = result;
            return result;
        }

        private int MakeKey(int worldX, int seed) => (worldX << 16) ^ seed;
    }
}
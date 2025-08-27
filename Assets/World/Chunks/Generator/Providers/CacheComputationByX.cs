using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.Chunks.Generator.Providers
{
    public class CacheComputationByX<T>
    {
        private readonly Dictionary<int, T> _cache = new();

        private readonly Func<int, T> _compute;
        private readonly int _seed;

        public CacheComputationByX(Func<int, T> compute, int seed)
        {
            _compute = compute;
            _seed = seed;
        }

        public void CacheComputation(RectInt rect)
        {
            // Собираем все ключи, которые нужны для текущего окна
            int width = rect.width;
            var neededKeys = new HashSet<int>(width);
            for (int x = rect.xMin; x <= rect.xMax; x++)
                neededKeys.Add(MakeKey(x));

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
                int key = MakeKey(x);
                if (!_cache.ContainsKey(key))
                    _cache[key] = _compute(x);
            }
        }

        public T GetValue(int worldX)
        {
            int key = MakeKey(worldX);
            if (_cache.TryGetValue(key, out var value))
                return value;

            T result = _compute(worldX);
            _cache[key] = result;
            return result;
        }

        private int MakeKey(int worldX) => (worldX << 16) ^ _seed;
    }
}
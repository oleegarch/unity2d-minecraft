using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.Chunks.Generator.Providers
{
    public class CacheHelper<T>
    {
        private readonly Dictionary<int, T> _cache = new();
        private RectInt? _prevRect;

        private readonly Func<int, int, int> _makeKey;
        private readonly Func<int, int, T> _compute;

        public CacheHelper(Func<int, int, int> makeKey, Func<int, int, T> compute)
        {
            _makeKey = makeKey;
            _compute = compute;
        }

        public void CacheComputation(RectInt rect, int seed)
        {
            if (_prevRect.HasValue)
            {
                var prev = _prevRect.Value;

                // Удаляем слева
                if (rect.xMin > prev.xMin)
                {
                    for (int x = prev.xMin; x < rect.xMin; x++)
                        _cache.Remove(_makeKey(x, seed));
                }
                // Удаляем справа
                if (rect.xMax < prev.xMax)
                {
                    for (int x = rect.xMax + 1; x <= prev.xMax; x++)
                        _cache.Remove(_makeKey(x, seed));
                }

                // Добавляем слева
                if (rect.xMin < prev.xMin)
                {
                    for (int x = rect.xMin; x < prev.xMin; x++)
                        _cache[_makeKey(x, seed)] = _compute(x, seed);
                }
                // Добавляем справа
                if (rect.xMax > prev.xMax)
                {
                    for (int x = prev.xMax + 1; x <= rect.xMax; x++)
                        _cache[_makeKey(x, seed)] = _compute(x, seed);
                }
            }
            else
            {
                // Первое заполнение
                for (int x = rect.xMin; x <= rect.xMax; x++)
                    _cache[_makeKey(x, seed)] = _compute(x, seed);
            }

            _prevRect = rect;
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
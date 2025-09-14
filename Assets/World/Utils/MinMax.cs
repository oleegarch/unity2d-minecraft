using System;
using UnityEngine;

[Serializable]
public struct MinMax
{
    [SerializeField] private float min;
    [SerializeField] private float max;

    public float Min
    {
        get => min;
        set
        {
            if (value > max)
                throw new ArgumentException("Min cannot be greater than Max");
            min = value;
        }
    }

    public float Max
    {
        get => max;
        set
        {
            if (value < min)
                throw new ArgumentException("Max cannot be less than Min");
            max = value;
        }
    }

    public MinMax(float min, float max)
    {
        if (min > max)
            throw new ArgumentException("Min cannot be greater than Max");
        this.min = min;
        this.max = max;
    }

    /// <summary>
    /// Длина MinMax: max - min.
    /// </summary>
    public float Length => max - min;

    /// <summary>
    /// Сумма MinMax: min + max.
    /// </summary>
    public float Sum => min + max;

    /// <summary>
    /// Случайное число с использованием UnityEngine.Random.
    /// </summary>
    public float GetRandom() => UnityEngine.Random.Range(min, max);

    /// <summary>
    /// Детерминированное случайное число с использованием System.Random.
    /// </summary>
    public float GetRandom(System.Random rng) 
        => (float)(min + rng.NextDouble() * (max - min));

    /// <summary>
    /// Проверяет, входит ли число в диапазон [Min, Max].
    /// </summary>
    public bool Contains(float value) => value >= min && value <= max;
}

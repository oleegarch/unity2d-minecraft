using System;
using UnityEngine;

[Serializable]
public struct MinMaxInt
{
    [SerializeField] private int min;
    [SerializeField] private int max;

    public int Min
    {
        get => min;
        set
        {
            if (value > max)
                throw new ArgumentException("Min cannot be greater than Max");
            min = value;
        }
    }

    public int Max
    {
        get => max;
        set
        {
            if (value < min)
                throw new ArgumentException("Max cannot be less than Min");
            max = value;
        }
    }

    public MinMaxInt(int min, int max)
    {
        if (min > max)
            throw new ArgumentException("Min cannot be greater than Max");
        this.min = min;
        this.max = max;
    }

    /// <summary>
    /// Случайное число с использованием UnityEngine.Random.
    /// </summary>
    public int GetRandom() => UnityEngine.Random.Range(min, max + 1);

    /// <summary>
    /// Детерминированное случайное число с использованием System.Random.
    /// </summary>
    public int GetRandom(System.Random rng) => rng.Next(min, max + 1);

    /// <summary>
    /// Проверяет, входит ли число в диапазон [Min, Max].
    /// </summary>
    public bool Contains(int value) => value >= min && value <= max;
}
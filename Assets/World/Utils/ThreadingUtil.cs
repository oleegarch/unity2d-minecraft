using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class ThreadingUtil
{
#if UNITY_WEBGL && !UNITY_EDITOR
    public static bool UsingAsync = false;
#else
    public static bool UsingAsync = true;
#endif

    /// <summary>
    /// Выполняет функцию на ThreadPool (если поддерживается), иначе синхронно.
    /// </summary>
    public static async UniTask<T> RunSmart<T>(Func<T> func, string context = null)
    {
        if (!UsingAsync)
        {
            if (!string.IsNullOrEmpty(context))
                Debug.Log($"[RunSmart:WebGL→Main] Выполняем синхронно: {context}");

            // WebGL не поддерживает потоки
            return func();
        }

        if (!string.IsNullOrEmpty(context))
            Debug.Log($"[RunSmart:ThreadPool] Запущено в потоке: {context}");

        try
        {
            return await UniTask.RunOnThreadPool(func);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RunSmart Error:{context}] {ex}");
            throw;
        }
    }

    /// <summary>
    /// Версия без возвращаемого значения (void → UniTask).
    /// </summary>
    public static async UniTask RunSmart(Action action, string context = null)
    {
        if (!UsingAsync)
        {
            if (!string.IsNullOrEmpty(context))
                Debug.Log($"[RunSmart:WebGL→Main] Выполняем синхронно: {context}");

            action();

            return;
        }

        if (!string.IsNullOrEmpty(context))
            Debug.Log($"[RunSmart:ThreadPool] Запущено в потоке: {context}");

        try
        {
            await UniTask.RunOnThreadPool(action);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RunSmart Error:{context}] {ex}");
            throw;
        }
    }
}
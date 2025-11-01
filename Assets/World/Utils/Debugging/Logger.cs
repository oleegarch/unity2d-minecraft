using System;
using System.Threading.Tasks;
using UnityEngine;

namespace World.Utils.Debugging
{
    public static class Logger
    {

        public static void DevLog(string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(message);
#endif
        }
        public static void DevLogError(string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError(message);
#endif
        }

        [RuntimeInitializeOnLoadMethod]
        static void SetupGlobalExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Debug.LogError($"[ThreadPool UnhandledException] {e.ExceptionObject}");
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Debug.LogError($"[TaskScheduler UnobservedException] {e.Exception}");
                e.SetObserved();
            };
        }
    }
}
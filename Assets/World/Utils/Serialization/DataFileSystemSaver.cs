using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Logger = World.Utils.Debugging.Logger;

namespace World.Utils.Serialization
{
    /// <summary>
    /// Абстрактный универсальный сохранитель данных в файловой системе.
    /// </summary>
    public abstract class DataFileSystemSaver : DataSaver
    {
        protected readonly string BaseFolder;

        protected DataFileSystemSaver(string subfolder = "")
        {
            BaseFolder = string.IsNullOrEmpty(subfolder)
                ? Application.persistentDataPath
                : Path.Combine(Application.persistentDataPath, subfolder);

            if (!Directory.Exists(BaseFolder))
            {
                Directory.CreateDirectory(BaseFolder);
            }
        }

        /// <summary>
        /// Сохранить объект типа T в указанный файл (внутри persistentDataPath/subfolder).
        /// fileName может содержать подкаталоги (они будут созданы): "saves/world1.json"
        /// </summary>
        public override async UniTask SaveAsync<T>(T data, string fileName)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

            string fullPath = GetFullPath(fileName);
            EnsureDirectoryExistsForFile(fullPath);

            string payload = Serialize(data);

            // Асинхронная файловая операция в пуле потоков
            await UniTask.RunOnThreadPool(() => File.WriteAllText(fullPath, payload));

            Logger.DevLog($"DataFileSystemSaver: saved {typeof(T).Name} -> {fullPath}");
        }

        /// <summary>
        /// Прочитать объект T из файла. Если файла нет — вернёт default(T).
        /// </summary>
        public override async UniTask<T> LoadAsync<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            string fullPath = GetFullPath(fileName);
            if (!File.Exists(fullPath))
            {
                Logger.DevLog($"DataFileSystemSaver: file not found: {fullPath}");
                return default;
            }

            string json = await UniTask.RunOnThreadPool(() => File.ReadAllText(fullPath));
            if (string.IsNullOrEmpty(json)) return default;

            T result = Deserialize<T>(json);

            Logger.DevLog($"DataFileSystemSaver: loaded {typeof(T).Name} <- {fullPath}");

            return result;
        }

        protected string GetFullPath(string fileName)
        {
            if (Path.IsPathRooted(fileName)) return fileName;
            return Path.Combine(BaseFolder, fileName);
        }

        protected void EnsureDirectoryExistsForFile(string fullPath)
        {
            string dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        /// <summary>
        /// Сериализация объекта в строку (реализуется в подклассах)
        /// </summary>
        protected abstract string Serialize<T>(T obj);

        /// <summary>
        /// Десериализация строки в объект (реализуется в подклассах)
        /// </summary>
        protected abstract T Deserialize<T>(string payload);
    }
}
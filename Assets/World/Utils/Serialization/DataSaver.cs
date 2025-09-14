using Cysharp.Threading.Tasks;

namespace World.Utils.Serialization
{
    /// <summary>
    /// Абстрактный универсальный сохранитель данных.
    /// Реализации должны определить, как именно сериализовать/десериализовать объект T.
    /// Методы работают с именем файла (без абсолютного пути) и сохраняют/читают файл в Application.persistentDataPath.
    /// </summary>
    public abstract class DataSaver
    {
        /// <summary>
        /// Сохранить объект типа T в указанный файл.
        /// </summary>
        public abstract UniTask SaveAsync<T>(T data, string fileName);

        /// <summary>
        /// Прочитать объект T из файла. Если файла нет — вернёт default(T).
        /// </summary>
        public abstract UniTask<T> LoadAsync<T>(string fileName);
    }
}
using System;
using Newtonsoft.Json;
using Logger = World.Utils.Debugging.Logger;

namespace World.Utils.Serialization
{
    /// <summary>
    /// JSON-реализация в файловой системе, использует Newtonsoft.Json.
    /// Подходит для любых сериализуемых типов, включая Dictionary, массивы и сложные объекты.
    /// </summary>
    public class JSONDataFileSystemSaver : DataFileSystemSaver
    {
        private readonly Formatting _formatting;

        /// <param name="subfolder">подпапка внутри Application.persistentDataPath</param>
        /// <param name="prettyPrint">форматировать JSON для удобочитаемости</param>
        public JSONDataFileSystemSaver(string subfolder = "", bool prettyPrint = false) : base(subfolder)
        {
            _formatting = prettyPrint ? Formatting.Indented : Formatting.None;
        }

        protected override string Serialize<T>(T obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, _formatting);
            }
            catch (Exception ex)
            {
                Logger.DevLogError($"JSONDataFileSystemSaver.Serialize<{typeof(T).Name}> failed: {ex}");
                throw;
            }
        }

        protected override T Deserialize<T>(string payload)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(payload);
            }
            catch (Exception ex)
            {
                Logger.DevLogError($"JSONDataFileSystemSaver.Deserialize<{typeof(T).Name}> failed: {ex}");
                throw;
            }
        }
    }
}
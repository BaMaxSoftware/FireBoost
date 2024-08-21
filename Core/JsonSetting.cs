using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Core
{
    public static class JsonSetting<T> where T : IJsonSetting
    {
        public static T Open(string filePath = "")
        {
            T value = (T)Activator.CreateInstance(typeof(T));
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = GetFilePath(value);
            }

            if (File.Exists(filePath))
            {
                T deserilizedObject = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath, Encoding.UTF8), GetSerializerSettings());

                if(deserilizedObject == null)
                {
                    Save(value);
                }
                else
                {
                    value = deserilizedObject;
                }
            }
            else
            {
                Save(value);
            }

            return value;
        }

        public static void Save(T value, string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = GetFilePath(value);
            }
            File.WriteAllText(filePath, JsonConvert.SerializeObject(value, GetSerializerSettings()), Encoding.UTF8);
        }

        private static string GetFilePath(T value)
        {
            string FileLocation = Path.Combine("bimteam", value.Filename);
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), FileLocation);
        }

        private static JsonSerializerSettings GetSerializerSettings() => new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
    }
}

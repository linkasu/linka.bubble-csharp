using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Linka.Bubble
{
    internal static class SettingsStore
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LINKa", "Bubble", "settings.json");

        public static BubbleSettings Load()
        {
            try
            {
                using (var file = File.OpenRead(FilePath))
                {
                    var settings = (BubbleSettings)new DataContractJsonSerializer(typeof(BubbleSettings)).ReadObject(file);
                    settings.Clamp();
                    return settings;
                }
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException ||
                                          error is System.Runtime.Serialization.SerializationException ||
                                          error is InvalidCastException || error is NullReferenceException)
            {
                return new BubbleSettings();
            }
        }

        public static void Save(BubbleSettings settings)
        {
            var folder = Path.GetDirectoryName(FilePath);
            Directory.CreateDirectory(folder);
            var temporary = FilePath + ".tmp";
            try
            {
                using (var file = File.Create(temporary))
                    new DataContractJsonSerializer(typeof(BubbleSettings)).WriteObject(file, settings);

                if (File.Exists(FilePath)) File.Replace(temporary, FilePath, null);
                else File.Move(temporary, FilePath);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }
    }
}

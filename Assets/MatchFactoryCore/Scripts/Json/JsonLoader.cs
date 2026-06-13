using System.Collections.Generic;
using System.IO;
using MatchFactoryCore.Scripts.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Json
{
    public static class JsonLoader
    {
        static readonly string JsonDir = "Assets/MatchFactoryCore/Addressable/Json/";
        static readonly string DataLevelJson = "dataLevel.json";
        static readonly string UserDataJson = "dataUser.json";

        public static Dictionary<int, DataLevelMatchFactory> CacheLevel { get; private set; }
        public static int CacheWinStreak { get; private set; }
        public static List<ActionType> CacheActionTypes  { get; private set; }

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Converters =
            {
                new StringEnumConverter(),
                new RuntimeSpriteIgnoreConverter()
            }
        };

        public static void LoadDataLevel()
        {
            string fullPath = Path.Combine(JsonDir, DataLevelJson);

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[JsonLoader] File not found: {fullPath}");
                CacheLevel = new Dictionary<int, DataLevelMatchFactory>();
                return;
            }

            string json = File.ReadAllText(fullPath);
            var data = JsonConvert.DeserializeObject<List<DataLevelMatchFactory>>(json, JsonSettings);

            CacheLevel = new Dictionary<int, DataLevelMatchFactory>();
            if (data == null) return;

            foreach (var entry in data)
                CacheLevel[entry.Level] = entry;

            Debug.Log($"[JsonLoader] Loaded {CacheLevel.Count} levels.");
        }

        public static void LoadUserData()
        {
            string fullPath = Path.Combine(JsonDir, UserDataJson);

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[JsonLoader] File not found: {fullPath}");
                CacheWinStreak = 0;
                CacheActionTypes = new List<ActionType>();
                return;
            }

            string json = File.ReadAllText(fullPath);
            var data = JsonConvert.DeserializeObject<DataUser>(json);
            
            CacheActionTypes = new List<ActionType>();
            CacheWinStreak = 0;
            if (data == null) return;

            CacheWinStreak = data.WinStreak;
            CacheActionTypes = data.ActionTypes;
            
            Debug.Log($"[JsonLoader] Loaded {CacheWinStreak} winStreak.");
        }
    }

    internal class RuntimeSpriteIgnoreConverter : JsonConverter<Sprite>
    {
        public override Sprite ReadJson(JsonReader reader, System.Type objectType, Sprite existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            reader.Skip();
            return null;
        }

        public override void WriteJson(JsonWriter writer, Sprite value, JsonSerializer serializer)
            => writer.WriteNull();
    }
}
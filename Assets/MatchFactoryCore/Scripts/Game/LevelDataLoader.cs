using System.Collections.Generic;
using System.IO;
using MatchFactoryCore.Scripts.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace MatchFactoryCore.Scripts.Game
{
    [DefaultExecutionOrder(-101)]
    public class LevelDataLoader : MonoBehaviour
    {
        public const string JsonRelativePath = "MatchFactory/dataLevel.json";

        private static LevelDataLoader _instance;
        public static LevelDataLoader Ins => _instance;

        public Dictionary<int, DataLevelMatchFactoryNew> LevelCache { get; private set; }

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Converters =
            {
                new StringEnumConverter(),
                new RuntimeSpriteIgnoreConverter()
            }
        };

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            LoadFromJson();
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void LoadFromJson()
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, JsonRelativePath);

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[LevelDataLoader] File not found: {fullPath}");
                LevelCache = new Dictionary<int, DataLevelMatchFactoryNew>();
                return;
            }

            string json = File.ReadAllText(fullPath);
            var list = JsonConvert.DeserializeObject<List<DataLevelMatchFactoryNew>>(json, JsonSettings);

            LevelCache = new Dictionary<int, DataLevelMatchFactoryNew>();
            if (list == null) return;

            foreach (var entry in list)
                LevelCache[entry.Level] = entry;

            Debug.Log($"[LevelDataLoader] Loaded {LevelCache.Count} levels.");
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
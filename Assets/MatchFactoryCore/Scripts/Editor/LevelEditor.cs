using System;
using System.Collections.Generic;
using System.IO;
using MatchFactoryCore.Scripts.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MatchFactoryCore.Scripts.Editor
{
    // ─── Custom converter: serialize Sprite thành asset path ─────────────────────
    internal class SpriteJsonConverter : JsonConverter<Sprite>
    {
        public override void WriteJson(JsonWriter writer, Sprite value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
                writer.WriteValue(AssetDatabase.GetAssetPath(value));
        }

        public override Sprite ReadJson(JsonReader reader, Type objectType, Sprite existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            string path = reader.Value?.ToString();
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────

    public class LevelEditor : EditorWindow
    {
        static readonly string JsonDir  = "Assets/MatchFactoryCore/Addressables/Jsons/";
        static readonly string JsonFile = "dataLevel.json";
        static string FullJsonPath => JsonDir + JsonFile;

        static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = { new SpriteJsonConverter(), new StringEnumConverter() }
        };

        private MultiColumnListView _dataTable;
        private List<DataLevelMatchFactoryNew> _dataLevels;

        [MenuItem("Tools/MatchFactory/LevelEditor")]
        public static void ShowLevelEditor() => GetWindow<LevelEditor>();

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            Button loadDataBtn = new Button { text = "Load Data" };
            loadDataBtn.clicked += OnLoadDataButtonClick;
            root.Add(loadDataBtn);

            Button addRow = new Button { text = "+ Add Row" };
            addRow.clicked += AddLevel;
            root.Add(addRow);

            _dataTable = CreateDataTable();
            root.Add(_dataTable);
        }

        // ─── Save / Load ────────────────────────────────────────────────────────

        private void SaveToJson()
        {
            if (_dataLevels == null) return;

            string json = JsonConvert.SerializeObject(_dataLevels, JsonSettings);

            // 1. Lưu vào thư mục Addressables (tham chiếu editor)
            Directory.CreateDirectory(JsonDir);
            File.WriteAllText(FullJsonPath, json);

            AssetDatabase.Refresh();
            Debug.Log($"[LevelEditor] Saved → {FullJsonPath}");
        }

        private void OnLoadDataButtonClick()
        {
            if (!File.Exists(FullJsonPath))
            {
                Debug.LogWarning($"[LevelEditor] File not found: {FullJsonPath}");
                return;
            }

            string json = File.ReadAllText(FullJsonPath);
            _dataLevels = JsonConvert.DeserializeObject<List<DataLevelMatchFactoryNew>>(json, JsonSettings);
            _dataLevels ??= new List<DataLevelMatchFactoryNew>();

            _dataTable.itemsSource = _dataLevels;
            _dataTable.Rebuild();
            Debug.Log($"[LevelEditor] Loaded {_dataLevels.Count} levels from {FullJsonPath}");
        }

        // ─── Build Table ────────────────────────────────────────────────────────

        private MultiColumnListView CreateDataTable()
        {
            var dataTable = new MultiColumnListView();

            // ── Level ──
            dataTable.columns.Add(new Column
            {
                title = "Level",
                width = 50,
                makeCell = () =>
                {
                    var field = new IntegerField();
                    field.RegisterValueChangedCallback(evt =>
                    {
                        if (field.userData is int idx && _dataLevels != null && idx < _dataLevels.Count)
                        {
                            _dataLevels[idx].Level = evt.newValue;
                            SaveToJson();
                        }
                    });
                    return field;
                },
                bindCell = (e, index) =>
                {
                    var field = (IntegerField)e;
                    field.userData = index;
                    field.SetValueWithoutNotify(_dataLevels[index].Level);
                }
            });

            // ── Time ──
            dataTable.columns.Add(new Column
            {
                title = "Time",
                width = 50,
                makeCell = () =>
                {
                    var field = new FloatField();
                    field.RegisterValueChangedCallback(evt =>
                    {
                        if (field.userData is int idx && _dataLevels != null && idx < _dataLevels.Count)
                        {
                            _dataLevels[idx].TimeOnLevel = evt.newValue;
                            SaveToJson();
                        }
                    });
                    return field;
                },
                bindCell = (e, index) =>
                {
                    var field = (FloatField)e;
                    field.userData = index;
                    field.SetValueWithoutNotify(_dataLevels[index].TimeOnLevel);
                }
            });

            // ── Target Items ──
            dataTable.columns.Add(new Column
            {
                title = "Target Item",
                width = 450,
                makeCell = () => BuildSubTable(isTargetItems: true),
                bindCell = (e, idx) =>
                {
                    var sub = (MultiColumnListView)e;
                    sub.userData = idx;
                    sub.itemsSource = _dataLevels[idx].TargetItems;
                    sub.Rebuild();
                }
            });

            // ── Other Items ──
            dataTable.columns.Add(new Column
            {
                title = "Other Item",
                width = 450,
                makeCell = () => BuildSubTable(isTargetItems: false),
                bindCell = (e, idx) =>
                {
                    var sub = (MultiColumnListView)e;
                    sub.userData = idx;
                    sub.itemsSource = _dataLevels[idx].OtherItems;
                    sub.Rebuild();
                }
            });

            // ── Delete Level ──
            dataTable.columns.Add(new Column
            {
                title = "Delete Level",
                width = 90,
                makeCell = () => new Button { text = "✕ Delete" },
                bindCell = (e, idx) =>
                {
                    var btn = (Button)e;
                    btn.clicked -= btn.userData as Action;
                    Action onClick = () => RemoveLevel(idx);
                    btn.userData = onClick;
                    btn.clicked += onClick;
                }
            });

            dataTable.itemsSource = _dataLevels;
            dataTable.style.height = 500;
            dataTable.fixedItemHeight = 100;
            dataTable.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;

            return dataTable;
        }

        private MultiColumnListView BuildSubTable(bool isTargetItems)
        {
            var subTable = new MultiColumnListView();

            List<DataItemLevel> GetList(int levelIdx) => isTargetItems
                ? _dataLevels[levelIdx].TargetItems
                : _dataLevels[levelIdx].OtherItems;

            // ── Type ──
            subTable.columns.Add(new Column
            {
                title = "Type",
                width = 100,
                makeCell = () =>
                {
                    var field = new EnumField(ItemFactoryType.None);
                    field.RegisterValueChangedCallback(evt =>
                    {
                        if (field.userData is (int levelIdx, int itemIdx))
                        {
                            var list = GetList(levelIdx);
                            if (itemIdx < list.Count)
                            {
                                list[itemIdx].ItemType = (ItemFactoryType)evt.newValue;
                                SaveToJson();
                            }
                        }
                    });
                    return field;
                },
                bindCell = (e, index) =>
                {
                    var field = (EnumField)e;
                    var items = (List<DataItemLevel>)subTable.itemsSource;
                    field.userData = ((int)subTable.userData, index);
                    field.SetValueWithoutNotify(items[index].ItemType);
                }
            });

            // ── Value (Count) ──
            subTable.columns.Add(new Column
            {
                title = "Value",
                width = 50,
                makeCell = () =>
                {
                    var field = new IntegerField();
                    field.RegisterValueChangedCallback(evt =>
                    {
                        if (field.userData is (int levelIdx, int itemIdx))
                        {
                            var list = GetList(levelIdx);
                            if (itemIdx < list.Count)
                            {
                                list[itemIdx].Count = evt.newValue;
                                SaveToJson();
                            }
                        }
                    });
                    return field;
                },
                bindCell = (e, index) =>
                {
                    var field = (IntegerField)e;
                    var items = (List<DataItemLevel>)subTable.itemsSource;
                    field.userData = ((int)subTable.userData, index);
                    field.SetValueWithoutNotify(items[index].Count);
                }
            });

            // ── Sprite ──
            subTable.columns.Add(new Column
            {
                title = "Sprite",
                width = 100,
                makeCell = () =>
                {
                    var field = new ObjectField { objectType = typeof(Sprite) };
                    field.RegisterValueChangedCallback(evt =>
                    {
                        if (field.userData is (int levelIdx, int itemIdx))
                        {
                            var list = GetList(levelIdx);
                            if (itemIdx < list.Count)
                            {
                                list[itemIdx].ItemSprite = evt.newValue as Sprite;
                                SaveToJson();
                            }
                        }
                    });
                    return field;
                },
                bindCell = (e, index) =>
                {
                    var field = (ObjectField)e;
                    var items = (List<DataItemLevel>)subTable.itemsSource;
                    field.userData = ((int)subTable.userData, index);
                    field.SetValueWithoutNotify(items[index].ItemSprite);
                }
            });

            // ── Add Data ──
            subTable.columns.Add(new Column
            {
                title = "AddData",
                width = 75,
                makeCell = () => new Button { text = "+ Add Data" },
                bindCell = (e, itemIdx) =>
                {
                    var btn = (Button)e;
                    btn.clicked -= btn.userData as Action;
                    Action onClick = () =>
                    {
                        int levelIdx = (int)subTable.userData;
                        if (isTargetItems) AddDataItem(levelIdx, itemIdx);
                        else AddOtherItem(levelIdx, itemIdx);
                    };
                    btn.userData = onClick;
                    btn.clicked += onClick;
                }
            });

            // ── Delete ──
            subTable.columns.Add(new Column
            {
                title = "Delete",
                width = 75,
                makeCell = () => new Button { text = "✕ Delete" },
                bindCell = (e, itemIdx) =>
                {
                    var btn = (Button)e;
                    btn.SetEnabled(itemIdx != 0);
                    btn.clicked -= btn.userData as Action;
                    Action onClick = () =>
                    {
                        int levelIdx = (int)subTable.userData;
                        if (isTargetItems) RemoveDataItem(levelIdx, itemIdx);
                        else RemoveOtherItem(levelIdx, itemIdx);
                    };
                    btn.userData = onClick;
                    btn.clicked += onClick;
                }
            });

            return subTable;
        }

        // ─── Level CRUD ─────────────────────────────────────────────────────────

        private void AddLevel()
        {
            _dataLevels ??= new List<DataLevelMatchFactoryNew>();
            _dataLevels.Add(new DataLevelMatchFactoryNew
            {
                Level = _dataLevels.Count + 1,
                TimeOnLevel = 60f,
                TargetItems = new List<DataItemLevel> { new DataItemLevel { ItemType = ItemFactoryType.None } },
                OtherItems  = new List<DataItemLevel> { new DataItemLevel { ItemType = ItemFactoryType.None } }
            });
            _dataTable.itemsSource = _dataLevels;
            _dataTable.Rebuild();
            SaveToJson();
        }

        private void RemoveLevel(int index)
        {
            if (_dataLevels == null || index < 0 || index >= _dataLevels.Count) return;
            _dataLevels.RemoveAt(index);
            _dataTable.itemsSource = _dataLevels;
            _dataTable.Rebuild();
            SaveToJson();
        }

        private void AddDataItem(int levelIndex, int itemIndex)
        {
            _dataLevels[levelIndex].TargetItems ??= new List<DataItemLevel>();
            _dataLevels[levelIndex].TargetItems.Insert(itemIndex + 1, new DataItemLevel { ItemType = ItemFactoryType.None });
            _dataTable.Rebuild();
            SaveToJson();
        }

        private void RemoveDataItem(int levelIndex, int itemIndex)
        {
            var items = _dataLevels[levelIndex].TargetItems;
            if (items == null || itemIndex < 0 || itemIndex >= items.Count) return;
            items.RemoveAt(itemIndex);
            _dataTable.Rebuild();
            SaveToJson();
        }

        private void AddOtherItem(int levelIndex, int itemIndex)
        {
            _dataLevels[levelIndex].OtherItems ??= new List<DataItemLevel>();
            _dataLevels[levelIndex].OtherItems.Insert(itemIndex + 1, new DataItemLevel { ItemType = ItemFactoryType.None });
            _dataTable.Rebuild();
            SaveToJson();
        }

        private void RemoveOtherItem(int levelIndex, int itemIndex)
        {
            var items = _dataLevels[levelIndex].OtherItems;
            if (items == null || itemIndex < 0 || itemIndex >= items.Count) return;
            items.RemoveAt(itemIndex);
            _dataTable.Rebuild();
            SaveToJson();
        }
    }
}
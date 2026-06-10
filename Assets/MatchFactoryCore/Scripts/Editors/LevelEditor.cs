using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.UIElements;
//using Button = UnityEngine.UIElements.Button;
using System.Collections.Generic;
using Assets.MatchFactoryCore.Scripts.Data;
using System.Linq;

namespace Assets.MatchFactoryCore.Scripts.Editors
{
    public class LevelEditor : EditorWindow
    {
        static string jsonPath = "Assets/MatchFactoryCore/Addressables/Jsons/";

        private MultiColumnListView _dataTable;
        private List<DataLevelMatchFactoryNew> _dataLevels;

        [MenuItem("Tools/MatchFactory/LevelEditor")]
        public static void ShowLevelEditor()
        {
            LevelEditor window = GetWindow<LevelEditor>();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            Button loadDataBtn = new Button();
            loadDataBtn.text = "Load Data";
            loadDataBtn.clicked += OnLoadDataButtonClick;
            root.Add(loadDataBtn);

            _dataTable = CreateDataTable();
            root.Add(_dataTable);
        }

        private MultiColumnListView CreateDataTable()
        {
            MultiColumnListView dataTable = new MultiColumnListView();

            dataTable.columns.Add(new Column()
            {
                title = "Level",
                width = 50,
                makeCell = () => new Label(),
                bindCell = (e, index) =>
                {
                    ((Label)e).text = _dataLevels[index].Level.ToString();
                }

            });

            dataTable.columns.Add(new Column()
            {
                title = "Time",
                width = 50,
                makeCell = () => new Label(),
                bindCell = (e, index) =>
                {
                    ((Label)e).text = _dataLevels[index].TimeOnLevel.ToString();
                }
            });

            dataTable.columns.Add(new Column()
            {
                title = "TargetItem",
                width = 150,
                makeCell = () => CreateSubItemTable(),
                bindCell = (e, index) =>
                {

                }
            });

            dataTable.itemsSource = _dataLevels;
            dataTable.style.height = 500;

            return dataTable;
        }

        private MultiColumnListView CreateSubItemTable(int level = -1)
        {
            MultiColumnListView listView = new MultiColumnListView();
            listView.columns.Add(new Column()
            {
                width = 50,
                makeCell = () => new Label(),
                bindCell = (e, index) =>
                {
                    ((Label)e).text = _dataLevels[level].TargetItemDictionary.ElementAt(index).Key.ToString();
                }
            });

            listView.columns.Add(new Column()
            {
                width = 50,
                makeCell = () => new Label(),
                bindCell = (e, index) =>
                {
                    ((Label)e).text = _dataLevels[level].TargetItemDictionary.ElementAt(index).Value.ToString();
                }
            });

            listView.itemsSource = _dataLevels[level].TargetItemDictionary.ToList();
            listView.style.height = 100;

            return listView;
        }

        private void OnLoadDataButtonClick()
        {
            Debug.Log("Load Data");
            var path = jsonPath + "dataLevel.json";
            if (File.Exists(path))
            {
                Debug.Log($"{path} loaded");
            }
            else
            {
                Debug.Log($"{path} null");
            }
        }
    }
}
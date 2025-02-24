using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using DivineSkies.Modules.Core;

namespace DivineSkies.Modules.SaveGame
{
    public class SaveGameController : ModuleBase<SaveGameController>
    {
        private class SaveGameData
        {
            public string Path;
            public string SerializedData;
        }

        [SerializeField]
        private GameObject _savingDisplay;
        private readonly Queue<SaveGameData> _saveDataQueue = new();

        public override void Initialize()
        {
            _savingDisplay = Instantiate(Resources.Load<GameObject>("Prefabs/UI/SavingDisplay"), ModuleController.ConstantUiParent);
            _savingDisplay.SetActive(false);
            StartCoroutine(SavingRoutine());
        }

        public override void BeforeUnregister()
        {
            base.BeforeUnregister();
            SaveAllDirtySync();
        }

        private string GetPath(object instance)
        {
            string fileName = instance.GetType().ToString();
            fileName = fileName.Remove(0, fileName.LastIndexOf('.') + 1);
            return Application.persistentDataPath + "/Data/" + fileName + ".json";
        }

        public void RegisterDirtySavegame(ModuleBase parent, SaveGameBase data)
        {
            var path = GetPath(parent);

            if (!Directory.Exists($@"{Application.persistentDataPath}/Data"))
                Directory.CreateDirectory($@"{Application.persistentDataPath}/Data");

            if (!File.Exists(path))
                File.Create(path).Close();

            var entry = _saveDataQueue.FirstOrDefault(e => e.Path == path);

            if(entry == null) // only enqueue new if not already in queue, else just change serialized Data
            {
                entry = new SaveGameData();
                entry.Path = path;
                _saveDataQueue.Enqueue(entry);
            }

            entry.SerializedData = data.Serialize();
        }

        public IEnumerator<T> Load<T>(ModuleBase parent) where T : SaveGameBase, new()
        {
            string path = GetPath(parent);

            if (!File.Exists(path))
            {
                this.PrintLog("Creating new saveGame for " + parent.GetType());
                yield break;
            }

            string json = File.ReadAllText(path);

            yield return null;

            var data = JsonConvert.DeserializeObject<T>(json, new StringEnumConverter());

            yield return null;

            if(data == null)
            {
                this.PrintError("Failed to read savegame for " + parent.GetType() + " | data: " + json);
                yield break;
            }

            yield return data;
        }

        //will keep running the whole time and wait until something is set dirty
        private IEnumerator SavingRoutine()
        {
            while (true)
            {
                yield return null;

                _savingDisplay.SetActive(_saveDataQueue.Count > 0);

                if (_saveDataQueue.Count == 0)
                {
                    yield return new WaitUntil(() => _saveDataQueue.Count > 0);
                    continue;
                }

                var data = _saveDataQueue.Dequeue();
                SaveSingleSavegame(data.Path, data.SerializedData);
            }
        }

        private void SaveAllDirtySync()
        {
            foreach (var data in _saveDataQueue)
            {
                SaveSingleSavegame(data.Path, data.SerializedData);
            }

            _saveDataQueue.Clear();
        }

        private void SaveSingleSavegame(string path, string serializedJson)
        {
            this.PrintLog("Saving Data for " + path.Remove(0, path.LastIndexOf('/') + 1));
            File.WriteAllText(path, serializedJson);
        }
    }
}
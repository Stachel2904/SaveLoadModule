using System;
using System.Collections;
using DivineSkies.Modules.SaveGame;

namespace DivineSkies.Modules
{
    public abstract class SaveGameModuleBase<TSaveGame, TModule> : ModuleBase<TModule> where TSaveGame : SaveGameBase, new() where TModule : Core.ModuleBase
    {
        public event Action OnDataChanged;
        public override int InitPriority => 2;
        protected TSaveGame Data { get; private set; }

        protected abstract TSaveGame CreateDefaultSaveGame();

        public override IEnumerator InitializeAsync()
        {
            var loadRoutine = SaveGameController.Main.Load<TSaveGame>(this);
            while (loadRoutine.MoveNext())
            {
                yield return loadRoutine.Current;
            }

            Data = loadRoutine.Current ?? CreateDefaultSaveGame();
            Initialize();
            SetDirty();
            yield return null;
        }

        protected void SetDirty()
        {
            SaveGameController.Main.RegisterDirtySavegame(this, Data);
            OnDataChanged?.Invoke();
        }
    }
}

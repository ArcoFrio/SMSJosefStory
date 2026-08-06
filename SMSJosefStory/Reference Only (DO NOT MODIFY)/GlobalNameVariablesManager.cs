using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Variables
{
    [AddComponentMenu("")]
    public class GlobalNameVariablesManager : Singleton<GlobalNameVariablesManager>, IGameSave
    {
        [field: NonSerialized]
        private Dictionary<IdString, NameVariableRuntime> Values { get; set; }
        [field: NonSerialized]
        private HashSet<IdString> SaveValues { get; set; }
        public string SaveID => "global-name-variables";
        public LoadMode LoadMode => LoadMode.Greedy;
        public bool IsShared => false;
        public Type SaveType => typeof(SaveGroupNameVariables);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnSubsystemsInit() { Singleton<GlobalNameVariablesManager>.Instance.WakeUp(); }
        protected override void OnCreate()
        {
            base.OnCreate();
            Values = new Dictionary<IdString, NameVariableRuntime>();
            SaveValues = new HashSet<IdString>();
            GlobalNameVariables[] nameVariables = TRepository<VariablesRepository>.Get.Variables.NameVariables;
            foreach (GlobalNameVariables globalNameVariables in nameVariables)
            {
                if (globalNameVariables == null) return;
                Singleton<GlobalNameVariablesManager>.Instance.RequireInit(globalNameVariables);
            }
            SaveLoadManager.Subscribe(this);
        }
        public bool Exists(GlobalNameVariables asset, string name)
        {
            if (Values.TryGetValue(asset.UniqueID, out var value)) return value.Exists(name);
            return false;
        }
        public object Get(GlobalNameVariables asset, string name)
        {
            if (!Values.TryGetValue(asset.UniqueID, out var value)) return null;
            return value.Get(name);
        }
        public string Title(GlobalNameVariables asset, string name)
        {
            if (!Values.TryGetValue(asset.UniqueID, out var value)) return string.Empty;
            return value.Title(name);
        }
        public Texture Icon(GlobalNameVariables asset, string name)
        {
            if (!Values.TryGetValue(asset.UniqueID, out var value)) return null;
            return value.Icon(name);
        }
        public void Set(GlobalNameVariables asset, string name, object value)
        {
            if (Values.TryGetValue(asset.UniqueID, out var value2))
            {
                value2.Set(name, value);
                if (asset.Save) SaveValues.Add(asset.UniqueID);
            }
        }
        public void Register(GlobalNameVariables asset, Action<string> callback)
        {
            if (Values.TryGetValue(asset.UniqueID, out var value)) value.EventChange += callback;
        }
        public void Unregister(GlobalNameVariables asset, Action<string> callback)
        {
            if (Values.TryGetValue(asset.UniqueID, out var value)) value.EventChange -= callback;
        }
        private void RequireInit(GlobalNameVariables asset)
        {
            if (!Values.ContainsKey(asset.UniqueID))
            {
                NameVariableRuntime nameVariableRuntime = new NameVariableRuntime(asset.NameList);
                nameVariableRuntime.OnStartup();
                Values[asset.UniqueID] = nameVariableRuntime;
            }
        }
        public object GetSaveData(bool includeNonSavable)
        {
            Dictionary<string, NameVariableRuntime> dictionary = new Dictionary<string, NameVariableRuntime>();
            foreach (KeyValuePair<IdString, NameVariableRuntime> value in Values)
            {
                if (includeNonSavable)
                {
                    dictionary[value.Key.String] = value.Value;
                    continue;
                }
                GlobalNameVariables nameVariablesAsset = TRepository<VariablesRepository>.Get.Variables.GetNameVariablesAsset(value.Key);
                if (!(nameVariablesAsset == null) && nameVariablesAsset.Save)
                {
                    dictionary[value.Key.String] = value.Value;
                }
            }
            return new SaveGroupNameVariables(dictionary);
        }
        public Task OnLoad(object value)
        {
            if (!(value is SaveGroupNameVariables saveGroupNameVariables)) return Task.FromResult(result: false);
            int num = saveGroupNameVariables.Count();
            for (int i = 0; i < num; i++)
            {
                IdString key = new IdString(saveGroupNameVariables.GetID(i));
                List<NameVariable> variables = saveGroupNameVariables.GetData(i).Variables;
                if (!Values.TryGetValue(key, out var value2)) continue;
                foreach (NameVariable item in variables)
                {
                    if (value2.Exists(item.Name)) value2.Set(item.Name, item.Value);
                }
            }
            return Task.FromResult(result: true);
        }
    }
} 
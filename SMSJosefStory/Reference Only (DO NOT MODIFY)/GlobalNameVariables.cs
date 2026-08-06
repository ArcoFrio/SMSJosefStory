using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Variables
{
    [Serializable]
    [CreateAssetMenu(fileName = "Global Variables", menuName = "Game Creator/Variables/Name Variables")]
    public class GlobalNameVariables : TGlobalVariables, INameVariable
    {
        [SerializeReference]
        private NameList m_NameList = new NameList(new NameVariable("my-variable", new ValueNumber(5f)));
        internal NameList NameList => m_NameList;
        public string[] Names => m_NameList.Names;
        public bool Exists(string name) => Singleton<GlobalNameVariablesManager>.Instance.Exists(this, name);
        public object Get(string name) => Singleton<GlobalNameVariablesManager>.Instance.Get(this, name);
        public void Set(string name, object value) => Singleton<GlobalNameVariablesManager>.Instance.Set(this, name, value);
        public void Register(Action<string> callback)
        {
            if (!ApplicationManager.IsExiting)
            {
                Singleton<GlobalNameVariablesManager>.Instance.Register(this, callback);
            }
        }
        public void Unregister(Action<string> callback)
        {
            if (!ApplicationManager.IsExiting)
            {
                Singleton<GlobalNameVariablesManager>.Instance.Unregister(this, callback);
            }
        }
        public string Title(string name) => Singleton<GlobalNameVariablesManager>.Instance.Title(this, name);
        public Texture Icon(string name) => Singleton<GlobalNameVariablesManager>.Instance.Icon(this, name);
    }
} 
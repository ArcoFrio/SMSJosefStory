using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Variables
{
    [Serializable]
    [HelpURL("https://docs.gamecreator.io/gamecreator/variables/local-name-variables")]
    [AddComponentMenu("Game Creator/Variables/Local Name Variables")]
    [DisallowMultipleComponent]
    public class LocalNameVariables : TLocalVariables, INameVariable
    {
        [SerializeReference]
        private NameVariableRuntime m_Runtime = new NameVariableRuntime();
        internal NameVariableRuntime Runtime => m_Runtime;
        public override Type SaveType => typeof(SaveSingleNameVariables);
        private event Action<string> EventChange;
        protected override void Awake()
        {
            m_Runtime.OnStartup();
            m_Runtime.EventChange += OnRuntimeChange;
            base.Awake();
        }
        public static LocalNameVariables Create(GameObject target, NameVariableRuntime variables)
        {
            LocalNameVariables localNameVariables = target.Add<LocalNameVariables>();
            localNameVariables.m_Runtime = variables;
            localNameVariables.m_Runtime.OnStartup();
            localNameVariables.m_Runtime.EventChange += localNameVariables.OnRuntimeChange;
            return localNameVariables;
        }
        public bool Exists(string name) => m_Runtime.Exists(name);
        public object Get(string name) => m_Runtime.Get(name);
        public void Set(string name, object value) => m_Runtime.Set(name, value);
        public void Register(Action<string> callback) { EventChange += callback; }
        public void Unregister(Action<string> callback) { EventChange -= callback; }
        private void OnRuntimeChange(string name) { this.EventChange?.Invoke(name); }
        public override object GetSaveData(bool includeNonSavable)
        {
            if (!m_SaveUniqueID.SaveValue) return null;
            return new SaveSingleNameVariables(m_Runtime);
        }
        public override Task OnLoad(object value)
        {
            SaveSingleNameVariables saveSingleNameVariables = value as SaveSingleNameVariables;
            if (saveSingleNameVariables != null && m_SaveUniqueID.SaveValue)
            {
                NameVariable[] array = saveSingleNameVariables.Variables.ToArray();
                foreach (NameVariable nameVariable in array)
                {
                    if (m_Runtime.Exists(nameVariable.Name))
                    {
                        m_Runtime.Set(nameVariable.Name, nameVariable.Value);
                    }
                }
            }
            return Task.FromResult(saveSingleNameVariables != null || !m_SaveUniqueID.SaveValue);
        }
    }
} 
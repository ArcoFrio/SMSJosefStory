using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Variables
{
    [Serializable]
    public class NameVariableRuntime : TVariableRuntime<NameVariable>
    {
        [SerializeReference]
        private NameList m_List = new NameList();

        internal Dictionary<string, NameVariable> Variables { get; private set; }
        public NameList TemplateList => m_List;
        public event Action<string> EventChange;

        public NameVariableRuntime() { Variables = new Dictionary<string, NameVariable>(); }
        public NameVariableRuntime(NameList nameList) : this() { m_List = nameList; }
        public NameVariableRuntime(params NameVariable[] nameList) : this() { m_List = new NameList(nameList); }

        public override void OnStartup()
        {
            Variables = new Dictionary<string, NameVariable>();
            for (int i = 0; i < m_List.Length; i++)
            {
                NameVariable nameVariable = m_List.Get(i);
                if (nameVariable != null && !Variables.ContainsKey(nameVariable.Name))
                {
                    Variables.Add(nameVariable.Name, nameVariable.Copy as NameVariable);
                }
            }
        }

        public bool Exists(string name) => Variables.ContainsKey(name);
        public object Get(string name) => AccessRuntimeVariable(name)?.Value;
        public string Title(string name) => AccessRuntimeVariable(name)?.Title;
        public Texture Icon(string name) => AccessRuntimeVariable(name)?.Icon;
        public void Set(string name, object value)
        {
            NameVariable nameVariable = AccessRuntimeVariable(name);
            if (nameVariable != null)
            {
                nameVariable.Value = value;
                this.EventChange?.Invoke(name);
            }
        }

        private NameVariable AccessRuntimeVariable(string name)
        {
            string[] array = name.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
            string key = (array.Length != 0) ? array[0] : string.Empty;
            NameVariable value;
            NameVariable nameVariable = (Variables.TryGetValue(key, out value) ? value : null);
            if (array.Length <= 1) return nameVariable;
            if (!(nameVariable?.Value is GameObject gameObject)) return null;
            LocalNameVariables localNameVariables = gameObject.Get<LocalNameVariables>();
            if (!(localNameVariables != null)) return null;
            return localNameVariables.Runtime.AccessRuntimeVariable(array[1]);
        }

        public override IEnumerator<NameVariable> GetEnumerator() => Variables.Values.GetEnumerator();
    }
} 
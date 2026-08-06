using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCreator.Runtime.Variables
{
    [Serializable]
    internal class SaveGroupNameVariables
    {
        [Serializable]
        private class Group
        {
            [SerializeField]
            private string m_ID;
            [SerializeField]
            private SaveSingleNameVariables m_Data;
            public string ID => m_ID;
            public SaveSingleNameVariables Data => m_Data;
            public Group(string id, NameVariableRuntime runtime)
            {
                m_ID = id;
                m_Data = new SaveSingleNameVariables(runtime);
            }
        }
        [SerializeField]
        private List<Group> m_Groups;
        public SaveGroupNameVariables(Dictionary<string, NameVariableRuntime> runtime)
        {
            m_Groups = new List<Group>();
            foreach (KeyValuePair<string, NameVariableRuntime> item in runtime)
            {
                m_Groups.Add(new Group(item.Key, item.Value));
            }
        }
        public int Count() => m_Groups?.Count ?? 0;
        public string GetID(int index) => m_Groups?[index].ID ?? string.Empty;
        public SaveSingleNameVariables GetData(int index) => m_Groups?[index].Data;
    }
} 
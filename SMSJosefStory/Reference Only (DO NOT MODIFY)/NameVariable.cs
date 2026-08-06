using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Variables
{
    [Serializable]
    public class NameVariable : TVariable
    {
        [SerializeField]
        private IdString m_Name;

        public string Name => m_Name.String;

        public override string Title => $"{m_Name.String}: {m_Value}";

        public override TVariable Copy => new NameVariable
        {
            m_Name = m_Name,
            m_Value = m_Value.Copy
        };

        public NameVariable() { }

        public NameVariable(IdString typeID)
            : base(typeID)
        {
        }

        public NameVariable(string name, TValue value)
            : this()
        {
            m_Name = new IdString(name);
            m_Value = value;
        }
    }
} 
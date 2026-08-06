using System;
using System.Reflection;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Variables
{
    [Serializable]
    public abstract class TVariable : TPolymorphicItem<TVariable>
    {
        [SerializeReference]
        protected TValue m_Value = new ValueNull();

        public object Value
        {
            get { return m_Value?.Value; }
            set { if (m_Value != null) m_Value.Value = value; }
        }

        public IdString TypeID => m_Value.TypeID;
        public Type Type => m_Value.Type;
        public Texture Icon => m_Value.GetType().GetCustomAttribute<ImageAttribute>()?.Image;

        public abstract override string Title { get; }
        public abstract TVariable Copy { get; }

        protected TVariable() { }
        protected TVariable(IdString typeID) { m_Value = TValue.CreateValue(typeID); }
    }
} 
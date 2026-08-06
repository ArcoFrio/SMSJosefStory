using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Variables
{
    [Serializable]
    public abstract class TList<T> : TPolymorphicList<T> where T : TVariable
    {
        [SerializeReference]
        private List<T> m_Source = new List<T>();
        public override int Length => m_Source.Count;
        protected TList() { }
        protected TList(params T[] variables) : this() { m_Source = new List<T>(variables); }
        public T Get(int index) => m_Source[index];
        public void Set(int index, T value) { m_Source[index] = value; }
        public void Add(T value) { m_Source.Add(value); }
        public void Remove(int index) { m_Source.RemoveAt(index); }
    }
} 
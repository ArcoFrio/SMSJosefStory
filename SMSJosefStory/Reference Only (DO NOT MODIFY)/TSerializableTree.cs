using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    [Serializable]
    public class TSerializableTree<TValue> where TValue : class
    {
        public const string NAME_DATA = "m_Data";
        public const string NAME_NODES = "m_Nodes";
        public const string NAME_ROOTS = "m_Roots";
        public const string NAME_DATA_KEYS = "m_Keys";
        public const string NAME_DATA_VALUES = "m_Values";
        public const string NAME_NODES_KEYS = "m_Keys";
        public const string NAME_NODES_VALUES = "m_Values";
        public const string NAME_NODE_CHILDREN = "m_Children";
        public const int NODE_INVALID = -1;

        [SerializeField]
        internal int m_Dirty;

        [SerializeField]
        protected TTreeData<TValue> m_Data;

        [SerializeField]
        protected TreeNodes m_Nodes;

        [SerializeField]
        protected List<int> m_Roots;

        public int[] RootIds
        {
            get { return m_Roots.ToArray(); }
            set { m_Roots = new List<int>(value); }
        }

        public int FirstRootId => m_Roots.Count <= 0 ? -1 : m_Roots[0];
        public TreeNodes Nodes => m_Nodes;
        public event Action EventChange;

        public TSerializableTree()
        {
            m_Data = new TTreeData<TValue>();
            m_Nodes = new TreeNodes();
            m_Roots = new List<int>();
        }

        public bool Contains(int id) => m_Data.ContainsKey(id);

        public int SiblingIndex(int id)
        {
            int num = Parent(id);
            int[] array = (num == -1) ? RootIds : (Children(num)?.ToArray() ?? Array.Empty<int>());
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == id) return i;
            }
            return -1;
        }

        public TValue Get(int id)
        {
            if (!m_Data.TryGetValue(id, out var value)) return null;
            return value.Value;
        }

        public int Parent(int id)
        {
            if (!m_Nodes.TryGetValue(id, out var value)) return -1;
            return value.Parent;
        }

        public int PreviousSibling(int id)
        {
            int num = SiblingIndex(id);
            int num2 = Parent(id);
            int[] array = (num2 == -1) ? RootIds : (Children(num2)?.ToArray() ?? Array.Empty<int>());
            int num3 = num - 1;
            if (num3 < 0) return -1;
            return array[num3];
        }

        public int NextSibling(int id)
        {
            int num = SiblingIndex(id);
            int num2 = Parent(id);
            int[] array = (num2 == -1) ? RootIds : (Children(num2)?.ToArray() ?? Array.Empty<int>());
            int num3 = num + 1;
            if (num3 >= array.Length) return -1;
            return array[num3];
        }

        public List<int> Children(int id)
        {
            if (!m_Nodes.TryGetValue(id, out var value)) return new List<int>();
            return new List<int>(value.Children);
        }

        public List<int> Siblings(int id)
        {
            int num = Parent(id);
            if (num == -1) return new List<int>(RootIds);
            return Children(num);
        }

        public int AddToRoot(TValue value) => Add(value, -1, m_Roots.Count);
        public int AddToRoot(TValue value, int index) => Add(value, -1, index);
        public int AddBeforeSibling(TValue value, int sibling)
        {
            if (!Contains(sibling)) return -1;
            int num = Parent(sibling);
            int index = (num != -1) ? m_Nodes[num].Children.IndexOf(sibling) : m_Roots.IndexOf(sibling);
            return Add(value, num, index);
        }
        public int AddAfterSibling(TValue value, int sibling)
        {
            if (!Contains(sibling)) return -1;
            int num = Parent(sibling);
            int num2 = (num != -1) ? m_Nodes[num].Children.IndexOf(sibling) : m_Roots.IndexOf(sibling);
            return Add(value, num, num2 + 1);
        }
        public int AddChild(TValue value, int parent)
        {
            if (!Contains(parent)) return -1;
            List<int> children = m_Nodes[parent].Children;
            return Add(value, parent, children.Count);
        }
        public int AddChild(TValue value, int parent, int index) => Add(value, parent, index);
        public bool Remove(int node)
        {
            if (node == -1) return false;
            int num = Parent(node);
            List<int> list = Children(node);
            for (int num2 = list.Count - 1; num2 >= 0; num2--) Remove(list[num2]);
            TreeNode value;
            if (num == -1)
            {
                for (int num3 = m_Roots.Count - 1; num3 >= 0; num3--)
                {
                    if (m_Roots[num3] == node) m_Roots.RemoveAt(num3);
                }
            }
            else if (m_Nodes.TryGetValue(num, out value))
            {
                value.Children.Remove(node);
            }
            m_Data.Remove(node);
            m_Nodes.Remove(node);
            return true;
        }
        private int NewId()
        {
            int hashCode = Guid.NewGuid().GetHashCode();
            while (m_Data.ContainsKey(hashCode) || hashCode == -1)
            {
                hashCode = Guid.NewGuid().GetHashCode();
            }
            return hashCode;
        }
        private int Add(TValue value, int parent, int index)
        {
            int num = NewId();
            m_Data.Add(num, new TTreeDataItem<TValue>(num, value));
            if (parent != -1)
            {
                m_Nodes[parent].Children.Insert(index, num);
            }
            else
            {
                m_Roots.Insert(index, num);
            }
            TreeNode value2 = new TreeNode(num, parent);
            m_Nodes.Add(num, value2);
            this.EventChange?.Invoke();
            return num;
        }
    }
} 
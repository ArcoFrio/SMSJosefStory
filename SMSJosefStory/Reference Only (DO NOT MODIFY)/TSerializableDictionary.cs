using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCreator.Runtime.Common;

[Serializable]
public abstract class TSerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, ISerializationCallbackReceiver
{
    public const string NAME_KEYS = "m_Keys";

    public const string NAME_VALUES = "m_Values";

    [NonSerialized]
    protected Dictionary<TKey, TValue> m_Dictionary = new Dictionary<TKey, TValue>();

    [SerializeField]
    private TKey[] m_Keys;

    [SerializeField]
    private TValue[] m_Values;

    public int Count => m_Dictionary.Count;

    public ICollection<TKey> Keys => m_Dictionary.Keys;

    public ICollection<TValue> Values => m_Dictionary.Values;

    public TValue this[TKey key]
    {
        get
        {
            return m_Dictionary[key];
        }
        set
        {
            m_Dictionary[key] = value;
        }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        m_Dictionary.Add(key, value);
    }

    public bool ContainsKey(TKey key)
    {
        return m_Dictionary.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        return m_Dictionary.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return m_Dictionary.TryGetValue(key, out value);
    }

    public void Clear()
    {
        m_Dictionary.Clear();
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)m_Dictionary).Add(item);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>)m_Dictionary).Contains(item);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)m_Dictionary).CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>)m_Dictionary).Remove(item);
    }

    public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
    {
        return m_Dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return m_Dictionary.GetEnumerator();
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return m_Dictionary.GetEnumerator();
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        if (!AssemblyUtils.IsReloading)
        {
            BeforeSerialize();
        }
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        if (!AssemblyUtils.IsReloading)
        {
            AfterSerialize();
        }
    }

    protected virtual void BeforeSerialize()
    {
        if (m_Dictionary == null || m_Dictionary.Count == 0)
        {
            m_Keys = null;
            m_Values = null;
            return;
        }

        int count = m_Dictionary.Count;
        m_Keys = new TKey[count];
        m_Values = new TValue[count];
        int num = 0;
        using Dictionary<TKey, TValue>.Enumerator enumerator = m_Dictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            m_Keys[num] = enumerator.Current.Key;
            m_Values[num] = enumerator.Current.Value;
            num++;
        }
    }

    protected virtual void AfterSerialize()
    {
        if (m_Dictionary == null)
        {
            m_Dictionary = new Dictionary<TKey, TValue>();
        }

        m_Dictionary.Clear();
        if (m_Keys != null && m_Values != null)
        {
            for (int i = 0; i < m_Keys.Length; i++)
            {
                m_Dictionary[m_Keys[i]] = ((i < m_Values.Length) ? m_Values[i] : default(TValue));
            }
        }
    }
} 
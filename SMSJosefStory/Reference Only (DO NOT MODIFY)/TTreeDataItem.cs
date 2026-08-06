using System;
using UnityEngine;

namespace GameCreator.Runtime.Common;

[Serializable]
public class TTreeDataItem<T> where T : class
{
    public const string NAME_ID = "m_Id";

    public const string NAME_VALUE = "m_Value";

    [SerializeField]
    private int m_Id;

    [SerializeReference]
    private T m_Value;

    public int Id => m_Id;

    public T Value
    {
        get
        {
            return m_Value;
        }
        set
        {
            m_Value = value;
        }
    }

    public TTreeDataItem(int nodeId, T value)
    {
        m_Id = nodeId;
        m_Value = value;
    }
} 
using System;
using UnityEngine;

namespace GameCreator.Runtime.Common;

[Serializable]
public abstract class TPropertyGet<TType, TValue> : IProperty where TType : TPropertyTypeGet<TValue>
{
    [SerializeReference]
    protected TType m_Property;

    public TValue EditorValue => m_Property.EditorValue;

    protected TPropertyGet(TType defaultType)
    {
        m_Property = defaultType;
    }

    public virtual TValue Get(Args args)
    {
        if (m_Property == null)
        {
            return default(TValue);
        }

        return m_Property.Get(args);
    }

    public virtual TValue Get(GameObject target)
    {
        if (m_Property == null)
        {
            return default(TValue);
        }

        return m_Property.Get(target);
    }

    public virtual TValue Get(Component component)
    {
        return Get((component != null) ? component.gameObject : null);
    }

    public override string ToString()
    {
        return m_Property?.String ?? "(none)";
    }
} 
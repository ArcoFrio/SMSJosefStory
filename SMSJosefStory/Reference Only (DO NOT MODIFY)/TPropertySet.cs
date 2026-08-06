using System;
using UnityEngine;

namespace GameCreator.Runtime.Common;

[Serializable]
public abstract class TPropertySet<TType, TValue> : IProperty where TType : TPropertyTypeSet<TValue>
{
    [SerializeReference]
    protected TType m_Property;

    protected TPropertySet(TType defaultType)
    {
        m_Property = defaultType;
    }

    public virtual void Set(TValue value, Args args)
    {
        m_Property.Set(value, args);
    }

    public virtual void Set(TValue value, GameObject target)
    {
        m_Property.Set(value, target);
    }

    public virtual void Set(TValue value, Component component)
    {
        Set(value, component ? component.gameObject : null);
    }

    public virtual TValue Get(Args args)
    {
        return m_Property.Get(args);
    }

    public virtual TValue Get(GameObject target)
    {
        return m_Property.Get(target);
    }

    public virtual TValue Set(Component component)
    {
        return Get(component ? component.gameObject : null);
    }

    public override string ToString()
    {
        return m_Property?.String ?? "(none)";
    }
} 
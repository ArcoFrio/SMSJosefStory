using UnityEngine;

namespace GameCreator.Runtime.Common;

[Image(typeof(IconCircleSolid), ColorTheme.Type.Green)]
public abstract class TPropertyTypeSet<T>
{
    public abstract string String { get; }

    public virtual void Set(T value, Args args)
    {
    }

    public virtual void Set(T value, GameObject gameObject)
    {
        Set(value, new Args(gameObject));
    }

    public virtual T Get(Args args)
    {
        return default(T);
    }

    public virtual T Get(GameObject gameObject)
    {
        return Get(new Args(gameObject));
    }
} 
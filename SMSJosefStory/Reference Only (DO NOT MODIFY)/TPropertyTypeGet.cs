using UnityEngine;

namespace GameCreator.Runtime.Common;

[Image(typeof(IconCircleSolid), ColorTheme.Type.Green)]
public abstract class TPropertyTypeGet<T>
{
    public abstract string String { get; }

    public virtual T EditorValue { get; }

    public virtual T Get(Args args)
    {
        return default(T);
    }

    public virtual T Get(GameObject gameObject)
    {
        return Get(new Args(gameObject));
    }
} 
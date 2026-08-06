using System;
using UnityEngine;

namespace GameCreator.Runtime.Common;

[Serializable]
public abstract class TPolymorphicItem<TType> : IPolymorphicItem
{
    [SerializeField]
    [HideInInspector]
    private bool m_Breakpoint;

    [SerializeField]
    [HideInInspector]
    private bool m_IsEnabled = true;

    public Type BaseType => typeof(TType);

    public Type FullType => GetType();

    public bool Breakpoint => m_Breakpoint;

    public bool IsEnabled => m_IsEnabled;

    public virtual Color Color => ColorTheme.Get(ColorTheme.Type.TextNormal);

    public virtual string Title => TextUtils.Humanize(GetType().ToString());
} 
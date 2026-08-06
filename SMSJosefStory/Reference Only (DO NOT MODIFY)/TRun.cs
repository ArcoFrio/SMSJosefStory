using System;
using UnityEngine;

namespace GameCreator.Runtime.Common;

[Serializable]
public abstract class TRun<TValue>
{
    [NonSerialized]
    protected GameObject m_Template;

    protected abstract TValue Value { get; }

    protected abstract GameObject Template { get; }
} 
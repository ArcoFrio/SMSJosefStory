using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting;

[Serializable]
public abstract class Condition : TPolymorphicItem<Condition>
{
    [SerializeField]
    [HideInInspector]
    private bool m_Sign = true;

    public sealed override string Title => string.Format("{0} [object Object]1, m_Sign ? "If :Not", Summary);

    protected virtual string Summary => TextUtils.Humanize(GetType().ToString());

    public bool Check(Args args)
 [object Object]
        if (!base.IsEnabled)
   [object Object]            return m_Sign;
        }

        if (base.Breakpoint)
   [object Object]
            Debug.Break();
        }

        if (!m_Sign)
   [object Object]            return !Run(args);
        }

        return Run(args);
    }

    protected abstract bool Run(Args args);
} 
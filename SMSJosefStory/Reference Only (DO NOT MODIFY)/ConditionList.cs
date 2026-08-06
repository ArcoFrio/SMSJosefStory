using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting;

[Serializable]
public class ConditionList : TPolymorphicList<Condition>
{
    [SerializeReference]
    private Condition[] m_Conditions = Array.Empty<Condition>();

    public override int Length => m_Conditions.Length;

    public event Action EventStartCheck;

    public event Action EventEndCheck;

    public ConditionList()
    {
    }

    public ConditionList(params Condition[] conditions)
        : this()
    {
        m_Conditions = conditions;
    }

    public bool Check(Args args, CheckMode mode)
    {
        this.EventStartCheck?.Invoke();
        Condition[] conditions = m_Conditions;
        foreach (Condition condition in conditions)
        {
            if (condition == null)
            {
                continue;
            }

            bool flag = condition.Check(args);
            switch (mode)
            {
                case CheckMode.And:
                    if (!flag)
                    {
                        this.EventEndCheck?.Invoke();
                        return false;
                    }

                    break;
                case CheckMode.Or:
                    if (flag)
                    {
                        this.EventEndCheck?.Invoke();
                        return true;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException("mode", mode, null);
            }
        }

        this.EventEndCheck?.Invoke();
        return mode == CheckMode.And;
    }

    public Condition Get(int index)
    {
        index = Mathf.Clamp(index, 0, Length - 1);
        return m_Conditions[index];
    }

    public override string ToString()
    {
        return m_Conditions.Length switch
        {
            0 => string.Empty,
            1 => m_Conditions[0]?.Title,
            _ => $"{m_Conditions[0]?.Title} +{m_Conditions.Length - 1}",
        };
    }

    public string ToString(string join)
    {
        return m_Conditions.Length switch
        {
            0 => string.Empty,
            1 => m_Conditions[0]?.Title,
            2 => m_Conditions[0]?.Title + " " + join + " " + m_Conditions[1]?.Title,
            _ => $"{m_Conditions[0]?.Title} {join} {m_Conditions[1]?.Title} +{m_Conditions.Length - 2}",
        };
    }
} 
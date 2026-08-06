using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting;

[Serializable]
[Title("Compare Bool")]
[Description("Returns true if a comparison between two boolean values is satisfied")]
[Category("Math/Boolean/Compare Boolean")]
[Parameter("Value", "The boolean value that is being compared")]
[Parameter("Comparison", "The comparison operation performed between both values")]
[Parameter("Compare To", "The boolean value that is compared against")]
[Keywords(new string[] { "Boolean" })]
[Image(typeof(IconToggleOn), ColorTheme.Type.Yellow)]
public class ConditionMathCompareBooleans : Condition
{
    private enum Comparison
    {
        Equals,
        Different
    }

    [SerializeField]
    private PropertyGetBool m_Value = new PropertyGetBool(value: true);

    [SerializeField]
    private Comparison m_Comparison;

    [SerializeField]
    private PropertyGetBool m_CompareTo = GetBoolTrue.Create;

    protected override string Summary
    {
        get
        {
            object value = m_Value;
            return string.Format("{0} {1} {2}", value, m_Comparison switch
            {
                Comparison.Equals => "=",
                Comparison.Different => "≠",
                _ => string.Empty,
            }, m_CompareTo);
        }
    }

    protected override bool Run(Args args)
    {
        bool flag = m_Value.Get(args);
        bool flag2 = m_CompareTo.Get(args);
        return m_Comparison switch
        {
            Comparison.Equals => flag == flag2,
            Comparison.Different => flag != flag2,
            _ => throw new ArgumentOutOfRangeException($"Boolean Comparison '{m_Comparison}' not found"),
        };
    }
}
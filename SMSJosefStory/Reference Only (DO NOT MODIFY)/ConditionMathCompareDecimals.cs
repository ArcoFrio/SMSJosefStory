using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting;

[Serializable]
[Title("Compare Decimal")]
[Description("Returns true if a comparison between two decimal values is satisfied")]
[Category("Math/Arithmetic/Compare Decimal")]
[Parameter("Value", "The decimal value that is being compared")]
[Parameter("Comparison", "The comparison operation performed between both values")]
[Parameter("Compare To", "The decimal value that is compared against")]
[Keywords(new string[] { "Number", "Float", "Comma", "Equals", "Different", "Bigger", "Greater", "Larger", "Smaller" })]
[Image(typeof(IconNumber), ColorTheme.Type.Blue)]
public class ConditionMathCompareDecimals : Condition
{
    [SerializeField]
    private PropertyGetDecimal m_Value = new PropertyGetDecimal(0f);

    [SerializeField]
    private CompareDouble m_CompareTo = new CompareDouble();

    protected override string Summary => $"{m_Value} {m_CompareTo}";

    protected override bool Run(Args args)
    {
        double value = m_Value.Get(args);
        return m_CompareTo.Match(value, args);
    }
}
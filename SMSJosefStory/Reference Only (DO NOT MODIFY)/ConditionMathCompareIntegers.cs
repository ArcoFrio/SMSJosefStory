using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting;

[Serializable]
[Title("Compare Integer")]
[Description("Returns true if a comparison between two integer values is satisfied")]
[Category("Math/Arithmetic/Compare Integer")]
[Parameter("Value", "The integer value that is being compared")]
[Parameter("Comparison", "The comparison operation performed between both values")]
[Parameter("Compare To", "The integer value that is compared against")]
[Keywords(new string[] { "Number", "Whole", "Equals", "Different", "Bigger", "Greater", "Larger", "Smaller" })]
[Image(typeof(IconNumber), ColorTheme.Type.Blue)]
public class ConditionMathCompareIntegers : Condition
{
    [SerializeField]
    private PropertyGetInteger m_Value = new PropertyGetInteger(0);

    [SerializeField]
    private CompareInteger m_CompareTo = new CompareInteger();

    protected override string Summary => $"{m_Value} {m_CompareTo}";

    protected override bool Run(Args args)
    {
        int value = (int)m_Value.Get(args);
        return m_CompareTo.Match(value, args);
    }
}
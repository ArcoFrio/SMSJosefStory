using System;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.VisualScripting;

[Serializable]
[Title("Always True")]
[Description("Always returns true")]
[Category("Math/Boolean/Always True")]
[Keywords(new string[] { "Boolean", "Yes", "Tautology" })]
[Image(typeof(IconToggleOn), ColorTheme.Type.Green)]
public class ConditionMathAlwaysTrue : Condition
{
    protected override string Summary => "True";

    protected override bool Run(Args args)
    {
        return true;
    }
} 
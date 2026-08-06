using System;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.VisualScripting;

[Serializable]
[Title("Always False")]
[Description("Always returns false")]
[Category("Math/Boolean/Always False")]
[Keywords(new string[] { "Boolean", "No", "Contradiction" })]
[Image(typeof(IconToggleOff), ColorTheme.Type.Red)]
public class ConditionMathAlwaysFalse : Condition
{
    protected override string Summary => "False";

    protected override bool Run(Args args)
    {
        return false;
    }
}
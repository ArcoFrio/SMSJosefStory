using System;
using UnityEngine;

namespace GameCreator.Runtime.Common;

[Serializable]
[Title("String")]
[Category("Constants/String")]
[Image(typeof(IconString), ColorTheme.Type.Yellow)]
[Description("A string of characters")]
[Keywords(new string[] { "String", "Value" })]
[HideLabelsInEditor(true)]
public class GetStringString : PropertyTypeGetString
{
    [SerializeField]
    protected string m_Value = "";

    public static PropertyGetString Create => new PropertyGetString(new GetStringString());

    public override string String
    {
        get
        {
            if (!string.IsNullOrEmpty(m_Value))
            {
                return m_Value;
            }

            return "<empty>";
        }
    }

    public override string EditorValue => m_Value;

    public override string Get(Args args)
    {
        return m_Value;
    }

    public override string Get(GameObject gameObject)
    {
        return m_Value;
    }

    public GetStringString()
    {
    }

    public GetStringString(string value = "")
        : this()
    {
        m_Value = value;
    }
} 
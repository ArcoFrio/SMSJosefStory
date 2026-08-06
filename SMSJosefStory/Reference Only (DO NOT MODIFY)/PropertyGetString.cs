using System;

namespace GameCreator.Runtime.Common;

[Serializable]
public class PropertyGetString : TPropertyGet<PropertyTypeGetString, string>
{
    public PropertyGetString()
        : base((PropertyTypeGetString)new GetStringString())
    {
    }

    public PropertyGetString(PropertyTypeGetString defaultType)
        : base(defaultType)
    {
    }

    public PropertyGetString(string value)
        : base((PropertyTypeGetString)new GetStringString(value))
    {
    }
} 
using System;

namespace GameCreator.Runtime.Common;

[Serializable]
public class PropertyGetBool : TPropertyGet<PropertyTypeGetBool, bool>
{
    public PropertyGetBool()
        : base((PropertyTypeGetBool)new GetBoolValue())
    {
    }

    public PropertyGetBool(PropertyTypeGetBool defaultType)
        : base(defaultType)
    {
    }

    public PropertyGetBool(bool value)
        : base((PropertyTypeGetBool)new GetBoolValue(value))
    {
    }
}
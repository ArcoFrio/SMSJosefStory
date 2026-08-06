using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Variables;

[Serializable]
[Title("Value Type")]
[Image(typeof(IconCircleOutline), ColorTheme.Type.Yellow)]
public abstract class TValue : TPolymorphicItem<TValue>
{
    protected class TypeData
    {
        public readonly Type type;

        public readonly Func<object, TValue> callback;

        public TypeData(Type type, Func<object, TValue> callback)
        {
            this.type = type;
            this.callback = callback;
        }
    }

    private class Type_LUT : Dictionary<IdString, TypeData>
    {
    }

    private class ID_LUT : Dictionary<Type, IdString>
    {
    }

    private class ConverterType_LUT : Dictionary<Type, IdString>
    {
    }

    private static readonly Type_LUT LUT_ID_TO_DATA = new Type_LUT();

    private static readonly ID_LUT LUT_TYPE_TO_ID = new ID_LUT();

    private static readonly ConverterType_LUT LUT_CONVERTER = new ConverterType_LUT();

    public object Value
    {
        get
        {
            return Get();
        }
        set
        {
            if (Get() != value)
            {
                Set(value);
                this.EventChange?.Invoke(Get());
            }
        }
    }

    public override string Title => ToString();

    public abstract IdString TypeID { get; }

    public abstract Type Type { get; }

    public abstract bool CanSave { get; }

    public abstract TValue Copy { get; }

    public event Action<object> EventChange;

    protected abstract object Get();

    protected abstract void Set(object value);

    public abstract override string ToString();

    protected static void RegisterValueType(IdString typeID, TypeData data, Type convertFrom)
    {
        ((Dictionary<IdString, TypeData>)LUT_ID_TO_DATA).TryAdd(typeID, data);
        ((Dictionary<Type, IdString>)LUT_TYPE_TO_ID).TryAdd(data.type, typeID);
        if (!(convertFrom == null))
        {
            ((Dictionary<Type, IdString>)LUT_CONVERTER).TryAdd(convertFrom, typeID);
        }
    }

    public static TValue CreateValue(IdString typeID, object value = null)
    {
        if (!LUT_ID_TO_DATA.TryGetValue(typeID, out var value2))
        {
            return new ValueNull();
        }

        return value2.callback(value);
    }

    public static Type GetType(IdString typeID)
    {
        if (!LUT_ID_TO_DATA.TryGetValue(typeID, out var value))
        {
            return typeof(ValueNull);
        }

        return value.type;
    }

    public static IdString GetTypeIDFromValueType(Type type)
    {
        if (!LUT_TYPE_TO_ID.TryGetValue(type, out var value))
        {
            return ValueNull.TYPE_ID;
        }

        return value;
    }

    public static IdString GetTypeIDFromObjectType(Type objectType)
    {
        if (!LUT_CONVERTER.TryGetValue(objectType, out var value))
        {
            return ValueNull.TYPE_ID;
        }

        return value;
    }
} 
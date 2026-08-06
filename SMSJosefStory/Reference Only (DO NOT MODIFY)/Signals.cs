
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCreator.Runtime.Common;

public static class Signals
{
    [NonSerialized]
    private static Dictionary<PropertyName, List<ISignalReceiver>> SIGNALS = new Dictionary<PropertyName, List<ISignalReceiver>>();

    public static void Emit(SignalArgs args)
    {
        if (ApplicationManager.IsExiting || !SIGNALS.TryGetValue(args.signal, out var value))
        {
            return;
        }

        foreach (ISignalReceiver item in value)
        {
            item.OnReceiveSignal(args);
        }
    }

    public static void Subscribe(ISignalReceiver source, PropertyName signal)
    {
        if (ApplicationManager.IsExiting)
        {
            return;
        }

        if (!SIGNALS.TryGetValue(signal, out var value))
        {
            value = new List<ISignalReceiver>();
            SIGNALS.Add(signal, value);
        }

        foreach (ISignalReceiver item in value)
        {
            if (item == source)
            {
                return;
            }
        }

        value.Add(source);
    }

    public static void Unsubscribe(ISignalReceiver source, PropertyName signal)
    {
        if (!ApplicationManager.IsExiting && SIGNALS.TryGetValue(signal, out var value))
        {
            value.Remove(source);
            if (value.Count <= 0)
            {
                SIGNALS.Remove(signal);
            }
        }
    }
}
using System;
using UnityEngine;

namespace GameCreator.Runtime.Common;

[Serializable]
public abstract class TRunner<TValue> : Runner
{
    [SerializeField]
    protected TValue m_Value;

    public TValue Value => m_Value;

    public static GameObject CreateTemplate<TRunnerType>(TValue value) where TRunnerType : TRunner<TValue>
    {
        HideFlags hideFlags = HideFlags.HideAndDontSave;
        GameObject gameObject = new GameObject
        {
            hideFlags = hideFlags
        };
        GameObject obj = new GameObject
        {
            hideFlags = HideFlags.None
        };
        gameObject.name = gameObject.GetInstanceID().ToString();
        obj.transform.SetParent(gameObject.transform);
        obj.Add<TRunnerType>().m_Value = value;
        return obj;
    }

    public static TRunnerType Pick<TRunnerType>(GameObject template, RunnerConfig config, int prewarmCounter) where TRunnerType : TRunner<TValue>
    {
        if (template == null)
        {
            return null;
        }

        Vector3 position = config.Location.Position;
        Quaternion quaternion = config.Location.Rotation;
        if (config.Location.Parent != null)
        {
            position = config.Location.Parent.TransformPoint(position);
            quaternion = config.Location.Parent.rotation * quaternion;
        }

        int instanceID = template.GetInstanceID();
        if (!Runner.Pool.ContainsKey(instanceID))
        {
            template.transform.parent.gameObject.name = config.Name;
            template.name = config.Name ?? "";
            RunnerPool value = new RunnerPool(template, prewarmCounter);
            Runner.Pool.Add(instanceID, value);
        }

        TRunnerType val = Runner.Pool[instanceID].Pick<TRunnerType>();
        if ((UnityEngine.Object)val == (UnityEngine.Object)null)
        {
            return null;
        }

        val.transform.SetPositionAndRotation(position, quaternion);
        if (config.Location.Parent != null)
        {
            val.transform.SetParent(config.Location.Parent);
        }

        val.gameObject.SetActive(value: true);
        return val.Get<TRunnerType>();
    }

    public static void Restore(Runner runner)
    {
        if (!(runner == null))
        {
            if (runner.Template == null)
            {
                UnityEngine.Object.Destroy(runner.gameObject);
            }

            if (Runner.Pool.TryGetValue(runner.Template.GetInstanceID(), out var value))
            {
                value.Restore(runner.gameObject);
            }
        }
    }
} 
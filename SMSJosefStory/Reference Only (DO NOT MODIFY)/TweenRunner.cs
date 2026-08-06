using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCreator.Runtime.Common;

[AddComponentMenu("")]
internal class TweenRunner : MonoBehaviour
{
    [NonSerialized]
    private readonly List<ITweenInput> m_Inputs = new List<ITweenInput>();

    private void OnEnable()
    {
        base.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
    }

    private void OnDisable()
    {
        CancelAll();
    }

    private void Update()
    {
        for (int num = m_Inputs.Count - 1; num >= 0; num--)
        {
            if (m_Inputs[num].OnUpdate())
            {
                m_Inputs[num].OnComplete();
                m_Inputs.RemoveAt(num);
            }
        }
    }

    public void To(ITweenInput input)
    {
        Cancel(input.Hash);
        if (input.Duration <= float.Epsilon)
        {
            input.OnUpdate();
            input.OnComplete();
        }
        else
        {
            m_Inputs.Add(input);
        }
    }

    public void Cancel(int hash)
    {
        for (int num = m_Inputs.Count - 1; num >= 0; num--)
        {
            if (m_Inputs[num].Hash == hash)
            {
                m_Inputs[num].OnCancel();
                m_Inputs.RemoveAt(num);
            }
        }
    }

    public void CancelAll()
    {
        for (int num = m_Inputs.Count - 1; num >= 0; num--)
        {
            m_Inputs[num].OnCancel();
            m_Inputs.RemoveAt(num);
        }
    }
}
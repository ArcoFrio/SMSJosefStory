using System;
using UnityEngine;

namespace GameCreator.Runtime.Common;

[Serializable]
public abstract class BaseTextArea
{
    [SerializeField]
    private string m_Text;

    public string Text => m_Text;

    protected BaseTextArea()
    {
        m_Text = string.Empty;
    }

    protected BaseTextArea(string text)
        : this()
    {
        m_Text = text;
    }
} 
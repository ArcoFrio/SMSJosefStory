using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue;

[Serializable]
public class NodeText : TPolymorphicList<NodeText.NodeTextValue>
{
    [Serializable]
    public class NodeTextValue : TPolymorphicItem<NodeTextValue>
    {
        [SerializeField]
        private PropertyGetString m_Value = GetStringEmpty.Create;

        [SerializeField]
        private bool m_InBold;

        [SerializeField]
        private bool m_InItalic;

        [SerializeField]
        private bool m_UseColor;

        [SerializeField]
        private PropertyGetColor m_Color = GetColorColorsYellow.Create;

        public bool InBold => m_InBold;

        public bool InItalic => m_InItalic;

        public bool UseColor => m_UseColor;

        public string GetText(Args args)
        {
            return m_Value.Get(args);
        }

        public Color GetColor(Args args)
        {
            return m_Color.Get(args);
        }

        public override string ToString()
        {
            return m_Value.ToString();
        }
    }

    [SerializeField]
    private PropertyGetString m_Text = GetStringTextArea.Create();

    [SerializeReference]
    private NodeTextValue[] m_Values = Array.Empty<NodeTextValue>();

    [field: NonSerialized]
    public string Value { get; private set; }

    public override int Length => m_Values.Length;

    public NodeText()
    {
    }

    public NodeText(string text)
        : this()
    {
        m_Text = GetStringTextArea.Create(text);
    }

    public void Init(Args args)
    {
        Value = Get(args);
    }

    public string Get(Args args)
    {
        string text = m_Text.Get(args);
        for (int i = 0; i < m_Values.Length; i++)
        {
            NodeTextValue nodeTextValue = m_Values[i];
            if (nodeTextValue != null)
            {
                string oldValue = $"{{{i}}}";
                string text2 = nodeTextValue.GetText(args);
                if (nodeTextValue.InBold)
                {
                    text2 = "<b>" + text2 + "</b>";
                }

                if (nodeTextValue.InItalic)
                {
                    text2 = "<i>" + text2 + "</i>";
                }

                if (nodeTextValue.UseColor)
                {
                    string text3 = ColorUtility.ToHtmlStringRGBA(nodeTextValue.GetColor(args));
                    text2 = "<color=#" + text3 + ">" + text2 + "</color>";
                }

                text = text.Replace(oldValue, text2);
            }
        }

        Value[] get = TRepository<DialogueRepository>.Get.Values.Get;
        foreach (Value value in get)
        {
            string oldValue2 = "{" + value.Key + "}";
            string text4 = value.GetText(args);
            if (value.InBold)
            {
                text4 = "<b>" + text4 + "</b>";
            }

            if (value.InItalic)
            {
                text4 = "<i>" + text4 + "</i>";
            }

            if (value.UseColor)
            {
                string text5 = ColorUtility.ToHtmlStringRGBA(value.GetColor(args));
                text4 = "<color=#" + text5 + ">" + text4 + "</color>";
            }

            text = text.Replace(oldValue2, text4);
        }

        return text;
    }

    public override string ToString()
    {
        return m_Text.ToString();
    }
} 
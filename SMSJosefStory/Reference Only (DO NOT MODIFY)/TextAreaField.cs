using System;

namespace GameCreator.Runtime.Common;

[Serializable]
public class TextAreaField : BaseTextArea
{
    public TextAreaField()
    {
    }

    public TextAreaField(string text)
        : base(text)
    {
    }
} 
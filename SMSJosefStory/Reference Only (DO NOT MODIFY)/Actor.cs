using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue;

[CreateAssetMenu(fileName = "Actor", menuName = "Game Creator/Dialogue/Actor")]
public class Actor : ScriptableObject
{
    [SerializeField]
    private Actant m_Actant = new Actant();

    [SerializeField]
    private Expressions m_Expressions = new Expressions();

    [SerializeField]
    private Typewriter m_Typewriter = new Typewriter();

    [SerializeField]
    private SpeechSkin m_OverrideSpeechSkin;

    [SerializeField]
    private Portrait m_Portrait = Portrait.Primary;

    public int ExpressionsLength => m_Expressions.Length;

    public Typewriter Typewriter => m_Typewriter;

    public SpeechSkin OverrideSpeechSkin => m_OverrideSpeechSkin;

    public Portrait Portrait => m_Portrait;

    public string GetName(Args args)
    {
        return m_Actant.GetName(args);
    }

    public string GetDescription(Args args)
    {
        return m_Actant.GetDescription(args);
    }

    public Expression GetExpressionFromId(IdString id)
    {
        return m_Expressions.FromId(id);
    }

    public Expression GetExpressionFromIndex(int index)
    {
        return m_Expressions.FromIndex(index);
    }
}
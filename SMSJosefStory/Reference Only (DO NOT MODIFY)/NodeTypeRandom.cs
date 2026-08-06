using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue;

[Serializable]
[Title("Random")]
[Category("Random")]
[Image(typeof(IconNodeRandom), ColorTheme.Type.TextLight)]
[Description("Picks and runs a random element from its children")]
public class NodeTypeRandom : TNodeType
{
    [SerializeField]
    private bool m_AllowRepeat = true;

    [NonSerialized]
    private int m_LastIndex = -1;

    public override bool IsBranch => true;

    public override Task Run(int id, Story story, Args args)
    {
        return Task.CompletedTask;
    }

    public override List<int> GetNext(int id, Story story, Args args)
    {
        List<int> list = story.Content.Children(id);
        DialogueSkin dialogueSkin = story.Content.DialogueSkin;
        for (int num = list.Count - 1; num >= 0; num--)
        {
            int id2 = list[num];
            if (!story.Content.Get(id2).CanRun(args))
            {
                list.RemoveAt(num);
            }
        }

        if (list.Count == 0)
        {
            return new List<int>();
        }

        int num2 = m_Options switch
        {
            NodeTypeData.FromSkin => (dialogueSkin != null && dialogueSkin.ValuesRandom.AllowRepeat) ? 1 : 0,
            NodeTypeData.FromNode => m_AllowRepeat ? 1 : 0,
            _ => throw new ArgumentOutOfRangeException(),
        };
        int maxExclusive = ((num2 != 0 || m_LastIndex < 0) ? list.Count : (list.Count - 1));
        int num3 = UnityEngine.Random.Range(0, maxExclusive);
        if (num2 == 0 && num3 == m_LastIndex)
        {
            num3++;
        }

        m_LastIndex = num3;
        int item = list[num3];
        return new List<int> { item };
    }
} 
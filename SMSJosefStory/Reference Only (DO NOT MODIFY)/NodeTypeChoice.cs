using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue;

[Serializable]
[Title("Choice")]
[Category("Choice")]
[Image(typeof(IconNodeChoice), ColorTheme.Type.TextLight)]
[Description("Lets the user choose an option from its children")]
public class NodeTypeChoice : TNodeType
{
    public static readonly string NAME_SKIP_CHOICE = "m_SkipChoice";

    [SerializeField]
    private bool m_HideUnavailable;

    [SerializeField]
    private bool m_HideVisited;

    [SerializeField]
    private bool m_SkipChoice;

    [SerializeField]
    private bool m_ShuffleChoices;

    [SerializeField]
    private TimedChoice m_TimedChoice = new TimedChoice();

    [NonSerialized]
    private int m_ChosenId;

    [NonSerialized]
    private float m_CurrentDuration;

    [NonSerialized]
    private float m_CurrentElapsed;

    public override bool IsBranch => true;

    public float CurrentDuration => m_CurrentDuration;

    public float CurrentElapsed => m_CurrentElapsed;

    public bool GetHideUnavailable(DialogueSkin skin)
    {
        return m_Options switch
        {
            NodeTypeData.FromSkin => skin != null && skin.ValuesChoices.HideUnavailable,
            NodeTypeData.FromNode => m_HideUnavailable,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public bool GetHideVisited(DialogueSkin skin)
    {
        return m_Options switch
        {
            NodeTypeData.FromSkin => skin != null && skin.ValuesChoices.HideVisited,
            NodeTypeData.FromNode => m_HideVisited,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public bool GetShuffleChoices(DialogueSkin skin)
    {
        return m_Options switch
        {
            NodeTypeData.FromSkin => skin != null && skin.ValuesChoices.ShuffleChoices,
            NodeTypeData.FromNode => m_ShuffleChoices,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public bool GetTimedChoice(DialogueSkin skin)
    {
        return m_Options switch
        {
            NodeTypeData.FromSkin => skin != null && skin.ValuesChoices.TimedChoice.IsTimed,
            NodeTypeData.FromNode => m_TimedChoice.IsTimed,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public TimeoutBehavior GetTimeout(DialogueSkin skin)
    {
        return m_Options switch
        {
            NodeTypeData.FromSkin => (skin != null) ? skin.ValuesChoices.TimedChoice.Timeout : TimeoutBehavior.ChooseRandom,
            NodeTypeData.FromNode => m_TimedChoice.Timeout,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public float GetDuration(DialogueSkin skin, Args args)
    {
        return m_Options switch
        {
            NodeTypeData.FromSkin => (skin != null) ? skin.ValuesChoices.TimedChoice.GetDuration(args) : 0f,
            NodeTypeData.FromNode => m_TimedChoice.GetDuration(args),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public List<int> GetChoices(Story story, int nodeId, Args args, bool removeUnavailable)
    {
        List<int> list = story.Content.Children(nodeId);
        DialogueSkin dialogueSkin = story.Content.DialogueSkin;
        bool flag = GetHideUnavailable(dialogueSkin) || removeUnavailable;
        for (int num = list.Count - 1; num >= 0; num--)
        {
            int num2 = list[num];
            Node node = story.Content.Get(num2);
            if (GetHideVisited(dialogueSkin) && story.Visits.Nodes.Contains(num2))
            {
                list.RemoveAt(num);
            }
            else if (flag && !node.CanRun(args))
            {
                list.RemoveAt(num);
            }
        }

        if (GetShuffleChoices(dialogueSkin))
        {
            list.Shuffle();
        }

        return list;
    }

    public void Choose(int nodeId)
    {
        if (m_ChosenId == -1)
        {
            m_ChosenId = nodeId;
        }
    }

    public override async Task Run(int id, Story story, Args args)
    {
        DialogueSkin skin = story.Content.DialogueSkin;
        m_ChosenId = -1;
        m_CurrentDuration = GetDuration(skin, args);
        m_CurrentElapsed = 0f;
        InvokeEventStartChoice(id);
        List<int> choices = GetChoices(story, id, args, removeUnavailable: true);
        if (choices.Count == 1 && story.Content.DialogueSkin.ValuesChoices.AutoOneChoice)
        {
            Choose(choices[0]);
        }

        float startTime = story.Time.Time;
        while (PendingChoice(story, args) && !story.IsCanceled)
        {
            await Task.Yield();
            if (!GetTimedChoice(skin) || story.Time.Time < startTime + m_CurrentDuration)
            {
                m_CurrentElapsed = story.Time.Time - startTime;
                continue;
            }

            m_CurrentElapsed = m_CurrentDuration;
            choices = GetChoices(story, id, args, removeUnavailable: true);
            if (choices.Count == 0)
            {
                Debug.LogError("There cannot be zero choices");
            }

            List<int> list = choices;
            Choose(list[GetTimeout(skin) switch
            {
                TimeoutBehavior.ChooseRandom => UnityEngine.Random.Range(0, choices.Count),
                TimeoutBehavior.ChooseFirst => 0,
                TimeoutBehavior.ChooseLast => choices.Count - 1,
                _ => throw new ArgumentOutOfRangeException(),
            }]);
        }

        InvokeEventFinishChoice(id);
    }

    public override List<int> GetNext(int id, Story story, Args args)
    {
        if (ApplicationManager.IsExiting)
        {
            return new List<int>();
        }

        if (story.IsCanceled)
        {
            return new List<int>();
        }

        story.Visits.Nodes.Add(m_ChosenId);
        story.Visits.Tags.Add(story.Content.Get(m_ChosenId).Tag);
        DialogueSkin dialogueSkin = story.Content.DialogueSkin;
        if (m_Options switch
        {
            NodeTypeData.FromSkin => (dialogueSkin != null && dialogueSkin.ValuesChoices.SkipChoice) ? 1 : 0,
            NodeTypeData.FromNode => m_SkipChoice ? 1 : 0,
            _ => throw new ArgumentOutOfRangeException(),
        } == 0)
        {
            return new List<int> { m_ChosenId };
        }

        return story.Content.Children(m_ChosenId);
    }

    private bool PendingChoice(Story story, Args args)
    {
        if (m_ChosenId == -1)
        {
            return true;
        }

        if (story.Content.Get(m_ChosenId).CanRun(args))
        {
            return false;
        }

        m_ChosenId = -1;
        return true;
    }
} 
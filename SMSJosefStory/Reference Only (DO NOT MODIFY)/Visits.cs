using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue;

[Serializable]
public class Visits
{
    [SerializeField]
    private VisitsNodes m_Nodes = new VisitsNodes();

    [SerializeField]
    private VisitsTags m_Tags = new VisitsTags();

    [NonSerialized]
    private bool m_IsVisited;

    public VisitsNodes Nodes => m_Nodes;

    public VisitsTags Tags => m_Tags;

    public bool IsVisited
    {
        get
        {
            return m_IsVisited;
        }
        set
        {
            m_IsVisited = value;
        }
    }

    public void Clear()
    {
        m_Nodes.Clear();
        m_Tags.Clear();
    }

    public bool RemoveNode(int nodeId)
    {
        return m_Nodes.Remove(nodeId);
    }

    public bool RemoveTag(IdString tag)
    {
        return m_Tags.Remove(tag);
    }
} 
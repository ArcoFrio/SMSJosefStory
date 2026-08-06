using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue;

[Serializable]
public class Content : TSerializableTree<Node>, ISerializationCallbackReceiver
{
    [SerializeField]
    private Role[] m_Roles = Array.Empty<Role>();

    [SerializeField]
    private DialogueSkin m_DialogueSkin;

    [SerializeField]
    private TimeMode m_Time = new TimeMode(TimeMode.UpdateMode.UnscaledTime);

    [NonSerialized]
    private Dictionary<Actor, PropertyGetGameObject> m_Troupe;

    public DialogueSkin DialogueSkin
    {
        get
        {
            return m_DialogueSkin;
        }
        set
        {
            m_DialogueSkin = value;
        }
    }

    public TimeMode Time => m_Time;

    public Content()
    {
        m_Troupe = new Dictionary<Actor, PropertyGetGameObject>();
    }

    public int FindByTag(IdString tag)
    {
        foreach (KeyValuePair<int, TTreeDataItem<Node>> datum in m_Data)
        {
            if (datum.Value.Value.Tag.Hash == tag.Hash)
            {
                return datum.Key;
            }
        }

        return -1;
    }

    public List<Tag> GetTags()
    {
        List<Tag> list = new List<Tag>();
        foreach (KeyValuePair<int, TTreeDataItem<Node>> datum in m_Data)
        {
            IdString tag = datum.Value.Value.Tag;
            if (tag.Hash != IdString.EMPTY.Hash)
            {
                list.Add(new Tag(tag, datum.Key));
            }
        }

        return list;
    }

    public GameObject GetTargetFromActor(Actor actor, Args args)
    {
        if (actor == null)
        {
            return null;
        }

        if (!m_Troupe.TryGetValue(actor, out var value))
        {
            return null;
        }

        return value.Get(args);
    }

    private Actor[] FindActors()
    {
        List<Actor> list = new List<Actor>();
        foreach (KeyValuePair<int, TTreeDataItem<Node>> datum in m_Data)
        {
            Actor actor = datum.Value.Value.Actor;
            if (!(actor == null) && !list.Contains(actor))
            {
                list.Add(actor);
            }
        }

        return list.ToArray();
    }

    private bool ContainsActor(IEnumerable<Role> roles, Actor actor)
    {
        foreach (Role role in roles)
        {
            if (role.Actor == actor)
            {
                return true;
            }
        }

        return false;
    }

    private int SortRoles(Role a, Role b)
    {
        Actor actor = a.Actor;
        Actor actor2 = b.Actor;
        if (actor == null)
        {
            return 0;
        }

        if (actor2 == null)
        {
            return 0;
        }

        return string.Compare(actor.name, actor2.name, StringComparison.InvariantCultureIgnoreCase);
    }

    internal void EditorReset()
    {
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        if (AssemblyUtils.IsReloading)
        {
            return;
        }

        Actor[] array = FindActors();
        List<Role> list = new List<Role>();
        Actor[] array2 = array;
        foreach (Actor actor in array2)
        {
            if (actor == null || ContainsActor(list, actor))
            {
                continue;
            }

            bool flag = false;
            Role[] roles = m_Roles;
            foreach (Role role in roles)
            {
                if (!(role.Actor != actor))
                {
                    list.Add(role);
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                list.Add(new Role(actor));
            }
        }

        list.Sort(SortRoles);
        m_Roles = list.ToArray();
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        m_Troupe = new Dictionary<Actor, PropertyGetGameObject>();
        Role[] roles = m_Roles;
        foreach (Role role in roles)
        {
            if (!(role?.Actor == null))
            {
                m_Troupe.Add(role.Actor, role.Target);
            }
        }
    }
} 
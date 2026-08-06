using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    public class Args
    {
        public static readonly Args EMPTY = new Args();

        [NonSerialized]
        private readonly Dictionary<int, Component> m_SelfComponents;

        [NonSerialized]
        private readonly Dictionary<int, Component> m_TargetComponents;

        [field: NonSerialized]
        public GameObject Self { get; private set; }

        [field: NonSerialized]
        public GameObject Target { get; private set; }

        public Args Clone => new Args(Self, Target);

        private Args()
        {
            m_SelfComponents = new Dictionary<int, Component>();
            m_TargetComponents = new Dictionary<int, Component>();
        }

        public Args(Component target)
            : this(target, target)
        {
        }

        public Args(GameObject target)
            : this(target, target)
        {
        }

        public Args(Component self, Component target)
            : this()
        {
            Self = ((self == null) ? null : self.gameObject);
            Target = ((target == null) ? null : target.gameObject);
        }

        public Args(GameObject self, GameObject target)
            : this()
        {
            Self = self;
            Target = target;
        }

        public T ComponentFromSelf<T>(bool inChildren = false) where T : Component
        {
            return GetComponent<T>(m_SelfComponents, Self, inChildren);
        }

        public T ComponentFromTarget<T>(bool inChildren = false) where T : Component
        {
            return GetComponent<T>(m_TargetComponents, Target, inChildren);
        }

        public void ChangeSelf(GameObject self)
        {
            if (!(Self == self))
            {
                Self = self;
                m_SelfComponents.Clear();
            }
        }

        public void ChangeSelf<T>(T self) where T : Component
        {
            ChangeSelf(((UnityEngine.Object)self != (UnityEngine.Object)null) ? self.gameObject : null);
        }

        public void ChangeTarget(GameObject target)
        {
            if (!(Target == target))
            {
                Target = target;
                m_TargetComponents.Clear();
            }
        }

        public void ChangeTarget<T>(T target) where T : Component
        {
            ChangeTarget(((UnityEngine.Object)target != (UnityEngine.Object)null) ? target.gameObject : null);
        }

        private TComponent GetComponent<TComponent>(IDictionary<int, Component> dictionary, GameObject gameObject, bool inChildren) where TComponent : Component
        {
            if (gameObject == null)
            {
                return null;
            }

            int hashCode = typeof(TComponent).GetHashCode();
            if (!dictionary.TryGetValue(hashCode, out var value) || value == null)
            {
                value = (inChildren ? gameObject.GetComponent<TComponent>() : gameObject.GetComponentInChildren<TComponent>());
                if (value == null)
                {
                    return null;
                }

                dictionary[hashCode] = value;
            }

            return value as TComponent;
        }
    }
} 
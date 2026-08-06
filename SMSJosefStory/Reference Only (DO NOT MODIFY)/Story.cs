using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue
{
    [Serializable]
    public class Story
    {
        [SerializeReference]
        private Content m_Content;

        [SerializeField]
        private Visits m_Visits;

        [NonSerialized]
        private bool m_IsCanceled;

        [NonSerialized]
        private int m_CurrentId;

        [NonSerialized]
        private Actor m_LastActor;

        [NonSerialized]
        private int m_LastExpression;

        public Content Content => m_Content;

        public Visits Visits => m_Visits;

        public TimeMode Time => m_Content.Time;

        public bool IsCanceled
        {
            get
            {
                if (!AsyncManager.ExitRequest)
                {
                    return m_IsCanceled;
                }

                return true;
            }
            set
            {
                m_IsCanceled = value;
            }
        }

        public event Action<int> EventStartNext;

        public event Action<int> EventFinishNext;

        public Story()
        {
            m_Content = new Content();
            m_Visits = new Visits();
        }

        public async Task Play(Args args)
        {
            int[] rootIds = m_Content.RootIds;
            if (rootIds.Length == 0)
            {
                return;
            }

            Stack<int> stack = new Stack<int>();
            for (int num = rootIds.Length - 1; num >= 0; num--)
            {
                stack.Push(rootIds[num]);
            }

            m_IsCanceled = false;
            m_LastActor = null;
            m_LastExpression = -1;
            while (stack.Count > 0 && !IsCanceled)
            {
                m_CurrentId = stack.Pop();
                Node node = m_Content.Get(m_CurrentId);
                if (node == null || !node.CanRun(args))
                {
                    continue;
                }

                if (node.Actor != m_LastActor || node.Expression != m_LastExpression)
                {
                    if (m_LastActor != null)
                    {
                        Expression expressionFromIndex = m_LastActor.GetExpressionFromIndex(m_LastExpression);
                        if (expressionFromIndex != null)
                        {
                            await expressionFromIndex.OnEnd(args);
                        }
                    }

                    if (node.Actor != null)
                    {
                        Expression expressionFromIndex2 = node.Actor.GetExpressionFromIndex(node.Expression);
                        if (expressionFromIndex2 != null)
                        {
                            await expressionFromIndex2.OnStart(args);
                        }
                    }
                }

                m_LastActor = node.Actor;
                m_LastExpression = node.Expression;
                this.EventStartNext?.Invoke(m_CurrentId);
                NodeJump nodeJump = await node.Run(m_CurrentId, this, args);
                this.EventFinishNext?.Invoke(m_CurrentId);
                switch (nodeJump.Jump)
                {
                    case JumpType.Continue:
                        {
                            List<int> next = node.GetNext(m_CurrentId, this, args);
                            for (int num2 = next.Count - 1; num2 >= 0; num2--)
                            {
                                stack.Push(next[num2]);
                            }

                            break;
                        }
                    case JumpType.Exit:
                        stack.Clear();
                        await Finish(args);
                        return;
                    case JumpType.Jump:
                        {
                            int nodeId = m_Content.FindByTag(nodeJump.JumpTo);
                            stack.Clear();
                            StackFromJump(stack, nodeId);
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            await Finish(args);
        }

        public void Continue()
        {
            m_Content.Get(m_CurrentId)?.Continue();
        }

        internal void StopTypewriter()
        {
            m_Content.Get(m_CurrentId)?.StopTypewriter();
        }

        private async Task Finish(Args args)
        {
            if (m_LastActor != null)
            {
                Actor lastActor = m_LastActor;
                int lastExpression = m_LastExpression;
                Expression expressionFromIndex = lastActor.GetExpressionFromIndex(lastExpression);
                if (expressionFromIndex != null)
                {
                    await expressionFromIndex.OnEnd(args);
                }
            }

            m_Visits.IsVisited = true;
        }

        private void StackFromJump(Stack<int> stack, int nodeId)
        {
            int num = Content.Parent(nodeId);
            if (num != -1)
            {
                StackFromJump(stack, num);
                if (Content.Get(num).NodeType.IsBranch)
                {
                    stack.Push(nodeId);
                    return;
                }
            }

            List<int> list = m_Content.Siblings(nodeId);
            int num2 = list.Count - 1;
            while (num2 >= 0)
            {
                int num3 = list[num2];
                stack.Push(num3);
                if (num3 != nodeId)
                {
                    num2--;
                    continue;
                }

                break;
            }
        }
    }
} 
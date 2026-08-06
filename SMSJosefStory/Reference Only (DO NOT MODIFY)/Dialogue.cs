using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Dialogue.UnityUI;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue
{
    [AddComponentMenu("Game Creator/Dialogue/Dialogue")]
    [DisallowMultipleComponent]
    public class Dialogue : MonoBehaviour
    {
        private const string ERR_NO_SKIN = "Failed to run Dialogue: No skin found";

        [SerializeField]
        private Story m_Story = new Story();

        public Story Story => m_Story;

        public static Dialogue Current { get; private set; }

        public static event Action<Dialogue> EventStartLine;

        public static event Action<Dialogue> EventFinishLine;

        public event Action EventStart;

        public event Action EventFinish;

        public event Action<int> EventStartNext;

        public event Action<int> EventFinishNext;

        public async Task Play(Args args)
        {
            if (m_Story.Content.DialogueSkin == null)
            {
                Debug.LogError("Failed to run Dialogue: No skin found");
                return;
            }

            Current = this;
            await DialogueUI.Open(m_Story.Content.DialogueSkin, this, isNew: true);
            this.EventStart?.Invoke();
            m_Story.EventStartNext -= OnStartNext;
            m_Story.EventFinishNext -= OnFinishNext;
            m_Story.EventStartNext += OnStartNext;
            m_Story.EventFinishNext += OnFinishNext;
            await m_Story.Play(args);
            Stop();
        }

        public void Stop()
        {
            m_Story.EventStartNext -= OnStartNext;
            m_Story.EventFinishNext -= OnFinishNext;
            m_Story.IsCanceled = true;
            this.EventFinish?.Invoke();
            Current = null;
        }

        private void Reset()
        {
            m_Story.Content.EditorReset();
        }

        private void OnStartNext(int nodeId)
        {
            this.EventStartNext?.Invoke(nodeId);
            Dialogue.EventStartLine?.Invoke(this);
        }

        private void OnFinishNext(int nodeId)
        {
            this.EventFinishNext?.Invoke(nodeId);
            Dialogue.EventFinishLine?.Invoke(this);
        }
    }
} 
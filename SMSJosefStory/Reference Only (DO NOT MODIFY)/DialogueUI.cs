using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue.UnityUI
{
    [AddComponentMenu("Game Creator/UI/Dialogue/Dialogue UI")]
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField]
        private RectTransform m_SpeechContainer;

        [SerializeField]
        private SpeechSkin m_DefaultSpeech;

        [NonSerialized]
        private Dialogue m_Dialogue;

        [NonSerialized]
        private SpeechUI m_SpeechUI;

        [NonSerialized]
        private Args m_Args;

        [NonSerialized]
        private bool m_IsClosing;

        [field: NonSerialized]
        public static DialogueUI Current { get; private set; }

        [field: NonSerialized]
        public static bool IsOpen { get; private set; }

        [field: NonSerialized]
        public DialogueSkin DialogueSkin { get; private set; }

        [field: NonSerialized]
        public SpeechSkin SpeechSkin { get; private set; }

        public static event Action EventStart;

        public static event Action EventFinish;

        public event Action<Story, int, Args> EventOnStartNext;

        public event Action<Story, int, Args> EventOnFinishNext;

        private void Awake()
        {
            TDialogueUnitUI[] componentsInChildren = GetComponentsInChildren<TDialogueUnitUI>(includeInactive: true);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].OnAwake(this);
            }
        }

        public static async Task Open(DialogueSkin dialogueSkin, Dialogue dialogue, bool isNew)
        {
            if (dialogueSkin == null || dialogue == null)
            {
                return;
            }

            if (Current != null)
            {
                if (IsOpen)
                {
                    Current.m_Dialogue.Stop();
                }

                while (!ApplicationManager.IsExiting && Current.m_IsClosing)
                {
                    await Task.Yield();
                }
            }

            Current = dialogueSkin.RequireSkin();
            Current.DialogueSkin = dialogueSkin;
            Current.m_Dialogue = dialogue;
            dialogue.EventFinish += Current.OnStop;
            dialogue.EventStartNext += Current.OnStartNext;
            dialogue.EventFinishNext += Current.OnFinishNext;
            TDialogueUnitUI[] componentsInChildren = Current.GetComponentsInChildren<TDialogueUnitUI>(includeInactive: true);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].OnReset(isNew);
            }

            IsOpen = true;
            Current.m_Args = new Args(dialogue.gameObject, Current.gameObject);
            Current.gameObject.SetActive(value: true);
            dialogueSkin.PlayClipStart();
            Animator val = ((Component)Current).Get<Animator>();
            if ((UnityEngine.Object)(object)val != null)
            {
                val.runtimeAnimatorController = dialogueSkin.Controller;
                val.SetTrigger(DialogueSkin.ANIMATOR_OPEN);
                TimeMode time = Current.m_Dialogue.Story.Time;
                float timeout = time.Time + dialogueSkin.DurationOpen;
                while (!ApplicationManager.IsExiting && time.Time < timeout)
                {
                    await Task.Yield();
                }
            }

            DialogueUI.EventStart?.Invoke();
        }

        private void OnStop()
        {
            if (!ApplicationManager.IsExiting)
            {
                if (m_Dialogue != null)
                {
                    m_Dialogue.EventFinish -= OnStop;
                    m_Dialogue.EventStartNext -= OnStartNext;
                    m_Dialogue.EventFinishNext -= OnFinishNext;
                }

                IsOpen = false;
                Animator val = ((Component)this).Get<Animator>();
                if ((UnityEngine.Object)(object)val != null)
                {
                    val.SetTrigger(DialogueSkin.ANIMATOR_CLOSE);
                }

                DialogueSkin.PlayClipFinish();
                DialogueUI.EventFinish?.Invoke();
                float durationClose = DialogueSkin.DurationClose;
                CloseUI(durationClose);
            }
        }

        private async Task CloseUI(float duration)
        {
            m_IsClosing = true;
            TimeMode time = m_Dialogue.Story.Time;
            float timeout = time.Time + duration;
            while (!ApplicationManager.IsExiting && time.Time < timeout)
            {
                await Task.Yield();
            }

            base.gameObject.SetActive(value: false);
            m_IsClosing = false;
        }

        private void OnStartNext(int nodeId)
        {
            if (ApplicationManager.IsExiting)
            {
                return;
            }

            Node node = m_Dialogue.Story.Content.Get(nodeId);
            SpeechSkin speechSkin = m_DefaultSpeech;
            if (node?.Actor != null)
            {
                if (node.Actor.OverrideSpeechSkin != null)
                {
                    speechSkin = node.Actor.OverrideSpeechSkin;
                }

                Expression expressionFromIndex = node.Actor.GetExpressionFromIndex(node.Expression);
                if (expressionFromIndex?.OverrideSpeechSkin != null)
                {
                    speechSkin = expressionFromIndex.OverrideSpeechSkin;
                }
            }

            if (SpeechSkin != speechSkin)
            {
                for (int num = m_SpeechContainer.childCount - 1; num >= 0; num--)
                {
                    UnityEngine.Object.Destroy(m_SpeechContainer.GetChild(num).gameObject);
                }

                GameObject gameObject = UIUtils.Instantiate(speechSkin.Value, m_SpeechContainer);
                SpeechSkin = speechSkin;
                m_SpeechUI = gameObject.Get<SpeechUI>();
                if (m_SpeechUI != null)
                {
                    m_SpeechUI.OnAwake(this);
                }

                RuntimeAnimatorController controller = speechSkin.Controller;
                Animator val = gameObject.Get<Animator>();
                if ((UnityEngine.Object)(object)val != null && (UnityEngine.Object)(object)val.runtimeAnimatorController != (UnityEngine.Object)(object)controller)
                {
                    val.runtimeAnimatorController = controller;
                }
            }

            TDialogueUnitUI[] componentsInChildren = GetComponentsInChildren<TDialogueUnitUI>(includeInactive: true);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].OnStartNext(m_Dialogue.Story, nodeId, m_Args);
            }

            this.EventOnStartNext?.Invoke(m_Dialogue.Story, nodeId, m_Args);
        }

        private void OnFinishNext(int nodeId)
        {
            if (!ApplicationManager.IsExiting)
            {
                TDialogueUnitUI[] componentsInChildren = GetComponentsInChildren<TDialogueUnitUI>(includeInactive: true);
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].OnFinishNext(m_Dialogue.Story, nodeId, m_Args);
                }

                this.EventOnFinishNext?.Invoke(m_Dialogue.Story, nodeId, m_Args);
            }
        }
    }
} 
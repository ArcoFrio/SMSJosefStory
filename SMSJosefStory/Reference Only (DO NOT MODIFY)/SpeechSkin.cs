
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using GameCreator.Runtime.Dialogue.UnityUI;
using UnityEngine;

namespace GameCreator.Runtime.Dialogue;

public class SpeechSkin : TSkin<GameObject>, ISerializationCallbackReceiver
{
    public enum AnimationWhen
    {
        NewSpeaker,
        Always
    }

    private const string MSG = "A game object prefab with the Speech UI skin for a speech";

    private const string ERR_NO_VALUE = "Prefab value cannot be empty";

    private const string ERR_COMP_UI = "Prefab does not contain a 'SpeechUI' component";

    public static readonly int ANIMATOR_OPEN = Animator.StringToHash("Open");

    [SerializeField]
    private AnimatorOverrideController m_Controller;

    [SerializeField]
    private AnimationWhen m_When;

    [SerializeField]
    private AnimationClip m_Idle;

    [SerializeField]
    private AnimationClip m_Open;

    [SerializeField]
    private AudioClip m_Start;

    [SerializeField]
    private AudioClip m_Finish;

    [SerializeField]
    private GameObject m_Log;

    public RuntimeAnimatorController Controller => (RuntimeAnimatorController)(object)m_Controller;

    public override string Description => "A game object prefab with the Speech UI skin for a speech";

    public override string HasError
    {
        get
        {
            if (base.Value == null)
            {
                return "Prefab value cannot be empty";
            }

            if ((bool)base.Value.Get<SpeechUI>())
            {
                return string.Empty;
            }

            return "Prefab does not contain a 'SpeechUI' component";
        }
    }

    public AnimationWhen AnimateWhen => m_When;

    public GameObject OverrideLog => m_Log;

    public void PlayClipStart()
    {
        if (!(m_Start == null))
        {
            Singleton<AudioManager>.Instance.UserInterface.Play(m_Start, AudioConfigSoundUI.Default, Args.EMPTY);
        }
    }

    public void PlayClipFinish()
    {
        if (!(m_Start == null))
        {
            Singleton<AudioManager>.Instance.UserInterface.Play(m_Finish, AudioConfigSoundUI.Default, Args.EMPTY);
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
    }
}
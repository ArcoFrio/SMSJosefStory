using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

namespace GameCreator.Runtime.Dialogue;

[Serializable]
public class Node
{
    [SerializeReference]
    private TNodeType m_NodeType = new NodeTypeText();

    [SerializeField]
    private RunConditionsList m_Conditions = new RunConditionsList();

    [SerializeField]
    private NodeText m_Text = new NodeText();

    [SerializeField]
    private PropertyGetAudio m_Audio = GetAudioNone.Create;

    [SerializeField]
    private Acting m_Acting = new Acting();

    [SerializeField]
    private AnimationClip m_Animation;

    [SerializeField]
    private NodeAnimation m_AnimationData = new NodeAnimation();

    [SerializeField]
    private NodeSequence m_Sequence = new NodeSequence(new Track[1]
    {
        new TrackDefault()
    });

    [SerializeField]
    private RunInstructionsList m_OnStart = new RunInstructionsList();

    [SerializeField]
    private RunInstructionsList m_OnFinish = new RunInstructionsList();

    [SerializeField]
    private NodeDuration m_Duration;

    [SerializeField]
    private PropertyGetDecimal m_Timeout = GetDecimalDecimal.Create(3f);

    [SerializeField]
    private IdString m_Tag = IdString.EMPTY;

    [SerializeField]
    private NodeJump m_Jump = NodeJump.Continue();

    [NonSerialized]
    private bool m_Continue;

    [NonSerialized]
    private bool m_RunTypewriter;

    [NonSerialized]
    private float m_TypewriterLength;

    [NonSerialized]
    private float m_AudioLength;

    [NonSerialized]
    private float m_AnimationLength;

    public TNodeType NodeType
    {
        get
        {
            return m_NodeType;
        }
        set
        {
            m_NodeType = value;
        }
    }

    public string Text => m_Text.Value;

    public Actor Actor => m_Acting.Actor;

    public int Expression => m_Acting.Expression;

    public PortraitMode Portrait => m_Acting.Portrait;

    public AnimationClip Animation => m_Animation;

    public NodeDuration Duration => m_Duration;

    public IdString Tag => m_Tag;

    public NodeJump Jump => m_Jump;

    public event Action<int> EventStart;

    public event Action<int> EventStartOnStart;

    public event Action<int> EventFinishOnStart;

    public event Action<int> EventStartText;

    public event Action<int> EventFinishText;

    public event Action<int> EventStartType;

    public event Action<int> EventFinishType;

    public event Action<int> EventStartOnFinish;

    public event Action<int> EventFinishOnFinish;

    public event Action<int> EventFinish;

    public event Action<int> EventStartChoice;

    public event Action<int> EventFinishChoice;

    public Node()
    {
    }

    public Node(string text)
        : this()
    {
        m_Text = new NodeText(text);
    }

    public string GetText(Args args)
    {
        return m_Text.Get(args);
    }

    public AudioClip GetAudio(Args args)
    {
        return m_Audio.Get(args);
    }

    public async Task<NodeJump> Run(int id, Story story, Args args)
    {
        m_Continue = false;
        m_RunTypewriter = true;
        m_TypewriterLength = 0f;
        m_AudioLength = 0f;
        m_AnimationLength = 0f;
        m_Text.Init(args);
        this.EventStart?.Invoke(id);
        this.EventStartOnStart?.Invoke(id);
        await RunOnStart(args);
        this.EventFinishOnStart?.Invoke(id);
        GameObject speaker = story.Content.GetTargetFromActor(Actor, args);
        AudioClip audio = GetAudio(args);
        if (audio != null)
        {
            AudioConfigSpeech audioConfig = ((speaker != null) ? AudioConfigSpeech.Create(1f, SpatialBlending.Spatial, speaker) : AudioConfigSpeech.Create(1f, SpatialBlending.None, null));
            Singleton<AudioManager>.Instance.Speech.Play(audio, audioConfig, args);
            if (m_Duration == NodeDuration.Audio)
            {
                m_AudioLength = audio.length;
            }
        }

        if ((UnityEngine.Object)(object)m_Animation != null)
        {
            Character character = speaker.Get<Character>();
            if (character != null)
            {
                ConfigGesture config = new ConfigGesture(0f, m_Animation.length, 1f, m_AnimationData.UseRootMotion, m_AnimationData.TransitionIn, m_AnimationData.TransitionOut);
                character.Gestures.CrossFade(m_Animation, m_AnimationData.AvatarMask, m_AnimationData.BlendMode, config, stopPreviousGestures: true);
            }
            else
            {
                Animator val = speaker.Get<Animator>();
                if ((UnityEngine.Object)(object)val != null)
                {
                    PlayableGraph playableGraph = default(PlayableGraph);
                    AnimationPlayableUtilities.PlayClip(val, m_Animation, ref playableGraph);
                }
            }

            m_Sequence.Run(story.Time, m_Animation, args);
            if (m_Duration == NodeDuration.Animation)
            {
                m_AnimationLength = m_Animation.length;
            }
        }

        if (Actor != null)
        {
            m_TypewriterLength = Actor.Typewriter.GetDuration(Text);
        }

        Typewriter typewriter = ((Actor != null) ? Actor.Typewriter : null);
        TimeMode time = story.Time;
        float startTime = time.Time;
        AudioClip gibberishClip = null;
        AudioConfigSoundUI gibberishConfig = AudioConfigSoundUI.Create(1f, typewriter?.Pitch ?? new Vector2(1f, 1f));
        this.EventStartText?.Invoke(id);
        while (!CanContinue(startTime, time, args) && !story.IsCanceled)
        {
            if (startTime + m_TypewriterLength > time.Time && m_RunTypewriter)
            {
                if (gibberishClip == null)
                {
                    gibberishClip = typewriter?.GetGibberish(args);
                }

                if (!Singleton<AudioManager>.Instance.UserInterface.IsPlaying(gibberishClip))
                {
                    Singleton<AudioManager>.Instance.UserInterface.Play(gibberishClip, gibberishConfig, args);
                }
            }

            await Task.Yield();
        }

        if (gibberishClip != null)
        {
            Singleton<AudioManager>.Instance.Speech.Stop(gibberishClip, 0.25f);
        }

        this.EventFinishText?.Invoke(id);
        m_NodeType.EventStartChoice -= OnStartChoice;
        m_NodeType.EventFinishChoice -= OnFinishChoice;
        m_NodeType.EventStartChoice += OnStartChoice;
        m_NodeType.EventFinishChoice += OnFinishChoice;
        this.EventStartType?.Invoke(id);
        await m_NodeType.Run(id, story, args);
        story.Visits.Nodes.Add(id);
        story.Visits.Tags.Add(m_Tag);
        this.EventFinishType?.Invoke(id);
        m_NodeType.EventStartChoice -= OnStartChoice;
        m_NodeType.EventFinishChoice -= OnFinishChoice;
        if (audio != null)
        {
            Singleton<AudioManager>.Instance.Speech.Stop(audio, 0f);
        }

        if ((UnityEngine.Object)(object)m_Animation != null)
        {
            Character character2 = speaker.Get<Character>();
            if (character2 != null)
            {
                character2.Gestures.Stop(0f, 0f);
            }

            if (m_Sequence.IsRunning)
            {
                m_Sequence.Cancel(args);
            }
        }

        this.EventStartOnFinish?.Invoke(id);
        await RunOnFinish(args);
        this.EventFinishOnFinish?.Invoke(id);
        this.EventFinish?.Invoke(id);
        return Jump;
    }

    public List<int> GetNext(int id, Story story, Args args)
    {
        return m_NodeType.GetNext(id, story, args);
    }

    public bool CanRun(Args args)
    {
        RunnerConfig runnerConfig = default(RunnerConfig);
        runnerConfig.Name = "Can Run Node";
        runnerConfig.Location = new RunnerLocationParent(args.Self.transform);
        RunnerConfig config = runnerConfig;
        return m_Conditions.Check(args, config);
    }

    public void Continue()
    {
        m_Continue = true;
    }

    internal void StopTypewriter()
    {
        m_RunTypewriter = false;
    }

    private bool CanContinue(float startTime, TimeMode time, Args args)
    {
        return m_Duration switch
        {
            NodeDuration.UntilInteraction => m_Continue,
            NodeDuration.Timeout => (double)time.Time > (double)(startTime + m_TypewriterLength) + m_Timeout.Get(args),
            NodeDuration.Audio => time.Time > startTime + m_AudioLength,
            NodeDuration.Animation => time.Time > startTime + m_AnimationLength,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private void OnStartChoice(int nodeId)
    {
        this.EventStartChoice?.Invoke(nodeId);
    }

    private void OnFinishChoice(int nodeId)
    {
        this.EventFinishChoice?.Invoke(nodeId);
    }

    private async Task RunOnStart(Args args)
    {
        RunnerConfig runnerConfig = default(RunnerConfig);
        runnerConfig.Name = "On Start Node";
        runnerConfig.Location = new RunnerLocationParent(args.ComponentFromSelf<Transform>());
        RunnerConfig config = runnerConfig;
        await m_OnStart.Run(args.Clone, config);
    }

    private async Task RunOnFinish(Args args)
    {
        RunnerConfig runnerConfig = default(RunnerConfig);
        runnerConfig.Name = "On Finish Node";
        RunnerConfig.Location = new RunnerLocationParent(args.ComponentFromSelf<Transform>());
        RunnerConfig config = runnerConfig;
        await m_OnFinish.Run(args.Clone, config);
    }

    public override string ToString()
    {
        return m_Text.ToString();
    }
} 
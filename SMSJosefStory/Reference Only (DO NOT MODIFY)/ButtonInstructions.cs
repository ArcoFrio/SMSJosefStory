using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace GameCreator.Runtime.Common.UnityUI;

[AddComponentMenu("Game Creator/UI/Button")]
[RequireComponent(typeof(Image))]
public class ButtonInstructions : Button
{
    [SerializeField]
    private InstructionList m_Instructions = new InstructionList(new InstructionCommonDebugText("Click!"));

    private Args m_Args;

    protected override void Awake()
    {
        base.Start();
        if (Application.isPlaying)
        {
            m_Args = new Args(base.gameObject);
            m_Instructions.EventStartRunning += OnStartRunning;
            m_Instructions.EventEndRunning += OnEndRunning;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (Application.isPlaying)
        {
            m_Instructions.EventStartRunning -= OnStartRunning;
            m_Instructions.EventEndRunning -= OnEndRunning;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (Application.isPlaying)
        {
            base.onClick.AddListener(RunInstructions);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (Application.isPlaying)
        {
            base.onClick.RemoveListener(RunInstructions);
        }
    }

    private void RunInstructions()
    {
        m_Instructions.Run(m_Args);
    }

    private void OnStartRunning()
    {
        base.interactable = false;
    }

    private void OnEndRunning()
    {
        base.interactable = true;
    }
}

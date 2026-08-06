using System;
using System.Threading.Tasks;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Common;

[Serializable]
public class RunInstructionsList : TRun<InstructionList>
{
    private const int PREWARM_COUNTER = 5;

    [SerializeField]
    private InstructionList m_Instructions;

    protected override InstructionList Value => m_Instructions;

    protected override GameObject Template
    {
        get
        {
            if (m_Template == null)
            {
                m_Template = CreateTemplate(Value);
            }

            return m_Template;
        }
    }

    public RunInstructionsList()
    {
        m_Instructions = new InstructionList();
    }

    public RunInstructionsList(params Instruction[] instructions)
    {
        m_Instructions = new InstructionList(instructions);
    }

    public RunInstructionsList(InstructionList instructionList)
    {
        m_Instructions = new InstructionList(instructionList);
    }

    public async Task Run(Args args)
    {
        await Run(args, RunnerConfig.Default);
    }

    public async Task Run(Args args, RunnerConfig config)
    {
        if (!ApplicationManager.IsExiting && (m_Instructions?.Length ?? 0) != 0)
        {
            GameObject template = Template;
            await Run(args, template, config);
        }
    }

    public static async Task Run(Args args, GameObject template)
    {
        await Run(args, template, RunnerConfig.Default);
    }

    public static async Task Run(Args args, GameObject template, RunnerConfig config)
    {
        if (ApplicationManager.IsExiting || (template.Get<RunnerInstructionsList>().Value?.Length ?? 0) == 0)
        {
            return;
        }

        RunnerInstructionsList runner = TRunner<InstructionList>.Pick<RunnerInstructionsList>(template, config, 5);
        if (!(runner == null))
        {
            await runner.Value.Run(args, config.Cancellable);
            if (runner != null)
            {
                TRunner<InstructionList>.Restore(runner);
            }
        }
    }

    private static GameObject CreateTemplate(InstructionList value)
    {
        return TRunner<InstructionList>.CreateTemplate<RunnerInstructionsList>(value);
    }

    public override string ToString()
    {
        return m_Instructions.Length switch
        {
            0 => string.Empty,
            1 => m_Instructions.Get(0).ToString(),
            _ => $"{m_Instructions.Get(0)} +{m_Instructions.Length - 1}",
        };
    }
}
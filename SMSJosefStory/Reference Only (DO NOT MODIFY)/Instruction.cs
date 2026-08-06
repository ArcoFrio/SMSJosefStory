using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting;

[Serializable]
public abstract class Instruction : TPolymorphicItem<Instruction>
{
    private const int DEFAULT_NEXT_INSTRUCTION = 1;

    protected static readonly TimeMode DefaultTime = new TimeMode(TimeMode.UpdateMode.GameTime);

    protected static readonly Task DefaultResult = Task.FromResult(result: true);

    protected int NextInstruction { get; set; }

    protected InstructionList Parent { get; private set; }

    protected bool IsCanceled
    {
        get
        {
            InstructionList parent = Parent;
            if (parent == null || !parent.IsCancelled)
            {
                return AsyncManager.ExitRequest;
            }

            return true;
        }
    }

    public async Task<InstructionResult> Schedule(Args args, InstructionList parent)
    {
        NextInstruction = 1;
        Parent = parent;
        if (base.Breakpoint)
        {
            Debug.Break();
        }

        if (base.IsEnabled)
        {
            await Run(args);
        }

        if (IsCanceled)
        {
            return InstructionResult.Stop;
        }

        if (NextInstruction == 1)
        {
            return InstructionResult.Default;
        }

        if (NextInstruction == int.MaxValue)
        {
            return InstructionResult.Stop;
        }

        return InstructionResult.JumpTo(NextInstruction);
    }

    protected abstract Task Run(Args args);

    private async Task Yield()
    {
        if (!IsCanceled)
        {
            await Task.Yield();
        }
    }

    protected async Task NextFrame()
    {
        await Yield();
    }

    protected async Task Time(float duration)
    {
        await Time(duration, DefaultTime);
    }

    protected async Task Time(float duration, TimeMode time)
    {
        float startTime = time.Time;
        while (!IsCanceled && time.Time < startTime + duration)
        {
            await Yield();
        }
    }

    protected async Task While(Func<bool> function)
    {
        while (!IsCanceled && function())
        {
            await Yield();
        }
    }

    protected async Task Until(Func<bool> function)
    {
        while (!IsCanceled && !function())
        {
            await Yield();
        }
    }
}
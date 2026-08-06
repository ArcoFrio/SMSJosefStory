using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting;

[Serializable]
public class InstructionList : TPolymorphicList<Instruction>, ICancellable
{
    [SerializeReference]
    private Instruction[] m_Instructions = Array.Empty<Instruction>();

    public bool IsRunning { get; private set; }

    public bool IsStopped { get; private set; }

    public ICancellable Cancellable { get; private set; }

    public bool IsCancelled
    {
        get
        {
            if (!IsStopped)
            {
                return Cancellable?.IsCancelled ?? false;
            }

            return true;
        }
    }

    public int RunningIndex { get; private set; }

    public override int Length => m_Instructions.Length;

    public event Action EventStartRunning;

    public event Action EventEndRunning;

    public event Action<int> EventRunInstruction;

    public InstructionList()
    {
        IsRunning = false;
        IsStopped = false;
    }

    public InstructionList(params Instruction[] instructions)
        : this()
    {
        m_Instructions = instructions;
    }

    public InstructionList(InstructionList instructionList)
        : this()
    {
        m_Instructions = instructionList.m_Instructions;
    }

    public async Task Run(Args args, int fromIndex = 0)
    {
        await Run(args, null, fromIndex);
    }

    public async Task Run(Args args, ICancellable cancellable, int fromIndex = 0)
    {
        if (IsRunning)
        {
            return;
        }

        Cancellable = cancellable;
        IsRunning = true;
        RunningIndex = Math.Max(0, fromIndex);
        IsStopped = false;
        this.EventStartRunning?.Invoke();
        while (RunningIndex < Length)
        {
            if (IsCancelled)
            {
                IsStopped = true;
                IsRunning = false;
                this.EventEndRunning?.Invoke();
                return;
            }

            if (m_Instructions[RunningIndex] == null)
            {
                RunningIndex++;
                continue;
            }

            this.EventRunInstruction?.Invoke(RunningIndex);
            InstructionResult instructionResult = await m_Instructions[RunningIndex].Schedule(args, this);
            if (instructionResult.DontContinue)
            {
                IsRunning = false;
                this.EventEndRunning?.Invoke();
                return;
            }

            RunningIndex += instructionResult.NextInstruction;
        }

        IsRunning = false;
        this.EventEndRunning?.Invoke();
    }

    public void Cancel()
    {
        IsStopped = true;
    }

    public Instruction Get(int index)
    {
        index = Mathf.Clamp(index, 0, Length - 1);
        return m_Instructions[index];
    }
}

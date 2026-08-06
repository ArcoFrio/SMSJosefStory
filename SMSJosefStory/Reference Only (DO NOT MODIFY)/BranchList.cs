using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting;

[Serializable]
public class BranchList : TPolymorphicList<Branch>, ICancellable
{
    [SerializeReference]
    private Branch[] m_Branches = Array.Empty<Branch>();

    public bool IsRunning { get; private set; }

    public bool IsStopped { get; private set; }

    public int EvaluatingIndex { get; private set; }

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

    public override int Length => m_Branches.Length;

    public event Action EventStartRunning;

    public event Action EventEndRunning;

    public event Action<int> EventRunBranch;

    public BranchList()
    {
        IsRunning = false;
        IsStopped = false;
    }

    public BranchList(params Branch[] branches)
        : this()
    {
        m_Branches = branches;
    }

    public async Task Evaluate(Args args)
    {
        await Evaluate(args, null);
    }

    public async Task Evaluate(Args args, ICancellable cancellable)
    {
        if (IsRunning)
        {
            return;
        }

        Cancellable = cancellable;
        EvaluatingIndex = -1;
        IsRunning = true;
        IsStopped = false;
        this.EventStartRunning?.Invoke();
        int i = 0;
        while (i < Length)
        {
            Branch branch = m_Branches[i];
            if (branch != null)
            {
                EvaluatingIndex = i;
                this.EventRunBranch?.Invoke(EvaluatingIndex);
                if ((await branch.Evaluate(args, this)).Value)
                {
                    break;
                }
            }

            int num = i + 1;
            i = num;
        }

        EvaluatingIndex = -1;
        IsRunning = false;
        this.EventEndRunning?.Invoke();
    }

    public void Cancel()
    {
        IsStopped = true;
    }
}

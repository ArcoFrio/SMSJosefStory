using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting;

[Serializable]
[Image(typeof(IconBranch), ColorTheme.Type.Green)]
public class Branch : TPolymorphicItem<Branch>
{
    [SerializeField]
    private string m_Description = "";

    [SerializeField]
    private ConditionList m_ConditionList = new ConditionList();

    [SerializeField]
    private InstructionList m_InstructionList = new InstructionList();

    public override string Title
    {
        get
        {
            if (!string.IsNullOrEmpty(m_Description))
            {
                return m_Description;
            }

            return "Branch";
        }
    }

    public async Task<BranchResult> Evaluate(Args args, ICancellable cancellable)
    {
        if (!base.IsEnabled)
        {
            return BranchResult.False;
        }

        if (base.Breakpoint)
        {
            Debug.Break();
        }

        if (!m_ConditionList.Check(args, CheckMode.And))
        {
            return BranchResult.False;
        }

        await m_InstructionList.Run(args, cancellable);
        return BranchResult.True;
    }
}

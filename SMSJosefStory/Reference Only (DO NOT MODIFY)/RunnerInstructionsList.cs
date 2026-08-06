using GameCreator.Runtime.VisualScripting;

namespace GameCreator.Runtime.Common;

public class RunnerInstructionsList : TRunner<InstructionList>
{
    public void Cancel()
    {
        m_Value?.Cancel();
    }
}
using GameCreator.Runtime.VisualScripting;

namespace GameCreator.Runtime.Common;

public struct RunnerConfig : IRunnerConfig
{
    public static readonly RunnerConfig Default;

    private string m_Name;

    private IRunnerLocation m_Location;

    private ICancellable m_Cancellable;

    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(m_Name))
            {
                return "Runner;
            }

            return m_Name;
        }
        set
        {
            m_Name = value;
        }
    }

    public IRunnerLocation Location
    {
        get
        {
            return (IRunnerLocation)(m_Location ?? ((object)RunnerLocationNone.Create));
        }
        set
        {
            m_Location = value;
        }
    }

    public ICancellable Cancellable
    {
        get
        {
            return m_Cancellable ?? null;
        }
        set
        {
            m_Cancellable = value;
        }
    }
} 
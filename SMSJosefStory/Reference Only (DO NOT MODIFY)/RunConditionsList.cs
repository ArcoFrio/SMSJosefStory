using System;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Common;

[Serializable]
public class RunConditionsList : TRun<ConditionList>
{
    private const int PREWARM_COUNTER = 1;

    [SerializeField]
    private ConditionList m_Conditions;

    protected override ConditionList Value => m_Conditions;

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

    public RunConditionsList()
    {
        m_Conditions = new ConditionList();
    }

    public RunConditionsList(params Condition[] conditions)
    {
        m_Conditions = new ConditionList(conditions);
    }

    public bool Check(Args args)
    {
        return Check(args, RunnerConfig.Default);
    }

    public bool Check(Args args, RunnerConfig config)
    {
        ConditionList conditions = m_Conditions;
        if (conditions == null || conditions.Length == 0)
        {
            return true;
        }

        GameObject template = Template;
        return Check(args, template, config);
    }

    public static bool Check(Args args, GameObject template)
    {
        return Check(args, template, RunnerConfig.Default);
    }

    public static bool Check(Args args, GameObject template, RunnerConfig config)
    {
        ConditionList value = template.Get<RunnerConditionsList>().Value;
        if (value == null || value.Length == 0)
        {
            return true;
        }

        RunnerConditionsList runnerConditionsList = TRunner<ConditionList>.Pick<RunnerConditionsList>(template, config, 1);
        if (runnerConditionsList == null)
        {
            return false;
        }

        bool result = runnerConditionsList.Value.Check(args, CheckMode.And);
        if (runnerConditionsList != null)
        {
            TRunner<ConditionList>.Restore(runnerConditionsList);
        }

        return result;
    }

    private static GameObject CreateTemplate(ConditionList value)
    {
        return TRunner<ConditionList>.CreateTemplate<RunnerConditionsList>(value);
    }

    public override string ToString()
    {
        return m_Conditions.Length switch
        {
            0 => string.Empty,
            1 => m_Conditions.Get(0)?.Title,
            _ => $"{m_Conditions.Get(0)?.Title} +{m_Conditions.Length - 1}",
        };
    }
} 
using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting;

[Serializable]
[Title("Set Active")]
[Description("Changes the state of a game object to active or inactive")]
[Category("Game Objects/Set Active")]
[Keywords(new string[] { "Activate", "Deactivate", "Enable", "Disable" })]
[Keywords(new string[] { "MonoBehaviour", "Behaviour", "Script" })]
[Image(typeof(IconCubeSolid), ColorTheme.Type.Yellow)]
public class InstructionGameObjectSetActive : TInstructionGameObject
{
    [SerializeField]
    private PropertyGetBool m_Active = GetBoolValue.Create(value: true);

    public override string Title => $"Set Active {m_GameObject} to {m_Active}";

    protected override Task Run(Args args)
    {
        GameObject gameObject = m_GameObject.Get(args);
        if (gameObject == null)
        {
            return Instruction.DefaultResult;
        }

        bool active = m_Active.Get(args);
        gameObject.SetActive(active);
        return Instruction.DefaultResult;
    }
}
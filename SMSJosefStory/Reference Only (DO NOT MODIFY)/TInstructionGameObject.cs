using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting;

[Serializable]
[Parameter("Game Object", "Target game object")]
public abstract class TInstructionGameObject : Instruction
{
    [SerializeField]
    protected PropertyGetGameObject m_GameObject = new PropertyGetGameObject();
}
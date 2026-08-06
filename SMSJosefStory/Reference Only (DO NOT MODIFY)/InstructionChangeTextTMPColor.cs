using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
[Version(1, 0, 0)]
[Title("Change Text Color")]
[Category("UI/Change Text Color")]
[Description("Changes the color of a Text or TMP component")]
[Image(typeof(IconUIText), ColorTheme.Type.Yellow)]
public class InstructionChangeTextTMPColor : Instruction
{
	[SerializeField]
	private PropertyGetGameObject m_Target;

	[SerializeField]
	private PropertyGetColor m_Color;

	public override string Title => $"Change {m_Target} Color to {m_Color}";

	protected override Task Run(Args args)
	{
		GameObject gameObject = m_Target.Get(args);
		if (gameObject == null)
		{
			return Instruction.DefaultResult;
		}
		Color color = m_Color.Get(args);
		Text component = gameObject.GetComponent<Text>();
		if (component != null)
		{
			component.color = color;
			return Instruction.DefaultResult;
		}
		TextMeshProUGUI component2 = gameObject.GetComponent<TextMeshProUGUI>();
		if (component2 != null)
		{
			component2.color = color;
			return Instruction.DefaultResult;
		}
		Debug.LogWarning("No Text or TextMeshProUGUI component found on target");
		return Instruction.DefaultResult;
	}
}

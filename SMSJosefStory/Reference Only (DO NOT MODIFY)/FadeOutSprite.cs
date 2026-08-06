using System.Collections;
using UnityEngine;

public class FadeOutSprite : MonoBehaviour
{
	private SpriteRenderer spriteRenderer;

	private Color initialColor;

	private float duration = 1f;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		initialColor = spriteRenderer.color;
	}

	private void OnEnable()
	{
		Color color = initialColor;
		color.a = 1f;
		spriteRenderer.color = color;
		StartCoroutine(FadeOut());
	}

	private IEnumerator FadeOut()
	{
		float fadeOutTime = 0f;
		while (fadeOutTime < duration)
		{
			fadeOutTime += Time.deltaTime;
			float a = Mathf.Lerp(1f, 0f, fadeOutTime / duration);
			Color color = spriteRenderer.color;
			color.a = a;
			spriteRenderer.color = color;
			yield return null;
		}
		Color color2 = spriteRenderer.color;
		color2.a = 0f;
		spriteRenderer.color = color2;
	}
}

using System.Collections;
using UnityEngine;

public class FadeInSprite : MonoBehaviour
{
	private SpriteRenderer spriteRenderer;

	private float fadeDuration = 2f;

	private float targetAlpha = 1f;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void OnEnable()
	{
		StartCoroutine(FadeIn());
	}

	private IEnumerator FadeIn()
	{
		Color color = spriteRenderer.color;
		color.a = 0f;
		spriteRenderer.color = color;
		float elapsedTime = 0f;
		while (elapsedTime < fadeDuration)
		{
			elapsedTime += Time.deltaTime;
			color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
			spriteRenderer.color = color;
			yield return null;
		}
		color.a = targetAlpha;
		spriteRenderer.color = color;
	}
}

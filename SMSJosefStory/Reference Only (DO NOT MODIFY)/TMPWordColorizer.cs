using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPWordColorizer : MonoBehaviour
{
	[Serializable]
	public class WordColorPair
	{
		public string word;

		public Color color = Color.white;
	}

	[Header("Word Color Mappings")]
	[SerializeField]
	private List<WordColorPair> wordColors = new List<WordColorPair>();

	[Header("Timing")]
	[SerializeField]
	private float delayBeforeColoring = 0.3f;

	[SerializeField]
	private float transitionDuration = 0.5f;

	[Header("Fallback")]
	[SerializeField]
	private Color defaultColor = Color.white;

	private TextMeshProUGUI tmpText;

	private string previousText = "";

	private Coroutine colorRoutine;

	private void Awake()
	{
		tmpText = GetComponent<TextMeshProUGUI>();
		if (tmpText == null)
		{
			Debug.LogError("TMPWordColorizer: TextMeshProUGUI component not found!", this);
			base.enabled = false;
		}
	}

	private void OnEnable()
	{
		if (tmpText != null)
		{
			previousText = tmpText.text;
		}
	}

	private void OnDisable()
	{
		if (colorRoutine != null)
		{
			StopCoroutine(colorRoutine);
			colorRoutine = null;
		}
	}

	private void Update()
	{
		if (!(tmpText == null) && tmpText.text != previousText)
		{
			previousText = tmpText.text;
			if (colorRoutine != null)
			{
				StopCoroutine(colorRoutine);
			}
			colorRoutine = StartCoroutine(ColorizeAfterDelay());
		}
	}

	private IEnumerator ColorizeAfterDelay()
	{
		yield return new WaitForSeconds(delayBeforeColoring);
		if (tmpText == null)
		{
			colorRoutine = null;
			yield break;
		}
		Color colorForCurrentText = GetColorForCurrentText();
		yield return TransitionToColor(colorForCurrentText);
		colorRoutine = null;
	}

	private string RemoveTags(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		return Regex.Replace(text, "<[^>]*>", "").Trim();
	}

	private Color GetColorForCurrentText()
	{
		if (tmpText == null || string.IsNullOrEmpty(tmpText.text))
		{
			return defaultColor;
		}
		string text = RemoveTags(tmpText.text);
		if (string.IsNullOrEmpty(text))
		{
			return defaultColor;
		}
		string text2 = text.ToLower();
		if (wordColors != null)
		{
			foreach (WordColorPair wordColor in wordColors)
			{
				if (wordColor != null && !string.IsNullOrEmpty(wordColor.word) && text2 == wordColor.word.ToLower())
				{
					return wordColor.color;
				}
			}
		}
		return defaultColor;
	}

	private IEnumerator TransitionToColor(Color targetColor)
	{
		if (tmpText == null)
		{
			yield break;
		}
		Color startColor = tmpText.color;
		float elapsed = 0f;
		while (elapsed < transitionDuration)
		{
			if (tmpText == null)
			{
				yield break;
			}
			elapsed += Time.deltaTime;
			float t = elapsed / transitionDuration;
			tmpText.color = Color.Lerp(startColor, targetColor, t);
			yield return null;
		}
		if (tmpText != null)
		{
			tmpText.color = targetColor;
		}
	}
}

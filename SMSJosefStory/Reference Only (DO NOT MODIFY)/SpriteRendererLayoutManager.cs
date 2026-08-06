using System.Collections.Generic;
using UnityEngine;

public class SpriteRendererLayoutManager : MonoBehaviour
{
	[Header("Target Settings")]
	[Tooltip("List of GameObjects with SpriteRenderers (e.g., dialogue busts)")]
	public List<GameObject> targetObjects;

	[Tooltip("The anchor point for centering the arrangement")]
	public Transform anchorPoint;

	[Header("Movement Settings")]
	[Tooltip("Speed at which objects move to their target positions")]
	public float moveSpeed = 5f;

	[Tooltip("Vertical offset step based on distance from the center")]
	public float yOffsetStep = 0.1f;

	[Tooltip("Horizontal spacing between objects")]
	public float spacing = 0.01f;

	[Header("Parallax Settings")]
	[Tooltip("Multiplier for parallax offset based on mouse position (in world units)")]
	public float parallaxMultiplier = 0.1f;

	[Tooltip("Only update parallax if mouse moved this many pixels")]
	public float parallaxUpdateThreshold = 1f;

	[Header("Scale Monitoring")]
	[Tooltip("Enable automatic layout updates when GameObjects are rescaled")]
	public bool monitorScaleChanges;

	[Header("Layout Adjustments")]
	[Tooltip("Additional Y offset applied to all sprites in the layout")]
	public float yOffset;

	private List<GameObject> activeObjects = new List<GameObject>();

	private Dictionary<GameObject, bool> previousActiveState = new Dictionary<GameObject, bool>();

	private Dictionary<GameObject, SpriteRenderer> spriteRendererCache = new Dictionary<GameObject, SpriteRenderer>();

	private Dictionary<GameObject, Vector3> previousScales = new Dictionary<GameObject, Vector3>();

	private Vector3 lastMousePosition;

	private Vector3 cachedParallaxOffset;

	private bool layoutNeedsRecalculation = true;

	private List<float> cachedSpriteWidths = new List<float>();

	private List<Vector3> targetPositions = new List<Vector3>();

	private void Start()
	{
		CacheSpriteRenderers();
		if (monitorScaleChanges)
		{
			InitializeScaleMonitoring();
		}
	}

	private void Update()
	{
		if (anchorPoint == null)
		{
			Debug.LogWarning("[" + base.gameObject.name + "] Anchor point is null. Layout disabled.");
			return;
		}
		bool num = UpdateActiveObjects();
		bool flag = false;
		if (monitorScaleChanges)
		{
			flag = CheckForScaleChanges();
		}
		if (num || flag)
		{
			layoutNeedsRecalculation = true;
		}
		ArrangeObjects();
	}

	private void InitializeScaleMonitoring()
	{
		previousScales.Clear();
		foreach (GameObject targetObject in targetObjects)
		{
			if (targetObject != null)
			{
				previousScales[targetObject] = targetObject.transform.localScale;
			}
		}
	}

	private bool CheckForScaleChanges()
	{
		bool result = false;
		foreach (GameObject activeObject in activeObjects)
		{
			if (activeObject == null)
			{
				continue;
			}
			Vector3 localScale = activeObject.transform.localScale;
			if (previousScales.TryGetValue(activeObject, out var value))
			{
				if (Vector3.Distance(localScale, value) > 0.001f)
				{
					result = true;
					previousScales[activeObject] = localScale;
				}
			}
			else
			{
				previousScales[activeObject] = localScale;
			}
		}
		return result;
	}

	private void CacheSpriteRenderers()
	{
		spriteRendererCache.Clear();
		foreach (GameObject targetObject in targetObjects)
		{
			if (targetObject != null)
			{
				SpriteRenderer component = targetObject.GetComponent<SpriteRenderer>();
				if (component != null)
				{
					spriteRendererCache[targetObject] = component;
					continue;
				}
				Debug.LogWarning("[" + base.gameObject.name + "] GameObject '" + targetObject.name + "' doesn't have a SpriteRenderer component.");
			}
		}
	}

	private bool UpdateActiveObjects()
	{
		int count = activeObjects.Count;
		activeObjects.Clear();
		foreach (GameObject targetObject in targetObjects)
		{
			if (targetObject == null)
			{
				continue;
			}
			bool activeSelf = targetObject.activeSelf;
			if (activeSelf)
			{
				activeObjects.Add(targetObject);
				if (!previousActiveState.ContainsKey(targetObject) || !previousActiveState[targetObject])
				{
					Vector3 position = targetObject.transform.position;
					targetObject.transform.position = new Vector3(0f, 0f, position.z);
				}
			}
			previousActiveState[targetObject] = activeSelf;
		}
		return count != activeObjects.Count;
	}

	private void ArrangeObjects()
	{
		if (activeObjects.Count == 0 || anchorPoint == null)
		{
			return;
		}
		if (layoutNeedsRecalculation)
		{
			CalculateLayout();
			layoutNeedsRecalculation = false;
		}
		Vector3 optimizedParallaxOffset = GetOptimizedParallaxOffset();
		for (int i = 0; i < activeObjects.Count; i++)
		{
			GameObject gameObject = activeObjects[i];
			if (!(gameObject == null))
			{
				Vector3 target = targetPositions[i] + optimizedParallaxOffset;
				gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, target, moveSpeed * Time.deltaTime);
			}
		}
	}

	private void CalculateLayout()
	{
		ResizeListsToFit(activeObjects.Count);
		float totalWidth = CalculateTotalWidth();
		CalculateTargetPositions(totalWidth);
	}

	private void ResizeListsToFit(int requiredSize)
	{
		while (cachedSpriteWidths.Count < requiredSize)
		{
			cachedSpriteWidths.Add(0f);
		}
		while (targetPositions.Count < requiredSize)
		{
			targetPositions.Add(Vector3.zero);
		}
	}

	private float CalculateTotalWidth()
	{
		float num = 0f;
		for (int i = 0; i < activeObjects.Count; i++)
		{
			GameObject gameObject = activeObjects[i];
			if (spriteRendererCache.TryGetValue(gameObject, out var value))
			{
				cachedSpriteWidths[i] = value.bounds.size.x;
				num += cachedSpriteWidths[i];
				continue;
			}
			cachedSpriteWidths[i] = 0f;
			Debug.LogWarning("[" + base.gameObject.name + "] No cached SpriteRenderer found for '" + gameObject.name + "'. Layout may be incorrect.");
		}
		return num + (float)(activeObjects.Count - 1) * spacing;
	}

	private void CalculateTargetPositions(float totalWidth)
	{
		Vector3 vector = anchorPoint.position - new Vector3(totalWidth / 2f, 0f, 0f);
		int middleIndex = activeObjects.Count / 2;
		for (int i = 0; i < activeObjects.Count; i++)
		{
			GameObject gameObject = activeObjects[i];
			if (!(gameObject == null))
			{
				float num = CalculateVerticalOffset(i, middleIndex);
				float x = vector.x + cachedSpriteWidths[i] * 0.5f;
				float y = anchorPoint.position.y + num + yOffset;
				targetPositions[i] = new Vector3(x, y, gameObject.transform.position.z);
				vector.x += cachedSpriteWidths[i] + spacing;
			}
		}
	}

	private float CalculateVerticalOffset(int index, int middleIndex)
	{
		return 0f - (float)Mathf.Abs(index - middleIndex) * yOffsetStep;
	}

	private Vector3 GetOptimizedParallaxOffset()
	{
		Vector3 mousePosition = Input.mousePosition;
		if (Vector3.Distance(mousePosition, lastMousePosition) > parallaxUpdateThreshold)
		{
			lastMousePosition = mousePosition;
			cachedParallaxOffset = CalculateParallaxOffset(mousePosition);
		}
		return cachedParallaxOffset;
	}

	private Vector3 CalculateParallaxOffset(Vector3 mousePosition)
	{
		Vector2 screenCenter = GetScreenCenter();
		Vector2 normalizedMouseOffset = GetNormalizedMouseOffset(mousePosition, screenCenter);
		float x = normalizedMouseOffset.x * parallaxMultiplier;
		float num = normalizedMouseOffset.y * parallaxMultiplier;
		if (num > 0f)
		{
			num = 0f;
		}
		return new Vector3(x, num, 0f);
	}

	private Vector2 GetScreenCenter()
	{
		return new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
	}

	private Vector2 GetNormalizedMouseOffset(Vector3 mousePosition, Vector2 screenCenter)
	{
		return new Vector2((mousePosition.x - screenCenter.x) / screenCenter.x, (mousePosition.y - screenCenter.y) / screenCenter.y);
	}

	public void RefreshCache()
	{
		CacheSpriteRenderers();
		layoutNeedsRecalculation = true;
		if (monitorScaleChanges)
		{
			InitializeScaleMonitoring();
		}
	}

	public void ForceLayoutUpdate()
	{
		layoutNeedsRecalculation = true;
	}

	public void SetScaleMonitoring(bool enabled)
	{
		monitorScaleChanges = enabled;
		if (enabled)
		{
			InitializeScaleMonitoring();
		}
		else
		{
			previousScales.Clear();
		}
	}

	private void OnDrawGizmos()
	{
		if (anchorPoint != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(anchorPoint.position, 0.05f);
		}
	}

	private void OnValidate()
	{
		if (Application.isPlaying)
		{
			RefreshCache();
		}
	}
}

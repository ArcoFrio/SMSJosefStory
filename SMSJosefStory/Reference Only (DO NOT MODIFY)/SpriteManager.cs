using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
	private struct BreathingData
	{
		public float offset;

		public float amplitude;

		public float speed;
	}

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

	[Tooltip("Minimum random Y position when sprite is enabled")]
	public float minRandomYPosition = -0.8f;

	[Tooltip("Maximum random Y position when sprite is enabled")]
	public float maxRandomYPosition = -0.6f;

	[Header("Prefab Spawning (Optional)")]
	[Tooltip("Enable prefab spawning feature")]
	public bool enablePrefabSpawning;

	[Tooltip("List of prefabs that can be spawned as children of the last enabled target")]
	public List<GameObject> spawnablePrefabs = new List<GameObject>();

	[Header("Hover Zoom (Optional)")]
	[Tooltip("Enable zoom effect on mouse hover")]
	public bool enableHoverZoom;

	[Tooltip("Scale multiplier when hovering (1.0 = no change, 1.2 = 20% bigger)")]
	[Range(1f, 2f)]
	public float hoverZoomScale = 1.1f;

	[Tooltip("Time in seconds to reach full zoom")]
	[Range(0.1f, 2f)]
	public float zoomInDuration = 0.3f;

	[Tooltip("Time in seconds to return to normal scale")]
	[Range(0.1f, 2f)]
	public float zoomOutDuration = 0.5f;

	[Tooltip("Push other sprites away when zooming to prevent overlap")]
	public bool pushAwayOnZoom = true;

	[Tooltip("Extra spacing to add when pushing sprites away (in world units)")]
	[Range(0f, 2f)]
	public float pushAwayDistance = 0.5f;

	[Tooltip("Speed of push away animation")]
	[Range(1f, 10f)]
	public float pushAwaySpeed = 5f;

	[Header("Breathing Animation (Optional)")]
	[Tooltip("Enable breathing animation for all active sprites")]
	public bool enableBreathing;

	[Tooltip("Base amplitude of breathing movement in world units")]
	[Range(0.01f, 50f)]
	public float breathingAmplitude = 0.1f;

	[Tooltip("Base speed of breathing animation")]
	[Range(0.5f, 50f)]
	public float breathingSpeed = 1.5f;

	[Tooltip("Random variation in amplitude (0 = uniform, 1 = fully random)")]
	[Range(0f, 10f)]
	public float breathingAmplitudeVariation = 0.3f;

	[Tooltip("Random variation in speed (0 = synchronized, 1 = fully random)")]
	[Range(0f, 10f)]
	public float breathingSpeedVariation = 0.4f;

	private List<GameObject> activeObjects = new List<GameObject>();

	private Dictionary<GameObject, bool> previousActiveState = new Dictionary<GameObject, bool>();

	private Dictionary<GameObject, SpriteRenderer> spriteRendererCache = new Dictionary<GameObject, SpriteRenderer>();

	private Dictionary<GameObject, Vector3> previousScales = new Dictionary<GameObject, Vector3>();

	private Vector3 lastMousePosition;

	private Vector3 cachedParallaxOffset;

	private bool layoutNeedsRecalculation = true;

	private List<float> cachedSpriteWidths = new List<float>();

	private List<Vector3> targetPositions = new List<Vector3>();

	private Dictionary<GameObject, GameObject> spawnedPrefabInstances = new Dictionary<GameObject, GameObject>();

	private GameObject mostRecentlyEnabledTarget;

	private GameObject currentHoveredObject;

	private Dictionary<GameObject, float> zoomProgress = new Dictionary<GameObject, float>();

	private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();

	private Dictionary<GameObject, Vector3> pushAwayOffsets = new Dictionary<GameObject, Vector3>();

	private int hoveredObjectIndex = -1;

	private Dictionary<GameObject, BreathingData> breathingData = new Dictionary<GameObject, BreathingData>();

	private Dictionary<GameObject, Vector3> smoothedPositions = new Dictionary<GameObject, Vector3>();

	private string debugPrefix;

	private Camera cachedMainCamera;

	private bool cameraInitialized;

	private List<GameObject> keysToRemoveBuffer = new List<GameObject>();

	private void Start()
	{
		debugPrefix = "[" + base.gameObject.name + "]";
		CacheSpriteRenderers();
		CacheCamera();
		if (monitorScaleChanges)
		{
			InitializeScaleMonitoring();
		}
		if (enableBreathing)
		{
			InitializeBreathing();
		}
	}

	private void Update()
	{
		if (anchorPoint == null)
		{
			Debug.LogWarning(debugPrefix + " Anchor point is null. Layout disabled.");
			return;
		}
		bool num = UpdateActiveObjects();
		bool flag = monitorScaleChanges && CheckForScaleChanges();
		if (num || flag)
		{
			layoutNeedsRecalculation = true;
		}
		UpdatePositions();
		if (enableHoverZoom)
		{
			UpdateHoverZoom();
		}
		if (enablePrefabSpawning)
		{
			HandlePrefabVisibility();
		}
	}

	private void CacheCamera()
	{
		cachedMainCamera = Camera.main;
		cameraInitialized = true;
	}

	private Camera GetMainCamera()
	{
		if (!cameraInitialized || cachedMainCamera == null)
		{
			CacheCamera();
		}
		return cachedMainCamera;
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
				if (Vector3.SqrMagnitude(localScale - value) > 1E-06f)
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
				}
				else
				{
					Debug.LogWarning(debugPrefix + " GameObject '" + targetObject.name + "' doesn't have a SpriteRenderer component.");
				}
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
			bool value;
			bool flag = previousActiveState.TryGetValue(targetObject, out value) && value;
			if (activeSelf)
			{
				activeObjects.Add(targetObject);
				if (!flag)
				{
					mostRecentlyEnabledTarget = targetObject;
					OnObjectEnabled(targetObject);
					Debug.Log(debugPrefix + " Newly enabled target: '" + targetObject.name + "' is now the most recent");
				}
			}
			previousActiveState[targetObject] = activeSelf;
		}
		if (mostRecentlyEnabledTarget != null && !mostRecentlyEnabledTarget.activeSelf)
		{
			Debug.Log(debugPrefix + " Most recent target '" + mostRecentlyEnabledTarget.name + "' was disabled");
			mostRecentlyEnabledTarget = null;
		}
		return count != activeObjects.Count;
	}

	private void OnObjectEnabled(GameObject obj)
	{
		Vector3 position = obj.transform.position;
		float y = UnityEngine.Random.Range(minRandomYPosition, maxRandomYPosition);
		Vector3 vector = new Vector3(0f, y, position.z);
		obj.transform.position = vector;
		smoothedPositions[obj] = vector;
		if (enableBreathing)
		{
			InitializeBreathingForObject(obj);
		}
		if (enableHoverZoom && !originalScales.ContainsKey(obj))
		{
			originalScales[obj] = obj.transform.localScale;
		}
	}

	private void UpdatePositions()
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
		float t = Time.deltaTime * moveSpeed;
		for (int i = 0; i < activeObjects.Count; i++)
		{
			GameObject gameObject = activeObjects[i];
			if (!(gameObject == null))
			{
				Vector3 b = targetPositions[i] + optimizedParallaxOffset;
				if (enableHoverZoom && pushAwayOffsets.TryGetValue(gameObject, out var value))
				{
					b += value;
				}
				if (!smoothedPositions.TryGetValue(gameObject, out var value2))
				{
					value2 = gameObject.transform.position;
					smoothedPositions[gameObject] = value2;
				}
				Vector3 vector = Vector3.Lerp(value2, b, t);
				smoothedPositions[gameObject] = vector;
				Vector3 zero = Vector3.zero;
				if (enableBreathing && breathingData.TryGetValue(gameObject, out var value3))
				{
					float f = Time.time * value3.speed + value3.offset;
					zero.y = Mathf.Sin(f) * value3.amplitude;
				}
				gameObject.transform.position = vector + zero;
			}
		}
	}

	private void CalculateLayout()
	{
		cachedSpriteWidths.Clear();
		targetPositions.Clear();
		float num = 0f;
		foreach (GameObject activeObject in activeObjects)
		{
			if (!(activeObject == null))
			{
				float spriteWidth = GetSpriteWidth(activeObject);
				cachedSpriteWidths.Add(spriteWidth);
				num += spriteWidth;
			}
		}
		if (activeObjects.Count > 1)
		{
			num += spacing * (float)(activeObjects.Count - 1);
		}
		float num2 = anchorPoint.position.x - num * 0.5f;
		for (int i = 0; i < activeObjects.Count; i++)
		{
			if (!(activeObjects[i] == null))
			{
				float num3 = cachedSpriteWidths[i];
				float num4 = num3 * 0.5f;
				float x = num2 + num4;
				float num5 = Mathf.Abs((float)i - (float)(activeObjects.Count - 1) * 0.5f);
				float y = anchorPoint.position.y - num5 * yOffsetStep + yOffset;
				targetPositions.Add(new Vector3(x, y, activeObjects[i].transform.position.z));
				num2 += num3 + spacing;
			}
		}
	}

	private float GetSpriteWidth(GameObject obj)
	{
		if (!spriteRendererCache.TryGetValue(obj, out var value) || value?.sprite == null)
		{
			return 1f;
		}
		float pixelsPerUnit = value.sprite.pixelsPerUnit;
		return value.sprite.rect.width / pixelsPerUnit * obj.transform.localScale.x;
	}

	private void HandlePrefabVisibility()
	{
		keysToRemoveBuffer.Clear();
		foreach (KeyValuePair<GameObject, GameObject> spawnedPrefabInstance in spawnedPrefabInstances)
		{
			if (spawnedPrefabInstance.Key == null || spawnedPrefabInstance.Value == null)
			{
				keysToRemoveBuffer.Add(spawnedPrefabInstance.Key);
			}
		}
		for (int i = 0; i < keysToRemoveBuffer.Count; i++)
		{
			spawnedPrefabInstances.Remove(keysToRemoveBuffer[i]);
		}
	}

	private Vector3 GetOptimizedParallaxOffset()
	{
		Vector3 mousePosition = Input.mousePosition;
		if (Vector3.SqrMagnitude(mousePosition - lastMousePosition) > parallaxUpdateThreshold * parallaxUpdateThreshold)
		{
			lastMousePosition = mousePosition;
			cachedParallaxOffset = CalculateParallaxOffset(mousePosition);
		}
		return cachedParallaxOffset;
	}

	private Vector3 CalculateParallaxOffset(Vector3 mousePosition)
	{
		float num = (float)Screen.width * 0.5f;
		float num2 = (float)Screen.height * 0.5f;
		float num3 = (mousePosition.x - num) / num;
		float num4 = (mousePosition.y - num2) / num2;
		float x = num3 * parallaxMultiplier;
		float num5 = num4 * parallaxMultiplier;
		if (num5 > 0f)
		{
			num5 = 0f;
		}
		return new Vector3(x, num5, 0f);
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

	private void UpdateHoverZoom()
	{
		Vector3 mouseWorldPosition = GetMouseWorldPosition();
		GameObject gameObject = null;
		int num = -1;
		float num2 = float.MaxValue;
		for (int i = 0; i < activeObjects.Count; i++)
		{
			GameObject gameObject2 = activeObjects[i];
			if (!(gameObject2 == null) && IsMouseOverSprite(gameObject2, mouseWorldPosition))
			{
				float num3 = Vector3.SqrMagnitude(gameObject2.transform.position - mouseWorldPosition);
				if (num3 < num2)
				{
					num2 = num3;
					gameObject = gameObject2;
					num = i;
				}
			}
		}
		if (gameObject != currentHoveredObject)
		{
			currentHoveredObject = gameObject;
			hoveredObjectIndex = num;
		}
		float deltaTime = Time.deltaTime;
		for (int j = 0; j < activeObjects.Count; j++)
		{
			GameObject gameObject3 = activeObjects[j];
			if (!(gameObject3 == null))
			{
				if (!zoomProgress.TryGetValue(gameObject3, out var value))
				{
					value = 0f;
				}
				if (!pushAwayOffsets.TryGetValue(gameObject3, out var value2))
				{
					value2 = Vector3.zero;
				}
				if (!originalScales.TryGetValue(gameObject3, out var value3))
				{
					value3 = gameObject3.transform.localScale;
					originalScales[gameObject3] = value3;
				}
				float num4 = ((gameObject3 == currentHoveredObject) ? 1f : 0f);
				float num5 = ((num4 > value) ? (1f / zoomInDuration) : (1f / zoomOutDuration));
				value = Mathf.MoveTowards(value, num4, num5 * deltaTime);
				zoomProgress[gameObject3] = value;
				if (value > 0.001f)
				{
					float num6 = Mathf.Lerp(1f, hoverZoomScale, value);
					gameObject3.transform.localScale = value3 * num6;
				}
				else
				{
					gameObject3.transform.localScale = value3;
				}
				Vector3 b = Vector3.zero;
				if (pushAwayOnZoom && hoveredObjectIndex >= 0 && hoveredObjectIndex != j)
				{
					b = CalculatePushAwayOffset(j, hoveredObjectIndex);
				}
				value2 = Vector3.Lerp(value2, b, deltaTime * pushAwaySpeed);
				pushAwayOffsets[gameObject3] = value2;
			}
		}
	}

	private Vector3 CalculatePushAwayOffset(int spriteIndex, int hoveredIndex)
	{
		if (hoveredIndex < 0 || currentHoveredObject == null)
		{
			return Vector3.zero;
		}
		if (!zoomProgress.TryGetValue(currentHoveredObject, out var value) || value <= 0.001f)
		{
			return Vector3.zero;
		}
		float num = pushAwayDistance * value;
		float num2 = 0f;
		if (spriteIndex < hoveredIndex)
		{
			num2 = 0f - num;
		}
		else if (spriteIndex > hoveredIndex)
		{
			num2 = num;
		}
		float num3 = Mathf.Abs(spriteIndex - hoveredIndex);
		float num4 = 1f / (1f + (num3 - 1f) * 0.5f);
		return new Vector3(num2 * num4, 0f, 0f);
	}

	private Vector3 GetMouseWorldPosition()
	{
		Camera mainCamera = GetMainCamera();
		if (mainCamera != null)
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = 0f - mainCamera.transform.position.z;
			return mainCamera.ScreenToWorldPoint(mousePosition);
		}
		return Vector3.zero;
	}

	private bool IsMouseOverSprite(GameObject obj, Vector3 mouseWorldPos)
	{
		if (!spriteRendererCache.TryGetValue(obj, out var value) || value?.sprite == null)
		{
			return false;
		}
		Bounds bounds = value.bounds;
		if (mouseWorldPos.x >= bounds.min.x && mouseWorldPos.x <= bounds.max.x && mouseWorldPos.y >= bounds.min.y)
		{
			return mouseWorldPos.y <= bounds.max.y;
		}
		return false;
	}

	private void InitializeBreathing()
	{
		foreach (GameObject targetObject in targetObjects)
		{
			if (targetObject != null)
			{
				InitializeBreathingForObject(targetObject);
			}
		}
	}

	private void InitializeBreathingForObject(GameObject obj)
	{
		if (!(obj == null))
		{
			BreathingData value = new BreathingData
			{
				offset = UnityEngine.Random.Range(0f, MathF.PI * 2f),
				amplitude = breathingAmplitude * (1f - breathingAmplitudeVariation * UnityEngine.Random.Range(0f, 1f)),
				speed = breathingSpeed * (1f - breathingSpeedVariation * UnityEngine.Random.Range(0f, 1f))
			};
			breathingData[obj] = value;
		}
	}

	public void SetBreathing(bool enabled)
	{
		enableBreathing = enabled;
		if (enabled)
		{
			InitializeBreathing();
		}
		else
		{
			breathingData.Clear();
		}
	}

	public void SetHoverZoom(bool enabled)
	{
		enableHoverZoom = enabled;
		if (enabled)
		{
			return;
		}
		foreach (GameObject activeObject in activeObjects)
		{
			if (activeObject != null && originalScales.TryGetValue(activeObject, out var value))
			{
				activeObject.transform.localScale = value;
			}
		}
		zoomProgress.Clear();
		pushAwayOffsets.Clear();
		currentHoveredObject = null;
		hoveredObjectIndex = -1;
	}

	public void GC2_SpawnPrefab0()
	{
		SpawnPrefabOnLastTarget(0);
	}

	public void GC2_SpawnPrefab1()
	{
		SpawnPrefabOnLastTarget(1);
	}

	public void GC2_SpawnPrefab2()
	{
		SpawnPrefabOnLastTarget(2);
	}

	public void GC2_SpawnPrefab3()
	{
		SpawnPrefabOnLastTarget(3);
	}

	public void GC2_SpawnPrefab4()
	{
		SpawnPrefabOnLastTarget(4);
	}

	public void GC2_ClearLastTargetPrefabs()
	{
		ClearSpawnedPrefabsFromLastTarget();
	}

	public void GC2_ClearAllSpawnedPrefabs()
	{
		ClearAllSpawnedPrefabs();
	}

	public void GC2_ForceLayoutUpdate()
	{
		ForceLayoutUpdate();
	}

	public void GC2_EnableBreathing()
	{
		SetBreathing(enabled: true);
	}

	public void GC2_DisableBreathing()
	{
		SetBreathing(enabled: false);
	}

	public void GC2_ToggleBreathing()
	{
		SetBreathing(!enableBreathing);
	}

	public void GC2_EnableHoverZoom()
	{
		SetHoverZoom(enabled: true);
	}

	public void GC2_DisableHoverZoom()
	{
		SetHoverZoom(enabled: false);
	}

	public void GC2_ToggleHoverZoom()
	{
		SetHoverZoom(!enableHoverZoom);
	}

	[ContextMenu("Spawn Prefab 0 on Last Target")]
	private void SpawnPrefab0()
	{
		SpawnPrefabOnLastTarget(0);
	}

	[ContextMenu("Spawn Prefab 1 on Last Target")]
	private void SpawnPrefab1()
	{
		SpawnPrefabOnLastTarget(1);
	}

	[ContextMenu("Spawn Prefab 2 on Last Target")]
	private void SpawnPrefab2()
	{
		SpawnPrefabOnLastTarget(2);
	}

	[ContextMenu("Spawn Prefab 3 on Last Target")]
	private void SpawnPrefab3()
	{
		SpawnPrefabOnLastTarget(3);
	}

	[ContextMenu("Spawn Prefab 4 on Last Target")]
	private void SpawnPrefab4()
	{
		SpawnPrefabOnLastTarget(4);
	}

	public GameObject SpawnPrefabOnLastTarget(int prefabIndex)
	{
		if (!enablePrefabSpawning)
		{
			Debug.LogWarning(debugPrefix + " Prefab spawning is disabled. Enable it in the inspector.");
			return null;
		}
		if (spawnablePrefabs == null || spawnablePrefabs.Count == 0)
		{
			Debug.LogWarning(debugPrefix + " No spawnable prefabs assigned.");
			return null;
		}
		if (prefabIndex < 0 || prefabIndex >= spawnablePrefabs.Count)
		{
			Debug.LogWarning($"{debugPrefix} Prefab index {prefabIndex} is out of range. Valid range: 0-{spawnablePrefabs.Count - 1}");
			return null;
		}
		GameObject gameObject = spawnablePrefabs[prefabIndex];
		if (gameObject == null)
		{
			Debug.LogWarning($"{debugPrefix} Prefab at index {prefabIndex} is null.");
			return null;
		}
		GameObject gameObject2 = GetMostRecentlyEnabledTarget();
		if (gameObject2 == null)
		{
			Debug.LogWarning(debugPrefix + " No recently enabled target objects found to spawn prefab on.");
			return null;
		}
		if (spawnedPrefabInstances.TryGetValue(gameObject2, out var value) && value != null)
		{
			Debug.LogWarning(debugPrefix + " Target '" + gameObject2.name + "' already has a spawned prefab. Remove it first with ClearSpawnedPrefabsFromLastTarget().");
			return null;
		}
		GameObject gameObject3 = UnityEngine.Object.Instantiate(gameObject, gameObject2.transform);
		gameObject3.transform.localPosition = Vector3.zero;
		gameObject3.transform.localRotation = Quaternion.identity;
		gameObject3.transform.localScale = Vector3.one;
		spawnedPrefabInstances[gameObject2] = gameObject3;
		Debug.Log(debugPrefix + " Spawned prefab '" + gameObject.name + "' on most recently enabled target '" + gameObject2.name + "'");
		return gameObject3;
	}

	[ContextMenu("Clear Spawned Prefabs from Last Target")]
	public bool ClearSpawnedPrefabsFromLastTarget()
	{
		if (!enablePrefabSpawning)
		{
			Debug.LogWarning(debugPrefix + " Prefab spawning is disabled. Enable it in the inspector.");
			return false;
		}
		GameObject gameObject = GetMostRecentlyEnabledTarget();
		if (gameObject == null)
		{
			Debug.LogWarning(debugPrefix + " No recently enabled target objects found.");
			return false;
		}
		bool result = false;
		if (spawnedPrefabInstances.TryGetValue(gameObject, out var value))
		{
			if (value != null)
			{
				Debug.Log(debugPrefix + " Deleting spawned prefab '" + value.name + "' from most recently enabled target '" + gameObject.name + "'");
				UnityEngine.Object.Destroy(value);
				result = true;
			}
			spawnedPrefabInstances.Remove(gameObject);
		}
		else
		{
			Debug.Log(debugPrefix + " No spawned prefab found on most recently enabled target '" + gameObject.name + "'");
		}
		return result;
	}

	[ContextMenu("Clear ALL Spawned Prefabs")]
	public void ClearAllSpawnedPrefabs()
	{
		if (!enablePrefabSpawning)
		{
			Debug.LogWarning(debugPrefix + " Prefab spawning is disabled. Enable it in the inspector.");
			return;
		}
		int num = 0;
		keysToRemoveBuffer.Clear();
		foreach (KeyValuePair<GameObject, GameObject> spawnedPrefabInstance in spawnedPrefabInstances)
		{
			if (spawnedPrefabInstance.Value != null)
			{
				Debug.Log(debugPrefix + " Deleting spawned prefab '" + spawnedPrefabInstance.Value.name + "' from target '" + (spawnedPrefabInstance.Key?.name ?? "null") + "'");
				UnityEngine.Object.Destroy(spawnedPrefabInstance.Value);
				num++;
			}
			keysToRemoveBuffer.Add(spawnedPrefabInstance.Key);
		}
		for (int i = 0; i < keysToRemoveBuffer.Count; i++)
		{
			spawnedPrefabInstances.Remove(keysToRemoveBuffer[i]);
		}
		if (num > 0)
		{
			Debug.Log($"{debugPrefix} Cleared {num} spawned prefab(s) from all targets");
		}
		else
		{
			Debug.Log(debugPrefix + " No spawned prefabs to clear");
		}
	}

	private GameObject GetMostRecentlyEnabledTarget()
	{
		if (mostRecentlyEnabledTarget != null && mostRecentlyEnabledTarget.activeSelf)
		{
			return mostRecentlyEnabledTarget;
		}
		if (activeObjects != null && activeObjects.Count > 0)
		{
			GameObject gameObject = activeObjects[activeObjects.Count - 1];
			Debug.LogWarning(debugPrefix + " No tracked recent target, falling back to '" + gameObject.name + "'");
			mostRecentlyEnabledTarget = gameObject;
			return gameObject;
		}
		return null;
	}

	private GameObject GetLastEnabledTargetObject()
	{
		return GetMostRecentlyEnabledTarget();
	}

	public void ClearSpawnedPrefabTracking()
	{
		spawnedPrefabInstances.Clear();
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

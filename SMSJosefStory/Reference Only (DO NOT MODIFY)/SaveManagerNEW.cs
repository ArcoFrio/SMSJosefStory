using BepInEx;
using GameCreator;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using GameCreator.Runtime.Common.SaveSystem;
using GameCreator.Runtime.Common.UnityUI;
using GameCreator.Runtime.Dialogue;
using GameCreator.Runtime.Dialogue.UnityUI;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Xml.XPath;
using TMPro;
using TransitionsPlusDemos;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using System.Xml.Serialization;

namespace SMSGallery
{
    [BepInPlugin(pluginGuid, Core.pluginName + " - SaveManager", Core.pluginVersion)]
    public class SaveManager : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsgallery.savemanager";
        #endregion

        private static SaveManager instance;
        
        // Dictionary to cache values for the current save slot
        private Dictionary<string, object> currentSlotCache = new Dictionary<string, object>();
        private int currentSaveSlot = -1;
        private string currentSaveFilePath;
        private bool afterSleepEventsProc = false;
        private bool autosaveProcedThisSession = false;  // Persistent flag: tracks if autosave was triggered during this gameplay session
        private bool saveListenersAdded = false;
        private bool saveMenuListenersAttached = false;  // Prevents listener stacking
        private bool newGame = false;
        private bool modSaveThisSession = false;

        // Multi-save mode cache - stores merged data from all saves
        private Dictionary<string, object> multiSaveCache = new Dictionary<string, object>();
        private bool multiSaveMode = false;

        // Default values for all mod variables
        private static readonly Dictionary<string, object> defaultValues = new Dictionary<string, object>
        {
            // General mod variables
            { "Mod_Version", Core.pluginVersion }
        };

        // Gallery scene tracking
        private Dictionary<string, GameObject> sceneGameObjects = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> bustGameObjects = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> npcGameObjects = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> specialSceneGameObjects = new Dictionary<string, GameObject>();
        private Dictionary<string, (GameObject npcGO, Transform directParent, Transform levelsParent, Transform topLevelParent)> npcHierarchy = new Dictionary<string, (GameObject, Transform, Transform, Transform)>();
        private Dictionary<string, (GameObject specialSceneGO, Transform directParent, Transform topLevelParent)> specialSceneHierarchy = new Dictionary<string, (GameObject, Transform, Transform)>();
        // Barista NPCs have custom unlock conditions (NPC ID -> GNV variable name, or null if unlocked by Barista_Game activation)
        private Dictionary<string, string> baristaUnlockConditions = new Dictionary<string, string>();
        private HashSet<string> markedScenesThisSession = new HashSet<string>();
        private HashSet<string> markedBustsThisSession = new HashSet<string>();
        private HashSet<string> markedNpcsThisSession = new HashSet<string>();
        private HashSet<string> markedSpecialScenesThisSession = new HashSet<string>();
        private static readonly string[] CG_MANAGER_NAMES = new string[]
        {
            "4_CG_Manager-PrivatePhotos",
            "4_CG_Manager-Sexy",
            "4_CG_Manager-Photos",
            "4_CG_Manager-Sexy_Two",
            "4_CG_Manager-Sexy_Three",
            "4_CG_Manager-Sexy_Four",
            "5_CG_EndingArt"
        };

        public void Awake()
        {
            instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        public void OnSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            ////MainStory.relaxed = false;
            modSaveThisSession = false;
            autosaveProcedThisSession = false;  // Reset autosave session flag on scene load
            if (scene.name == "CoreGameScene")
            {
                //Schedule.day = Core.GetVariableNumber("Day");
                //MainStory.generalLotteryNumber1 = Core.GetRandomNumber(100);
                //MainStory.generalLotteryNumber2 = Core.GetRandomNumber(100);
                //MainStory.generalLotteryNumber3 = Core.GetRandomNumber(100);
                //MainStory.voyeurLotteryNumber = Core.GetRandomNumber(100);
                //Debug.Log("General Lottery Number 1: " + //MainStory.generalLotteryNumber1);
                //Debug.Log("General Lottery Number 2: " + //MainStory.generalLotteryNumber2);
                //Debug.Log("General Lottery Number 3: " + //MainStory.generalLotteryNumber3);
                //Debug.Log("Voyeur Lottery Number: " + //MainStory.voyeurLotteryNumber);
                //Debug.Log("Day: " + Schedule.day);
                //Invoke(nameof(UpdateScheduleInvoke), 1.0f);
            }
            StartCoroutine(WaitAndLoadSaveFile());
        }

        private IEnumerator WaitAndLoadSaveFile()
        {
            // Wait until SaveLoadManager is initialized and SlotLoaded is valid
            while (Core.saveLoadManager == null || Core.saveLoadManager.SlotLoaded <= 0)
            {
                yield return null; // Wait one frame
            }
            currentSaveSlot = Core.saveLoadManager.SlotLoaded;
            LoadSaveFile();
        }

        public void Update()
        {
            // SANITY CHECK: Is this method even being called?
            // Debug.Log("[SaveManager] Update() called, currentScene=" + Core.currentScene.name + ", loadedCore=" + Core.loadedCore);

            if (Core.currentScene.name == "CoreGameScene")
            {
                if (Core.loadedCore)
                {
                    // Monitor scene activation for gallery tracking
                    MonitorSceneActivation();
                    MonitorBustActivation();
                    MonitorNpcActivation();
                    MonitorSpecialSceneActivation();

                    if (Core.afterSleepEvents.activeSelf && !afterSleepEventsProc)
                    {
                        afterSleepEventsProc = true;
                        autosaveProcedThisSession = true;  // Mark that autosave was triggered during this session
                    }
                    if (Core.savedUI.activeSelf && afterSleepEventsProc)
                    {
                        SaveToFile(1);  // Always save auto-save to slot 1
                        afterSleepEventsProc = false;
                    }

                    // Find NanoSave (root-level GameObject that becomes active when save menu opens)
                    Transform nanoSaveTransform = Core.currentScene.GetRootGameObjects()
                        .FirstOrDefault(go => go.name == "NanoSave")?
                        .transform;

                    // Attach listeners if NanoSave just became active
                    bool nanoSaveIsActive = nanoSaveTransform != null && nanoSaveTransform.gameObject.activeSelf;
                    bool shouldAttach = nanoSaveIsActive && !saveListenersAdded;
                    
                    if (shouldAttach)
                    {
                        if (!saveMenuListenersAttached)
                        {
                            AttachSaveSlotListenersCoreGameScene(nanoSaveTransform);
                            saveMenuListenersAttached = true;
                        }
                        saveListenersAdded = true;
                    }
                    
                    // While menu is open, check if EmptySaveSlot was recreated and needs listener re-attached
                    if (nanoSaveIsActive && saveListenersAdded)
                    {
                        Transform emptySlot = FindEmptySaveSlotInNanoSave(nanoSaveTransform);
                        if (emptySlot != null && emptySlot.GetComponent<SaveMenuEmptySaveSlotMarker>() == null)
                        {
                            // Button was recreated, re-attach listener
                            AttachEmptySaveSlotListener(emptySlot);
                        }
                    }
                    
                    // Reset flags when NanoSave is deactivated
                    if (!nanoSaveIsActive && saveListenersAdded)
                    {
                        saveMenuListenersAttached = false;
                        saveListenersAdded = false;
                    }

                    if (Core.introMomentNewGame.activeSelf && !newGame)
                    {
                        //ResetToDefaults();
                        newGame = true;
                    }
                }
            }

            if (Core.currentScene.name == "GameStart")
            {
                if (newGame)
                {
                    newGame = false;
                }
            }
        }

        #region Public Methods

        /// Sets a string value for the current save slot
        public static void SetString(string variableName, string value)
        {
            if (instance == null) return;
            instance.SetValueInternal(variableName, value);
        }
        /// Gets a string value for the current save slot
        public static string GetString(string variableName, string defaultValue = "")
        {
            if (instance == null) return defaultValue;
            return instance.GetValueInternal(variableName, defaultValue);
        }
        /// Sets an integer value for the current save slot
        public static void SetInt(string variableName, int value)
        {
            if (instance == null) return;
            // Clamp affection variables to a maximum of 5
            if (variableName.StartsWith("Affection_"))
            {
                value = Mathf.Clamp(value, 0, 5);
            }
            instance.SetValueInternal(variableName, value);
        }
        /// Gets an integer value for the current save slot
        public static int GetInt(string variableName, int defaultValue = 0)
        {
            if (instance == null) return defaultValue;
            return instance.GetValueInternal(variableName, defaultValue);
        }
        /// Sets a float value for the current save slot
        public static void SetFloat(string variableName, float value)
        {
            if (instance == null) return;
            instance.SetValueInternal(variableName, value);
        }
        /// Gets a float value for the current save slot
        public static float GetFloat(string variableName, float defaultValue = 0f)
        {
            if (instance == null) return defaultValue;
            return instance.GetValueInternal(variableName, defaultValue);
        }
        /// Sets a boolean value for the current save slot
        public static void SetBool(string variableName, bool value)
        {
            if (instance == null) return;
            instance.SetValueInternal(variableName, value);
        }
        /// Gets a boolean value for the current save slot
        public static bool GetBool(string variableName, bool defaultValue = false)
        {
            if (instance == null) return defaultValue;
            return instance.GetValueInternal(variableName, defaultValue);
        }
        /// Deletes a variable for the current save slot
        public static void DeleteVariable(string variableName)
        {
            if (instance == null) return;
            instance.DeleteVariableInternal(variableName);
        }
        /// Checks if a variable exists for the current save slot
        public static bool HasVariable(string variableName)
        {
            if (instance == null) return false;
            return instance.HasVariableInternal(variableName);
        }
        /// Gets all variables for the current save slot
        public static Dictionary<string, object> GetAllVariables()
        {
            if (instance == null) return new Dictionary<string, object>();
            return new Dictionary<string, object>(instance.currentSlotCache);
        }
        /// Clears all variables for the current save slot
        public static void ClearAllVariables()
        {
            if (instance == null) return;
            instance.ClearAllVariablesInternal();
        }
        /// Resets all variables to their default values for the current save slot
        public static void ResetToDefaults()
        {
            if (instance == null) return;
            instance.ResetToDefaultsInternal();
        }
        /// Load all saves for multi-save gallery mode (called from Core)
        public void LoadAllSavesForGallery()
        {
            LoadAllSavesForMultiMode();
        }

        /// <summary>
        /// Refresh the MultiSaveMode setting from Core config and reload the appropriate save data.
        /// Call this after toggling MultiSaveMode to ensure gallery reflects the correct saved items.
        /// </summary>
        public void RefreshMultiSaveMode()
        {
            // First, reload the current slot from file to ensure we have the latest data
            LoadSaveFile();
            
            // Then, update multiSaveMode and reload multi-save cache if needed
            LoadAllSavesForMultiMode();
            
            Debug.Log($"[SaveManager] Refreshed MultiSaveMode: {multiSaveMode}");
        }

        /// <summary>
        /// Toggle MultiSaveMode while preserving all "seen" flags marked in the current session.
        /// This ensures items marked as seen remain unlocked regardless of the mode.
        /// Called from the gallery toggle button.
        /// </summary>
        public void ToggleMultiSaveModePreservingRuntime()
        {
            // CRITICAL: Collect only items marked DURING THIS GAMEPLAY SESSION
            // We must distinguish between:
            // 1. Items marked THIS SESSION (should always be preserved)
            // 2. Items from OTHER SAVES (should not be preserved when switching OFF)
            // 
            // The markedScenesThisSession/etc HashSets track items actually marked during this session,
            // so we use them to filter what to preserve when toggling modes.
            Dictionary<string, bool> runtimeOnlyChanges = new Dictionary<string, bool>();
            
            if (multiSaveMode)
            {
                // Currently in MULTI-SAVE mode: only preserve items marked this session from multiSaveCache
                // Check multiSaveCache for values of items we marked this session
                foreach (string sceneId in markedScenesThisSession)
                {
                    string varName = $"Scene_Seen_{sceneId}";
                    if (multiSaveCache.TryGetValue(varName, out object val) && val is bool && (bool)val)
                    {
                        runtimeOnlyChanges[varName] = true;
                    }
                }
                foreach (string bustId in markedBustsThisSession)
                {
                    string varName = $"Bust_Seen_{bustId}";
                    if (multiSaveCache.TryGetValue(varName, out object val) && val is bool && (bool)val)
                    {
                        runtimeOnlyChanges[varName] = true;
                    }
                }
                foreach (string npcId in markedNpcsThisSession)
                {
                    string varName = $"NPC_Seen_{npcId}";
                    if (multiSaveCache.TryGetValue(varName, out object val) && val is bool && (bool)val)
                    {
                        runtimeOnlyChanges[varName] = true;
                    }
                }
                foreach (string sceneId in markedSpecialScenesThisSession)
                {
                    string varName = $"SpecialScene_Seen_{sceneId}";
                    if (multiSaveCache.TryGetValue(varName, out object val) && val is bool && (bool)val)
                    {
                        runtimeOnlyChanges[varName] = true;
                    }
                }
            }
            else
            {
                // Currently in SINGLE-SAVE mode: only preserve items marked this session from currentSlotCache
                // Check currentSlotCache for values of items we marked this session
                foreach (string sceneId in markedScenesThisSession)
                {
                    string varName = $"Scene_Seen_{sceneId}";
                    if (currentSlotCache.TryGetValue(varName, out object val) && val is bool && (bool)val)
                    {
                        runtimeOnlyChanges[varName] = true;
                    }
                }
                foreach (string bustId in markedBustsThisSession)
                {
                    string varName = $"Bust_Seen_{bustId}";
                    if (currentSlotCache.TryGetValue(varName, out object val) && val is bool && (bool)val)
                    {
                        runtimeOnlyChanges[varName] = true;
                    }
                }
                foreach (string npcId in markedNpcsThisSession)
                {
                    string varName = $"NPC_Seen_{npcId}";
                    if (currentSlotCache.TryGetValue(varName, out object val) && val is bool && (bool)val)
                    {
                        runtimeOnlyChanges[varName] = true;
                    }
                }
                foreach (string sceneId in markedSpecialScenesThisSession)
                {
                    string varName = $"SpecialScene_Seen_{sceneId}";
                    if (currentSlotCache.TryGetValue(varName, out object val) && val is bool && (bool)val)
                    {
                        runtimeOnlyChanges[varName] = true;
                    }
                }
            }
            
            Debug.Log($"[SaveManager] Collected {runtimeOnlyChanges.Count} session-marked items before toggling MultiSaveMode (Scenes: {markedScenesThisSession.Count}, Busts: {markedBustsThisSession.Count}, NPCs: {markedNpcsThisSession.Count}, SpecialScenes: {markedSpecialScenesThisSession.Count})");
            
            // Reload the current slot from file to restore all saved flags
            // This ensures we preserve disk-saved data when toggling multisave mode
            LoadSaveFile();
            
            // Update multiSaveMode and reload caches appropriately
            multiSaveMode = Core.GetMultiSaveMode();
            
            if (multiSaveMode)
            {
                // Switching to MULTI-SAVE mode: merge all saves + restore session-marked items
                LoadAllSavesForMultiMode();
                
                // Restore only session-marked items to multiSaveCache (they have priority over merged data)
                foreach (var kvp in runtimeOnlyChanges)
                {
                    multiSaveCache[kvp.Key] = true;
                }
                Debug.Log($"[SaveManager] Restored {runtimeOnlyChanges.Count} session-marked items to multiSaveCache");
            }
            else
            {
                // Switching to SINGLE-SAVE mode: currentSlotCache now has save file data
                // Only restore items marked this session, not merged data from other saves
                multiSaveCache.Clear();
                
                // Restore only session-marked items to currentSlotCache (they have priority over disk values)
                foreach (var kvp in runtimeOnlyChanges)
                {
                    currentSlotCache[kvp.Key] = true;
                }
                Debug.Log($"[SaveManager] Restored {runtimeOnlyChanges.Count} session-marked items to currentSlotCache");
            }
            
            Debug.Log($"[SaveManager] Toggled MultiSaveMode: {multiSaveMode}");
        }

        /// <summary>
        /// Returns true if any bool save variable whose name contains the given substring is true.
        /// </summary>
        public static bool AnyBoolVariableWithNameContains(string substring)
        {
            if (instance == null || string.IsNullOrEmpty(substring)) return false;
            string substringLower = substring.ToLower();
            foreach (var kvp in instance.currentSlotCache)
            {
                if (kvp.Key.ToLower().Contains(substringLower) && kvp.Value is bool b && b)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the count of bool save variables whose name contains the given substring and are true.
        /// </summary>
        public static int CountBoolVariablesWithNameContains(string substring)
        {
            if (instance == null || string.IsNullOrEmpty(substring)) return 0;
            string substringLower = substring.ToLower();
            int count = 0;
            foreach (var kvp in instance.currentSlotCache)
            {
                if (kvp.Key.ToLower().Contains(substringLower) && kvp.Value is bool b && b)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Find EmptySaveSlot(Clone) within the NanoSave hierarchy.
        /// Returns null if not found.
        /// </summary>
        private Transform FindEmptySaveSlotInNanoSave(Transform nanoSaveTransform)
        {
            if (nanoSaveTransform == null) return null;

            // Navigate: NanoSave > Content > Content > List > Viewport > Content
            Transform contentLevel1 = nanoSaveTransform.Find("Content");
            if (contentLevel1 == null) return null;

            Transform contentLevel2 = contentLevel1.Find("Content");
            if (contentLevel2 == null) return null;

            Transform listTransform = contentLevel2.Find("List");
            if (listTransform == null) return null;

            Transform viewportTransform = listTransform.Find("Viewport");
            if (viewportTransform == null) return null;

            Transform contentParent = viewportTransform.Find("Content");
            if (contentParent == null) return null;

            // Find EmptySaveSlot(Clone) in children
            for (int i = 0; i < contentParent.childCount; i++)
            {
                Transform child = contentParent.GetChild(i);
                if (child.name.StartsWith("EmptySaveSlot"))
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        /// Dynamically discover and attach listeners to all save slot buttons in CoreGameScene.
        /// Navigates through NanoSave UI hierarchy to find SaveSlots(Clone) GOs.
        /// Also attaches listener to EmptySaveSlot(Clone) for creating new saves.
        /// </summary>
        private void AttachSaveSlotListenersCoreGameScene(Transform nanoSaveTransform)
        {
            if (nanoSaveTransform == null)
            {
                Debug.LogError("[SaveManager] nanoSaveTransform is null, cannot attach listeners");
                return;
            }

            if (!nanoSaveTransform.gameObject.activeSelf)
            {
                Debug.LogWarning("[SaveManager] NanoSave is not active");
                return;
            }

            Debug.Log("[SaveManager] NanoSave is active, attempting to navigate hierarchy");

            // Navigate to the Content GO containing SaveSlots(Clone) GOs
            // Path: NanoSave > Content > Content > List > Viewport > Content
            Transform contentLevel1 = nanoSaveTransform.Find("Content");
            if (contentLevel1 == null)
            {
                Debug.LogError("[SaveManager] Could not find Content (level 1) in NanoSave");
                return;
            }

            Transform contentLevel2 = contentLevel1.Find("Content");
            if (contentLevel2 == null)
            {
                Debug.LogError("[SaveManager] Could not find Content (level 2) in NanoSave");
                return;
            }

            Transform listTransform = contentLevel2.Find("List");
            if (listTransform == null)
            {
                Debug.LogError("[SaveManager] Could not find List in NanoSave hierarchy");
                return;
            }

            Transform viewportTransform = listTransform.Find("Viewport");
            if (viewportTransform == null)
            {
                Debug.LogError("[SaveManager] Could not find Viewport in NanoSave hierarchy");
                return;
            }

            Transform contentParent = viewportTransform.Find("Content");
            if (contentParent == null)
            {
                Debug.LogError("[SaveManager] Could not find Content (parent) in NanoSave hierarchy");
                return;
            }

            Debug.Log($"[SaveManager] Found content parent with {contentParent.childCount} children");

            // Iterate through all children to find EmptySaveSlot(Clone) and SaveSlots(Clone) GOs
            bool emptySlotFound = false;
            for (int i = 0; i < contentParent.childCount; i++)
            {
                Transform child = contentParent.GetChild(i);
                Debug.Log($"[SaveManager] Child {i}: {child.name}");
                
                // Look for EmptySaveSlot(Clone) - attach save button listener
                if (child.name.StartsWith("EmptySaveSlot"))
                {
                    Debug.Log($"[SaveManager] Found EmptySaveSlot at index {i}, attaching listener");
                    AttachEmptySaveSlotListener(child);
                    emptySlotFound = true;
                }
            }

            if (!emptySlotFound)
            {
                Debug.LogWarning("[SaveManager] EmptySaveSlot(Clone) not found in content parent");
            }

            Debug.Log("[SaveManager] Attached listeners to save UI buttons in CoreGameScene");
        }

        /// <summary>
        /// Attach listener to EmptySaveSlot(Clone) GO to save to the next available slot.
        /// </summary>
        private void AttachEmptySaveSlotListener(Transform emptySlotTransform)
        {
            Debug.Log($"[SaveManager] AttachEmptySaveSlotListener called for {emptySlotTransform.name}");
            
            // Check if listener is already attached to prevent stacking
            if (emptySlotTransform.GetComponent<SaveMenuEmptySaveSlotMarker>() != null)
            {
                Debug.Log($"[SaveManager] Marker already exists on {emptySlotTransform.name}, listener already attached, skipping to prevent stacking");
                return;
            }

            Debug.Log($"[SaveManager] Searching for ButtonInstructions on {emptySlotTransform.name}...");
            
            ButtonInstructions saveButton = emptySlotTransform.GetComponent<ButtonInstructions>();
            if (saveButton != null)
            {
                Debug.Log($"[SaveManager] ✓ Found ButtonInstructions directly on {emptySlotTransform.name}");
            }
            else
            {
                Debug.LogWarning($"[SaveManager] ✗ ButtonInstructions NOT found directly on {emptySlotTransform.name}, searching in children...");
                
                // Try to find it in children
                ButtonInstructions[] childButtons = emptySlotTransform.GetComponentsInChildren<ButtonInstructions>();
                Debug.Log($"[SaveManager] GetComponentsInChildren returned {childButtons.Length} ButtonInstructions components");
                
                if (childButtons.Length > 0)
                {
                    saveButton = childButtons[0];
                    Debug.Log($"[SaveManager] ✓ Using ButtonInstructions from child: {saveButton.gameObject.name}");
                }
                else
                {
                    Debug.LogError($"[SaveManager] ✗ ERROR: Could not find ButtonInstructions anywhere on {emptySlotTransform.name} or its children!");
                    
                    // Debug: Log the hierarchy
                    Debug.Log($"[SaveManager] EmptySaveSlot hierarchy:");
                    foreach (Transform child in emptySlotTransform.GetComponentsInChildren<Transform>())
                    {
                        Debug.Log($"[SaveManager]   - {child.name} (components: {string.Join(", ", child.GetComponents<UnityEngine.Component>().Select(c => c.GetType().Name).Take(5))})");
                    }
                    return;
                }
            }

            // Attach listener to save to the next available slot
            Debug.Log($"[SaveManager] Attaching onClick listener to ButtonInstructions on {saveButton.gameObject.name}");
            saveButton.onClick.AddListener(() =>
            {
                Debug.Log("[SaveManager] ► EmptySaveSlot button clicked! Saving to next available slot...");
                SaveToNextAvailableSlot();
            });
            
            // Mark this transform as having a listener attached
            emptySlotTransform.gameObject.AddComponent<SaveMenuEmptySaveSlotMarker>();
            Debug.Log($"[SaveManager] Successfully attached listener to {emptySlotTransform.name}");

            Debug.Log("[SaveManager] Successfully attached save listener to EmptySaveSlot(Clone)");
        }

        /// <summary>
        /// Detect the most recently created/modified save folder and save mod data to it.
        /// This syncs with the vanilla game's save by waiting for it to complete.
        /// Only proceeds if 5_Levels > 5_MyRoom is active (player is in the correct location).
        /// Uses Invoke to avoid blocking the game flow.
        /// </summary>
        private void SaveToNextAvailableSlot()
        {
            // Check if 5_Levels > 5_MyRoom is active before saving
            GameObject levelsGO = GameObject.Find("5_Levels");
            if (levelsGO == null)
            {
                Debug.LogWarning("[SaveManager] 5_Levels not found, cannot save");
                return;
            }

            Transform myRoomTransform = levelsGO.transform.Find("5_MyRoom");
            if (myRoomTransform == null || !myRoomTransform.gameObject.activeSelf)
            {
                Debug.LogWarning("[SaveManager] 5_Levels > 5_MyRoom is not active, saving is not allowed from this location");
                return;
            }

            Debug.Log("[SaveManager] SaveToNextAvailableSlot() called, using Invoke to wait for game save");
            // Cancel any pending invokes to allow multiple clicks
            CancelInvoke(nameof(SaveToLatestSlot));
            // Wait 0.2 seconds for the vanilla game save to complete, then save our data
            Invoke(nameof(SaveToLatestSlot), 0.2f);
        }

        /// <summary>
        /// Find the most recently modified NANOSAVE_* folder and save mod data to it.
        /// Called via Invoke after button click to ensure game's save is complete.
        /// </summary>
        private void SaveToLatestSlot()
        {
            Debug.Log("[SaveManager] SaveToLatestSlot() invoked");
            
            try
            {
                // Get the base saves directory
                string localLowPath = Path.Combine(
                    Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)),
                    "LocalLow",
                    "Arvus Games",
                    "Starmaker Story",
                    "Saves"
                );

                if (!Directory.Exists(localLowPath))
                {
                    Debug.LogError("[SaveManager] Saves directory does not exist");
                    return;
                }

                // Find all NANOSAVE_XXXX folders
                string[] slotFolders = Directory.GetDirectories(localLowPath, "NANOSAVE_*");
                if (slotFolders.Length == 0)
                {
                    Debug.LogError("[SaveManager] No save folders found");
                    return;
                }

                // Find the most recently modified folder
                string latestFolder = slotFolders
                    .OrderByDescending(folder => Directory.GetLastWriteTime(folder))
                    .FirstOrDefault();

                if (latestFolder == null)
                {
                    Debug.LogError("[SaveManager] Could not find latest save folder");
                    return;
                }

                // Extract the slot number from the folder name
                string folderName = Path.GetFileName(latestFolder);
                if (!folderName.StartsWith("NANOSAVE_") || !int.TryParse(folderName.Substring("NANOSAVE_".Length), out int latestSlot))
                {
                    Debug.LogError($"[SaveManager] Could not parse slot number from folder: {folderName}");
                    return;
                }

                // Determine which slot to copy from:
                // - If on slot -1 (new game): copy from auto-save (slot 1)
                // - If auto-save was triggered during this gameplay session: copy from auto-save (slot 1)
                // - Otherwise: copy from current slot
                int sourceSlot;
                string copyReason;
                
                if (currentSaveSlot == -1)
                {
                    sourceSlot = 1;
                    copyReason = "new game (slot -1)";
                }
                else if (autosaveProcedThisSession)
                {
                    sourceSlot = 1;
                    copyReason = "auto-save triggered during gameplay";
                }
                else
                {
                    sourceSlot = currentSaveSlot;
                    copyReason = $"current slot ({currentSaveSlot})";
                }

                string sourceFile = instance.GetSaveFilePath(sourceSlot);
                string destFile = instance.GetSaveFilePath(latestSlot);
                
                if (!File.Exists(sourceFile))
                {
                    Debug.LogError($"[SaveManager] Source file does not exist: {sourceFile}");
                    return;
                }
                
                try
                {
                    File.Copy(sourceFile, destFile, true);
                    Debug.Log($"[SaveManager] Copied slot {sourceSlot} ({copyReason}) to slot {latestSlot}");
                }
                catch (Exception copyEx)
                {
                    Debug.LogError($"[SaveManager] Error copying slot {sourceSlot} to slot {latestSlot}: {copyEx.Message}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error finding latest save slot: {e.Message}\n{e.StackTrace}");
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the full path to a save file in the game's save directory.
        /// Path: AppData\LocalLow\Arvus Games\Starmaker Story\Saves\NANOSAVE_XXXX\GalleryModSave.txt
        /// Where XXXX is the slot number formatted with leading zeros.
        /// </summary>
        private string GetSaveFilePath(int slot)
        {
            if (slot < 1)
            {
                Debug.LogError($"[SaveManager] Invalid save slot: {slot}");
                return null;
            }

            // Get LocalLow folder dynamically
            string localLowPath = Path.Combine(
                Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)),
                "LocalLow",
                "Arvus Games",
                "Starmaker Story",
                "Saves"
            );

            // Format slot number with leading zeros (1 -> 0001, 14 -> 0014, etc.)
            string slotFolder = $"NANOSAVE_{slot:D4}";
            string slotPath = Path.Combine(localLowPath, slotFolder);
            
            return Path.Combine(slotPath, "GalleryModSave.txt");
        }

        /// <summary>
        /// Ensures the directory for a save slot exists.
        /// </summary>
        private bool EnsureSaveDirectoryExists(int slot)
        {
            try
            {
                string filePath = GetSaveFilePath(slot);
                string directory = Path.GetDirectoryName(filePath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.Log($"[SaveManager] Created save directory: {directory}");
                }
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error ensuring save directory exists for slot {slot}: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Internal Methods
        private void DeleteAllSaveFiles()
        {
            Debug.Log("[SaveManager] Deleting all mod save files...");
            
            try
            {
                // Get the base saves directory
                string localLowPath = Path.Combine(
                    Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)),
                    "LocalLow",
                    "Arvus Games",
                    "Starmaker Story",
                    "Saves"
                );

                if (!Directory.Exists(localLowPath))
                {
                    Debug.Log("[SaveManager] Saves directory does not exist, nothing to delete");
                    return;
                }

                // Find all NANOSAVE_XXXX folders that contain our GalleryModSave.txt
                string[] slotFolders = Directory.GetDirectories(localLowPath, "NANOSAVE_*");
                foreach (string slotFolder in slotFolders)
                {
                    try
                    {
                        string galleryModSaveFile = Path.Combine(slotFolder, "GalleryModSave.txt");
                        if (File.Exists(galleryModSaveFile))
                        {
                            File.Delete(galleryModSaveFile);
                            Debug.Log($"[SaveManager] Deleted mod save file: {galleryModSaveFile}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[SaveManager] Error deleting mod save file in {slotFolder}: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error accessing saves directory: {e.Message}");
            }
            
            // Reset current slot cache and variables
            currentSlotCache.Clear();
            currentSaveSlot = -1;
            modSaveThisSession = false;
            Debug.Log("[SaveManager] All mod save files deleted.");
        }
        private void SetValueInternal(string variableName, object value)
        {
            // Allow setting values even when currentSaveSlot is -1 (new game, before any save loaded)
            // This ensures gallery tracking works from game start
            if (currentSaveSlot < -1) return;

            // Update cache
            currentSlotCache[variableName] = value;
            Debug.Log($"[SaveManager] Set runtime variable {variableName} = {value} for save slot {currentSaveSlot}");
        }
        private T GetValueInternal<T>(string variableName, T defaultValue)
        {
            // Allow getting values even when currentSaveSlot is -1 (new game, before any save loaded)
            // This ensures gallery tracking works from game start
            if (currentSaveSlot < -1) return defaultValue;

            // If multi-save mode is enabled and we're looking for seen flags, check multi-save cache
            if (multiSaveMode && (variableName.StartsWith("Scene_Seen_") || variableName.StartsWith("Bust_Seen_") || variableName.StartsWith("NPC_Seen_") || variableName.StartsWith("SpecialScene_Seen_")))
            {
                if (multiSaveCache.TryGetValue(variableName, out object cachedValue))
                {
                    if (cachedValue is T typedValue)
                    {
                        return typedValue;
                    }
                }
            }

            // Check normal cache first
            if (currentSlotCache.TryGetValue(variableName, out object normalCachedValue))
            {
                if (normalCachedValue is T typedValue)
                {
                    return typedValue;
                }
            }

            // If not in cache, return default value
            return defaultValue;
        }
        private void DeleteVariableInternal(string variableName)
        {
            // Allow deletion even when currentSaveSlot is -1 (new game start)
            if (currentSaveSlot < -1) return;
            currentSlotCache.Remove(variableName);
            Debug.Log($"[SaveManager] Deleted runtime variable {variableName} for save slot {currentSaveSlot}");
        }
        private bool HasVariableInternal(string variableName)
        {
            // Allow checking existence even when currentSaveSlot is -1 (new game start)
            if (currentSaveSlot < -1) return false;
            return currentSlotCache.ContainsKey(variableName);
        }
        private void ClearAllVariablesInternal()
        {
            // Allow clearing even when currentSaveSlot is -1 (new game start)
            if (currentSaveSlot < -1) return;
            currentSlotCache.Clear();
            Debug.Log($"[SaveManager] Cleared all runtime variables for save slot {currentSaveSlot}");
        }
        private void ResetToDefaultsInternal()
        {
            // Allow resetting even when currentSaveSlot is -1 (new game start)
            if (currentSaveSlot < -1) return;
            Debug.Log($"[SaveManager] ResetToDefaultsInternal called for slot {currentSaveSlot}");
            Debug.Log($"[SaveManager] defaultValues count: {defaultValues.Count}");
            currentSlotCache.Clear();

            // Add all default values
            foreach (var kvp in defaultValues)
            {
                Debug.Log($"[SaveManager] Adding default: {kvp.Key} = {kvp.Value}");
                currentSlotCache[kvp.Key] = kvp.Value;
            }
            // We still want to save here to create the initial file
            SaveToFile(currentSaveSlot);
            modSaveThisSession = false;
            Debug.Log($"[SaveManager] Reset all variables to defaults for save slot {currentSaveSlot}");
        }
        private void LoadSaveFile()
        {
            currentSaveFilePath = GetSaveFilePath(currentSaveSlot);
            currentSlotCache.Clear();
            try
            {
                if (File.Exists(currentSaveFilePath))
                {
                    foreach (var line in File.ReadAllLines(currentSaveFilePath))
                    {
                        if (string.IsNullOrWhiteSpace(line) || !line.Contains("=")) continue;
                        var parts = line.Split(new[] { '=' }, 2);
                        var key = parts[0];
                        var value = parts[1];
                        if (key == "saveSlot" || key == "lastSaved") continue;
                        
                        // Try to parse value type based on defaultValues
                        if (defaultValues.TryGetValue(key, out object defaultVal))
                        {
                            if (defaultVal is int && int.TryParse(value, out int intVal))
                                currentSlotCache[key] = intVal;
                            else if (defaultVal is bool && bool.TryParse(value, out bool boolVal))
                                currentSlotCache[key] = boolVal;
                            else if (defaultVal is float && float.TryParse(value, out float floatVal))
                                currentSlotCache[key] = floatVal;
                            else
                                currentSlotCache[key] = value;
                        }
                        else
                        {
                            // For gallery "seen" flags, always try to parse as boolean
                            if (key.StartsWith("Scene_Seen_") || key.StartsWith("Bust_Seen_") || 
                                key.StartsWith("NPC_Seen_") || key.StartsWith("SpecialScene_Seen_"))
                            {
                                if (bool.TryParse(value, out bool boolVal))
                                {
                                    currentSlotCache[key] = boolVal;
                                }
                                else
                                {
                                    currentSlotCache[key] = value;
                                }
                            }
                            else
                            {
                                currentSlotCache[key] = value;
                            }
                        }
                    }
                    Debug.Log($"[SaveManager] Loaded save file: {currentSaveFilePath}");
                }
                else
                {
                    // Create new save file with default values
                    ResetToDefaultsInternal();
                    // SaveToFile is already called by ResetToDefaultsInternal
                    Debug.Log($"[SaveManager] Created new save file with defaults: {currentSaveFilePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error loading save file: {e.Message}");
                // Fallback to defaults
                ResetToDefaultsInternal();
            }
        }

        private void LoadAllSavesForMultiMode()
        {
            multiSaveCache.Clear();
            multiSaveMode = Core.GetMultiSaveMode();
            
            if (!multiSaveMode)
                return;

            try
            {
                // Get the base saves directory
                string localLowPath = Path.Combine(
                    Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)),
                    "LocalLow",
                    "Arvus Games",
                    "Starmaker Story",
                    "Saves"
                );

                if (!Directory.Exists(localLowPath))
                {
                    Debug.LogWarning($"[SaveManager] Saves directory not found: {localLowPath}");
                    return;
                }

                // Get all NANOSAVE_XXXX folders and sort them
                string[] slotFolders = Directory.GetDirectories(localLowPath, "NANOSAVE_*")
                    .OrderBy(f => f).ToArray();

                if (slotFolders.Length == 0)
                {
                    Debug.Log("[SaveManager] No save folders found for multi-save mode");
                    return;
                }

                Debug.Log($"[SaveManager] Multi-save mode enabled. Loading {slotFolders.Length} save slots...");

                // Merge data from all save files, prioritizing any boolean seen flags
                foreach (var slotFolder in slotFolders)
                {
                    string galleryModSaveFile = Path.Combine(slotFolder, "GalleryModSave.txt");
                    
                    if (!File.Exists(galleryModSaveFile))
                        continue;
                    
                    try
                    {
                        foreach (var line in File.ReadAllLines(galleryModSaveFile))
                        {
                            if (string.IsNullOrWhiteSpace(line) || !line.Contains("=")) continue;
                            var parts = line.Split(new[] { '=' }, 2);
                            var key = parts[0];
                            var value = parts[1];
                            if (key == "saveSlot" || key == "lastSaved") continue;

                            // For seen flags, use OR logic (if seen in ANY save, it's seen)
                            if (key.StartsWith("Scene_Seen_") || key.StartsWith("Bust_Seen_") || key.StartsWith("NPC_Seen_") || key.StartsWith("SpecialScene_Seen_"))
                            {
                                if (bool.TryParse(value, out bool boolVal) && boolVal)
                                {
                                    multiSaveCache[key] = true;
                                }
                                else if (!multiSaveCache.ContainsKey(key))
                                {
                                    multiSaveCache[key] = false;
                                }
                            }
                            else
                            {
                                // For other values, just store (don't overwrite)
                                if (!multiSaveCache.ContainsKey(key))
                                {
                                    multiSaveCache[key] = value;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[SaveManager] Error reading save file {galleryModSaveFile}: {e.Message}");
                    }
                }

                Debug.Log($"[SaveManager] Loaded multi-save cache with {multiSaveCache.Count} merged values");
                
                // Also merge the current slot's runtime cache to include items marked as seen this session
                // but not yet saved to disk (seen flags use OR logic)
                foreach (var kvp in currentSlotCache)
                {
                    if (kvp.Key.StartsWith("Scene_Seen_") || kvp.Key.StartsWith("Bust_Seen_") || kvp.Key.StartsWith("NPC_Seen_") || kvp.Key.StartsWith("SpecialScene_Seen_"))
                    {
                        if (kvp.Value is bool boolVal && boolVal)
                        {
                            multiSaveCache[kvp.Key] = true;
                        }
                        else if (!multiSaveCache.ContainsKey(kvp.Key) && kvp.Value is bool)
                        {
                            multiSaveCache[kvp.Key] = kvp.Value;
                        }
                    }
                }
                
                Debug.Log($"[SaveManager] Merged current slot runtime cache into multi-save cache. Final count: {multiSaveCache.Count}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error loading all saves for multi-mode: {e.Message}");
            }
        }

        private void SaveToFile(int? slotOverride = null)
        {
            int slot = slotOverride ?? currentSaveSlot;
            Debug.Log($"[SaveManager] SaveToFile called: slotOverride={slotOverride}, currentSaveSlot={currentSaveSlot}, using slot={slot}");
            
            if (slot < 0) 
            {
                Debug.LogError($"[SaveManager] SaveToFile: slot is negative ({slot}), returning");
                return;
            }
            
            // Ensure the save directory exists
            if (!EnsureSaveDirectoryExists(slot))
            {
                Debug.LogError($"[SaveManager] Failed to create save directory for slot {slot}");
                return;
            }

            string saveFilePath = GetSaveFilePath(slot);
            Debug.Log($"[SaveManager] Saving to file: {saveFilePath}");

            try
            {
                using (var writer = new StreamWriter(saveFilePath))
                {
                    writer.WriteLine($"saveSlot={slot}");
                    writer.WriteLine($"lastSaved={DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    foreach (var kvp in currentSlotCache)
                    {
                        writer.WriteLine($"{kvp.Key}={kvp.Value}");
                    }
                }
                Debug.Log($"[SaveManager] ✓ Successfully saved to slot {slot} at {saveFilePath}");
                modSaveThisSession = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error saving to slot {slot}: {e.Message}");
            }
        }

        #endregion
        /// Overwrites the specified save slot's file with the contents of the current save slot's file.
        /// This copies only the serialized file data, not the current in-memory variables.
        /// <param name="targetSlot">The save slot to overwrite (e.g., 2, 3, ...)</param>
        public static void OverwriteSaveSlotWithCurrentFile(int targetSlot)
        {
            if (instance == null) return;

            int sourceSlot = instance.modSaveThisSession ? 1 : instance.currentSaveSlot;
            string sourceFile = instance.GetSaveFilePath(sourceSlot);
            string destFile = instance.GetSaveFilePath(targetSlot);

            if (!File.Exists(sourceFile))
            {
                Debug.LogError($"[SaveManager] Source save file does not exist: {sourceFile}");
                return;
            }

            // Ensure destination directory exists
            if (!instance.EnsureSaveDirectoryExists(targetSlot))
            {
                Debug.LogError($"[SaveManager] Failed to create save directory for target slot {targetSlot}");
                return;
            }

            try
            {
                File.Copy(sourceFile, destFile, true);
                Debug.Log($"[SaveManager] Overwrote save slot {targetSlot} with data from slot {sourceSlot}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error overwriting save slot {targetSlot}: {e.Message}");
            }
        }

        // Add utility method to check if a variable has changed between runtime and save file
        public static bool HasVariableChanged(string variableName)
        {
            if (instance == null || instance.currentSaveSlot < 0) return false;

            // Get runtime value
            if (!instance.currentSlotCache.TryGetValue(variableName, out object runtimeValue))
                return false; // Variable doesn't exist in runtime

            // Get saved value
            string saveFilePath = instance.GetSaveFilePath(instance.currentSaveSlot);
            if (!File.Exists(saveFilePath))
                return true; // No save file means runtime value is different

            try
            {
                foreach (var line in File.ReadAllLines(saveFilePath))
                {
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains("=")) continue;
                    var parts = line.Split(new[] { '=' }, 2);
                    if (parts[0] == variableName)
                    {
                        var value = parts[1];
                        // Try to parse value based on defaultValues
                        if (defaultValues.TryGetValue(variableName, out object defaultVal))
                        {
                            object savedValue = null;
                            if (defaultVal is int && int.TryParse(value, out int intVal))
                                savedValue = intVal;
                            else if (defaultVal is bool && bool.TryParse(value, out bool boolVal))
                                savedValue = boolVal;
                            else if (defaultVal is float && float.TryParse(value, out float floatVal))
                                savedValue = floatVal;
                            else
                                savedValue = value;

                            return !Equals(runtimeValue, savedValue);
                        }
                        return !Equals(runtimeValue.ToString(), value);
                    }
                }
                return true; // Variable not found in save file means it's different
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Error checking variable change: {e.Message}");
                return false;
            }
        }

        //private void UpdateScheduleInvoke()
        //{
        //    Schedule.UpdateScheduleForDay();
        //}

        #region Gallery Scene Tracking

        /// <summary>
        /// Initialize gallery scene tracking by scanning all CG Manager GOs for scenes
        /// </summary>
        public void InitializeSceneTracking()
        {
            int sceneCount = 0;
            foreach (string managerName in CG_MANAGER_NAMES)
            {
                // Search from scene root using GameObject.Find instead of Core.level
                GameObject managerGO = GameObject.Find(managerName);
                if (managerGO != null)
                {
                    Transform manager = managerGO.transform;
                    for (int i = 0; i < manager.childCount; i++)
                    {
                        Transform scene = manager.GetChild(i);
                        string sceneId = scene.gameObject.name;
                        
                        // Add to defaultValues if not already there
                        string sceneVarName = $"Scene_Seen_{sceneId}";
                        if (!defaultValues.ContainsKey(sceneVarName))
                        {
                            defaultValues[sceneVarName] = false;
                        }
                        
                        sceneGameObjects[sceneId] = scene.gameObject;
                        sceneCount++;
                    }
                }
                else
                {
                    Debug.LogWarning($"[SaveManager] CG Manager not found: {managerName}");
                }
            }
            Debug.Log($"[SaveManager] Initialized scene tracking for {sceneCount} scenes");
        }

        /// <summary>
        /// Initialize bust tracking from 2_Bust_Manager and SpecialBusts
        /// </summary>
        public void InitializeBustTracking()
        {
            int bustCount = 0;
            
            string[] bustManagerNames = new string[]
            {
                "2_Bust_Manager",
                "SpecialBusts"
            };
            
            foreach (string bustManagerName in bustManagerNames)
            {
                GameObject bustManagerGO = GameObject.Find(bustManagerName);
                if (bustManagerGO != null)
                {
                    Transform bustManager = bustManagerGO.transform;
                    for (int i = 0; i < bustManager.childCount; i++)
                    {
                        Transform bust = bustManager.GetChild(i);
                        string bustId = bust.gameObject.name;
                        
                        // Add to defaultValues if not already there
                        string bustVarName = $"Bust_Seen_{bustId}";
                        if (!defaultValues.ContainsKey(bustVarName))
                        {
                            defaultValues[bustVarName] = false;
                        }
                        
                        bustGameObjects[bustId] = bust.gameObject;
                        bustCount++;
                    }
                }
            }
            Debug.Log($"[SaveManager] Initialized bust tracking for {bustCount} busts");
        }

        public void InitializeNpcTracking()
        {
            int npcCount = 0;
            npcHierarchy.Clear();
            GameObject levelsGO = GameObject.Find("5_Levels");
            if (levelsGO != null)
            {
                // Iterate through all children of 5_Levels
                for (int i = 0; i < levelsGO.transform.childCount; i++)
                {
                    Transform levelsParentTransform = levelsGO.transform.GetChild(i);
                    
                    // Look for "NPCs" child in this parent
                    Transform npcsFolder = levelsParentTransform.Find("NPCs");
                    if (npcsFolder != null)
                    {
                        // Recursively find all GOs with SpriteRenderers in NPCs hierarchy
                        // Pass the 5_Levels parent transform for hierarchical naming
                        CollectNpcsRecursive(npcsFolder, levelsParentTransform, ref npcCount);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[SaveManager] 5_Levels not found");
            }
            
            // Also collect Adorevia NPCs with the same method as 5_Levels NPCs
            GameObject adoreviaGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "Adorevia" && go.scene.name != null);
            
            if (adoreviaGO != null)
            {
                Debug.Log($"[SaveManager] Found Adorevia GameObject");
                Transform canvas = adoreviaGO.transform.Find("Canvas");
                if (canvas != null)
                {
                    Debug.Log($"[SaveManager] Found Canvas");
                    Transform adoMainGame = canvas.Find("Ado_MainGame");
                    if (adoMainGame != null)
                    {
                        Debug.Log($"[SaveManager] Found Ado_MainGame");
                        Transform adoEvents = adoMainGame.Find("Ado_Events");
                        if (adoEvents != null)
                        {
                            Debug.Log($"[SaveManager] Found Ado_Events, collecting NPCs");
                            // Adorevia NPCs use Image components, not SpriteRenderers
                            // Collect all Image-based NPCs within event folders
                            CollectAdovereiaUIPhotoNpcs(adoEvents, ref npcCount);
                        }
                        else
                        {
                            Debug.LogWarning($"[SaveManager] Ado_Events not found under Ado_MainGame");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[SaveManager] Ado_MainGame not found under Canvas");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SaveManager] Canvas not found under Adorevia");
                }
            }
            else
            {
                Debug.LogWarning($"[SaveManager] Adorevia GameObject not found");
            }
            
            // Collect Milking NPCs early
            GameObject milkingGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "MilkingMinigame" && go.scene.name != null);
            
            if (milkingGO != null)
            {
                Debug.Log($"[SaveManager] Found MilkingMinigame GameObject");
                Transform list = milkingGO.transform.Find("List");
                if (list != null)
                {
                    // Iterate through all character folders (1_Anna, 4_Katarina, 5_Emma, 6_Amelia, 7_Phoenix)
                    for (int charIndex = 0; charIndex < list.childCount; charIndex++)
                    {
                        Transform charTransform = list.GetChild(charIndex);
                        string charName = charTransform.gameObject.name;
                        
                        // Skip empty/control folders
                        if (charName.StartsWith("X_") || charName == "0_Empty")
                        {
                            continue;
                        }
                        
                        // Look for Default folder which contains the actual sprite
                        Transform defaultFolder = charTransform.Find("Default");
                        if (defaultFolder != null)
                        {
                            // Look for DefaultFace which has the SpriteRenderer
                            Transform defaultFace = defaultFolder.Find("DefaultFace");
                            if (defaultFace != null)
                            {
                                SpriteRenderer sr = defaultFace.GetComponent<SpriteRenderer>();
                                if (sr != null && sr.sprite != null)
                                {
                                    string hierarchicalId = $"Milking_Minigame_{charName}";
                                    
                                    if (!npcHierarchy.ContainsKey(hierarchicalId))
                                    {
                                        // Add to defaultValues
                                        string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                        if (!defaultValues.ContainsKey(npcVarName))
                                        {
                                            defaultValues[npcVarName] = false;
                                        }
                                        
                                        // Store with same format as 5_Levels and Adorevia: (GO, directParent, levelsParent, null topLevelParent)
                                        npcHierarchy[hierarchicalId] = (defaultFace.gameObject, defaultFolder, charTransform, null);
                                        Debug.Log($"[SaveManager] Registered NPC: {hierarchicalId} (Active state - NPC: {defaultFace.gameObject.activeSelf}, DirectParent: {defaultFolder?.gameObject.activeSelf ?? false}, LevelsParent: {charTransform.gameObject.activeSelf})");
                                        npcCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[SaveManager] MilkingMinigame not found");
            }
            
            // Collect Tabletop NPCs early - they use Image components, not SpriteRenderer
            GameObject tabletopMinigameGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "Tabletop_Minigame" && go.scene.name != null);
            
            if (tabletopMinigameGO != null)
            {
                Debug.Log($"[SaveManager] Found Tabletop_Minigame GameObject");
                Transform tabletopCanvas = tabletopMinigameGO.transform.Find("Tabletop_Canvas");
                if (tabletopCanvas != null)
                {
                    // Collect Tabletop Ally Fighters
                    Transform allyFighters = tabletopCanvas.Find("Tabletop Ally Fighters");
                    if (allyFighters != null)
                    {
                        for (int fighterIndex = 0; fighterIndex < allyFighters.childCount; fighterIndex++)
                        {
                            Transform fighter = allyFighters.GetChild(fighterIndex);
                            string fighterName = fighter.gameObject.name;
                            
                            // Skip 0_empty
                            if (fighterName == "0_empty")
                            {
                                continue;
                            }
                            
                            // Look for Char_Sprite which has the Image component
                            Transform charSprite = fighter.Find("Char_Sprite");
                            if (charSprite != null)
                            {
                                UnityEngine.UI.Image imageComponent = charSprite.GetComponent<UnityEngine.UI.Image>();
                                if (imageComponent != null && imageComponent.sprite != null)
                                {
                                    string hierarchicalId = $"Tabletop_Minigame_Tabletop Ally Fighters_{fighterName}";
                                    
                                    if (!npcHierarchy.ContainsKey(hierarchicalId))
                                    {
                                        // Add to defaultValues
                                        string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                        if (!defaultValues.ContainsKey(npcVarName))
                                        {
                                            defaultValues[npcVarName] = false;
                                        }
                                        
                                        // Store with same format as 5_Levels, Adorevia, Milking: (GO, directParent, levelsParent, null topLevelParent)
                                        npcHierarchy[hierarchicalId] = (charSprite.gameObject, fighter, allyFighters, null);
                                        Debug.Log($"[SaveManager] Registered NPC: {hierarchicalId} (Active state - NPC: {charSprite.gameObject.activeSelf}, DirectParent: {fighter.gameObject.activeSelf}, AllyFighters: {allyFighters.gameObject.activeSelf})");
                                        npcCount++;
                                    }
                                }
                            }
                        }
                    }
                    
                    // Collect Tabletop Enemy Fighters
                    Transform enemyFighters = tabletopCanvas.Find("Tabletop Enemy Fighters");
                    if (enemyFighters != null)
                    {
                        for (int fighterIndex = 0; fighterIndex < enemyFighters.childCount; fighterIndex++)
                        {
                            Transform fighter = enemyFighters.GetChild(fighterIndex);
                            string fighterName = fighter.gameObject.name;
                            
                            // Skip 0_empty
                            if (fighterName == "0_empty")
                            {
                                continue;
                            }
                            
                            // Look for Char_Sprite which has the Image component
                            Transform charSprite = fighter.Find("Char_Sprite");
                            if (charSprite != null)
                            {
                                UnityEngine.UI.Image imageComponent = charSprite.GetComponent<UnityEngine.UI.Image>();
                                if (imageComponent != null && imageComponent.sprite != null)
                                {
                                    string hierarchicalId = $"Tabletop_Minigame_Tabletop Enemy Fighters_{fighterName}";
                                    
                                    if (!npcHierarchy.ContainsKey(hierarchicalId))
                                    {
                                        // Add to defaultValues
                                        string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                        if (!defaultValues.ContainsKey(npcVarName))
                                        {
                                            defaultValues[npcVarName] = false;
                                        }
                                        
                                        // Store with same format as 5_Levels, Adorevia, Milking: (GO, directParent, levelsParent, null topLevelParent)
                                        npcHierarchy[hierarchicalId] = (charSprite.gameObject, fighter, enemyFighters, null);
                                        Debug.Log($"[SaveManager] Registered NPC: {hierarchicalId} (Active state - NPC: {charSprite.gameObject.activeSelf}, DirectParent: {fighter.gameObject.activeSelf}, EnemyFighters: {enemyFighters.gameObject.activeSelf})");
                                        npcCount++;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[SaveManager] Tabletop_Canvas not found under Tabletop_Minigame");
                }
            }
            else
            {
                Debug.LogWarning($"[SaveManager] Tabletop_Minigame not found");
            }
            
            // Collect EventUI Photo NPCs early - they use Image components
            GameObject eventUIGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "15_EventUI" && go.scene.name != null);
            
            if (eventUIGO != null)
            {
                Debug.Log($"[SaveManager] Found 15_EventUI GameObject");
                Transform bpcNew = eventUIGO.transform.Find("BPC_New");
                if (bpcNew != null)
                {
                    // Collect Adrian photos
                    Transform adrianPC = bpcNew.Find("AdrianPC_Wor");
                    if (adrianPC != null)
                    {
                        Transform adrianGO = adrianPC.Find("GameObject");
                        if (adrianGO != null)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                Transform imageTransform;
                                if (i == 0)
                                {
                                    imageTransform = adrianGO.Find("Image");
                                }
                                else
                                {
                                    imageTransform = adrianGO.Find($"Image ({i})");
                                }
                                
                                if (imageTransform != null)
                                {
                                    UnityEngine.UI.Image imageComponent = imageTransform.GetComponent<UnityEngine.UI.Image>();
                                    if (imageComponent != null && imageComponent.sprite != null)
                                    {
                                        string hierarchicalId = $"EventUI_Adrian_Image ({i})";
                                        
                                        if (!npcHierarchy.ContainsKey(hierarchicalId))
                                        {
                                            // Add to defaultValues
                                            string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                            if (!defaultValues.ContainsKey(npcVarName))
                                            {
                                                defaultValues[npcVarName] = false;
                                            }
                                            
                                            // Store with same format as other minigames: (GO, directParent, levelsParent, null topLevelParent)
                                            npcHierarchy[hierarchicalId] = (imageTransform.gameObject, adrianGO, adrianPC, null);
                                            Debug.Log($"[SaveManager] Registered NPC: {hierarchicalId} (Active state - NPC: {imageTransform.gameObject.activeSelf}, DirectParent: {adrianGO.gameObject.activeSelf}, AdrianPC: {adrianPC.gameObject.activeSelf})");
                                            npcCount++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    // Collect Other character photos - only GameObject (4)
                    Transform otherPC = bpcNew.Find("OtherPC_Browser");
                    if (otherPC != null)
                    {
                        Transform otherCore = otherPC.Find("OtherPC_Core");
                        if (otherCore != null)
                        {
                            // Only collect GameObject (4)
                            Transform gameObjTransform = otherCore.Find("GameObject (4)");
                            if (gameObjTransform != null)
                            {
                                UnityEngine.UI.Image imageComponent = gameObjTransform.GetComponent<UnityEngine.UI.Image>();
                                if (imageComponent != null && imageComponent.sprite != null)
                                {
                                    string hierarchicalId = $"EventUI_Other_GameObject (4)";
                                    
                                    if (!npcHierarchy.ContainsKey(hierarchicalId))
                                    {
                                        // Add to defaultValues
                                        string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                        if (!defaultValues.ContainsKey(npcVarName))
                                        {
                                            defaultValues[npcVarName] = false;
                                        }
                                        
                                        // Store with same format: (GO, directParent, levelsParent, null topLevelParent)
                                        npcHierarchy[hierarchicalId] = (gameObjTransform.gameObject, otherCore, otherPC, null);
                                        Debug.Log($"[SaveManager] Registered NPC: {hierarchicalId} (Active state - NPC: {gameObjTransform.gameObject.activeSelf}, OtherCore: {otherCore.gameObject.activeSelf}, OtherPC: {otherPC.gameObject.activeSelf})");
                                        npcCount++;
                                    }
                                }
                            }
                        }
                    }

                }
            }
            else
            {
                Debug.LogWarning($"[SaveManager] 15_EventUI not found");
            }
            
            // Collect Livestream NPCs early - they use SpriteRenderer components
            GameObject livestreamActiveGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "Livestream_Active" && go.scene.name != null);
            
            if (livestreamActiveGO != null)
            {
                Debug.Log($"[SaveManager] Found Livestream_Active GameObject");
                Transform liveAnna = livestreamActiveGO.transform.Find("Live_Anna");
                if (liveAnna != null)
                {
                    // Collect from all three outfits: DefaultOutfit, CoolOutfit, SlutOutfit
                    string[] outfitNames = { "Live_DefaultOutfit", "Live_CoolOutfit", "Live_SlutOutfit" };
                    
                    foreach (string outfitName in outfitNames)
                    {
                        Transform outfitTransform = liveAnna.Find(outfitName);
                        if (outfitTransform != null)
                        {
                            for (int npcIndex = 0; npcIndex < outfitTransform.childCount; npcIndex++)
                            {
                                Transform npcTransform = outfitTransform.GetChild(npcIndex);
                                string npcName = npcTransform.gameObject.name;
                                
                                // Look for SpriteRenderer component
                                SpriteRenderer sr = npcTransform.GetComponent<SpriteRenderer>();
                                if (sr != null && sr.sprite != null)
                                {
                                    // Extract outfit name from folder (Live_DefaultOutfit -> DefaultOutfit)
                                    string outfitNameShort = outfitName.Substring("Live_".Length);
                                    string hierarchicalId = $"Livestream_{outfitNameShort}_{npcName}";
                                    
                                    if (!npcHierarchy.ContainsKey(hierarchicalId))
                                    {
                                        // Add to defaultValues
                                        string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                        if (!defaultValues.ContainsKey(npcVarName))
                                        {
                                            defaultValues[npcVarName] = false;
                                        }
                                        
                                        // Store with same format as other minigames: (GO, directParent, levelsParent, topLevelParent)
                                        npcHierarchy[hierarchicalId] = (npcTransform.gameObject, outfitTransform, liveAnna, livestreamActiveGO.transform);
                                        Debug.Log($"[SaveManager] Registered NPC: {hierarchicalId} (Active state - NPC: {npcTransform.gameObject.activeSelf}, Outfit: {outfitTransform.gameObject.activeSelf}, LiveAnna: {liveAnna.gameObject.activeSelf}, LivestreamActive: {livestreamActiveGO.activeSelf})");
                                        npcCount++;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[SaveManager] Live_Anna not found under Livestream_Active");
                }
            }
            else
            {
                Debug.LogWarning($"[SaveManager] Livestream_Active not found");
            }
            
            // Collect Bafa NPCs early - they use SpriteRenderer components in Bafa_Sprites container
            GameObject bafaSpritesGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "Bafa_Sprites" && go.scene.name != null);
            
            if (bafaSpritesGO != null)
            {
                Debug.Log($"[SaveManager] Found Bafa_Sprites GameObject");
                
                // Iterate through all Bafa NPC folders
                for (int npcIndex = 0; npcIndex < bafaSpritesGO.transform.childCount; npcIndex++)
                {
                    Transform npcTransform = bafaSpritesGO.transform.GetChild(npcIndex);
                    string npcName = npcTransform.gameObject.name;
                    
                    // Skip 0_Empty
                    if (npcName == "0_Empty")
                    {
                        continue;
                    }
                    
                    // Look for SpriteRenderer component on the NPC itself
                    SpriteRenderer sr = npcTransform.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.sprite != null)
                    {
                        string hierarchicalId = $"Bafa_{npcName}";
                        
                        if (!npcHierarchy.ContainsKey(hierarchicalId))
                        {
                            // Add to defaultValues
                            string npcVarName = $"NPC_Seen_{hierarchicalId}";
                            if (!defaultValues.ContainsKey(npcVarName))
                            {
                                defaultValues[npcVarName] = false;
                            }
                            
                            // Store with same format as other minigames: (GO, directParent, levelsParent, topLevelParent)
                            npcHierarchy[hierarchicalId] = (npcTransform.gameObject, bafaSpritesGO.transform, bafaSpritesGO.transform, null);
                            Debug.Log($"[SaveManager] Registered NPC: {hierarchicalId} (Active state - NPC: {npcTransform.gameObject.activeSelf}, BafaSprites: {bafaSpritesGO.activeSelf})");
                            npcCount++;
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[SaveManager] Bafa_Sprites not found");
            }
            
            // Collect special case NPCs from 5_Levels early
            // These are NPCs that don't follow the normal discovery pattern and must be found by explicit path
            GameObject levelsGO2 = GameObject.Find("5_Levels");
            if (levelsGO2 != null)
            {
                // Special case NPCs paths organized by level
                var specialCasesByLevel = new Dictionary<string, List<string[]>>()
                {
                    { "4_Bath", new List<string[]> { new[] { "AnnaInShower" }, new[] { "AnnaInBathtubSprite" }, new[] { "BathB", "Square" }, new[] { "BathB", "Square (1)" } } },
                    { "17_CarPark", new List<string[]> { new[] { "ToniWorking1" } } },
                    { "28_Upstairs", new List<string[]> { new[] { "AdrianAtDoorSprite" } } },
                    { "33_Club", new List<string[]> { new[] { "NPC (1)" }, new[] { "NPC (2)" }, new[] { "NPC (3)" }, new[] { "NPC (4)" }, new[] { "NPC (5)" } } },
                    { "76_Villa_Lab", new List<string[]> { new[] { "Androidfloat" } } },
                    { "80_Shrine", new List<string[]> { new[] { "Nyx25_PhoneSprite1" } } },
                    { "82_Abandonedshackinside", new List<string[]> { new[] { "SofiaRestrained" } } },
                    { "85_HospitalHallway", new List<string[]> { new[] { "hospital1" }, new[] { "hospital1 (1)" }, new[] { "Hospital2" } } },
                    { "90_JapaneseOldStreet", new List<string[]> { new[] { "NPC (8)" }, new[] { "NPC (9)" } } },
                    { "92_JapanesePlaza", new List<string[]> { new[] { "NPC (3)" }, new[] { "NPC (4)" }, new[] { "NPC (5)" }, new[] { "NPC (6)" } } },
                    { "95_JapaneseSubwaytrain", new List<string[]> { new[] { "NPC (6)" }, new[] { "NPC (8)" }, new[] { "NPC (9)" } } },
                    { "100_TropicalPool", new List<string[]> { new[] { "NPC" }, new[] { "NPC (1)" }, new[] { "NPC (2)" }, new[] { "NPC (3)" } } },
                    { "101_TropicalClub", new List<string[]> { new[] { "NPC" }, new[] { "NPC (1)" }, new[] { "NPC (2)" }, new[] { "NPC (3)" } } },
                    { "103_WinterVillageOutside", new List<string[]> { new[] { "NPC (1)" }, new[] { "NPC (2)" } } },
                    { "104_WinterHotspring", new List<string[]> { new[] { "NPC (2)" }, new[] { "NPC (3)" }, new[] { "NPC (4)" }, new[] { "NPC (5)" }, new[] { "NPC (6)" }, new[] { "AnnaAndPlayer", "NPC (7)" }, new[] { "AnnaAndPlayer", "NPC (8)" } } },
                    { "106_BeerTent", new List<string[]> { new[] { "NPC (2)" }, new[] { "NPC (3)" }, new[] { "NPC (4)" } } },
                    { "110_BadlandsParkingLot", new List<string[]> { new[] { "ParkingLotNPCs", "NPC (8)" }, new[] { "ParkingLotNPCs", "NPC (9)" }, new[] { "ParkingLotNPCs", "NPC (10)" }, new[] { "ParkingLotNPCs", "NPC (11)" }, new[] { "ParkingLotNPCs", "NPC (12)" }, new[] { "ParkingLotNPCs", "NPC (13)" }, new[] { "ParkingLotNPCs", "NPC (14)" } } },
                    { "112_GasStationInterior", new List<string[]> { new[] { "VampireReachesUp" } } },
                    { "119_AirDuct", new List<string[]> { new[] { "CharlotteVent1" } } },
                    { "121_DiningRoomHome", new List<string[]> { new[] { "Dining_Sprites", "Dining_AnnaSitSprite" }, new[] { "Dining_Sprites", "Dining_AnnaSitSprite_Nude" }, new[] { "Dining_Sprites", "Dining_AnnaSitSprite_NudeWithCum" }, new[] { "Dining_Sprites", "Dining_JosefSitSprite" }, new[] { "Dining_Sprites", "Dining_AdrianSitSprite" }, new[] { "Dining_Sprites", "Dining_AdrianSitSprite_NoPhone" } } },
                    { "123_CharlotteHalloweenHome", new List<string[]> { new[] { "HalloweenPartyGuest1" }, new[] { "HalloweenPartyGuest1 (1)" }, new[] { "HalloweenPartyGuest1 (2)" } } },
                    { "133_HarborDistrict", new List<string[]> { new[] { "137HarborDistrict_O1", "Harbor_PoBa_01" }, new[] { "137HarborDistrict_O1", "Harbor_PoBa_01 (1)" }, new[] { "137HarborDistrict_O1", "Harbor_PoBa_01 (2)" } } },
                    { "136_HarborScifiPrison", new List<string[]> { new[] { "MaleDocMura_Sitting" } } },
                    { "137_HarborHauntedHouse", new List<string[]> { new[] { "Harbor_PoBa_01" }, new[] { "Harbor_PoBa_01 (1)" }, new[] { "Harbor_PoBa_01 (2)" } } },
                    { "142_NightDistrict", new List<string[]> { new[] { "Nightclub_Sign_2" }, new[] { "Nightclub_Sign_2 (1)" }, new[] { "Nightclub_Sign_2 (2)" } } }
                };
                
                // Iterate through all level parents to find special case NPCs
                for (int i = 0; i < levelsGO2.transform.childCount; i++)
                {
                    Transform levelParent = levelsGO2.transform.GetChild(i);
                    string levelName = levelParent.gameObject.name;
                    
                    // Check if this level has special cases
                    if (!specialCasesByLevel.ContainsKey(levelName))
                    {
                        continue;
                    }
                    
                    // Process each special case path for this level
                    foreach (string[] pathArray in specialCasesByLevel[levelName])
                    {
                        Transform currentTransform = levelParent;
                        
                        // Navigate through the path
                        for (int j = 0; j < pathArray.Length; j++)
                        {
                            string pathSegment = pathArray[j];
                            currentTransform = currentTransform.Find(pathSegment);
                            
                            if (currentTransform == null)
                            {
                                Debug.LogWarning($"[SaveManager] Special case NPC not found in {levelName}: {string.Join(" > ", pathArray)}");
                                break;
                            }
                        }
                        
                        if (currentTransform != null)
                        {
                            SpriteRenderer spriteRenderer = currentTransform.GetComponent<SpriteRenderer>();
                            if (spriteRenderer != null && spriteRenderer.sprite != null)
                            {
                                // Create hierarchical ID
                                string npcName = pathArray[pathArray.Length - 1];
                                string directParentName = currentTransform.parent != null ? currentTransform.parent.gameObject.name : "Unknown";
                                string hierarchicalId = $"{levelName}_{directParentName}_{npcName}";
                                
                                if (!npcHierarchy.ContainsKey(hierarchicalId))
                                {
                                    // Add to defaultValues
                                    string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                    if (!defaultValues.ContainsKey(npcVarName))
                                    {
                                        defaultValues[npcVarName] = false;
                                    }
                                    
                                    // Store hierarchy: (npcGO, directParent, levelParent, null topLevelParent)
                                    Transform directParent = currentTransform.parent;
                                    npcHierarchy[hierarchicalId] = (currentTransform.gameObject, directParent, levelParent, null);
                                    Debug.Log($"[SaveManager] Registered special case NPC: {hierarchicalId} (Active state - NPC: {currentTransform.gameObject.activeSelf}, DirectParent: {directParent?.gameObject.activeSelf ?? false}, LevelParent: {levelParent.gameObject.activeSelf})");
                                    npcCount++;
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"[SaveManager] Special case NPC found but has no SpriteRenderer in {levelName}: {string.Join(" > ", pathArray)}");
                            }
                        }
                    }
                }
            }
            
            // Collect X_Movies NPCs early - they use SpriteRenderer components
            GameObject xMoviesGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "X_Movies" && go.scene.name != null);
            
            if (xMoviesGO != null)
            {
                Debug.Log($"[SaveManager] Found X_Movies GameObject");
                
                // Define X_Movies NPCs by path
                var xMoviesNpcs = new List<(string movieType, string slidePath, string npcName)>
                {
                    // Scifi_Movie
                    ("Scifi_Movie", "Slide_1", "M_scifi01"),
                    ("Scifi_Movie", "Slide_1", "M_scifi01 (1)"),
                    ("Scifi_Movie", "Slide_2", "M_scifi05"),
                    ("Scifi_Movie", "Slide_2", "M_scifi05 (1)"),
                    ("Scifi_Movie", "Slide_2", "M_scifi05 (2)"),
                    ("Scifi_Movie", "Slide_2", "M_scifi05 (3)"),
                    ("Scifi_Movie", "Slide_2", "M_scifi05 (4)"),
                    ("Scifi_Movie", "Slide_2", "M_scifi05 (5)"),
                    
                    // News_Movie
                    ("News_Movie", "News_Screen", "News_0"),
                    ("News_Movie", "News_Screen", "News_1"),
                    ("News_Movie", "News_Screen", "News_2"),
                    ("News_Movie", "News_Screen", "News_3"),
                    ("News_Movie", "News_Screen", "News_4"),
                    ("News_Movie", "News_Screen", "News_5"),
                    ("News_Movie", "News_Screen", "News_7"),
                    ("News_Movie", "News_Screen", "News_8"),
                    
                    // Fantasy_Movie
                    ("Fantasy_Movie", "Slide1", "M_Fantasy3"),
                    ("Fantasy_Movie", "Slide2", "M_Fantasy3"),
                    ("Fantasy_Movie", "Slide3", "M_Fantasy3"),
                    
                    // Adult_Movie
                    ("Adult_Movie", "Slide1", "M_Fantasy3"),
                    ("Adult_Movie", "Slide2", "M_Fantasy3"),
                    ("Adult_Movie", "Slide3", "M_Fantasy3")
                };
                
                // Collect all X_Movies NPCs
                foreach (var (movieType, slidePath, npcName) in xMoviesNpcs)
                {
                    Transform movieTypeTransform = xMoviesGO.transform.Find(movieType);
                    if (movieTypeTransform != null)
                    {
                        Transform slideTransform = movieTypeTransform.Find(slidePath);
                        if (slideTransform != null)
                        {
                            Transform npcTransform = slideTransform.Find(npcName);
                            if (npcTransform != null)
                            {
                                SpriteRenderer sr = npcTransform.GetComponent<SpriteRenderer>();
                                if (sr != null && sr.sprite != null)
                                {
                                    string hierarchicalId = $"X_Movies_{movieType}_{slidePath}_{npcName}";
                                    
                                    if (!npcHierarchy.ContainsKey(hierarchicalId))
                                    {
                                        // Add to defaultValues
                                        string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                        if (!defaultValues.ContainsKey(npcVarName))
                                        {
                                            defaultValues[npcVarName] = false;
                                        }
                                        
                                        // Store hierarchy: (npcGO, directParent, movieType, xMovies)
                                        npcHierarchy[hierarchicalId] = (npcTransform.gameObject, slideTransform, movieTypeTransform, xMoviesGO.transform);
                                        //Debug.Log($"[SaveManager] Registered X_Movies NPC: {hierarchicalId}");
                                        npcCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[SaveManager] X_Movies not found");
            }
            
            // Collect Starmaker Creator NPCs early - they use Image components from 9_MainCanvas
            npcCount += CollectStarmakerCreatorNpcs();
            
            // Also collect remaining minigame NPCs early so they're available for monitoring even before gallery is opened
            npcCount += CollectMinigameNpcs();
            
            Debug.Log($"[SaveManager] Initialized NPC tracking for {npcCount} NPCs");
        }

        private int CollectStarmakerCreatorNpcs()
        {
            int count = 0;
            GameObject mainCanvasGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "9_MainCanvas" && go.scene.name != null);
            
            if (mainCanvasGO != null)
            {
                Transform starmaker = mainCanvasGO.transform.Find("Starmaker");
                if (starmaker != null)
                {
                    Transform browseCreators = starmaker.Find("Browse_Creators");
                    if (browseCreators != null)
                    {
                        Transform creatorList = browseCreators.Find("CreatorList");
                        if (creatorList != null)
                        {
                            // Define creators to collect
                            var creatorsToCollect = new (string creatorFolder, string npcId)[]
                            {
                                ("Creator_MsUmbra", "9_MainCanvas_Starmaker_MsUmbra"),
                                ("Creator_DreamyWisp", "9_MainCanvas_Starmaker_DreamyWisp"),
                                ("Creator_Rose", "9_MainCanvas_Starmaker_Rose")
                            };
                            
                            foreach (var (creatorFolder, npcId) in creatorsToCollect)
                            {
                                Transform creatorTransform = creatorList.Find(creatorFolder);
                                if (creatorTransform != null)
                                {
                                    Transform imageTransform = creatorTransform.Find("Image");
                                    if (imageTransform != null)
                                    {
                                        UnityEngine.UI.Image imageComponent = imageTransform.GetComponent<UnityEngine.UI.Image>();
                                        if (imageComponent != null && imageComponent.sprite != null)
                                        {
                                            if (!npcHierarchy.ContainsKey(npcId))
                                            {
                                                // Add to defaultValues
                                                string npcVarName = $"NPC_Seen_{npcId}";
                                                if (!defaultValues.ContainsKey(npcVarName))
                                                {
                                                    defaultValues[npcVarName] = false;
                                                }
                                                
                                                // Store hierarchy: (npcGO, directParent, creatorList, mainCanvas)
                                                npcHierarchy[npcId] = (imageTransform.gameObject, creatorTransform, creatorList, mainCanvasGO.transform);
                                                //Debug.Log($"[SaveManager] Registered Starmaker Creator NPC: {npcId} (Active state - NPC: {imageTransform.gameObject.activeSelf}, Creator: {creatorTransform.gameObject.activeSelf}, CreatorList: {creatorList.gameObject.activeSelf}, MainCanvas: {mainCanvasGO.activeSelf})");
                                                count++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[SaveManager] 9_MainCanvas not found");
            }
            
            return count;
        }

        /// <summary>
        /// Collect all minigame NPCs (Sword, Milking, Tabletop, Barista, Racing, EventUI)
        /// </summary>
        private int CollectMinigameNpcs()
        {
            int npcCount = 0;
            
            // Collect Sword NPCs
            npcCount += CollectSwordNpcs();
            
            // NOTE: Milking NPCs are now collected inline in InitializeNpcTracking
            // npcCount += CollectMilkingNpcs();
            
            // NOTE: Tabletop NPCs are now collected inline in InitializeNpcTracking
            // npcCount += CollectTabletopNpcs();
            
            // Collect Barista NPCs
            npcCount += CollectBaristaNpcs();
            
            // Collect Racing NPCs
            npcCount += CollectRacingNpcs();
            
            // Collect Sprite_Overlay NPCs
            npcCount += CollectSpriteOverlayNpcs();
            
            // Collect Ghost_Minigame NPCs
            npcCount += CollectGhostMinigameNpcs();
            
            // Collect Camcorder_Minigame NPCs
            npcCount += CollectCamcorderMinigameNpcs();
            
            // NOTE: EventUI NPCs are now collected inline in InitializeNpcTracking
            // npcCount += CollectEventUINpcs();
            
            return npcCount;
        }

        private int CollectSwordNpcs()
        {
            int count = 0;
            GameObject swordMinigame = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "Sword_Minigame" && go.scene.name != null);
            
            if (swordMinigame != null)
            {
                Transform enemy = swordMinigame.transform.Find("Enemy");
                if (enemy != null)
                {
                    Transform sprites = enemy.Find("Sprites");
                    if (sprites != null)
                    {
                        for (int i = 0; i < sprites.childCount; i++)
                        {
                            Transform npcTransform = sprites.GetChild(i);
                            string hierarchicalId = $"Sword_Minigame_Sword Enemy_{npcTransform.gameObject.name}";
                            
                            SpriteRenderer sr = npcTransform.GetComponent<SpriteRenderer>();
                            if (sr != null && sr.sprite != null && !npcHierarchy.ContainsKey(hierarchicalId))
                            {
                                string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                if (!defaultValues.ContainsKey(npcVarName))
                                {
                                    defaultValues[npcVarName] = false;
                                }
                                npcHierarchy[hierarchicalId] = (npcTransform.gameObject, sprites, enemy, swordMinigame.transform);
                                count++;
                            }
                        }
                    }
                }
            }
            return count;
        }

        private int CollectMilkingNpcs()
        {
            int count = 0;
            GameObject milkingGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "MilkingMinigame" && go.scene.name != null);
            
            if (milkingGO != null)
            {
                Transform list = milkingGO.transform.Find("List");
                if (list != null)
                {
                    // Iterate through all character folders (1_Anna, 4_Katarina, 5_Emma, 6_Amelia, 7_Phoenix)
                    for (int charIndex = 0; charIndex < list.childCount; charIndex++)
                    {
                        Transform charTransform = list.GetChild(charIndex);
                        string charName = charTransform.gameObject.name;
                        
                        // Skip empty/control folders - look for character folders with Actions component
                        if (charName.StartsWith("X_") || charName == "0_Empty")
                        {
                            continue;
                        }
                        
                        // Look for Default folder which contains the actual sprite
                        Transform defaultFolder = charTransform.Find("Default");
                        if (defaultFolder != null)
                        {
                            // Look for DefaultFace which has the SpriteRenderer
                            Transform defaultFace = defaultFolder.Find("DefaultFace");
                            if (defaultFace != null)
                            {
                                SpriteRenderer sr = defaultFace.GetComponent<SpriteRenderer>();
                                if (sr != null && sr.sprite != null)
                                {
                                    string hierarchicalId = $"Milking_Minigame_{charName}";
                                    
                                    if (!npcHierarchy.ContainsKey(hierarchicalId))
                                    {
                                        // Add to defaultValues
                                        string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                        if (!defaultValues.ContainsKey(npcVarName))
                                        {
                                            defaultValues[npcVarName] = false;
                                        }
                                        
                                        // Store with same format as 5_Levels and Adorevia: (GO, directParent, levelsParent, null topLevelParent)
                                        npcHierarchy[hierarchicalId] = (defaultFace.gameObject, defaultFolder, charTransform, null);
                                        Debug.Log($"[SaveManager] Registered NPC: {hierarchicalId} (Active state - NPC: {defaultFace.gameObject.activeSelf}, DirectParent: {defaultFolder?.gameObject.activeSelf ?? false}, LevelsParent: {charTransform.gameObject.activeSelf})");
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return count;
        }

        private int CollectTabletopNpcs()
        {
            int count = 0;
            GameObject tabletopMinigameGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "Tabletop_Minigame" && go.scene.name != null);
            
            if (tabletopMinigameGO != null)
            {
                Transform tabletopCanvas = tabletopMinigameGO.transform.Find("Tabletop_Canvas");
                if (tabletopCanvas != null)
                {
                    // Register Tabletop Ally Fighters
                    Transform allyFighters = tabletopCanvas.Find("Tabletop Ally Fighters");
                    if (allyFighters != null)
                    {
                        foreach (Transform fighter in allyFighters)
                        {
                            if (fighter.gameObject.activeSelf)
                            {
                                string hierarchicalId = $"Tabletop_Minigame_Tabletop Ally Fighters_{fighter.gameObject.name}";
                                if (!npcHierarchy.ContainsKey(hierarchicalId))
                                {
                                    string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                    if (!defaultValues.ContainsKey(npcVarName))
                                    {
                                        defaultValues[npcVarName] = false;
                                    }
                                    npcHierarchy[hierarchicalId] = (fighter.gameObject, allyFighters, tabletopCanvas, tabletopMinigameGO.transform);
                                    count++;
                                }
                            }
                        }
                    }
                    
                    // Register Tabletop Enemy Fighters
                    Transform enemyFighters = tabletopCanvas.Find("Tabletop Enemy Fighters");
                    if (enemyFighters != null)
                    {
                        foreach (Transform fighter in enemyFighters)
                        {
                            if (fighter.gameObject.activeSelf)
                            {
                                string hierarchicalId = $"Tabletop_Minigame_Tabletop Enemy Fighters_{fighter.gameObject.name}";
                                if (!npcHierarchy.ContainsKey(hierarchicalId))
                                {
                                    string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                    if (!defaultValues.ContainsKey(npcVarName))
                                    {
                                        defaultValues[npcVarName] = false;
                                    }
                                    npcHierarchy[hierarchicalId] = (fighter.gameObject, enemyFighters, tabletopCanvas, tabletopMinigameGO.transform);
                                    count++;
                                }
                            }
                        }
                    }
                }
            }
            return count;
        }

        private int CollectBaristaNpcs()
        {
            int count = 0;
            GameObject baristaGameGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "Barista_Game" && go.scene.name != null);
            
            if (baristaGameGO != null)
            {
                Transform canvas = baristaGameGO.transform.Find("Canvas");
                if (canvas != null)
                {
                    // Collect regular Barista NPCs
                    Transform continueGO = canvas.Find("Continue");
                    if (continueGO != null)
                    {
                        Transform buyMenu = continueGO.Find("BuyMenu");
                        if (buyMenu != null)
                        {
                            foreach (Transform child in buyMenu)
                            {
                                if (child.gameObject.name != "0_empty" && child.gameObject.activeSelf)
                                {
                                    string hierarchicalId = $"Barista_Game_{child.gameObject.name}";
                                    if (!npcHierarchy.ContainsKey(hierarchicalId))
                                    {
                                        string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                        if (!defaultValues.ContainsKey(npcVarName))
                                        {
                                            defaultValues[npcVarName] = false;
                                        }
                                        npcHierarchy[hierarchicalId] = (child.gameObject, buyMenu, continueGO, baristaGameGO.transform);
                                        
                                        // Store unlock condition
                                        string unlockCondition = GetBaristaUnlockCondition(child.gameObject.name);
                                        baristaUnlockConditions[hierarchicalId] = unlockCondition;
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                    
                    // Collect FreeGob2 special NPC
                    Transform freePerk = canvas.Find("FreePerk");
                    if (freePerk != null)
                    {
                        Transform cardsLeft = freePerk.Find("CardsLeft");
                        if (cardsLeft != null)
                        {
                            Transform freeGob2 = cardsLeft.Find("FreeGob2 (4)");
                            if (freeGob2 != null)
                            {
                                Transform card = freeGob2.Find("Card");
                                if (card != null)
                                {
                                    Transform content = card.Find("Content");
                                    if (content != null)
                                    {
                                        string hierarchicalId = $"Barista_Game_FreeGob2";
                                        if (!npcHierarchy.ContainsKey(hierarchicalId))
                                        {
                                            string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                            if (!defaultValues.ContainsKey(npcVarName))
                                            {
                                                defaultValues[npcVarName] = false;
                                            }
                                            npcHierarchy[hierarchicalId] = (content.gameObject, null, null, baristaGameGO.transform);
                                            baristaUnlockConditions[hierarchicalId] = null;
                                            count++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return count;
        }

        private int CollectRacingNpcs()
        {
            int count = 0;
            GameObject racingGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "Racing_Minigame" && go.scene.name != null);
            
            if (racingGO != null)
            {
                Transform racingPC = racingGO.transform.Find("Racing_PC");
                if (racingPC != null)
                {
                    Transform racingBody = racingPC.Find("RacingBody");
                    if (racingBody != null)
                    {
                        // Racing Ally
                        Transform racingAlly = racingBody.Find("Racing_Ally");
                        if (racingAlly != null && racingAlly.childCount > 0)
                        {
                            Transform pcRacing1 = racingAlly.GetChild(0);
                            string hierarchicalId = $"Racing_Minigame_Racing Ally_{pcRacing1.gameObject.name}";
                            if (!npcHierarchy.ContainsKey(hierarchicalId))
                            {
                                string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                if (!defaultValues.ContainsKey(npcVarName))
                                {
                                    defaultValues[npcVarName] = false;
                                }
                                npcHierarchy[hierarchicalId] = (pcRacing1.gameObject, racingAlly, racingBody, racingGO.transform);
                                count++;
                            }
                        }
                        
                        // Racing Guests
                        Transform guestGroup = racingBody.Find("Racing_Guest");
                        if (guestGroup != null)
                        {
                            foreach (Transform child in guestGroup)
                            {
                                string hierarchicalId = $"Racing_Minigame_Racing Guest_{child.gameObject.name}";
                                if (!npcHierarchy.ContainsKey(hierarchicalId))
                                {
                                    string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                    if (!defaultValues.ContainsKey(npcVarName))
                                    {
                                        defaultValues[npcVarName] = false;
                                    }
                                    npcHierarchy[hierarchicalId] = (child.gameObject, guestGroup, racingBody, racingGO.transform);
                                    count++;
                                }
                            }
                        }
                    }
                }
            }
            return count;
        }

        private int CollectSpriteOverlayNpcs()
        {
            int count = 0;
            GameObject spriteOverlayGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "Sprite_Overlay" && go.scene.name != null);
            
            if (spriteOverlayGO != null)
            {
                // Collect Weightlifting NPCs
                Transform weightliftingFolder = spriteOverlayGO.transform.Find("Weightlifting");
                if (weightliftingFolder != null)
                {
                    // Weightlifting2
                    Transform weightlifting2 = weightliftingFolder.Find("Weightlifting2");
                    if (weightlifting2 != null)
                    {
                        SpriteRenderer sr = weightlifting2.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            string hierarchicalId = $"Sprite_Overlay_Weightlifting/Weightlifting2";
                            if (!npcHierarchy.ContainsKey(hierarchicalId))
                            {
                                string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                if (!defaultValues.ContainsKey(npcVarName))
                                {
                                    defaultValues[npcVarName] = false;
                                }
                                npcHierarchy[hierarchicalId] = (weightlifting2.gameObject, weightliftingFolder, spriteOverlayGO.transform, spriteOverlayGO.transform);
                                count++;
                            }
                        }
                    }
                    
                    // Weightlifting2 (1)
                    Transform weightlifting2Alt = weightliftingFolder.Find("Weightlifting2 (1)");
                    if (weightlifting2Alt != null)
                    {
                        SpriteRenderer sr = weightlifting2Alt.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            string hierarchicalId = $"Sprite_Overlay_Weightlifting/Weightlifting2 (1)";
                            if (!npcHierarchy.ContainsKey(hierarchicalId))
                            {
                                string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                if (!defaultValues.ContainsKey(npcVarName))
                                {
                                    defaultValues[npcVarName] = false;
                                }
                                npcHierarchy[hierarchicalId] = (weightlifting2Alt.gameObject, weightliftingFolder, spriteOverlayGO.transform, spriteOverlayGO.transform);
                                count++;
                            }
                        }
                    }
                }
                
                // Collect Sam Gaming NPCs
                Transform samGamingFolder = spriteOverlayGO.transform.Find("Sam_Gamingnight_Loading");
                if (samGamingFolder != null)
                {
                    // GamingCSMage01
                    Transform gamingCSMage01 = samGamingFolder.Find("GamingCSMage01");
                    if (gamingCSMage01 != null)
                    {
                        SpriteRenderer sr = gamingCSMage01.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            string hierarchicalId = $"Sprite_Overlay_Sam_Gaming/GamingCSMage01";
                            if (!npcHierarchy.ContainsKey(hierarchicalId))
                            {
                                string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                if (!defaultValues.ContainsKey(npcVarName))
                                {
                                    defaultValues[npcVarName] = false;
                                }
                                npcHierarchy[hierarchicalId] = (gamingCSMage01.gameObject, samGamingFolder, spriteOverlayGO.transform, spriteOverlayGO.transform);
                                count++;
                            }
                        }
                    }
                    
                    // GamingCSMage01 (1)
                    Transform gamingCSMage01Alt = samGamingFolder.Find("GamingCSMage01 (1)");
                    if (gamingCSMage01Alt != null)
                    {
                        SpriteRenderer sr = gamingCSMage01Alt.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            string hierarchicalId = $"Sprite_Overlay_Sam_Gaming/GamingCSMage01 (1)";
                            if (!npcHierarchy.ContainsKey(hierarchicalId))
                            {
                                string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                if (!defaultValues.ContainsKey(npcVarName))
                                {
                                    defaultValues[npcVarName] = false;
                                }
                                npcHierarchy[hierarchicalId] = (gamingCSMage01Alt.gameObject, samGamingFolder, spriteOverlayGO.transform, spriteOverlayGO.transform);
                                count++;
                            }
                        }
                    }
                }
            }
            return count;
        }

        private int CollectGhostMinigameNpcs()
        {
            int count = 0;
            GameObject ghostMinigameGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "Ghost_Minigame" && go.scene.name != null);
            
            if (ghostMinigameGO != null)
            {
                Transform ghostMapCore = ghostMinigameGO.transform.Find("Ghost_Map_Core");
                if (ghostMapCore != null)
                {
                    // Collect Asgo_Astrid_Looking_0 and its variants from AstridLooking folder
                    Transform astridLooking = ghostMapCore.Find("AstridLooking");
                    if (astridLooking != null)
                    {
                        for (int i = 0; i < astridLooking.childCount; i++)
                        {
                            Transform astridChild = astridLooking.GetChild(i);
                            if (astridChild.name.StartsWith("Asgo_Astrid_Looking_0"))
                            {
                                SpriteRenderer sr = astridChild.GetComponent<SpriteRenderer>();
                                if (sr != null && sr.sprite != null)
                                {
                                    string hierarchicalId = $"Ghost_Minigame_AstridLooking_{astridChild.name}";
                                    if (!npcHierarchy.ContainsKey(hierarchicalId))
                                    {
                                        string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                        if (!defaultValues.ContainsKey(npcVarName))
                                        {
                                            defaultValues[npcVarName] = false;
                                        }
                                        npcHierarchy[hierarchicalId] = (astridChild.gameObject, astridLooking, ghostMapCore, ghostMinigameGO.transform);
                                        Debug.Log($"[SaveManager] Registered Ghost NPC: {hierarchicalId}");
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                    
                    // Collect Noire_Minigame_1 from ghost_Monsters folder
                    Transform ghostMonsters = ghostMapCore.Find("ghost_Monsters");
                    if (ghostMonsters != null)
                    {
                        Transform noireMinigame1 = ghostMonsters.Find("Noire_Minigame_1");
                        if (noireMinigame1 != null)
                        {
                            SpriteRenderer sr = noireMinigame1.GetComponent<SpriteRenderer>();
                            if (sr != null && sr.sprite != null)
                            {
                                string hierarchicalId = "Ghost_Minigame_ghost_Monsters_Noire_Minigame_1";
                                if (!npcHierarchy.ContainsKey(hierarchicalId))
                                {
                                    string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                    if (!defaultValues.ContainsKey(npcVarName))
                                    {
                                        defaultValues[npcVarName] = false;
                                    }
                                    npcHierarchy[hierarchicalId] = (noireMinigame1.gameObject, ghostMonsters, ghostMapCore, ghostMinigameGO.transform);
                                    Debug.Log($"[SaveManager] Registered Ghost NPC: {hierarchicalId}");
                                    count++;
                                }
                            }
                        }
                    }
                    
                    // Collect Noire_Jumpscare from Ghost_Map_Core
                    Transform noireJumpscare = ghostMapCore.Find("Noire_Jumpscare");
                    if (noireJumpscare != null)
                    {
                        SpriteRenderer sr = noireJumpscare.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            string hierarchicalId = "Ghost_Minigame_Noire_Jumpscare";
                            if (!npcHierarchy.ContainsKey(hierarchicalId))
                            {
                                string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                if (!defaultValues.ContainsKey(npcVarName))
                                {
                                    defaultValues[npcVarName] = false;
                                }
                                npcHierarchy[hierarchicalId] = (noireJumpscare.gameObject, ghostMapCore, ghostMapCore, ghostMinigameGO.transform);
                                Debug.Log($"[SaveManager] Registered Ghost NPC: {hierarchicalId}");
                                count++;
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.Log("[SaveManager] Ghost_Minigame not found");
            }
            return count;
        }

        private int CollectCamcorderMinigameNpcs()
        {
            int count = 0;
            GameObject camcorderMinigameGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "Camcorder_Minigame" && go.scene.name != null);
            
            if (camcorderMinigameGO != null)
            {
                // Collect from Camcorder_Bigfoot
                Transform camcorderBigfoot = camcorderMinigameGO.transform.Find("Camcorder_Bigfoot");
                if (camcorderBigfoot != null)
                {
                    // Collect Circle variants from Camcorder_Targets
                    Transform camcorderTargets = camcorderBigfoot.Find("Camcorder_Targets");
                    if (camcorderTargets != null)
                    {
                        for (int i = 0; i < camcorderTargets.childCount; i++)
                        {
                            Transform circleChild = camcorderTargets.GetChild(i);
                            if (circleChild.name.StartsWith("Circle"))
                            {
                                SpriteRenderer sr = circleChild.GetComponent<SpriteRenderer>();
                                if (sr != null && sr.sprite != null)
                                {
                                    string hierarchicalId = $"Camcorder_Minigame_Bigfoot_{circleChild.name}";
                                    if (!npcHierarchy.ContainsKey(hierarchicalId))
                                    {
                                        string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                        if (!defaultValues.ContainsKey(npcVarName))
                                        {
                                            defaultValues[npcVarName] = false;
                                        }
                                        npcHierarchy[hierarchicalId] = (circleChild.gameObject, camcorderTargets, camcorderBigfoot, camcorderMinigameGO.transform);
                                        Debug.Log($"[SaveManager] Registered Camcorder NPC: {hierarchicalId}");
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                    
                    // Collect Camcorder_Sprite_Emma1
                    Transform camcorderSpriteEmma1 = camcorderBigfoot.Find("Camcorder_Sprite_Emma1");
                    if (camcorderSpriteEmma1 != null)
                    {
                        SpriteRenderer sr = camcorderSpriteEmma1.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            string hierarchicalId = "Camcorder_Minigame_Bigfoot_Camcorder_Sprite_Emma1";
                            if (!npcHierarchy.ContainsKey(hierarchicalId))
                            {
                                string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                if (!defaultValues.ContainsKey(npcVarName))
                                {
                                    defaultValues[npcVarName] = false;
                                }
                                npcHierarchy[hierarchicalId] = (camcorderSpriteEmma1.gameObject, camcorderBigfoot, camcorderBigfoot, camcorderMinigameGO.transform);
                                Debug.Log($"[SaveManager] Registered Camcorder NPC: {hierarchicalId}");
                                count++;
                            }
                        }
                    }
                }
                
                // Collect from Camcorder_Lizard
                Transform camcorderLizard = camcorderMinigameGO.transform.Find("Camcorder_Lizard");
                if (camcorderLizard != null)
                {
                    // Collect Circle variants from Camcorder_Targets
                    Transform camcorderTargets = camcorderLizard.Find("Camcorder_Targets");
                    if (camcorderTargets != null)
                    {
                        for (int i = 0; i < camcorderTargets.childCount; i++)
                        {
                            Transform circleChild = camcorderTargets.GetChild(i);
                            if (circleChild.name.StartsWith("Circle"))
                            {
                                SpriteRenderer sr = circleChild.GetComponent<SpriteRenderer>();
                                if (sr != null && sr.sprite != null)
                                {
                                    string hierarchicalId = $"Camcorder_Minigame_Lizard_{circleChild.name}";
                                    if (!npcHierarchy.ContainsKey(hierarchicalId))
                                    {
                                        string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                        if (!defaultValues.ContainsKey(npcVarName))
                                        {
                                            defaultValues[npcVarName] = false;
                                        }
                                        npcHierarchy[hierarchicalId] = (circleChild.gameObject, camcorderTargets, camcorderLizard, camcorderMinigameGO.transform);
                                        Debug.Log($"[SaveManager] Registered Camcorder NPC: {hierarchicalId}");
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                    
                    // Collect Camcorder_Sprite_Emma1
                    Transform camcorderSpriteEmma1 = camcorderLizard.Find("Camcorder_Sprite_Emma1");
                    if (camcorderSpriteEmma1 != null)
                    {
                        SpriteRenderer sr = camcorderSpriteEmma1.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            string hierarchicalId = "Camcorder_Minigame_Lizard_Camcorder_Sprite_Emma1";
                            if (!npcHierarchy.ContainsKey(hierarchicalId))
                            {
                                string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                if (!defaultValues.ContainsKey(npcVarName))
                                {
                                    defaultValues[npcVarName] = false;
                                }
                                npcHierarchy[hierarchicalId] = (camcorderSpriteEmma1.gameObject, camcorderLizard, camcorderLizard, camcorderMinigameGO.transform);
                                Debug.Log($"[SaveManager] Registered Camcorder NPC: {hierarchicalId}");
                                count++;
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.Log("[SaveManager] Camcorder_Minigame not found");
            }
            return count;
        }

        private int CollectEventUINpcs()
        {
            int count = 0;
            GameObject eventUI = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go.name == "15_EventUI" && go.scene.name != null);
            
            if (eventUI != null)
            {
                Transform bpcNew = eventUI.transform.Find("BPC_New");
                if (bpcNew != null)
                {
                    // Adrian photos
                    Transform adrianPC = bpcNew.Find("AdrianPC_Wor");
                    if (adrianPC != null)
                    {
                        Transform adrianGO = adrianPC.Find("GameObject");
                        if (adrianGO != null)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                Transform imageTransform = adrianGO.Find($"Image ({i})");
                                if (imageTransform != null)
                                {
                                    string hierarchicalId = $"EventUI_Adrian_Image ({i})";
                                    if (!npcHierarchy.ContainsKey(hierarchicalId))
                                    {
                                        string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                        if (!defaultValues.ContainsKey(npcVarName))
                                        {
                                            defaultValues[npcVarName] = false;
                                        }
                                        npcHierarchy[hierarchicalId] = (imageTransform.gameObject, adrianGO, adrianPC, eventUI.transform);
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                    
                    // Other photos
                    Transform otherPC = bpcNew.Find("OtherPC_Browser");
                    if (otherPC != null)
                    {
                        Transform otherCore = otherPC.Find("OtherPC_Core");
                        if (otherCore != null)
                        {
                            Transform otherGO = otherCore.Find("GameObject");
                            if (otherGO != null)
                            {
                                for (int i = 1; i <= 4; i++)
                                {
                                    Transform gameObjectChild = otherGO.Find($"GameObject ({i})");
                                    if (gameObjectChild != null)
                                    {
                                        string hierarchicalId = $"EventUI_Other_GameObject ({i})";
                                        if (!npcHierarchy.ContainsKey(hierarchicalId))
                                        {
                                            string npcVarName = $"NPC_Seen_{hierarchicalId}";
                                            if (!defaultValues.ContainsKey(npcVarName))
                                            {
                                                defaultValues[npcVarName] = false;
                                            }
                                            npcHierarchy[hierarchicalId] = (gameObjectChild.gameObject, otherGO, otherCore, eventUI.transform);
                                            count++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return count;
        }


        /// <summary>
        /// Register a special case NPC for tracking (ones that don't follow standard hierarchy)
        /// </summary>
        public void RegisterSpecialCaseNpc(string hierarchicalNpcId, GameObject npcGO, Transform directParent, Transform levelsParent)
        {
            if (!npcHierarchy.ContainsKey(hierarchicalNpcId))
            {
                // Add to defaultValues if not already there
                string npcVarName = $"NPC_Seen_{hierarchicalNpcId}";
                if (!defaultValues.ContainsKey(npcVarName))
                {
                    defaultValues[npcVarName] = false;
                }
                
                // Store: NPC GO, its direct parent, and the 5_Levels child parent, and null for topLevelParent
                npcHierarchy[hierarchicalNpcId] = (npcGO, directParent, levelsParent, null);
                //Debug.Log($"[SaveManager] Registered special case NPC: {hierarchicalNpcId}");
            }
        }

        public void RegisterTabletopNpc(string hierarchicalNpcId, GameObject fighterGO, Transform directParent = null, Transform levelParent = null, Transform topLevelParent = null)
        {
            if (!npcHierarchy.ContainsKey(hierarchicalNpcId))
            {
                // Add to defaultValues if not already there
                string npcVarName = $"NPC_Seen_{hierarchicalNpcId}";
                if (!defaultValues.ContainsKey(npcVarName))
                {
                    defaultValues[npcVarName] = false;
                }
                
                // If parent transforms not provided, calculate them
                if (directParent == null)
                {
                    directParent = fighterGO.transform.parent;
                    levelParent = directParent != null ? directParent.parent : null;
                    topLevelParent = levelParent != null ? levelParent.parent : null;  // Tabletop_Minigame
                }
                
                npcHierarchy[hierarchicalNpcId] = (fighterGO, directParent, levelParent, topLevelParent);
                Debug.Log($"[SaveManager] Registered Tabletop NPC: {hierarchicalNpcId}");
            }
        }

        public void RegisterRacingNpc(string hierarchicalNpcId, GameObject racerGO, Transform directParent = null, Transform levelParent = null, Transform topLevelParent = null)
        {
            if (!npcHierarchy.ContainsKey(hierarchicalNpcId))
            {
                // Add to defaultValues if not already there
                string npcVarName = $"NPC_Seen_{hierarchicalNpcId}";
                if (!defaultValues.ContainsKey(npcVarName))
                {
                    defaultValues[npcVarName] = false;
                }
                
                // If parent transforms not provided, traverse up to find them
                if (directParent == null)
                {
                    directParent = racerGO.transform.parent;
                    levelParent = directParent != null ? directParent.parent : null;
                    
                    // Find Racing_Minigame by traversing up the hierarchy
                    Transform current = racerGO.transform;
                    while (current != null)
                    {
                        if (current.gameObject.name == "Racing_Minigame")
                        {
                            topLevelParent = current;
                            break;
                        }
                        current = current.parent;
                    }
                }
                
                npcHierarchy[hierarchicalNpcId] = (racerGO, directParent, levelParent, topLevelParent);
                Debug.Log($"[SaveManager] Registered Racing NPC: {hierarchicalNpcId}");
            }
        }

        public void RegisterBaristaaNpc(string hierarchicalNpcId, GameObject baristaGO, Transform directParent = null, Transform levelParent = null, Transform topLevelParent = null)
        {
            if (!npcHierarchy.ContainsKey(hierarchicalNpcId))
            {
                // Add to defaultValues if not already there
                string npcVarName = $"NPC_Seen_{hierarchicalNpcId}";
                if (!defaultValues.ContainsKey(npcVarName))
                {
                    defaultValues[npcVarName] = false;
                }
                
                // If parent transforms not provided, traverse up to find them
                if (directParent == null)
                {
                    directParent = baristaGO.transform.parent;
                    levelParent = directParent != null ? directParent.parent : null;
                    
                    // Find Barista_Game by traversing up the hierarchy
                    Transform current = baristaGO.transform;
                    while (current != null)
                    {
                        if (current.gameObject.name == "Barista_Game")
                        {
                            topLevelParent = current;
                            break;
                        }
                        current = current.parent;
                    }
                }
                
                npcHierarchy[hierarchicalNpcId] = (baristaGO, directParent, levelParent, topLevelParent);
                
                // Store unlock condition based on NPC name
                string npcName = baristaGO.name;
                string unlockCondition = GetBaristaUnlockCondition(npcName);
                baristaUnlockConditions[hierarchicalNpcId] = unlockCondition;
                
                Debug.Log($"[SaveManager] Registered Barista NPC: {hierarchicalNpcId} (Unlock: {unlockCondition ?? "Barista_Game activation"})");
            }
        }

        private string GetBaristaUnlockCondition(string npcName)
        {
            // Return GNV variable name, or null if unlocked by Barista_Game activation
            switch (npcName)
            {
                case "Goblin":
                case "Dwarf":
                case "Shaman":
                    return null;  // Unlocked by Barista_Game activation
                case "Thief":
                    return "thief";
                case "Angel":
                    return "angel";
                case "IceMage":
                    return "icewitch";
                case "Warrior":
                    return "warrior";
                case "Necromancer":
                    return "necromancer";
                case "Paladin":
                    return "paladin";
                case "Fire":
                    return "firewitch";
                case "Ghost":
                    return "ghost";
                case "Lancer":
                    return "lancer";
                case "Cerise":
                    return "cerise";
                case "PlantGirl":
                    return "plantgirl";
                case "Demon":
                    return "demon";
                default:
                    return null;
            }
        }

        public void RegisterBaristaSpecialNpc(string hierarchicalNpcId, GameObject specialNpc, Transform topLevelParent = null)
        {
            if (!npcHierarchy.ContainsKey(hierarchicalNpcId))
            {
                // Add to defaultValues if not already there
                string npcVarName = $"NPC_Seen_{hierarchicalNpcId}";
                if (!defaultValues.ContainsKey(npcVarName))
                {
                    defaultValues[npcVarName] = false;
                }
                
                // If topLevelParent not provided, traverse up to find it
                if (topLevelParent == null)
                {
                    Transform current = specialNpc.transform;
                    
                    // Find Barista_Game by traversing up the hierarchy
                    while (current != null)
                    {
                        if (current.gameObject.name == "Barista_Game")
                        {
                            topLevelParent = current;
                            break;
                        }
                        current = current.parent;
                    }
                }
                
                // Store minimal hierarchy info, topLevelParent is Barista_Game
                npcHierarchy[hierarchicalNpcId] = (specialNpc, null, null, topLevelParent);
                
                // FreeGob2 is unlocked by Barista_Game activation
                baristaUnlockConditions[hierarchicalNpcId] = null;
                
                Debug.Log($"[SaveManager] Registered Barista special NPC: {hierarchicalNpcId}");
            }
        }

        public void RegisterLiveStreamNpc(string hierarchicalNpcId, GameObject livestreamNpcGO)
        {
            if (!npcHierarchy.ContainsKey(hierarchicalNpcId))
            {
                // Add to defaultValues if not already there
                string npcVarName = $"NPC_Seen_{hierarchicalNpcId}";
                if (!defaultValues.ContainsKey(npcVarName))
                {
                    defaultValues[npcVarName] = false;
                }
                
                // For Livestream NPCs, find Livestream_Active as topLevelParent
                // Path: Livestream_Active > Live_Anna > [outfit] > [NPC]
                Transform current = livestreamNpcGO.transform;
                Transform livestreamActive = null;
                
                // Find Livestream_Active by traversing up the hierarchy
                while (current != null)
                {
                    if (current.gameObject.name == "Livestream_Active")
                    {
                        livestreamActive = current;
                        break;
                    }
                    current = current.parent;
                }
                
                // Get the outfit parent (Live_DefaultOutfit/CoolOutfit/SlutOutfit)
                Transform outfitParent = livestreamNpcGO.transform.parent;
                Transform liveAnna = outfitParent?.parent;
                
                // Store hierarchy info: NPC GO, outfit parent, Live_Anna, Livestream_Active
                npcHierarchy[hierarchicalNpcId] = (livestreamNpcGO, outfitParent, liveAnna, livestreamActive);
                Debug.Log($"[SaveManager] Registered Livestream NPC: {hierarchicalNpcId}");
            }
        }

        public void RegisterSwordNpc(string hierarchicalNpcId, GameObject swordNpcGO, Transform directParent = null, Transform levelParent = null, Transform topLevelParent = null)
        {
            if (!npcHierarchy.ContainsKey(hierarchicalNpcId))
            {
                // Add to defaultValues if not already there
                string npcVarName = $"NPC_Seen_{hierarchicalNpcId}";
                if (!defaultValues.ContainsKey(npcVarName))
                {
                    defaultValues[npcVarName] = false;
                }
                
                // If parent transforms not provided, traverse up to find them
                if (directParent == null)
                {
                    directParent = swordNpcGO.transform.parent;  // Sprites
                    levelParent = directParent != null ? directParent.parent : null;  // Enemy
                    
                    // Find Sword_Minigame by traversing up the hierarchy
                    Transform current = swordNpcGO.transform;
                    while (current != null)
                    {
                        if (current.gameObject.name == "Sword_Minigame")
                        {
                            topLevelParent = current;
                            break;
                        }
                        current = current.parent;
                    }
                }
                
                npcHierarchy[hierarchicalNpcId] = (swordNpcGO, directParent, levelParent, topLevelParent);
                Debug.Log($"[SaveManager] Registered Sword NPC: {hierarchicalNpcId}");
            }
        }

        public void RegisterAdoreviaaNpc(string hierarchicalNpcId, GameObject adoreviaaNpcGO, Transform directParent = null, Transform levelParent = null, Transform topLevelParent = null)
        {
            if (!npcHierarchy.ContainsKey(hierarchicalNpcId))
            {
                // Add to defaultValues if not already there
                string npcVarName = $"NPC_Seen_{hierarchicalNpcId}";
                if (!defaultValues.ContainsKey(npcVarName))
                {
                    defaultValues[npcVarName] = false;
                }
                
                // If parent transforms not provided, traverse up to find them
                if (directParent == null)
                {
                    directParent = adoreviaaNpcGO.transform.parent;  // Ado_Merchant or Ado_Mere
                    levelParent = directParent != null ? directParent.parent : null;  // Ado_E1BMerchant or Ado_E2Mere
                    
                    // Find Adorevia by traversing up the hierarchy
                    Transform current = adoreviaaNpcGO.transform;
                    while (current != null)
                    {
                        if (current.gameObject.name == "Adorevia")
                        {
                            topLevelParent = current;
                            break;
                        }
                        current = current.parent;
                    }
                }
                
                npcHierarchy[hierarchicalNpcId] = (adoreviaaNpcGO, directParent, levelParent, topLevelParent);
                Debug.Log($"[SaveManager] Registered Adorevia NPC: {hierarchicalNpcId}");
            }
        }

        public void RegisterMilkingNpc(string hierarchicalNpcId, GameObject milkingNpcGO, Transform directParent = null, Transform levelParent = null, Transform topLevelParent = null)
        {
            if (!npcHierarchy.ContainsKey(hierarchicalNpcId))
            {
                // Add to defaultValues if not already there
                string npcVarName = $"NPC_Seen_{hierarchicalNpcId}";
                if (!defaultValues.ContainsKey(npcVarName))
                {
                    defaultValues[npcVarName] = false;
                }
                
                // If parent transforms not provided, traverse up to find them
                if (directParent == null)
                {
                    directParent = milkingNpcGO.transform.parent;  // Default or Squeeze
                    levelParent = directParent != null ? directParent.parent : null;  // Character folder
                    
                    // Find MilkingMinigame by traversing up the hierarchy
                    Transform current = milkingNpcGO.transform;
                    while (current != null)
                    {
                        if (current.gameObject.name == "MilkingMinigame")
                        {
                            topLevelParent = current;
                            break;
                        }
                        current = current.parent;
                    }
                }
                
                npcHierarchy[hierarchicalNpcId] = (milkingNpcGO, directParent, levelParent, topLevelParent);
                Debug.Log($"[SaveManager] Registered Milking NPC: {hierarchicalNpcId}");
            }
        }

        public void RegisterSpriteOverlayNpc(string hierarchicalNpcId, GameObject npcGO)
        {
            if (!npcHierarchy.ContainsKey(hierarchicalNpcId))
            {
                // Add to defaultValues if not already there
                string npcVarName = $"NPC_Seen_{hierarchicalNpcId}";
                if (!defaultValues.ContainsKey(npcVarName))
                {
                    defaultValues[npcVarName] = false;
                }

                // Traverse upward to find Sprite_Overlay as topLevelParent
                Transform current = npcGO.transform;
                Transform directParent = current.parent;
                Transform levelParent = directParent != null ? directParent.parent : null;
                Transform spriteOverlay = null;

                while (current != null)
                {
                    if (current.gameObject.name == "Sprite_Overlay")
                    {
                        spriteOverlay = current;
                        break;
                    }
                    current = current.parent;
                }

                if (spriteOverlay == null)
                {
                    Debug.LogWarning($"[SaveManager] Could not find Sprite_Overlay parent for NPC '{hierarchicalNpcId}'");
                    return;
                }

                npcHierarchy[hierarchicalNpcId] = (npcGO, directParent, levelParent, spriteOverlay);
                Debug.Log($"[SaveManager] Registered Sprite_Overlay NPC: {hierarchicalNpcId}");
            }
        }

        public void RegisterCompareitScene(string sceneId, GameObject sceneGO)
        {
            if (!npcHierarchy.ContainsKey(sceneId))
            {
                // Add to defaultValues if not already there
                string sceneVarName = $"Scene_Seen_{sceneId}";
                if (!defaultValues.ContainsKey(sceneVarName))
                {
                    defaultValues[sceneVarName] = false;
                }

                // Traverse upward to find 16_Minigames as topLevelParent
                Transform current = sceneGO.transform;
                Transform directParent = current.parent;
                Transform levelParent = directParent != null ? directParent.parent : null;
                Transform minigamesRoot = null;

                while (current != null)
                {
                    if (current.gameObject.name == "16_Minigames")
                    {
                        minigamesRoot = current;
                        break;
                    }
                    current = current.parent;
                }

                if (minigamesRoot == null)
                {
                    Debug.LogWarning($"[SaveManager] Could not find 16_Minigames parent for scene '{sceneId}'");
                    return;
                }

                npcHierarchy[sceneId] = (sceneGO, directParent, levelParent, minigamesRoot);
                Debug.Log($"[SaveManager] Registered Compareit scene: {sceneId}");
            }
        }

        /// <summary>
        /// Register a scene for tracking (both regular scenes and special scenes)
        /// Ensures the scene GameObject is tracked in sceneGameObjects for monitoring activation
        /// </summary>
        public void RegisterSceneForTracking(string sceneId, GameObject sceneGO)
        {
            if (!sceneGameObjects.ContainsKey(sceneId))
            {
                // Add to defaultValues if not already there
                string sceneVarName = $"Scene_Seen_{sceneId}";
                if (!defaultValues.ContainsKey(sceneVarName))
                {
                    defaultValues[sceneVarName] = false;
                }

                // Store in sceneGameObjects for monitoring
                sceneGameObjects[sceneId] = sceneGO;
                //Debug.Log($"[SaveManager] Registered scene for tracking: {sceneId}");
            }
        }

        public void RegisterEventUIPhotoNpc(string hierarchicalNpcId, GameObject npcGO, Transform directParent = null, Transform levelParent = null, Transform topLevelParent = null)
        {
            if (!npcHierarchy.ContainsKey(hierarchicalNpcId))
            {
                // Add to defaultValues if not already there
                string npcVarName = $"NPC_Seen_{hierarchicalNpcId}";
                if (!defaultValues.ContainsKey(npcVarName))
                {
                    defaultValues[npcVarName] = false;
                }

                // If parent transforms not provided, traverse up to find them
                if (directParent == null)
                {
                    directParent = npcGO.transform.parent;
                    levelParent = directParent != null ? directParent.parent : null;
                    
                    // Traverse upward to find 15_EventUI as topLevelParent
                    Transform current = npcGO.transform;
                    while (current != null)
                    {
                        if (current.gameObject.name == "15_EventUI")
                        {
                            topLevelParent = current;
                            break;
                        }
                        current = current.parent;
                    }
                }

                if (topLevelParent == null)
                {
                    Debug.LogWarning($"[SaveManager] Could not find 15_EventUI parent for NPC '{hierarchicalNpcId}'");
                    return;
                }

                npcHierarchy[hierarchicalNpcId] = (npcGO, directParent, levelParent, topLevelParent);
                Debug.Log($"[SaveManager] Registered EventUI Photo NPC: {hierarchicalNpcId}");
            }
        }

        /// <summary>
        /// Generic recursive NPC collector. Walks hierarchy and finds all GOs with SpriteRenderers.
        /// </summary>
        /// <param name="parent">Current transform being checked</param>
        /// <param name="levelsParent">The "level" parent for hierarchical ID naming</param>
        /// <param name="npcCount">Reference to increment when NPCs are found</param>
        private void CollectNpcsRecursive(Transform parent, Transform levelsParent, ref int npcCount)
        {
            // Check if current GO has a SpriteRenderer
            SpriteRenderer spriteRenderer = parent.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // Get the immediate parent
                Transform directParent = parent.parent;
                
                // Create hierarchical ID: LevelsParentName_DirectParentName_NpcName
                string directParentName = directParent != null ? directParent.gameObject.name : "Unknown";
                string levelsParentName = levelsParent.gameObject.name;
                string npcName = parent.gameObject.name;
                string hierarchicalNpcId = $"{levelsParentName}_{directParentName}_{npcName}";
                
                // Exclude Circle NPCs from 133_HarborDistrict Groups 1 and 3
                if (levelsParentName == "133_HarborDistrict")
                {
                    if ((directParentName == "Group_1" || directParentName == "Group_3") && npcName.StartsWith("Circle"))
                    {
                        // Skip Circle NPCs from these groups
                        return;
                    }
                }
                
                if (!npcHierarchy.ContainsKey(hierarchicalNpcId))
                {
                    // Add to defaultValues if not already there
                    string npcVarName = $"NPC_Seen_{hierarchicalNpcId}";
                    if (!defaultValues.ContainsKey(npcVarName))
                    {
                        defaultValues[npcVarName] = false;
                    }
                    
                    // Store: (NPC GO, directParent, levelsParent, null topLevelParent)
                    npcHierarchy[hierarchicalNpcId] = (parent.gameObject, directParent, levelsParent, null);
                    //Debug.Log($"[SaveManager] Registered NPC: {hierarchicalNpcId} (Active state - NPC: {parent.gameObject.activeSelf}, DirectParent: {directParent?.gameObject.activeSelf ?? false}, LevelsParent: {levelsParent.gameObject.activeSelf})");
                    npcCount++;
                }
                
                // Don't recurse into children if this GO has a sprite
                return;
            }
            
            // Recurse to children only if current doesn't have a sprite
            for (int i = 0; i < parent.childCount; i++)
            {
                CollectNpcsRecursive(parent.GetChild(i), levelsParent, ref npcCount);
            }
        }

        /// <summary>
        /// Collect Adorevia NPCs which use UI Image components instead of SpriteRenderers
        /// Only collects specific NPC objects: Ado_Merchant (from Ado_E1BMerchant) and Ado_Mere (from Ado_E2Mere)
        /// </summary>
        private void CollectAdovereiaUIPhotoNpcs(Transform adoEvents, ref int npcCount)
        {
            // Collect Ado_Merchant from Ado_E1BMerchant event
            Transform ado_E1BMerchant = adoEvents.Find("Ado_E1BMerchant");
            if (ado_E1BMerchant != null)
            {
                Transform ado_Merchant = ado_E1BMerchant.Find("Ado_Merchant");
                if (ado_Merchant != null)
                {
                    RegisterAdovereiaUIPhotoNpc("Ado_Merchant", ado_Merchant, ado_E1BMerchant, adoEvents, ref npcCount);
                }
            }
            
            // Collect Ado_Mere from Ado_E2Mere event
            Transform ado_E2Mere = adoEvents.Find("Ado_E2Mere");
            if (ado_E2Mere != null)
            {
                Transform ado_Mere = ado_E2Mere.Find("Ado_Mere");
                if (ado_Mere != null)
                {
                    RegisterAdovereiaUIPhotoNpc("Ado_Mere", ado_Mere, ado_E2Mere, adoEvents, ref npcCount);
                }
            }
        }

        /// <summary>
        /// Register a single Adorevia UI-based NPC (Image component)
        /// </summary>
        private void RegisterAdovereiaUIPhotoNpc(string npcName, Transform npcTransform, Transform eventParent, Transform adoEvents, ref int npcCount)
        {
            UnityEngine.UI.Image image = npcTransform.GetComponent<UnityEngine.UI.Image>();
            if (image != null && image.sprite != null)
            {
                string hierarchicalNpcId = $"Adorevia_Ado_Events_{eventParent.gameObject.name}_{npcName}";
                
                if (!npcHierarchy.ContainsKey(hierarchicalNpcId))
                {
                    // Add to defaultValues if not already there
                    string npcVarName = $"NPC_Seen_{hierarchicalNpcId}";
                    if (!defaultValues.ContainsKey(npcVarName))
                    {
                        defaultValues[npcVarName] = false;
                    }
                    
                    // Store: (NPC GO, eventParent, adoEvents, null)
                    npcHierarchy[hierarchicalNpcId] = (npcTransform.gameObject, eventParent, adoEvents, null);
                    Debug.Log($"[SaveManager] Registered NPC: {hierarchicalNpcId} (Active state - NPC: {npcTransform.gameObject.activeSelf}, DirectParent: {eventParent?.gameObject.activeSelf ?? false}, LevelsParent: {adoEvents.gameObject.activeSelf})");
                    npcCount++;
                }
            }
        }

        /// <summary>
        /// Monitor scene activation and mark as seen when first activated
        /// </summary>
        private void MonitorSceneActivation()
        {
            foreach (var kvp in sceneGameObjects)
            {
                string sceneId = kvp.Key;
                GameObject sceneGO = kvp.Value;
                string sceneVarName = $"Scene_Seen_{sceneId}";

                // If scene is active in hierarchy and not yet marked as seen and not marked this session
                if (sceneGO != null && sceneGO.activeInHierarchy && !GetBool(sceneVarName, false) && !markedScenesThisSession.Contains(sceneId))
                {
                    MarkSceneAsSeen(sceneId);
                    markedScenesThisSession.Add(sceneId);
                    Debug.Log($"==================== [SaveManager] Scene marked as seen: {sceneId} ====================");
                }
            }
        }

        /// <summary>
        /// Monitor bust activation and mark as seen when first activated
        /// </summary>
        private void MonitorBustActivation()
        {
            foreach (var kvp in bustGameObjects)
            {
                string bustId = kvp.Key;
                GameObject bustGO = kvp.Value;
                string bustVarName = $"Bust_Seen_{bustId}";

                // If bust is active in hierarchy and not yet marked as seen and not marked this session
                if (bustGO != null && bustGO.activeInHierarchy && !GetBool(bustVarName, false) && !markedBustsThisSession.Contains(bustId))
                {
                    MarkBustAsSeen(bustId);
                    markedBustsThisSession.Add(bustId);
                    Debug.Log($"==================== [SaveManager] Bust marked as seen: {bustId} ====================");
                }
            }
        }

        /// <summary>
        /// Monitor NPC activation and mark as seen when first activated
        /// Requires NPC GO, its direct parent (Group_X), and its 5_Levels parent to all be active
        /// Barista NPCs have special unlock conditions based on GNV variables or Barista_Game activation
        /// </summary>
        private void MonitorNpcActivation()
        {
            foreach (var kvp in npcHierarchy)
            {
                string npcId = kvp.Key;
                var (npcGO, directParent, levelsParent, topLevelParent) = kvp.Value;
                string npcVarName = $"NPC_Seen_{npcId}";

                // Skip if NPC GO is null (destroyed or never loaded)
                if (npcGO == null)
                {
                    continue;
                }

                // Check if this is a Barista NPC with special unlock conditions
                bool isBarista = baristaUnlockConditions.ContainsKey(npcId);
                bool unlocked = false;

                if (isBarista)
                {
                    // For Barista NPCs, check the specific unlock condition
                    string unlockCondition = baristaUnlockConditions[npcId];
                    if (unlockCondition == null)
                    {
                        // Unlocked by Barista_Game activation - check if the Barista_Game GameObject is active
                        // (not the NPC itself, since it might be nested inside inactive containers)
                        GameObject baristaGameGO = GameObject.Find("Barista_Game");
                        unlocked = baristaGameGO != null && baristaGameGO.activeSelf;
                    }
                    else
                    {
                        // Unlocked by GNV variable
                        unlocked = SMSGallery.Core.GetVariableBool(unlockCondition);
                    }
                }
                else
                {
                    // For non-Barista NPCs, simply check if active in hierarchy
                    // activeInHierarchy automatically checks all parents
                    unlocked = npcGO.activeInHierarchy;
                }

                // If unlocked and not yet marked as seen and not marked this session
                if (unlocked && !GetBool(npcVarName, false) && !markedNpcsThisSession.Contains(npcId))
                {
                    // Check if this is a blacklisted NPC that unlocks another NPC
                    if (SMSGallery.Core.TryGetBlacklistedNpcMapping(npcId, out string primaryNpcId))
                    {
                        MarkBlacklistedNpcAsSeen(npcId, primaryNpcId);
                    }
                    else
                    {
                        MarkNpcAsSeen(npcId);
                    }
                    markedNpcsThisSession.Add(npcId);
                    Debug.Log($"==================== [SaveManager] NPC marked as seen: {npcId} ====================");
                }
            }
        }

        /// <summary>
        /// Monitor special scene activation and mark as seen when first activated
        /// Requires special scene GO, all its direct parents, and the 15_EventUI parent to all be active
        /// </summary>
        private void MonitorSpecialSceneActivation()
        {
            foreach (var kvp in specialSceneHierarchy)
            {
                string specialSceneId = kvp.Key;
                var (specialSceneGO, directParent, eventUIParent) = kvp.Value;
                string specialSceneVarName = $"SpecialScene_Seen_{specialSceneId}";

                // Skip if special scene GO is null (destroyed or never loaded)
                if (specialSceneGO == null)
                {
                    continue;
                }

                // Check if special scene is unlocked: activeInHierarchy automatically checks all parents
                bool unlocked = specialSceneGO.activeInHierarchy;

                // If unlocked and not yet marked as seen and not marked this session
                if (unlocked && !GetBool(specialSceneVarName, false) && !markedSpecialScenesThisSession.Contains(specialSceneId))
                {
                    MarkSpecialSceneAsSeen(specialSceneId);
                    markedSpecialScenesThisSession.Add(specialSceneId);
                    Debug.Log($"==================== [SaveManager] Special Scene marked as seen: {specialSceneId} ====================");
                }
            }
        }

        /// <summary>
        /// Mark a scene as seen and save
        /// </summary>
        public void MarkSceneAsSeen(string sceneId)
        {
            SetBool($"Scene_Seen_{sceneId}", true);
        }

        /// <summary>
        /// Check if a scene has been seen
        /// </summary>
        public bool IsSceneSeen(string sceneId)
        {
            return GetBool($"Scene_Seen_{sceneId}", false);
        }

        /// <summary>
        /// Get all scenes with their seen status
        /// </summary>
        public Dictionary<string, bool> GetAllScenes()
        {
            Dictionary<string, bool> scenes = new Dictionary<string, bool>();
            foreach (var kvp in currentSlotCache)
            {
                if (kvp.Key.StartsWith("Scene_Seen_") && kvp.Value is bool)
                {
                    string sceneId = kvp.Key.Substring("Scene_Seen_".Length);
                    scenes[sceneId] = (bool)kvp.Value;
                }
            }
            return scenes;
        }

        /// <summary>
        /// Check if a bust has been seen
        /// </summary>
        public bool IsBustSeen(string bustId)
        {
            return GetBool($"Bust_Seen_{bustId}", false);
        }

        /// <summary>
        /// Mark a bust as seen and save
        /// </summary>
        public void MarkBustAsSeen(string bustId)
        {
            SetBool($"Bust_Seen_{bustId}", true);
        }

        /// <summary>
        /// Check if an NPC has been seen
        /// </summary>
        public bool IsNpcSeen(string npcId)
        {
            return GetBool($"NPC_Seen_{npcId}", false);
        }

        /// <summary>
        /// Mark an NPC as seen and save
        /// </summary>
        public void MarkNpcAsSeen(string npcId)
        {
            SetBool($"NPC_Seen_{npcId}", true);
        }

        /// <summary>
        /// Mark a blacklisted NPC as seen, which also marks the primary NPC as seen
        /// Used for NPCs that share textures with other NPCs
        /// </summary>
        public void MarkBlacklistedNpcAsSeen(string blacklistedNpcId, string primaryNpcId)
        {
            // Mark both the blacklisted NPC and the primary NPC as seen
            MarkNpcAsSeen(blacklistedNpcId);
            MarkNpcAsSeen(primaryNpcId);
            Debug.Log($"[SaveManager] Marked blacklisted NPC {blacklistedNpcId} as seen, which unlocks {primaryNpcId}");
        }

        /// <summary>
        /// Check if a special scene has been seen
        /// </summary>
        public bool IsSpecialSceneSeen(string specialSceneId)
        {
            return GetBool($"SpecialScene_Seen_{specialSceneId}", false);
        }

        /// <summary>
        /// Mark a special scene as seen and save
        /// </summary>
        public void MarkSpecialSceneAsSeen(string specialSceneId)
        {
            SetBool($"SpecialScene_Seen_{specialSceneId}", true);
        }

        /// <summary>
        /// Register a special scene for tracking
        /// </summary>
        public void RegisterSpecialScene(string specialSceneId, GameObject specialSceneGO)
        {
            if (!specialSceneHierarchy.ContainsKey(specialSceneId))
            {
                // Add to defaultValues if not already there
                string sceneVarName = $"SpecialScene_Seen_{specialSceneId}";
                if (!defaultValues.ContainsKey(sceneVarName))
                {
                    defaultValues[sceneVarName] = false;
                }

                // Find the 15_EventUI parent by traversing up the hierarchy
                Transform current = specialSceneGO.transform;
                Transform eventUIParent = null;
                
                while (current != null)
                {
                    if (current.gameObject.name == "15_EventUI")
                    {
                        eventUIParent = current;
                        break;
                    }
                    current = current.parent;
                }

                // Store: Special Scene GO, its direct parent, and 15_EventUI parent
                Transform directParent = specialSceneGO.transform.parent;
                specialSceneHierarchy[specialSceneId] = (specialSceneGO, directParent, eventUIParent);
                Debug.Log($"[SaveManager] Registered special scene: {specialSceneId}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Marker component used to track whether listeners have already been attached 
    /// to the EmptySaveSlot button to prevent listener stacking.
    /// </summary>
    public class SaveMenuEmptySaveSlotMarker : MonoBehaviour
    {
        // This is just a marker - no functionality needed
    }
} 
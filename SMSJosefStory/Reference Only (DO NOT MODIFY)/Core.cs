using BepInEx;
using GameCreator;
using GameCreator.Runtime.Cameras;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
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
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace SMSAndroidsCore
{
    [BepInPlugin(pluginGuid, Core.pluginName, Core.pluginVersion)]
    public class Core : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsandroidscore.core";
        public const string pluginName = "Androids Core";
        public const string pluginVersion = "0.7.0";
        #endregion

        public static AssetBundle characterBundle;
        public static AssetBundle dialogueBundle;
        public static AssetBundle minigameBundle;
        public static AssetBundle otherBundle;
        public static bool loadedCore = false;
        public static bool loadedBases = false;
        public static bool loadedMenu = false;
        public static GameObject afterSleepEvents;
        public static GameObject affectionIncrease;
        public static GameObject baseBust;
        public static GameObject disableAllBusts;
        public static GameObject introMomentNewGame;
        public static GameObject levelBeach;
        public static GameObject mainCamera;
        public static GameObject menuModHeader;
        public static GameObject savedUI;
        public static GameObject saveLoadSystem;
        public static GameObject toggleRepeatableBedEvents;
        public static int loadedSaveFile;
        public static SaveLoadManager saveLoadManager;
        public static Scene currentScene;
        public static string assetPath = "BepInEx\\plugins\\SMSAndroidsCore\\Assets\\";
        public static string audioPath = "BepInEx\\plugins\\SMSAndroidsCore\\Audio\\";
        public static string minigamePath = "BepInEx\\plugins\\SMSAndroidsCore\\Minigame\\";
        public static string bustPath = "BepInEx\\plugins\\SMSAndroidsCore\\Busts\\";
        public static string bustNikkePath = "BepInEx\\plugins\\SMSAndroidsCore\\Busts\\Nikke\\";
        public static string itemsPath = "BepInEx\\plugins\\SMSAndroidsCore\\Items\\";
        public static string locationPath = "BepInEx\\plugins\\SMSAndroidsCore\\Locations\\";
        // savesPath removed - saves are now stored in AppData\LocalLow, not in plugin folder
        public static string scenePath = "BepInEx\\plugins\\SMSAndroidsCore\\Scenes\\";
        public static string uiPath = "BepInEx\\plugins\\SMSAndroidsCore\\UI\\";
        public static string wallpaperPath = "BepInEx\\plugins\\SMSAndroidsCore\\Wallpaper\\";
        public static string exePath;
        public static Transform attractorCanvas;
        public static Transform audioPlayer;
        public static Transform bustManager;
        public static Transform cGManagerSexy;
        public static Transform coreEvents;
        public static Transform effects;
        public static Transform gameplay;
        public static Transform level;
        public static Transform mainCanvas;
        public static Transform roomTalk;

        public static GlobalNameVariables proxyVariables;
        private static bool proxyVariablesInitialized = false;
        private static float lastSyncTime = 0f;
        private static float syncInterval = 1.5f; // Sync every 1.5 seconds

        public void Awake()
        {
            exePath = Application.dataPath;
            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                exePath += "/../../";
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                exePath += "/../";
            }
            characterBundle = AssetBundle.LoadFromFile(exePath + assetPath + "characterbundle");
            dialogueBundle = AssetBundle.LoadFromFile(exePath + assetPath + "dialoguebundle");
            otherBundle = AssetBundle.LoadFromFile(exePath + assetPath + "otherbundle");

            // Minigame bundle is optional — visual assets fall back to procedural if absent
            string minigameBundlePath = exePath + assetPath + "minigamebundle";
            if (System.IO.File.Exists(minigameBundlePath))
                minigameBundle = AssetBundle.LoadFromFile(minigameBundlePath);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        public void OnSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            currentScene = scene;
            loadedCore = false;
            loadedBases = false;
            Characters.loadedBusts = false;
            Dialogues.loadedDialogues = false;
            MainStory.loadedStory = false;
            Minigames.loadedMinigame = false;
            Places.loadedPlaces = false;
            Scenes.loadedScenes = false;
            Schedule.loadedSchedule = false;
            Wallpaper.loadedWallpaper = false;
            ScheduleVisualizer.loadedVisualizer = false;
        }
        public void Update()
        {
            if (loadedCore && Characters.loadedBusts && Dialogues.loadedDialogues && MainStory.loadedStory && Places.loadedPlaces && Scenes.loadedScenes)
            {
                loadedBases = true;
            }
            else
            {
                loadedBases = false;
            }

            // Continuously sync vanilla variables to proxy variables (throttled)
            if (proxyVariablesInitialized && proxyVariables != null)
            {
                float currentTime = Time.time;
                if (currentTime - lastSyncTime >= syncInterval)
                {
                    SyncVanillaToProxyVariables();
                    lastSyncTime = currentTime;
                }
            }

            if (currentScene.name == "CoreGameScene")
            {
                if (!loadedCore)
                {
                    saveLoadManager = GameObject.FindFirstObjectByType<SaveLoadManager>();
                    loadedSaveFile = saveLoadManager.SlotLoaded;

                    attractorCanvas = GameObject.Find("Attractor_Canvas").transform;
                    bustManager = GameObject.Find("2_Bust_Manager").transform;
                    cGManagerSexy = GameObject.Find("4_CG_Manager-Sexy").transform;
                    level = GameObject.Find("5_Levels").transform;
                    effects = GameObject.Find("6_Effects").transform;
                    coreEvents = GameObject.Find("8_Core_Events").transform;
                    roomTalk = GameObject.Find("8_Room_Talk").transform;
                    mainCanvas = GameObject.Find("9_MainCanvas").transform;
                    gameplay = GameObject.Find("10_Gameplay").transform;
                    audioPlayer = GameObject.Find("12_AudioPlayer").transform;

                    afterSleepEvents = mainCanvas.Find("AfterSleepEvents").gameObject;
                    baseBust = bustManager.Find("Anna_YellowSexy").gameObject; 
                    disableAllBusts = GameObject.Find("Disable_All_chars");
                    introMomentNewGame = mainCanvas.transform.Find("Starmaker").Find("Intro_Moment").gameObject;
                    levelBeach = level.Find("14_Beach").gameObject;
                    mainCamera = GameObject.Find("Main Camera");
                    savedUI = effects.Find("Effect_Canvas").Find("Game_Saved").Find("Saved").gameObject;
                    saveLoadSystem = mainCanvas.transform.Find("SaveLoadSystem").gameObject;
                    toggleRepeatableBedEvents = mainCanvas.transform.Find("Starmaker").Find("Starmaker_Profile").Find("Extra_settings").Find("Toggle").gameObject;

                    affectionIncrease = GameObject.Instantiate(effects.Find("CFXR2 Expression Loving (Timed)").gameObject, effects);
                    affectionIncrease.name = "CFXR2 Expression Affectionate (Timed)";
                    affectionIncrease.GetComponent<ParticleSystem>().startColor = new Color(1.0f, 0.6f, 0.9f);
                    affectionIncrease.transform.Find("Hearts").gameObject.GetComponent<ParticleSystem>().startColor = new Color(1.0f, 0.35f, 0.7f);

                    // Initialize Proxy Variables from asset bundle
                    InitializeProxyVariables();

                    Logger.LogInfo("----- CORE LOADED -----");
                    loadedMenu = false;
                    loadedCore = true;

                    //Debugging.PrintConditionsAndTriggers(gameplay.Find("Sleeping").gameObject);
                    //Debugging.PrintButtonInstructions(Core.level.Find("5_MyRoom").Find("PlayerRoom_ButtonCanvas").Find("Player_Room_Buttons").Find("SleepButton").gameObject);
                    //Debugging.PrintDialogueComponentDeep(coreEvents.Find("Trips").Find("AmeliaMeetingDefault").Find("DefaultDialogue").gameObject, 10);
                    //Debugging.PrintDialogueComponentInfo(coreEvents.Find("Himari_Dinner").Find("Himari_Dinner_Three").Find("himaribathroom").gameObject);
                    //Debugging.PrintToggleOnValueChanged(mainCanvas.Find("Starmaker").Find("Starmaker_Profile").Find("Extra_settings").Find("Toggle").gameObject);
                    //Core.EmitSignalDelayed("FadeIn2025", 5f);
                    //Core.EmitSignalDelayed("FadeOut2025", 6f);
                    //Debugging.PrintAllGlobalNameVariables();
                    //StartCoroutine(Debugging.PrintAllGlobalNameVariablesDelayed(5f));
                }
            }
            if (currentScene.name == "GameStart")
            {
                if (!loadedMenu)
                {
                    CreateModHeader();
                    //Logger.LogInfo("----- MENU LOADED -----");
                    loadedMenu = true;
                }
                if (loadedCore)
                {
                    Logger.LogInfo("----- CORE UNLOADED -----");
                    loadedCore = false;
                }
            }
        }

        
        public void CreateModHeader()
        {
            GameObject originalText = GameObject.Find("Part_One").transform.Find("Canvas_MM").Find("MainMenu").Find("Text (TMP)").gameObject;
            menuModHeader = GameObject.Instantiate(originalText, GameObject.Find("Part_One").transform.Find("Canvas_MM").Find("MainMenu"));
            
            string originalTextContent = originalText.GetComponent<TextMeshProUGUI>().text;
            string modHeaderText = "Androids Mod " + pluginVersion + " for 1.8E";
            menuModHeader.GetComponent<TextMeshProUGUI>().text = modHeaderText;
            menuModHeader.GetComponent<TextMeshProUGUI>().fontSize = 40;
            menuModHeader.GetComponent<TextMeshProUGUI>().fontSizeMin = 26;
            menuModHeader.GetComponent<TextMeshProUGUI>().fontSizeMax = 84;
            
            // Check if mod header text ending matches original header text ending
            // Extract the last part after the last space
            string originalEnding = originalTextContent.Substring(originalTextContent.LastIndexOf(' ') + 1);
            string modHeaderEnding = modHeaderText.Substring(modHeaderText.LastIndexOf(' ') + 1);
            if (originalEnding != modHeaderEnding)
            {
                menuModHeader.GetComponent<TextMeshProUGUI>().color = Color.red;
            }
            
            //menuModHeader.GetComponent<TextMeshProUGUI>().outlineColor = new Color32(235, 192, 52, 255);
            RectTransform originalRectTransform = originalText.GetComponent<RectTransform>();
            RectTransform newRectTransform = menuModHeader.GetComponent<RectTransform>();
            newRectTransform.anchoredPosition = originalRectTransform.anchoredPosition - new Vector2(-100, 35);
            newRectTransform.sizeDelta = new Vector2(400, 50);
            Debug.Log(menuModHeader.GetComponent<TextMeshProUGUI>().text);
        }
        public static void FindAndModifyVariableDouble(string variableNameToFind, Double newValue)
        {
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GlobalNameVariablesManager not initialized");
                return;
            }

            // Access private Values dictionary
            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty(
                "Values",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null) return;

            bool found = false;

            foreach (var pair in values)
            {
                // Access the runtime's Variables dictionary
                PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null) continue;

                if (variables.TryGetValue(variableNameToFind, out var nameVar))
                {
                    // Get the asset name for logging
                    var repo = TRepository<VariablesRepository>.Get;
                    var asset = repo?.Variables.GetNameVariablesAsset(pair.Key);
                    string assetName = asset != null ? asset.name : $"Unknown Asset ({pair.Key})";

                    // Store old value
                    object oldValue = nameVar.Value;

                    // Modify the value
                    nameVar.Value = newValue;
                    found = true;

                    Debug.Log($"Modified {variableNameToFind} in {assetName} " +
                             $"from {oldValue} to {newValue}");

                    // Trigger change event if needed
                    MethodInfo eventMethod = typeof(NameVariableRuntime).GetMethod(
                        "OnRuntimeChange",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    eventMethod?.Invoke(pair.Value, new object[] { variableNameToFind });
                }
            }

            if (!found)
            {
                Debug.LogError($"Variable '{variableNameToFind}' not found in any global variable set");
            }
        }
        public static void FindAndModifyVariableBool(string variableNameToFind, bool newValue)
        {
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GlobalNameVariablesManager not initialized");
                return;
            }

            // Access private Values dictionary
            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty(
                "Values",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null) return;

            bool found = false;

            foreach (var pair in values)
            {
                // Access the runtime's Variables dictionary
                PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null) continue;

                if (variables.TryGetValue(variableNameToFind, out var nameVar))
                {
                    // Get the asset name for logging
                    var repo = TRepository<VariablesRepository>.Get;
                    var asset = repo?.Variables.GetNameVariablesAsset(pair.Key);
                    string assetName = asset != null ? asset.name : $"Unknown Asset ({pair.Key})";

                    // Store old value
                    object oldValue = nameVar.Value;

                    // Modify the value
                    nameVar.Value = newValue;
                    found = true;

                    Debug.Log($"Modified {variableNameToFind} in {assetName} " +
                             $"from {oldValue} to {newValue}");

                    // Trigger change event if needed
                    MethodInfo eventMethod = typeof(NameVariableRuntime).GetMethod(
                        "OnRuntimeChange",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    eventMethod?.Invoke(pair.Value, new object[] { variableNameToFind });
                }
            }

            if (!found)
            {
                Debug.LogError($"Variable '{variableNameToFind}' not found in any global variable set");
            }
        }
        public static void FindAndModifyVariableGameObject(string variableNameToFind, GameObject newValue)
        {
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GlobalNameVariablesManager not initialized");
                return;
            }

            // Access private Values dictionary
            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty(
                "Values",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null) return;

            bool found = false;

            foreach (var pair in values)
            {
                // Access the runtime's Variables dictionary
                PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null) continue;

                if (variables.TryGetValue(variableNameToFind, out var nameVar))
                {
                    // Get the asset name for logging
                    var repo = TRepository<VariablesRepository>.Get;
                    var asset = repo?.Variables.GetNameVariablesAsset(pair.Key);
                    string assetName = asset != null ? asset.name : $"Unknown Asset ({pair.Key})";

                    // Store old value
                    object oldValue = nameVar.Value;

                    // Modify the value
                    nameVar.Value = newValue;
                    found = true;

                    Debug.Log($"Modified {variableNameToFind} in {assetName} " +
                             $"from {oldValue} to {newValue}");

                    // Trigger change event if needed
                    MethodInfo eventMethod = typeof(NameVariableRuntime).GetMethod(
                        "OnRuntimeChange",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    eventMethod?.Invoke(pair.Value, new object[] { variableNameToFind });
                }
            }

            if (!found)
            {
                Debug.LogError($"Variable '{variableNameToFind}' not found in any global variable set");
            }
        }
        public static bool GetVariableBool(string variableNameToFind)
        {
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GlobalNameVariablesManager not initialized");
                return false;
            }

            // Access private Values dictionary
            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty(
                "Values",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null) return false;

            foreach (var pair in values)
            {
                // Access the runtime's Variables dictionary
                PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null) continue;

                if (variables.TryGetValue(variableNameToFind, out var nameVar))
                {
                    return (bool)nameVar.Value;
                }
            }

            Debug.LogError($"Variable '{variableNameToFind}' not found in any global variable set");
            return false;
        }
        public static double GetVariableNumber(string variableNameToFind)
        {
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GlobalNameVariablesManager not initialized");
                return 0.0;
            }

            // Access private Values dictionary
            PropertyInfo valuesProp = typeof(GlobalNameVariablesManager).GetProperty(
                "Values",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var values = valuesProp.GetValue(manager) as Dictionary<IdString, NameVariableRuntime>;
            if (values == null) return 0.0;

            foreach (var pair in values)
            {
                // Access the runtime's Variables dictionary
                PropertyInfo varsProp = typeof(NameVariableRuntime).GetProperty(
                    "Variables",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                var variables = varsProp.GetValue(pair.Value) as Dictionary<string, NameVariable>;
                if (variables == null) continue;

                if (variables.TryGetValue(variableNameToFind, out var nameVar))
                {
                    try
                    {
                        return Convert.ToDouble(nameVar.Value);
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning($"Variable '{variableNameToFind}' found but could not be converted to a number. Value: {nameVar.Value}");
                        return 0.0;
                    }
                }
            }

            Debug.LogError($"Variable '{variableNameToFind}' not found in any global variable set");
            return 0.0;
        }
        public static void AddGameObjectToLocalListVariables(GameObject targetGameObject, GameObject valueToAdd)
        {
            if (targetGameObject == null)
            {
                Debug.LogError("Target GameObject is null");
                return;
            }

            LocalListVariables localListVariables = targetGameObject.GetComponent<LocalListVariables>();
            if (localListVariables == null)
            {
                Debug.LogError($"LocalListVariables component not found on GameObject: {targetGameObject.name}");
                return;
            }

            localListVariables.Push(valueToAdd);
            Debug.Log($"Added GameObject '{valueToAdd?.name ?? "null"}' to LocalListVariables on GameObject: {targetGameObject.name}. New count: {localListVariables.Count}");
        }
        public static int GetRandomNumber(int max)
        {
            return UnityEngine.Random.Range(0, max + 1);
        }
        public static float GetRandomFloat(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        /// <summary>
        /// Gets a boolean value from Proxy Variables.
        /// </summary>
        public static bool GetProxyVariableBool(string variableName, bool defaultValue = false)
        {
            if (proxyVariables == null)
            {
                Debug.LogWarning($"[ProxyVariables] Proxy variables not initialized, cannot get '{variableName}'");
                return defaultValue;
            }

            if (!proxyVariables.Exists(variableName))
            {
                Debug.LogWarning($"[ProxyVariables] Variable '{variableName}' not found in Proxy Variables");
                return defaultValue;
            }

            try
            {
                object value = proxyVariables.Get(variableName);
                if (value is bool boolValue)
                {
                    return boolValue;
                }
                else if (value is int intValue)
                {
                    return intValue != 0;
                }
                else
                {
                    Debug.LogWarning($"[ProxyVariables] Variable '{variableName}' could not be converted to boolean. Value: {value}");
                    return defaultValue;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProxyVariables] Error getting variable '{variableName}': {e.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a numeric value from Proxy Variables.
        /// </summary>
        public static double GetProxyVariableNumber(string variableName, double defaultValue = 0.0)
        {
            if (proxyVariables == null)
            {
                Debug.LogWarning($"[ProxyVariables] Proxy variables not initialized, cannot get '{variableName}'");
                return defaultValue;
            }

            if (!proxyVariables.Exists(variableName))
            {
                Debug.LogWarning($"[ProxyVariables] Variable '{variableName}' not found in Proxy Variables");
                return defaultValue;
            }

            try
            {
                object value = proxyVariables.Get(variableName);
                return Convert.ToDouble(value);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ProxyVariables] Variable '{variableName}' could not be converted to number. Value: {proxyVariables.Get(variableName)}. Error: {e.Message}");
                return defaultValue;
            }
        }

        public static string GetProxyVariableString(string variableName, string defaultValue = "")
        {
            if (proxyVariables == null)
            {
                Debug.LogWarning($"[ProxyVariables] Proxy variables not initialized, cannot get '{variableName}'");
                return defaultValue;
            }

            if (!proxyVariables.Exists(variableName))
            {
                Debug.LogWarning($"[ProxyVariables] Variable '{variableName}' not found in Proxy Variables");
                return defaultValue;
            }

            try
            {
                object value = proxyVariables.Get(variableName);
                if (value is string stringValue)
                {
                    return stringValue;
                }
                else
                {
                    return value?.ToString() ?? defaultValue;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ProxyVariables] Variable '{variableName}' could not be converted to string. Value: {proxyVariables.Get(variableName)}. Error: {e.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a value from Proxy Variables as an object.
        /// </summary>
        public static object GetProxyVariable(string variableName, object defaultValue = null)
        {
            if (proxyVariables == null)
            {
                Debug.LogWarning($"[ProxyVariables] Proxy variables not initialized, cannot get '{variableName}'");
                return defaultValue;
            }

            if (!proxyVariables.Exists(variableName))
            {
                Debug.LogWarning($"[ProxyVariables] Variable '{variableName}' not found in Proxy Variables");
                return defaultValue;
            }

            try
            {
                return proxyVariables.Get(variableName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProxyVariables] Error getting variable '{variableName}': {e.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Resets all Proxy Variables that start with "DailyProc_" to false.
        /// </summary>
        public static void RefreshDailyProxyVariables()
        {
            if (proxyVariables == null)
            {
                Debug.LogWarning("[ProxyVariables] Proxy variables not initialized, cannot refresh daily variables");
                return;
            }

            string[] allVariables = proxyVariables.Names;
            if (allVariables == null || allVariables.Length == 0)
            {
                Debug.Log("[ProxyVariables] No variables found in Proxy Variables");
                return;
            }

            var keysToReset = new List<string>();
            foreach (var varName in allVariables)
            {
                if (varName.StartsWith("DailyProc_"))
                {
                    keysToReset.Add(varName);
                }
            }

            foreach (var varName in keysToReset)
            {
                proxyVariables.Set(varName, false);
                Debug.Log($"[ProxyVariables] Reset daily variable: {varName}");
            }

            Debug.Log($"[ProxyVariables] Refreshed {keysToReset.Count} daily proxy variables");
        }

        /// <summary>
        /// Sets a boolean value in Proxy Variables.
        /// </summary>
        public static void FindAndModifyProxyVariableBool(string variableName, bool newValue)
        {
            if (proxyVariables == null)
            {
                Debug.LogError("[ProxyVariables] Proxy variables not initialized, cannot modify variable");
                return;
            }

            if (!proxyVariables.Exists(variableName))
            {
                Debug.LogError($"[ProxyVariables] Variable '{variableName}' not found in Proxy Variables");
                return;
            }

            try
            {
                object oldValue = proxyVariables.Get(variableName);
                proxyVariables.Set(variableName, newValue);
                Debug.Log($"[ProxyVariables] Modified {variableName} from {oldValue} to {newValue}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProxyVariables] Error modifying variable '{variableName}': {e.Message}");
            }
        }

        /// <summary>
        /// Sets a string value in Proxy Variables.
        /// </summary>
        public static void FindAndModifyProxyVariableString(string variableName, string newValue)
        {
            if (proxyVariables == null)
            {
                Debug.LogError("[ProxyVariables] Proxy variables not initialized, cannot modify variable");
                return;
            }

            if (!proxyVariables.Exists(variableName))
            {
                Debug.LogError($"[ProxyVariables] Variable '{variableName}' not found in Proxy Variables");
                return;
            }

            try
            {
                object oldValue = proxyVariables.Get(variableName);
                proxyVariables.Set(variableName, newValue);
                Debug.Log($"[ProxyVariables] Modified {variableName} from {oldValue} to {newValue}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProxyVariables] Error modifying variable '{variableName}': {e.Message}");
            }
        }

        /// <summary>
        /// Sets a Gift_ proxy variable and syncs it to SaveManager.
        /// This should be called when a gift is given/used to ensure the change persists.
        /// </summary>
        public static void SetAndSyncGiftVariable(string giftVariableName, bool newValue)
        {
            if (proxyVariables == null)
            {
                Debug.LogError("[ProxyVariables] Proxy variables not initialized, cannot modify gift variable");
                return;
            }

            if (!proxyVariables.Exists(giftVariableName))
            {
                Debug.LogError($"[ProxyVariables] Gift variable '{giftVariableName}' not found in Proxy Variables");
                return;
            }

            try
            {
                proxyVariables.Set(giftVariableName, newValue);
                Debug.Log($"[ProxyVariables] Set gift variable {giftVariableName} to {newValue}");
                
                // Sync to SaveManager to ensure persistence
                SaveManager.SetBool(giftVariableName, newValue);
                Debug.Log($"[ProxyVariables] Synced gift variable {giftVariableName} to SaveManager with value {newValue}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProxyVariables] Error setting gift variable '{giftVariableName}': {e.Message}");
            }
        }

        /// <summary>
        /// Increments affection for a character if they like the given gift.
        /// Checks the characterGiftLikesMap in Characters.cs to determine if the character likes the gift.
        /// </summary>
        public static void IncrementAffectionForGiftIfLiked(string giftName, string characterName)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(giftName) || string.IsNullOrEmpty(characterName))
            {
                Debug.LogWarning("[Core] Gift name or character name is null/empty in IncrementAffectionForGiftIfLiked");
                return;
            }

            // Check if the character exists in the mapping
            if (!Characters.characterGiftLikesMap.ContainsKey(characterName))
            {
                Debug.Log($"[Core] Character '{characterName}' not in characterGiftLikesMap");
                return;
            }

            // Check if this character likes this gift
            List<string> giftsCharacterLikes = Characters.characterGiftLikesMap[characterName];
            if (!giftsCharacterLikes.Contains(giftName))
            {
                Debug.Log($"[Core] Character '{characterName}' does not like gift '{giftName}'");
                return;
            }

            // Character likes the gift, increment affection
            string affectionVarName = $"Affection_{characterName}";
            try
            {
                int currentAffection = SaveManager.GetInt(affectionVarName, 0);
                SaveManager.SetInt(affectionVarName, currentAffection + 1);
                Debug.Log($"[Core] Incremented {affectionVarName} from {currentAffection} to {currentAffection + 1}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Core] Error incrementing affection for '{characterName}': {ex.Message}");
            }
        }

        private static void InitializeProxyVariables()
        {
            if (proxyVariablesInitialized)
            {
                Debug.LogWarning("[ProxyVariables] Already initialized, skipping");
                return;
            }

            // Load Proxy Variables asset from dialogue bundle
            proxyVariables = dialogueBundle.LoadAsset<GlobalNameVariables>("Proxy Variables");
            if (proxyVariables == null)
            {
                Debug.LogError("[ProxyVariables] Failed to load 'Proxy Variables' asset from dialogue bundle");
                return;
            }

            Debug.Log($"[ProxyVariables] Loaded asset: {proxyVariables.name} (UniqueID: {proxyVariables.UniqueID})");

            // Manually initialize the proxy variables in the GlobalNameVariablesManager
            var manager = GlobalNameVariablesManager.Instance;
            if (manager == null)
            {
                Debug.LogError("[ProxyVariables] GlobalNameVariablesManager not initialized");
                return;
            }

            // Use reflection to call private RequireInit method
            MethodInfo requireInitMethod = typeof(GlobalNameVariablesManager).GetMethod(
                "RequireInit",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (requireInitMethod != null)
            {
                requireInitMethod.Invoke(manager, new object[] { proxyVariables });
                Debug.Log("[ProxyVariables] Successfully initialized in GlobalNameVariablesManager");
            }
            else
            {
                Debug.LogError("[ProxyVariables] Failed to find RequireInit method via reflection");
                return;
            }

            // Set up synchronization between vanilla and proxy variables
            SetupVariableSync();

            proxyVariablesInitialized = true;
            Debug.Log("[ProxyVariables] Initialization complete");
        }

        private static readonly Dictionary<string, string> booleanVariableMappings = new Dictionary<string, string>
        {
            { "Beer", "Gift_Beer" },
            { "Body-Oil", "Gift_Body-Oil" },
            { "Chocolate", "Gift_Chocolate" },
            { "Flowers", "Gift_Flowers" },
            { "red-meat", "Gift_red-meat" },
            { "Wine", "Gift_Wine" },
            { "Whiskey", "Gift_Whiskey" },
            { "Inv-energydrink", "Gift_Inv-energydrink" },
            { "inv-lovegum", "Gift_inv-lovegum" },
            { "inv-vape", "Gift_inv-vape" }
        };

        private static readonly Dictionary<string, string> numericVariableMappings = new Dictionary<string, string>
        {
            { "Cash", "Gameplay_Cash" }
        };

        public static Transform FindInActiveObjectByName(string name)
        {
            Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].hideFlags == HideFlags.None)
                {
                    if (objs[i].name == name)
                    {
                        return objs[i].gameObject.transform;
                    }
                }
            }
            return null;
        }

        private static Dictionary<string, bool> lastVanillaBoolValues = new Dictionary<string, bool>();
        private static Dictionary<string, double> lastVanillaNumericValues = new Dictionary<string, double>();
        private static Dictionary<string, int> lastSaveManagerIntValues = new Dictionary<string, int>();
        private static Dictionary<string, bool> lastSaveManagerBoolValues = new Dictionary<string, bool>();
        private static HashSet<string> excludedProxyGiftNames = new HashSet<string>(booleanVariableMappings.Values);

        // SaveManager to Proxy variable mappings
        private static readonly List<string> saveManagerIntMappings = new List<string>
        {
            "Affection_Amber",
            "Affection_Claire",
            "Affection_Sarah",
            "Affection_Anis",
            "Affection_Centi",
            "Affection_Dorothy",
            "Affection_Elegg",
            "Affection_Frima",
            "Affection_Guilty",
            "Affection_Helm",
            "Affection_Maiden",
            "Affection_Mary",
            "Affection_Mast",
            "Affection_Neon",
            "Affection_Pepper",
            "Affection_Rapi",
            "Affection_Rosanna",
            "Affection_Sakura",
            "Affection_Tove",
            "Affection_Viper",
            "Affection_Yan"
        };

        private static readonly List<string> saveManagerBoolMappings = new List<string>
        {
            "Affection_Anis_Seen2",
            "Affection_Anis_Seen3",
            "Gift_Action-Figure",
            "Gift_Bikini",
            "Gift_Bonsai-Tree",
            "Gift_Parasol",
            "Gift_Ring",
            "Gift_Shark-Tooth-Necklace",
            "Gift_Sunglasses",
            "Gift_Sunscreen",
            "Gift_Tropical-Flower-Bouquet",
            "HarborHome_Bought",
            "HarborHome_FirstVisited"
        };

        private static void SetupVariableSync()
        {
            Debug.Log("[ProxyVariables] Setting up variable synchronization");

            // Initialize tracking dictionaries
            foreach (var mapping in booleanVariableMappings)
            {
                lastVanillaBoolValues[mapping.Key] = false;
            }

            foreach (var mapping in numericVariableMappings)
            {
                lastVanillaNumericValues[mapping.Key] = 0.0;
            }

            // Register callback on proxy variables to log changes
            proxyVariables.Register((changedVarName) =>
            {
                object value = proxyVariables.Get(changedVarName);
                Debug.Log($"[ProxyVariables] Variable changed: {changedVarName} = {value}");
            });

            Debug.Log("[ProxyVariables] Variable synchronization setup complete (using polling)");
        }

        private static void SyncVanillaToProxyVariables()
        {
            // Sync vanilla boolean variables (gifts)
            foreach (var mapping in booleanVariableMappings)
            {
                string vanillaName = mapping.Key;
                string proxyName = mapping.Value;

                bool currentValue = GetVariableBool(vanillaName);

                // Only update if value changed
                if (!lastVanillaBoolValues.ContainsKey(vanillaName) || lastVanillaBoolValues[vanillaName] != currentValue)
                {
                    proxyVariables.Set(proxyName, currentValue);
                    lastVanillaBoolValues[vanillaName] = currentValue;
                    Debug.Log($"[ProxyVariables] Synced: {proxyName} = {currentValue} (vanilla {vanillaName} changed)");
                }
            }

            // Sync vanilla numeric variables
            foreach (var mapping in numericVariableMappings)
            {
                string vanillaName = mapping.Key;
                string proxyName = mapping.Value;

                double currentValue = GetVariableNumber(vanillaName);

                // Only update if value changed
                if (!lastVanillaNumericValues.ContainsKey(vanillaName) || Math.Abs(lastVanillaNumericValues[vanillaName] - currentValue) > 0.001)
                {
                    proxyVariables.Set(proxyName, currentValue);
                    lastVanillaNumericValues[vanillaName] = currentValue;
                    Debug.Log($"[ProxyVariables] Synced: {proxyName} = {currentValue} (vanilla {vanillaName} changed)");
                }
            }

            // Sync SaveManager integer variables to Proxy
            foreach (var variableName in saveManagerIntMappings)
            {
                int currentValue = SaveManager.GetInt(variableName, 0);

                // Check if proxy variable exists
                if (!proxyVariables.Exists(variableName))
                {
                    Debug.LogWarning($"[ProxyVariables] SaveManager variable '{variableName}' not found in Proxy Variables asset");
                    continue;
                }

                // Only update if value changed
                if (!lastSaveManagerIntValues.ContainsKey(variableName) || lastSaveManagerIntValues[variableName] != currentValue)
                {
                    proxyVariables.Set(variableName, currentValue);
                    lastSaveManagerIntValues[variableName] = currentValue;
                    Debug.Log($"[ProxyVariables] Synced: {variableName} = {currentValue} (SaveManager int changed)");
                }
            }

            // Sync SaveManager boolean variables to Proxy
            foreach (var variableName in saveManagerBoolMappings)
            {
                bool currentValue = SaveManager.GetBool(variableName, false);

                // Check if proxy variable exists
                if (!proxyVariables.Exists(variableName))
                {
                    Debug.LogWarning($"[ProxyVariables] SaveManager variable '{variableName}' not found in Proxy Variables asset");
                    continue;
                }

                // Only update if value changed
                if (!lastSaveManagerBoolValues.ContainsKey(variableName) || lastSaveManagerBoolValues[variableName] != currentValue)
                {
                    proxyVariables.Set(variableName, currentValue);
                    lastSaveManagerBoolValues[variableName] = currentValue;
                    Debug.Log($"[ProxyVariables] Synced: {variableName} = {currentValue} (SaveManager bool changed)");
                }
            }
        }

        public static void EmitSignalDelayed(string signalName, float delay = 3f)
        {
            var debugging = FindObjectOfType<Debugging>();
            if (debugging != null)
            {
                debugging.StartCoroutine(EmitSignalDelayedCoroutine(signalName, delay));
            }
            else
            {
                Debug.LogError("[EmitSignalDelayed] Debugging instance not found");
            }
        }

        private static IEnumerator EmitSignalDelayedCoroutine(string signalName, float delay)
        {
            Debug.Log($"[EmitSignalDelayed] Emitting signal '{signalName}' in {delay} seconds...");
            yield return new WaitForSeconds(delay);
            
            var signalArgs = new SignalArgs(new PropertyName(signalName), null);
            Signals.Emit(signalArgs);
            Debug.Log($"[EmitSignalDelayed] Signal '{signalName}' emitted!");
        }
        public static GameObject ChangeOutfitDelayed(GameObject currentOutfitGO, GameObject newOutfitGO, string variableName, string newValue, float delay)
        {
            var debugging = FindObjectOfType<Debugging>();
            if (debugging != null)
            {
                debugging.StartCoroutine(ChangeOutfitDelayedCoroutine(currentOutfitGO, newOutfitGO, variableName, newValue, delay));
            }
            else
            {
                Debug.LogError("[ChangeOutfitDelayed] Debugging instance not found");
            }
            return newOutfitGO;
        }
        public static IEnumerator ChangeOutfitDelayedCoroutine(GameObject currentOutfitGO,GameObject newOutfitGO, string variableName, string newValue, float delay)
        {
            Debug.Log($"[ChangeOutfitDelayed] Changing '{variableName}' to {newValue} in {delay} seconds...");
            yield return new WaitForSeconds(delay);

            currentOutfitGO.SetActive(false);
            SaveManager.SetString(variableName, newValue);
            newOutfitGO.SetActive(true);

            Debug.Log($"[ChangeOutfitDelayed] Variable '{variableName}' changed to {newValue}");
        }

        public static void EmitSignalGameObjectDelayed(string signalName, GameObject go1, GameObject go2, float delay = 3f)
        {
            var debugging = FindObjectOfType<Debugging>();
            if (debugging != null)
            {
                debugging.StartCoroutine(EmitSignalGameObjectDelayedCoroutine(signalName, go1, go2, delay));
            }
            else
            {
                Debug.LogError("[EmitSignalGameObjectDelayed] Debugging instance not found");
            }
        }

        private static IEnumerator EmitSignalGameObjectDelayedCoroutine(string signalName, GameObject go1, GameObject go2, float delay)
        {
            Debug.Log($"[EmitSignalGameObjectDelayed] Starting sequence with signal '{signalName}' and delay {delay} seconds...");
            
            // Step 1: Enable Trigger and Conditions components of GO1 if they exist and are disabled
            if (go1 != null)
            {
                var triggerComponent = go1.GetComponent<Trigger>();
                if (triggerComponent != null && !triggerComponent.enabled)
                {
                    triggerComponent.enabled = true;
                    Debug.Log($"[EmitSignalGameObjectDelayed] Enabled Trigger component on {go1.name}");
                }

                var conditionsComponent = go1.GetComponent<Conditions>();
                if (conditionsComponent != null && !conditionsComponent.enabled)
                {
                    conditionsComponent.enabled = true;
                    Debug.Log($"[EmitSignalGameObjectDelayed] Enabled Conditions component on {go1.name}");
                }
            }

            // Step 2: Wait 2/3 of the full wait time
            float twoThirdDelay = delay * 2f / 3f;
            yield return new WaitForSeconds(twoThirdDelay);

            // Step 3: Disable GO1
            if (go1 != null)
            {
                go1.SetActive(false);
                Debug.Log($"[EmitSignalGameObjectDelayed] Disabled {go1.name}");
            }

            // Step 4: Disable Trigger and Conditions components of GO2
            if (go2 != null)
            {
                var triggerComponent = go2.GetComponent<Trigger>();
                if (triggerComponent != null && triggerComponent.enabled)
                {
                    triggerComponent.enabled = false;
                    Debug.Log($"[EmitSignalGameObjectDelayed] Disabled Trigger component on {go2.name}");
                }

                var conditionsComponent = go2.GetComponent<Conditions>();
                if (conditionsComponent != null && conditionsComponent.enabled)
                {
                    conditionsComponent.enabled = false;
                    Debug.Log($"[EmitSignalGameObjectDelayed] Disabled Conditions component on {go2.name}");
                }
            }

            // Step 5: Enable GO2
            if (go2 != null)
            {
                go2.SetActive(true);
                Debug.Log($"[EmitSignalGameObjectDelayed] Enabled {go2.name}");
            }

            // Step 6: Wait the last 1/3 of the full wait time
            float oneThirdDelay = delay * 1f / 3f;
            yield return new WaitForSeconds(oneThirdDelay);

            // Step 7: Emit the signal
            var signalArgs = new SignalArgs(new PropertyName(signalName), null);
            Signals.Emit(signalArgs);
            Debug.Log($"[EmitSignalGameObjectDelayed] Signal '{signalName}' emitted!");
        }

        public static void DisableAllActiveBustChildren()
        {
            if (bustManager == null)
            {
                Debug.LogError("[DisableAllActiveBustChildren] bustManager is null");
                return;
            }

            int disabledCount = 0;
            foreach (Transform child in bustManager)
            {
                if (child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                    disabledCount++;
                }
            }
            
            Debug.Log($"[DisableAllActiveBustChildren] Disabled {disabledCount} active bust children");
        }
    }
}

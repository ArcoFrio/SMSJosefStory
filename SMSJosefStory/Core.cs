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
using System.Text.RegularExpressions;
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

namespace SMSJosefStory
{
    [BepInPlugin(pluginGuid, Core.pluginName, Core.pluginVersion)]
    public class Core : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsjosefstory.core";
        public const string pluginName = "Josef Story";
        public const string pluginVersion = "1.0.0";
        #endregion

        public static bool loadedCore = false;
        public static bool loadedBases = false;
        public static bool loadedMenu = false;
        public static bool hasVisitedCore = false;
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
        public static string assetPath = "BepInEx\\plugins\\SMSJosefStory\\Assets\\";
        public static string audioPath = "BepInEx\\plugins\\SMSJosefStory\\Audio\\";
        public static string minigamePath = "BepInEx\\plugins\\SMSJosefStory\\Minigame\\";
        public static string bustPath = "BepInEx\\plugins\\SMSJosefStory\\Busts\\";
        public static string bustNikkePath = "BepInEx\\plugins\\SMSJosefStory\\Busts\\Nikke\\";
        public static string itemsPath = "BepInEx\\plugins\\SMSJosefStory\\Items\\";
        public static string locationPath = "BepInEx\\plugins\\SMSJosefStory\\Locations\\";
        // savesPath removed - saves are now stored in AppData\LocalLow, not in plugin folder
        public static string scenePath = "BepInEx\\plugins\\SMSJosefStory\\Scenes\\";
        public static string uiPath = "BepInEx\\plugins\\SMSJosefStory\\UI\\";
        public static string wallpaperPath = "BepInEx\\plugins\\SMSJosefStory\\Wallpaper\\";
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

        public static AudioClip josefGibberishClip;

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
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Extract and load custom gibberish audio
            StartCoroutine(LoadJosefGibberish());

            var harmony = new Harmony(pluginGuid);
            harmony.PatchAll();
        }
        public void OnSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            currentScene = scene;
            loadedCore = false;
            loadedBases = false;
        }
        public void Update()
        {
            if (loadedCore)
            {
                loadedBases = true;
            }
            else
            {
                loadedBases = false;
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

                    Logger.LogInfo("----- CORE LOADED -----");
                    loadedMenu = false;
                    loadedCore = true;
                    hasVisitedCore = true;

                    // Josef-ify all busts on load
                    JosefifyAllBusts();

                    // Josef-ify all CG scenes on load
                    JosefifyAllScenes();

                    // Josef-ify all NPCs on load
                    JosefifyAllNPCs();

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
                    if (hasVisitedCore)
                    {
                        JosefifyMainMenu();
                    }
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

        /// <summary>
        /// Loads a PNG embedded in the DLL as a Texture2D.
        /// resourcePath uses dots as separators, relative to the root namespace.
        /// Example: LoadEmbeddedTexture("Textures.Bust.Josef.D1Base.png")
        /// </summary>
        public static Texture2D LoadEmbeddedTexture(string resourcePath, bool pointFilter = true)
        {
            string fullName = "SMSJosefStory." + resourcePath;
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullName);
            if (stream == null)
            {
                Debug.LogError($"[EmbeddedTexture] Resource not found: {fullName}");
                return null;
            }

            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(data);
            if (pointFilter) tex.filterMode = FilterMode.Point;
            return tex;
        }

        /// <summary>
        /// Creates a Sprite from an embedded PNG texture.
        /// Example: LoadEmbeddedSprite("Textures.Bust.Josef.D1Base.png")
        /// </summary>
        public static Sprite LoadEmbeddedSprite(string resourcePath, bool pointFilter = true)
        {
            Texture2D tex = LoadEmbeddedTexture(resourcePath, pointFilter);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        private IEnumerator LoadJosefGibberish()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "SMSJosefStory_Hmmm.ogg");

            // Extract embedded OGG to temp file
            using (Stream resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SMSJosefStory.Audio.Hmmm.ogg"))
            {
                if (resStream == null)
                {
                    Debug.LogError("[LoadJosefGibberish] Embedded resource Audio.Hmmm.ogg not found");
                    yield break;
                }
                using (FileStream fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                {
                    resStream.CopyTo(fs);
                }
            }

            // Load via UnityWebRequest
            string fileUri = "file:///" + tempPath.Replace("\\", "/");
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.OGGVORBIS))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    josefGibberishClip = DownloadHandlerAudioClip.GetContent(www);
                    josefGibberishClip.name = "Josef_Hmmm";
                    Debug.Log("[LoadJosefGibberish] Custom gibberish audio loaded successfully");
                }
                else
                {
                    Debug.LogError($"[LoadJosefGibberish] Failed to load audio: {www.error}");
                }
            }
        }

        #region Josef-ify Busts

        // Random Josef textures that will be assigned to all non-exception busts
        private static readonly string[] josefTextures = new string[]
        {
            "Textures.Bust.Josef.D1Base.png",
            "Textures.Bust.Josef.D1BaseHike.png",
            "Textures.Bust.Josef.D1BaseSmoke.png",
            "Textures.Bust.Josef.D1Base_Dout.png",
            "Textures.Bust.Josef.Bust_JosefGreyTShirt.png",
            "Textures.Bust.Josef.Bust_JosefNude.png",
            "Textures.Bust.Josef.JosefChristmasBust.png",
            "Textures.Bust.Josef.JosefHalloweenBust.png"
        };

        // The shared mask for all Josef-ified busts
        private const string josefMask = "Textures.Bust.D1BaseMask.png";

        // Exception bust names -> (texture, mask) resource paths
        // Add bust names here that should get a specific J-character instead of random Josef
        private static readonly Dictionary<string, (string texture, string mask)> josefExceptions = new Dictionary<string, (string, string)>
        {
            { "Adrian",      ("Textures.Bust.Jadrian00.png",   "Textures.Bust.Jadrian00Mask.png") },
            { "Alice",       ("Textures.Bust.Jalice00.png",    "Textures.Bust.Jalice00Mask.png") },
            { "Anna",        ("Textures.Bust.Janna00.png",     "Textures.Bust.Janna00Mask.png") },
            { "Charlotte",   ("Textures.Bust.Jarlotte00.png",  "Textures.Bust.Jarlotte00Mask.png") },
        };

        /// <summary>
        /// Replaces the base sprite and mask of every bust child in bustManager.
        /// Non-exception busts get a random Josef texture with the shared D1BaseMask.
        /// Exception busts (matched by name containing the key) get specific J-character textures.
        /// </summary>
        public static void JosefifyAllBusts()
        {
            if (bustManager == null)
            {
                Debug.LogError("[JosefifyAllBusts] bustManager is null");
                return;
            }

            // Pre-load the shared Josef mask texture once
            Texture2D sharedMask = LoadEmbeddedTexture(josefMask);
            if (sharedMask == null)
            {
                Debug.LogError("[JosefifyAllBusts] Failed to load shared Josef mask");
                return;
            }

            // Pre-load all Josef sprites
            Sprite[] josefSprites = new Sprite[josefTextures.Length];
            for (int i = 0; i < josefTextures.Length; i++)
            {
                josefSprites[i] = LoadEmbeddedSprite(josefTextures[i]);
            }

            // Pre-load exception textures
            var exceptionSprites = new Dictionary<string, (Sprite sprite, Texture2D mask)>();
            foreach (var kvp in josefExceptions)
            {
                Sprite s = LoadEmbeddedSprite(kvp.Value.texture);
                Texture2D m = LoadEmbeddedTexture(kvp.Value.mask);
                exceptionSprites[kvp.Key] = (s, m);
            }

            // Find Josef's own bust to grab Blink/Mouth/Expressions source sprites
            Transform josefBust = bustManager.Find("Josef");
            Transform josefMBase = josefBust?.Find("D1Base");
            Sprite josefBlinkSprite = null;
            Sprite[] josefMouthSprites = new Sprite[4];
            Sprite josefHappySprite = null, josefAngrySprite = null, josefSadSprite = null, josefFlirtySprite = null;

            if (josefMBase != null)
            {
                Transform josefBlink = josefMBase.Find("Blink");
                if (josefBlink != null)
                {
                    SpriteRenderer blinkSR = josefBlink.GetComponent<SpriteRenderer>();
                    if (blinkSR != null) josefBlinkSprite = blinkSR.sprite;
                }

                Transform josefMouth = josefMBase.Find("Mouth");
                if (josefMouth != null)
                {
                    for (int m = 0; m < 4; m++)
                    {
                        Transform mouthChild = josefMouth.Find((m + 1).ToString());
                        if (mouthChild != null)
                        {
                            SpriteRenderer mSR = mouthChild.GetComponent<SpriteRenderer>();
                            if (mSR != null) josefMouthSprites[m] = mSR.sprite;
                        }
                    }
                }

                Transform josefExpr = josefMBase.Find("Expressions");
                if (josefExpr != null)
                {
                    SpriteRenderer sr;
                    Transform t;
                    t = josefExpr.Find("Happy"); if (t != null) { sr = t.GetComponent<SpriteRenderer>(); if (sr != null) josefHappySprite = sr.sprite; }
                    t = josefExpr.Find("Angry"); if (t != null) { sr = t.GetComponent<SpriteRenderer>(); if (sr != null) josefAngrySprite = sr.sprite; }
                    t = josefExpr.Find("Sad"); if (t != null) { sr = t.GetComponent<SpriteRenderer>(); if (sr != null) josefSadSprite = sr.sprite; }
                    t = josefExpr.Find("Flirty"); if (t != null) { sr = t.GetComponent<SpriteRenderer>(); if (sr != null) josefFlirtySprite = sr.sprite; }
                }
            }

            // Exception busts whose Blink/Mouth/Expressions should be flipped
            var flipExceptionKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Adrian", "Anna", "Charlotte" };

            int josefCount = 0;
            int exceptionCount = 0;
            int skippedJosef = 0;

            foreach (Transform child in bustManager)
            {
                // Skip Josef's own busts
                if (child.name.StartsWith("Josef", StringComparison.OrdinalIgnoreCase))
                {
                    skippedJosef++;
                    continue;
                }

                // Try both MBase1 and D1Base child patterns
                Transform mBase = child.Find("MBase1");
                if (mBase == null) mBase = child.Find("D1Base");
                if (mBase == null) continue;

                SpriteRenderer sr = mBase.GetComponent<SpriteRenderer>();
                if (sr == null) continue;

                // Check if this bust matches any exception
                string matchedKey = null;
                foreach (var key in josefExceptions.Keys)
                {
                    if (child.name.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matchedKey = key;
                        break;
                    }
                }

                bool isFlipException = false;

                if (matchedKey != null)
                {
                    // Apply exception J-character texture
                    var (sprite, mask) = exceptionSprites[matchedKey];
                    if (sprite != null) sr.sprite = sprite;
                    if (mask != null) sr.material.SetTexture("_MaskTex", mask);
                    isFlipException = flipExceptionKeys.Contains(matchedKey);
                    exceptionCount++;
                    Debug.Log($"[JosefifyAllBusts] Exception: {child.name} -> {matchedKey}");
                }
                else
                {
                    // Apply random Josef texture
                    int randomIndex = UnityEngine.Random.Range(0, josefSprites.Length);
                    Sprite chosen = josefSprites[randomIndex];
                    if (chosen != null) sr.sprite = chosen;
                    sr.material.SetTexture("_MaskTex", sharedMask);

                    // Randomly flip horizontally (flips both diffuse and mask uniformly)
                    if (UnityEngine.Random.value > 0.5f)
                    {
                        Vector3 scale = mBase.localScale;
                        scale.x = -Mathf.Abs(scale.x);
                        mBase.localScale = scale;
                    }

                    josefCount++;
                }

                // Copy Josef's Blink/Mouth/Expressions sprites onto this bust
                CopyJosefFacialSprites(mBase, josefBlinkSprite, josefMouthSprites,
                    josefHappySprite, josefAngrySprite, josefSadSprite, josefFlirtySprite,
                    isFlipException);
            }

            Debug.Log($"[JosefifyAllBusts] Josef-ified {josefCount} busts, {exceptionCount} exceptions, {skippedJosef} Josef busts skipped");
        }

        /// <summary>
        /// Copies Josef's Blink, Mouth, and Expressions sprites onto a bust's mBase children.
        /// Creates the children if they don't exist. If flipFacial is true, flips Blink/Mouth/Expressions localScale.x.
        /// </summary>
        private static void CopyJosefFacialSprites(Transform mBase, Sprite blinkSprite, Sprite[] mouthSprites,
            Sprite happySprite, Sprite angrySprite, Sprite sadSprite, Sprite flirtySprite, bool flipFacial)
        {
            // Blink
            Transform blink = mBase.Find("Blink");
            if (blink != null && blinkSprite != null)
            {
                SpriteRenderer blinkSR = blink.GetComponent<SpriteRenderer>();
                if (blinkSR != null) blinkSR.sprite = blinkSprite;
                if (flipFacial) FlipLocalScaleX(blink);
            }

            // Mouth children (1-4)
            Transform mouth = mBase.Find("Mouth");
            if (mouth != null)
            {
                for (int m = 0; m < 4; m++)
                {
                    if (mouthSprites[m] == null) continue;
                    Transform mouthChild = mouth.Find((m + 1).ToString());
                    if (mouthChild != null)
                    {
                        SpriteRenderer mSR = mouthChild.GetComponent<SpriteRenderer>();
                        if (mSR != null) mSR.sprite = mouthSprites[m];
                    }
                }
                if (flipFacial) FlipLocalScaleX(mouth);
            }

            // Expressions children (Happy, Angry, Sad, Flirty)
            Transform expressions = mBase.Find("Expressions");
            if (expressions != null)
            {
                if (happySprite != null) { Transform t = expressions.Find("Happy"); if (t != null) { SpriteRenderer s = t.GetComponent<SpriteRenderer>(); if (s != null) s.sprite = happySprite; } }
                if (angrySprite != null) { Transform t = expressions.Find("Angry"); if (t != null) { SpriteRenderer s = t.GetComponent<SpriteRenderer>(); if (s != null) s.sprite = angrySprite; } }
                if (sadSprite != null) { Transform t = expressions.Find("Sad"); if (t != null) { SpriteRenderer s = t.GetComponent<SpriteRenderer>(); if (s != null) s.sprite = sadSprite; } }
                if (flirtySprite != null) { Transform t = expressions.Find("Flirty"); if (t != null) { SpriteRenderer s = t.GetComponent<SpriteRenderer>(); if (s != null) s.sprite = flirtySprite; } }
                if (flipFacial) FlipLocalScaleX(expressions);
            }
        }

        private static void FlipLocalScaleX(Transform t)
        {
            Vector3 scale = t.localScale;
            scale.x = -scale.x;
            t.localScale = scale;
        }

        #endregion

        #region Josef-ify Scenes

        private const string josefSceneTexture = "Textures.Scene.Josefy.png";

        /// <summary>
        /// Replaces the Art sprite of every CG scene, plus special scenes and Compareit avatars, with Josefy.png.
        /// </summary>
        public static void JosefifyAllScenes()
        {
            Sprite josefSprite = LoadEmbeddedSprite(josefSceneTexture);
            if (josefSprite == null)
            {
                Debug.LogError("[JosefifyAllScenes] Failed to load Josefy sprite");
                return;
            }

            int count = 0;

            // --- CG Manager scenes (SpriteRenderer on Core > Art) ---
            string[] cgManagerNames = new string[]
            {
                "4_CG_Manager-PrivatePhotos",
                "4_CG_Manager-Sexy",
                "4_CG_Manager-Photos",
                "4_CG_Manager-Sexy_Two",
                "4_CG_Manager-Sexy_Three",
                "4_CG_Manager-Sexy_Four",
                "5_CG_EndingArt"
            };

            foreach (string managerName in cgManagerNames)
            {
                GameObject managerGO = GameObject.Find(managerName);
                if (managerGO == null) continue;

                Transform manager = managerGO.transform;
                for (int i = 0; i < manager.childCount; i++)
                {
                    Transform scene = manager.GetChild(i);
                    Transform core = scene.Find("Core");
                    if (core == null) continue;
                    Transform art = core.Find("Art");
                    if (art == null) continue;
                    SpriteRenderer sr = art.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = josefSprite;
                        count++;
                    }
                }
            }

            // --- Compareit avatars (Image component) ---
            GameObject minigamesGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "16_Minigames" && go.scene.name != null);
            if (minigamesGO != null)
            {
                Transform avatarsFolder = minigamesGO.transform.Find("Compareit");
                if (avatarsFolder != null) avatarsFolder = avatarsFolder.Find("Avatars");
                if (avatarsFolder != null)
                {
                    for (int i = 0; i < avatarsFolder.childCount; i++)
                    {
                        GameObject avatarGO = avatarsFolder.GetChild(i).gameObject;
                        if (avatarGO.name == "Empty") continue;
                        UnityEngine.UI.Image img = avatarGO.GetComponent<UnityEngine.UI.Image>();
                        if (img != null) { img.sprite = josefSprite; count++; }
                    }
                }
            }

            // --- Cinema movie frames (SpriteRenderer) ---
            GameObject levelsGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "5_Levels" && go.scene.name != null);
            if (levelsGO != null)
            {
                Transform cinema = levelsGO.transform.Find("122_Cinema");
                if (cinema != null)
                {
                    Transform darkCinema = cinema.Find("Dark_Cinema");
                    if (darkCinema != null)
                    {
                        Transform movieFrames = darkCinema.Find("Cinema_MovieFrames");
                        if (movieFrames != null)
                        {
                            for (int i = 0; i < movieFrames.childCount; i++)
                            {
                                SpriteRenderer sr = movieFrames.GetChild(i).GetComponent<SpriteRenderer>();
                                if (sr != null) { sr.sprite = josefSprite; count++; }
                            }
                        }
                    }
                }
            }

            // --- EventUI special scenes (Image component) ---
            GameObject eventUIGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "15_EventUI" && go.scene.name != null);
            if (eventUIGO != null)
            {
                count += JosefifyEventUIImages(eventUIGO.transform, josefSprite);
            }

            // --- MainCanvas special scenes (Image component) ---
            GameObject mainCanvasGO = GameObject.Find("9_MainCanvas");
            if (mainCanvasGO != null)
            {
                count += JosefifyMainCanvasImages(mainCanvasGO.transform, josefSprite);
            }

            Debug.Log($"[JosefifyAllScenes] Replaced {count} scene sprites with Josef");
        }

        private static void SetImageSprite(Transform parent, string childName, Sprite sprite, ref int count)
        {
            Transform t = parent.Find(childName);
            if (t == null) return;
            UnityEngine.UI.Image img = t.GetComponent<UnityEngine.UI.Image>();
            if (img != null) { img.sprite = sprite; count++; }
        }

        private static int JosefifyEventUIImages(Transform eventUI, Sprite josefSprite)
        {
            int count = 0;

            // BPC_New messenger pics
            Transform bpc = eventUI.Find("BPC_New");
            if (bpc != null)
            {
                Transform msgList = bpc.Find("OtherPC_Messenger")?.Find("OtherPC_Core")?.Find("OtherPC_MSGList");
                if (msgList != null)
                {
                    if (msgList.childCount > 11)
                    {
                        Transform pic = msgList.GetChild(11)?.Find("OtherPC_Pic");
                        if (pic != null) { var img = pic.GetComponent<UnityEngine.UI.Image>(); if (img != null) { img.sprite = josefSprite; count++; } }
                    }
                    Transform img5 = msgList.Find("Image (5)");
                    if (img5 != null)
                    {
                        Transform pic = img5.Find("OtherPC_Pic");
                        if (pic != null) { var img = pic.GetComponent<UnityEngine.UI.Image>(); if (img != null) { img.sprite = josefSprite; count++; } }
                    }
                }
            }

            // DPC_New images
            Transform dpc = eventUI.Find("DPC_New");
            if (dpc != null)
            {
                // MImages > List
                Transform list = dpc.Find("MImages")?.Find("List");
                if (list != null)
                {
                    SetImageSprite(list, "Image", josefSprite, ref count);
                    SetImageSprite(list, "Image (3)", josefSprite, ref count);
                }
                // MImages (1) > List
                Transform list1 = dpc.Find("MImages (1)")?.Find("List");
                if (list1 != null)
                {
                    foreach (string n in new[] { "Image", "Image (1)", "Image (2)", "Image (3)", "Image (4)", "Image (5)" })
                        SetImageSprite(list1, n, josefSprite, ref count);
                }
                // DPC messenger pic
                Transform dpcMsgList = dpc.Find("OtherPC_Messenger")?.Find("OtherPC_Core")?.Find("OtherPC_MSGList");
                if (dpcMsgList != null)
                {
                    Transform imgT = dpcMsgList.Find("Image");
                    if (imgT != null)
                    {
                        Transform pic = imgT.Find("OtherPC_Pic");
                        if (pic != null) { var img = pic.GetComponent<UnityEngine.UI.Image>(); if (img != null) { img.sprite = josefSprite; count++; } }
                    }
                }
            }

            // PrismeMagazine1
            SetImageSprite(eventUI, "PrismeMagazine1", josefSprite, ref count);

            return count;
        }

        private static int JosefifyMainCanvasImages(Transform mainCanvas, Sprite josefSprite)
        {
            int count = 0;

            // Payout > CelesteStoleFollowers > CelesteStoleFollowers (1)
            Transform celeste = mainCanvas.Find("Payout")?.Find("CelesteStoleFollowers")?.Find("CelesteStoleFollowers (1)");
            if (celeste != null) { var img = celeste.GetComponent<UnityEngine.UI.Image>(); if (img != null) { img.sprite = josefSprite; count++; } }

            // Desktop > Cats > Image
            Transform catsImage = mainCanvas.Find("Desktop")?.Find("Cats")?.Find("Image");
            if (catsImage != null) { var img = catsImage.GetComponent<UnityEngine.UI.Image>(); if (img != null) { img.sprite = josefSprite; count++; } }

            // Desktop > Desktop_SpamAds > DesAds_List > DesAd variants
            Transform desAdsList = mainCanvas.Find("Desktop")?.Find("Desktop_SpamAds")?.Find("DesAds_List");
            if (desAdsList != null)
            {
                Transform desAd0Win = desAdsList.Find("DesAd (0)")?.Find("DesWin");
                if (desAd0Win != null) SetImageSprite(desAd0Win, "Image (1)", josefSprite, ref count);
                for (int i = 1; i <= 24; i++)
                {
                    Transform desWin = desAdsList.Find($"DesAd ({i})")?.Find("DesWin");
                    if (desWin != null) SetImageSprite(desWin, "img", josefSprite, ref count);
                }
            }

            // Wallpaperselection > UI_Core > List > children
            Transform wpList = mainCanvas.Find("Wallpaperselection")?.Find("UI_Core")?.Find("List");
            if (wpList != null)
            {
                for (int i = 0; i < wpList.childCount; i++)
                {
                    UnityEngine.UI.Image img = wpList.GetChild(i).GetComponent<UnityEngine.UI.Image>();
                    if (img != null) { img.sprite = josefSprite; count++; }
                }
            }

            // Starmaker > Browse_Creators > Creator_Windows > CW_* > Unlocked > Creator_Pictures > Scrollarea > Creator_PicsContent > Images
            Transform creatorWindows = mainCanvas.Find("Starmaker")?.Find("Browse_Creators")?.Find("Creator_Windows");
            if (creatorWindows != null)
            {
                var creators = new Dictionary<string, string[]>
                {
                    { "CW_Celeste",    new[] { "Image", "Image (1)", "Image (2)", "Image (3)", "Image (4)" } },
                    { "CW_MsUmbra",    new[] { "Image", "Image (1)", "Image (2)", "Image (3)" } },
                    { "CW_DreamyWisp", new[] { "Image", "Image (1)", "Image (2)", "Image (3)", "Image (4)", "Image (5)", "Image (6)", "Image (7)", "Image (8)" } },
                    { "CW_Rose",       new[] { "Image (2)", "Image (5)", "Image (6)", "Image (7)", "Image (8)" } }
                };
                foreach (var kvp in creators)
                {
                    Transform picContent = creatorWindows.Find(kvp.Key)?.Find("Unlocked")?.Find("Creator_Pictures")?.Find("Scrollarea")?.Find("Creator_PicsContent");
                    if (picContent != null)
                    {
                        foreach (string imgName in kvp.Value)
                            SetImageSprite(picContent, imgName, josefSprite, ref count);
                    }
                }
            }

            return count;
        }

        #endregion

        #region Josef-ify NPCs

        private const string josefNpcTexture = "Textures.Bust.Tabletop_Josef.png";

        /// <summary>
        /// Replaces all NPC textures (Livestream, Sword, Milking, Sprite_Overlay, Convention, Stage, Bafa, Tabletop, X_Movies, EventUI Photos, Starmaker Creators) with Tabletop_Josef.png.
        /// </summary>
        public static void JosefifyAllNPCs()
        {
            Sprite josefSprite = LoadEmbeddedSprite(josefNpcTexture);
            if (josefSprite == null)
            {
                Debug.LogError("[JosefifyAllNPCs] Failed to load Tabletop_Josef sprite");
                return;
            }

            int count = 0;

            // --- Livestream NPCs (SpriteRenderer) ---
            GameObject livestreamGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "Livestream_Active" && go.scene.name != null);
            if (livestreamGO != null)
            {
                Transform liveAnna = livestreamGO.transform.Find("Live_Anna");
                if (liveAnna != null)
                {
                    foreach (string outfit in new[] { "Live_DefaultOutfit", "Live_CoolOutfit", "Live_SlutOutfit" })
                    {
                        Transform outfitT = liveAnna.Find(outfit);
                        if (outfitT == null) continue;
                        for (int i = 0; i < outfitT.childCount; i++)
                        {
                            SpriteRenderer sr = outfitT.GetChild(i).GetComponent<SpriteRenderer>();
                            if (sr != null && sr.sprite != null) { sr.sprite = josefSprite; count++; }
                        }
                    }
                }
            }

            // --- Sword Minigame NPCs (SpriteRenderer) ---
            GameObject swordGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "Sword_Minigame" && go.scene.name != null);
            if (swordGO != null)
            {
                Transform sprites = swordGO.transform.Find("Enemy")?.Find("Sprites");
                if (sprites != null)
                {
                    for (int i = 0; i < sprites.childCount; i++)
                    {
                        SpriteRenderer sr = sprites.GetChild(i).GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null) { sr.sprite = josefSprite; count++; }
                    }
                }
            }

            // --- Milking Minigame NPCs (SpriteRenderer on all variants) ---
            GameObject milkingGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "MilkingMinigame" && go.scene.name != null);
            if (milkingGO != null)
            {
                Transform list = milkingGO.transform.Find("List");
                if (list != null)
                {
                    for (int i = 0; i < list.childCount; i++)
                    {
                        Transform charFolder = list.GetChild(i);
                        // Default folder: DefaultFace, MilkedFace
                        Transform defaultFolder = charFolder.Find("Default");
                        if (defaultFolder != null)
                        {
                            count += ReplaceSpriteRendererChildren(defaultFolder, josefSprite);
                        }
                        // Squeeze folder: main + secondary children (RightBreast, LeftBreast, etc.)
                        Transform squeezeFolder = charFolder.Find("Squeeze");
                        if (squeezeFolder != null)
                        {
                            SpriteRenderer sqSR = squeezeFolder.GetComponent<SpriteRenderer>();
                            if (sqSR != null && sqSR.sprite != null) { sqSR.sprite = josefSprite; count++; }
                            count += ReplaceSpriteRendererChildren(squeezeFolder, josefSprite);
                        }
                    }
                }
            }

            // --- Sprite_Overlay NPCs (SpriteRenderer) ---
            GameObject spriteOverlayGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "Sprite_Overlay" && go.scene.name != null);
            if (spriteOverlayGO != null)
            {
                // Weightlifting
                Transform wl = spriteOverlayGO.transform.Find("Weightlifting");
                if (wl != null) count += ReplaceSpriteRendererChildren(wl, josefSprite);
                // Sam_Gamingnight_Loading
                Transform sg = spriteOverlayGO.transform.Find("Sam_Gamingnight_Loading");
                if (sg != null) count += ReplaceSpriteRendererChildren(sg, josefSprite);
            }

            // --- Convention NPCs (SpriteRenderer) ---
            GameObject levelsGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "5_Levels" && go.scene.name != null);
            if (levelsGO != null)
            {
                Transform convention = levelsGO.transform.Find("54_Convention");
                if (convention != null)
                {
                    for (int i = 0; i < convention.childCount; i++)
                    {
                        string childName = convention.GetChild(i).gameObject.name;
                        if (childName.StartsWith("Convention_NPC_1") || childName.StartsWith("Convenation_Ad_1"))
                        {
                            SpriteRenderer sr = convention.GetChild(i).GetComponent<SpriteRenderer>();
                            if (sr != null && sr.sprite != null) { sr.sprite = josefSprite; count++; }
                        }
                    }
                }

                // --- Stage NPCs (SpriteRenderer) ---
                Transform stage = levelsGO.transform.Find("55_Stage")?.Find("AnnaOnStage_Sprite");
                if (stage != null)
                {
                    foreach (string npcName in new[] { "AnnaOnStage_0", "AnnaOnStage_0 (1)", "AnnaOnStage_0 (2)" })
                    {
                        Transform t = stage.Find(npcName);
                        if (t != null)
                        {
                            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
                            if (sr != null && sr.sprite != null) { sr.sprite = josefSprite; count++; }
                        }
                    }
                }
            }

            // --- Bafa NPCs (SpriteRenderer + secondary children) ---
            GameObject bafaGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "Bafa_Sprites" && go.scene.name != null);
            if (bafaGO != null)
            {
                for (int i = 0; i < bafaGO.transform.childCount; i++)
                {
                    Transform npc = bafaGO.transform.GetChild(i);
                    if (npc.gameObject.name == "0_Empty") continue;
                    SpriteRenderer sr = npc.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.sprite != null) { sr.sprite = josefSprite; count++; }
                    // Secondary sprites (Bafa_Boobs, Bafa_Boobs (1), Bafa_Dick)
                    count += ReplaceSpriteRendererChildren(npc, josefSprite);
                }
            }

            // --- Tabletop Minigame NPCs (Image on Char_Sprite) ---
            GameObject tabletopGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "Tabletop_Minigame" && go.scene.name != null);
            if (tabletopGO != null)
            {
                Transform tabletopCanvas = tabletopGO.transform.Find("Tabletop_Canvas");
                if (tabletopCanvas != null)
                {
                    foreach (string groupName in new[] { "Tabletop Ally Fighters", "Tabletop Enemy Fighters" })
                    {
                        Transform group = tabletopCanvas.Find(groupName);
                        if (group == null) continue;
                        for (int i = 0; i < group.childCount; i++)
                        {
                            Transform fighter = group.GetChild(i);
                            if (fighter.gameObject.name == "0_empty") continue;
                            Transform charSprite = fighter.Find("Char_Sprite");
                            if (charSprite != null)
                            {
                                UnityEngine.UI.Image img = charSprite.GetComponent<UnityEngine.UI.Image>();
                                if (img != null && img.sprite != null) { img.sprite = josefSprite; count++; }
                            }
                        }
                    }
                }
            }

            // --- X_Movies NPCs (SpriteRenderer) ---
            GameObject xMoviesGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "X_Movies" && go.scene.name != null);
            if (xMoviesGO != null)
            {
                var movieNpcs = new (string movie, string slide, string npc)[]
                {
                    ("Scifi_Movie", "Slide_1", "M_scifi01"), ("Scifi_Movie", "Slide_1", "M_scifi01 (1)"),
                    ("Scifi_Movie", "Slide_2", "M_scifi05"), ("Scifi_Movie", "Slide_2", "M_scifi05 (1)"),
                    ("Scifi_Movie", "Slide_2", "M_scifi05 (2)"), ("Scifi_Movie", "Slide_2", "M_scifi05 (3)"),
                    ("Scifi_Movie", "Slide_2", "M_scifi05 (4)"), ("Scifi_Movie", "Slide_2", "M_scifi05 (5)"),
                    ("News_Movie", "News_Screen", "News_0"), ("News_Movie", "News_Screen", "News_1"),
                    ("News_Movie", "News_Screen", "News_2"), ("News_Movie", "News_Screen", "News_3"),
                    ("News_Movie", "News_Screen", "News_4"), ("News_Movie", "News_Screen", "News_5"),
                    ("News_Movie", "News_Screen", "News_7"), ("News_Movie", "News_Screen", "News_8"),
                    ("Fantasy_Movie", "Slide1", "M_Fantasy3"), ("Fantasy_Movie", "Slide2", "M_Fantasy3"),
                    ("Fantasy_Movie", "Slide3", "M_Fantasy3"),
                    ("Adult_Movie", "Slide1", "M_Fantasy3"), ("Adult_Movie", "Slide2", "M_Fantasy3"),
                    ("Adult_Movie", "Slide3", "M_Fantasy3")
                };
                foreach (var (movie, slide, npc) in movieNpcs)
                {
                    Transform t = xMoviesGO.transform.Find(movie)?.Find(slide)?.Find(npc);
                    if (t != null)
                    {
                        SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null) { sr.sprite = josefSprite; count++; }
                    }
                }
            }

            // --- EventUI Photo NPCs (Image) ---
            GameObject eventUIGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "15_EventUI" && go.scene.name != null);
            if (eventUIGO != null)
            {
                Transform bpcNew = eventUIGO.transform.Find("BPC_New");
                if (bpcNew != null)
                {
                    // AdrianPC_Wor > GameObject > Image, Image (1), Image (2)
                    Transform adrianGO = bpcNew.Find("AdrianPC_Wor")?.Find("GameObject");
                    if (adrianGO != null)
                    {
                        foreach (string imgName in new[] { "Image", "Image (1)", "Image (2)" })
                        {
                            Transform t = adrianGO.Find(imgName);
                            if (t != null)
                            {
                                UnityEngine.UI.Image img = t.GetComponent<UnityEngine.UI.Image>();
                                if (img != null && img.sprite != null) { img.sprite = josefSprite; count++; }
                            }
                        }
                    }
                    // OtherPC_Browser > OtherPC_Core > GameObject (4)
                    Transform go4 = bpcNew.Find("OtherPC_Browser")?.Find("OtherPC_Core")?.Find("GameObject (4)");
                    if (go4 != null)
                    {
                        UnityEngine.UI.Image img = go4.GetComponent<UnityEngine.UI.Image>();
                        if (img != null && img.sprite != null) { img.sprite = josefSprite; count++; }
                    }
                }
            }

            // --- Starmaker Creator NPCs (Image) ---
            GameObject mainCanvasGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "9_MainCanvas" && go.scene.name != null);
            if (mainCanvasGO != null)
            {
                Transform creatorList = mainCanvasGO.transform.Find("Starmaker")?.Find("Browse_Creators")?.Find("CreatorList");
                if (creatorList != null)
                {
                    foreach (string creatorFolder in new[] { "Creator_MsUmbra", "Creator_DreamyWisp", "Creator_Rose" })
                    {
                        Transform imgT = creatorList.Find(creatorFolder)?.Find("Image");
                        if (imgT != null)
                        {
                            UnityEngine.UI.Image img = imgT.GetComponent<UnityEngine.UI.Image>();
                            if (img != null && img.sprite != null) { img.sprite = josefSprite; count++; }
                        }
                    }
                }
            }

            // --- 5_Levels NPCs: every child's "NPCs" folder, recursively (SpriteRenderer) ---
            if (levelsGO == null)
            {
                levelsGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                    go => go.name == "5_Levels" && go.scene.name != null);
            }
            if (levelsGO != null)
            {
                for (int i = 0; i < levelsGO.transform.childCount; i++)
                {
                    Transform levelChild = levelsGO.transform.GetChild(i);
                    Transform npcsFolder = levelChild.Find("NPCs");
                    if (npcsFolder != null)
                    {
                        count += ReplaceSpriteRendererRecursive(npcsFolder, josefSprite);
                    }
                    // Special case NPCs (not under NPCs folder)
                    count += ReplaceSpecialCaseNpcs(levelChild, josefSprite);
                }
            }

            // --- Ghost Minigame NPCs (SpriteRenderer) ---
            GameObject ghostGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "Ghost_Minigame" && go.scene.name != null);
            if (ghostGO != null)
            {
                Transform ghostMapCore = ghostGO.transform.Find("Ghost_Map_Core");
                if (ghostMapCore != null)
                {
                    // AstridLooking > Asgo_Astrid_Looking_0* variants
                    Transform astridLooking = ghostMapCore.Find("AstridLooking");
                    if (astridLooking != null)
                    {
                        for (int i = 0; i < astridLooking.childCount; i++)
                        {
                            Transform child = astridLooking.GetChild(i);
                            if (child.name.StartsWith("Asgo_Astrid_Looking_0"))
                            {
                                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                                if (sr != null && sr.sprite != null) { sr.sprite = josefSprite; count++; }
                            }
                        }
                    }
                    // ghost_Monsters > Noire_Minigame_1
                    Transform noireM = ghostMapCore.Find("ghost_Monsters")?.Find("Noire_Minigame_1");
                    if (noireM != null)
                    {
                        SpriteRenderer sr = noireM.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null) { sr.sprite = josefSprite; count++; }
                    }
                    // Noire_Jumpscare
                    Transform noireJ = ghostMapCore.Find("Noire_Jumpscare");
                    if (noireJ != null)
                    {
                        SpriteRenderer sr = noireJ.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null) { sr.sprite = josefSprite; count++; }
                    }
                }
            }

            // --- Camcorder Minigame NPCs (SpriteRenderer) ---
            GameObject camcorderGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "Camcorder_Minigame" && go.scene.name != null);
            if (camcorderGO != null)
            {
                foreach (string camType in new[] { "Camcorder_Bigfoot", "Camcorder_Lizard" })
                {
                    Transform camFolder = camcorderGO.transform.Find(camType);
                    if (camFolder == null) continue;
                    // Circle targets
                    Transform targets = camFolder.Find("Camcorder_Targets");
                    if (targets != null)
                    {
                        for (int i = 0; i < targets.childCount; i++)
                        {
                            Transform child = targets.GetChild(i);
                            if (child.name.StartsWith("Circle"))
                            {
                                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                                if (sr != null && sr.sprite != null) { sr.sprite = josefSprite; count++; }
                            }
                        }
                    }
                    // Camcorder_Sprite_Emma1
                    Transform emma = camFolder.Find("Camcorder_Sprite_Emma1");
                    if (emma != null)
                    {
                        SpriteRenderer sr = emma.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null) { sr.sprite = josefSprite; count++; }
                    }
                }
            }

            // --- Racing Minigame NPCs (SpriteRenderer) ---
            GameObject racingGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "Racing_Minigame" && go.scene.name != null);
            if (racingGO != null)
            {
                Transform racingBody = racingGO.transform.Find("Racing_PC")?.Find("RacingBody");
                if (racingBody != null)
                {
                    // PCRacing1
                    Transform pcRacing = racingBody.Find("Racing_BikeTypes")?.Find("Racing_PCAlone")?.Find("PCRacing1");
                    if (pcRacing != null)
                    {
                        SpriteRenderer sr = pcRacing.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null) { sr.sprite = josefSprite; count++; }
                    }
                    // Racing_Guest children
                    Transform guestGroup = racingBody.Find("Racing_Guest");
                    if (guestGroup != null)
                    {
                        for (int i = 0; i < guestGroup.childCount; i++)
                        {
                            SpriteRenderer sr = guestGroup.GetChild(i).GetComponent<SpriteRenderer>();
                            if (sr != null && sr.sprite != null) { sr.sprite = josefSprite; count++; }
                        }
                    }
                }
            }

            // --- Barista Game NPCs (Image) ---
            GameObject baristaGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "Barista_Game" && go.scene.name != null);
            if (baristaGO != null)
            {
                // BuyMenu children > Image (1)
                Transform buyMenu = baristaGO.transform.Find("Canvas")?.Find("Continue")?.Find("BuyMenu");
                if (buyMenu != null)
                {
                    for (int i = 0; i < buyMenu.childCount; i++)
                    {
                        Transform child = buyMenu.GetChild(i);
                        if (child.gameObject.name == "0_empty") continue;
                        Transform img1 = child.Find("Image (1)");
                        if (img1 != null)
                        {
                            UnityEngine.UI.Image img = img1.GetComponent<UnityEngine.UI.Image>();
                            if (img != null && img.sprite != null) { img.sprite = josefSprite; count++; }
                        }
                    }
                }
                // FreeGob2 special NPC
                Transform freeGob2Content = baristaGO.transform.Find("Canvas")?.Find("FreePerk")?.Find("CardsLeft")?.Find("FreeGob2 (4)")?.Find("Card")?.Find("Content");
                if (freeGob2Content != null)
                {
                    UnityEngine.UI.Image img = freeGob2Content.GetComponent<UnityEngine.UI.Image>();
                    if (img != null && img.sprite != null) { img.sprite = josefSprite; count++; }
                }
            }

            // --- Bath Mirror (MirrorPC.png) ---
            if (levelsGO != null)
            {
                Transform bath = levelsGO.transform.Find("4_Bath");
                if (bath != null)
                {
                    Transform square = bath.Find("BathB")?.Find("Square");
                    if (square != null)
                    {
                        SpriteRenderer sr = square.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            Sprite mirrorSprite = LoadEmbeddedSprite("Textures.Bust.MirrorPC.png");
                            if (mirrorSprite != null) { sr.sprite = mirrorSprite; count++; }
                        }
                    }
                }
            }

            Debug.Log($"[JosefifyAllNPCs] Replaced {count} NPC sprites with Josef");
        }

        /// <summary>
        /// Replaces main menu character sprites and their masks with Josef versions.
        /// Only called after returning to the menu from the game.
        /// </summary>
        public static void JosefifyMainMenu()
        {
            GameObject partOne = GameObject.Find("Part_One");
            if (partOne == null) return;

            Transform canvas = partOne.transform.Find("TheCityBackground")?.Find("Canvas");
            if (canvas == null) return;

            // Mapping: parent Image name -> (NMM_C_ number, child under Image (3))
            var menuCharacters = new (string parentName, int nmmNumber)[]
            {
                ("Image (1)", 8),
                ("Image (3)", 2),
                ("Image (4)", 1),
                ("Image (5)", 3),
                ("Image (6)", 4),
                ("Image (7)", 6),
                ("Image (8)", 5),
                ("Image (9)", 7),
            };

            int count = 0;
            foreach (var (parentName, nmmNumber) in menuCharacters)
            {
                Transform parent = canvas.Find(parentName);
                if (parent == null) continue;

                // The character sprite is on Image (3)'s Image component (sprite named NMM_C_X)
                Transform imageThree = parent.Find("Image (3)");
                if (imageThree != null)
                {
                    UnityEngine.UI.Image charImg = imageThree.GetComponent<UnityEngine.UI.Image>();
                    if (charImg != null)
                    {
                        Sprite charSprite = LoadEmbeddedSprite($"Textures.Bust.NMM_C_{nmmNumber}.png");
                        if (charSprite != null) { charImg.sprite = charSprite; count++; }

                        // Set mask via shader property on material instance (UI/JiggleCanvas)
                        Texture2D maskTex = LoadEmbeddedTexture($"Textures.Bust.NMM_C_{nmmNumber}Mask.png");
                        if (maskTex != null)
                        {
                            charImg.material = new Material(charImg.material);
                            charImg.material.SetTexture("_MaskTex", maskTex);
                        }
                    }
                }
            }

            // Add the menu logo image under MainMenu > Image
            Transform mainMenuImage = partOne.transform.Find("Canvas_MM")?.Find("MainMenu")?.Find("Image");
            if (mainMenuImage != null)
            {
                Sprite logoSprite = LoadEmbeddedSprite("Textures.UI.MenuLogo 1.png");
                if (logoSprite != null)
                {
                    GameObject logoGO = new GameObject("JosefMenuLogo");
                    logoGO.transform.SetParent(mainMenuImage, false);
                    UnityEngine.UI.Image logoImg = logoGO.AddComponent<UnityEngine.UI.Image>();
                    logoImg.sprite = logoSprite;
                    logoImg.SetNativeSize();
                    logoGO.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                    logoGO.SetActive(true);
                }
            }

            Debug.Log($"[JosefifyMainMenu] Replaced {count} main menu character sprites");
        }

        /// <summary>
        /// Recursively replaces SpriteRenderer sprites on all descendants that have one.
        /// Mirrors the CollectNpcsRecursive pattern: if a node has a SpriteRenderer, replace it and stop recursing.
        /// </summary>
        private static int ReplaceSpriteRendererRecursive(Transform parent, Sprite sprite)
        {
            int count = 0;
            SpriteRenderer sr = parent.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                sr.sprite = sprite;
                count++;
                // Don't recurse into children of an NPC (matches CollectNpcsRecursive behavior)
                return count;
            }
            for (int i = 0; i < parent.childCount; i++)
            {
                count += ReplaceSpriteRendererRecursive(parent.GetChild(i), sprite);
            }
            return count;
        }

        /// <summary>
        /// Replaces sprites on special-case NPCs that don't live under the "NPCs" folder.
        /// </summary>
        private static int ReplaceSpecialCaseNpcs(Transform levelsParent, Sprite sprite)
        {
            int count = 0;
            string levelName = levelsParent.gameObject.name;

            var specialCasesByLevel = new Dictionary<string, string[][]>()
            {
                { "4_Bath", new[] { new[] { "AnnaInShower" }, new[] { "AnnaInBathtubSprite" }, new[] { "BathB", "Square (1)" } } },
                { "17_CarPark", new[] { new[] { "ToniWorking1" } } },
                { "28_Upstairs", new[] { new[] { "AdrianAtDoorSprite" } } },
                { "33_Club", new[] { new[] { "NPC (1)" }, new[] { "NPC (2)" }, new[] { "NPC (3)" }, new[] { "NPC (4)" }, new[] { "NPC (5)" } } },
                { "76_Villa_Lab", new[] { new[] { "Androidfloat" } } },
                { "80_Shrine", new[] { new[] { "Nyx25_PhoneSprite1" } } },
                { "82_Abandonedshackinside", new[] { new[] { "SofiaRestrained" } } },
                { "85_HospitalHallway", new[] { new[] { "hospital1" }, new[] { "hospital1 (1)" }, new[] { "Hospital2" } } },
                { "90_JapaneseOldStreet", new[] { new[] { "NPC (8)" }, new[] { "NPC (9)" } } },
                { "92_JapanesePlaza", new[] { new[] { "NPC (3)" }, new[] { "NPC (4)" }, new[] { "NPC (5)" }, new[] { "NPC (6)" } } },
                { "95_JapaneseSubwaytrain", new[] { new[] { "NPC (6)" }, new[] { "NPC (8)" }, new[] { "NPC (9)" } } },
                { "100_TropicalPool", new[] { new[] { "NPC" }, new[] { "NPC (1)" }, new[] { "NPC (2)" }, new[] { "NPC (3)" } } },
                { "101_TropicalClub", new[] { new[] { "NPC" }, new[] { "NPC (1)" }, new[] { "NPC (2)" }, new[] { "NPC (3)" } } },
                { "103_WinterVillageOutside", new[] { new[] { "NPC (1)" }, new[] { "NPC (2)" } } },
                { "104_WinterHotspring", new[] { new[] { "NPC (2)" }, new[] { "NPC (3)" }, new[] { "NPC (4)" }, new[] { "NPC (5)" }, new[] { "NPC (6)" }, new[] { "AnnaAndPlayer", "NPC (7)" }, new[] { "AnnaAndPlayer", "NPC (8)" } } },
                { "106_BeerTent", new[] { new[] { "NPC (2)" }, new[] { "NPC (3)" }, new[] { "NPC (4)" } } },
                { "110_BadlandsParkingLot", new[] { new[] { "ParkingLotNPCs", "NPC (8)" }, new[] { "ParkingLotNPCs", "NPC (9)" }, new[] { "ParkingLotNPCs", "NPC (10)" }, new[] { "ParkingLotNPCs", "NPC (11)" }, new[] { "ParkingLotNPCs", "NPC (12)" }, new[] { "ParkingLotNPCs", "NPC (13)" }, new[] { "ParkingLotNPCs", "NPC (14)" } } },
                { "112_GasStationInterior", new[] { new[] { "VampireReachesUp" } } },
                { "119_AirDuct", new[] { new[] { "CharlotteVent1" } } },
                { "121_DiningRoomHome", new[] { new[] { "Dining_Sprites", "Dining_AnnaSitSprite" }, new[] { "Dining_Sprites", "Dining_AnnaSitSprite_Nude" }, new[] { "Dining_Sprites", "Dining_AnnaSitSprite_NudeWithCum" }, new[] { "Dining_Sprites", "Dining_JosefSitSprite" }, new[] { "Dining_Sprites", "Dining_AdrianSitSprite" }, new[] { "Dining_Sprites", "Dining_AdrianSitSprite_NoPhone" } } },
                { "123_CharlotteHalloweenHome", new[] { new[] { "HalloweenPartyGuest1" }, new[] { "HalloweenPartyGuest1 (1)" }, new[] { "HalloweenPartyGuest1 (2)" } } },
                { "133_HarborDistrict", new[] { new[] { "137HarborDistrict_O1", "Harbor_PoBa_01" }, new[] { "137HarborDistrict_O1", "Harbor_PoBa_01 (1)" }, new[] { "137HarborDistrict_O1", "Harbor_PoBa_01 (2)" } } },
                { "136_HarborScifiPrison", new[] { new[] { "MaleDocMura_Sitting" } } },
                { "137_HarborHauntedHouse", new[] { new[] { "Harbor_PoBa_01" }, new[] { "Harbor_PoBa_01 (1)" }, new[] { "Harbor_PoBa_01 (2)" } } },
                { "142_NightDistrict", new[] { new[] { "Nightclub_Sign_2" }, new[] { "Nightclub_Sign_2 (1)" }, new[] { "Nightclub_Sign_2 (2)" } } }
            };

            if (!specialCasesByLevel.ContainsKey(levelName)) return count;

            foreach (string[] pathArray in specialCasesByLevel[levelName])
            {
                Transform current = levelsParent;
                foreach (string segment in pathArray)
                {
                    current = current.Find(segment);
                    if (current == null) break;
                }
                if (current != null)
                {
                    SpriteRenderer sr = current.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.sprite != null) { sr.sprite = sprite; count++; }
                }
            }
            return count;
        }

        private static int ReplaceSpriteRendererChildren(Transform parent, Sprite sprite)
        {
            int count = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                SpriteRenderer sr = parent.GetChild(i).GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null) { sr.sprite = sprite; count++; }
            }
            return count;
        }

        #endregion
    }

    [HarmonyPatch(typeof(Actor), nameof(Actor.GetName))]
    static class Patch_ActorGetName
    {
        static void Postfix(ref string __result)
        {
            if (Dialogue.Current != null)
            {
                __result = "Josef";
            }
        }
    }

    [HarmonyPatch(typeof(Typewriter), nameof(Typewriter.GetGibberish))]
    static class Patch_TypewriterGetGibberish
    {
        static void Postfix(ref AudioClip __result)
        {
            if (Core.josefGibberishClip != null)
            {
                __result = Core.josefGibberishClip;
            }
        }
    }

    [HarmonyPatch(typeof(NodeText), nameof(NodeText.Get))]
    static class Patch_NodeTextGet
    {
        // Sorted longest-first to avoid partial matches (e.g. "Samantha" before "Sam")
        static readonly string[] characterNames = new string[]
        {
            "Doctor Frost", "Samantha", "Charlotte", "Isabella", "Katarina",
            "Rosanna", "Nyxara", "Dorothy", "Michelle", "Claudia",
            "Leilani", "Chihiro", "Vanessa", "Gabriel", "Himari",
            "Phoenix", "Celeste", "Amelia", "Sakura", "Robert",
            "Adrian", "Jasper", "Astrid", "Carina", "Elfina",
            "Tasha", "Alice", "Norah", "Sofia", "Nadia", "Mario",
            "Freya", "Kirby", "River", "Amber", "Clara", "Roxy",
            "Emma", "Kate", "Zuri", "Joey", "Toni", "Nina", "Liam",
            "Riku", "Clay", "Anna", "Chloe", "Haniya",
            "Liz", "Mei", "Ken", "Sam"
        };

        static readonly Regex namePattern;

        static Patch_NodeTextGet()
        {
            string joined = string.Join("|", characterNames.Select(Regex.Escape));
            namePattern = new Regex(@"\b(" + joined + @")\b", RegexOptions.Compiled);
        }

        static void Postfix(ref string __result)
        {
            if (Dialogue.Current != null && __result != null)
            {
                __result = namePattern.Replace(__result, "Josef");
            }
        }
    }
}

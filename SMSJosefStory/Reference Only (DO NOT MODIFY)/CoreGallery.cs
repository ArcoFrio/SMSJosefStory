using BepInEx;
using GameCreator;
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
using UnityEngine.InputSystem.HID;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace SMSGallery
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Core : BaseUnityPlugin
    {
        #region Plugin Info
        public const string pluginGuid = "treboy.starmakerstory.smsgallery.core";
        public const string pluginName = "Gallery";
        public const string pluginVersion = "1.1.3";
        #endregion

        public static GameObject afterSleepEvents;
        public static GameObject introMomentNewGame;
        public static bool loadedCore = false;
        public static GameObject savedUI;
        public static GameObject saveLoadSystem;
        public static int loadedSaveFile;
        public static SaveLoadManager saveLoadManager;
        public static Scene currentScene;
        public static string configPath = "BepInEx\\plugins\\SMSGallery\\Config\\";
        public static string savesPath = "BepInEx\\plugins\\SMSGallery\\Saves\\";
        public static string UIPath = "BepInEx\\plugins\\SMSGallery\\UI\\";
        public static string exePath;
        public static Transform attractorCanvas;
        public static Transform bustManager;
        public static Transform cGManagerSexy;
        public static Transform coreEvents;
        public static Transform effects;
        public static Transform gameplay;
        public static Transform level;
        public static Transform mainCanvas;
        public static Transform roomTalk;

        public static GameObject buttonGallery;
        public static GameObject gallery;
        public static GameObject fullscreenSceneButton;
        private static Dictionary<SpriteRenderer, (string layer, int order)> originalSpriteSettings = new Dictionary<SpriteRenderer, (string, int)>();
        private static Dictionary<Texture2D, FilterMode> originalTextureFilterModes = new Dictionary<Texture2D, FilterMode>();
        private static string originalCanvasRenderMode;
        private static int originalCanvasSortingOrder;
        private static bool playerRoomButtonCanvasWasActive = false;
        private static bool mainCanvasWasActive = false;
        private static Vector3 originalSceneScale;
        private static Vector3 originalScenePosition;
        private static List<GameObject> disabledCanvasChildren = new List<GameObject>();
        private static Trigger disabledSceneTrigger = null; // Store disabled trigger to re-enable on exit
        
        // GameStart version text
        private static bool versionTextCreated = false;

        // Gallery performance optimization - refresh throttling
        private static float lastGalleryRefreshTime = 0f;
        private const float GALLERY_REFRESH_INTERVAL = 0.1f; // Refresh every 0.1 seconds max

        // Gallery sorting configuration
        private static List<GalleryGroup> galleryGroups = new List<GalleryGroup>();

        // Gallery display modes
        private enum GalleryDisplayMode { ScenesOnly, BustsAndNpcs }
        private static GalleryDisplayMode currentGalleryDisplayMode = GalleryDisplayMode.ScenesOnly;

        // Config system
        private static GalleryConfig galleryConfig = new GalleryConfig();

        private class GalleryConfig
        {
            public bool MultiSaveMode = false;

            public void LoadFromFile(string configPath)
            {
                string filePath = Path.Combine(configPath, "Config.ini");

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[Gallery] Config.ini not found at: {filePath}. Creating with default values.");
                    SaveToFile(configPath);
                    return;
                }

                try
                {
                    string[] lines = File.ReadAllLines(filePath);
                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();
                        
                        // Skip comments and empty lines
                        if (trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#") || string.IsNullOrEmpty(trimmedLine))
                            continue;

                        // Parse key=value pairs
                        if (trimmedLine.Contains("="))
                        {
                            string[] parts = trimmedLine.Split(new char[] { '=' }, 2);
                            if (parts.Length == 2)
                            {
                                string key = parts[0].Trim();
                                string value = parts[1].Trim();

                                // Remove inline comments
                                if (value.Contains(";"))
                                {
                                    value = value.Substring(0, value.IndexOf(";")).Trim();
                                }

                                // Parse specific config values
                                if (key.Equals("MultiSaveMode", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (bool.TryParse(value, out bool boolValue))
                                    {
                                        MultiSaveMode = boolValue;
                                    }
                                }
                            }
                        }
                    }

                    Debug.Log($"[Gallery] Loaded config from: {filePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Gallery] Failed to load Config.ini: {ex.Message}");
                }
            }

            public void SaveToFile(string configPath)
            {
                try
                {
                    // Ensure directory exists
                    if (!Directory.Exists(configPath))
                    {
                        Directory.CreateDirectory(configPath);
                    }

                    string filePath = Path.Combine(configPath, "Config.ini");

                    // Create config content with comments
                    string configContent = @"; Gallery Mod Configuration
; This file is auto-generated and can be edited while the game is running

[Settings]
; Enable multi-save mode for advanced save management (true/false)
MultiSaveMode=" + MultiSaveMode.ToString().ToLower() + @"
";

                    File.WriteAllText(filePath, configContent);
                    Debug.Log($"[Gallery] Created/updated Config.ini at: {filePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Gallery] Failed to save Config.ini: {ex.Message}");
                }
            }
        }

        private class GalleryGroup
        {
            public string name;
            public List<string> sceneNames;
            public List<string> bustNames;
            public List<string> npcNames;
        }

        [System.Serializable]
        public class GalleryGroupsConfig
        {
            [System.Xml.Serialization.XmlElement("Group")]
            public GalleryGroupData[] Groups;
        }

        [System.Serializable]
        public class GalleryGroupData
        {
            [System.Xml.Serialization.XmlAttribute("name")]
            public string name;

            [System.Xml.Serialization.XmlElement("Scene")]
            public string[] sceneNames;

            [System.Xml.Serialization.XmlElement("Bust")]
            public string[] bustNames;

            [System.Xml.Serialization.XmlElement("NPC")]
            public string[] npcNames;
        }

        private class NpcGalleryData
        {
            public GameObject npcGO;
            public Sprite mainSprite;
            public List<Sprite> secondarySprites; // For Bafa_Boobs, Bafa_Boobs (1), Bafa_Dick, etc.
            
            public NpcGalleryData()
            {
                secondarySprites = new List<Sprite>();
            }
        }

        /// <summary>
        /// NPCs that should not appear in the gallery but unlock another NPC when seen
        /// Key: Hierarchical NPC ID (e.g., "25_Mall_Group_1_NPC (10)")
        /// Value: Hierarchical NPC ID of the NPC to unlock (e.g., "25_Mall_Group_1_NPC (1)")
        /// </summary>
        private static Dictionary<string, string> npcBlacklist = new Dictionary<string, string>()
        {
            // 25_Mall > NPCs > Group_1
            { "25_Mall_Group_1_NPC (10)", "25_Mall_Group_1_NPC (1)" },
            { "25_Mall_Group_1_NPC (11)", "25_Mall_Group_1_NPC (3)" },
            { "25_Mall_Group_1_NPC (12)", "25_Mall_Group_1_NPC (9)" },
            { "25_Mall_Group_1_NPC (13)", "25_Mall_Group_1_NPC (4)" },
            { "25_Mall_Group_1_NPC (14)", "25_Mall_Group_1_NPC (8)" },
            { "25_Mall_Group_1_NPC (17)", "25_Mall_Group_1_NPC (5)" },
            { "25_Mall_Group_1_NPC (18)", "25_Mall_Group_1_NPC (2)" },
            
            // 25_Mall > NPCs > Group_2
            { "25_Mall_Group_2_NPC (4)", "25_Mall_Group_2_NPC (1)" },
            { "25_Mall_Group_2_NPC (6)", "25_Mall_Group_2_NPC (2)" },
            { "25_Mall_Group_2_NPC (11)", "25_Mall_Group_2_NPC (3)" },
            { "25_Mall_Group_2_NPC (12)", "25_Mall_Group_2_NPC (9)" },
            { "25_Mall_Group_2_NPC (13)", "25_Mall_Group_2_NPC (8)" },
            { "25_Mall_Group_2_NPC (14)", "25_Mall_Group_2_NPC (7)" },
            { "25_Mall_Group_2_NPC (15)", "25_Mall_Group_2_NPC (10)" },
            { "25_Mall_Group_2_NPC (16)", "25_Mall_Group_2_NPC (5)" },
            
            // 25_Mall > NPCs > Group_3
            { "25_Mall_Group_3_NPC (4)", "25_Mall_Group_3_NPC (2)" },
            { "25_Mall_Group_3_NPC (5)", "25_Mall_Group_3_NPC (3)" },
            { "25_Mall_Group_3_NPC (6)", "25_Mall_Group_3_NPC (9)" },
            { "25_Mall_Group_3_NPC (12)", "25_Mall_Group_3_NPC (8)" },
            { "25_Mall_Group_3_NPC (13)", "25_Mall_Group_3_NPC (7)" },
            { "25_Mall_Group_3_NPC (14)", "25_Mall_Group_3_NPC (10)" },
            { "25_Mall_Group_3_NPC (15)", "25_Mall_Group_3_NPC (11)" }
        };

        /// <summary>
        /// Check if an NPC ID is blacklisted and get the primary NPC it unlocks
        /// </summary>
        public static bool TryGetBlacklistedNpcMapping(string npcId, out string primaryNpcId)
        {
            return npcBlacklist.TryGetValue(npcId, out primaryNpcId);
        }

        private static void LoadGalleryGroupsFromXML()
        {
            galleryGroups.Clear();

            string xmlPath = Path.Combine(exePath, configPath, "GalleryGroups.xml");

            if (!File.Exists(xmlPath))
            {
                // Don't create template yet - we need discovered scenes first
                // This will be handled in PopulateGalleryWithScenes
                Debug.LogWarning($"[Gallery] GalleryGroups.xml not found at: {xmlPath}. Will create template after discovering scenes.");
                return;
            }

            try
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(xmlPath);

                System.Xml.XmlElement rootElement = doc.DocumentElement;
                
                foreach (System.Xml.XmlNode groupNode in rootElement.ChildNodes)
                {
                    if (groupNode.NodeType != System.Xml.XmlNodeType.Element)
                        continue;
                        
                    System.Xml.XmlElement groupElement = (System.Xml.XmlElement)groupNode;
                    if (groupElement.Name != "Group")
                        continue;
                    
                    string groupName = groupElement.GetAttribute("name");
                    
                    List<string> sceneNames = new List<string>();
                    List<string> bustNames = new List<string>();
                    List<string> npcNames = new List<string>();
                    
                    // Only get direct children of this group
                    foreach (System.Xml.XmlNode childNode in groupElement.ChildNodes)
                    {
                        if (childNode.NodeType != System.Xml.XmlNodeType.Element)
                            continue;
                            
                        System.Xml.XmlElement childElement = (System.Xml.XmlElement)childNode;
                        
                        if (childElement.Name == "Scene")
                        {
                            sceneNames.Add(childElement.InnerText);
                        }
                        else if (childElement.Name == "Bust")
                        {
                            bustNames.Add(childElement.InnerText);
                        }
                        else if (childElement.Name == "NPC")
                        {
                            npcNames.Add(childElement.InnerText);
                        }
                    }

                    GalleryGroup group = new GalleryGroup
                    {
                        name = groupName,
                        sceneNames = sceneNames,
                        bustNames = bustNames,
                        npcNames = npcNames
                    };
                    galleryGroups.Add(group);
                }
                
                Debug.Log($"[Gallery] Loaded {galleryGroups.Count} gallery groups from XML");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Gallery] Failed to load GalleryGroups.xml: {ex.Message}");
            }
        }

        private static void SaveUnsortedItemsToXml(List<string> unsortedScenes, List<string> unsortedBusts, List<string> unsortedNpcs)
        {
            try
            {
                string xmlPath = Path.Combine(exePath, configPath, "GalleryGroups.xml");
                
                if (!File.Exists(xmlPath))
                {
                    Debug.LogWarning("[Gallery] GalleryGroups.xml not found - cannot save unsorted items");
                    return;
                }

                // Load the existing XML
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load(xmlPath);

                // Find or create the Unsorted group
                System.Xml.XmlElement rootElement = doc.DocumentElement;
                System.Xml.XmlElement unsortedGroupElement = null;

                // Look for existing Unsorted group
                foreach (System.Xml.XmlElement groupElement in rootElement.GetElementsByTagName("Group"))
                {
                    if (groupElement.GetAttribute("name") == "Unsorted")
                    {
                        unsortedGroupElement = groupElement;
                        break;
                    }
                }

                // If not found, create a new Unsorted group
                if (unsortedGroupElement == null)
                {
                    unsortedGroupElement = doc.CreateElement("Group");
                    unsortedGroupElement.SetAttribute("name", "Unsorted");
                    rootElement.AppendChild(unsortedGroupElement);
                    Debug.Log("[Gallery] Created new Unsorted group in GalleryGroups.xml");
                }
                else
                {
                    // Always clear the Unsorted group before populating to avoid duplicates
                    // Get current item count for debug logging
                    int existingItemCount = unsortedGroupElement.ChildNodes.Count;
                    if (existingItemCount > 0)
                    {
                        Debug.Log($"[Gallery] Unsorted group currently has {existingItemCount} items, clearing for fresh population");
                        unsortedGroupElement.RemoveAll();
                    }
                }

                int itemsAdded = 0;

                // Add unsorted scenes
                foreach (string sceneName in unsortedScenes)
                {
                    System.Xml.XmlElement sceneElement = doc.CreateElement("Scene");
                    sceneElement.InnerText = sceneName;
                    unsortedGroupElement.AppendChild(sceneElement);
                    itemsAdded++;
                }

                // Add unsorted busts
                foreach (string bustName in unsortedBusts)
                {
                    System.Xml.XmlElement bustElement = doc.CreateElement("Bust");
                    bustElement.InnerText = bustName;
                    unsortedGroupElement.AppendChild(bustElement);
                    itemsAdded++;
                }

                // Add unsorted NPCs
                foreach (string npcName in unsortedNpcs)
                {
                    System.Xml.XmlElement npcElement = doc.CreateElement("NPC");
                    npcElement.InnerText = npcName;
                    unsortedGroupElement.AppendChild(npcElement);
                    itemsAdded++;
                }

                // Only save if we actually added items
                if (itemsAdded > 0)
                {
                    // Save the modified XML with proper formatting
                    System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = "  "
                    };

                    using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(xmlPath, settings))
                    {
                        doc.Save(writer);
                    }

                    Debug.Log($"[Gallery] Saved {itemsAdded} unsorted items to GalleryGroups.xml - Scenes: {unsortedScenes.Count}, Busts: {unsortedBusts.Count}, NPCs: {unsortedNpcs.Count}");
                }
                else
                {
                    Debug.Log("[Gallery] No unsorted items to save to GalleryGroups.xml");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Gallery] Failed to save unsorted items to XML: {ex.Message}");
            }
        }

        private static void CreateTemplateGalleryGroupsXML(string xmlPath, Dictionary<string, (GameObject sceneGO, Sprite sprite)> allScenes, List<string> bustNames, List<string> npcNames)
        {
            try
            {
                // Ensure directory exists
                string directoryPath = Path.GetDirectoryName(xmlPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Create XML document directly with proper structure
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                System.Xml.XmlElement rootElement = doc.CreateElement("GalleryGroupsConfig");
                doc.AppendChild(rootElement);

                // Create "All" group with all discovered items
                System.Xml.XmlElement allGroupElement = doc.CreateElement("Group");
                allGroupElement.SetAttribute("name", "All");

                foreach (string sceneName in allScenes.Keys)
                {
                    System.Xml.XmlElement sceneElement = doc.CreateElement("Scene");
                    sceneElement.InnerText = sceneName;
                    allGroupElement.AppendChild(sceneElement);
                }

                foreach (string bustName in bustNames)
                {
                    System.Xml.XmlElement bustElement = doc.CreateElement("Bust");
                    bustElement.InnerText = bustName;
                    allGroupElement.AppendChild(bustElement);
                }

                foreach (string npcName in npcNames)
                {
                    System.Xml.XmlElement npcElement = doc.CreateElement("NPC");
                    npcElement.InnerText = npcName;
                    allGroupElement.AppendChild(npcElement);
                }

                rootElement.AppendChild(allGroupElement);

                // Create "Unsorted" group (empty initially)
                System.Xml.XmlElement unsortedGroupElement = doc.CreateElement("Group");
                unsortedGroupElement.SetAttribute("name", "Unsorted");
                rootElement.AppendChild(unsortedGroupElement);

                // Save with proper formatting
                System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  "
                };

                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(xmlPath, settings))
                {
                    doc.Save(writer);
                }

                Debug.Log($"[Gallery] Created template GalleryGroups.xml with 'All' group containing {allScenes.Count} scenes, {bustNames.Count} busts, and {npcNames.Count} NPCs at: {xmlPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Gallery] Failed to create template GalleryGroups.xml: {ex.Message}");
            }
        }

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

            // Load gallery config from INI file
            string fullConfigPath = Path.Combine(exePath, configPath);
            galleryConfig.LoadFromFile(fullConfigPath);

            // Load gallery groups from XML config
            LoadGalleryGroupsFromXML();
        }
        public void OnSceneLoaded(Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            currentScene = scene;
            loadedCore = false;
            
            // Reset version text flag when entering GameStart so it gets recreated
            if (scene.name == "GameStart")
            {
                versionTextCreated = false;
            }
        }
        public void Update()
        {
            if (currentScene.name == "CoreGameScene")
            {
                if (!loadedCore)
                {
                    saveLoadManager = GameObject.FindFirstObjectByType<SaveLoadManager>();

                    attractorCanvas = GameObject.Find("Attractor_Canvas").transform;
                    bustManager = GameObject.Find("2_Bust_Manager").transform;
                    cGManagerSexy = GameObject.Find("4_CG_Manager-Sexy").transform;
                    level = GameObject.Find("5_Levels").transform;
                    effects = GameObject.Find("6_Effects").transform;
                    coreEvents = GameObject.Find("8_Core_Events").transform;
                    roomTalk = GameObject.Find("8_Room_Talk").transform;
                    mainCanvas = GameObject.Find("9_MainCanvas").transform;
                    gameplay = GameObject.Find("10_Gameplay").transform;

                    afterSleepEvents = mainCanvas.Find("AfterSleepEvents").gameObject;
                    introMomentNewGame = mainCanvas.transform.Find("Starmaker").Find("Intro_Moment").gameObject;
                    savedUI = effects.Find("Effect_Canvas").Find("Game_Saved").Find("Saved").gameObject;
                    saveLoadSystem = mainCanvas.transform.Find("SaveLoadSystem").gameObject;

                    buttonGallery = GameObject.Instantiate(mainCanvas.Find("Navigator").Find("Buttone").GetChild(0).gameObject, mainCanvas.Find("Navigator").Find("Buttone"));
                    buttonGallery.name = "ButtonModGallery";
                    mainCanvas.Find("Navigator").Find("Buttone").gameObject.GetComponent<SpriteRendererCircleLayoutManager>().targetObjects.Add(buttonGallery);
                    gallery = GameObject.Instantiate(mainCanvas.Find("Inventory").gameObject, mainCanvas);
                    gallery.name = "GalleryMod";
                    gallery.transform.Find("Items").gameObject.name = "Scenes";
                    gallery.transform.GetChild(2).gameObject.SetActive(false);
                    gallery.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>().text = "Gallery";
                    gallery.transform.GetChild(4).gameObject.transform.position = new Vector2(gallery.transform.GetChild(4).gameObject.transform.position.x, gallery.transform.GetChild(4).gameObject.transform.position.y + 150);

                    // Replace GridLayoutGroup with GalleryGridLayoutGroup
                    GameObject coreGO = gallery.transform.Find("Scenes").GetChild(0).gameObject;
                    
                    // Set local x position to 0
                    Vector3 coreLocalPos = coreGO.transform.localPosition;
                    coreLocalPos.x = 0;
                    coreGO.transform.localPosition = coreLocalPos;
                    
                    GridLayoutGroup oldGridLayout = coreGO.GetComponent<GridLayoutGroup>();
                    
                    // Remove old GridLayoutGroup immediately
                    DestroyImmediate(oldGridLayout);
                    
                    GalleryGridLayoutGroup newGridLayout = coreGO.AddComponent<GalleryGridLayoutGroup>();
                    
                    // Copy settings from old to new
                    newGridLayout.startAxis = GalleryGridLayoutGroup.Axis.Horizontal;
                    newGridLayout.constraint = GalleryGridLayoutGroup.Constraint.FixedColumnCount;
                    newGridLayout.constraintCount = 6;
                    newGridLayout.spacing = new Vector2(15, 15);
                    newGridLayout.cellSize = new Vector2(225, 225);
                    newGridLayout.firstRowScale = 0.25f; // 1/4 size for first row
                    
                    gallery.transform.Find("Scenes").gameObject.GetComponent<ScrollRect>().scrollSensitivity = 10;
                    gallery.transform.Find("Scenes").gameObject.GetComponent<ScrollRect>().horizontal = false;
                    gallery.transform.Find("Scenes").gameObject.GetComponent<ScrollRect>().vertical = true;
                    gallery.transform.Find("Scenes").gameObject.GetComponent<ScrollRect>().horizontalScrollbar = null;
                    gallery.transform.Find("Scenes").gameObject.GetComponent<ScrollRect>().verticalScrollbar = gallery.transform.Find("Scenes").GetChild(1).gameObject.GetComponent<Scrollbar>();
                    //gallery.transform.Find("Scenes").gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition = 1.0f;
                    gallery.transform.Find("Scenes").GetChild(1).gameObject.GetComponent<Scrollbar>().direction = Scrollbar.Direction.BottomToTop;
                    //gallery.transform.Find("Scenes").GetChild(1).gameObject.GetComponent<Scrollbar>().value = 1.0f;
                    GameObject.Destroy(gallery.transform.GetChild(6).gameObject);

                    Transform galleryBG = gallery.transform.GetChild(1);
                    if (galleryBG != null)
                    {
                        RectTransform galleryBGRect = galleryBG.GetComponent<RectTransform>();
                        if (galleryBGRect != null)
                        {
                            galleryBGRect.sizeDelta = new Vector2(1500, 800);
                        }
                    }
                    Transform galleryFrame = gallery.transform.GetChild(0);
                    if (galleryFrame != null)
                    {
                        RectTransform galleryFrameRect = galleryFrame.GetComponent<RectTransform>();
                        if (galleryFrameRect != null)
                        {
                            galleryFrameRect.sizeDelta = new Vector2(1560, 850);
                        }
                    }

                    // Set Scenes/GetChild(1) scrollbar position
                    Transform scenesChild1 = gallery.transform.Find("Scenes").GetChild(1);
                    if (scenesChild1 != null)
                    {
                        RectTransform scenesChild1Rect = scenesChild1.GetComponent<RectTransform>();
                        if (scenesChild1Rect != null)
                        {
                            // Set width and position, height will be calculated dynamically after content is populated
                            scenesChild1Rect.sizeDelta = new Vector2(25, scenesChild1Rect.sizeDelta.y);
                            scenesChild1Rect.position = new Vector2(scenesChild1Rect.position.x - 250, scenesChild1Rect.position.y + 390);
                            scenesChild1Rect.anchoredPosition = new Vector2(730, 0); // Align to center vertically (anchor is already at center)
                        }
                    }
                    
                    // Set Scenes RectTransform height to 1000
                    Transform scenesTransform = gallery.transform.Find("Scenes");
                    if (scenesTransform != null)
                    {
                        RectTransform scenesRect = scenesTransform.GetComponent<RectTransform>();
                        if (scenesRect != null)
                        {
                            scenesRect.sizeDelta = new Vector2(1700, 560);
                            scenesRect.position = new Vector2(scenesRect.position.x - 24, scenesRect.position.y + 40);
                        }
                    }

                    // Initialize scene tracking in SaveManager
                    SaveManager saveManagerInstance = FindFirstObjectByType<SaveManager>();
                    if (saveManagerInstance != null)
                    {
                        saveManagerInstance.InitializeSceneTracking();
                    }

                    // Delete all children of Scenes/Core
                    Transform scenesCore = gallery.transform.Find("Scenes").Find("Core");
                    if (scenesCore != null)
                    {
                        foreach (Transform child in scenesCore)
                        {
                            GameObject.Destroy(child.gameObject);
                        }
                        Logger.LogInfo("Deleted all children of Gallery Scenes/Core");
                    }
                    else
                    {
                        Logger.LogWarning("Could not find Scenes/Core transform in gallery");
                    }

                    // Delay gallery population by 2 seconds to allow other mods to initialize their content
                    StartCoroutine(DelayedPopulateGallery());

                    // Load and set the gallery icon image
                    string imagePath = Path.Combine(exePath, UIPath, "function_icon_photo.png");
                    if (File.Exists(imagePath))
                    {
                        byte[] imageData = File.ReadAllBytes(imagePath);
                        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        texture.LoadImage(imageData);
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        
                        UnityEngine.UI.Image imageComponent = buttonGallery.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Image>();
                        if (imageComponent != null)
                        {
                            imageComponent.sprite = sprite;
                            Logger.LogInfo("Gallery button image loaded successfully");
                        }
                        else
                        {
                            Logger.LogWarning("Image component not found on buttonGallery child");
                        }
                    }
                    else
                    {
                        Logger.LogWarning($"Gallery icon image not found at: {imagePath}");
                    }

                    // Scale the button child to half its size
                    buttonGallery.transform.GetChild(0).localScale = new Vector3(0.5f, 0.5f, 0.5f);

                    // Create custom button in the gallery
                    CreateGalleryCustomButton();

                    // Create chars button in the gallery
                    CreateGalleryCharsButton();

                    // Create MultiSaveMode toggle button in the gallery
                    CreateMultiSaveModeToggleButton();

                    // Modify the last instruction in buttonGallery to activate gallery instead of inventory
                    ModifyButtonGalleryLastInstruction();

                    Logger.LogInfo("----- CORE LOADED -----");
                    loadedCore = true;

                    //Debugging.PrintButtonInstructionsDetailed(buttonGallery);
                }
                else
                {
                    // Refresh gallery when it becomes active
                    if (gallery != null && gallery.activeSelf)
                    {
                        Transform scenesCore = gallery.transform.Find("Scenes").Find("Core");
                        if (scenesCore != null && scenesCore.childCount == 0)
                        {
                            // Gallery is empty, populate it
                            PopulateGalleryWithScenes();
                        }
                        else if (scenesCore != null && scenesCore.childCount > 0)
                        {
                            // Gallery exists, refresh the seen status with throttling to reduce lag
                            // Only refresh if enough time has passed since last refresh
                            if (Time.realtimeSinceStartup - lastGalleryRefreshTime >= GALLERY_REFRESH_INTERVAL)
                            {
                                RefreshGallerySceneStatus();
                                RefreshGalleryBustStatus();
                                RefreshGalleryNpcStatus();
                                lastGalleryRefreshTime = Time.realtimeSinceStartup;
                            }
                        }
                    }
                }
            }
            if (currentScene.name == "GameStart")
            {
                if (loadedCore)
                {
                    Logger.LogInfo("----- CORE UNLOADED -----");
                    loadedCore = false;
                    versionTextCreated = false;
                }

                if (!versionTextCreated)
                {
                    CreateVersionText();
                    versionTextCreated = true;
                }
            }
        }

        /// <summary>
        /// Creates a version text label on the GameStart screen (top-right corner).
        /// Clones Part_One > Canvas_MM > MainMenu > Text (TMP) and sets it to display
        /// the mod version. If the original text doesn't end with "1.8C", the text is colored red
        /// to indicate a potential version mismatch.
        /// </summary>
        private void CreateVersionText()
        {
            // Find the original text: Part_One > Canvas_MM > MainMenu > Text (TMP)
            GameObject partOne = GameObject.Find("Part_One");
            if (partOne == null)
            {
                Debug.LogWarning("[Gallery] Part_One not found in GameStart scene");
                return;
            }

            Transform canvasMM = partOne.transform.Find("Canvas_MM");
            if (canvasMM == null)
            {
                Debug.LogWarning("[Gallery] Canvas_MM not found in Part_One");
                return;
            }

            Transform mainMenu = canvasMM.Find("MainMenu");
            if (mainMenu == null)
            {
                Debug.LogWarning("[Gallery] MainMenu not found in Canvas_MM");
                return;
            }

            Transform originalTextTMP = mainMenu.Find("Text (TMP)");
            if (originalTextTMP == null)
            {
                Debug.LogWarning("[Gallery] Text (TMP) not found in MainMenu");
                return;
            }

            // Clone the original text GO
            GameObject versionTextGO = GameObject.Instantiate(originalTextTMP.gameObject, mainMenu);
            versionTextGO.name = "ModVersionText";

            // Set the version text
            string versionText = "Gallery mod " + pluginVersion + " for 1.8E";
            TextMeshProUGUI versionTMP = versionTextGO.GetComponent<TextMeshProUGUI>();
            if (versionTMP == null)
            {
                Debug.LogWarning("[Gallery] TextMeshProUGUI not found on cloned text GO");
                return;
            }

            versionTMP.text = versionText;

            // Right-align the text
            versionTMP.alignment = TextAlignmentOptions.TopRight;

            // Position using the original's position but with inverted X
            RectTransform originalRect = originalTextTMP.GetComponent<RectTransform>();
            RectTransform versionRect = versionTextGO.GetComponent<RectTransform>();
            if (originalRect != null && versionRect != null)
            {
                Vector3 originalPos = originalRect.anchoredPosition;
                versionRect.anchoredPosition = new Vector2(-originalPos.x, originalPos.y);
            }

            // Compare original text ending - if the original doesn't end with the same version, make text red
            TextMeshProUGUI originalTMP = originalTextTMP.GetComponent<TextMeshProUGUI>();
            if (originalTMP != null)
            {
                string originalText = originalTMP.text.TrimEnd();
                
                // Extract the version part from our version text (e.g., "1.8C" from "Gallery mod 1.1.0 for 1.8C")
                string expectedVersion = versionText.Substring(versionText.LastIndexOf("for ") + 4).Trim();
                
                if (!originalText.EndsWith(expectedVersion))
                {
                    versionTMP.color = Color.red;
                    Debug.LogWarning($"[Gallery] Game version mismatch! Original text: '{originalText}' does not end with '{expectedVersion}'. Version text colored red.");
                }
                else
                {
                    Debug.Log($"[Gallery] Game version matches: '{originalText}' ends with '{expectedVersion}'");
                }
            }

            Debug.Log($"[Gallery] Created version text on GameStart: '{versionText}'");
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

        private static void RefreshGallerySceneStatus()
        {
            if (gallery == null) return;

            Transform galleryCore = gallery.transform.Find("Scenes").Find("Core");
            if (galleryCore == null) return;

            SaveManager saveManager = FindFirstObjectByType<SaveManager>();

            // Cache child count to avoid repeated lookups
            int childCount = galleryCore.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = galleryCore.GetChild(i);
                string itemId = child.gameObject.name;
                
                // Skip group titles and spacers
                if (itemId.StartsWith("GroupTitle_") || itemId.Contains("Spacer"))
                    continue;
                
                UnityEngine.UI.Image image = child.GetComponent<UnityEngine.UI.Image>();
                UnityEngine.UI.Button button = child.GetComponent<UnityEngine.UI.Button>();
                
                // Skip items that aren't actual scene items (skip if they're busts)
                if (saveManager != null && saveManager.IsBustSeen(itemId))
                    continue; // This is a bust item, skip it
                
                if (image != null)
                {
                    // Update the image based on current seen status
                    if (saveManager != null && !saveManager.IsSceneSeen(itemId))
                    {
                        ApplyGrayscaleToImage(image);
                    }
                    else
                    {
                        // Reset to original color if scene has been seen
                        image.color = new Color(1, 1, 1, image.color.a);
                    }
                }
                
                // Update button interactivity
                if (button != null)
                {
                    bool sceneSeen = saveManager != null && saveManager.IsSceneSeen(itemId);
                    button.interactable = sceneSeen;
                }
            }
        }

        private static void RefreshGalleryBustStatus()
        {
            if (gallery == null) return;

            Transform galleryCore = gallery.transform.Find("Scenes").Find("Core");
            if (galleryCore == null) return;

            SaveManager saveManager = FindFirstObjectByType<SaveManager>();

            // Cache child count to avoid repeated lookups
            int childCount = galleryCore.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = galleryCore.GetChild(i);
                string itemId = child.gameObject.name;
                
                // Skip group titles and spacers
                if (itemId.StartsWith("GroupTitle_") || itemId.Contains("Spacer"))
                    continue;
                
                UnityEngine.UI.Image image = child.GetComponent<UnityEngine.UI.Image>();
                UnityEngine.UI.Button button = child.GetComponent<UnityEngine.UI.Button>();
                
                // Skip items that aren't actual bust items (skip if they're scenes)
                if (saveManager != null && saveManager.IsSceneSeen(itemId))
                    continue; // This is a scene item, skip it
                
                if (image != null)
                {
                    // Update the image based on current seen status
                    if (saveManager != null && !saveManager.IsBustSeen(itemId))
                    {
                        ApplyGrayscaleToImage(image);
                    }
                    else
                    {
                        // Reset to original color if bust has been seen
                        image.color = new Color(1, 1, 1, image.color.a);
                    }
                }
                
                // Update button interactivity
                if (button != null)
                {
                    bool bustSeen = saveManager != null && saveManager.IsBustSeen(itemId);
                    button.interactable = bustSeen;
                }
            }
        }

        private static void RefreshGalleryNpcStatus()
        {
            if (gallery == null) return;

            Transform galleryCore = gallery.transform.Find("Scenes").Find("Core");
            if (galleryCore == null) return;

            SaveManager saveManager = FindFirstObjectByType<SaveManager>();

            // Cache child count to avoid repeated lookups
            int childCount = galleryCore.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = galleryCore.GetChild(i);
                string itemId = child.gameObject.name;
                
                // Skip group titles and spacers
                if (itemId.StartsWith("GroupTitle_") || itemId.Contains("Spacer"))
                    continue;
                
                UnityEngine.UI.Image image = child.GetComponent<UnityEngine.UI.Image>();
                
                // Skip items that aren't NPC items (skip if they're scenes or busts)
                if (saveManager != null && (saveManager.IsSceneSeen(itemId) || saveManager.IsBustSeen(itemId)))
                    continue; // This is a scene or bust item, skip it
                
                if (image != null)
                {
                    // Update the image based on current seen status
                    if (saveManager != null && !saveManager.IsNpcSeen(itemId))
                    {
                        ApplyGrayscaleToImage(image);
                    }
                    else
                    {
                        // Reset to original color if NPC has been seen
                        image.color = new Color(1, 1, 1, image.color.a);
                    }
                }
                
                // Also update secondary sprites (Bafa_Boobs, Bafa_Boobs (1), Bafa_Dick, etc.)
                int secondaryChildCount = child.childCount;
                for (int j = 0; j < secondaryChildCount; j++)
                {
                    Transform secondaryChild = child.GetChild(j);
                    UnityEngine.UI.Image secondaryImage = secondaryChild.GetComponent<UnityEngine.UI.Image>();
                    
                    if (secondaryImage != null)
                    {
                        if (saveManager != null && !saveManager.IsNpcSeen(itemId))
                        {
                            ApplyGrayscaleToImage(secondaryImage);
                        }
                        else
                        {
                            // Reset to original color if NPC has been seen
                            secondaryImage.color = new Color(1, 1, 1, secondaryImage.color.a);
                        }
                    }
                }
                
                // Update button interactability
                UnityEngine.UI.Button button = child.GetComponent<UnityEngine.UI.Button>();
                if (button != null && saveManager != null)
                {
                    // Check if this is an EventUI NPC (special UI elements, always clickable)
                    bool isEventUiNpc = itemId.StartsWith("EventUI_");
                    
                    // For EventUI NPCs, always make clickable
                    // For regular NPCs, only enable button if NPC has been seen
                    bool npcSeen = saveManager.IsNpcSeen(itemId);
                    button.interactable = isEventUiNpc ? true : npcSeen;
                }
            }
        }

        private static void ModifyButtonGalleryLastInstruction()
        {
            if (buttonGallery == null) return;

            // Get the Button component and replace its listeners with our custom handler
            UnityEngine.UI.Button button = buttonGallery.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                // Clear existing listeners
                button.onClick.RemoveAllListeners();
                
                // Add our custom handler that mimics the original instructions
                button.onClick.AddListener(OnButtonGalleryClicked);
                
                Debug.Log("[Gallery] Set up custom button click handler for gallery button");
            }
            else
            {
                Debug.LogWarning("[Gallery] Button component not found on buttonGallery");
            }
        }

        private static void CreateGalleryCustomButton()
        {
            if (gallery == null) return;

            // Find the close button (Button)
            Transform closeButtonTransform = gallery.transform.Find("Button");
            if (closeButtonTransform == null)
            {
                Debug.LogWarning("[Gallery] Close button not found in gallery");
                return;
            }

            // Instantiate the close button as our new custom button
            GameObject scenesButtonGO = GameObject.Instantiate(closeButtonTransform.gameObject, gallery.transform);
            scenesButtonGO.name = "ScenesButton";

            // Get the original close button's position and move our button to the left
            RectTransform closeButtonRect = closeButtonTransform.GetComponent<RectTransform>();
            RectTransform scenesButtonRect = scenesButtonGO.GetComponent<RectTransform>();
            if (closeButtonRect != null && scenesButtonRect != null)
            {
                scenesButtonRect.anchoredPosition = new Vector2(closeButtonRect.anchoredPosition.x - 400, closeButtonRect.anchoredPosition.y + 116.5f);
            }

            // Remove ButtonInstructions component to prevent duplicate functionality
            ButtonInstructions buttonInstructions = scenesButtonGO.GetComponent<ButtonInstructions>();
            if (buttonInstructions != null)
            {
                DestroyImmediate(buttonInstructions);
                Debug.Log("[Gallery] Removed ButtonInstructions from scenes button");
            }

            // Ensure Button component exists
            UnityEngine.UI.Button buttonComponent = scenesButtonGO.GetComponent<UnityEngine.UI.Button>();
            if (buttonComponent == null)
            {
                buttonComponent = scenesButtonGO.AddComponent<UnityEngine.UI.Button>();
                Debug.Log("[Gallery] Added Button component to scenes button");
            }

            // Remove Shadow component
            Shadow shadow = scenesButtonGO.GetComponent<Shadow>();
            if (shadow != null)
            {
                DestroyImmediate(shadow);
                Debug.Log("[Gallery] Removed Shadow from scenes button");
            }

            // Remove Trigger component if present
            Trigger trigger = scenesButtonGO.GetComponent<Trigger>();
            if (trigger != null)
            {
                DestroyImmediate(trigger);
                Debug.Log("[Gallery] Removed Trigger from scenes button");
            }

            // Update the button's Image component with RoundedEdge_L.png
            UnityEngine.UI.Image buttonImage = scenesButtonGO.GetComponent<UnityEngine.UI.Image>();
            if (buttonImage != null)
            {
                string roundedEdgePath = Path.Combine(exePath, UIPath, "RoundedEdge_L.png");
                if (File.Exists(roundedEdgePath))
                {
                    byte[] imageData = File.ReadAllBytes(roundedEdgePath);
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    texture.LoadImage(imageData);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    buttonImage.sprite = sprite;
                    Debug.Log("[Gallery] Set custom button background to RoundedEdge_L.png");
                }
                else
                {
                    Debug.LogWarning($"[Gallery] RoundedEdge_L.png not found at: {roundedEdgePath}");
                }
            }

            // Update the button's child Image component with Button_Scenes.png
            if (scenesButtonGO.transform.childCount > 0)
            {
                Transform childTransform = scenesButtonGO.transform.GetChild(0);
                UnityEngine.UI.Image childImage = childTransform.GetComponent<UnityEngine.UI.Image>();
                if (childImage != null)
                {
                    string buttonScenesPath = Path.Combine(exePath, UIPath, "Button_Scenes.png");
                    if (File.Exists(buttonScenesPath))
                    {
                        byte[] imageData = File.ReadAllBytes(buttonScenesPath);
                        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        texture.LoadImage(imageData);
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        childImage.sprite = sprite;
                        Debug.Log("[Gallery] Set custom button icon to Button_Scenes.png");
                    }
                    else
                    {
                        Debug.LogWarning($"[Gallery] Button_Scenes.png not found at: {buttonScenesPath}");
                    }
                }
            }

            // Add click listener to the button's onClick event
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(OnScenesButtonClicked);
                Debug.Log("[Gallery] Added click listener to scenes button");
            }
            else
            {
                Debug.LogWarning("[Gallery] Button component not found on scenes button GO");
            }

            Debug.Log("[Gallery] Created custom button in gallery");
        }

        private static void CreateGalleryCharsButton()
        {
            if (gallery == null) return;

            // Find the close button (Button) to use as template
            Transform closeButtonTransform = gallery.transform.Find("Button");
            if (closeButtonTransform == null)
            {
                Debug.LogWarning("[Gallery] Close button not found in gallery");
                return;
            }

            // Instantiate the close button as our new chars button
            GameObject charsButtonGO = GameObject.Instantiate(closeButtonTransform.gameObject, gallery.transform);
            charsButtonGO.name = "CharsButton";

            // Get the original close button's position and move our button to the right
            RectTransform closeButtonRect = closeButtonTransform.GetComponent<RectTransform>();
            RectTransform charsButtonRect = charsButtonGO.GetComponent<RectTransform>();
            if (closeButtonRect != null && charsButtonRect != null)
            {
                charsButtonRect.anchoredPosition = new Vector2(closeButtonRect.anchoredPosition.x - 315, closeButtonRect.anchoredPosition.y + 116.5f);
            }

            // Remove ButtonInstructions component to prevent duplicate functionality
            ButtonInstructions buttonInstructions = charsButtonGO.GetComponent<ButtonInstructions>();
            if (buttonInstructions != null)
            {
                DestroyImmediate(buttonInstructions);
                Debug.Log("[Gallery] Removed ButtonInstructions from chars button");
            }

            // Ensure Button component exists
            UnityEngine.UI.Button buttonComponent = charsButtonGO.GetComponent<UnityEngine.UI.Button>();
            if (buttonComponent == null)
            {
                buttonComponent = charsButtonGO.AddComponent<UnityEngine.UI.Button>();
                Debug.Log("[Gallery] Added Button component to chars button");
            }

            // Remove Shadow component
            Shadow shadow = charsButtonGO.GetComponent<Shadow>();
            if (shadow != null)
            {
                DestroyImmediate(shadow);
                Debug.Log("[Gallery] Removed Shadow from chars button");
            }

            // Remove Trigger component if present
            Trigger trigger = charsButtonGO.GetComponent<Trigger>();
            if (trigger != null)
            {
                DestroyImmediate(trigger);
                Debug.Log("[Gallery] Removed Trigger from chars button");
            }

            // Update the button's Image component with RoundedEdge_R.png
            UnityEngine.UI.Image buttonImage = charsButtonGO.GetComponent<UnityEngine.UI.Image>();
            if (buttonImage != null)
            {
                string roundedEdgePath = Path.Combine(exePath, UIPath, "RoundedEdge_R.png");
                if (File.Exists(roundedEdgePath))
                {
                    byte[] imageData = File.ReadAllBytes(roundedEdgePath);
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    texture.LoadImage(imageData);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    buttonImage.sprite = sprite;
                    Debug.Log("[Gallery] Set chars button background to RoundedEdge_R.png");
                }
                else
                {
                    Debug.LogWarning($"[Gallery] RoundedEdge_R.png not found at: {roundedEdgePath}");
                }
            }

            // Update the button's child Image component with Button_Chars.png
            if (charsButtonGO.transform.childCount > 0)
            {
                Transform childTransform = charsButtonGO.transform.GetChild(0);
                UnityEngine.UI.Image childImage = childTransform.GetComponent<UnityEngine.UI.Image>();
                if (childImage != null)
                {
                    string buttonCharsPath = Path.Combine(exePath, UIPath, "Button_Chars.png");
                    if (File.Exists(buttonCharsPath))
                    {
                        byte[] imageData = File.ReadAllBytes(buttonCharsPath);
                        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        texture.LoadImage(imageData);
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        childImage.sprite = sprite;
                        Debug.Log("[Gallery] Set chars button icon to Button_Chars.png");
                    }
                    else
                    {
                        Debug.LogWarning($"[Gallery] Button_Chars.png not found at: {buttonCharsPath}");
                    }
                }
            }

            // Add click listener to the button's onClick event
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(OnCharsButtonClicked);
                Debug.Log("[Gallery] Added click listener to chars button");
            }
            else
            {
                Debug.LogWarning("[Gallery] Button component not found on chars button GO");
            }

            Debug.Log("[Gallery] Created chars button in gallery");
        }

        private static void CreateMultiSaveModeToggleButton()
        {
            if (gallery == null) return;

            // Find the close button (Button) to use as template
            Transform closeButtonTransform = gallery.transform.Find("Button");
            if (closeButtonTransform == null)
            {
                Debug.LogWarning("[Gallery] Close button not found in gallery");
                return;
            }

            // Instantiate the close button as our MultiSaveMode toggle button
            GameObject toggleButtonGO = GameObject.Instantiate(closeButtonTransform.gameObject, gallery.transform);
            toggleButtonGO.name = "MultiSaveModeToggleButton";

            // Get the original close button's position and move our button below it
            RectTransform closeButtonRect = closeButtonTransform.GetComponent<RectTransform>();
            RectTransform toggleButtonRect = toggleButtonGO.GetComponent<RectTransform>();
            if (closeButtonRect != null && toggleButtonRect != null)
            {
                toggleButtonRect.anchoredPosition = new Vector2(closeButtonRect.anchoredPosition.x - 280, closeButtonRect.anchoredPosition.y - 800);
                // Scale the toggle button to 2x width to accommodate the wider switch texture
                toggleButtonRect.sizeDelta = new Vector2(closeButtonRect.sizeDelta.x * 1.3f, closeButtonRect.sizeDelta.y / 1.5f);
                Debug.Log($"[Gallery] Sized toggle button: {toggleButtonRect.sizeDelta}");
            }

            // Remove ButtonInstructions component to prevent duplicate functionality
            ButtonInstructions buttonInstructions = toggleButtonGO.GetComponent<ButtonInstructions>();
            if (buttonInstructions != null)
            {
                DestroyImmediate(buttonInstructions);
                Debug.Log("[Gallery] Removed ButtonInstructions from MultiSaveMode toggle button");
            }

            // Ensure Button component exists
            UnityEngine.UI.Button buttonComponent = toggleButtonGO.GetComponent<UnityEngine.UI.Button>();
            if (buttonComponent == null)
            {
                buttonComponent = toggleButtonGO.AddComponent<UnityEngine.UI.Button>();
                Debug.Log("[Gallery] Added Button component to MultiSaveMode toggle button");
            }

            // Remove Shadow component
            Shadow shadow = toggleButtonGO.GetComponent<Shadow>();
            if (shadow != null)
            {
                DestroyImmediate(shadow);
                Debug.Log("[Gallery] Removed Shadow from MultiSaveMode toggle button");
            }

            // Remove Trigger component if present
            Trigger trigger = toggleButtonGO.GetComponent<Trigger>();
            if (trigger != null)
            {
                DestroyImmediate(trigger);
                Debug.Log("[Gallery] Removed Trigger from MultiSaveMode toggle button");
            }

            // Update the button's Image component with appropriate texture based on config
            UnityEngine.UI.Image buttonImage = toggleButtonGO.GetComponent<UnityEngine.UI.Image>();
            if (buttonImage != null)
            {
                string textureFileName = GetMultiSaveMode() ? "Switch_On.png" : "Switch_Off.png";
                string texturePath = Path.Combine(exePath, UIPath, textureFileName);
                if (File.Exists(texturePath))
                {
                    byte[] imageData = File.ReadAllBytes(texturePath);
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    texture.LoadImage(imageData);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    buttonImage.sprite = sprite;
                    Debug.Log($"[Gallery] Set MultiSaveMode toggle button background to {textureFileName}");
                }
                else
                {
                    Debug.LogWarning($"[Gallery] {textureFileName} not found at: {texturePath}");
                }
            }

            // Remove or hide the child image if present
            if (toggleButtonGO.transform.childCount > 0)
            {
                Transform childTransform = toggleButtonGO.transform.GetChild(0);
                childTransform.gameObject.SetActive(false);
                Debug.Log("[Gallery] Hid child image on MultiSaveMode toggle button");
            }

            // Add click listener to the button's onClick event
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(OnMultiSaveModeToggleClicked);
                Debug.Log("[Gallery] Added click listener to MultiSaveMode toggle button");
            }
            else
            {
                Debug.LogWarning("[Gallery] Button component not found on MultiSaveMode toggle button GO");
            }

            Debug.Log("[Gallery] Created MultiSaveMode toggle button in gallery");
            
            // Create text label next to the toggle button
            CreateMultiSaveModeToggleLabel();
        }

        private static void CreateMultiSaveModeToggleLabel()
        {
            if (gallery == null) return;

            // Create a new GameObject for the label
            GameObject labelGO = new GameObject("MultiSaveModeToggleLabel");
            labelGO.transform.SetParent(gallery.transform, false);

            // Add RectTransform
            RectTransform labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchoredPosition = new Vector2(785, -452); // Position to the right of the toggle button (same Y as button)
            labelRect.sizeDelta = new Vector2(400, 100);

            // Add TextMeshProUGUI component
            TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.text = "Show all saves";
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            labelText.fontSize = 36;

            // Get the font from the gallery title to match styling
            TextMeshProUGUI galleryTitleText = gallery.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>();
            if (galleryTitleText != null)
            {
                labelText.font = galleryTitleText.font;
                labelText.fontStyle = galleryTitleText.fontStyle;
                labelText.color = galleryTitleText.color;
                Debug.Log("[Gallery] Applied gallery title font to toggle label");
            }

            Debug.Log("[Gallery] Created MultiSaveMode toggle label");
        }

        private static void OnMultiSaveModeToggleClicked()
        {
            Debug.Log("[Gallery] MultiSaveMode toggle button clicked");
            
            // Toggle the MultiSaveMode setting
            bool currentMode = GetMultiSaveMode();
            SetMultiSaveMode(!currentMode);
            Debug.Log($"[Gallery] MultiSaveMode toggled to: {!currentMode}");
            
            // Refresh SaveManager's MultiSaveMode setting and reload save data
            // Use the preserve runtime version to keep items marked as seen this session
            SaveManager saveManagerInstance = FindFirstObjectByType<SaveManager>();
            if (saveManagerInstance != null)
            {
                saveManagerInstance.ToggleMultiSaveModePreservingRuntime();
                Debug.Log("[Gallery] Toggled MultiSaveMode while preserving runtime cache");
            }
            
            // Update the button's image to reflect the new state
            Transform toggleButtonTransform = gallery.transform.Find("MultiSaveModeToggleButton");
            if (toggleButtonTransform != null)
            {
                UnityEngine.UI.Image buttonImage = toggleButtonTransform.GetComponent<UnityEngine.UI.Image>();
                if (buttonImage != null)
                {
                    string textureFileName = !currentMode ? "Switch_On.png" : "Switch_Off.png";
                    string texturePath = Path.Combine(exePath, UIPath, textureFileName);
                    if (File.Exists(texturePath))
                    {
                        byte[] imageData = File.ReadAllBytes(texturePath);
                        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        texture.LoadImage(imageData);
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        buttonImage.sprite = sprite;
                        Debug.Log($"[Gallery] Updated MultiSaveMode toggle button to {textureFileName}");
                    }
                }
            }
            
            // Reload the gallery display to accommodate the change
            Transform scenesCore = gallery.transform.Find("Scenes").Find("Core");
            if (scenesCore != null)
            {
                foreach (Transform child in scenesCore)
                {
                    GameObject.Destroy(child.gameObject);
                }
                
                // Clear the layout cache from GalleryGridLayoutGroup
                GalleryGridLayoutGroup layoutGroup = scenesCore.GetComponent<GalleryGridLayoutGroup>();
                if (layoutGroup != null)
                {
                    layoutGroup.ClearLayoutCache();
                    Debug.Log("[Gallery] Cleared layout cache");
                }
                
                // Mark layout for rebuild
                LayoutRebuilder.MarkLayoutForRebuild(scenesCore as RectTransform);
                Debug.Log("[Gallery] Marked layout for rebuild after MultiSaveMode toggle");
            }
            
            // Reset scroll position to top
            Transform scenesTransform = gallery.transform.Find("Scenes");
            if (scenesTransform != null)
            {
                ScrollRect scrollRect = scenesTransform.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    scrollRect.verticalNormalizedPosition = 1.0f;
                    Debug.Log("[Gallery] Reset scroll position to top");
                }
            }
            
            PopulateGalleryWithScenes(currentGalleryDisplayMode);
        }

        private static void SetGalleryDisplayMode(GalleryDisplayMode newMode)
        {
            currentGalleryDisplayMode = newMode;
            Debug.Log($"[Gallery] Display mode changed to: {newMode}");
            
            // Clear and repopulate the gallery with the new mode
            Transform scenesCore = gallery.transform.Find("Scenes").Find("Core");
            if (scenesCore != null)
            {
                foreach (Transform child in scenesCore)
                {
                    GameObject.Destroy(child.gameObject);
                }
                
                // Clear the layout cache from GalleryGridLayoutGroup
                GalleryGridLayoutGroup layoutGroup = scenesCore.GetComponent<GalleryGridLayoutGroup>();
                if (layoutGroup != null)
                {
                    layoutGroup.ClearLayoutCache();
                    Debug.Log("[Gallery] Cleared layout cache");
                }
                
                // Mark layout for rebuild
                LayoutRebuilder.MarkLayoutForRebuild(scenesCore as RectTransform);
                Debug.Log("[Gallery] Marked layout for rebuild after clearing");
            }
            
            // Reset scroll position to top
            Transform scenesTransform = gallery.transform.Find("Scenes");
            if (scenesTransform != null)
            {
                ScrollRect scrollRect = scenesTransform.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    scrollRect.verticalNormalizedPosition = 1.0f;
                    Debug.Log("[Gallery] Reset scroll position to top");
                }
            }
            
            PopulateGalleryWithScenes(newMode);
        }

        private static void OnScenesButtonClicked()
        {
            Debug.Log("[Gallery] Scenes button clicked");
            SetGalleryDisplayMode(GalleryDisplayMode.ScenesOnly);
        }

        private static void OnCharsButtonClicked()
        {
            Debug.Log("[Gallery] Chars button clicked");
            SetGalleryDisplayMode(GalleryDisplayMode.BustsAndNpcs);
        }

        private static void OnButtonGalleryClicked()
        {
            // Check if Lock-Game is False - only proceed if game is not locked
            bool gameLocked = GetVariableBool("Lock-Game");
            if (gameLocked)
            {
                Debug.Log("[Gallery] Game is locked, button action blocked");
                return;
            }

            // Load multi-save data if enabled (non-blocking, done in background)
            SaveManager saveManager = FindFirstObjectByType<SaveManager>();
            if (saveManager != null && GetMultiSaveMode())
            {
                saveManager.LoadAllSavesForGallery();
            }

            // Play SFX - "Mountain Audio - Bubble Button 1"
            try
            {
                // Try to find and play the audio
                AudioManager audioManager = FindFirstObjectByType<AudioManager>();
                if (audioManager != null)
                {
                    // Try to call PlaySfx with the audio name
                    // The method signature might be: PlaySfx(string audioName) or similar
                    MethodInfo playMethod = audioManager.GetType().GetMethod("PlaySfx", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string) }, null);
                    if (playMethod != null)
                    {
                        playMethod.Invoke(audioManager, new object[] { "Mountain Audio - Bubble Button 1" });
                        Debug.Log("[Gallery] Played SFX: Mountain Audio - Bubble Button 1");
                    }
                    else
                    {
                        Debug.LogWarning("[Gallery] Could not find PlaySfx method on AudioManager");
                    }
                }
                else
                {
                    Debug.LogWarning("[Gallery] AudioManager not found");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Gallery] Could not play SFX: {ex.Message}");
            }

            // Lock the game
            FindAndModifyVariableBool("Lock-Game", true);
            Debug.Log("[Gallery] Set Lock-Game to True");

            // Activate the Gallery GO
            if (gallery != null)
            {
                gallery.SetActive(true);
                Debug.Log("[Gallery] Activated gallery");
                
                // Reset scroll position to top
                Transform scenesCore = gallery.transform.Find("Scenes").Find("Core");
                if (scenesCore != null)
                {
                    RectTransform coreRect = scenesCore.GetComponent<RectTransform>();
                    if (coreRect != null)
                    {
                        Vector3 newPos = coreRect.localPosition;
                        newPos.y = -100000;
                        coreRect.localPosition = newPos;
                        Debug.Log("[Gallery] Reset scroll position to top");
                    }
                }
            }
        }

        private IEnumerator DelayedPopulateGallery()
        {
            yield return new WaitForSeconds(2.0f);
            
            // Initialize bust tracking after delay to ensure other mods have loaded their busts
            SaveManager saveManagerInstance = FindFirstObjectByType<SaveManager>();
            if (saveManagerInstance != null)
            {
                saveManagerInstance.InitializeBustTracking();
                saveManagerInstance.InitializeNpcTracking();
                // All NPCs (5_Levels and minigame) are now registered in InitializeNpcTracking
                
                // Ensure SaveManager is properly initialized with correct MultiSaveMode setting before populating gallery
                if (GetMultiSaveMode())
                {
                    // Load multi-save data if MultiSaveMode is enabled
                    saveManagerInstance.LoadAllSavesForGallery();
                    Debug.Log("[Gallery] Loaded multi-save data for initial gallery population");
                }
                else
                {
                    // Ensure currentSlotCache is properly loaded when MultiSaveMode is OFF
                    saveManagerInstance.RefreshMultiSaveMode();
                    Debug.Log("[Gallery] Refreshed SaveManager for single-save mode during initial gallery population");
                }
            }
            
            PopulateGalleryWithScenes();
        }

        /// <summary>
        /// Register special NPCs (Sword, Racing, Livestream, Milking, Sprite_Overlay, EventUI) 
        /// for early activation monitoring so they get marked as seen even before the gallery is opened.
        /// </summary>
        private void RegisterSpecialNpcsForEarlyMonitoring(SaveManager saveManager)
        {
            // Temporary dictionary just for registration (not for display)
            Dictionary<string, NpcGalleryData> tempNpcs = new Dictionary<string, NpcGalleryData>();
            
            // Register Livestream NPCs
            RegisterLiveStreamNpcsOnly(saveManager, tempNpcs);
            
            // Register Sword NPCs
            RegisterSwordNpcsOnly(saveManager, tempNpcs);
            
            // Register Milking NPCs
            RegisterMilkingNpcsOnly(saveManager, tempNpcs);
            
            // Register Sprite_Overlay NPCs
            RegisterSpriteOverlayNpcsOnly(saveManager, tempNpcs);
            
            // Register EventUI Photo NPCs
            RegisterEventUIPhotosNpcsOnly(saveManager, tempNpcs);
            
            // Register Racing NPCs
            RegisterRacingNpcsOnly(saveManager, tempNpcs);
            
            // Register Tabletop NPCs
            RegisterTabletopNpcsOnly(saveManager, tempNpcs);
            
            // Register Barista NPCs
            RegisterBaristaNpcsOnly(saveManager, tempNpcs);
        }

        /// <summary>
        /// Register only Livestream NPCs without collecting for display.
        /// </summary>
        private void RegisterLiveStreamNpcsOnly(SaveManager saveManager, Dictionary<string, NpcGalleryData> tempNpcs)
        {
            GameObject livestreamActive = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "Livestream_Active" && go.scene.name != null);
            
            if (livestreamActive != null)
            {
                Transform liveAnna = livestreamActive.transform.Find("Live_Anna");
                if (liveAnna != null)
                {
                    for (int outfitIndex = 0; outfitIndex < liveAnna.childCount; outfitIndex++)
                    {
                        Transform outfitTransform = liveAnna.GetChild(outfitIndex);
                        if (outfitTransform.gameObject.name.StartsWith("Outfit"))
                        {
                            for (int npcIndex = 0; npcIndex < outfitTransform.childCount; npcIndex++)
                            {
                                Transform npcTransform = outfitTransform.GetChild(npcIndex);
                                string npcName = npcTransform.gameObject.name;
                                string hierarchicalId = $"Livestream_Outfits_{outfitTransform.gameObject.name}_{npcName}";
                                
                                SpriteRenderer sr = npcTransform.GetComponent<SpriteRenderer>();
                                if (sr != null && sr.sprite != null)
                                {
                                    saveManager.RegisterSpecialCaseNpc(hierarchicalId, npcTransform.gameObject, outfitTransform, livestreamActive.transform);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Register only Sword NPCs without collecting for display.
        /// </summary>
        private void RegisterSwordNpcsOnly(SaveManager saveManager, Dictionary<string, NpcGalleryData> tempNpcs)
        {
            GameObject swordMinigame = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "Sword_Minigame" && go.scene.name != null);
            
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
                            if (sr != null && sr.sprite != null)
                            {
                                // Pass parent transforms explicitly to avoid traversal issues with disabled parents
                                saveManager.RegisterSwordNpc(hierarchicalId, npcTransform.gameObject, sprites, enemy, swordMinigame.transform);
                            }
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Register only Milking NPCs without collecting for display.
        /// </summary>
        private void RegisterMilkingNpcsOnly(SaveManager saveManager, Dictionary<string, NpcGalleryData> tempNpcs)
        {
            GameObject milkingGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "MilkingMinigame" && go.scene.name != null);
            
            if (milkingGO != null)
            {
                Transform list = milkingGO.transform.Find("List");
                if (list != null)
                {
                    for (int charIndex = 0; charIndex < list.childCount; charIndex++)
                    {
                        Transform charTransform = list.GetChild(charIndex);
                        if (charTransform.gameObject.name.StartsWith("Character"))
                        {
                            for (int folderIndex = 0; folderIndex < charTransform.childCount; folderIndex++)
                            {
                                Transform folderTransform = charTransform.GetChild(folderIndex);
                                for (int npcIndex = 0; npcIndex < folderTransform.childCount; npcIndex++)
                                {
                                    Transform npcTransform = folderTransform.GetChild(npcIndex);
                                    string hierarchicalId = $"Milking_Minigame_{charTransform.gameObject.name}_{folderTransform.gameObject.name}_{npcTransform.gameObject.name}";
                                    
                                    SpriteRenderer sr = npcTransform.GetComponent<SpriteRenderer>();
                                    if (sr != null && sr.sprite != null)
                                    {
                                        // Pass parent transforms explicitly to avoid traversal issues with disabled parents
                                        saveManager.RegisterMilkingNpc(hierarchicalId, npcTransform.gameObject, folderTransform, charTransform, milkingGO.transform);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Register only Sprite_Overlay NPCs without collecting for display.
        /// </summary>
        private void RegisterSpriteOverlayNpcsOnly(SaveManager saveManager, Dictionary<string, NpcGalleryData> tempNpcs)
        {
            GameObject spriteOverlayGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "Sprite_Overlay" && go.scene.name != null);
            
            if (spriteOverlayGO != null)
            {
                for (int categoryIndex = 0; categoryIndex < spriteOverlayGO.transform.childCount; categoryIndex++)
                {
                    Transform categoryTransform = spriteOverlayGO.transform.GetChild(categoryIndex);
                    for (int npcIndex = 0; npcIndex < categoryTransform.childCount; npcIndex++)
                    {
                        Transform npcTransform = categoryTransform.GetChild(npcIndex);
                        string hierarchicalId = $"Sprite_Overlay_{categoryTransform.gameObject.name}_{npcTransform.gameObject.name}";
                        
                        SpriteRenderer sr = npcTransform.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            saveManager.RegisterSpriteOverlayNpc(hierarchicalId, npcTransform.gameObject);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Register only EventUI Photo NPCs without collecting for display.
        /// </summary>
        private void RegisterEventUIPhotosNpcsOnly(SaveManager saveManager, Dictionary<string, NpcGalleryData> tempNpcs)
        {
            GameObject eventUI = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "15_EventUI" && go.scene.name != null);
            
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
                                    // Pass parent transforms explicitly to avoid traversal issues with disabled parents
                                    saveManager.RegisterEventUIPhotoNpc(hierarchicalId, imageTransform.gameObject, adrianGO, adrianPC, eventUI.transform);
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
                                        // Pass parent transforms explicitly to avoid traversal issues with disabled parents
                                        saveManager.RegisterEventUIPhotoNpc(hierarchicalId, gameObjectChild.gameObject, otherGO, otherCore, eventUI.transform);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Register only Racing NPCs without collecting for display.
        /// </summary>
        private void RegisterRacingNpcsOnly(SaveManager saveManager, Dictionary<string, NpcGalleryData> tempNpcs)
        {
            GameObject racingGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "Racing_Minigame" && go.scene.name != null);
            
            if (racingGO != null)
            {
                Transform racingPC = racingGO.transform.Find("Racing_PC");
                if (racingPC != null)
                {
                    Transform racingBody = racingPC.Find("RacingBody");
                    if (racingBody != null)
                    {
                        // Register PCRacing1 from Racing_BikeTypes/Racing_PCAlone
                        Transform bikeTypes = racingBody.Find("Racing_BikeTypes");
                        if (bikeTypes != null)
                        {
                            Transform pcAlone = bikeTypes.Find("Racing_PCAlone");
                            if (pcAlone != null)
                            {
                                Transform pcRacing1 = pcAlone.Find("PCRacing1");
                                if (pcRacing1 != null)
                                {
                                    string hierarchicalId = $"Racing_Minigame_Racing Ally_{pcRacing1.gameObject.name}";
                                    saveManager.RegisterRacingNpc(hierarchicalId, pcRacing1.gameObject, pcAlone, bikeTypes, racingGO.transform);
                                }
                            }
                        }
                        
                        // Racing Guests
                        Transform guestGroup = racingBody.Find("Racing_Guest");
                        if (guestGroup != null)
                        {
                            foreach (Transform child in guestGroup)
                            {
                                string hierarchicalId = $"Racing_Minigame_Racing Guest_{child.gameObject.name}";
                                // Pass parent transforms explicitly to avoid traversal issues with disabled parents
                                saveManager.RegisterRacingNpc(hierarchicalId, child.gameObject, guestGroup, racingBody, racingGO.transform);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Register only Tabletop NPCs without collecting for display.
        /// </summary>
        private void RegisterTabletopNpcsOnly(SaveManager saveManager, Dictionary<string, NpcGalleryData> tempNpcs)
        {
            GameObject tabletopMinigameGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "Tabletop_Minigame" && go.scene.name != null);
            
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
                                // Pass parent transforms explicitly to avoid traversal issues with disabled parents
                                saveManager.RegisterTabletopNpc(hierarchicalId, fighter.gameObject, allyFighters, tabletopCanvas, tabletopMinigameGO.transform);
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
                                // Pass parent transforms explicitly to avoid traversal issues with disabled parents
                                saveManager.RegisterTabletopNpc(hierarchicalId, fighter.gameObject, enemyFighters, tabletopCanvas, tabletopMinigameGO.transform);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Register only Barista NPCs without collecting for display.
        /// </summary>
        private void RegisterBaristaNpcsOnly(SaveManager saveManager, Dictionary<string, NpcGalleryData> tempNpcs)
        {
            GameObject baristaGameGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "Barista_Game" && go.scene.name != null);
            
            if (baristaGameGO != null)
            {
                Transform canvas = baristaGameGO.transform.Find("Canvas");
                if (canvas != null)
                {
                    // Register regular Barista NPCs
                    Transform continueGO = canvas.Find("Continue");
                    if (continueGO != null)
                    {
                        Transform buyMenu = continueGO.Find("BuyMenu");
                        if (buyMenu != null)
                        {
                            foreach (Transform child in buyMenu)
                            {
                                // Skip inactive or template objects
                                if (child.gameObject.name != "0_empty" && child.gameObject.activeSelf)
                                {
                                    string hierarchicalId = $"Barista_Game_{child.gameObject.name}";
                                    // Pass parent transforms explicitly to avoid traversal issues with disabled parents
                                    saveManager.RegisterBaristaaNpc(hierarchicalId, child.gameObject, buyMenu, continueGO, baristaGameGO.transform);
                                }
                            }
                        }
                    }
                    
                    // Register FreeGob2 special NPC
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
                                        // Pass topLevelParent explicitly
                                        saveManager.RegisterBaristaSpecialNpc(hierarchicalId, content.gameObject, baristaGameGO.transform);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        private static void CollectLiveStreamNpcs(SaveManager saveManager, Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Use Resources.FindObjectsOfTypeAll to find Livestream_Active even if disabled
            GameObject livestreamActive = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "Livestream_Active" && go.scene.name != null);
            
            if (livestreamActive != null)
            {
                Debug.Log($"[Gallery] Found Livestream_Active GO");
                Transform liveAnna = livestreamActive.transform.Find("Live_Anna");
                if (liveAnna != null)
                {
                    Debug.Log($"[Gallery] Found Live_Anna");
                    
                    // Collect from Live_DefaultOutfit
                    CollectLiveStreamOutfitNpcs(liveAnna.Find("Live_DefaultOutfit"), "DefaultOutfit", saveManager, allNpcs);
                    
                    // Collect from Live_CoolOutfit
                    CollectLiveStreamOutfitNpcs(liveAnna.Find("Live_CoolOutfit"), "CoolOutfit", saveManager, allNpcs);
                    
                    // Collect from Live_SlutOutfit
                    CollectLiveStreamOutfitNpcs(liveAnna.Find("Live_SlutOutfit"), "SlutOutfit", saveManager, allNpcs);
                }
                else
                {
                    Debug.Log($"[Gallery] Could not find Live_Anna");
                }
            }
            else
            {
                Debug.Log($"[Gallery] Could not find Livestream_Active GO (may be inactive)");
            }
        }

        private static void CollectLiveStreamOutfitNpcs(Transform outfitTransform, string outfitName, SaveManager saveManager, Dictionary<string, NpcGalleryData> allNpcs)
        {
            if (outfitTransform == null)
            {
                Debug.Log($"[Gallery] Could not find Live_{outfitName}");
                return;
            }

            Debug.Log($"[Gallery] Found Live_{outfitName} with {outfitTransform.childCount} children");
            foreach (Transform child in outfitTransform)
            {
                string npcId = $"Livestream_{outfitName}_{child.gameObject.name}";
                Debug.Log($"[Gallery] Processing Livestream NPC: {npcId}");
                
                // Check if child has a SpriteRenderer
                SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    AddLiveStreamNpc(child, "Livestream", outfitName, allNpcs, saveManager);
                }
                else
                {
                    Debug.LogWarning($"[Gallery] Livestream NPC '{child.gameObject.name}' has no SpriteRenderer or sprite");
                }
            }
        }

        private static void AddLiveStreamNpc(Transform npcTransform, string category, string subcategory, Dictionary<string, NpcGalleryData> allNpcs, SaveManager saveManager)
        {
            GameObject npcGO = npcTransform.gameObject;
            string npcId = $"Livestream_{subcategory}_{npcGO.name}";
            
            SpriteRenderer spriteRenderer = npcGO.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                allNpcs[npcId] = new NpcGalleryData
                {
                    npcGO = npcGO,
                    mainSprite = spriteRenderer.sprite
                };
                
                // Register with SaveManager for seen-state tracking
                saveManager.RegisterLiveStreamNpc(npcId, npcGO);
                Debug.Log($"[Gallery] Added Livestream NPC: {npcId}");
            }
        }

        private static void CollectSwordNpcs(SaveManager saveManager, Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find Sword_Minigame using Resources.FindObjectsOfTypeAll (handles disabled GameObjects)
            GameObject swordMinigameGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "Sword_Minigame" && go.scene.name != null);
            
            if (swordMinigameGO != null)
            {
                Debug.Log($"[Gallery] Found Sword_Minigame GO");
                
                // Navigate to Enemy > Sprites
                Transform enemy = swordMinigameGO.transform.Find("Enemy");
                if (enemy != null)
                {
                    Transform sprites = enemy.Find("Sprites");
                    if (sprites != null)
                    {
                        Debug.Log($"[Gallery] Found Sword Enemy/Sprites with {sprites.childCount} children");
                        
                        // Process all direct children of Sprites as NPCs
                        for (int i = 0; i < sprites.childCount; i++)
                        {
                            Transform npcTransform = sprites.GetChild(i);
                            AddSwordNpc(npcTransform, "Sword_Minigame", "Sword Enemy", allNpcs, saveManager);
                        }
                    }
                    else
                    {
                        Debug.Log($"[Gallery] Could not find Sword Enemy/Sprites");
                    }
                }
                else
                {
                    Debug.Log($"[Gallery] Could not find Sword Enemy");
                }
            }
            else
            {
                Debug.Log($"[Gallery] Could not find Sword_Minigame GO (may be inactive)");
            }
        }

        private static void AddSwordNpc(Transform npcTransform, string levelName, string category, Dictionary<string, NpcGalleryData> allNpcs, SaveManager saveManager)
        {
            // Sword NPCs have their sprite directly in a SpriteRenderer on the GO
            SpriteRenderer spriteRenderer = npcTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[Gallery] Sword NPC '{npcTransform.gameObject.name}' has no SpriteRenderer");
                return;
            }

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"[Gallery] Sword NPC '{npcTransform.gameObject.name}' SpriteRenderer has no sprite");
                return;
            }

            // Create hierarchical ID for Sword NPC
            string npcName = npcTransform.gameObject.name;
            string hierarchicalNpcId = $"{levelName}_{category}_{npcName}";

            if (!allNpcs.ContainsKey(hierarchicalNpcId))
            {
                NpcGalleryData data = new NpcGalleryData
                {
                    npcGO = npcTransform.gameObject,
                    mainSprite = spriteRenderer.sprite,
                    secondarySprites = new List<Sprite>()
                };

                allNpcs[hierarchicalNpcId] = data;
                Debug.Log($"[Gallery] Added Sword NPC: {hierarchicalNpcId}");

                // Register with SaveManager for seen tracking
                // For Sword NPCs, we need to check if the NPC and all its ancestors are enabled
                if (saveManager != null)
                {
                    saveManager.RegisterSwordNpc(hierarchicalNpcId, npcTransform.gameObject);
                }
            }
        }

        private static void CollectStageNpcs(SaveManager saveManager, Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find 5_Levels using Resources.FindObjectsOfTypeAll (handles disabled GameObjects)
            GameObject levelsGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "5_Levels" && go.scene.name != null);

            if (levelsGO == null)
            {
                Debug.Log($"[Gallery] Could not find 5_Levels GO for Stage NPCs");
                return;
            }

            // Navigate to 55_Stage > AnnaOnStage_Sprite
            Transform stage = levelsGO.transform.Find("55_Stage");
            if (stage == null)
            {
                Debug.Log($"[Gallery] Could not find 55_Stage in 5_Levels");
                return;
            }

            Transform annaOnStageSprite = stage.Find("AnnaOnStage_Sprite");
            if (annaOnStageSprite == null)
            {
                Debug.Log($"[Gallery] Could not find AnnaOnStage_Sprite in 55_Stage");
                return;
            }

            Debug.Log($"[Gallery] Found AnnaOnStage_Sprite with {annaOnStageSprite.childCount} children");

            // Collect the three specific NPCs: AnnaOnStage_0, AnnaOnStage_0 (1), AnnaOnStage_0 (2)
            string[] npcNames = new string[] { "AnnaOnStage_0", "AnnaOnStage_0 (1)", "AnnaOnStage_0 (2)" };
            foreach (string npcName in npcNames)
            {
                Transform npcTransform = annaOnStageSprite.Find(npcName);
                if (npcTransform != null)
                {
                    AddStageNpc(npcTransform, allNpcs, saveManager);
                }
                else
                {
                    Debug.LogWarning($"[Gallery] Could not find Stage NPC: {npcName}");
                }
            }
        }

        private static void AddStageNpc(Transform npcTransform, Dictionary<string, NpcGalleryData> allNpcs, SaveManager saveManager)
        {
            // Stage NPCs have their sprite directly in a SpriteRenderer on the GO
            SpriteRenderer spriteRenderer = npcTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[Gallery] Stage NPC '{npcTransform.gameObject.name}' has no SpriteRenderer");
                return;
            }

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"[Gallery] Stage NPC '{npcTransform.gameObject.name}' SpriteRenderer has no sprite");
                return;
            }

            // Create hierarchical ID for Stage NPC
            string npcName = npcTransform.gameObject.name.Replace(" ", "_").Replace("(", "").Replace(")", "");
            string hierarchicalNpcId = $"5_Levels_55_Stage_AnnaOnStage_{npcName}";

            if (!allNpcs.ContainsKey(hierarchicalNpcId))
            {
                NpcGalleryData data = new NpcGalleryData
                {
                    npcGO = npcTransform.gameObject,
                    mainSprite = spriteRenderer.sprite,
                    secondarySprites = new List<Sprite>()
                };

                allNpcs[hierarchicalNpcId] = data;
                Debug.Log($"[Gallery] Added Stage NPC: {hierarchicalNpcId}");

                // Register with SaveManager for seen tracking
                if (saveManager != null)
                {
                    saveManager.RegisterStageNpc(hierarchicalNpcId, npcTransform.gameObject);
                }
            }
        }

        private static void CollectConventionNpcs(SaveManager saveManager, Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find 5_Levels using Resources.FindObjectsOfTypeAll (handles disabled GameObjects)
            GameObject levelsGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "5_Levels" && go.scene.name != null);

            if (levelsGO == null)
            {
                Debug.Log($"[Gallery] Could not find 5_Levels GO for Convention NPCs");
                return;
            }

            // Navigate to 54_Convention
            Transform convention = levelsGO.transform.Find("54_Convention");
            if (convention == null)
            {
                Debug.Log($"[Gallery] Could not find 54_Convention in 5_Levels");
                return;
            }

            Debug.Log($"[Gallery] Found 54_Convention with {convention.childCount} children");

            // Collect all direct children whose names start with Convention_NPC_1 or Convenation_Ad_1
            for (int i = 0; i < convention.childCount; i++)
            {
                Transform childTransform = convention.GetChild(i);
                string childName = childTransform.gameObject.name;

                if (childName.StartsWith("Convention_NPC_1") || childName.StartsWith("Convenation_Ad_1"))
                {
                    AddConventionNpc(childTransform, allNpcs, saveManager);
                }
            }
        }

        private static void AddConventionNpc(Transform npcTransform, Dictionary<string, NpcGalleryData> allNpcs, SaveManager saveManager)
        {
            // Convention NPCs have their sprite directly in a SpriteRenderer on the GO
            SpriteRenderer spriteRenderer = npcTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[Gallery] Convention NPC '{npcTransform.gameObject.name}' has no SpriteRenderer");
                return;
            }

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"[Gallery] Convention NPC '{npcTransform.gameObject.name}' SpriteRenderer has no sprite");
                return;
            }

            // Create hierarchical ID for Convention NPC
            string npcName = npcTransform.gameObject.name.Replace(" ", "_").Replace("(", "").Replace(")", "");
            string hierarchicalNpcId = $"5_Levels_54_Convention_{npcName}";

            if (!allNpcs.ContainsKey(hierarchicalNpcId))
            {
                NpcGalleryData data = new NpcGalleryData
                {
                    npcGO = npcTransform.gameObject,
                    mainSprite = spriteRenderer.sprite,
                    secondarySprites = new List<Sprite>()
                };

                allNpcs[hierarchicalNpcId] = data;
                Debug.Log($"[Gallery] Added Convention NPC: {hierarchicalNpcId}");

                // Register with SaveManager for seen tracking
                if (saveManager != null)
                {
                    saveManager.RegisterConventionNpc(hierarchicalNpcId, npcTransform.gameObject);
                }
            }
        }


        /// <summary>
        /// Add Milking NPCs to gallery display only (don't register with SaveManager - already done early)
        /// Only add DefaultFace variant with matching ID to SaveManager registration
        /// </summary>
        private static void AddMilkingNpcsToGallery(Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find MilkingMinigame using Resources.FindObjectsOfTypeAll (handles disabled GameObjects)
            GameObject milkingMinigameGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "MilkingMinigame" && go.scene.name != null);
            
            if (milkingMinigameGO != null)
            {
                Debug.Log($"[Gallery] Adding Milking NPCs to gallery");
                
                // Navigate to List
                Transform list = milkingMinigameGO.transform.Find("List");
                if (list != null)
                {
                    // Process each character folder (1_Anna, 4_Katarina, 5_Emma, 6_Amelia, 7_Phoenix)
                    for (int i = 0; i < list.childCount; i++)
                    {
                        Transform characterFolder = list.GetChild(i);
                        string characterName = characterFolder.gameObject.name;
                        
                        // Skip empty/control folders
                        if (characterName.StartsWith("X_") || characterName == "0_Empty")
                        {
                            continue;
                        }
                        
                        // Only add DefaultFace with matching SaveManager ID format: Milking_Minigame_{characterName}
                        Transform defaultFolder = characterFolder.Find("Default");
                        if (defaultFolder != null)
                        {
                            // DefaultFace - this is the one tracked by SaveManager
                            Transform defaultFace = defaultFolder.Find("DefaultFace");
                            if (defaultFace != null)
                            {
                                SpriteRenderer sr = defaultFace.GetComponent<SpriteRenderer>();
                                if (sr != null && sr.sprite != null)
                                {
                                    // Match SaveManager's ID format exactly
                                    string hierarchicalNpcId = $"Milking_Minigame_{characterName}";
                                    
                                    if (!allNpcs.ContainsKey(hierarchicalNpcId))
                                    {
                                        NpcGalleryData data = new NpcGalleryData
                                        {
                                            npcGO = defaultFace.gameObject,
                                            mainSprite = sr.sprite,
                                            secondarySprites = new List<Sprite>()
                                        };

                                        allNpcs[hierarchicalNpcId] = data;
                                        Debug.Log($"[Gallery] Added Milking NPC to gallery: {hierarchicalNpcId}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void AddTabletopNpcsToGallery(Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find Tabletop_Minigame using Resources.FindObjectsOfTypeAll (handles disabled GameObjects)
            GameObject tabletopMinigameGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "Tabletop_Minigame" && go.scene.name != null);
            
            if (tabletopMinigameGO != null)
            {
                Debug.Log($"[Gallery] Adding Tabletop NPCs to gallery");
                
                Transform tabletopCanvas = tabletopMinigameGO.transform.Find("Tabletop_Canvas");
                if (tabletopCanvas != null)
                {
                    // Add Ally Fighters
                    Transform allyFighters = tabletopCanvas.Find("Tabletop Ally Fighters");
                    if (allyFighters != null)
                    {
                        for (int i = 0; i < allyFighters.childCount; i++)
                        {
                            Transform fighter = allyFighters.GetChild(i);
                            string fighterName = fighter.gameObject.name;
                            
                            // Skip 0_empty
                            if (fighterName == "0_empty")
                            {
                                continue;
                            }
                            
                            // Look for Char_Sprite with Image component
                            Transform charSprite = fighter.Find("Char_Sprite");
                            if (charSprite != null)
                            {
                                UnityEngine.UI.Image imageComponent = charSprite.GetComponent<UnityEngine.UI.Image>();
                                if (imageComponent != null && imageComponent.sprite != null)
                                {
                                    // Match SaveManager's ID format exactly
                                    string hierarchicalNpcId = $"Tabletop_Minigame_Tabletop Ally Fighters_{fighterName}";
                                    
                                    if (!allNpcs.ContainsKey(hierarchicalNpcId))
                                    {
                                        NpcGalleryData data = new NpcGalleryData
                                        {
                                            npcGO = charSprite.gameObject,
                                            mainSprite = imageComponent.sprite,
                                            secondarySprites = new List<Sprite>()
                                        };

                                        allNpcs[hierarchicalNpcId] = data;
                                        Debug.Log($"[Gallery] Added Tabletop Ally NPC to gallery: {hierarchicalNpcId}");
                                    }
                                }
                            }
                        }
                    }
                    
                    // Add Enemy Fighters
                    Transform enemyFighters = tabletopCanvas.Find("Tabletop Enemy Fighters");
                    if (enemyFighters != null)
                    {
                        for (int i = 0; i < enemyFighters.childCount; i++)
                        {
                            Transform fighter = enemyFighters.GetChild(i);
                            string fighterName = fighter.gameObject.name;
                            
                            // Skip 0_empty
                            if (fighterName == "0_empty")
                            {
                                continue;
                            }
                            
                            // Look for Char_Sprite with Image component
                            Transform charSprite = fighter.Find("Char_Sprite");
                            if (charSprite != null)
                            {
                                UnityEngine.UI.Image imageComponent = charSprite.GetComponent<UnityEngine.UI.Image>();
                                if (imageComponent != null && imageComponent.sprite != null)
                                {
                                    // Match SaveManager's ID format exactly
                                    string hierarchicalNpcId = $"Tabletop_Minigame_Tabletop Enemy Fighters_{fighterName}";
                                    
                                    if (!allNpcs.ContainsKey(hierarchicalNpcId))
                                    {
                                        NpcGalleryData data = new NpcGalleryData
                                        {
                                            npcGO = charSprite.gameObject,
                                            mainSprite = imageComponent.sprite,
                                            secondarySprites = new List<Sprite>()
                                        };

                                        allNpcs[hierarchicalNpcId] = data;
                                        Debug.Log($"[Gallery] Added Tabletop Enemy NPC to gallery: {hierarchicalNpcId}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void AddLiveStreamNpcsToGallery(Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find Livestream_Active using Resources.FindObjectsOfTypeAll (handles disabled GameObjects)
            GameObject livestreamActiveGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "Livestream_Active" && go.scene.name != null);
            
            if (livestreamActiveGO != null)
            {
                Debug.Log($"[Gallery] Adding Livestream NPCs to gallery");
                
                Transform liveAnna = livestreamActiveGO.transform.Find("Live_Anna");
                if (liveAnna != null)
                {
                    // Add from all three outfits: DefaultOutfit, CoolOutfit, SlutOutfit
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
                                    string hierarchicalNpcId = $"Livestream_{outfitNameShort}_{npcName}";
                                    
                                    if (!allNpcs.ContainsKey(hierarchicalNpcId))
                                    {
                                        NpcGalleryData data = new NpcGalleryData
                                        {
                                            npcGO = npcTransform.gameObject,
                                            mainSprite = sr.sprite,
                                            secondarySprites = new List<Sprite>()
                                        };

                                        allNpcs[hierarchicalNpcId] = data;
                                        Debug.Log($"[Gallery] Added Livestream NPC to gallery: {hierarchicalNpcId}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void AddEventUIPhotosNpcsToGallery(Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find 15_EventUI using Resources.FindObjectsOfTypeAll (handles disabled GameObjects)
            GameObject eventUIGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "15_EventUI" && go.scene.name != null);
            
            if (eventUIGO != null)
            {
                Debug.Log($"[Gallery] Adding EventUI NPCs to gallery");
                
                Transform bpcNew = eventUIGO.transform.Find("BPC_New");
                if (bpcNew != null)
                {
                    // Add Adrian photos
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
                                        // Match SaveManager's ID format exactly
                                        string hierarchicalNpcId = $"EventUI_Adrian_Image ({i})";
                                        
                                        if (!allNpcs.ContainsKey(hierarchicalNpcId))
                                        {
                                            NpcGalleryData data = new NpcGalleryData
                                            {
                                                npcGO = imageTransform.gameObject,
                                                mainSprite = imageComponent.sprite,
                                                secondarySprites = new List<Sprite>()
                                            };

                                            allNpcs[hierarchicalNpcId] = data;
                                            Debug.Log($"[Gallery] Added EventUI Adrian NPC to gallery: {hierarchicalNpcId}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    // Add Other character photos - only GameObject (4)
                    Transform otherPC = bpcNew.Find("OtherPC_Browser");
                    if (otherPC != null)
                    {
                        Transform otherCore = otherPC.Find("OtherPC_Core");
                        if (otherCore != null)
                        {
                            // Only add GameObject (4)
                            Transform gameObjTransform = otherCore.Find("GameObject (4)");
                            if (gameObjTransform != null)
                            {
                                UnityEngine.UI.Image imageComponent = gameObjTransform.GetComponent<UnityEngine.UI.Image>();
                                if (imageComponent != null && imageComponent.sprite != null)
                                {
                                    // Match SaveManager's ID format exactly
                                    string hierarchicalNpcId = $"EventUI_Other_GameObject (4)";
                                    
                                    if (!allNpcs.ContainsKey(hierarchicalNpcId))
                                    {
                                        NpcGalleryData data = new NpcGalleryData
                                        {
                                            npcGO = gameObjTransform.gameObject,
                                            mainSprite = imageComponent.sprite,
                                            secondarySprites = new List<Sprite>()
                                        };

                                        allNpcs[hierarchicalNpcId] = data;
                                        Debug.Log($"[Gallery] Added EventUI Other NPC to gallery: {hierarchicalNpcId}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void AddBafaNpcsToGallery(Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find Bafa_Sprites using Resources.FindObjectsOfTypeAll (handles disabled GameObjects)
            GameObject bafaSpritesGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "Bafa_Sprites" && go.scene.name != null);
            
            if (bafaSpritesGO != null)
            {
                Debug.Log($"[Gallery] Adding Bafa NPCs to gallery");
                
                // Iterate through all Bafa NPC folders
                for (int i = 0; i < bafaSpritesGO.transform.childCount; i++)
                {
                    Transform npcTransform = bafaSpritesGO.transform.GetChild(i);
                    string npcName = npcTransform.gameObject.name;
                    
                    // Skip 0_Empty
                    if (npcName == "0_Empty")
                    {
                        continue;
                    }
                    
                    // Look for SpriteRenderer component on the NPC
                    SpriteRenderer spriteRenderer = npcTransform.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        // Match SaveManager's ID format exactly
                        string hierarchicalNpcId = $"Bafa_{npcName}";
                        
                        if (!allNpcs.ContainsKey(hierarchicalNpcId))
                        {
                            // Collect secondary sprites (Bafa_Boobs, Bafa_Boobs (1), Bafa_Dick)
                            List<Sprite> secondarySprites = new List<Sprite>();
                            string[] secondarySpriteNames = new string[] { "Bafa_Boobs", "Bafa_Boobs (1)", "Bafa_Dick" };
                            foreach (string spriteName in secondarySpriteNames)
                            {
                                Transform spriteTransform = npcTransform.Find(spriteName);
                                if (spriteTransform != null)
                                {
                                    SpriteRenderer secondaryRenderer = spriteTransform.GetComponent<SpriteRenderer>();
                                    if (secondaryRenderer != null && secondaryRenderer.sprite != null)
                                    {
                                        secondarySprites.Add(secondaryRenderer.sprite);
                                    }
                                }
                            }
                            
                            NpcGalleryData data = new NpcGalleryData
                            {
                                npcGO = npcTransform.gameObject,
                                mainSprite = spriteRenderer.sprite,
                                secondarySprites = secondarySprites
                            };
                            
                            allNpcs[hierarchicalNpcId] = data;
                            Debug.Log($"[Gallery] Added Bafa NPC to gallery: {hierarchicalNpcId}");
                        }
                    }
                }
            }
        }

        private static void AddStarmakerCreatorNpcsToGallery(Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find 9_MainCanvas using Resources.FindObjectsOfTypeAll (handles disabled GameObjects)
            GameObject mainCanvasGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "9_MainCanvas" && go.scene.name != null);
            
            if (mainCanvasGO != null)
            {
                Debug.Log($"[Gallery] Looking for Starmaker Creator NPCs");
                
                Transform starmaker = mainCanvasGO.transform.Find("Starmaker");
                if (starmaker != null)
                {
                    Transform browseCreators = starmaker.Find("Browse_Creators");
                    if (browseCreators != null)
                    {
                        Transform creatorList = browseCreators.Find("CreatorList");
                        if (creatorList != null)
                        {
                            // Define which creators to collect and their Image child names
                            var creatorsToCollect = new (string creatorFolder, string npcName)[]
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
                                    // Look for Image component directly in the Creator folder
                                    Transform imageTransform = creatorTransform.Find("Image");
                                    if (imageTransform != null)
                                    {
                                        UnityEngine.UI.Image imageComponent = imageTransform.GetComponent<UnityEngine.UI.Image>();
                                        if (imageComponent != null && imageComponent.sprite != null)
                                        {
                                            if (!allNpcs.ContainsKey(npcId))
                                            {
                                                NpcGalleryData data = new NpcGalleryData
                                                {
                                                    npcGO = imageTransform.gameObject,
                                                    mainSprite = imageComponent.sprite,
                                                    secondarySprites = new List<Sprite>()
                                                };
                                                
                                                allNpcs[npcId] = data;
                                                Debug.Log($"[Gallery] Added Starmaker Creator NPC to gallery: {npcId}");
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogWarning($"[Gallery] Image component not found or has no sprite in {creatorFolder}/Image");
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"[Gallery] Image child not found in {creatorFolder}");
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning($"[Gallery] Creator folder not found: {creatorFolder}");
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[Gallery] CreatorList not found under Browse_Creators");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[Gallery] Browse_Creators not found under Starmaker");
                    }
                }
                else
                {
                    Debug.LogWarning($"[Gallery] Starmaker not found in 9_MainCanvas");
                }
            }
            else
            {
                Debug.LogWarning($"[Gallery] 9_MainCanvas not found");
            }
        }

        private static void AddXMoviesNpcsToGallery(Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find X_Movies using Resources.FindObjectsOfTypeAll (handles disabled GameObjects)
            GameObject xMoviesGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "X_Movies" && go.scene.name != null);
            
            if (xMoviesGO != null)
            {
                Debug.Log($"[Gallery] Adding X_Movies NPCs to gallery");
                
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
                
                // Add all X_Movies NPCs to gallery
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
                                SpriteRenderer spriteRenderer = npcTransform.GetComponent<SpriteRenderer>();
                                if (spriteRenderer != null && spriteRenderer.sprite != null)
                                {
                                    // Match SaveManager's ID format exactly
                                    string hierarchicalNpcId = $"X_Movies_{movieType}_{slidePath}_{npcName}";
                                    
                                    if (!allNpcs.ContainsKey(hierarchicalNpcId))
                                    {
                                        NpcGalleryData data = new NpcGalleryData
                                        {
                                            npcGO = npcTransform.gameObject,
                                            mainSprite = spriteRenderer.sprite,
                                            secondarySprites = new List<Sprite>()
                                        };
                                        
                                        allNpcs[hierarchicalNpcId] = data;
                                        Debug.Log($"[Gallery] Added X_Movies NPC to gallery: {hierarchicalNpcId}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void CollectMilkingNpcs(SaveManager saveManager, Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find MilkingMinigame using Resources.FindObjectsOfTypeAll (handles disabled GameObjects)
            GameObject milkingMinigameGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "MilkingMinigame" && go.scene.name != null);
            
            if (milkingMinigameGO != null)
            {
                Debug.Log($"[Gallery] Found MilkingMinigame GO");
                
                // Navigate to List
                Transform list = milkingMinigameGO.transform.Find("List");
                if (list != null)
                {
                    Debug.Log($"[Gallery] Found Milking List with {list.childCount} children");
                    
                    // Process each character folder (1_Anna, 4_Katarina, 5_Emma, 6_Amelia, 7_Phoenix, 8_Kirby)
                    for (int i = 0; i < list.childCount; i++)
                    {
                        Transform characterFolder = list.GetChild(i);
                        string characterName = characterFolder.gameObject.name;
                        
                        // Process Default folder (DefaultFace and MilkedFace)
                        Transform defaultFolder = characterFolder.Find("Default");
                        if (defaultFolder != null)
                        {
                            // DefaultFace
                            Transform defaultFace = defaultFolder.Find("DefaultFace");
                            if (defaultFace != null)
                            {
                                Debug.Log($"[Gallery] Processing Milking NPC: {characterName}_DefaultFace");
                                AddMilkingNpc(defaultFace, "MilkingMinigame", characterName, "DefaultFace", allNpcs, saveManager);
                            }
                            
                            // MilkedFace
                            Transform milkedFace = defaultFolder.Find("MilkedFace");
                            if (milkedFace != null)
                            {
                                Debug.Log($"[Gallery] Processing Milking NPC: {characterName}_MilkedFace");
                                AddMilkingNpc(milkedFace, "MilkingMinigame", characterName, "MilkedFace", allNpcs, saveManager);
                            }
                        }
                        else
                        {
                            Debug.Log($"[Gallery] Could not find Default folder for {characterName}");
                        }
                        
                        // Process Squeeze folder (with RightBreast and LeftBreast as secondary sprites)
                        Transform squeezeFolder = characterFolder.Find("Squeeze");
                        if (squeezeFolder != null)
                        {
                            Debug.Log($"[Gallery] Processing Milking NPC: {characterName}_Squeeze");
                            AddMilkingNpcWithSecondaries(squeezeFolder, "MilkingMinigame", characterName, "Squeeze", allNpcs, saveManager);
                        }
                        else
                        {
                            Debug.Log($"[Gallery] Could not find Squeeze folder for {characterName}");
                        }
                    }
                }
                else
                {
                    Debug.Log($"[Gallery] Could not find MilkingMinigame List");
                }
            }
            else
            {
                Debug.Log($"[Gallery] Could not find MilkingMinigame GO (may be inactive)");
            }
        }

        private static void AddMilkingNpc(Transform npcTransform, string minigameName, string characterName, string variant, Dictionary<string, NpcGalleryData> allNpcs, SaveManager saveManager)
        {
            // Milking NPCs have their sprite directly in a SpriteRenderer on the GO
            SpriteRenderer spriteRenderer = npcTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[Gallery] Milking NPC '{characterName}_{variant}' has no SpriteRenderer");
                return;
            }

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"[Gallery] Milking NPC '{characterName}_{variant}' SpriteRenderer has no sprite");
                return;
            }

            // Create hierarchical ID for Milking NPC
            string hierarchicalNpcId = $"{minigameName}_{characterName}_{variant}";

            if (!allNpcs.ContainsKey(hierarchicalNpcId))
            {
                NpcGalleryData data = new NpcGalleryData
                {
                    npcGO = npcTransform.gameObject,
                    mainSprite = spriteRenderer.sprite,
                    secondarySprites = new List<Sprite>()
                };

                allNpcs[hierarchicalNpcId] = data;
                Debug.Log($"[Gallery] Added Milking NPC: {hierarchicalNpcId}");

                // Register with SaveManager for seen tracking
                if (saveManager != null)
                {
                    saveManager.RegisterMilkingNpc(hierarchicalNpcId, npcTransform.gameObject);
                }
            }
        }

        private static void AddMilkingNpcWithSecondaries(Transform squeezeTransform, string minigameName, string characterName, string variant, Dictionary<string, NpcGalleryData> allNpcs, SaveManager saveManager)
        {
            // Squeeze folder itself contains the main sprite
            SpriteRenderer spriteRenderer = squeezeTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[Gallery] Milking NPC '{characterName}_{variant}' has no SpriteRenderer");
                return;
            }

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"[Gallery] Milking NPC '{characterName}_{variant}' SpriteRenderer has no sprite");
                return;
            }

            // Create hierarchical ID for Milking NPC
            string hierarchicalNpcId = $"{minigameName}_{characterName}_{variant}";

            if (!allNpcs.ContainsKey(hierarchicalNpcId))
            {
                // Collect secondary sprites (RightBreast and LeftBreast)
                List<Sprite> secondarySprites = new List<Sprite>();
                
                Transform rightBreast = squeezeTransform.Find("RightBreast");
                if (rightBreast != null)
                {
                    SpriteRenderer rightBreastRenderer = rightBreast.GetComponent<SpriteRenderer>();
                    if (rightBreastRenderer != null && rightBreastRenderer.sprite != null)
                    {
                        secondarySprites.Add(rightBreastRenderer.sprite);
                    }
                }
                
                Transform leftBreast = squeezeTransform.Find("LeftBreast");
                if (leftBreast != null)
                {
                    SpriteRenderer leftBreastRenderer = leftBreast.GetComponent<SpriteRenderer>();
                    if (leftBreastRenderer != null && leftBreastRenderer.sprite != null)
                    {
                        secondarySprites.Add(leftBreastRenderer.sprite);
                    }
                }

                NpcGalleryData data = new NpcGalleryData
                {
                    npcGO = squeezeTransform.gameObject,
                    mainSprite = spriteRenderer.sprite,
                    secondarySprites = secondarySprites
                };

                allNpcs[hierarchicalNpcId] = data;
                Debug.Log($"[Gallery] Added Milking NPC: {hierarchicalNpcId} (with {secondarySprites.Count} secondary sprites)");

                // Register with SaveManager for seen tracking
                if (saveManager != null)
                {
                    saveManager.RegisterMilkingNpc(hierarchicalNpcId, squeezeTransform.gameObject);
                }
            }
        }

        private static void CollectSpriteOverlayNpcs(SaveManager saveManager, Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find Sprite_Overlay via Resources.FindObjectsOfTypeAll (may be inactive)
            GameObject spriteOverlayGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "Sprite_Overlay" && go.scene.name != null);

            if (spriteOverlayGO == null)
            {
                Debug.Log("[Gallery] Sprite_Overlay not found (may be inactive)");
                return;
            }

            Debug.Log("[Gallery] Found Sprite_Overlay GO");

            // Process Weightlifting NPCs: Sprite_Overlay > Weightlifting > Weightlifting2
            Transform weightliftingFolder = spriteOverlayGO.transform.Find("Weightlifting");
            if (weightliftingFolder != null)
            {
                Debug.Log("[Gallery] Found Weightlifting folder");
                Transform weightlifting2 = weightliftingFolder.Find("Weightlifting2");
                if (weightlifting2 != null)
                {
                    AddSpriteOverlayNpc(weightlifting2, "Sprite_Overlay", "Weightlifting", "Weightlifting2", allNpcs, saveManager);
                }

                Transform weightlifting2Alt = weightliftingFolder.Find("Weightlifting2 (1)");
                if (weightlifting2Alt != null)
                {
                    AddSpriteOverlayNpc(weightlifting2Alt, "Sprite_Overlay", "Weightlifting", "Weightlifting2 (1)", allNpcs, saveManager);
                }
            }
            else
            {
                Debug.Log("[Gallery] Weightlifting folder not found");
            }

            // Process Sam Gaming NPCs: Sprite_Overlay > Sam_Gamingnight_Loading > GamingCSMage01
            Transform samGamingFolder = spriteOverlayGO.transform.Find("Sam_Gamingnight_Loading");
            if (samGamingFolder != null)
            {
                Debug.Log("[Gallery] Found Sam_Gamingnight_Loading folder");
                Transform gamingCSMage01 = samGamingFolder.Find("GamingCSMage01");
                if (gamingCSMage01 != null)
                {
                    AddSpriteOverlayNpc(gamingCSMage01, "Sprite_Overlay", "Sam_Gaming", "GamingCSMage01", allNpcs, saveManager);
                }

                Transform gamingCSMage01Alt = samGamingFolder.Find("GamingCSMage01 (1)");
                if (gamingCSMage01Alt != null)
                {
                    AddSpriteOverlayNpc(gamingCSMage01Alt, "Sprite_Overlay", "Sam_Gaming", "GamingCSMage01 (1)", allNpcs, saveManager);
                }
            }
            else
            {
                Debug.Log("[Gallery] Sam_Gamingnight_Loading folder not found");
            }
        }

        private static void AddSpriteOverlayNpc(Transform npcTransform, string minigameName, string category, string npcName, Dictionary<string, NpcGalleryData> allNpcs, SaveManager saveManager)
        {
            SpriteRenderer spriteRenderer = npcTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[Gallery] Sprite_Overlay NPC '{npcName}' has no SpriteRenderer");
                return;
            }

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"[Gallery] Sprite_Overlay NPC '{npcName}' SpriteRenderer has no sprite");
                return;
            }

            // Create hierarchical ID for Sprite_Overlay NPC
            // NOTE: Sprite_Overlay NPCs are now registered early in SaveManager.CollectMinigameNpcs()
            // This method only adds them to the gallery display
            string hierarchicalNpcId = $"{minigameName}_{category}/{npcName}";

            if (!allNpcs.ContainsKey(hierarchicalNpcId))
            {
                NpcGalleryData data = new NpcGalleryData
                {
                    npcGO = npcTransform.gameObject,
                    mainSprite = spriteRenderer.sprite,
                    secondarySprites = new List<Sprite>()
                };

                allNpcs[hierarchicalNpcId] = data;
                Debug.Log($"[Gallery] Added Sprite_Overlay NPC: {hierarchicalNpcId}");
            }
        }

        private static void CollectCompareitScenes(Dictionary<string, (GameObject sceneGO, Sprite sprite)> allScenes, SaveManager saveManager)
        {
            // Find 16_Minigames via Resources.FindObjectsOfTypeAll (may be inactive)
            GameObject minigamesGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "16_Minigames" && go.scene.name != null);

            if (minigamesGO == null)
            {
                Debug.Log("[Gallery] 16_Minigames not found (may be inactive)");
                return;
            }

            Debug.Log("[Gallery] Found 16_Minigames GO");

            // Navigate to Compareit > Avatars
            Transform compareitFolder = minigamesGO.transform.Find("Compareit");
            if (compareitFolder == null)
            {
                Debug.Log("[Gallery] Compareit folder not found");
                return;
            }

            Transform avatarsFolder = compareitFolder.Find("Avatars");
            if (avatarsFolder == null)
            {
                Debug.Log("[Gallery] Avatars folder not found");
                return;
            }

            Debug.Log($"[Gallery] Found Avatars folder with {avatarsFolder.childCount} children");

            // Iterate through all direct children (except "Empty")
            for (int i = 0; i < avatarsFolder.childCount; i++)
            {
                Transform avatarTransform = avatarsFolder.GetChild(i);
                GameObject avatarGO = avatarTransform.gameObject;

                // Skip "Empty" or inactive objects
                if (avatarGO.name == "Empty" || !avatarGO.activeSelf)
                {
                    Debug.Log($"[Gallery] Skipping Compareit avatar: {avatarGO.name}");
                    continue;
                }

                // Get the Image component from this GO
                UnityEngine.UI.Image imageComponent = avatarGO.GetComponent<UnityEngine.UI.Image>();
                if (imageComponent == null)
                {
                    Debug.LogWarning($"[Gallery] Compareit avatar '{avatarGO.name}' has no Image component");
                    continue;
                }

                if (imageComponent.sprite == null)
                {
                    Debug.LogWarning($"[Gallery] Compareit avatar '{avatarGO.name}' Image has no sprite");
                    continue;
                }

                // Create a scene ID for this avatar
                string sceneId = $"Compareit_{avatarGO.name}";

                // Add to allScenes if not already present
                if (!allScenes.ContainsKey(sceneId))
                {
                    allScenes[sceneId] = (avatarGO, imageComponent.sprite);
                    Debug.Log($"[Gallery] Added Compareit scene: {sceneId}");

                    // Register with SaveManager using the same method as other special scenes
                    // This ensures Compareit scenes are tracked in sceneGameObjects and monitored
                    // for activation in MonitorSceneActivation(), making them behave identically
                    // to EventUI and Starmaker special scenes
                    if (saveManager != null)
                    {
                        saveManager.RegisterSceneForTracking(sceneId, avatarGO);
                    }
                }
            }
        }

        private static void CollectSpecialScenes(Dictionary<string, (GameObject sceneGO, Sprite sprite)> allScenes, SaveManager saveManager)
        {
            // Use SpecialSceneCollector to gather all special scenes from EventUI
            SpecialSceneCollector specialSceneCollector = new SpecialSceneCollector(saveManager);
            Dictionary<string, SpecialSceneCollector.SpecialSceneData> specialScenes = specialSceneCollector.CollectAll();

            // Add all special scenes to allScenes with proper registration for tracking
            foreach (var specialSceneEntry in specialScenes)
            {
                string sceneId = specialSceneEntry.Key;
                GameObject sceneGO = specialSceneEntry.Value.sceneGO;
                Sprite sprite = specialSceneEntry.Value.sprite;

                if (!allScenes.ContainsKey(sceneId))
                {
                    allScenes[sceneId] = (sceneGO, sprite);
                    Debug.Log($"[Gallery] Added special scene to allScenes: {sceneId}");

                    // Register with SaveManager for proper activation monitoring
                    if (saveManager != null)
                    {
                        saveManager.RegisterSceneForTracking(sceneId, sceneGO);
                    }
                }
            }
        }

        private static void CollectEventUIPhotosNpcs(SaveManager saveManager, Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find 15_EventUI via Resources.FindObjectsOfTypeAll (may be inactive)
            GameObject eventUIGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(),
                go => go.name == "15_EventUI" && go.scene.name != null);

            if (eventUIGO == null)
            {
                Debug.Log("[Gallery] 15_EventUI not found (may be inactive)");
                return;
            }

            Debug.Log("[Gallery] Found 15_EventUI GO");

            // Process AdrianPC_Wor NPCs: 15_EventUI > BPC_New > AdrianPC_Wor > GameObject > Image
            Transform bpcNew = eventUIGO.transform.Find("BPC_New");
            if (bpcNew != null)
            {
                Debug.Log("[Gallery] Found BPC_New");
                
                Transform adrianPCWor = bpcNew.Find("AdrianPC_Wor");
                if (adrianPCWor != null)
                {
                    Debug.Log("[Gallery] Found AdrianPC_Wor");
                    
                    Transform gameObject = adrianPCWor.Find("GameObject");
                    if (gameObject != null)
                    {
                        Debug.Log("[Gallery] Found AdrianPC_Wor GameObject");
                        
                        // Process Image, Image (1), Image (2)
                        for (int i = 0; i < 3; i++)
                        {
                            Transform imageTransform;
                            if (i == 0)
                            {
                                imageTransform = gameObject.Find("Image");
                            }
                            else
                            {
                                imageTransform = gameObject.Find($"Image ({i})");
                            }
                            
                            if (imageTransform != null)
                            {
                                AddEventUIPhotoNpc(imageTransform, "EventUI_AdrianPC", $"Image_{i}", allNpcs, saveManager);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("[Gallery] AdrianPC_Wor GameObject not found");
                    }
                }
                else
                {
                    Debug.Log("[Gallery] AdrianPC_Wor folder not found");
                }

                // Process OtherPC_Browser NPC: 15_EventUI > BPC_New > OtherPC_Browser > OtherPC_Core > GameObject (4)
                Transform otherPCBrowser = bpcNew.Find("OtherPC_Browser");
                if (otherPCBrowser != null)
                {
                    Debug.Log("[Gallery] Found OtherPC_Browser");
                    
                    Transform otherPCCore = otherPCBrowser.Find("OtherPC_Core");
                    if (otherPCCore != null)
                    {
                        Debug.Log("[Gallery] Found OtherPC_Core");
                        
                        Transform gameObject4 = otherPCCore.Find("GameObject (4)");
                        if (gameObject4 != null)
                        {
                            AddEventUIPhotoNpc(gameObject4, "EventUI_OtherPC", "GameObject_4", allNpcs, saveManager);
                        }
                        else
                        {
                            Debug.Log("[Gallery] OtherPC_Core GameObject (4) not found");
                        }
                    }
                    else
                    {
                        Debug.Log("[Gallery] OtherPC_Core not found");
                    }
                }
                else
                {
                    Debug.Log("[Gallery] OtherPC_Browser not found");
                }
            }
            else
            {
                Debug.Log("[Gallery] BPC_New not found");
            }
        }

        private static void AddEventUIPhotoNpc(Transform npcTransform, string category, string npcName, Dictionary<string, NpcGalleryData> allNpcs, SaveManager saveManager)
        {
            UnityEngine.UI.Image imageComponent = npcTransform.GetComponent<UnityEngine.UI.Image>();
            if (imageComponent == null)
            {
                Debug.LogWarning($"[Gallery] EventUI NPC '{npcName}' has no Image component");
                return;
            }

            if (imageComponent.sprite == null)
            {
                Debug.LogWarning($"[Gallery] EventUI NPC '{npcName}' Image has no sprite");
                return;
            }

            // Create hierarchical ID for EventUI NPC
            string hierarchicalNpcId = $"EventUI_{category}/{npcName}";

            if (!allNpcs.ContainsKey(hierarchicalNpcId))
            {
                NpcGalleryData data = new NpcGalleryData
                {
                    npcGO = npcTransform.gameObject,
                    mainSprite = imageComponent.sprite
                };

                allNpcs[hierarchicalNpcId] = data;
                Debug.Log($"[Gallery] Added EventUI NPC: {hierarchicalNpcId}");

                // Register with SaveManager for seen tracking
                if (saveManager != null)
                {
                    saveManager.RegisterEventUIPhotoNpc(hierarchicalNpcId, npcTransform.gameObject);
                }
            }
        }

        private static void CollectGhostMinigameNpcs(SaveManager saveManager, Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find Ghost_Minigame using Resources.FindObjectsOfTypeAll (handles disabled GameObjects)
            GameObject ghostMinigameGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "Ghost_Minigame" && go.scene.name != null);
            
            if (ghostMinigameGO == null)
            {
                Debug.Log($"[Gallery] Could not find Ghost_Minigame GO (may be inactive)");
                return;
            }

            Debug.Log($"[Gallery] Found Ghost_Minigame GO");
            
            Transform ghostMapCore = ghostMinigameGO.transform.Find("Ghost_Map_Core");
            if (ghostMapCore == null)
            {
                Debug.Log($"[Gallery] Could not find Ghost_Map_Core");
                return;
            }

            // Collect Asgo_Astrid_Looking_0 variants from AstridLooking folder
            Transform astridLooking = ghostMapCore.Find("AstridLooking");
            if (astridLooking != null)
            {
                Debug.Log($"[Gallery] Found AstridLooking with {astridLooking.childCount} children");
                for (int i = 0; i < astridLooking.childCount; i++)
                {
                    Transform astridChild = astridLooking.GetChild(i);
                    if (astridChild.name.StartsWith("Asgo_Astrid_Looking_0"))
                    {
                        SpriteRenderer sr = astridChild.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null)
                        {
                            string hierarchicalId = $"Ghost_Minigame_AstridLooking_{astridChild.name}";
                            if (!allNpcs.ContainsKey(hierarchicalId))
                            {
                                allNpcs[hierarchicalId] = new NpcGalleryData
                                {
                                    npcGO = astridChild.gameObject,
                                    mainSprite = sr.sprite,
                                    secondarySprites = new List<Sprite>()
                                };
                                Debug.Log($"[Gallery] Added Ghost NPC: {hierarchicalId}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[Gallery] Ghost NPC '{astridChild.name}' has no SpriteRenderer or sprite");
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"[Gallery] Could not find AstridLooking folder");
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
                        if (!allNpcs.ContainsKey(hierarchicalId))
                        {
                            allNpcs[hierarchicalId] = new NpcGalleryData
                            {
                                npcGO = noireMinigame1.gameObject,
                                mainSprite = sr.sprite,
                                secondarySprites = new List<Sprite>()
                            };
                            Debug.Log($"[Gallery] Added Ghost NPC: {hierarchicalId}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[Gallery] Noire_Minigame_1 has no SpriteRenderer or sprite");
                    }
                }
                else
                {
                    Debug.Log($"[Gallery] Could not find Noire_Minigame_1");
                }
            }
            else
            {
                Debug.Log($"[Gallery] Could not find ghost_Monsters folder");
            }

            // Collect Noire_Jumpscare from Ghost_Map_Core
            Transform noireJumpscare = ghostMapCore.Find("Noire_Jumpscare");
            if (noireJumpscare != null)
            {
                SpriteRenderer sr = noireJumpscare.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    string hierarchicalId = "Ghost_Minigame_Noire_Jumpscare";
                    if (!allNpcs.ContainsKey(hierarchicalId))
                    {
                        allNpcs[hierarchicalId] = new NpcGalleryData
                        {
                            npcGO = noireJumpscare.gameObject,
                            mainSprite = sr.sprite,
                            secondarySprites = new List<Sprite>()
                        };
                        Debug.Log($"[Gallery] Added Ghost NPC: {hierarchicalId}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[Gallery] Noire_Jumpscare has no SpriteRenderer or sprite");
                }
            }
            else
            {
                Debug.Log($"[Gallery] Could not find Noire_Jumpscare");
            }
        }

        private static void CollectCamcorderMinigameNpcs(SaveManager saveManager, Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Find Camcorder_Minigame using Resources.FindObjectsOfTypeAll (handles disabled GameObjects)
            GameObject camcorderMinigameGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "Camcorder_Minigame" && go.scene.name != null);
            
            if (camcorderMinigameGO == null)
            {
                Debug.Log($"[Gallery] Could not find Camcorder_Minigame GO (may be inactive)");
                return;
            }

            Debug.Log($"[Gallery] Found Camcorder_Minigame GO");

            // Collect from Camcorder_Bigfoot
            Transform camcorderBigfoot = camcorderMinigameGO.transform.Find("Camcorder_Bigfoot");
            if (camcorderBigfoot != null)
            {
                Debug.Log($"[Gallery] Found Camcorder_Bigfoot");
                
                // Collect Circle variants from Camcorder_Targets
                Transform camcorderTargets = camcorderBigfoot.Find("Camcorder_Targets");
                if (camcorderTargets != null)
                {
                    Debug.Log($"[Gallery] Found Camcorder_Bigfoot/Camcorder_Targets with {camcorderTargets.childCount} children");
                    for (int i = 0; i < camcorderTargets.childCount; i++)
                    {
                        Transform circleChild = camcorderTargets.GetChild(i);
                        if (circleChild.name.StartsWith("Circle"))
                        {
                            SpriteRenderer sr = circleChild.GetComponent<SpriteRenderer>();
                            if (sr != null && sr.sprite != null)
                            {
                                string hierarchicalId = $"Camcorder_Minigame_Bigfoot_{circleChild.name}";
                                if (!allNpcs.ContainsKey(hierarchicalId))
                                {
                                    allNpcs[hierarchicalId] = new NpcGalleryData
                                    {
                                        npcGO = circleChild.gameObject,
                                        mainSprite = sr.sprite,
                                        secondarySprites = new List<Sprite>()
                                    };
                                    Debug.Log($"[Gallery] Added Camcorder NPC: {hierarchicalId}");
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"[Gallery] Camcorder NPC '{circleChild.name}' has no SpriteRenderer or sprite");
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log($"[Gallery] Could not find Camcorder_Bigfoot/Camcorder_Targets");
                }

                // Collect Camcorder_Sprite_Emma1
                Transform camcorderSpriteEmma1 = camcorderBigfoot.Find("Camcorder_Sprite_Emma1");
                if (camcorderSpriteEmma1 != null)
                {
                    SpriteRenderer sr = camcorderSpriteEmma1.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.sprite != null)
                    {
                        string hierarchicalId = "Camcorder_Minigame_Bigfoot_Camcorder_Sprite_Emma1";
                        if (!allNpcs.ContainsKey(hierarchicalId))
                        {
                            allNpcs[hierarchicalId] = new NpcGalleryData
                            {
                                npcGO = camcorderSpriteEmma1.gameObject,
                                mainSprite = sr.sprite,
                                secondarySprites = new List<Sprite>()
                            };
                            Debug.Log($"[Gallery] Added Camcorder NPC: {hierarchicalId}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[Gallery] Camcorder_Sprite_Emma1 (Bigfoot) has no SpriteRenderer or sprite");
                    }
                }
                else
                {
                    Debug.Log($"[Gallery] Could not find Camcorder_Sprite_Emma1 in Bigfoot");
                }
            }
            else
            {
                Debug.Log($"[Gallery] Could not find Camcorder_Bigfoot");
            }

            // Collect from Camcorder_Lizard
            Transform camcorderLizard = camcorderMinigameGO.transform.Find("Camcorder_Lizard");
            if (camcorderLizard != null)
            {
                Debug.Log($"[Gallery] Found Camcorder_Lizard");
                
                // Collect Circle variants from Camcorder_Targets
                Transform camcorderTargets = camcorderLizard.Find("Camcorder_Targets");
                if (camcorderTargets != null)
                {
                    Debug.Log($"[Gallery] Found Camcorder_Lizard/Camcorder_Targets with {camcorderTargets.childCount} children");
                    for (int i = 0; i < camcorderTargets.childCount; i++)
                    {
                        Transform circleChild = camcorderTargets.GetChild(i);
                        if (circleChild.name.StartsWith("Circle"))
                        {
                            SpriteRenderer sr = circleChild.GetComponent<SpriteRenderer>();
                            if (sr != null && sr.sprite != null)
                            {
                                string hierarchicalId = $"Camcorder_Minigame_Lizard_{circleChild.name}";
                                if (!allNpcs.ContainsKey(hierarchicalId))
                                {
                                    allNpcs[hierarchicalId] = new NpcGalleryData
                                    {
                                        npcGO = circleChild.gameObject,
                                        mainSprite = sr.sprite,
                                        secondarySprites = new List<Sprite>()
                                    };
                                    Debug.Log($"[Gallery] Added Camcorder NPC: {hierarchicalId}");
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"[Gallery] Camcorder NPC '{circleChild.name}' has no SpriteRenderer or sprite");
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log($"[Gallery] Could not find Camcorder_Lizard/Camcorder_Targets");
                }

                // Collect Camcorder_Sprite_Emma1
                Transform camcorderSpriteEmma1 = camcorderLizard.Find("Camcorder_Sprite_Emma1");
                if (camcorderSpriteEmma1 != null)
                {
                    SpriteRenderer sr = camcorderSpriteEmma1.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.sprite != null)
                    {
                        string hierarchicalId = "Camcorder_Minigame_Lizard_Camcorder_Sprite_Emma1";
                        if (!allNpcs.ContainsKey(hierarchicalId))
                        {
                            allNpcs[hierarchicalId] = new NpcGalleryData
                            {
                                npcGO = camcorderSpriteEmma1.gameObject,
                                mainSprite = sr.sprite,
                                secondarySprites = new List<Sprite>()
                            };
                            Debug.Log($"[Gallery] Added Camcorder NPC: {hierarchicalId}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[Gallery] Camcorder_Sprite_Emma1 (Lizard) has no SpriteRenderer or sprite");
                    }
                }
                else
                {
                    Debug.Log($"[Gallery] Could not find Camcorder_Sprite_Emma1 in Lizard");
                }
            }
            else
            {
                Debug.Log($"[Gallery] Could not find Camcorder_Lizard");
            }
        }

        private static void PopulateGalleryWithScenes(GalleryDisplayMode displayMode = GalleryDisplayMode.ScenesOnly)
        {
            if (gallery == null)
            {
                Debug.LogError("[Gallery] Cannot populate gallery - gallery is null");
                return;
            }

            Transform scenesTransform = gallery.transform.Find("Scenes");
            Transform galleryCore = scenesTransform.Find("Core");
            if (galleryCore == null)
            {
                Debug.LogError("[Gallery] Cannot find Scenes/Core in gallery");
                return;
            }

            // Track unsorted items for later persistence to XML
            List<string> unsortedSceneNames = new List<string>();
            List<string> unsortedBustNames = new List<string>();
            List<string> unsortedNpcNames = new List<string>();

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

            SaveManager saveManager = FindFirstObjectByType<SaveManager>();
            
            // Collect all scenes first
            Dictionary<string, (GameObject sceneGO, Sprite sprite)> allScenes = new Dictionary<string, (GameObject, Sprite)>();

            foreach (string managerName in cgManagerNames)
            {
                GameObject managerGO = GameObject.Find(managerName);
                if (managerGO == null)
                {
                    Debug.LogWarning($"[Gallery] CG Manager not found: {managerName}");
                    continue;
                }

                Transform manager = managerGO.transform;

                for (int i = 0; i < manager.childCount; i++)
                {
                    Transform sceneTransform = manager.GetChild(i);
                    GameObject sceneGO = sceneTransform.gameObject;
                    string sceneId = sceneGO.name;

                    // Get the sprite from the SpriteRenderer in the scene hierarchy
                    Sprite sceneSprite = null;
                    Transform coreTransform = sceneTransform.Find("Core");
                    if (coreTransform != null)
                    {
                        Transform artTransform = coreTransform.Find("Art");
                        if (artTransform != null)
                        {
                            SpriteRenderer spriteRenderer = artTransform.GetComponent<SpriteRenderer>();
                            if (spriteRenderer != null)
                            {
                                sceneSprite = spriteRenderer.sprite;
                            }
                        }
                    }

                    if (sceneSprite != null)
                    {
                        allScenes[sceneId] = (sceneGO, sceneSprite);
                    }
                }
            }

            // Collect Compareit minigame scenes
            CollectCompareitScenes(allScenes, saveManager);

            // Collect Special Scenes (EventUI scenes that display like Compareit scenes in Scenes page)
            CollectSpecialScenes(allScenes, saveManager);

            int sceneCount = 0;
            int itemsInCurrentRow = 0;
            const int ITEMS_PER_ROW = 6;
            
            // Get the font from the gallery title for group titles
            TextMeshProUGUI galleryTitleText = gallery.transform.GetChild(4).gameObject.GetComponent<TextMeshProUGUI>();
            TMP_FontAsset groupTitleFont = galleryTitleText != null ? galleryTitleText.font : null;

            // Check if we need to create template XML (if it doesn't exist)
            string xmlPath = Path.Combine(exePath, configPath, "GalleryGroups.xml");
            if (!File.Exists(xmlPath) && allScenes.Count > 0)
            {
                // Discover bust names for the template
                List<string> discoveredBustNames = new List<string>();
                
                string[] bustManagerNames = new string[]
                {
                    "2_Bust_Manager",
                    "SpecialBusts"
                };
                
                foreach (string bustManagerName in bustManagerNames)
                {
                    GameObject bustMgrGO = GameObject.Find(bustManagerName);
                    if (bustMgrGO != null)
                    {
                        Transform bustManager = bustMgrGO.transform;
                        for (int i = 0; i < bustManager.childCount; i++)
                        {
                            Transform bustTransform = bustManager.GetChild(i);
                            SpriteRenderer spriteRenderer = null;
                            
                            if (bustManagerName == "SpecialBusts")
                            {
                                // For SpecialBusts, the sprite is directly on the bust GO
                                spriteRenderer = bustTransform.GetComponent<SpriteRenderer>();
                            }
                            else
                            {
                                // For 2_Bust_Manager, the sprite is on the first child
                                if (bustTransform.childCount > 0)
                                {
                                    Transform firstChild = bustTransform.GetChild(0);
                                    spriteRenderer = firstChild.GetComponent<SpriteRenderer>();
                                }
                            }
                            
                            if (spriteRenderer != null && spriteRenderer.sprite != null)
                            {
                                discoveredBustNames.Add(bustTransform.gameObject.name);
                            }
                        }
                    }
                }

                // Discover NPC names for the template
                List<string> discoveredNpcNames = new List<string>();
                GameObject npcLevelsGO = GameObject.Find("5_Levels");
                if (npcLevelsGO != null)
                {
                    for (int i = 0; i < npcLevelsGO.transform.childCount; i++)
                    {
                        Transform levelsParentTransform = npcLevelsGO.transform.GetChild(i);
                        Transform npcsFolder = levelsParentTransform.Find("NPCs");
                        if (npcsFolder != null)
                        {
                            CollectNpcNamesRecursive(npcsFolder, levelsParentTransform, discoveredNpcNames);
                        }
                    }
                }

                CreateTemplateGalleryGroupsXML(xmlPath, allScenes, discoveredBustNames, discoveredNpcNames);
                LoadGalleryGroupsFromXML();
            }

            // Add empty row before first group
            AddEmptySpaceForMode(galleryCore, displayMode);

            // Add scenes only if in ScenesOnly mode
            if (displayMode == GalleryDisplayMode.ScenesOnly)
            {
                foreach (GalleryGroup group in galleryGroups)
            {
                // Add scenes in this group
                int scenesInGroup = 0;
                GameObject firstSceneInGroup = null;
                foreach (string sceneName in group.sceneNames)
                {
                    if (allScenes.TryGetValue(sceneName, out var sceneData))
                    {
                        GameObject sceneItemGO = AddSceneToGallery(galleryCore, sceneName, sceneData.sceneGO, sceneData.sprite, saveManager);
                        if (firstSceneInGroup == null)
                        {
                            firstSceneInGroup = sceneItemGO;
                        }
                        sceneCount++;
                        scenesInGroup++;
                        itemsInCurrentRow++;
                    }
                }

                // Add title to the first scene item in the group ONLY if at least one scene was added
                if (firstSceneInGroup != null && scenesInGroup > 0)
                {
                    GameObject groupTitleGO = new GameObject($"GroupTitle_{group.name}");
                    groupTitleGO.transform.SetParent(firstSceneInGroup.transform, false);
                    
                    TextMeshProUGUI groupTitle = groupTitleGO.AddComponent<TextMeshProUGUI>();
                    groupTitle.text = group.name;
                    groupTitle.fontSize = 36;
                    groupTitle.alignment = TextAlignmentOptions.BottomLeft;
                    if (groupTitleFont != null)
                    {
                        groupTitle.font = groupTitleFont;
                    }
                    
                    // Position title above the item
                    RectTransform groupTitleRect = groupTitleGO.GetComponent<RectTransform>();
                    groupTitleRect.sizeDelta = new Vector2(240, 60);
                    groupTitleRect.anchoredPosition = new Vector3(8, 125, 0); // Position higher above
                    
                    CanvasGroup canvasGroup = groupTitleGO.AddComponent<CanvasGroup>();
                    canvasGroup.blocksRaycasts = false;
                    canvasGroup.interactable = false;
                }

                // Fill the rest of the row with empty spacers only if scenes were added
                if (scenesInGroup > 0)
                {
                    int remainingSpaces = (ITEMS_PER_ROW - (itemsInCurrentRow % ITEMS_PER_ROW)) % ITEMS_PER_ROW;
                    for (int i = 0; i < remainingSpaces; i++)
                    {
                        AddEmptySpacerToGallery(galleryCore);
                        itemsInCurrentRow++;
                    }
                    itemsInCurrentRow = 0;
                    
                    // Add spacing row between groups (using scaled height like the first row)
                    for (int i = 0; i < ITEMS_PER_ROW; i++)
                    {
                        GameObject spacingGO = new GameObject("GroupSpacing");
                        spacingGO.transform.SetParent(galleryCore, false);
                        
                        RectTransform spacingRect = spacingGO.AddComponent<RectTransform>();
                        spacingRect.sizeDelta = new Vector2(1000, 1000);
                        
                        LayoutElement spacingLayout = spacingGO.AddComponent<LayoutElement>();
                        spacingLayout.preferredWidth = 1000;
                        spacingLayout.preferredHeight = 50; // 1/4 height for spacing, matching first row scale
                    }
                }
            }

            // Then add unsorted scenes
            HashSet<string> sortedScenes = new HashSet<string>();
            foreach (GalleryGroup group in galleryGroups)
            {
                foreach (string sceneName in group.sceneNames)
                {
                    sortedScenes.Add(sceneName);
                }
            }

            Debug.Log($"[Gallery] Detecting unsorted scenes: allScenes.Count={allScenes.Count}, sortedScenes.Count={sortedScenes.Count}");

            // Check for any scenes not in the groups (unsorted)
            int unsortedCount = 0;
            GameObject firstUnsortedScene = null;
            foreach (var sceneEntry in allScenes)
            {
                if (!sortedScenes.Contains(sceneEntry.Key))
                {
                    Debug.Log($"[Gallery] Found unsorted scene: {sceneEntry.Key}");
                    GameObject sceneItemGO = AddSceneToGallery(galleryCore, sceneEntry.Key, sceneEntry.Value.sceneGO, sceneEntry.Value.sprite, saveManager);
                    unsortedSceneNames.Add(sceneEntry.Key);
                    if (firstUnsortedScene == null)
                    {
                        firstUnsortedScene = sceneItemGO;
                    }
                    sceneCount++;
                    unsortedCount++;
                    itemsInCurrentRow++;
                }
            }
            
            Debug.Log($"[Gallery] Total unsorted scenes detected: {unsortedCount}");
            
            if (unsortedCount > 0)
            {

                // Add "Non-Sorted" title ONLY if at least one unsorted scene was added
                if (firstUnsortedScene != null)
                {
                    GameObject unsortedTitleGO = new GameObject("GroupTitle_Non-Sorted");
                    unsortedTitleGO.transform.SetParent(firstUnsortedScene.transform, false);
                    
                    TextMeshProUGUI unsortedTitle = unsortedTitleGO.AddComponent<TextMeshProUGUI>();
                    unsortedTitle.text = "Unsorted";
                    unsortedTitle.fontSize = 36;
                    unsortedTitle.alignment = TextAlignmentOptions.BottomLeft;
                    if (groupTitleFont != null)
                    {
                        unsortedTitle.font = groupTitleFont;
                    }
                    
                    RectTransform unsortedTitleRect = unsortedTitleGO.GetComponent<RectTransform>();
                    unsortedTitleRect.sizeDelta = new Vector2(240, 60);
                    unsortedTitleRect.anchoredPosition = new Vector3(8, 125, 0);
                    
                    CanvasGroup unsortedCanvasGroup = unsortedTitleGO.AddComponent<CanvasGroup>();
                    unsortedCanvasGroup.blocksRaycasts = false;
                    unsortedCanvasGroup.interactable = false;

                    // Fill remaining spaces in the unsorted section to complete the row
                    int remainingSpaces = (ITEMS_PER_ROW - (itemsInCurrentRow % ITEMS_PER_ROW)) % ITEMS_PER_ROW;
                    for (int i = 0; i < remainingSpaces; i++)
                    {
                        AddEmptySpacerToGallery(galleryCore);
                    }
                    itemsInCurrentRow = 0;

                    // Add spacing row after Unsorted section (before Busts)
                    for (int i = 0; i < ITEMS_PER_ROW; i++)
                    {
                        GameObject spacingGO = new GameObject("GroupSpacing");
                        spacingGO.transform.SetParent(galleryCore, false);
                        
                        RectTransform spacingRect = spacingGO.AddComponent<RectTransform>();
                        spacingRect.sizeDelta = new Vector2(1000, 1000);
                        
                        LayoutElement spacingLayout = spacingGO.AddComponent<LayoutElement>();
                        spacingLayout.preferredWidth = 1000;
                        spacingLayout.preferredHeight = 50;
                    }
                }
            }
            }

            // Add busts and NPCs only if in BustsAndNpcs mode
            if (displayMode == GalleryDisplayMode.BustsAndNpcs)
            {

            // Add busts from groups in XML (or create groups dynamically)
            string[] bustManagerNames = new string[]
            {
                "2_Bust_Manager",
                "SpecialBusts"
            };
            
            Dictionary<string, (GameObject bustGO, Sprite sprite)> allBusts = new Dictionary<string, (GameObject, Sprite)>();
            
            foreach (string bustManagerName in bustManagerNames)
            {
                GameObject bustManagerGO = GameObject.Find(bustManagerName);
                if (bustManagerGO != null)
                {
                    Transform bustManager = bustManagerGO.transform;
                    
                    // Collect all busts available from this manager
                    for (int i = 0; i < bustManager.childCount; i++)
                    {
                        Transform bustTransform = bustManager.GetChild(i);
                        GameObject bustGO = bustTransform.gameObject;
                        string bustId = bustGO.name;

                        // Get the sprite based on manager type
                        Sprite bustSprite = null;
                        
                        if (bustManagerName == "SpecialBusts")
                        {
                            // For SpecialBusts, the sprite is directly on the bust GO
                            SpriteRenderer spriteRenderer = bustTransform.GetComponent<SpriteRenderer>();
                            if (spriteRenderer != null)
                            {
                                bustSprite = spriteRenderer.sprite;
                            }
                        }
                        else
                        {
                            // For 2_Bust_Manager, the sprite is on the first child
                            if (bustTransform.childCount > 0)
                            {
                                Transform firstChild = bustTransform.GetChild(0);
                                SpriteRenderer spriteRenderer = firstChild.GetComponent<SpriteRenderer>();
                                if (spriteRenderer != null)
                                {
                                    bustSprite = spriteRenderer.sprite;
                                }
                            }
                        }

                        if (bustSprite != null)
                        {
                            allBusts[bustId] = (bustGO, bustSprite);
                        }
                    }
                }
            }
            
            // Collect all NPCs once for reuse in groups
            Dictionary<string, NpcGalleryData> allNpcs = new Dictionary<string, NpcGalleryData>();
            SaveManager npcSaveManager = FindFirstObjectByType<SaveManager>();
            
            GameObject levelsGO = GameObject.Find("5_Levels");
            if (levelsGO != null)
            {
                for (int i = 0; i < levelsGO.transform.childCount; i++)
                {
                    Transform levelsParentTransform = levelsGO.transform.GetChild(i);
                    
                    Transform npcsFolder = levelsParentTransform.Find("NPCs");
                    if (npcsFolder != null)
                    {
                        CollectNpcsRecursive(npcsFolder, levelsParentTransform, allNpcs);
                    }
                    
                    AddSpecialCaseNpcs(levelsParentTransform, allNpcs, npcSaveManager);
                }
            }

            // Add Bafa NPCs to gallery
            // NOTE: Bafa NPCs are now registered early in SaveManager.InitializeNpcTracking
            AddBafaNpcsToGallery(allNpcs);

            // Add Starmaker Creator NPCs to gallery
            // NOTE: Starmaker Creator NPCs (Ms Umbra, Dreamy Wisp, Rose) use Image components in 9_MainCanvas
            AddStarmakerCreatorNpcsToGallery(allNpcs);

            // Add X_Movies NPCs to gallery
            // NOTE: X_Movies NPCs are now registered early in SaveManager.InitializeNpcTracking
            AddXMoviesNpcsToGallery(allNpcs);

            // Add Tabletop NPCs to gallery
            // NOTE: Tabletop NPCs are now registered early in SaveManager.InitializeNpcTracking
            // and their location IS IN THE CODE. We already have an implementation for Tabletop NPCs.
            // Skip the bust folder (Tabletop_Busts) - that's completely ignored and we ONLY use
            // the fighters from Tabletop Ally Fighters and Tabletop Enemy Fighters
            AddTabletopNpcsToGallery(allNpcs);

            // Collect Racing NPCs separately (not in 5_Levels)
            // Use Resources.FindObjectsOfTypeAll to find disabled GameObjects
            GameObject racingMinigameGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "Racing_Minigame" && go.scene.name != null);
            
            if (racingMinigameGO != null)
            {
                Debug.Log($"[Gallery] Found Racing_Minigame GO");
                Transform racingPC = racingMinigameGO.transform.Find("Racing_PC");
                if (racingPC != null)
                {
                    Debug.Log($"[Gallery] Found Racing_PC");
                    Transform racingBody = racingPC.Find("RacingBody");
                    if (racingBody != null)
                    {
                        Debug.Log($"[Gallery] Found RacingBody");
                        
                        // Add Racing_PCAlone > PCRacing1
                        Sprite baseSprite = null;
                        Transform pcAlone = racingBody.Find("Racing_BikeTypes/Racing_PCAlone");
                        if (pcAlone != null)
                        {
                            Transform pcRacing1 = pcAlone.Find("PCRacing1");
                            if (pcRacing1 != null)
                            {
                                Debug.Log($"[Gallery] Processing Racing NPC: PCRacing1");
                                SpriteRenderer baseSR = pcRacing1.GetComponent<SpriteRenderer>();
                                baseSprite = baseSR?.sprite;
                                AddRacingNpc(pcRacing1, "Racing_Minigame", "Racing Ally", allNpcs, npcSaveManager, null);
                            }
                        }
                        
                        // Add Racing_Guest NPCs
                        Transform guestGroup = racingBody.Find("Racing_Guest");
                        if (guestGroup != null)
                        {
                            Debug.Log($"[Gallery] Found Racing_Guest with {guestGroup.childCount} children");
                            foreach (Transform child in guestGroup)
                            {
                                Debug.Log($"[Gallery] Processing Racing NPC: {child.gameObject.name}");
                                AddRacingNpc(child, "Racing_Minigame", "Racing Guest", allNpcs, npcSaveManager, baseSprite);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log($"[Gallery] Could not find RacingBody");
                    }
                }
                else
                {
                    Debug.Log($"[Gallery] Could not find Racing_PC");
                }
            }
            else
            {
                Debug.Log($"[Gallery] Could not find Racing_Minigame GO (may be inactive)");
            }

            // Collect Barista NPCs separately (not in 5_Levels)
            // Use Resources.FindObjectsOfTypeAll to find disabled GameObjects
            GameObject baristaGameGO = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), 
                go => go.name == "Barista_Game" && go.scene.name != null);
            
            if (baristaGameGO != null)
            {
                Debug.Log($"[Gallery] Found Barista_Game GO");
                Transform canvas = baristaGameGO.transform.Find("Canvas");
                if (canvas != null)
                {
                    Debug.Log($"[Gallery] Found Barista Canvas");
                    Transform continueGO = canvas.Find("Continue");
                    if (continueGO != null)
                    {
                        Debug.Log($"[Gallery] Found Barista Continue");
                        Transform buyMenu = continueGO.Find("BuyMenu");
                        if (buyMenu != null)
                        {
                            Debug.Log($"[Gallery] Found Barista BuyMenu with {buyMenu.childCount} children");
                            foreach (Transform child in buyMenu)
                            {
                                // Skip inactive or template objects
                                if (child.gameObject.name != "0_empty" && child.gameObject.activeSelf)
                                {
                                    Debug.Log($"[Gallery] Processing Barista NPC: {child.gameObject.name}");
                                    AddBaristaNpc(child, "Barista_Game", allNpcs, npcSaveManager);
                                }
                            }
                        }
                        else
                        {
                            Debug.Log($"[Gallery] Could not find Barista BuyMenu");
                        }
                    }
                    else
                    {
                        Debug.Log($"[Gallery] Could not find Barista Continue");
                    }
                }
                else
                {
                    Debug.Log($"[Gallery] Could not find Barista Canvas");
                }
            }
            else
            {
                Debug.Log($"[Gallery] Could not find Barista_Game GO (may be inactive)");
            }

            // Collect FreeGob2 NPC separately (special path in Barista_Game)
            if (baristaGameGO != null)
            {
                Debug.Log($"[Gallery] Looking for FreeGob2 in Barista_Game");
                Transform freePerk = baristaGameGO.transform.Find("Canvas/FreePerk");
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
                                    Debug.Log($"[Gallery] Processing FreeGob2 NPC");
                                    AddBaristaSpecialNpc(content, "Barista_Game", allNpcs, npcSaveManager);
                                }
                                else
                                {
                                    Debug.Log($"[Gallery] Could not find FreeGob2 Content");
                                }
                            }
                            else
                            {
                                Debug.Log($"[Gallery] Could not find FreeGob2 Card");
                            }
                        }
                        else
                        {
                            Debug.Log($"[Gallery] Could not find FreeGob2 (4)");
                        }
                    }
                    else
                    {
                        Debug.Log($"[Gallery] Could not find CardsLeft");
                    }
                }
                else
                {
                    Debug.Log($"[Gallery] Could not find FreePerk");
                }
            }

            // Add Livestream NPCs to gallery
            // NOTE: Livestream NPCs are now registered early in SaveManager.InitializeNpcTracking
            // We only add them to the gallery here with matching IDs
            AddLiveStreamNpcsToGallery(allNpcs);

            // Collect Sword NPCs separately (not in 5_Levels)
            CollectSwordNpcs(npcSaveManager, allNpcs);

            // Collect Stage NPCs (AnnaOnStage variants)
            CollectStageNpcs(npcSaveManager, allNpcs);

            // Collect Convention NPCs (Convention_NPC_1 and Convenation_Ad_1 variants)
            CollectConventionNpcs(npcSaveManager, allNpcs);

            // NOTE: Milking NPCs are now collected early in SaveManager.InitializeNpcTracking()
            // and added to gallery in AddMilkingNpcsToGallery()
            // CollectMilkingNpcs(npcSaveManager, allNpcs);
            AddMilkingNpcsToGallery(allNpcs);

            // Collect Sprite_Overlay NPCs separately (not in 5_Levels)
            CollectSpriteOverlayNpcs(npcSaveManager, allNpcs);

            // Add EventUI Photo NPCs to gallery
            // NOTE: EventUI Photo NPCs are now registered early in SaveManager.InitializeNpcTracking
            // We only add them to the gallery here with matching IDs
            AddEventUIPhotosNpcsToGallery(allNpcs);

            // Collect Ghost_Minigame NPCs
            CollectGhostMinigameNpcs(npcSaveManager, allNpcs);

            // Collect Camcorder_Minigame NPCs
            CollectCamcorderMinigameNpcs(npcSaveManager, allNpcs);

            // Declare sorted tracking outside group loop
            HashSet<string> sortedBusts = new HashSet<string>();
            HashSet<string> sortedNpcs = new HashSet<string>();
            
            if (allBusts.Count > 0)
            {

                // Add busts and NPCs from groups together
                foreach (GalleryGroup group in galleryGroups)
                {
                    int itemsInGroup = 0;
                    GameObject firstItemInGroup = null;
                    // Add busts in this group
                    foreach (string bustName in group.bustNames)
                    {
                        if (allBusts.TryGetValue(bustName, out var bustData))
                        {
                            GameObject bustItemGO = AddBustToGallery(galleryCore, bustName, bustData.bustGO, bustData.sprite, saveManager);
                            if (firstItemInGroup == null)
                            {
                                firstItemInGroup = bustItemGO;
                            }
                            sceneCount++;
                            itemsInGroup++;
                            itemsInCurrentRow++;
                            sortedBusts.Add(bustName);
                        }
                    }

                    // Add NPCs in this group (same group as busts)
                    foreach (string npcName in group.npcNames)
                    {
                        if (allNpcs.TryGetValue(npcName, out var npcData) && !npcBlacklist.ContainsKey(npcName))
                        {
                            GameObject npcItemGO = AddNpcToGallery(galleryCore, npcName, npcData.npcGO, npcData.mainSprite, npcData.secondarySprites, saveManager);
                            if (firstItemInGroup == null)
                            {
                                firstItemInGroup = npcItemGO;
                            }
                            sceneCount++;
                            itemsInGroup++;
                            itemsInCurrentRow++;
                            sortedNpcs.Add(npcName);
                        }
                    }

                    // Add group title if any items were added
                    if (firstItemInGroup != null && itemsInGroup > 0)
                    {
                        GameObject groupTitleGO = new GameObject("GroupTitle_" + group.name);
                        groupTitleGO.transform.SetParent(firstItemInGroup.transform, false);
                        
                        TextMeshProUGUI groupTitle = groupTitleGO.AddComponent<TextMeshProUGUI>();
                        groupTitle.text = group.name;
                        groupTitle.fontSize = 36;
                        groupTitle.alignment = TextAlignmentOptions.BottomLeft;
                        if (groupTitleFont != null)
                        {
                            groupTitle.font = groupTitleFont;
                        }
                        
                        RectTransform groupTitleRect = groupTitleGO.GetComponent<RectTransform>();
                        groupTitleRect.sizeDelta = new Vector2(240, 60);
                        groupTitleRect.anchoredPosition = new Vector3(8, 125, 0);
                        
                        CanvasGroup canvasGroup = groupTitleGO.AddComponent<CanvasGroup>();
                        canvasGroup.blocksRaycasts = false;
                        canvasGroup.interactable = false;
                    }

                    // Fill remaining spaces in group and add spacing row
                    if (itemsInGroup > 0)
                    {
                        int remainingSpaces = (ITEMS_PER_ROW - (itemsInCurrentRow % ITEMS_PER_ROW)) % ITEMS_PER_ROW;
                        for (int i = 0; i < remainingSpaces; i++)
                        {
                            AddEmptySpacerToGallery(galleryCore);
                            itemsInCurrentRow++;
                        }
                        itemsInCurrentRow = 0;

                        // Add spacing row between groups
                        for (int i = 0; i < ITEMS_PER_ROW; i++)
                        {
                            GameObject spacingGO = new GameObject("GroupSpacing");
                            spacingGO.transform.SetParent(galleryCore, false);
                            
                            RectTransform spacingRect = spacingGO.AddComponent<RectTransform>();
                            spacingRect.sizeDelta = new Vector2(1000, 1000);
                            
                            LayoutElement spacingLayout = spacingGO.AddComponent<LayoutElement>();
                            spacingLayout.preferredWidth = 1000;
                            spacingLayout.preferredHeight = 50;
                        }
                    }
                }

                // Add unsorted busts
                Debug.Log($"[Gallery] Detecting unsorted busts: allBusts.Count={allBusts.Count}, sortedBusts.Count={sortedBusts.Count}");
                
                int unsortedBustCount = 0;
                GameObject firstUnsortedBust = null;
                foreach (var bustEntry in allBusts)
                {
                    if (!sortedBusts.Contains(bustEntry.Key))
                    {
                        Debug.Log($"[Gallery] Found unsorted bust: {bustEntry.Key}");
                        GameObject bustItemGO = AddBustToGallery(galleryCore, bustEntry.Key, bustEntry.Value.bustGO, bustEntry.Value.sprite, saveManager);
                        unsortedBustNames.Add(bustEntry.Key);
                        if (firstUnsortedBust == null)
                        {
                            firstUnsortedBust = bustItemGO;
                        }
                        sceneCount++;
                        unsortedBustCount++;
                        itemsInCurrentRow++;
                    }
                }
                
                Debug.Log($"[Gallery] Total unsorted busts detected: {unsortedBustCount}");
                
                if (unsortedBustCount > 0)
                {

                    // Add "Unsorted" title for busts ONLY if at least one unsorted bust was added
                    if (firstUnsortedBust != null)
                    {
                        GameObject unsortedBustTitleGO = new GameObject("GroupTitle_Unsorted_Busts");
                        unsortedBustTitleGO.transform.SetParent(firstUnsortedBust.transform, false);
                        
                        TextMeshProUGUI unsortedBustTitle = unsortedBustTitleGO.AddComponent<TextMeshProUGUI>();
                        unsortedBustTitle.text = "Unsorted Busts";
                        unsortedBustTitle.fontSize = 36;
                        unsortedBustTitle.alignment = TextAlignmentOptions.BottomLeft;
                        if (groupTitleFont != null)
                        {
                            unsortedBustTitle.font = groupTitleFont;
                        }
                        
                        RectTransform unsortedBustTitleRect = unsortedBustTitleGO.GetComponent<RectTransform>();
                        unsortedBustTitleRect.sizeDelta = new Vector2(240, 60);
                        unsortedBustTitleRect.anchoredPosition = new Vector3(8, 125, 0);
                        
                        CanvasGroup unsortedBustCanvasGroup = unsortedBustTitleGO.AddComponent<CanvasGroup>();
                        unsortedBustCanvasGroup.blocksRaycasts = false;
                        unsortedBustCanvasGroup.interactable = false;

                        // Fill remaining spaces in the unsorted bust section to complete the row
                        int remainingSpaces = (ITEMS_PER_ROW - (itemsInCurrentRow % ITEMS_PER_ROW)) % ITEMS_PER_ROW;
                        for (int i = 0; i < remainingSpaces; i++)
                        {
                            AddEmptySpacerToGallery(galleryCore);
                        }
                        itemsInCurrentRow = 0;

                        // Add spacing row after Unsorted Busts (before NPCs)
                        for (int i = 0; i < ITEMS_PER_ROW; i++)
                        {
                            GameObject spacingGO = new GameObject("GroupSpacing");
                            spacingGO.transform.SetParent(galleryCore, false);
                            
                            RectTransform spacingRect = spacingGO.AddComponent<RectTransform>();
                            spacingRect.sizeDelta = new Vector2(1000, 1000);
                            
                            LayoutElement spacingLayout = spacingGO.AddComponent<LayoutElement>();
                            spacingLayout.preferredWidth = 1000;
                            spacingLayout.preferredHeight = 50;
                        }
                    }
                    }
                }

                // Add unsorted busts (any not in groups)
                if (sortedBusts.Count < allBusts.Count)
                {
                    int unsortedBustCount = 0;
                    GameObject firstUnsortedBust = null;
                    foreach (var bustEntry in allBusts)
                    {
                        if (!sortedBusts.Contains(bustEntry.Key))
                        {
                            GameObject bustItemGO = AddBustToGallery(galleryCore, bustEntry.Key, bustEntry.Value.bustGO, bustEntry.Value.sprite, saveManager);
                            unsortedBustNames.Add(bustEntry.Key);
                            if (firstUnsortedBust == null)
                            {
                                firstUnsortedBust = bustItemGO;
                            }
                            sceneCount++;
                            unsortedBustCount++;
                            itemsInCurrentRow++;
                        }
                    }

                    // Add "Unsorted" title for busts ONLY if at least one unsorted bust was added
                    if (firstUnsortedBust != null && unsortedBustCount > 0)
                    {
                        GameObject unsortedBustTitleGO = new GameObject("GroupTitle_Unsorted_Busts");
                        unsortedBustTitleGO.transform.SetParent(firstUnsortedBust.transform, false);
                        
                        TextMeshProUGUI unsortedBustTitle = unsortedBustTitleGO.AddComponent<TextMeshProUGUI>();
                        unsortedBustTitle.text = "Unsorted Busts";
                        unsortedBustTitle.fontSize = 36;
                        unsortedBustTitle.alignment = TextAlignmentOptions.BottomLeft;
                        if (groupTitleFont != null)
                        {
                            unsortedBustTitle.font = groupTitleFont;
                        }
                        
                        RectTransform unsortedBustTitleRect = unsortedBustTitleGO.GetComponent<RectTransform>();
                        unsortedBustTitleRect.sizeDelta = new Vector2(240, 60);
                        unsortedBustTitleRect.anchoredPosition = new Vector3(8, 125, 0);
                        
                        CanvasGroup unsortedBustCanvasGroup = unsortedBustTitleGO.AddComponent<CanvasGroup>();
                        unsortedBustCanvasGroup.blocksRaycasts = false;
                        unsortedBustCanvasGroup.interactable = false;

                        // Fill remaining spaces in the unsorted bust section to complete the row
                        int remainingSpaces = (ITEMS_PER_ROW - (itemsInCurrentRow % ITEMS_PER_ROW)) % ITEMS_PER_ROW;
                        for (int i = 0; i < remainingSpaces; i++)
                        {
                            AddEmptySpacerToGallery(galleryCore);
                        }
                        itemsInCurrentRow = 0;

                        // Add spacing row after Unsorted Busts (before NPCs)
                        for (int i = 0; i < ITEMS_PER_ROW; i++)
                        {
                            GameObject spacingGO = new GameObject("GroupSpacing");
                            spacingGO.transform.SetParent(galleryCore, false);
                            
                            RectTransform spacingRect = spacingGO.AddComponent<RectTransform>();
                            spacingRect.sizeDelta = new Vector2(1000, 1000);
                            
                            LayoutElement spacingLayout = spacingGO.AddComponent<LayoutElement>();
                            spacingLayout.preferredWidth = 1000;
                            spacingLayout.preferredHeight = 50;
                        }
                    }
                }

                // Add unsorted NPCs
                Debug.Log($"[Gallery] Detecting unsorted NPCs: allNpcs.Count={allNpcs.Count}, sortedNpcs.Count={sortedNpcs.Count}");
                if (sortedNpcs.Count < allNpcs.Count)
                {
                    int unsortedNpcCount = 0;
                    GameObject firstUnsortedNpc = null;
                    foreach (var npcEntry in allNpcs)
                    {
                        // Skip if already sorted or if blacklisted
                        if (!sortedNpcs.Contains(npcEntry.Key) && !npcBlacklist.ContainsKey(npcEntry.Key))
                        {
                            Debug.Log($"[Gallery] Found unsorted NPC: {npcEntry.Key}");
                            GameObject npcItemGO = AddNpcToGallery(galleryCore, npcEntry.Key, npcEntry.Value.npcGO, npcEntry.Value.mainSprite, npcEntry.Value.secondarySprites, saveManager);
                            unsortedNpcNames.Add(npcEntry.Key);
                            if (firstUnsortedNpc == null)
                            {
                                firstUnsortedNpc = npcItemGO;
                            }
                            sceneCount++;
                            unsortedNpcCount++;
                            itemsInCurrentRow++;
                        }
                    }
                    
                    Debug.Log($"[Gallery] Total unsorted NPCs detected: {unsortedNpcCount}");

                    // Add "Unsorted" title for NPCs ONLY if at least one unsorted NPC was added
                    if (firstUnsortedNpc != null)
                    {
                        GameObject unsortedNpcTitleGO = new GameObject("GroupTitle_Unsorted_NPCs");
                        unsortedNpcTitleGO.transform.SetParent(firstUnsortedNpc.transform, false);
                        
                        TextMeshProUGUI unsortedNpcTitle = unsortedNpcTitleGO.AddComponent<TextMeshProUGUI>();
                        unsortedNpcTitle.text = "Unsorted NPCs";
                        unsortedNpcTitle.fontSize = 36;
                        unsortedNpcTitle.alignment = TextAlignmentOptions.BottomLeft;
                        if (groupTitleFont != null)
                        {
                            unsortedNpcTitle.font = groupTitleFont;
                        }
                        
                        RectTransform unsortedNpcTitleRect = unsortedNpcTitleGO.GetComponent<RectTransform>();
                        unsortedNpcTitleRect.sizeDelta = new Vector2(240, 60);
                        unsortedNpcTitleRect.anchoredPosition = new Vector3(8, 125, 0);
                        
                        CanvasGroup unsortedNpcCanvasGroup = unsortedNpcTitleGO.AddComponent<CanvasGroup>();
                        unsortedNpcCanvasGroup.blocksRaycasts = false;
                        unsortedNpcCanvasGroup.interactable = false;

                        // Fill remaining spaces in the unsorted NPC section to complete the row
                        int remainingSpaces = (ITEMS_PER_ROW - (itemsInCurrentRow % ITEMS_PER_ROW)) % ITEMS_PER_ROW;
                        for (int i = 0; i < remainingSpaces; i++)
                        {
                            AddEmptySpacerToGallery(galleryCore);
                        }
                    }
                }
            }            // Save unsorted items to XML if any were discovered
            if (unsortedSceneNames.Count > 0 || unsortedBustNames.Count > 0 || unsortedNpcNames.Count > 0)
            {
                SaveUnsortedItemsToXml(unsortedSceneNames, unsortedBustNames, unsortedNpcNames);
            }

            Debug.Log($"[Gallery] Successfully populated gallery with scenes, busts, and NPCs");
            
            // Update scrollbar height to match content
            UpdateScrollbarHeight();
        }

        private static void UpdateScrollbarHeight()
        {
            if (gallery == null) return;

            Transform scenesTransform = gallery.transform.Find("Scenes");
            if (scenesTransform == null) return;

            // Get Image (2) which is the gallery background (GetChild(1))
            Transform galleryBG = gallery.transform.GetChild(1);
            if (galleryBG == null) return;

            RectTransform galleryBGRect = galleryBG.GetComponent<RectTransform>();
            if (galleryBGRect == null) return;

            // Get the background height and subtract 20
            float bgHeight = galleryBGRect.rect.height - 80f;
            float heightReduction = 20f;

            // Update the scrollbar (GetChild(1) of Scenes) to match background height - 20
            Transform scrollbarTransform = scenesTransform.GetChild(1);
            if (scrollbarTransform != null)
            {
                RectTransform scrollbarRect = scrollbarTransform.GetComponent<RectTransform>();
                if (scrollbarRect != null)
                {
                    Vector2 currentSize = scrollbarRect.sizeDelta;
                    scrollbarRect.sizeDelta = new Vector2(currentSize.x, bgHeight);

                    scrollbarRect.anchoredPosition = new Vector2(scrollbarRect.anchoredPosition.x, -80);
                    Debug.Log($"[Gallery] Updated scrollbar height to {bgHeight}px (background: {galleryBGRect.rect.height}px - 20)");
                }
            }
        }

        private static void AddEmptySpaceForMode(Transform galleryCore, GalleryDisplayMode displayMode)
        {
            // Add empty row before first group
            for (int i = 0; i < 6; i++)
            {
                AddEmptySpacerToGallery(galleryCore);
            }
        }

        private static void AddEmptySpacerToGallery(Transform galleryCore)
        {
            GameObject spacerGO = new GameObject("EmptySpacer");
            spacerGO.transform.SetParent(galleryCore, false);
            
            RectTransform spacerRect = spacerGO.AddComponent<RectTransform>();
            spacerRect.sizeDelta = new Vector2(1000, 1000);
            
            LayoutElement layoutElement = spacerGO.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 1000;
            layoutElement.preferredHeight = 1000;
        }

        private static void CollectNpcNamesRecursive(Transform parent, Transform levelsParent, List<string> npcNames)
        {
            // Check if current GO has a SpriteRenderer
            SpriteRenderer spriteRenderer = parent.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // Create hierarchical ID: LevelsParentName_DirectParentName_NpcName
                string directParentName = parent.parent != null ? parent.parent.gameObject.name : "Unknown";
                string levelsParentName = levelsParent.gameObject.name;
                string npcName = parent.gameObject.name;
                string hierarchicalNpcId = $"{levelsParentName}_{directParentName}_{npcName}";
                
                if (!npcNames.Contains(hierarchicalNpcId))
                {
                    npcNames.Add(hierarchicalNpcId);
                }
                
                // Do NOT recurse into children of this NPC
                return;
            }
            
            // Recurse to children only if this GO doesn't have a SpriteRenderer
            for (int i = 0; i < parent.childCount; i++)
            {
                CollectNpcNamesRecursive(parent.GetChild(i), levelsParent, npcNames);
            }
        }

        private static void CollectNpcsRecursive(Transform parent, Transform levelsParent, Dictionary<string, NpcGalleryData> allNpcs)
        {
            // Check if current GO has a SpriteRenderer (and hasn't been added yet)
            SpriteRenderer spriteRenderer = parent.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // Create hierarchical ID: LevelsParentName_DirectParentName_NpcName
                string directParentName = parent.parent != null ? parent.parent.gameObject.name : "Unknown";
                string levelsParentName = levelsParent.gameObject.name;
                string npcName = parent.gameObject.name;
                string hierarchicalNpcId = $"{levelsParentName}_{directParentName}_{npcName}";
                
                // Exclude Circle NPCs from 133_HarborDistrict Groups 1 and 3
                if (levelsParentName == "133_HarborDistrict")
                {
                    if ((directParentName == "Group_1" || directParentName == "Group_3") && npcName.StartsWith("Circle"))
                    {
                        // Skip Circle NPCs from these groups - do not add to gallery
                        return;
                    }
                }
                
                // Handle duplicates by adding a counter suffix if the ID already exists
                string finalNpcId = hierarchicalNpcId;
                int counter = 1;
                while (allNpcs.ContainsKey(finalNpcId))
                {
                    finalNpcId = $"{hierarchicalNpcId}#{counter}";
                    counter++;
                }
                
                // Collect all secondary sprites (Bafa_Boobs, Bafa_Boobs (1), Bafa_Dick, etc.)
                List<Sprite> secondarySprites = new List<Sprite>();
                
                string[] secondarySpriteNames = new string[] { "Bafa_Boobs", "Bafa_Boobs (1)", "Bafa_Dick" };
                foreach (string spriteName in secondarySpriteNames)
                {
                    Transform spriteTransform = parent.Find(spriteName);
                    if (spriteTransform != null)
                    {
                        SpriteRenderer secondaryRenderer = spriteTransform.GetComponent<SpriteRenderer>();
                        if (secondaryRenderer != null && secondaryRenderer.sprite != null)
                        {
                            secondarySprites.Add(secondaryRenderer.sprite);
                        }
                    }
                }
                
                NpcGalleryData data = new NpcGalleryData
                {
                    npcGO = parent.gameObject,
                    mainSprite = spriteRenderer.sprite
                };
                data.secondarySprites = secondarySprites;
                
                allNpcs[finalNpcId] = data;
                
                // Do NOT recurse into children of this NPC - we only want the NPC itself, not its children
                return;
            }
            
            // Recurse to children only if this GO doesn't have a SpriteRenderer
            for (int i = 0; i < parent.childCount; i++)
            {
                CollectNpcsRecursive(parent.GetChild(i), levelsParent, allNpcs);
            }
        }

        private static void AddSpecialCaseNpcs(Transform levelsParent, Dictionary<string, NpcGalleryData> allNpcs, SaveManager saveManager)
        {
            string levelName = levelsParent.gameObject.name;
            
            // Special case NPCs that don't follow the normal discovery pattern
            // NOTE: These NPCs are now registered early in SaveManager.InitializeNpcTracking()
            // This method only adds them to the gallery display
            // Format: new[] { "Path", "To", "NpcName" } - levelName is matched separately
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
            
            // Check if this level has special cases
            if (!specialCasesByLevel.ContainsKey(levelName))
            {
                return; // This level has no special cases
            }
            
            // Process special cases for this level
            foreach (string[] pathArray in specialCasesByLevel[levelName])
            {
                Transform currentTransform = levelsParent;
                
                // Navigate through the path
                for (int i = 0; i < pathArray.Length; i++)
                {
                    string pathSegment = pathArray[i];
                    currentTransform = currentTransform.Find(pathSegment);
                    
                    if (currentTransform == null)
                    {
                        Debug.LogWarning($"[Gallery] Special case NPC not found in {levelName}: {string.Join(" > ", pathArray)}");
                        break;
                    }
                }
                
                if (currentTransform != null)
                {
                    SpriteRenderer spriteRenderer = currentTransform.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        // Create hierarchical ID - must match SaveManager's ID format EXACTLY
                        string npcName = pathArray[pathArray.Length - 1];
                        string directParentName = currentTransform.parent != null ? currentTransform.parent.gameObject.name : "Unknown";
                        string hierarchicalNpcId = $"{levelName}_{directParentName}_{npcName}";
                        
                        if (!allNpcs.ContainsKey(hierarchicalNpcId))
                        {
                            NpcGalleryData data = new NpcGalleryData
                            {
                                npcGO = currentTransform.gameObject,
                                mainSprite = spriteRenderer.sprite,
                                secondarySprites = new List<Sprite>()
                            };
                            
                            allNpcs[hierarchicalNpcId] = data;
                            Debug.Log($"[Gallery] Added special case NPC: {hierarchicalNpcId}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[Gallery] Special case NPC found but has no SpriteRenderer in {levelName}: {string.Join(" > ", pathArray)}");
                    }
                }
            }
        }

        private static void AddTabletopNpc(Transform fighterTransform, string levelName, string category, Dictionary<string, NpcGalleryData> allNpcs, SaveManager saveManager)
        {
            // Tabletop NPCs have their sprite in a child named Char_sprite with Image component
            Transform charSpriteTransform = fighterTransform.Find("Char_Sprite");
            if (charSpriteTransform == null)
            {
                Debug.LogWarning($"[Gallery] Tabletop fighter '{fighterTransform.gameObject.name}' has no Char_Sprite child");
                return; // No character sprite found
            }

            UnityEngine.UI.Image imageComponent = charSpriteTransform.GetComponent<UnityEngine.UI.Image>();
            if (imageComponent == null)
            {
                Debug.LogWarning($"[Gallery] Tabletop fighter '{fighterTransform.gameObject.name}' Char_Sprite has no Image component");
                return;
            }

            if (imageComponent.sprite == null)
            {
                Debug.LogWarning($"[Gallery] Tabletop fighter '{fighterTransform.gameObject.name}' Image component has no sprite");
                return;
            }

            // Create hierarchical ID for Tabletop NPC
            string npcName = fighterTransform.gameObject.name;
            string hierarchicalNpcId = $"{levelName}_{category}_{npcName}";

            if (!allNpcs.ContainsKey(hierarchicalNpcId))
            {
                NpcGalleryData data = new NpcGalleryData
                {
                    npcGO = fighterTransform.gameObject,
                    mainSprite = imageComponent.sprite,
                    secondarySprites = new List<Sprite>()
                };

                allNpcs[hierarchicalNpcId] = data;
                Debug.Log($"[Gallery] Added Tabletop NPC: {hierarchicalNpcId}");

                // Register with SaveManager for seen tracking
                // For Tabletop NPCs, we need to check if the fighter and all its ancestors are enabled
                if (saveManager != null)
                {
                    saveManager.RegisterTabletopNpc(hierarchicalNpcId, fighterTransform.gameObject);
                }
            }
        }

        private static void AddRacingNpc(Transform racingTransform, string levelName, string category, Dictionary<string, NpcGalleryData> allNpcs, SaveManager saveManager, Sprite baseSprite = null)
        {
            // Racing NPCs have their sprite directly in a SpriteRenderer on the GO
            SpriteRenderer spriteRenderer = racingTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[Gallery] Racing NPC '{racingTransform.gameObject.name}' has no SpriteRenderer");
                return;
            }

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"[Gallery] Racing NPC '{racingTransform.gameObject.name}' SpriteRenderer has no sprite");
                return;
            }

            // Create hierarchical ID for Racing NPC
            string npcName = racingTransform.gameObject.name;
            string hierarchicalNpcId = $"{levelName}_{category}_{npcName}";

            if (!allNpcs.ContainsKey(hierarchicalNpcId))
            {
                List<Sprite> secondarySprites = new List<Sprite>();
                Sprite mainSprite = spriteRenderer.sprite;
                
                // If we have a baseSprite, it means this is a Racing Guest
                // The baseSprite should be the main display (background/bike)
                // and the guest sprite should be secondary (on top)
                if (baseSprite != null)
                {
                    // Swap: baseSprite becomes main, guest sprite becomes secondary
                    secondarySprites.Add(mainSprite);
                    mainSprite = baseSprite;
                    Debug.Log($"[Gallery] Added Racing NPC: {hierarchicalNpcId} (guest on base)");
                }
                else
                {
                    Debug.Log($"[Gallery] Added Racing NPC: {hierarchicalNpcId}");
                }

                NpcGalleryData data = new NpcGalleryData
                {
                    npcGO = racingTransform.gameObject,
                    mainSprite = mainSprite,
                    secondarySprites = secondarySprites
                };

                allNpcs[hierarchicalNpcId] = data;

                // Register with SaveManager for seen tracking
                // For Racing NPCs, we need to check if the NPC and all its ancestors are enabled
                if (saveManager != null)
                {
                    saveManager.RegisterRacingNpc(hierarchicalNpcId, racingTransform.gameObject);
                }
            }
        }

        private static void AddBaristaNpc(Transform baristaTransform, string levelName, Dictionary<string, NpcGalleryData> allNpcs, SaveManager saveManager)
        {
            // Barista NPCs have their sprite in a child named "Image (1)" with Image component
            Transform imageLabelTransform = baristaTransform.Find("Image (1)");
            if (imageLabelTransform == null)
            {
                Debug.LogWarning($"[Gallery] Barista NPC '{baristaTransform.gameObject.name}' has no 'Image (1)' child");
                return;
            }

            UnityEngine.UI.Image imageComponent = imageLabelTransform.GetComponent<UnityEngine.UI.Image>();
            if (imageComponent == null)
            {
                Debug.LogWarning($"[Gallery] Barista NPC '{baristaTransform.gameObject.name}' Image (1) has no Image component");
                return;
            }

            if (imageComponent.sprite == null)
            {
                Debug.LogWarning($"[Gallery] Barista NPC '{baristaTransform.gameObject.name}' Image component has no sprite");
                return;
            }

            // Create hierarchical ID for Barista NPC
            string npcName = baristaTransform.gameObject.name;
            string hierarchicalNpcId = $"{levelName}_Barista_{npcName}";

            if (!allNpcs.ContainsKey(hierarchicalNpcId))
            {
                NpcGalleryData data = new NpcGalleryData
                {
                    npcGO = baristaTransform.gameObject,
                    mainSprite = imageComponent.sprite,
                    secondarySprites = new List<Sprite>()
                };

                allNpcs[hierarchicalNpcId] = data;
                Debug.Log($"[Gallery] Added Barista NPC: {hierarchicalNpcId}");

                // Register with SaveManager for seen tracking
                // For Barista NPCs, we need to check if the NPC and all its ancestors are enabled
                if (saveManager != null)
                {
                    saveManager.RegisterBaristaaNpc(hierarchicalNpcId, baristaTransform.gameObject);
                }
            }
        }

        private static void AddBaristaSpecialNpc(Transform contentTransform, string levelName, Dictionary<string, NpcGalleryData> allNpcs, SaveManager saveManager)
        {
            // FreeGob2 has its sprite directly in the GO as an Image component
            UnityEngine.UI.Image imageComponent = contentTransform.GetComponent<UnityEngine.UI.Image>();
            if (imageComponent == null)
            {
                Debug.LogWarning($"[Gallery] Barista special NPC '{contentTransform.gameObject.name}' has no Image component");
                return;
            }

            if (imageComponent.sprite == null)
            {
                Debug.LogWarning($"[Gallery] Barista special NPC '{contentTransform.gameObject.name}' Image component has no sprite");
                return;
            }

            // Create hierarchical ID for Barista special NPC
            string npcName = "FreeGob2";
            string hierarchicalNpcId = $"{levelName}_Barista_{npcName}";

            if (!allNpcs.ContainsKey(hierarchicalNpcId))
            {
                NpcGalleryData data = new NpcGalleryData
                {
                    npcGO = contentTransform.gameObject,
                    mainSprite = imageComponent.sprite,
                    secondarySprites = new List<Sprite>()
                };

                allNpcs[hierarchicalNpcId] = data;
                Debug.Log($"[Gallery] Added Barista special NPC: {hierarchicalNpcId}");

                // Register with SaveManager for seen tracking
                // For Barista special NPCs, we need to check if Barista_Game is enabled
                if (saveManager != null)
                {
                    saveManager.RegisterBaristaSpecialNpc(hierarchicalNpcId, contentTransform.gameObject);
                }
            }
        }

        private static GameObject AddNpcToGallery(Transform galleryCore, string npcId, GameObject npcGO, Sprite npcSprite, List<Sprite> secondarySprites, SaveManager saveManager)
        {
            // Create a new GameObject in the gallery for this NPC
            GameObject galleryNpcDisplay = new GameObject(npcId);
            galleryNpcDisplay.transform.SetParent(galleryCore, false);

            // Add Layout Element to control size (on parent)
            RectTransform galleryRect = galleryNpcDisplay.AddComponent<RectTransform>();
            LayoutElement layoutElement = galleryNpcDisplay.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 1000;
            layoutElement.preferredHeight = 1000;
            galleryRect.sizeDelta = new Vector2(1000, 1000);

            // Check if NPC has been seen
            bool npcSeen = saveManager != null && saveManager.IsNpcSeen(npcId);

            // Add secondary sprites FIRST (they will render behind the main sprite due to child order)
            // This is important for Racing NPCs where baseSprite should be behind the character sprite
            if (secondarySprites != null && secondarySprites.Count > 0)
            {
                for (int i = 0; i < secondarySprites.Count; i++)
                {
                    Sprite secondarySprite = secondarySprites[i];
                    if (secondarySprite != null)
                    {
                        GameObject secondaryChild = new GameObject($"SecondarySprite_{i}");
                        secondaryChild.transform.SetParent(galleryNpcDisplay.transform, false);
                        
                        RectTransform secondaryRect = secondaryChild.AddComponent<RectTransform>();
                        UnityEngine.UI.Image secondaryImage = secondaryChild.AddComponent<UnityEngine.UI.Image>();
                        
                        // Match parent size
                        secondaryRect.anchorMin = Vector2.zero;
                        secondaryRect.anchorMax = Vector2.one;
                        secondaryRect.offsetMin = Vector2.zero;
                        secondaryRect.offsetMax = Vector2.zero;
                        
                        secondaryImage.sprite = secondarySprite;
                        secondaryImage.type = UnityEngine.UI.Image.Type.Simple;
                        secondaryImage.preserveAspect = true;
                        secondaryImage.color = new Color(1, 1, 1, 1);
                        
                        // Apply grayscale to secondary if not seen
                        if (!npcSeen)
                        {
                            ApplyGrayscaleToImage(secondaryImage);
                        }
                    }
                }
                Debug.Log($"[Gallery] Added {secondarySprites.Count} secondary sprite layer(s) for NPC: {npcId}");
            }

            // Add main sprite LAST so it renders on top of secondary sprites
            UnityEngine.UI.Image mainImage = galleryNpcDisplay.AddComponent<UnityEngine.UI.Image>();
            mainImage.sprite = npcSprite;
            mainImage.type = UnityEngine.UI.Image.Type.Simple;
            mainImage.preserveAspect = true;
            mainImage.color = new Color(1, 1, 1, 1);

            // Apply grayscale to main image if not seen
            if (!npcSeen)
            {
                ApplyGrayscaleToImage(mainImage);
            }

            // Add Button component for interactivity
            UnityEngine.UI.Button npcButton = galleryNpcDisplay.AddComponent<UnityEngine.UI.Button>();
            npcButton.targetGraphic = mainImage;
            
            // Check if this is an EventUI NPC (special UI elements, not scene NPCs)
            bool isEventUiNpc = npcId.StartsWith("EventUI_");
            
            // For EventUI NPCs, always make clickable (they're special UI elements)
            // For regular NPCs, only enable button if NPC has been seen
            npcButton.interactable = isEventUiNpc ? true : npcSeen;
            
            // Add click listener for fullscreen display - pass the gallery NPC item to include secondary sprites
            npcButton.onClick.AddListener(() => OnNpcClicked(galleryNpcDisplay, npcId));
            
            Debug.Log($"[Gallery] Added NPC to gallery: {npcId}, interactable: {(isEventUiNpc ? true : npcSeen)}, isEventUiNpc: {isEventUiNpc}");

            return galleryNpcDisplay;
        }

        private static GameObject AddSceneToGallery(Transform galleryCore, string sceneId, GameObject sceneGO, Sprite sceneSprite, SaveManager saveManager)
        {
            // Create a new GameObject in the gallery for this scene
            GameObject gallerySceneDisplay = new GameObject(sceneId);
            gallerySceneDisplay.transform.SetParent(galleryCore, false);

            // Add Image component for gallery display
            RectTransform galleryRect = gallerySceneDisplay.AddComponent<RectTransform>();
            UnityEngine.UI.Image galleryImage = gallerySceneDisplay.AddComponent<UnityEngine.UI.Image>();
            
            // Add Layout Element to control size
            LayoutElement layoutElement = gallerySceneDisplay.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 1000;
            layoutElement.preferredHeight = 1000;

            // Set the sprite from the original scene
            galleryImage.sprite = sceneSprite;
            galleryImage.type = UnityEngine.UI.Image.Type.Simple;
            galleryImage.preserveAspect = true;

            // Adjust size
            galleryRect.sizeDelta = new Vector2(1000, 1000);

            // Check if scene has been seen
            bool sceneSeen = saveManager != null && saveManager.IsSceneSeen(sceneId);
            
            if (sceneSeen)
            {
                // For unlocked scenes, use fully opaque white to ensure bright display
                galleryImage.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                // For locked scenes, use fully opaque white before applying grayscale
                galleryImage.color = new Color(1f, 1f, 1f, 1f);
                ApplyGrayscaleToImage(galleryImage);
            }

            // Add Button component for interactivity
            UnityEngine.UI.Button galleryButton = gallerySceneDisplay.AddComponent<UnityEngine.UI.Button>();
            galleryButton.targetGraphic = galleryImage;
            
            // Check if this is a special scene
            bool isSpecialScene = sceneId.StartsWith("15_EventUI_") || sceneId.StartsWith("Compareit_") || sceneId.StartsWith("9_MainCanvas_") || sceneId.StartsWith("5_Levels_");
            
            // For special scenes, always allow clicking (they have no seen state)
            // For regular scenes, check if they've been seen
            bool buttonInteractable = isSpecialScene ? true : sceneSeen;
            galleryButton.interactable = buttonInteractable;
            
            // Store the scene reference and add click listener
            if (isSpecialScene)
            {
                // Special scenes display fullscreen with their original size scaled to screen height
                galleryButton.onClick.AddListener(() => OnSpecialSceneClicked(sceneGO));
                //Debug.Log($"[Gallery] Added special scene to gallery: {sceneId}, interactable: {buttonInteractable}, sceneGO: {sceneGO.name}");
            }
            else
            {
                // Regular CG scenes use the standard fullscreen behavior
                galleryButton.onClick.AddListener(() => OnGalleryItemClicked(sceneGO));
                //Debug.Log($"[Gallery] Added regular scene to gallery: {sceneId}, interactable: {buttonInteractable}");
            }
            
            return gallerySceneDisplay;
        }

        private static GameObject AddBustToGallery(Transform galleryCore, string bustId, GameObject bustGO, Sprite bustSprite, SaveManager saveManager)
        {
            // Create a new GameObject in the gallery for this bust
            GameObject galleryBustDisplay = new GameObject(bustId);
            galleryBustDisplay.transform.SetParent(galleryCore, false);

            // Add Image component for gallery display
            RectTransform galleryRect = galleryBustDisplay.AddComponent<RectTransform>();
            UnityEngine.UI.Image galleryImage = galleryBustDisplay.AddComponent<UnityEngine.UI.Image>();
            
            // Add Layout Element to control size
            LayoutElement layoutElement = galleryBustDisplay.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 1000;
            layoutElement.preferredHeight = 1000;

            // Set the sprite from the original bust
            galleryImage.sprite = bustSprite;
            galleryImage.type = UnityEngine.UI.Image.Type.Simple;

            // Adjust size
            galleryRect.sizeDelta = new Vector2(1000, 1000);

            // Set default color to white (fully visible)
            galleryImage.color = new Color(1, 1, 1, 1);

            // Check if bust has been seen and apply black if not
            if (saveManager != null && !saveManager.IsBustSeen(bustId))
            {
                ApplyGrayscaleToImage(galleryImage);
            }

            // Add Button component for interactivity
            UnityEngine.UI.Button galleryButton = galleryBustDisplay.AddComponent<UnityEngine.UI.Button>();
            galleryButton.targetGraphic = galleryImage;
            
            // Disable button if bust hasn't been seen
            bool bustSeen = saveManager != null && saveManager.IsBustSeen(bustId);
            galleryButton.interactable = bustSeen;
            
            // Store the bust reference and add click listener
            galleryButton.onClick.AddListener(() => OnGalleryItemClicked(bustGO));

            Debug.Log($"[Gallery] Added bust to gallery: {bustId}");
            
            return galleryBustDisplay;
        }

        private static void OnSpecialSceneClicked(GameObject specialSceneGO)
        {
            if (specialSceneGO == null) 
            {
                Debug.LogError("[Gallery] OnSpecialSceneClicked called with NULL GameObject");
                return;
            }

            Debug.Log($"[Gallery] OnSpecialSceneClicked called with GameObject: {specialSceneGO.name}, active: {specialSceneGO.activeInHierarchy}");

            // Get the sprite from either Image component or SpriteRenderer
            Sprite specialSceneSprite = null;
            
            // First try Image component (for UI-based special scenes like Desktop Ads)
            UnityEngine.UI.Image imageComponent = specialSceneGO.GetComponent<UnityEngine.UI.Image>();
            if (imageComponent != null && imageComponent.sprite != null)
            {
                specialSceneSprite = imageComponent.sprite;
            }
            else
            {
                // Fallback to SpriteRenderer (for world-based special scenes like Cinema movie frames)
                SpriteRenderer spriteRenderer = specialSceneGO.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    specialSceneSprite = spriteRenderer.sprite;
                }
            }
            
            if (specialSceneSprite == null)
            {
                Debug.LogError($"[Gallery] Special scene GameObject '{specialSceneGO.name}' has NO Image or SpriteRenderer with a sprite");
                return;
            }
            Debug.Log($"[Gallery] Got sprite '{specialSceneSprite.name}' with dimensions {specialSceneSprite.rect.width}x{specialSceneSprite.rect.height}");

            // Create a new display GameObject on MainCanvas
            GameObject specialSceneDisplay = new GameObject("SpecialSceneDisplay");
            specialSceneDisplay.transform.SetParent(mainCanvas, false);

            // Add RectTransform - center it
            RectTransform rectTransform = specialSceneDisplay.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            // Add Image component with the sprite
            UnityEngine.UI.Image displayImage = specialSceneDisplay.AddComponent<UnityEngine.UI.Image>();
            displayImage.sprite = specialSceneSprite;
            displayImage.type = UnityEngine.UI.Image.Type.Simple;
            displayImage.preserveAspect = true;
            displayImage.color = new Color(1, 1, 1, 1); // Ensure fully opaque

            // Calculate dimensions based on sprite and screen size
            float spriteWidth = specialSceneSprite.rect.width;
            float spriteHeight = specialSceneSprite.rect.height;
            float screenHeight = Screen.height;
            float targetHeight = screenHeight * 0.7f; // 70% of screen height (with larger margins at top/bottom)
            
            // Calculate scale to fit target height while maintaining aspect ratio
            float scale = targetHeight / spriteHeight;
            float scaledWidth = spriteWidth * scale;
            float scaledHeight = spriteHeight * scale;
            
            // Apply the scaled dimensions to the RectTransform
            rectTransform.sizeDelta = new Vector2(scaledWidth, scaledHeight);
            
            Debug.Log($"[Gallery] Created special scene display: sprite {spriteWidth}x{spriteHeight}, scale {scale}, display {scaledWidth}x{scaledHeight}");

            // Store original scale and position
            originalSceneScale = specialSceneDisplay.transform.localScale;
            originalScenePosition = specialSceneDisplay.transform.localPosition;

            // Disable all root MainCanvas children except the fullscreen button
            disabledCanvasChildren.Clear();
            if (mainCanvas != null)
            {
                mainCanvasWasActive = mainCanvas.gameObject.activeSelf;
                foreach (Transform child in mainCanvas)
                {
                    if (child.gameObject.name != "FullscreenSceneButton" && child.gameObject != specialSceneDisplay && child.gameObject.activeSelf)
                    {
                        child.gameObject.SetActive(false);
                        disabledCanvasChildren.Add(child.gameObject);
                    }
                }
                Debug.Log("[Gallery] Disabled main canvas children for special scene");
            }

            // Check and disable PlayerRoom_ButtonCanvas if enabled
            Transform playerRoomTransform = level.Find("5_MyRoom").Find("PlayerRoom_ButtonCanvas");
            if (playerRoomTransform != null)
            {
                playerRoomButtonCanvasWasActive = playerRoomTransform.gameObject.activeSelf;
                if (playerRoomButtonCanvasWasActive)
                {
                    playerRoomTransform.gameObject.SetActive(false);
                    Debug.Log("[Gallery] Disabled PlayerRoom_ButtonCanvas");
                }
            }

            // Create fullscreen button to close the special scene
            CreateFullscreenSpecialSceneButton(specialSceneDisplay);
        }

        private static void OnNpcClicked(GameObject galleryNpcItem, string npcId)
        {
            if (galleryNpcItem == null)
            {
                Debug.LogError($"[Gallery] OnNpcClicked called with NULL gallery NPC item for NPC: {npcId}");
                return;
            }

            Debug.Log($"[Gallery] OnNpcClicked called for NPC: {npcId}");

            // Get the main sprite from the gallery NPC item
            UnityEngine.UI.Image mainImage = galleryNpcItem.GetComponent<UnityEngine.UI.Image>();
            if (mainImage == null || mainImage.sprite == null)
            {
                Debug.LogError($"[Gallery] Gallery NPC item '{npcId}' has NO main sprite");
                return;
            }

            Sprite npcSprite = mainImage.sprite;

            // Create a new display GameObject on MainCanvas
            GameObject npcDisplay = new GameObject("NpcDisplay");
            npcDisplay.transform.SetParent(mainCanvas, false);

            // Add RectTransform - center it
            RectTransform rectTransform = npcDisplay.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            // Calculate dimensions based on sprite and screen size
            float spriteWidth = npcSprite.rect.width;
            float spriteHeight = npcSprite.rect.height;
            float screenHeight = Screen.height;
            float targetHeight = screenHeight * 0.75f; // Full screen height for NPC display
            
            // Calculate scale to fit target height while maintaining aspect ratio
            float scale = targetHeight / spriteHeight;
            float scaledWidth = spriteWidth * scale;
            float scaledHeight = spriteHeight * scale;
            
            // Apply the scaled dimensions to the RectTransform
            rectTransform.sizeDelta = new Vector2(scaledWidth, scaledHeight);
            
            Debug.Log($"[Gallery] Created NPC display: sprite {spriteWidth}x{spriteHeight}, scale {scale}, display {scaledWidth}x{scaledHeight}");

            // Add secondary sprites as children FIRST (they will render behind the main sprite)
            // Get all secondary sprite children from the gallery NPC item
            int secondaryChildCount = galleryNpcItem.transform.childCount;
            for (int i = 0; i < secondaryChildCount; i++)
            {
                Transform secondaryGalleryChild = galleryNpcItem.transform.GetChild(i);
                UnityEngine.UI.Image secondaryGalleryImage = secondaryGalleryChild.GetComponent<UnityEngine.UI.Image>();
                
                if (secondaryGalleryImage != null && secondaryGalleryImage.sprite != null)
                {
                    // Create a matching secondary sprite in the fullscreen display
                    GameObject secondaryChild = new GameObject($"SecondarySprite_{i}");
                    secondaryChild.transform.SetParent(npcDisplay.transform, false);
                    
                    RectTransform secondaryRect = secondaryChild.AddComponent<RectTransform>();
                    UnityEngine.UI.Image secondaryImage = secondaryChild.AddComponent<UnityEngine.UI.Image>();
                    
                    // Match parent size and position
                    secondaryRect.anchorMin = Vector2.zero;
                    secondaryRect.anchorMax = Vector2.one;
                    secondaryRect.offsetMin = Vector2.zero;
                    secondaryRect.offsetMax = Vector2.zero;
                    
                    secondaryImage.sprite = secondaryGalleryImage.sprite;
                    secondaryImage.type = UnityEngine.UI.Image.Type.Simple;
                    secondaryImage.preserveAspect = true;
                    secondaryImage.color = secondaryGalleryImage.color; // Use the same color as gallery
                    
                    Debug.Log($"[Gallery] Added secondary sprite to fullscreen display: {secondaryGalleryImage.sprite.name}");
                }
            }

            // Add main sprite Image LAST so it renders on top of secondary sprites
            UnityEngine.UI.Image displayImage = npcDisplay.AddComponent<UnityEngine.UI.Image>();
            displayImage.sprite = npcSprite;
            displayImage.type = UnityEngine.UI.Image.Type.Simple;
            displayImage.preserveAspect = true;
            displayImage.color = new Color(1, 1, 1, 1); // Ensure fully opaque

            // Apply point filter mode to all NPC textures for crisp pixel art display
            ApplyPointFilterToNpcDisplay(npcDisplay, galleryNpcItem);

            // Store original scale and position
            originalSceneScale = npcDisplay.transform.localScale;
            originalScenePosition = npcDisplay.transform.localPosition;

            // Disable all root MainCanvas children except the fullscreen button
            disabledCanvasChildren.Clear();
            if (mainCanvas != null)
            {
                mainCanvasWasActive = mainCanvas.gameObject.activeSelf;
                foreach (Transform child in mainCanvas)
                {
                    if (child.gameObject.name != "FullscreenSceneButton" && child.gameObject != npcDisplay && child.gameObject.activeSelf)
                    {
                        child.gameObject.SetActive(false);
                        disabledCanvasChildren.Add(child.gameObject);
                    }
                }
                Debug.Log("[Gallery] Disabled main canvas children for NPC display");
            }

            // Check and disable PlayerRoom_ButtonCanvas if enabled
            Transform playerRoomTransform = level.Find("5_MyRoom").Find("PlayerRoom_ButtonCanvas");
            if (playerRoomTransform != null)
            {
                playerRoomButtonCanvasWasActive = playerRoomTransform.gameObject.activeSelf;
                if (playerRoomButtonCanvasWasActive)
                {
                    playerRoomTransform.gameObject.SetActive(false);
                    Debug.Log("[Gallery] Disabled PlayerRoom_ButtonCanvas");
                }
            }

            // Create fullscreen button to close the NPC display
            CreateFullscreenNpcButton(npcDisplay);
        }

        private static void OnGalleryItemClicked(GameObject sceneGO)
        {
            if (sceneGO == null) return;

            // Disable all scenes in all CG Managers
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
                if (managerGO != null)
                {
                    Transform manager = managerGO.transform;
                    for (int i = 0; i < manager.childCount; i++)
                    {
                        Transform scene = manager.GetChild(i);
                        scene.gameObject.SetActive(false);
                    }
                }
            }

            // Disable all busts in bust managers
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
                        bust.gameObject.SetActive(false);
                    }
                }
            }

            // Enable the clicked scene/bust
            // First, check if the scene has a Trigger component and disable it BEFORE activating
            disabledSceneTrigger = sceneGO.GetComponent<Trigger>();
            if (disabledSceneTrigger != null)
            {
                disabledSceneTrigger.enabled = false;
                Debug.Log($"[Gallery] Disabled Trigger component on scene: {sceneGO.name}");
            }

            // Now activate the scene with the trigger disabled
            sceneGO.SetActive(true);
            Debug.Log($"[Gallery] Enabled scene/bust: {sceneGO.name}");

            // Store original scale and position
            originalSceneScale = sceneGO.transform.localScale;
            originalScenePosition = sceneGO.transform.localPosition;
            
            // Check if this is a bust or a scene by checking its parent
            Transform parent = sceneGO.transform.parent;
            float scaleValue = 2.8f; // Default scale for scenes
            float bustYOffset = 0f; // Y offset for busts - adjust this value as needed
            bool isSpecialBust = parent != null && parent.gameObject.name == "SpecialBusts";
            
            if (parent != null && parent.gameObject.name == "2_Bust_Manager")
            {
                scaleValue = 4.7f; // Different scale for busts - adjust this value as needed
                bustYOffset = 1.0f; // Y offset for busts - adjust this value as needed
                
                // Apply Y offset for busts
                Vector3 newPosition = sceneGO.transform.localPosition;
                newPosition.y += bustYOffset;
                sceneGO.transform.localPosition = newPosition;
                
                sceneGO.transform.localScale = new Vector2(scaleValue, scaleValue);
            }
            else if (!isSpecialBust)
            {
                // Apply scaling for scenes, but not for SpecialBusts
                sceneGO.transform.localScale = new Vector2(scaleValue, scaleValue);
            }
            // SpecialBusts are not resized - they display at their original size

            // Check and disable PlayerRoom_ButtonCanvas if enabled
            Transform playerRoomTransform = level.Find("5_MyRoom").Find("PlayerRoom_ButtonCanvas");
            if (playerRoomTransform != null)
            {
                playerRoomButtonCanvasWasActive = playerRoomTransform.gameObject.activeSelf;
                if (playerRoomButtonCanvasWasActive)
                {
                    playerRoomTransform.gameObject.SetActive(false);
                    Debug.Log("[Gallery] Disabled PlayerRoom_ButtonCanvas");
                }
            }

            // Disable main canvas children except the fullscreen button (only active ones)
            disabledCanvasChildren.Clear();
            if (mainCanvas != null)
            {
                mainCanvasWasActive = mainCanvas.gameObject.activeSelf;
                foreach (Transform child in mainCanvas)
                {
                    if (child.gameObject.name != "FullscreenSceneButton" && child.gameObject.activeSelf)
                    {
                        child.gameObject.SetActive(false);
                        disabledCanvasChildren.Add(child.gameObject);
                    }
                }
                Debug.Log("[Gallery] Disabled main canvas children (except fullscreen button)");
            }

            // Create fullscreen button to disable the scene
            CreateFullscreenSceneButton(sceneGO);
        }

        private static void CreateFullscreenSceneButton(GameObject sceneGO)
        {
            // Destroy previous fullscreen button if it exists
            if (fullscreenSceneButton != null)
            {
                GameObject.Destroy(fullscreenSceneButton);
            }

            // Store and change Canvas render mode to allow scene to appear on top
            Canvas canvas = mainCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                originalCanvasRenderMode = canvas.renderMode.ToString();
                originalCanvasSortingOrder = canvas.sortingOrder;
                
                // Change to "Camera" render mode so sorting order applies
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera.main;
                canvas.sortingOrder = -1; // Put canvas behind the scene
            }

            // Store original sorting layer and order for each renderer, then set new values
            SpriteRenderer[] spriteRenderers = sceneGO.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in spriteRenderers)
            {
                // Store original values
                originalSpriteSettings[sr] = (sr.sortingLayerName, sr.sortingOrder);
                
                // Set new values - Art renders on top (1), everything else at 0
                if (sr.gameObject.name == "Art")
                {
                    sr.sortingOrder = 1; // Art in front
                }
                else
                {
                    sr.sortingOrder = 0; // Frame and Background behind
                }
            }

            // Create a new fullscreen button as child of mainCanvas
            fullscreenSceneButton = new GameObject("FullscreenSceneButton");
            fullscreenSceneButton.transform.SetParent(mainCanvas, false);

            // Setup RectTransform to cover entire screen
            RectTransform rectTransform = fullscreenSceneButton.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // Add Image component (transparent)
            UnityEngine.UI.Image image = fullscreenSceneButton.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0); // Transparent

            // Add Button component
            UnityEngine.UI.Button button = fullscreenSceneButton.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => OnFullscreenSceneButtonClicked(sceneGO));

            Debug.Log($"[Gallery] Created fullscreen button for scene: {sceneGO.name}");
        }

        private static void CreateFullscreenSpecialSceneButton(GameObject specialSceneGO)
        {
            // Destroy previous fullscreen button if it exists
            if (fullscreenSceneButton != null)
            {
                GameObject.Destroy(fullscreenSceneButton);
            }

            // Create a new fullscreen button as child of mainCanvas
            fullscreenSceneButton = new GameObject("FullscreenSceneButton");
            fullscreenSceneButton.transform.SetParent(mainCanvas, false);

            // Setup RectTransform to cover entire screen
            RectTransform rectTransform = fullscreenSceneButton.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // Add Image component (transparent)
            UnityEngine.UI.Image image = fullscreenSceneButton.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0); // Transparent

            // Add Button component
            UnityEngine.UI.Button button = fullscreenSceneButton.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => OnFullscreenSpecialSceneButtonClicked(specialSceneGO));

            Debug.Log($"[Gallery] Created fullscreen button for special scene: {specialSceneGO.name}");
        }

        private static void CreateFullscreenNpcButton(GameObject npcDisplayGO)
        {
            // Destroy previous fullscreen button if it exists
            if (fullscreenSceneButton != null)
            {
                GameObject.Destroy(fullscreenSceneButton);
            }

            // Create a new fullscreen button as child of mainCanvas
            fullscreenSceneButton = new GameObject("FullscreenSceneButton");
            fullscreenSceneButton.transform.SetParent(mainCanvas, false);

            // Setup RectTransform to cover entire screen
            RectTransform rectTransform = fullscreenSceneButton.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // Add Image component (transparent)
            UnityEngine.UI.Image image = fullscreenSceneButton.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0); // Transparent

            // Add Button component
            UnityEngine.UI.Button button = fullscreenSceneButton.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => OnFullscreenNpcButtonClicked(npcDisplayGO));

            Debug.Log($"[Gallery] Created fullscreen button for NPC display: {npcDisplayGO.name}");
        }

        private static void OnFullscreenSceneButtonClicked(GameObject sceneGO)
        {
            if (sceneGO != null)
            {
                // Restore original scale and position
                sceneGO.transform.localScale = originalSceneScale;
                sceneGO.transform.localPosition = originalScenePosition;

                // Restore original sorting layer and order for each renderer
                SpriteRenderer[] spriteRenderers = sceneGO.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer sr in spriteRenderers)
                {
                    if (originalSpriteSettings.TryGetValue(sr, out var settings))
                    {
                        sr.sortingLayerName = settings.layer;
                        sr.sortingOrder = settings.order;
                    }
                }

                // Deactivate the scene first
                sceneGO.SetActive(false);
                Debug.Log($"[Gallery] Disabled scene: {sceneGO.name}");

                // Re-enable the Trigger component AFTER deactivating the GO
                if (disabledSceneTrigger != null)
                {
                    disabledSceneTrigger.enabled = true;
                    Debug.Log($"[Gallery] Re-enabled Trigger component on scene: {sceneGO.name}");
                    disabledSceneTrigger = null;
                }
            }

            // Restore Canvas render mode
            Canvas canvas = mainCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = (RenderMode)System.Enum.Parse(typeof(RenderMode), originalCanvasRenderMode);
                canvas.sortingOrder = originalCanvasSortingOrder;
            }

            // Clear sprite settings
            originalSpriteSettings.Clear();

            // Re-enable PlayerRoom_ButtonCanvas if it was active before
            if (playerRoomButtonCanvasWasActive)
            {
                Transform playerRoomTransform = level.Find("5_MyRoom").Find("PlayerRoom_ButtonCanvas");
                if (playerRoomTransform != null)
                {
                    playerRoomTransform.gameObject.SetActive(true);
                    Debug.Log("[Gallery] Re-enabled PlayerRoom_ButtonCanvas");
                }
            }

            // Re-enable only the children that were disabled
            foreach (GameObject child in disabledCanvasChildren)
            {
                if (child != null)
                {
                    child.SetActive(true);
                }
            }
            disabledCanvasChildren.Clear();
            if (disabledCanvasChildren.Count > 0)
            {
                Debug.Log("[Gallery] Re-enabled previously disabled canvas children");
            }

            // Destroy the fullscreen button
            if (fullscreenSceneButton != null)
            {
                GameObject.Destroy(fullscreenSceneButton);
                fullscreenSceneButton = null;
            }
        }

        private static void OnFullscreenSpecialSceneButtonClicked(GameObject specialSceneGO)
        {
            if (specialSceneGO != null)
            {
                // Destroy the special scene display (created dynamically in OnSpecialSceneClicked)
                GameObject.Destroy(specialSceneGO);
                Debug.Log($"[Gallery] Destroyed special scene display: {specialSceneGO.name}");
            }

            // Re-enable PlayerRoom_ButtonCanvas if it was active before
            if (playerRoomButtonCanvasWasActive)
            {
                Transform playerRoomTransform = level.Find("5_MyRoom").Find("PlayerRoom_ButtonCanvas");
                if (playerRoomTransform != null)
                {
                    playerRoomTransform.gameObject.SetActive(true);
                    Debug.Log("[Gallery] Re-enabled PlayerRoom_ButtonCanvas");
                }
            }

            // Re-enable only the children that were disabled
            foreach (GameObject child in disabledCanvasChildren)
            {
                if (child != null)
                {
                    child.SetActive(true);
                }
            }
            disabledCanvasChildren.Clear();
            if (disabledCanvasChildren.Count > 0)
            {
                Debug.Log("[Gallery] Re-enabled previously disabled canvas children");
            }

            // Destroy the fullscreen button
            if (fullscreenSceneButton != null)
            {
                GameObject.Destroy(fullscreenSceneButton);
                fullscreenSceneButton = null;
            }
        }

        private static void OnFullscreenNpcButtonClicked(GameObject npcDisplayGO)
        {
            if (npcDisplayGO != null)
            {
                // Restore original texture filter modes before destroying the display
                RestoreTextureFilterModes();

                // Destroy the NPC display (created dynamically in OnNpcClicked)
                GameObject.Destroy(npcDisplayGO);
                Debug.Log($"[Gallery] Destroyed NPC display: {npcDisplayGO.name}");
            }

            // Re-enable PlayerRoom_ButtonCanvas if it was active before
            if (playerRoomButtonCanvasWasActive)
            {
                Transform playerRoomTransform = level.Find("5_MyRoom").Find("PlayerRoom_ButtonCanvas");
                if (playerRoomTransform != null)
                {
                    playerRoomTransform.gameObject.SetActive(true);
                    Debug.Log("[Gallery] Re-enabled PlayerRoom_ButtonCanvas");
                }
            }

            // Re-enable only the children that were disabled
            foreach (GameObject child in disabledCanvasChildren)
            {
                if (child != null)
                {
                    child.SetActive(true);
                }
            }
            disabledCanvasChildren.Clear();
            if (disabledCanvasChildren.Count > 0)
            {
                Debug.Log("[Gallery] Re-enabled previously disabled canvas children");
            }

            // Destroy the fullscreen button
            if (fullscreenSceneButton != null)
            {
                GameObject.Destroy(fullscreenSceneButton);
                fullscreenSceneButton = null;
            }
        }

        private static void ApplyGrayscaleToImage(UnityEngine.UI.Image image)
        {
            if (image == null) return;

            // Make unseen scenes black
            image.color = new Color(0, 0, 0, image.color.a);
        }

        /// <summary>
        /// Apply point filter mode to all textures in the NPC fullscreen display for crisp pixel art
        /// </summary>
        private static void ApplyPointFilterToNpcDisplay(GameObject npcDisplayGO, GameObject galleryNpcItem)
        {
            // Clear and repopulate the texture filter modes dictionary
            originalTextureFilterModes.Clear();

            // Get all Image components from the fullscreen display (main + secondary sprites)
            UnityEngine.UI.Image[] displayImages = npcDisplayGO.GetComponentsInChildren<UnityEngine.UI.Image>();
            foreach (UnityEngine.UI.Image image in displayImages)
            {
                if (image.sprite != null && image.sprite.texture != null)
                {
                    Texture2D texture = image.sprite.texture;
                    
                    // Store original filter mode
                    if (!originalTextureFilterModes.ContainsKey(texture))
                    {
                        originalTextureFilterModes[texture] = texture.filterMode;
                        Debug.Log($"[Gallery] Stored original filter mode for texture: {texture.name} = {texture.filterMode}");
                    }

                    // Apply point filter
                    texture.filterMode = FilterMode.Point;
                    Debug.Log($"[Gallery] Applied Point filter to NPC texture: {texture.name}");
                }
            }

            // Also check the gallery NPC item for any textures we need to track
            if (galleryNpcItem != null)
            {
                UnityEngine.UI.Image[] galleryImages = galleryNpcItem.GetComponentsInChildren<UnityEngine.UI.Image>();
                foreach (UnityEngine.UI.Image image in galleryImages)
                {
                    if (image.sprite != null && image.sprite.texture != null)
                    {
                        Texture2D texture = image.sprite.texture;
                        
                        // Store original filter mode if not already stored
                        if (!originalTextureFilterModes.ContainsKey(texture))
                        {
                            originalTextureFilterModes[texture] = texture.filterMode;
                            Debug.Log($"[Gallery] Stored original filter mode for gallery texture: {texture.name} = {texture.filterMode}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Restore original texture filter modes after NPC fullscreen display is closed
        /// </summary>
        private static void RestoreTextureFilterModes()
        {
            foreach (var entry in originalTextureFilterModes)
            {
                Texture2D texture = entry.Key;
                FilterMode originalMode = entry.Value;

                if (texture != null)
                {
                    texture.filterMode = originalMode;
                    Debug.Log($"[Gallery] Restored filter mode for texture: {texture.name} = {originalMode}");
                }
            }

            originalTextureFilterModes.Clear();
            Debug.Log("[Gallery] Restored all texture filter modes to original");
        }

        // Public config accessors
        public static bool GetMultiSaveMode()
        {
            return galleryConfig.MultiSaveMode;
        }

        public static void SetMultiSaveMode(bool value)
        {
            galleryConfig.MultiSaveMode = value;
            string fullConfigPath = Path.Combine(exePath, configPath);
            galleryConfig.SaveToFile(fullConfigPath);
            Debug.Log($"[Gallery] MultiSaveMode set to: {value}");
        }

        public static void ReloadConfig()
        {
            string fullConfigPath = Path.Combine(exePath, configPath);
            galleryConfig.LoadFromFile(fullConfigPath);
            Debug.Log("[Gallery] Config reloaded from file");
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace SMSGallery
{
    /// <summary>
    /// Base interface for all gallery item collectors.
    /// Collectors are responsible for discovering and collecting gallery items from the game world.
    /// </summary>
    public interface IGalleryItemCollector
    {
        void Collect();
    }

    /// <summary>
    /// Collects scene gallery items from CG managers.
    /// </summary>
    public class SceneCollector : IGalleryItemCollector
    {
        private Dictionary<string, (GameObject sceneGO, Sprite sprite)> allScenes;
        private SaveManager saveManager;

        public SceneCollector(Dictionary<string, (GameObject, Sprite)> scenes, SaveManager manager)
        {
            allScenes = scenes;
            saveManager = manager;
        }

        public void Collect()
        {
            CollectCGManagerScenes();
            CollectCompareitScenes();
        }

        private void CollectCGManagerScenes()
        {
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
        }

        private void CollectCompareitScenes()
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
    }
}

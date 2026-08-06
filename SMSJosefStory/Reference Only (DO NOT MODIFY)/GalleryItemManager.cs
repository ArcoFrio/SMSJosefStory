using System.Collections.Generic;
using UnityEngine;

namespace SMSGallery
{
    /// <summary>
    /// Central manager for all gallery item collection and organization.
    /// </summary>
    public class GalleryItemManager
    {
        private SaveManager saveManager;
        private SceneCollector sceneCollector;
        private BustCollector bustCollector;
        private NPCCollector npcCollector;

        public Dictionary<string, (GameObject sceneGO, Sprite sprite)> AllScenes { get; private set; }
        public Dictionary<string, (GameObject bustGO, Sprite sprite)> AllBusts { get; private set; }
        public Dictionary<string, NpcGalleryData> AllNpcs { get; private set; }

        public GalleryItemManager(SaveManager manager)
        {
            saveManager = manager;
            AllScenes = new Dictionary<string, (GameObject, Sprite)>();
            AllBusts = new Dictionary<string, (GameObject, Sprite)>();
            AllNpcs = new Dictionary<string, NpcGalleryData>();

            sceneCollector = new SceneCollector(AllScenes, manager);
            bustCollector = new BustCollector(manager);
            npcCollector = new NPCCollector(manager, AllNpcs);
        }

        /// <summary>
        /// Collects all gallery items from the game world.
        /// </summary>
        public void CollectAllItems()
        {
            Debug.Log("[Gallery] Starting item collection...");

            // Collect scenes
            sceneCollector.Collect();
            Debug.Log($"[Gallery] Collected {AllScenes.Count} scenes");

            // Collect busts
            AllBusts = bustCollector.CollectAll();
            Debug.Log($"[Gallery] Collected {AllBusts.Count} busts");

            // Collect NPCs
            npcCollector.CollectAll();
            Debug.Log($"[Gallery] Collected {AllNpcs.Count} NPCs");

            Debug.Log("[Gallery] Item collection complete");
        }

        /// <summary>
        /// Validates that all required items were collected.
        /// </summary>
        public bool ValidateCollection()
        {
            if (AllScenes.Count == 0)
            {
                Debug.LogWarning("[Gallery] No scenes were collected");
                return false;
            }

            if (AllBusts.Count == 0)
            {
                Debug.LogWarning("[Gallery] No busts were collected");
                return false;
            }

            if (AllNpcs.Count == 0)
            {
                Debug.LogWarning("[Gallery] No NPCs were collected");
                return false;
            }

            return true;
        }
    }
}

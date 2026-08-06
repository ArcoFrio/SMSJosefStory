using System.Collections.Generic;
using UnityEngine;

namespace SMSGallery
{
    /// <summary>
    /// Represents a single NPC gallery entry with sprite data.
    /// </summary>
    public class NpcGalleryData
    {
        public GameObject npcGO;
        public Sprite mainSprite;
        public List<Sprite> secondarySprites = new List<Sprite>();
    }

    /// <summary>
    /// Represents a grouping of gallery items organized by category.
    /// </summary>
    public class GalleryGroup
    {
        public string name;
        public List<string> sceneNames = new List<string>();
        public List<string> bustNames = new List<string>();
        public List<string> npcNames = new List<string>();
    }

    /// <summary>
    /// Configuration settings for the gallery system.
    /// </summary>
    public class GalleryConfig
    {
        public bool sortByName = true;
        public bool groupItems = true;
        public bool enableFiltering = true;
    }

    /// <summary>
    /// Display modes for the gallery.
    /// </summary>
    public enum GalleryDisplayMode
    {
        ScenesOnly,
        BustsAndNpcs
    }
}

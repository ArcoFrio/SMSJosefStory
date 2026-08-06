using System;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    [Serializable]
    public abstract class TRepository<T> : IRepository where T : class, IRepository, new()
    {
        public const string PATH_IN_RESOURCES = "Settings/";
        public const string DIRECTORY_ASSETS = "Assets/Plugins/GameCreator/Data/Resources/Settings/";
        protected static T Instance;
        public string AssetDirectory => "Assets/Plugins/GameCreator/Data/Resources/Settings/";
        public abstract string RepositoryID { get; }
        public static T Get
        {
            get
            {
                if (Instance != null) return Instance;
                T val = new T();
                AssetRepository<T> assetRepository = Resources.Load<AssetRepository<T>>(PathUtils.Combine("Settings/", val.RepositoryID));
                if (assetRepository != null) val = assetRepository.Get();
                Instance = val;
                return Instance;
            }
        }
    }
} 
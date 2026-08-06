using System;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    [Serializable]
    public abstract class AssetRepository<T> : TAssetRepository where T : class, IRepository, new()
    {
        [SerializeReference]
        private IRepository m_Repository = new T();
        public override string AssetPath => m_Repository.AssetDirectory;
        public override string RepositoryID => m_Repository.RepositoryID;
        public override int Priority => 10;
        public T Get() { return m_Repository as T; }
    }
} 
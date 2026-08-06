using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Variables
{
    public abstract class TGlobalVariables : ScriptableObject
    {
        [SerializeField]
        protected SaveUniqueID m_SaveUniqueID = new SaveUniqueID();
        public IdString UniqueID => m_SaveUniqueID.Get;
        public bool Save => m_SaveUniqueID.SaveValue;
    }
} 
using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Variables
{
    [Serializable]
    public abstract class TLocalVariables : MonoBehaviour, IGameSave
    {
        [SerializeField]
        protected SaveUniqueID m_SaveUniqueID = new SaveUniqueID();
        public string SaveID => m_SaveUniqueID.Get.String;
        public bool IsShared => false;
        public LoadMode LoadMode => LoadMode.Lazy;
        public abstract Type SaveType { get; }
        protected virtual void Awake() { SaveLoadManager.Subscribe(this); }
        protected virtual void OnDestroy() { SaveLoadManager.Unsubscribe(this); }
        public void ChangeId(IdString nextId)
        {
            if (m_SaveUniqueID.SaveValue)
            {
                Debug.LogError("Unable to change the Local Variable ID of a 'savable' component");
            }
            else
            {
                m_SaveUniqueID.Set = nextId;
            }
        }
        public abstract object GetSaveData(bool includeNonSavable);
        public abstract Task OnLoad(object value);
    }
} 
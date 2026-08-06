using System;
using System.Threading.Tasks;

namespace GameCreator.Runtime.Common
{
    public interface IGameSave
    {
        string SaveID { get; }
        bool IsShared { get; }
        Type SaveType { get; }
        LoadMode LoadMode { get; }
        object GetSaveData(bool includeNonSavable);
        Task OnLoad(object value);
    }
} 
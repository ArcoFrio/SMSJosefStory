using System;
using System.Collections;
using System.Collections.Generic;

namespace GameCreator.Runtime.Variables
{
    [Serializable]
    public abstract class TVariableRuntime<T> : IEnumerable<T>, IEnumerable where T : TVariable
    {
        public abstract void OnStartup();
        public abstract IEnumerator<T> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
} 
using System;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Variables
{
    [Serializable]
    public class NameList : TList<NameVariable>
    {
        public string[] Names
        {
            get
            {
                string[] array = new string[Length];
                for (int i = 0; i < Length; i++)
                {
                    array[i] = Get(i).Name;
                }
                return array;
            }
        }
        public IdString[] TypeIds
        {
            get
            {
                IdString[] array = new IdString[Length];
                for (int i = 0; i < Length; i++)
                {
                    array[i] = Get(i).TypeID;
                }
                return array;
            }
        }
        public NameList() { }
        public NameList(params NameVariable[] variables) : base(variables) { }
    }
} 
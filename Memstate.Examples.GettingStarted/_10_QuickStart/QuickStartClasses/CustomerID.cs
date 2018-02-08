using System;

namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses
{
    [Serializable]
    public struct CustomerID
    {
        public CustomerID(int iD)
        {
            ID = iD;
        }
        public int ID { get; set; }
        public override int GetHashCode() { return ID; }
        public override string ToString() => $"{ID,0000}";
    }
}
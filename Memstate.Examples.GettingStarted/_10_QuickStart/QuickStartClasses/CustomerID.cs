﻿using System;

namespace Memstate.Examples.GettingStarted._10_QuickStart.QuickStartClasses
{
    [Serializable]
    public class CustomerID
    {
        public CustomerID(int iD)
        {
            ID = iD;
        }
        public int ID { get; }
        public override int GetHashCode() { return ID;}
    }
}
using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class MushroomData : IComparable<MushroomData>
{
    public enum Type { None, Poisonous, Edible }

    public class TypeComparer : IComparer<MushroomData>
    {
        public int Compare(MushroomData x, MushroomData y)
        {
            if(x.type < y.type)
            {
                return -1;
            }
            else if(x.type == y.type)
            {
                return 0;
            }

            return 1;
        }
    }

    public int id;
    public GameObject prefab;
    public Type type;

    public int CompareTo(MushroomData other)
    {
        if(this.id > other.id)
        {
            return -1;
        }
        else if(this.id == other.id)
        {
            return 0;
        }

        return 1;
    }

    public override string ToString()
    {
        return "SENE_" + id.ToString("D2");
    }
}

#nullable enable
using UnityEngine;
using System;

namespace Assets.Prefabs.MapBuilder
{
    public class DoubledAreaAndBaseLength : IComparable<DoubledAreaAndBaseLength>
    {
        private readonly float doubleArea;
        private readonly float baseLen;

        public DoubledAreaAndBaseLength(float doubleArea, float baseLen)
        {
            this.doubleArea = doubleArea;
            this.baseLen = baseLen;
        }

        //Check if the height is bigger
        public int CompareTo(DoubledAreaAndBaseLength other)
        {
            if (other is null)
            {
                Debug.LogWarning("This shouldn't happen");
                return 1;
            }
            return (other.baseLen * doubleArea).CompareTo(baseLen * other.doubleArea);
        }

        public static readonly DoubledAreaAndBaseLength MAX = new(float.PositiveInfinity, -1);
    };

    public class GeometricUtils
    {
        public static bool ThereAreObtuseAnglesNearAB(Vector2 A, Vector2 B, Vector2 other)
        {
            return Vector2.Dot(A - B, other - B) < 0 || Vector2.Dot(B - A, other - A) < 0;
        }
    }
}

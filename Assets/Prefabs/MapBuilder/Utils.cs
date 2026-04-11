using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MapBuilder
{
    [Serializable]
    public class Point2D
    {
        [SerializeField]
        private float x;

        [SerializeField]
        private float y;

        public float X => x;
        public float Y => y;

        public Point2D(float x = 0f, float y = 0f)
        {
            this.x = x;
            this.y = y;
        }

        public float DistanceTo(Point2D other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public override bool Equals(object obj)
        {
            if (obj is not Point2D other)
            {
                return false;
            }

            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override int GetHashCode() => HashCode.Combine(x, y);

        public override string ToString() => $"({X}, {Y})";
    }

    [Serializable]
    public class Grid
    {
        [SerializeField]
        private Point2D bottomLeft = new();

        [SerializeField]
        private Point2D upperRight = new();

        public Point2D BottomLeft => bottomLeft;
        public Point2D UpperRight => upperRight;

        public Grid()
        {
        }

        public Grid(Point2D bottomLeft, Point2D upperRight)
        {
            this.bottomLeft = bottomLeft;
            this.upperRight = upperRight;
        }

        public override string ToString() => $"({BottomLeft}, {UpperRight})";
    }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> serializedKeys = new();

        [SerializeField]
        private List<TValue> serializedValues = new();

        public void OnBeforeSerialize()
        {
            ClearSerializationBuffers();

            foreach (var pair in this)
            {
                serializedKeys.Add(pair.Key);
                serializedValues.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();

            int count = Mathf.Min(serializedKeys.Count, serializedValues.Count);
            for (int i = 0; i < count; i++)
            {
                TKey key = serializedKeys[i];

                if (key is null || ContainsKey(key))
                {
                    continue;
                }

                Add(key, serializedValues[i]);
            }

            ClearSerializationBuffers();
        }

        private void ClearSerializationBuffers()
        {
            serializedKeys.Clear();
            serializedValues.Clear();
            serializedKeys.TrimExcess();
            serializedValues.TrimExcess();
        }
    }
}

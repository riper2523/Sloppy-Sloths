using UnityEngine;
using System.Linq;

namespace Assets.Prefabs.MapBuilder.MapBuilderManager
{
    readonly struct Transposition : IContainerStateTransformation
    {
        readonly Vector3 Vector;

        public Transposition(Vector3 vector)
        {
            Vector = vector;
        }

        public void TransformInPlace(NodeContainerState state)
        {
            for (int i = 0; i < state.Nodes.Count; i++)
            {
                state.Nodes[i] += Vector;
            }
        }
    }

    readonly struct Rotation : IContainerStateTransformation
    {
        readonly float RotationValue;

        public Rotation(float rotation)
        {
            RotationValue = rotation;
        }

        public void TransformInPlace(NodeContainerState state)
        {
            if (state.Nodes.Count == 0) return;
            var Centroid = state.Nodes.Aggregate(new Vector3(0, 0, 0), (sum, next) => sum + next, sum => sum / state.Nodes.Count);

            Quaternion rotation = Quaternion.Euler(0, 0, RotationValue);

            for (var i = 0; i < state.Nodes.Count; i++)
            {
                state.Nodes[i] = (rotation * (state.Nodes[i] - Centroid)) + Centroid;
            }
        }
    }

    readonly struct Scaling : IContainerStateTransformation
    {
        readonly float ScalingValue;

        public Scaling(float scaling)
        {
            ScalingValue = scaling;
        }

        public void TransformInPlace(NodeContainerState state)
        {
            if (state.Nodes.Count == 0) return;
            var Centroid = state.Nodes.Aggregate(new Vector3(0, 0, 0), (sum, next) => sum + next, sum => sum / state.Nodes.Count);
            for (var i = 0; i < state.Nodes.Count; i++)
            {
                state.Nodes[i] = (state.Nodes[i] - Centroid) * ScalingValue + Centroid;
            }
        }
    }
}

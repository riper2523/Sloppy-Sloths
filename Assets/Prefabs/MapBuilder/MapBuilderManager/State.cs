using UnityEngine;
using System.Threading.Tasks;
using System;

namespace Assets.Prefabs.MapBuilder.MapBuilderManager
{
    class Utils
    {
        public static void PrintUnsupportedCall(string stateName, string callType, string payloadDescription = "")
        {
            Debug.LogWarning($"State {stateName} doesn't support calls to {callType}. Payload description is {payloadDescription}");
        }
    }

    public enum StateID
    {
        BUILDER_MODE,
        GEAR_SELECT_MODE,
        TESTING_MODE,
        STAR_CONFIG_MODE
    }

    public interface IMapBuilderManagerState
    {
        #region State Change Callbacks

        void OnDeactivateState();
        void OnActivateState();

        #endregion

        #region Basic Input Handling

        void EscapeWasClicked() { }

        void VoidWasClicked(Vector3 where) { }

        #endregion

        #region State information

        StateID StateType { get; }

        #endregion
    }
}

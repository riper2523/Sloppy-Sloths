using System;
using System.Collections.Generic;
using Assets.Prefabs.LevelSystem.StarManager;
using Assets.Prefabs.MapBuilder.Inventory;

namespace Assets.Prefabs.MapBuilder.Serialization
{
    public interface IHasPositionAsDTO
    {
        float x { get; }
        float y { get; }
        float z { get; }
    }

    public interface IHasPayload
    {
        string GetPayload();
    }
    public enum NodeHandleType
    {
        CIRCULAR
    }
    public interface INodeHandleDTO
    {
        NodeHandleType Type { get; }
    }

    public enum NodeContainerType
    {
        POLYGON
    }
    public interface INodeContainerDTO : IHasPositionAsDTO
    {
        NodeContainerType Type { get; }
        List<INodeHandleDTO> NodeHandleDTOs { get; }
    }

    public enum NodeManagerType
    {
        ORDINARY
    }
    public interface INodeManagerDTO : IHasPayload
    {
        NodeManagerType Type { get; }
        List<INodeContainerDTO> NodeContainerDTOs { get; }
    }

    public interface IGridDTO : IHasPositionAsDTO
    {
        uint Width { get; }
        uint Height { get; }
    }

    public interface IVehicleBuilderDTO : IGridDTO
    {
        List<(VehiclePartID part, uint amount)> Parts { get; }
    }

    // Position is only used if 
    public interface IStarDataDTO
    {
        StarGoal StarGoal { get; }
        // Only if the StarGoal.goalType is CollectStar
        IHasPositionAsDTO Position { get; }
    }

    public interface IItemDataDTO
    {
        SupportedPartType itemType { get; }
        uint amount { get; }
    }

    public interface IFinishLineDTO : IGridDTO
    {
    }

    public interface IMapStateDTO : IHasPayload
    {
        INodeManagerDTO NodeManager { get; }
        IVehicleBuilderDTO VehicleBuilder { get; }
        List<IStarDataDTO> Stars { get; }
        IFinishLineDTO FinishLine { get; }
        Dictionary<SupportedPartType, uint> Items { get; }
    }
}

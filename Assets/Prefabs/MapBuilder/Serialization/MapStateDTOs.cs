using System;
using System.Collections.Generic;
using Assets.Prefabs.LevelSystem.StarManager;
using Newtonsoft.Json;
using Assets.Prefabs.MapBuilder.Inventory;

namespace Assets.Prefabs.MapBuilder.Serialization
{
    [System.Serializable]
    public class PositionDTO : IHasPositionAsDTO
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public PositionDTO() { }
        public PositionDTO(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }

    [System.Serializable]
    public class VehicleBuilderDTO : IVehicleBuilderDTO
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public List<(VehiclePartID part, uint amount)> Parts { get; set; } = new();
    }

    [System.Serializable]
    public class FinishLineDTO : IFinishLineDTO
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
    }

    [System.Serializable]
    public class StarDataDTO : IStarDataDTO
    {
        public StarGoal StarGoal { get; set; }
        public IHasPositionAsDTO Position { get; set; }
    }

    [System.Serializable]
    public class MapStateDTO : IMapStateDTO
    {
        public INodeManagerDTO NodeManager { get; set; }
        public IVehicleBuilderDTO VehicleBuilder { get; set; }
        public List<IStarDataDTO> Stars { get; set; } = new();
        public IFinishLineDTO FinishLine { get; set; }
        public Dictionary<SupportedPartType, uint> Items { get; set; } = new();

        public string GetPayload() => JsonConvert.SerializeObject(this);
    }
}

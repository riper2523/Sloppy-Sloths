#nullable enable
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Assets.Prefabs.MapBuilder.Serialization;

public enum SerializationError
{
    FILE_EXISTS,
    WRITING_TO_FILE_FAILED,
}

public static class SerializationErrorHelper
{
    public static string GetErrorMessage(SerializationError error)
    {
        return error switch
        {
            SerializationError.FILE_EXISTS => "The file already exists",
            SerializationError.WRITING_TO_FILE_FAILED => "Couldn't write to the file",
            _ => "Unknown error",
        };
    }
}

public enum DeserializationError
{
    FILE_NOT_FOUND,
    PARSING_FAILED
}

public static class DeserializationErrorHelper
{
    public static string GetErrorMessage(DeserializationError error)
    {
        return error switch
        {
            DeserializationError.FILE_NOT_FOUND => "The file was not found",
            DeserializationError.PARSING_FAILED => "Failed to parse the file",
            _ => "Unknown error",
        };
    }
}

#region Converters

public static class ConverterUtils
{
    /// <summary>
    /// Creates a serializer that doesn't include the specified converter type.
    /// This is essential to prevent infinite recursion in ReadJson/WriteJson.
    /// </summary>
    public static JsonSerializer CreateCleanSerializer(Type converterTypeToExclude)
    {
        var settings = SerializationManager.GetSettings();
        var converter = settings.Converters.FirstOrDefault(c => c.GetType() == converterTypeToExclude);
        if (converter != null)
        {
            settings.Converters.Remove(converter);
        }
        return JsonSerializer.Create(settings);
    }
}

public class NodeHandleConverter : JsonConverter<INodeHandleDTO>
{
    public override INodeHandleDTO? ReadJson(JsonReader reader, Type objectType, INodeHandleDTO? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        if (jo["Type"] == null) throw new JsonSerializationException("Missing 'Type' field in NodeHandle");
        NodeHandleType type = jo["Type"]!.ToObject<NodeHandleType>();

        return type switch
        {
            // NodeControllerDTO is a struct/simple object, so we can use a clean default serializer
            NodeHandleType.CIRCULAR => (INodeHandleDTO?)jo.ToObject<Assets.Prefabs.MapBuilder.Node.NodeControllerDTO>(new JsonSerializer()),
            _ => throw new JsonSerializationException($"Unknown NodeHandleType: {type}")
        };
    }

    public override void WriteJson(JsonWriter writer, INodeHandleDTO? value, JsonSerializer serializer)
    {
        JToken.FromObject(value!, new JsonSerializer()).WriteTo(writer);
    }
}

public class NodeContainerConverter : JsonConverter<INodeContainerDTO>
{
    public override INodeContainerDTO? ReadJson(JsonReader reader, Type objectType, INodeContainerDTO? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        if (jo["Type"] == null) throw new JsonSerializationException("Missing 'Type' field in NodeContainer");
        NodeContainerType type = jo["Type"]!.ToObject<NodeContainerType>();

        // Get a serializer that has all converters EXCEPT this one
        var cleanSerializer = ConverterUtils.CreateCleanSerializer(typeof(NodeContainerConverter));

        return type switch
        {
            NodeContainerType.POLYGON => jo.ToObject<PolygonBuilderDTO>(cleanSerializer),
            _ => throw new JsonSerializationException($"Unknown NodeContainerType: {type}")
        };
    }

    public override void WriteJson(JsonWriter writer, INodeContainerDTO? value, JsonSerializer serializer)
    {
        var cleanSerializer = ConverterUtils.CreateCleanSerializer(typeof(NodeContainerConverter));
        JToken.FromObject(value!, cleanSerializer).WriteTo(writer);
    }
}

public class NodeManagerConverter : JsonConverter<INodeManagerDTO>
{
    public override INodeManagerDTO? ReadJson(JsonReader reader, Type objectType, INodeManagerDTO? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        if (jo["Type"] == null) throw new JsonSerializationException("Missing 'Type' field in NodeManager");
        NodeManagerType type = jo["Type"]!.ToObject<NodeManagerType>();

        var cleanSerializer = ConverterUtils.CreateCleanSerializer(typeof(NodeManagerConverter));

        return type switch
        {
            NodeManagerType.ORDINARY => jo.ToObject<OrdinaryNodeManagerDTO>(cleanSerializer),
            _ => throw new JsonSerializationException($"Unknown NodeManagerType: {type}")
        };
    }

    public override void WriteJson(JsonWriter writer, INodeManagerDTO? value, JsonSerializer serializer)
    {
        var cleanSerializer = ConverterUtils.CreateCleanSerializer(typeof(NodeManagerConverter));
        JToken.FromObject(value!, cleanSerializer).WriteTo(writer);
    }
}

public class ExactTypeConverter<TInterface, TConcrete> : JsonConverter
    where TConcrete : class, TInterface, new()
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TInterface);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var cleanSerializer = ConverterUtils.CreateCleanSerializer(typeof(ExactTypeConverter<TInterface, TConcrete>));
        JObject jo = JObject.Load(reader);
        return jo.ToObject<TConcrete>(cleanSerializer);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var cleanSerializer = ConverterUtils.CreateCleanSerializer(typeof(ExactTypeConverter<TInterface, TConcrete>));
        JToken.FromObject(value!, cleanSerializer).WriteTo(writer);
    }
}

#endregion

public class SerializationManager : MonoBehaviour
{
    private string? mapsDirectoryPath;

    void Awake()
    {
        var appName = "SloppySloths";
        var mapsFolderName = "maps";
        var baseDir = Application.persistentDataPath;
        mapsDirectoryPath = Path.Combine(baseDir, appName, mapsFolderName);

        if (!Directory.Exists(mapsDirectoryPath))
        {
            Directory.CreateDirectory(mapsDirectoryPath);
        }
    }

    public static JsonSerializerSettings GetSettings()
    {
        return new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter>
            {
                new NodeHandleConverter(),
                new NodeContainerConverter(),
                new NodeManagerConverter(),
                new ExactTypeConverter<IMapStateDTO, MapStateDTO>(),
                new ExactTypeConverter<IVehicleBuilderDTO, VehicleBuilderDTO>(),
                new ExactTypeConverter<IStarDataDTO, StarDataDTO>(),
                new ExactTypeConverter<IFinishLineDTO, FinishLineDTO>(),
                new ExactTypeConverter<IHasPositionAsDTO, PositionDTO>()
            }
        };
    }

    public (string?, SerializationError?) SerializeAndSaveToFile(INodeManagerDTO dto, string filename)
    {
        if (mapsDirectoryPath == null) return (null, SerializationError.WRITING_TO_FILE_FAILED);

        var dataPath = Path.Combine(mapsDirectoryPath, filename);

        try
        {
            string json = JsonConvert.SerializeObject(dto, GetSettings());
            File.WriteAllText(dataPath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
            return (null, SerializationError.WRITING_TO_FILE_FAILED);
        }
        return (dataPath, null);
    }

    public (INodeManagerDTO?, DeserializationError?) DeserializeFromFile(string filename)
    {
        if (mapsDirectoryPath == null) return (null, DeserializationError.FILE_NOT_FOUND);
        var dataPath = Path.Combine(mapsDirectoryPath, filename);

        if (!File.Exists(dataPath))
        {
            return (null, DeserializationError.FILE_NOT_FOUND);
        }

        try
        {
            var json = File.ReadAllText(dataPath);
            var dto = JsonConvert.DeserializeObject<INodeManagerDTO>(json, GetSettings());
            return (dto, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}");
            return (null, DeserializationError.PARSING_FAILED);
        }
    }
}

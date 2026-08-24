using System;
using System.Collections.Generic;

namespace Rhino.Inside.AutoCAD.Core.IPC;

public enum MetadataQueryType
{
    Layers = 0,
    Blocks = 1,
    LineTypes = 2,
    Layouts = 3,
    Documents = 4
}

public class LayerInfoDto
{
    public string Name { get; set; } = string.Empty;
    public int ColorRgb { get; set; } = -1;
    public bool IsOff { get; set; }
    public bool IsFrozen { get; set; }
    public bool IsLocked { get; set; }
    public string LineTypeName { get; set; } = "Continuous";
    public string Handle { get; set; } = string.Empty;
}

public class BlockInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Handle { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public bool IsLayout { get; set; }
    public bool IsDynamicBlock { get; set; }
}

public class LineTypeInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Handle { get; set; } = string.Empty;
}

public class LayoutInfoDto
{
    public string Name { get; set; } = string.Empty;
    public int TabOrder { get; set; }
    public string Handle { get; set; } = string.Empty;
}

public class MetadataQueryRequest
{
    public MetadataQueryType QueryType { get; set; }
    public string FilterName { get; set; } = string.Empty;
}

public class MetadataQueryResponse
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public List<LayerInfoDto> Layers { get; set; } = new();
    public List<BlockInfoDto> Blocks { get; set; } = new();
    public List<LineTypeInfoDto> LineTypes { get; set; } = new();
    public List<LayoutInfoDto> Layouts { get; set; } = new();
}

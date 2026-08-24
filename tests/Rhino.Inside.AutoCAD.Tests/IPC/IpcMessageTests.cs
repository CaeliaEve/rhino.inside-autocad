using System;
using System.IO;
using System.Threading.Tasks;
using Rhino.Inside.AutoCAD.Core.IPC;
using Xunit;

namespace Rhino.Inside.AutoCAD.Tests;

public class IpcMessageTests
{
    [Fact]
    public void Create_WithPayload_ShouldSerializeCorrectly()
    {
        var bakeReq = new BakePayload
        {
            TargetLayer = "Layer1",
            Geometry3dmBytes = new byte[] { 1, 2, 3, 4, 5 }
        };

        var msg = IpcMessage.Create(IpcCommandType.BakeRequest, bakeReq);

        Assert.Equal(IpcCommandType.BakeRequest, msg.CommandType);
        Assert.NotNull(msg.Payload);
        Assert.NotEmpty(msg.Payload);

        var deserialized = msg.DeserializePayload<BakePayload>();
        Assert.NotNull(deserialized);
        Assert.Equal("Layer1", deserialized.TargetLayer);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, deserialized.Geometry3dmBytes);
    }

    [Fact]
    public async Task WriteAndRead_OverMemoryStream_ShouldRoundtrip()
    {
        var original = new SelectRequestPayload
        {
            PromptMessage = "Select a Curve:",
            SingleOnly = true,
            TargetType = "Curve"
        };

        var msg = IpcMessage.Create(IpcCommandType.SelectInCad, original);

        using var ms = new MemoryStream();
        await IpcMessage.WriteMessageAsync(ms, msg);

        ms.Position = 0;
        var readMsg = await IpcMessage.ReadMessageAsync(ms);

        Assert.NotNull(readMsg);
        Assert.Equal(IpcCommandType.SelectInCad, readMsg.CommandType);

        var readPayload = readMsg.DeserializePayload<SelectRequestPayload>();
        Assert.NotNull(readPayload);
        Assert.Equal("Select a Curve:", readPayload.PromptMessage);
        Assert.True(readPayload.SingleOnly);
        Assert.Equal("Curve", readPayload.TargetType);
    }

    [Fact]
    public void MetadataQueryResponse_ShouldSerializeCollections()
    {
        var resp = new MetadataQueryResponse
        {
            Success = true,
            Layers = { new LayerInfoDto { Name = "Wall", ColorRgb = 16711680, IsLocked = false } },
            Blocks = { new BlockInfoDto { Name = "Door_Block", Handle = "3A1" } },
            LineTypes = { new LineTypeInfoDto { Name = "DASHED", Description = "Dashed line" } },
            Layouts = { new LayoutInfoDto { Name = "Layout1", TabOrder = 1 } }
        };

        var msg = IpcMessage.Create(IpcCommandType.QueryMetadataResponse, resp);
        var read = msg.DeserializePayload<MetadataQueryResponse>();

        Assert.NotNull(read);
        Assert.True(read.Success);
        Assert.Single(read.Layers);
        Assert.Equal("Wall", read.Layers[0].Name);
        Assert.Equal(16711680, read.Layers[0].ColorRgb);
        Assert.Single(read.Blocks);
        Assert.Equal("Door_Block", read.Blocks[0].Name);
        Assert.Single(read.LineTypes);
        Assert.Equal("DASHED", read.LineTypes[0].Name);
        Assert.Single(read.Layouts);
        Assert.Equal("Layout1", read.Layouts[0].Name);
    }
}

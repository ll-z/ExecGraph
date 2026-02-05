using Xunit;
using ExecGraph.Builtins.Nodes;
using ExecGraph.Contracts.Common;
using System.Linq;

public class ConcatNodeTests
{
    [Fact]
    public async Task ConcatNode_JoinAllInputs()
    {
        var id = NodeId.New();
        var node = new ConcatNode(id, separator: " ", ignoreNulls: false, trim: true);
        var ctx = new TestRuntimeContext(id);
        ctx.SetInputs(("a", " Hello "), ("b", "World"), ("ignore", null));

        await node.ExecuteAsync(ctx);

        Assert.True(ctx.Outputs.ContainsKey("result"));
        var dv = ctx.Outputs["result"];
        Assert.Equal("Hello World", dv.Value);
    }

    [Fact]
    public async Task ConcatNode_ItemsArrayWins()
    {
        var id = NodeId.New();
        var node = new ConcatNode(id);
        var ctx = new TestRuntimeContext(id);
        ctx.SetInputs(("items", new[] { "A", "B", "C" }), ("x", "ignored"));

        await node.ExecuteAsync(ctx);

        Assert.True(ctx.Outputs.ContainsKey("result"));
        var dv = ctx.Outputs["result"];
        Assert.Equal("ABC", dv.Value);
    }
}

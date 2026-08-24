using System;
using Rhino.Inside.AutoCAD.Core.UI;
using Xunit;

namespace Rhino.Inside.AutoCAD.Tests;

public class WindowHelperTests
{
    [Fact]
    public void BringToFront_ZeroHandle_ShouldNotThrow()
    {
        var ex = Record.Exception(() => WindowHelper.BringToFront(IntPtr.Zero));
        Assert.Null(ex);
    }
}

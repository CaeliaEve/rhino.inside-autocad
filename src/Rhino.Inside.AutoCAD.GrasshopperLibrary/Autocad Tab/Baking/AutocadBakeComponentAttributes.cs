using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Custom attributes for <see cref="AutocadBakeComponent"/> that displays
/// a "Bake" button when Driven Button is enabled.
/// </summary>
public class AutocadBakeComponentAttributes : GH_ComponentAttributes
{
    private RectangleF _buttonBounds;
    private bool _buttonPressed;
    private const int ButtonHeight = 22;
    private const int ButtonPadding = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutocadBakeComponentAttributes"/> class.
    /// </summary>
    public AutocadBakeComponentAttributes(AutocadBakeComponent owner)
        : base(owner)
    {
    }

    private AutocadBakeComponent TypedOwner
        => (AutocadBakeComponent)this.Owner;

    /// <inheritdoc />
    protected override void Layout()
    {
        base.Layout();

        if (this.TypedOwner.DrivenButtonEnabled)
        {
            // Add space for button below component
            var bounds = this.Bounds;
            bounds.Height += ButtonHeight + ButtonPadding * 2;
            this.Bounds = bounds;

            // Calculate button bounds
            _buttonBounds = new RectangleF(
                this.Bounds.Left + ButtonPadding,
                this.Bounds.Bottom - ButtonHeight - ButtonPadding,
                this.Bounds.Width - ButtonPadding * 2,
                ButtonHeight);
        }
        else
        {
            _buttonBounds = RectangleF.Empty;
        }
    }

    /// <inheritdoc />
    protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
    {
        base.Render(canvas, graphics, channel);

        if (channel == GH_CanvasChannel.Objects && this.TypedOwner.DrivenButtonEnabled)
        {
            this.RenderButton(graphics);
        }
    }

    private void RenderButton(Graphics graphics)
    {
        // Guard against invalid button bounds (can happen if Layout hasn't been called yet)
        if (_buttonBounds.Width <= 0 || _buttonBounds.Height <= 0)
            return;

        // Draw button background using GH_Capsule for consistent Grasshopper styling
        var capsule = GH_Capsule.CreateTextCapsule(
            Rectangle.Round(_buttonBounds),
            Rectangle.Round(_buttonBounds),
            _buttonPressed ? GH_Palette.Grey : GH_Palette.Black,
            "Bake",
            GH_FontServer.Standard,
            GH_Orientation.horizontal_center,
            2,
           10);

        capsule.Render(graphics, this.Selected, this.Owner.Locked, false);
        capsule.Dispose();
    }

    /// <inheritdoc />
    public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        if (this.TypedOwner.DrivenButtonEnabled &&
            e.Button == System.Windows.Forms.MouseButtons.Left &&
            _buttonBounds.Contains(e.CanvasLocation))
        {
            _buttonPressed = true;
            sender.Invalidate();
            return GH_ObjectResponse.Capture;
        }
        return base.RespondToMouseDown(sender, e);
    }

    /// <inheritdoc />
    public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
    {
        if (_buttonPressed)
        {
            _buttonPressed = false;
            if (_buttonBounds.Contains(e.CanvasLocation))
            {
                this.TypedOwner.TriggerManualRun();
            }
            sender.Invalidate();
            return GH_ObjectResponse.Release;
        }
        return base.RespondToMouseUp(sender, e);
    }
}

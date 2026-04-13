using System.Numerics;

using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Rendering;

namespace Aprillz.MewUI.Gallery;

partial class GalleryView
{
    private FrameworkElement TransformPage()
    {
        // Shared image factory
        Image CatImage() => new Image()
            .Source(april).Width(120).Height(120)
            .StretchMode(Stretch.Uniform)
            .ImageScaleQuality(ImageScaleQuality.HighQuality);

        // --- Translate ---
        var translateBox = new TransformBox().Center().Child(CatImage());
        var sliderTX = new Slider().Minimum(-100).Maximum(100).Value(0).Width(150);
        var sliderTY = new Slider().Minimum(-100).Maximum(100).Value(0).Width(150);
        translateBox.Bind(TransformBox.TranslateXProperty, sliderTX, RangeBase.ValueProperty);
        translateBox.Bind(TransformBox.TranslateYProperty, sliderTY, RangeBase.ValueProperty);

        // --- Rotate ---
        var rotateBox = new TransformBox().Center().Child(CatImage());
        var sliderRotate = new Slider().Minimum(-180).Maximum(180).Value(0).Width(180);
        rotateBox.Bind(TransformBox.RotationDegreesProperty, sliderRotate, RangeBase.ValueProperty);

        // --- Scale ---
        var scaleBox = new TransformBox().Center().Child(CatImage());
        var sliderScale = new Slider().Minimum(10).Maximum(300).Value(100).Width(180);
        scaleBox.Bind(TransformBox.ScaleXProperty, sliderScale, RangeBase.ValueProperty,
            v => v / 100.0, v => v * 100.0);
        scaleBox.Bind(TransformBox.ScaleYProperty, sliderScale, RangeBase.ValueProperty,
            v => v / 100.0, v => v * 100.0);

        // --- Combined ---
        var combinedBox = new TransformBox().Center().Child(
            new Image().Source(logo).Width(200).Height(80)
                .StretchMode(Stretch.Uniform)
                .ImageScaleQuality(ImageScaleQuality.HighQuality));
        var sliderCombRot = new Slider().Minimum(-180).Maximum(180).Value(0).Width(140);
        var sliderCombScale = new Slider().Minimum(10).Maximum(300).Value(100).Width(140);
        combinedBox.Bind(TransformBox.RotationDegreesProperty, sliderCombRot, RangeBase.ValueProperty);
        combinedBox.Bind(TransformBox.ScaleXProperty, sliderCombScale, RangeBase.ValueProperty,
            v => v / 100.0, v => v * 100.0);
        combinedBox.Bind(TransformBox.ScaleYProperty, sliderCombScale, RangeBase.ValueProperty,
            v => v / 100.0, v => v * 100.0);

        // --- Transform Origin ---
        var originBox = new TransformBox().Center().Child(CatImage());
        var sliderOX = new Slider().Minimum(0).Maximum(100).Value(50).Width(120);
        var sliderOY = new Slider().Minimum(0).Maximum(100).Value(50).Width(120);
        var sliderOAngle = new Slider().Minimum(-180).Maximum(180).Value(30).Width(120);
        originBox.Bind(TransformBox.OriginXProperty, sliderOX, RangeBase.ValueProperty,
            v => v / 100.0, v => v * 100.0);
        originBox.Bind(TransformBox.OriginYProperty, sliderOY, RangeBase.ValueProperty,
            v => v / 100.0, v => v * 100.0);
        originBox.Bind(TransformBox.RotationDegreesProperty, sliderOAngle, RangeBase.ValueProperty);

        // Label helper: bind TextBlock.Text to a double MewProperty with format
        TextBlock Label(MewObject source, MewProperty<double> prop, string format, double width = 50) =>
            new TextBlock()
                .Bind(TextBlock.TextProperty, source, prop, v => v.ToString(format))
                .FontFamily("Consolas").Width(width)
                .VerticalAlignment(VerticalAlignment.Center);

        return CardGrid(
            Card(
                "Translate",
                new StackPanel().Vertical().Spacing(8).Children(
                    new Border().Height(160).ClipToBounds(true)
                        .HorizontalAlignment(HorizontalAlignment.Stretch)
                        .Child(translateBox),
                    new StackPanel().Horizontal().Spacing(8).Children(
                        new TextBlock().Text("X:").VerticalAlignment(VerticalAlignment.Center),
                        sliderTX,
                        Label(translateBox, TransformBox.TranslateXProperty, "F0", 40)),
                    new StackPanel().Horizontal().Spacing(8).Children(
                        new TextBlock().Text("Y:").VerticalAlignment(VerticalAlignment.Center),
                        sliderTY,
                        Label(translateBox, TransformBox.TranslateYProperty, "F0", 40))
                )
            ),

            Card(
                "Rotate",
                new StackPanel().Vertical().Spacing(8).Children(
                    new Border().Height(160).ClipToBounds(true)
                        .HorizontalAlignment(HorizontalAlignment.Stretch)
                        .Child(rotateBox),
                    new StackPanel().Horizontal().Spacing(8).Children(
                        new TextBlock().Text("Angle:").VerticalAlignment(VerticalAlignment.Center),
                        sliderRotate,
                        Label(rotateBox, TransformBox.RotationDegreesProperty, "0\u00b0"))
                )
            ),

            Card(
                "Scale",
                new StackPanel().Vertical().Spacing(8).Children(
                    new Border().Height(160).ClipToBounds(true)
                        .HorizontalAlignment(HorizontalAlignment.Stretch)
                        .Child(scaleBox),
                    new StackPanel().Horizontal().Spacing(8).Children(
                        new TextBlock().Text("Scale:").VerticalAlignment(VerticalAlignment.Center),
                        sliderScale,
                        Label(scaleBox, TransformBox.ScaleXProperty, "0.00\u0078"))
                )
            ),

            Card(
                "Rotate + Scale",
                new StackPanel().Vertical().Spacing(8).Children(
                    new Border().Height(160).ClipToBounds(true)
                        .HorizontalAlignment(HorizontalAlignment.Stretch)
                        .Child(combinedBox),
                    new StackPanel().Horizontal().Spacing(8).Children(
                        new TextBlock().Text("Angle:").VerticalAlignment(VerticalAlignment.Center),
                        sliderCombRot,
                        Label(combinedBox, TransformBox.RotationDegreesProperty, "0\u00b0")),
                    new StackPanel().Horizontal().Spacing(8).Children(
                        new TextBlock().Text("Scale:").VerticalAlignment(VerticalAlignment.Center),
                        sliderCombScale,
                        Label(combinedBox, TransformBox.ScaleXProperty, "0.00\u0078"))
                )
            ),

            Card(
                "Transform Origin",
                new StackPanel().Vertical().Spacing(8).Children(
                    new Border().Height(160).ClipToBounds(true)
                        .HorizontalAlignment(HorizontalAlignment.Stretch)
                        .Child(originBox),
                    new StackPanel().Horizontal().Spacing(8).Children(
                        new TextBlock().Text("Origin X:").VerticalAlignment(VerticalAlignment.Center),
                        sliderOX,
                        Label(originBox, TransformBox.OriginXProperty, "F2", 40)),
                    new StackPanel().Horizontal().Spacing(8).Children(
                        new TextBlock().Text("Origin Y:").VerticalAlignment(VerticalAlignment.Center),
                        sliderOY,
                        Label(originBox, TransformBox.OriginYProperty, "F2", 40)),
                    new StackPanel().Horizontal().Spacing(8).Children(
                        new TextBlock().Text("Angle:").VerticalAlignment(VerticalAlignment.Center),
                        sliderOAngle,
                        Label(originBox, TransformBox.RotationDegreesProperty, "0\u00b0"))
                )
            )
        );
    }
}

/// <summary>
/// Decorator that applies a composed Matrix3x2 transform before rendering the child.
/// Transform order: Translate(-origin) → Scale → Rotate → Translate(+origin + offset).
/// Origin is relative (0.0–1.0) to the content bounds; defaults to center (0.5, 0.5).
/// </summary>
file class TransformBox : FrameworkElement, IVisualTreeHost
{
    public static readonly MewProperty<UIElement?> ChildProperty =
        MewProperty<UIElement?>.Register<TransformBox>(nameof(Child), null,
            MewPropertyOptions.AffectsLayout,
            static (self, oldValue, newValue) => self.OnChildChanged(oldValue, newValue));

    private void OnChildChanged(UIElement? oldValue, UIElement? newValue)
    {
        if (oldValue != null) DetachChild(oldValue);
        if (newValue != null) AttachChild(newValue);
    }

    public static readonly MewProperty<double> TranslateXProperty =
        MewProperty<double>.Register<TransformBox>(nameof(TranslateX), 0.0, MewPropertyOptions.AffectsRender);

    public static readonly MewProperty<double> TranslateYProperty =
        MewProperty<double>.Register<TransformBox>(nameof(TranslateY), 0.0, MewPropertyOptions.AffectsRender);

    public static readonly MewProperty<double> RotationDegreesProperty =
        MewProperty<double>.Register<TransformBox>(nameof(RotationDegrees), 0.0, MewPropertyOptions.AffectsRender);

    public static readonly MewProperty<double> ScaleXProperty =
        MewProperty<double>.Register<TransformBox>(nameof(ScaleX), 1.0, MewPropertyOptions.AffectsRender);

    public static readonly MewProperty<double> ScaleYProperty =
        MewProperty<double>.Register<TransformBox>(nameof(ScaleY), 1.0, MewPropertyOptions.AffectsRender);

    public static readonly MewProperty<double> OriginXProperty =
        MewProperty<double>.Register<TransformBox>(nameof(OriginX), 0.5, MewPropertyOptions.AffectsRender);

    public static readonly MewProperty<double> OriginYProperty =
        MewProperty<double>.Register<TransformBox>(nameof(OriginY), 0.5, MewPropertyOptions.AffectsRender);

    public UIElement? Child
    {
        get => GetValue(ChildProperty);
        set => SetValue(ChildProperty, value);
    }

    public double TranslateX
    {
        get => GetValue(TranslateXProperty);
        set => SetValue(TranslateXProperty, value);
    }

    public double TranslateY
    {
        get => GetValue(TranslateYProperty);
        set => SetValue(TranslateYProperty, value);
    }

    public double RotationDegrees
    {
        get => GetValue(RotationDegreesProperty);
        set => SetValue(RotationDegreesProperty, value);
    }

    public double ScaleX
    {
        get => GetValue(ScaleXProperty);
        set => SetValue(ScaleXProperty, value);
    }

    public double ScaleY
    {
        get => GetValue(ScaleYProperty);
        set => SetValue(ScaleYProperty, value);
    }

    public double OriginX
    {
        get => GetValue(OriginXProperty);
        set => SetValue(OriginXProperty, value);
    }

    public double OriginY
    {
        get => GetValue(OriginYProperty);
        set => SetValue(OriginYProperty, value);
    }

    protected override Size MeasureContent(Size availableSize)
    {
        var child = Child;
        if (child == null) return Size.Empty;
        child.Measure(availableSize);
        return child.DesiredSize;
    }

    protected override void ArrangeContent(Rect bounds)
    {
        Child?.Arrange(bounds);
    }

    protected override UIElement? OnHitTest(Point point)
    {
        if (!IsVisible || !IsHitTestVisible) return null;

        if (Child?.HitTest(point) is UIElement hit)
        {
            return hit;
        }

        return Bounds.Contains(point) ? this : null;
    }

    private Matrix3x2 BuildTransformMatrix()
    {
        var bounds = Bounds;
        float cx = (float)(bounds.X + bounds.Width * OriginX);
        float cy = (float)(bounds.Y + bounds.Height * OriginY);

        var matrix = Matrix3x2.CreateTranslation(-cx, -cy);

        var sx = ScaleX;
        var sy = ScaleY;
        if (sx != 1.0 || sy != 1.0)
            matrix *= Matrix3x2.CreateScale((float)sx, (float)sy);

        var rotation = RotationDegrees;
        if (rotation != 0)
            matrix *= Matrix3x2.CreateRotation((float)(rotation * (Math.PI / 180.0)));

        matrix *= Matrix3x2.CreateTranslation(cx + (float)TranslateX, cy + (float)TranslateY);

        return matrix;
    }

    protected override void RenderSubtree(IGraphicsContext context)
    {
        var child = Child;
        if (child == null) return;

        bool hasTransform = TranslateX != 0 || TranslateY != 0
            || RotationDegrees != 0
            || ScaleX != 1.0 || ScaleY != 1.0;

        if (!hasTransform)
        {
            child.Render(context);
            return;
        }

        var current = context.GetTransform();
        var combined = BuildTransformMatrix() * current;

        context.Save();
        context.SetTransform(combined);
        child.Render(context);
        context.Restore();
    }

    bool IVisualTreeHost.VisitChildren(Func<Element, bool> visitor)
        => Child == null || visitor(Child);
}

file static class TransformBoxExtensions
{
    public static TransformBox Child(this TransformBox box, UIElement? child)
    {
        box.Child = child;
        return box;
    }
}

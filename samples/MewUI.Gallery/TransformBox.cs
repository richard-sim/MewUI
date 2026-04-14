using System.Numerics;

using Aprillz.MewUI.Controls;

using Aprillz.MewUI.Rendering;

namespace Aprillz.MewUI.Gallery;

/// <summary>
/// Decorator that applies a composed Matrix3x2 transform before rendering the child.
/// Transform order: Translate(-origin) → Scale → Rotate → Translate(+origin + offset).
/// Origin is relative (0.0–1.0) to the content bounds; defaults to center (0.5, 0.5).
/// </summary>
public class TransformBox : FrameworkElement, IVisualTreeHost
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

public static class TransformBoxExtensions
{
    public static TransformBox Child(this TransformBox box, UIElement? child)
    {
        box.Child = child;
        return box;
    }
}


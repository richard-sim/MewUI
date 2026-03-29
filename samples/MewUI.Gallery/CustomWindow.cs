using Aprillz.MewUI.Controls;

namespace Aprillz.MewUI.Gallery;

/// <summary>
/// A custom chrome window built on a transparent Window with Border-based rendering.
/// Demonstrates DragMove, IsActive/WindowState binding, CanMinimize/CanMaximize, and themed chrome.
/// </summary>
/// <remarks>
/// Provides rounded borders on Windows 10 and earlier where the OS does not support rounded corners natively.
/// However, this approach uses AllowsTransparency with per-frame alpha compositing, which has higher
/// CPU/GPU overhead on Win32. Prefer <see cref="NativeCustomWindow"/> for better performance on Windows 11+
/// and macOS where the OS provides native frame support (rounded corners, shadow, DWM border color).
/// </remarks>
public class CustomWindow : Window
{
    private const double TitleBarHeight = 28;
    private const double ButtonWidth = 32;
    private const double ChromeCornerRadius = 8;
    private const double ChromeButtonSize = 4;
    private const double ShadowExtent = 12;
    private const double ShadowOffset = 2;

    public static readonly MewProperty<Color> TitleBarColorProperty =
        MewProperty<Color>.Register<CustomWindow>("TitleBarColor", Color.Transparent, MewPropertyOptions.AffectsRender);

    public static readonly MewProperty<Color> ChromeBorderColorProperty =
        MewProperty<Color>.Register<CustomWindow>("ChromeBorderColor", Color.Transparent, MewPropertyOptions.AffectsRender);

    public static readonly MewProperty<Color> TitleForegroundProperty =
        MewProperty<Color>.Register<CustomWindow>("TitleForeground", Color.Transparent, MewPropertyOptions.AffectsRender);

    private readonly Border _chrome;
    private readonly ShadowDecorator _shadow;
    private readonly Border _contentArea;
    private readonly StackPanel _leftArea;
    private readonly StackPanel _rightArea;
    private readonly Button _minimizeBtn;
    private readonly Button _maximizeBtn;

    private static readonly Style ChromeButtonStyle = new(typeof(Button))
    {
        Transitions = [Transition.Create(Control.BackgroundProperty)],
        Setters =
        [
            Setter.Create(Control.BackgroundProperty, t => t.Palette.ButtonFace.WithAlpha(0)),
            Setter.Create(Control.BorderThicknessProperty, 0.0),
            Setter.Create(Control.CornerRadiusProperty, 0.0),
            Setter.Create(Control.PaddingProperty, new Thickness(0)),
        ],
        Triggers =
        [
            new StateTrigger
            {
                Match = VisualStateFlags.Hot,
                Setters = [Setter.Create(Control.BackgroundProperty, t => t.Palette.ButtonFace)],
            },
            new StateTrigger
            {
                Match = VisualStateFlags.Pressed,
                Setters = [Setter.Create(Control.BackgroundProperty, t => t.Palette.ButtonPressedBackground)],
            },
        ],
    };

    private static readonly Style CloseButtonStyle = new(typeof(Button))
    {
        Transitions = [Transition.Create(Control.BackgroundProperty)],
        Setters =
        [
            Setter.Create(Control.BackgroundProperty, Color.FromRgb(232, 17, 35).WithAlpha(0)),
            Setter.Create(Control.BorderThicknessProperty, 0.0),
            Setter.Create(Control.CornerRadiusProperty, 0.0),
            Setter.Create(Control.PaddingProperty, new Thickness(0)),
        ],
        Triggers =
        [
            new StateTrigger
            {
                Match = VisualStateFlags.Hot,
                Setters = [
                    Setter.Create(Control.BackgroundProperty, Color.FromRgb(232, 17, 35)),
                    Setter.Create(Control.ForegroundProperty, Color.White),
                ],
            },
            new StateTrigger
            {
                Match = VisualStateFlags.Pressed,
                Setters = [
                    Setter.Create(Control.BackgroundProperty, Color.FromRgb(200, 12, 28)),
                    Setter.Create(Control.ForegroundProperty, Color.White),
                ],
            },
        ],
    };


    public static MewProperty<HorizontalAlignment> TitleHorizontalAlignmentProperty = MewProperty<HorizontalAlignment>
        .Register<CustomWindow>(nameof(TitleHorizontalAlignment), HorizontalAlignment.Center, MewPropertyOptions.AffectsRender);

    public HorizontalAlignment TitleHorizontalAlignment
    {
        get => GetValue(TitleHorizontalAlignmentProperty);
        set => SetValue(TitleHorizontalAlignmentProperty, value);
    }
    public CustomWindow()
    {
        AllowsTransparency = true;
        base.Background = Color.Transparent;

        base.Padding = new Thickness(0);

        StyleSheet = new StyleSheet();
        StyleSheet.Define("chrome", ChromeButtonStyle);
        StyleSheet.Define("close", CloseButtonStyle);

        // Title text — bound directly to Window.TitleProperty
        var titleText = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(8, 0, 0, 0),
        };
        titleText.SetBinding(TextBlock.HorizontalAlignmentProperty, this, TitleHorizontalAlignmentProperty);
        titleText.SetBinding(TextBlock.TextProperty, this, TitleProperty);
        titleText.SetBinding(Control.ForegroundProperty, this, TitleForegroundProperty);

        // Chrome buttons
        _minimizeBtn = CreateChromeButton(GlyphKind.WindowMinimize);
        _minimizeBtn.Click += () => Minimize();
        _minimizeBtn.SetBinding(UIElement.IsVisibleProperty, this, CanMinimizeProperty);

        var maxGlyph = new GlyphElement().Kind(GlyphKind.WindowMaximize).GlyphSize(ChromeButtonSize);
        _maximizeBtn = CreateChromeButton(maxGlyph);
        _maximizeBtn.Click += () =>
        {
            if (WindowState == WindowState.Maximized)
                Restore();
            else
                Maximize();
        };
        _maximizeBtn.SetBinding(UIElement.IsVisibleProperty, this, CanMaximizeProperty);

        var closeBtn = CreateChromeButton(GlyphKind.Cross, isClose: true);
        closeBtn.Click += () => Close();
        closeBtn.SetBinding(UIElement.IsVisibleProperty, this, CanCloseProperty);

        var controlButtons = new StackPanel { Orientation = Orientation.Horizontal };
        controlButtons.Add(_minimizeBtn);
        controlButtons.Add(_maximizeBtn);
        controlButtons.Add(closeBtn);

        // Title bar areas
        _leftArea = new StackPanel { Orientation = Orientation.Horizontal };
        _rightArea = new StackPanel { Orientation = Orientation.Horizontal };

        // Title bar
        var titleBar = new Border
        {
            Padding = new Thickness(0),
            MinHeight = TitleBarHeight,
        };
        titleBar.SetBinding(Control.BackgroundProperty, this, TitleBarColorProperty);
        titleBar.Child = new DockPanel().Children(
            new Border().DockRight().Child(controlButtons),
            new Border().DockRight().Child(_rightArea),
            new Border().DockLeft().Child(_leftArea),
            titleText
        );

        // DragMove on title bar
        titleBar.MouseDown += e =>
        {
            if (e.Button == MouseButton.Left)
            {
                DragMove();
                e.Handled = true;
            }
        };

        titleBar.MouseDoubleClick += e =>
        {
            if (e.Button == MouseButton.Left && CanMaximize)
            {
                if (WindowState == WindowState.Maximized)
                {
                    Restore();
                }
                else
                {
                    Maximize();
                }
                e.Handled = true;
            }

        };

        // Content area
        _contentArea = new Border { Padding = new Thickness(16) };

        // Chrome border
        _chrome = new Border
        {
            CornerRadius = ChromeCornerRadius,
            BorderThickness = 1,
            ClipToBounds = true,
        };
        _chrome.SetBinding(Control.BorderBrushProperty, this, ChromeBorderColorProperty);
        _chrome.Child = new DockPanel().Children(
            new Border().DockTop().Child(titleBar),
            _contentArea
        );
        _chrome.WithTheme((t, b) => b.Background = t.Palette.WindowBackground);

        _shadow = new ShadowDecorator
        {
            BlurRadius = ShadowExtent,
            OffsetY = ShadowOffset,
            CornerRadius = ChromeCornerRadius,
            Child = _chrome,
        };
        _shadow.WithTheme((t, s) =>
            s.ShadowColor = Color.FromArgb((byte)(t.IsDark ? 100 : 48), 0, 0, 0));

        base.Content = _shadow;

        // React to IsActive, WindowState, and Theme changes
        Activated += UpdateChromeColors;
        Deactivated += UpdateChromeColors;
        this.WithTheme((_, _) => UpdateChromeColors());

        // WindowState → glyph + corner radius
        ClientSizeChanged += _ => OnWindowStateVisualUpdate();

        // Resize grip: detect mouse in shadow area (outside chrome border)
    }


    private void TitleBar_MouseDoubleClick(MouseEventArgs obj)
    {
        throw new NotImplementedException();
    }

    /// <summary>Left area of the title bar (e.g. MenuBar).</summary>
    public StackPanel TitleBarLeft => _leftArea;

    /// <summary>Right area of the title bar (e.g. theme toggle, search).</summary>
    public StackPanel TitleBarRight => _rightArea;

    public new UIElement? Content
    {
        get => _contentArea.Child;
        set => _contentArea.Child = value;
    }

    public new Thickness Padding
    {
        get => _contentArea.Padding;
        set => _contentArea.Padding = value;
    }


    private void UpdateChromeColors()
    {
        var p = Theme.Palette;
        SetValue(TitleBarColorProperty, p.WindowBackground.Lerp(p.ControlBackground, 0.5));
        SetValue(ChromeBorderColorProperty, IsActive ? p.Accent.Lerp(p.ControlBackground, 0.25) : p.ControlBorder);
        SetValue(TitleForegroundProperty, IsActive ? p.WindowText : p.DisabledText);
    }

    private void OnWindowStateVisualUpdate()
    {
        bool maximized = WindowState == WindowState.Maximized;
        if (_maximizeBtn.Content is GlyphElement glyph)
            glyph.Kind = maximized ? GlyphKind.WindowRestore : GlyphKind.WindowMaximize;

        _chrome.CornerRadius = maximized ? 0 : ChromeCornerRadius;
        _chrome.BorderThickness = maximized ? 0 : 1;
        _shadow.BlurRadius = maximized ? 0 : ShadowExtent;
        _shadow.CornerRadius = maximized ? 0 : ChromeCornerRadius;
    }

    private static Button CreateChromeButton(GlyphKind kind, bool isClose = false)
    {
        var glyph = new GlyphElement().Kind(kind).GlyphSize(ChromeButtonSize);
        return CreateChromeButton(glyph, isClose);
    }

    private static Button CreateChromeButton(Element content, bool isClose = false)
    {
        return new Button
        {
            Content = content,
            MinWidth = ButtonWidth,
            MinHeight = TitleBarHeight,
            StyleName = isClose ? "close" : "chrome",
        };
    }
}

public static class CustomWindowExtensions
{
    public static CustomWindow Content(this CustomWindow cw, UIElement? content)
    {
        cw.Content = content;
        return cw;
    }
}


using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Styling;

namespace Aprillz.MewUI;

public partial record class Theme
{
    private static readonly Transition[] ColorTransitions =
    [
        Transition.Create(Control.BackgroundProperty),
        Transition.Create(Control.BorderBrushProperty),
        Transition.Create(Control.ForegroundProperty),
    ];

    private static readonly Transition[] SliderColorTransitions =
    [
        ..ColorTransitions,
        Transition.Create(Slider.ThumbBrushProperty),
        Transition.Create(Slider.ThumbBorderBrushProperty),
    ];

    private static readonly Transition[] ToggleSwitchColorTransitions =
    [
        ..ColorTransitions,
        Transition.Create(ToggleSwitch.ThumbBrushProperty),
    ];

    private Dictionary<Type, Style>? _styles;

    // Tracks which Palette instance _styles was built from.
    // When a record 'with' expression copies _styles from a base theme,
    // this detects the stale cache and triggers a rebuild for the new Palette.
    private Palette? _stylesPalette;

    /// <summary>
    /// Gets the default style for the specified control type, or null if none registered.
    /// </summary>
    public Style? GetStyle(Type controlType)
    {
        if (_styles is null || !ReferenceEquals(_stylesPalette, Palette))
        {
            _stylesPalette = Palette;
            _styles = BuildDefaultStyles();
        }

        return _styles.GetValueOrDefault(controlType);
    }

    /// <summary>
    /// Registers an additional default style. Can be used by application code to add
    /// styles for custom control types.
    /// </summary>
    public void RegisterStyle(Style style)
    {
        if (_styles is null || !ReferenceEquals(_stylesPalette, Palette))
        {
            _stylesPalette = Palette;
            _styles = BuildDefaultStyles();
        }

        _styles[style.TargetType] = style;
    }

    private Dictionary<Type, Style> BuildDefaultStyles()
    {
        var styles = new Dictionary<Type, Style>();

        // Base
        Register(styles, CreateControlBaseStyle());

        // Button-like
        Register(styles, CreateButtonStyle());
        
        Register(styles, CreateToggleButtonStyle());
        
        Register(styles, CreateDropDownBaseStyle());
        
        Register(styles, CreateTabHeaderButtonStyle());

        Register(styles, CreateMenuBarStyle());

        // Input controls
        Register(styles, CreateTextBaseStyle());
        
        Register(styles, CreateControlBasedStyle(typeof(CheckBox),
            Setter.Create(Control.PaddingProperty, new Thickness(4, 2, 4, 2)),
            Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
            Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness)));
        
        Register(styles, CreateControlBasedStyle(typeof(RadioButton),
            Setter.Create(Control.PaddingProperty, new Thickness(4, 2, 4, 2)),
            Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
            Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness)));
        
        Register(styles, CreateToggleSwitchStyle());
        
        Register(styles, CreateControlBasedStyle(typeof(NumericUpDown),
            Setter.Create(Control.PaddingProperty, new Thickness(4, 2, 4, 2)),
            Setter.Create(FrameworkElement.MinHeightProperty, Metrics.BaseControlHeight),
            Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
            Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness)));
        
        Register(styles, CreateProgressBarStyle());
        
        Register(styles, CreateSliderStyle());

        // List / item controls
        Register(styles, CreateControlBasedStyle(typeof(ItemsControl),
            Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
            Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness)));
        
        Register(styles, CreateControlBasedStyle(typeof(VirtualizedItemsBase),
            Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
            Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness)));
        
        Register(styles, CreateControlBasedStyle(typeof(TreeView),
            Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
            Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness)));
        
        Register(styles, CreateControlBasedStyle(typeof(GridView),
            Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
            Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness)));

        // Popups
        Register(styles, CreateControlBasedStyle(typeof(ContextMenu),
            Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
            Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness)));
        
        Register(styles, CreateControlBasedStyle(typeof(ToolTip),
            Setter.Create(Control.PaddingProperty, new Thickness(8, 4, 8, 4)),
            Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
            Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness)));

        // Containers
        Register(styles, CreateExpanderStyle());
        Register(styles, CreateContainerStyle(typeof(GroupBox)));
        
        Register(styles, CreateTabControlStyle());
        
        Register(styles, CreateWindowStyle());

        // Misc
        Register(styles, CreateBorderStyle());

        Register(styles, CreateSplitterThumbStyle());
        
        Register(styles, CreateScrollBarStyle());

        return styles;
    }

    private static void Register(Dictionary<Type, Style> styles, Style style)
    {
        styles[style.TargetType] = style;
    }

    private Style CreateControlBaseStyle()
    {
        return new Style(typeof(Control))
        {
            Setters =
            [
                Setter.Create(Control.ForegroundProperty, Palette.WindowText),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
                Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness),
            ],
            Triggers =
            [
                new StateTrigger
                {
                    Match = VisualStateFlags.None,
                    Exclude = VisualStateFlags.Enabled,
                    Setters = [Setter.Create(Control.ForegroundProperty, Palette.DisabledText)],
                },
            ],
        };
    }

    private Style CreateControlBasedStyle(Type targetType, params SetterBase[] extraSetters)
    {
        var p = Palette;
        var borderHot = Color.Composite(p.ControlBorder, p.AccentBorderHotOverlay);

        return new Style(targetType)
        {
            Transitions = ColorTransitions,
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, p.ControlBackground),
                Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                Setter.Create(Control.ForegroundProperty, p.WindowText),
                ..extraSetters,
            ],
            Triggers =
            [
                new StateTrigger
                {
                    Match = VisualStateFlags.Hot,
                    Setters = [Setter.Create(Control.BorderBrushProperty, borderHot)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Focused,
                    Setters = [Setter.Create(Control.BorderBrushProperty, p.Accent)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.None,
                    Exclude = VisualStateFlags.Enabled,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.DisabledControlBackground),
                        Setter.Create(Control.ForegroundProperty, p.DisabledText),
                    ],
                },
            ],
        };
    }

    private Style CreateBorderStyle()
    {
        return new Style(typeof(Border))
        {
            Setters =
            [
                Setter.Create(Control.CornerRadiusProperty, 0.0),
                Setter.Create(Control.BorderThicknessProperty, 0.0),
            ],
        };
    }

    private Style CreateContainerStyle(Type targetType, params SetterBase[] extraSetters)
    {
        var p = Palette;
        return new Style(targetType)
        {
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, p.ContainerBackground),
                Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                Setter.Create(Control.PaddingProperty, new Thickness(8)),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
                Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness),
                ..extraSetters,
            ],
        };
    }

    private Style CreateWindowStyle()
    {
        return new Style(typeof(Window))
        {
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, Palette.WindowBackground),
                Setter.Create(Control.FontFamilyProperty, Metrics.FontFamily),
                Setter.Create(Control.FontSizeProperty, Metrics.FontSize),
                Setter.Create(Control.FontWeightProperty, Metrics.FontWeight),
                Setter.Create(Control.PaddingProperty, new Thickness(8)),
            ],
        };
    }

    private Style CreateSplitterThumbStyle()
    {
        var p = Palette;
        return new Style(typeof(SplitPanel.SplitterThumb))
        {
            Transitions = ColorTransitions,
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, Color.Transparent),
                Setter.Create(Control.BorderBrushProperty, p.ControlBorder.Lerp(p.Accent, 0.15)),
            ],
            Triggers =
            [
                new StateTrigger
                {
                    Match = VisualStateFlags.Hot,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.Accent.WithAlpha(26)),
                        Setter.Create(Control.BorderBrushProperty, p.ControlBorder.Lerp(p.Accent, 0.35)),
                    ],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Pressed,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.Accent.WithAlpha(48)),
                        Setter.Create(Control.BorderBrushProperty, p.ControlBorder.Lerp(p.Accent, 0.65)),
                    ],
                },
            ],
        };
    }

    private Style CreateScrollBarStyle()
    {
        var p = Palette;
        return new Style(typeof(ScrollBar))
        {
            Transitions = ColorTransitions,
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, p.ScrollBarThumb),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
            ],
            Triggers =
            [
                new StateTrigger
                {
                    Match = VisualStateFlags.Hot,
                    Setters = [Setter.Create(Control.BackgroundProperty, p.ScrollBarThumbHover)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Pressed,
                    Setters = [Setter.Create(Control.BackgroundProperty, p.ScrollBarThumbActive)],
                },
            ],
        };
    }

    private Style CreateToggleSwitchStyle()
    {
        var p = Palette;
        var borderHot = Color.Composite(p.ControlBorder, p.AccentBorderHotOverlay);

        return new Style(typeof(ToggleSwitch))
        {
            Transitions = ToggleSwitchColorTransitions,
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, p.ButtonFace),
                Setter.Create(Control.ForegroundProperty, p.WindowText),
                Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                Setter.Create(ToggleSwitch.ThumbBrushProperty, p.WindowText),
                Setter.Create(Control.PaddingProperty, new Thickness(8, 4, 8, 4)),
                Setter.Create(FrameworkElement.MinHeightProperty, Metrics.BaseControlHeight),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
                Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness),
            ],
            Triggers =
            [
                // Checked
                new StateTrigger
                {
                    Match = VisualStateFlags.Checked,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.Accent),
                        Setter.Create(ToggleSwitch.ThumbBrushProperty, p.AccentText),
                    ],
                },
                // Hot (unchecked)
                new StateTrigger
                {
                    Match = VisualStateFlags.Hot,
                    Exclude = VisualStateFlags.Checked,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.ButtonFace.Lerp(p.Accent, 0.08)),
                        Setter.Create(Control.BorderBrushProperty, borderHot),
                    ],
                },
                // Hot + Checked
                new StateTrigger
                {
                    Match = VisualStateFlags.Hot | VisualStateFlags.Checked,
                    Setters = [Setter.Create(Control.BackgroundProperty, p.Accent.Lerp(p.ControlBackground, 0.10))],
                },
                // Pressed (unchecked)
                new StateTrigger
                {
                    Match = VisualStateFlags.Pressed,
                    Exclude = VisualStateFlags.Checked,
                    Setters = [Setter.Create(Control.BackgroundProperty, p.ButtonFace.Lerp(p.Accent, 0.12))],
                },
                // Pressed + Checked
                new StateTrigger
                {
                    Match = VisualStateFlags.Pressed | VisualStateFlags.Checked,
                    Setters = [Setter.Create(Control.BackgroundProperty, p.Accent.Lerp(p.ControlBackground, 0.06))],
                },
                // Focused (unchecked)
                new StateTrigger
                {
                    Match = VisualStateFlags.Focused,
                    Exclude = VisualStateFlags.Checked,
                    Setters = [Setter.Create(Control.BorderBrushProperty, p.Accent)],
                },
                // Focused + Checked
                new StateTrigger
                {
                    Match = VisualStateFlags.Focused | VisualStateFlags.Checked,
                    Setters = [Setter.Create(Control.BorderBrushProperty, p.Accent)],
                },
                // Disabled
                new StateTrigger
                {
                    Match = VisualStateFlags.None,
                    Exclude = VisualStateFlags.Enabled,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.ButtonDisabledBackground),
                        Setter.Create(Control.ForegroundProperty, p.DisabledText),
                        Setter.Create(ToggleSwitch.ThumbBrushProperty, p.DisabledText),
                    ],
                },
                // Disabled + Checked
                new StateTrigger
                {
                    Match = VisualStateFlags.Checked,
                    Exclude = VisualStateFlags.Enabled,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.DisabledAccent),
                        Setter.Create(ToggleSwitch.ThumbBrushProperty, p.DisabledControlBackground),
                    ],
                },
            ],
        };
    }

    private Style CreateSliderStyle()
    {
        var p = Palette;
        // No triggers — thumb uses PickAccentBorder with its own thumbState (includes _isDragging).
        return new Style(typeof(Slider))
        {
            Transitions = SliderColorTransitions,
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, p.ControlBackground),
                Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                Setter.Create(Slider.ThumbBrushProperty, p.ControlBackground),
                Setter.Create(Slider.ThumbBorderBrushProperty, p.ControlBorder),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
                Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness),
            ],
            Triggers =
            [
                new StateTrigger
                {
                    Match = VisualStateFlags.Hot,
                    Setters = [Setter.Create(Slider.ThumbBorderBrushProperty, Color.Composite(p.ControlBorder, p.AccentBorderHotOverlay))],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Focused,
                    Setters = [Setter.Create(Slider.ThumbBorderBrushProperty, p.Accent)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Pressed,
                    Setters = [Setter.Create(Slider.ThumbBorderBrushProperty, p.Accent)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.None,
                    Exclude = VisualStateFlags.Enabled,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.ButtonDisabledBackground),
                        Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                        Setter.Create(Slider.ThumbBrushProperty, p.DisabledControlBackground),
                        Setter.Create(Slider.ThumbBorderBrushProperty, p.ControlBorder),
                    ],
                },
            ],
        };
    }

    private Style CreateProgressBarStyle()
    {
        var p = Palette;
        return new Style(typeof(ProgressBar))
        {
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, p.ControlBackground),
                Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
                Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness),
            ],
        };
    }

    private Style CreateExpanderStyle()
    {
        var p = Palette;
        var borderHot = Color.Composite(p.ControlBorder, p.AccentBorderHotOverlay);

        return new Style(typeof(Expander))
        {
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, Color.Transparent),
                Setter.Create(Control.BorderBrushProperty, Color.Transparent),
                Setter.Create(Control.ForegroundProperty, p.WindowText),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
                Setter.Create(Control.BorderThicknessProperty, 0.0),
            ],
            Triggers =
            [
                new StateTrigger
                {
                    Match = VisualStateFlags.Focused,
                    Setters = [Setter.Create(Control.BorderBrushProperty, p.Accent)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.None,
                    Exclude = VisualStateFlags.Enabled,
                    Setters = [Setter.Create(Control.ForegroundProperty, p.DisabledText)],
                },
            ],
        };
    }

    private Style CreateTabControlStyle()
    {
        var p = Palette;
        var activeTabBorder = p.ControlBorder.Lerp(p.Accent, 0.5);
        var tabContentBackground = p.ContainerBackground;
        return new Style(typeof(TabControl))
        {
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, tabContentBackground),
                Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                Setter.Create(Control.PaddingProperty, new Thickness(8)),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
                Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness),
            ],
            Triggers =
            [
                new StateTrigger
                {
                    Match = VisualStateFlags.Focused,
                    Setters = [Setter.Create(Control.BorderBrushProperty, activeTabBorder)],
                },
            ],
        };
    }

    private Style CreateMenuBarStyle()
    {
        var p = Palette;
        return new Style(typeof(MenuBar))
        {
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, p.ButtonFace),
                Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                Setter.Create(Control.ForegroundProperty, p.WindowText),
                Setter.Create(Control.PaddingProperty, new Thickness(4, 2, 4, 2)),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
            ],
        };
    }

    private Style CreateButtonStyle()
    {
        var p = Palette;
        var borderHot = Color.Composite(p.ControlBorder, p.AccentBorderHotOverlay);
        var borderActive = p.Accent;

        return new Style(typeof(Button))
        {
            Transitions = ColorTransitions,
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, p.ButtonFace),
                Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                Setter.Create(Control.ForegroundProperty, p.WindowText),
                Setter.Create(Control.PaddingProperty, new Thickness(8, 4, 8, 4)),
                Setter.Create(FrameworkElement.MinHeightProperty, Metrics.BaseControlHeight),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
                Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness),
            ],
            Triggers =
            [
                // Hot
                new StateTrigger
                {
                    Match = VisualStateFlags.Hot,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.ButtonHoverBackground),
                        Setter.Create(Control.BorderBrushProperty, borderHot),
                    ],
                },
                // Focused (border only)
                new StateTrigger
                {
                    Match = VisualStateFlags.Focused,
                    Setters = [Setter.Create(Control.BorderBrushProperty, borderActive)],
                },
                // Pressed
                new StateTrigger
                {
                    Match = VisualStateFlags.Pressed,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.ButtonPressedBackground),
                        Setter.Create(Control.BorderBrushProperty, borderActive),
                    ],
                },
                // Disabled
                new StateTrigger
                {
                    Match = VisualStateFlags.None,
                    Exclude = VisualStateFlags.Enabled,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.ButtonDisabledBackground),
                        Setter.Create(Control.ForegroundProperty, p.DisabledText),
                    ],
                },
            ],
        };
    }

    private Style CreateToggleButtonStyle()
    {
        var p = Palette;
        var borderHot = Color.Composite(p.ControlBorder, p.AccentBorderHotOverlay);
        var borderActive = p.Accent;
        var checkedBg = Color.Composite(p.ButtonFace, p.Accent.WithAlpha(96));
        var checkedHoverBg = Color.Composite(p.ButtonHoverBackground, p.Accent.WithAlpha(96));
        var checkedPressedBg = Color.Composite(p.ButtonPressedBackground, p.Accent.WithAlpha(96));
        var disabledCheckedBg = Color.Composite(p.ButtonFace, p.WindowText.WithAlpha(48));

        return new Style(typeof(ToggleButton))
        {
            Transitions = ColorTransitions,
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, p.ButtonFace),
                Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                Setter.Create(Control.ForegroundProperty, p.WindowText),
                Setter.Create(Control.PaddingProperty, new Thickness(8, 4, 8, 4)),
                Setter.Create(FrameworkElement.MinHeightProperty, Metrics.BaseControlHeight),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
                Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness),
            ],
            Triggers =
            [
                // Unchecked states
                new StateTrigger
                {
                    Match = VisualStateFlags.Hot,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.ButtonHoverBackground),
                        Setter.Create(Control.BorderBrushProperty, borderHot),
                    ],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Focused,
                    Setters = [Setter.Create(Control.BorderBrushProperty, borderActive)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Pressed,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.ButtonPressedBackground),
                        Setter.Create(Control.BorderBrushProperty, borderActive),
                    ],
                },
                // Checked states
                new StateTrigger
                {
                    Match = VisualStateFlags.Checked,
                    Setters = [Setter.Create(Control.BackgroundProperty, checkedBg)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Checked | VisualStateFlags.Hot,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, checkedHoverBg),
                        Setter.Create(Control.BorderBrushProperty, borderHot),
                    ],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Checked | VisualStateFlags.Focused,
                    Setters = [Setter.Create(Control.BorderBrushProperty, borderActive)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Checked | VisualStateFlags.Pressed,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, checkedPressedBg),
                        Setter.Create(Control.BorderBrushProperty, borderActive),
                    ],
                },
                // Disabled
                new StateTrigger
                {
                    Match = VisualStateFlags.None,
                    Exclude = VisualStateFlags.Enabled,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.ButtonDisabledBackground),
                        Setter.Create(Control.ForegroundProperty, p.DisabledText),
                    ],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Checked,
                    Exclude = VisualStateFlags.Enabled,
                    Setters = [Setter.Create(Control.BackgroundProperty, disabledCheckedBg)],
                },
            ],
        };
    }

    private Style CreateTextBaseStyle()
    {
        var p = Palette;
        var borderHot = Color.Composite(p.ControlBorder, p.AccentBorderHotOverlay);
        var borderActive = p.Accent;

        return new Style(typeof(TextBase))
        {
            Transitions = ColorTransitions,
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, p.ControlBackground),
                Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                Setter.Create(Control.ForegroundProperty, p.WindowText),
                Setter.Create(Control.PaddingProperty, new Thickness(4, 2, 4, 2)),
                Setter.Create(FrameworkElement.MinHeightProperty, Metrics.BaseControlHeight),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
                Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness),
            ],
            Triggers =
            [
                new StateTrigger
                {
                    Match = VisualStateFlags.Hot,
                    Setters = [Setter.Create(Control.BorderBrushProperty, borderHot)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Focused,
                    Setters = [Setter.Create(Control.BorderBrushProperty, borderActive)],
                },
                // Disabled
                new StateTrigger
                {
                    Match = VisualStateFlags.None,
                    Exclude = VisualStateFlags.Enabled,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.DisabledControlBackground),
                        Setter.Create(Control.ForegroundProperty, p.DisabledText),
                    ],
                },
            ],
        };
    }

    private Style CreateDropDownBaseStyle()
    {
        var p = Palette;
        var borderHot = Color.Composite(p.ControlBorder, p.AccentBorderHotOverlay);
        var borderActive = p.Accent;

        return new Style(typeof(DropDownBase))
        {
            Transitions = ColorTransitions,
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, p.ButtonFace),
                Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                Setter.Create(Control.ForegroundProperty, p.WindowText),
                Setter.Create(Control.PaddingProperty, new Thickness(8, 4, 8, 4)),
                Setter.Create(FrameworkElement.MinHeightProperty, Metrics.BaseControlHeight),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
                Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness),
            ],
            Triggers =
            [
                new StateTrigger
                {
                    Match = VisualStateFlags.Hot,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.ButtonHoverBackground),
                        Setter.Create(Control.BorderBrushProperty, borderHot),
                    ],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Focused,
                    Setters = [Setter.Create(Control.BorderBrushProperty, borderActive)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Active,
                    Setters = [Setter.Create(Control.BorderBrushProperty, borderActive)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Pressed,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.ButtonPressedBackground),
                        Setter.Create(Control.BorderBrushProperty, borderActive),
                    ],
                },
                // Disabled
                new StateTrigger
                {
                    Match = VisualStateFlags.None,
                    Exclude = VisualStateFlags.Enabled,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.ButtonDisabledBackground),
                        Setter.Create(Control.ForegroundProperty, p.DisabledText),
                    ],
                },
            ],
        };
    }

    private Style CreateTabHeaderButtonStyle()
    {
        var p = Palette;
        var borderHot = Color.Composite(p.ControlBorder, p.AccentBorderHotOverlay);
        var activeTabBorder = p.ControlBorder.Lerp(p.Accent, 0.5);
        var tabContentBackground = p.ContainerBackground;

        return new Style(typeof(TabHeaderButton))
        {
            Transitions = ColorTransitions,
            Setters =
            [
                Setter.Create(Control.BackgroundProperty, p.ButtonFace),
                Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                Setter.Create(Control.ForegroundProperty, p.WindowText),
                Setter.Create(Control.PaddingProperty, new Thickness(8, 4, 8, 4)),
                Setter.Create(FrameworkElement.MinHeightProperty, Metrics.BaseControlHeight),
                Setter.Create(Control.CornerRadiusProperty, Metrics.ControlCornerRadius),
                Setter.Create(Control.BorderThicknessProperty, Metrics.ControlBorderThickness),
            ],
            Triggers =
            [
                new StateTrigger
                {
                    Match = VisualStateFlags.Hot,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, p.ButtonHoverBackground),
                        Setter.Create(Control.BorderBrushProperty, borderHot),
                    ],
                },
                // Selected
                new StateTrigger
                {
                    Match = VisualStateFlags.Selected,
                    Setters = [Setter.Create(Control.BackgroundProperty, tabContentBackground)],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Selected,
                    Exclude = VisualStateFlags.Focused,
                    Setters =
                    [
                        Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                        Setter.Create(Control.ForegroundProperty, p.WindowText),
                    ],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Selected | VisualStateFlags.Focused,
                    Setters =
                    [
                        Setter.Create(Control.BorderBrushProperty, activeTabBorder),
                        Setter.Create(Control.ForegroundProperty, p.WindowText),
                    ],
                },
                new StateTrigger
                {
                    Match = VisualStateFlags.Selected,
                    Exclude = VisualStateFlags.Enabled,
                    Setters =
                    [
                        Setter.Create(Control.BackgroundProperty, tabContentBackground),
                        Setter.Create(Control.BorderBrushProperty, p.ControlBorder),
                        Setter.Create(Control.ForegroundProperty, p.DisabledText),
                    ],
                },
            ],
        };
    }
}

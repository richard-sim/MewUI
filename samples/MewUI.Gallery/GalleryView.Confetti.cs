using Aprillz.MewUI.Controls;

namespace Aprillz.MewUI.Gallery;

partial class GalleryView
{
    private FrameworkElement ConfettiPage()
    {
        ConfettiOverlay confetti = new(this);
        window.AdornerLayer.Add(confetti);

        return CardGrid(
            Card("Confetti",
                new StackPanel()
                    .Vertical()
                    .Spacing(8)
                    .Children(
                        new Label()
                            .Text("Port of WpfConfetti by caefale")
                            .WithTheme((t, c) => c.Foreground(t.Palette.DisabledText))
                            .FontSize(11),
                        new Button()
                            .Content("Burst")
                            .OnClick(() => confetti?.Burst()),
                        new Button()
                            .Content("Cannons")
                            .OnClick(() => confetti?.Cannons()),
                        new Button()
                            .Content("Start Rain")
                            .OnClick(() => confetti?.StartRain()),
                        new Button()
                            .Content("Stop Rain")
                            .OnClick(() => confetti?.StopRain()),
                        new Button()
                            .Content("Stop Cannons")
                            .OnClick(() => confetti?.StopCannons()),
                        new Button()
                            .Content("Clear All")
                            .OnClick(() => confetti?.Clear())
                    ))
        );
    }
}

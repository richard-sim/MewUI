namespace Aprillz.MewUI.Controls;

/// <summary>
/// A single-line password input control that masks entered text.
/// </summary>
public sealed class PasswordBox : SingleLineTextBase
{
    private bool _syncingText;

    static PasswordBox()
    {
        MaxLengthProperty.OverrideDefaultValue<PasswordBox>(32);
    }

    public static readonly MewProperty<string> PasswordProperty =
        MewProperty<string>.Register<PasswordBox>(nameof(Password), string.Empty,
            MewPropertyOptions.BindsTwoWayByDefault,
            static (self, _, newVal) => self.OnPasswordPropertyChanged(newVal));

    /// <summary>
    /// Gets or sets the character used to mask the password.
    /// </summary>
    public char PasswordChar { get; set; } = '●';

    /// <summary>
    /// Gets or sets the password text.
    /// </summary>
    public string Password
    {
        get => GetTextCore();
        set
        {
            var normalized = NormalizeText(value ?? string.Empty);
            if (GetTextCore() == normalized)
            {
                return;
            }

            SetValue(PasswordProperty, normalized);
        }
    }

    protected override void CopyDocumentTo(char[] buffer, int start, int length)
    {
        Array.Fill(buffer, PasswordChar, 0, length);
    }

    protected override void CopyToClipboardCore()
    {
        // Prevent copying password to clipboard.
    }

    protected override void CutToClipboardCore()
    {
        // Prevent cutting password to clipboard.
    }

    protected override void NotifyTextChanged()
    {
        _syncingText = true;
        try
        {
            SetValue(PasswordProperty, GetTextCore());
        }
        finally
        {
            _syncingText = false;
        }
        base.NotifyTextChanged();
    }

    private void OnPasswordPropertyChanged(string newValue)
    {
        if (_syncingText)
        {
            return;
        }

        _syncingText = true;
        try
        {
            ApplyExternalTextChange(newValue);
        }
        finally
        {
            _syncingText = false;
        }
    }
}

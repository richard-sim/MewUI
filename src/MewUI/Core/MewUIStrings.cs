namespace Aprillz.MewUI;

/// <summary>
/// Centralized UI strings for localization.
/// Default values are English. Assign <see cref="ObservableValue{T}.Value"/> at runtime to update all bound UI.
/// </summary>
public static class MewUIStrings
{
    // MessageBox
    public static ObservableValue<string> Information { get; } = new("Information");
    public static ObservableValue<string> Warning { get; } = new("Warning");
    public static ObservableValue<string> Error { get; } = new("Error");
    public static ObservableValue<string> Question { get; } = new("Confirm");
    public static ObservableValue<string> Success { get; } = new("Success");
    public static ObservableValue<string> Shield { get; } = new("Security");
    public static ObservableValue<string> Crash { get; } = new("Crash");
    public static ObservableValue<string> ShowDetail { get; } = new("Show Details");
    public static ObservableValue<string> OK { get; } = new("OK");
    public static ObservableValue<string> Cancel { get; } = new("Cancel");
    public static ObservableValue<string> Yes { get; } = new("Yes");
    public static ObservableValue<string> No { get; } = new("No");
    public static ObservableValue<string> Retry { get; } = new("Retry");
    public static ObservableValue<string> Ignore { get; } = new("Ignore");

    // BusyIndicator
    public static ObservableValue<string> Abort { get; } = new("Abort");
    public static ObservableValue<string> AbortConfirmation { get; } = new("Are you sure you want to abort this operation?");
    public static ObservableValue<string> Aborting { get; } = new("Aborting...");
}

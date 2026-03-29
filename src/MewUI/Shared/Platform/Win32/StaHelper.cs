using System.Runtime.Versioning;

namespace Aprillz.MewUI.Platform.Win32;

internal static class StaHelper
{
    public static T Run<T>(Func<T> func, Action? pump = null)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        {
            return func();
        }

        T result = default!;
        Exception? exception = null;
        using var done = new ManualResetEventSlim(false);

        var thread = new Thread(() =>
        {
            try
            {
                result = func();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                done.Set();
            }
        })
        {
            IsBackground = true
        };

#pragma warning disable CA1416 // Validate platform compatibility
        thread.SetApartmentState(ApartmentState.STA);
#pragma warning restore CA1416 // Validate platform compatibility
        thread.Start();

        while (!done.IsSet)
        {
            pump?.Invoke();
            Thread.Sleep(1);
        }

        if (exception != null)
        {
            throw new InvalidOperationException("STA call failed.", exception);
        }

        return result;
    }
}


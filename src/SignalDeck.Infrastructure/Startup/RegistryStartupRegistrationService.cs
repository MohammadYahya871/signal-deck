using Microsoft.Win32;
using SignalDeck.Core.Interfaces;

namespace SignalDeck.Infrastructure.Startup;

public sealed class RegistryStartupRegistrationService : IStartupRegistrationService
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "SignalDeck";

    public void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(RunKey, writable: true);

        if (enabled)
        {
            var executablePath = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(executablePath))
            {
                key.SetValue(ValueName, $"\"{executablePath}\"");
            }

            return;
        }

        if (key.GetValue(ValueName) is not null)
        {
            key.DeleteValue(ValueName);
        }
    }
}

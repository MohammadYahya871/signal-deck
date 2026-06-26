using Microsoft.Extensions.DependencyInjection;
using SignalDeck.Core.Interfaces;
using SignalDeck.Core.Services;
using SignalDeck.Infrastructure.AppLaunch;
using SignalDeck.Infrastructure.Audio;
using SignalDeck.Infrastructure.Idle;
using SignalDeck.Infrastructure.Persistence;
using SignalDeck.Infrastructure.Power;
using SignalDeck.Infrastructure.Session;
using SignalDeck.Infrastructure.Startup;

namespace SignalDeck.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSignalDeckInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ISettingsStore, JsonSettingsStore>();
        services.AddSingleton<IAudioDeviceService, NaudioDeviceService>();
        services.AddSingleton<IAudioPlaybackService, NaudioPlaybackService>();
        services.AddSingleton<AudioSessionPlaybackMonitor>();
        services.AddSingleton<WindowsMediaSessionPlaybackMonitor>();
        services.AddSingleton<IPlaybackActivityMonitor>(serviceProvider =>
            new CompositePlaybackActivityMonitor(
            [
                serviceProvider.GetRequiredService<AudioSessionPlaybackMonitor>(),
                serviceProvider.GetRequiredService<WindowsMediaSessionPlaybackMonitor>()
            ]));
        services.AddSingleton<IIdleMonitor, Win32IdleMonitor>();
        services.AddSingleton<IAppLaunchMonitor, PollingAppLaunchMonitor>();
        services.AddSingleton<ISessionStateMonitor, SessionStateMonitor>();
        services.AddSingleton<IPowerEventMonitor, SystemPowerEventMonitor>();
        services.AddSingleton<IStartupRegistrationService, RegistryStartupRegistrationService>();
        services.AddSingleton<SignalDeckCoordinator>();

        return services;
    }
}

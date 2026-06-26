using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalDeck.App.ViewModels;
using SignalDeck.App.Views;
using SignalDeck.Core.Interfaces;
using SignalDeck.Core.Services;
using SignalDeck.Infrastructure.DependencyInjection;

namespace SignalDeck.App;

public partial class App : System.Windows.Application
{
    private IHost? _host;
    private TrayIconHost? _trayIconHost;
    private MainWindow? _mainWindow;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSignalDeckInfrastructure();
                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainWindowViewModel>();
            })
            .Build();

        var settingsStore = _host.Services.GetRequiredService<ISettingsStore>();
        var settings = await settingsStore.LoadAsync();

        var coordinator = _host.Services.GetRequiredService<SignalDeckCoordinator>();
        coordinator.ApplySettings(settings);
        coordinator.Start();

        var viewModel = _host.Services.GetRequiredService<MainWindowViewModel>();
        await viewModel.InitializeAsync(settings);

        _mainWindow = _host.Services.GetRequiredService<MainWindow>();
        _mainWindow.DataContext = viewModel;

        _trayIconHost = new TrayIconHost(_mainWindow, ExitApplication);
        _trayIconHost.Initialize();
        _mainWindow.Hide();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _trayIconHost?.Dispose();

        if (_host is not null)
        {
            var coordinator = _host.Services.GetRequiredService<SignalDeckCoordinator>();
            coordinator.Dispose();
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }

    private void ExitApplication()
    {
        if (_mainWindow is not null)
        {
            _mainWindow.AllowClose = true;
            _mainWindow.Close();
        }

        Shutdown();
    }
}

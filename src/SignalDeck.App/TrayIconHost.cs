using System.Drawing;
using System.IO;
using System.Windows;
using Forms = System.Windows.Forms;

namespace SignalDeck.App;

public sealed class TrayIconHost : IDisposable
{
    private readonly Window _mainWindow;
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Action _exitApplication;

    public TrayIconHost(Window mainWindow, Action exitApplication)
    {
        _mainWindow = mainWindow;
        _exitApplication = exitApplication;
        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "SignalDeck",
            Icon = LoadIcon(),
            Visible = true
        };
    }

    public void Initialize()
    {
        var contextMenu = new Forms.ContextMenuStrip();
        contextMenu.Items.Add("Open", null, (_, _) => ShowMainWindow());
        contextMenu.Items.Add("Exit", null, (_, _) => _exitApplication());

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }

    private static Icon LoadIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "SignalDeck.ico");
        return File.Exists(iconPath)
            ? new Icon(iconPath)
            : SystemIcons.Application;
    }
}

using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SignalDeck.App.Views;

public partial class MainWindow : Window
{
    public bool AllowClose { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        LoadWindowIcon();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (AllowClose)
        {
            base.OnClosing(e);
            return;
        }

        e.Cancel = true;
        Hide();
    }

    private void LoadWindowIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "SignalDeck.ico");
        if (!File.Exists(iconPath))
        {
            return;
        }

        Icon = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
    }
}

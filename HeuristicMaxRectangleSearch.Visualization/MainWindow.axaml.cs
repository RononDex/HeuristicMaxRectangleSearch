using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace HeuristicMaxRectangleSearch.Visualization;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        OpenFile();
    }

    public async void OpenFile()
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open .out file",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>() { new FilePickerFileType("*.out") }
            });

            if (files.Count == 1)
            {
                var canvas = this.FindControl<Canvas>("canvas");
                var lines = File.ReadAllLines(files[0].Path.AbsolutePath);
                var scaleFactor = MathF.Min((float)this.Height, (float)this.Width);
                var lineSplitted = lines[2].Split(' ');
                for (var i = 0; i < int.Parse(lineSplitted[0]); i++)
                {
                    canvas!.Children.Add(new Line());
                }


            }
        }, DispatcherPriority.Default);


    }
}

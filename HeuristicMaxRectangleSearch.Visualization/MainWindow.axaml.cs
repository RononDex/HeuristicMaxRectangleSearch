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

        // OpenFile();
    }

    bool alreadyRendered = false;

    // public async void OpenFile()
    // {
    //     await Dispatcher.UIThread.InvokeAsync(async () =>
    //     {
    //         var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
    //         {
    //             Title = "Open .out file",
    //             AllowMultiple = false,
    //             FileTypeFilter = new List<FilePickerFileType>() { new FilePickerFileType("*.out") }
    //         });
    //
    //         if (files.Count == 1)
    //         {
    //             var canvas = this.FindControl<Canvas>("canvas");
    //             canvas.Background = new SolidColorBrush(Colors.White);
    //             var lines = File.ReadAllLines(files[0].Path.AbsolutePath);
    //             var scaleFactor = MathF.Min((float)this.Height, (float)this.Width);
    //             var lineSplitted = lines[2].Split(' ');
    //             var pointSize = 0.01f;
    //             var scalingFactor = this.Height * 0.9f;
    //             var xOffset = 0.05f;
    //             var yOffset = 0.05f;
    //             for (var i = 0; i < int.Parse(lineSplitted[0]); i++)
    //             {
    //                 var xPos = float.Parse(lineSplitted[i + 1].Split(",")[0]);
    //                 var yPos = float.Parse(lineSplitted[i + 1].Split(",")[1]);
    //                 var scaledPosX = (xPos + xOffset) * scaleFactor;
    //                 var scaledPosY = (yPos + yOffset) * scaleFactor;
    //                 var rect1 = new Rect(scaledPosX, scaledPosY, pointSize * scalingFactor, pointSize * 0.02f * scalingFactor);
    //                 var rect2 = new Rect(scaledPosX, scaledPosY, pointSize * 0.02f * scalingFactor, pointSize * scalingFactor);
    //
    //                 canvas!.Children.Add(rect1);
    //                 canvas!.Children.Add(rect2);
    //             }
    //
    //
    //         }
    //     }, DispatcherPriority.Default);
    //
    //
    // }

    public override void Render(DrawingContext context)
    {
        try
        {
            if (!double.IsNaN(this.Bounds.Height))
            {
                alreadyRendered = true;
                var field = 1;
                var folder = "/home/cobra/dev/HeuristicMaxRectangleSearch/HeuristicMaxRectangleSearch/";
                var inFilePath = $"{folder}points.in";
                var outFilePath = $"{folder}points.out";
                context.FillRectangle(new SolidColorBrush(Colors.White), this.Bounds);
                this.Background = null;

                var lines = File.ReadAllLines(inFilePath);
                var scaleFactor = MathF.Min((float)this.Height, (float)this.Width);
                var lineSplitted = lines[2 + field].Split(' ');
                var pointSize = 0.01f;
                var scalingFactor = scaleFactor * 0.9f;
                var xOffset = 0.05f;
                var yOffset = 0.05f;

                // Draw input
                context.DrawRectangle(new SolidColorBrush(Colors.Transparent), new Pen(new SolidColorBrush(Colors.Black), 2), new Rect(xOffset * scalingFactor, yOffset * scalingFactor, scalingFactor, scalingFactor));

                for (var i = 0; i < int.Parse(lineSplitted[0]); i++)
                {
                    var xPos = float.Parse(lineSplitted[i + 1].Split(",")[0]);
                    var yPos = float.Parse(lineSplitted[i + 1].Split(",")[1]);
                    var scaledPosX = xPos * scalingFactor + (xOffset * scalingFactor);
                    var scaledPosY = yPos * scalingFactor + (yOffset * scalingFactor);
                    var rect1 = new Rect(scaledPosX - (pointSize / 2f * scalingFactor), scaledPosY, pointSize * scalingFactor, pointSize * 0.02f * scalingFactor);
                    var rect2 = new Rect(scaledPosX, scaledPosY - (pointSize / 2f * scalingFactor), pointSize * 0.02f * scalingFactor, pointSize * scalingFactor);

                    context.DrawRectangle(new SolidColorBrush(Colors.Black), new Pen(new SolidColorBrush(Colors.Black), 2), rect1);
                    context.DrawRectangle(new SolidColorBrush(Colors.Black), new Pen(new SolidColorBrush(Colors.Black), 2), rect2);
                }

                // Draw output
                try
                {
                    var outLines = File.ReadAllLines(outFilePath);
                    var rectLine = outLines[field];
                    var rectLineSplitted = rectLine.Split(' ');
                    var points = new Point[4];
                    for (var i = 0; i < 4; i++)
                    {
                        var coordinateSplitted = rectLineSplitted[i].Split(',');
                        points[i] = new Point(
                                        float.Parse(coordinateSplitted[0]) * scalingFactor + (xOffset * scalingFactor),
                                        float.Parse(coordinateSplitted[1]) * scalingFactor + (yOffset * scalingFactor));
                    }
                    context.DrawLine(new Pen(new SolidColorBrush(Colors.Red), 2), points[0], points[1]);
                    context.DrawLine(new Pen(new SolidColorBrush(Colors.Red), 2), points[1], points[2]);
                    context.DrawLine(new Pen(new SolidColorBrush(Colors.Red), 2), points[2], points[3]);
                    context.DrawLine(new Pen(new SolidColorBrush(Colors.Red), 2), points[3], points[0]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error rendering output: {ex}");
                }
            }
            // Dispatcher.UIThread.Invoke(InvalidateVisual, DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rendering: {ex}");
        }

        // base.Render(context);
    }
}

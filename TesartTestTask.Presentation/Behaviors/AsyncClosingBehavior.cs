using Microsoft.Xaml.Behaviors;
using System.ComponentModel;
using System.Windows;
using TesartTestTask.Presentation.Interfaces;

namespace TesartTestTask.Presentation.Behaviors;

public class AsyncClosingBehavior : Behavior<Window>
{
    private bool _isClosing;

    protected override void OnAttached()
    {
        AssociatedObject.Closing += OnClosing;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Closing -= OnClosing;
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_isClosing)
            return;

        if (AssociatedObject.DataContext is not IAsyncCloseHandler handler)
            return;

        e.Cancel = true;
        AssociatedObject.IsEnabled = false;

        try
        {
            await handler.OnClosingAsync();
            _isClosing = true;
            await AssociatedObject.Dispatcher.BeginInvoke(new Action(AssociatedObject.Close));
        }
        finally
        {
            if (!_isClosing)
                AssociatedObject.IsEnabled = true;
        }
    }
}
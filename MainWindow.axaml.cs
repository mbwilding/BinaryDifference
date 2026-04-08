using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using BinaryDifference.Services;
using BinaryDifference.ViewModels;

namespace BinaryDifference;

public partial class MainWindow : Window
{
    private bool _syncingScroll;

    public MainWindow()
    {
        InitializeComponent();

        var vm = new MainWindowViewModel(
            new AvaloniaFileDialogService(this));

        DataContext = vm;
    }

    private void FormatComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.ChangeFormatCommand.Execute(null);
    }

    private void Scroll1_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_syncingScroll) return;
        _syncingScroll = true;
        Scroll2.Offset = Scroll1.Offset;
        _syncingScroll = false;
    }

    private void Scroll2_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_syncingScroll) return;
        _syncingScroll = true;
        Scroll1.Offset = Scroll2.Offset;
        _syncingScroll = false;
    }
}
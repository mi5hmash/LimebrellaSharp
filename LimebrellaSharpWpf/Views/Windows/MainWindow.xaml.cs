using System.Windows;
using LimebrellaSharpWpf.ViewModels;
using Mi5hmasH.WpfHelper.ControlProperties;

namespace LimebrellaSharpWpf.Views.Windows;

public partial class MainWindow
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow()
    {
        ViewModel = new MainWindowViewModel();
        DataContext = this;

        InitializeComponent();
    }

    #region FILE_DROP

    private void FileDrop_Drop(object sender, DragEventArgs e)
    {
        if (e.Data is not DataObject dataObject || !dataObject.ContainsFileDropList()) return;
        if (sender is not UIElement element) return;
        var dropOperationType = DropProperties.GetDropOperationType(element);
        ViewModel.OnFileDrop(dropOperationType, dataObject.GetFileDropList());
    }

    private void FileDrop_PreviewDragEnter(object sender, DragEventArgs e)
    {
        if (e.Data is not DataObject dataObject || !dataObject.ContainsFileDropList()) return;
        e.Effects = DragDropEffects.Copy;
    }

    private void FileDrop_PreviewDragOver(object sender, DragEventArgs e) => e.Handled = true;

    #endregion
}
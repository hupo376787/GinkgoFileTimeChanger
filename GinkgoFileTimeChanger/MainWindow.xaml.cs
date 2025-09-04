using GinkgoFileTimeChanger.Models;
using MicaWPF.Controls;
using MultiLanguageForXAML;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GinkgoFileTimeChanger;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MicaWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MyListView_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
    }

    private async void MyListView_Drop(object sender, DragEventArgs e)
    {
        var vm = DataContext as MainViewModel;

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            await Task.Run(async () =>
             {
                 // 获取拖入的文件路径
                 var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                 List<string> files = new List<string>();
                 SetStatusDescription(LanService.Get("analysising_files")!);

                 var dt = DateTime.Now;
                 foreach (var path in paths)
                 {
                     //文件夹
                     if (Directory.Exists(path))
                     {
                         foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                         {
                             files.Add(file);
                         }
                     }
                     //文件
                     else
                     {
                         files.Add(path);
                     }
                 }
                 Debug.WriteLine((DateTime.Now - dt).TotalMilliseconds);
                 await vm.AddFiles(files.ToArray());
             });
        }
    }
    private void SetStatusDescription(string msg)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var vm = DataContext as MainViewModel;
            vm.StatusDescription = msg;
        });
    }

    private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var vm = DataContext as MainViewModel;
        if ((sender as TextBlock)!.Text.Contains("Created") || (sender as TextBlock)!.Text.Contains("创建"))
            vm.CreatedTime = DateTime.Now;
        else if ((sender as TextBlock)!.Text.Contains("Modified") || (sender as TextBlock)!.Text.Contains("修改"))
            vm.ModifiedTime = DateTime.Now;
        else if ((sender as TextBlock)!.Text.Contains("Accessed") || (sender as TextBlock)!.Text.Contains("访问"))
            vm.AccessedTime = DateTime.Now;
    }
}
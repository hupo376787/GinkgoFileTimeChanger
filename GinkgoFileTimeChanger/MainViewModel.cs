using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GinkgoFileTimeChanger.Models;
using MultiLanguageForXAML;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace GinkgoFileTimeChanger
{
    public partial class MainViewModel : ObservableObject
    {
        string version = "v1.1";

        public MainViewModel()
        {
            StatusDescription = LanService.Get("ready")!;
            CreatedTime = DateTime.Now;
            ModifiedTime = DateTime.Now;
            AccessedTime = DateTime.Now;

            Files.CollectionChanged += Files_CollectionChanged;
        }

        [ObservableProperty]
        private ObservableCollection<FileItem> files = new();
        [ObservableProperty]
        private string statusDescription;
        [ObservableProperty]
        private DateTime createdTime;
        [ObservableProperty]
        private DateTime modifiedTime;
        [ObservableProperty]
        private DateTime accessedTime;
        [ObservableProperty]
        private Visibility dragDropHintVisibility;
        [ObservableProperty]
        private string currentLanguage = "en";

        private void Files_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DragDropHintVisibility = Files.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        [RelayCommand]
        private void ApplyAllTime()
        {
            AccessedTime = ModifiedTime = CreatedTime;
        }

        [RelayCommand]
        private async Task StartChange()
        {
            int i = 1;
            foreach (var file in Files)
            {
                if (file.Id == 0 || file.Path == "Ginkgo File Time Changer " + version || file.Path == "银杏文件时间修改器 " + version) continue;
                File.SetCreationTime(file.Path, CreatedTime);
                File.SetLastWriteTime(file.Path, ModifiedTime);
                File.SetLastAccessTime(file.Path, AccessedTime);
                file.Changed = true;

                StatusDescription = LanService.Get("changed_x_files")!.Replace("{0}", i.ToString());//$"Changed {i} files";
                await Task.Delay(1);
                i++;
            }
        }

        public async Task AddFiles(string[] files)
        {
            StatusDescription = LanService.Get("added_x_files")!.Replace("{0}", files.Count().ToString());// $"Found {files.Count()} files";
            int i = 0;
            foreach (var file in files)
            {
                await Application.Current!.Dispatcher.InvokeAsync(() => Files.Add(new FileItem() { Path = file }));

                StatusDescription = LanService.Get("added_x_files")!.Replace("{0}", i.ToString()).Replace("{1}", files.Count().ToString());//  $"Adding file {i}/{files.Count()}";
                await Task.Delay(new TimeSpan(0, 0, 0, 0, 1));
                i++;
            }
            ReorderId();
            StatusDescription = LanService.Get("added_x_files")!.Replace("{0}", i.ToString());// $"Added {files.Count()} files";
        }

        public async Task AddFolders(string[] folders)
        {
            foreach (var folder in folders)
            {
                List<string> files = new();
                foreach (var file in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
                {
                    files.Add(file);
                }
                await AddFiles(files.ToArray());
                await Task.Delay(new TimeSpan(0, 0, 0, 0, 1));
            }
        }

        private void ReorderId()
        {
            StatusDescription = LanService.Get("reordering_files")!;
            for (int i = 0; i < Files.Count; i++)
            {
                Files[i].Id = i + 1;
            }
        }

        [RelayCommand]
        private async Task AddFilesToList()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Multiselect = true;

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                await AddFiles(dialog.FileNames);
            }
        }

        [RelayCommand]
        private async Task AddFolderToList()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            //dialog.Multiselect = true;

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                await AddFolders(dialog.FolderNames);
            }
        }

        [RelayCommand]
        private void ClearList()
        {
            Files.Clear();
            StatusDescription = LanService.Get("ready")!;
        }

        [RelayCommand]
        private void Github()
        {
            Process.Start(new ProcessStartInfo("https://github.com/hupo376787/WeiboAlbumDownloader/releases") { UseShellExecute = true });
        }

        [RelayCommand]
        private void ChangeLanguage()
        {
            if (CurrentLanguage == "en")
            {
                LanService.UpdateCulture("zh");
                CurrentLanguage = "zh";
            }
            else
            {
                LanService.UpdateCulture("en");
                CurrentLanguage = "en";
            }
            StatusDescription = LanService.Get("ready")!;
        }

        [RelayCommand]
        private void About()
        {
            Files.Clear();
            Files.Add(new FileItem() { Id = 0, Path = LanService.Get("app_name") + " " + version });
        }
    }
}

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
        string version = "v2.2";

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
        private int maxParallel = 50;
        [ObservableProperty]
        private Visibility dragDropHintVisibility;
        [ObservableProperty]
        private string currentLanguage = "en";
        [ObservableProperty]
        private double progress;

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
            DateTime date = DateTime.Now;
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxParallel
            };

            int processed = 0;

            await Parallel.ForEachAsync(Files.ToArray(), options, async (file, token) =>
            {
                // 跳过无效文件
                if (file.Id == 0 ||
                    file.Path == "Ginkgo File Time Changer " + version ||
                    file.Path == "银杏文件时间修改器 " + version) return;

                if (!File.Exists(file.Path)) return;

                File.SetCreationTime(file.Path, CreatedTime);
                File.SetLastWriteTime(file.Path, ModifiedTime);
                File.SetLastAccessTime(file.Path, AccessedTime);
                file.Changed = true;

                int current = Interlocked.Increment(ref processed);
                Progress = current * 1.0 / Files.Count;

                // UI 更新必须通过 Dispatcher
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusDescription = LanService.Get("changed_x_files")!
                        .Replace("{0}", current.ToString());
                });
            });

            //避免并行更新导致的“竞态条件”，最终更新一次
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusDescription = LanService.Get("changed_x_files")!
                    .Replace("{0}", Files.Count.ToString());
            });
            Debug.WriteLine($"{MaxParallel}:{(DateTime.Now - date).TotalSeconds}");
        }

        [RelayCommand]
        private async Task SmartChange()
        {
            int i = 1;
            foreach (var file in Files)
            {
                if (file.Id == 0 || file.Path == "Ginkgo File Time Changer " + version || file.Path == "银杏文件时间修改器 " + version) continue;
                if (!File.Exists(file.Path)) continue;

                var dt = SmartDateParser.ExtractDateFromFileName(file.Path);
                File.SetCreationTime(file.Path, dt ?? CreatedTime);
                File.SetLastWriteTime(file.Path, dt ?? ModifiedTime);
                File.SetLastAccessTime(file.Path, dt ?? AccessedTime);
                file.Changed = true;

                StatusDescription = LanService.Get("changed_x_files")!.Replace("{0}", i.ToString());//$"Changed {i} files";
                await Task.Delay(1);
                i++;
            }
        }

        public async Task AddFiles(string[] files)
        {
            StatusDescription = LanService.Get("added_x_files")!.Replace("{0}", files.Count().ToString());
            int i = 0;
            foreach (var file in files)
            {
                Files.Add(new FileItem() { Path = file });
                StatusDescription = LanService.Get("added_x_files")!.Replace("{0}", i.ToString()).Replace("{1}", files.Count().ToString());
                //await Task.Delay(1);
                i++;
            }
            ReorderId();
            StatusDescription = LanService.Get("added_x_files")!.Replace("{0}", i.ToString());
        }

        public async Task AddFolders(string[] folders)
        {
            foreach (var folder in folders)
            {
                await AddFiles(Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories).ToArray());
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

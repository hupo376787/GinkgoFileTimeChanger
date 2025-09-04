using CommunityToolkit.Mvvm.ComponentModel;

namespace GinkgoFileTimeChanger.Models
{
    public partial class FileItem : ObservableObject
    {
        [ObservableProperty]
        private int id;
        [ObservableProperty]
        private string path;
        [ObservableProperty]
        private bool changed;
    }
}

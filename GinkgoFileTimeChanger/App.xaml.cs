using MultiLanguageForXAML;
using MultiLanguageForXAML.DB;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows;

namespace GinkgoFileTimeChanger;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        this.Startup += OnStartup;
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        InitLanguage();
    }

    private void InitLanguage()
    {
        string lan = "en";

        var culture = new CultureInfo(lan);
        // 设置当前线程的 UI 文化和区域设置
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;// 设置 WPF 的全局 Language 属性，影响绑定和转换器
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(
                System.Windows.Markup.XmlLanguage.GetLanguage(lan)));

        LanService.Init(new JsonFileDB("Languages"), true, lan, "en");
    }
}


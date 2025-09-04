using System.Windows;

namespace GinkgoFileTimeChanger
{
    public static class ButtonExtensions
    {
        // CornerRadius 附加属性
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                "CornerRadius",
                typeof(double),
                typeof(ButtonExtensions),
                new PropertyMetadata(0.0));

        public static double GetCornerRadius(DependencyObject obj)
        {
            return (double)obj.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(DependencyObject obj, double value)
        {
            obj.SetValue(CornerRadiusProperty, value);
        }
    }
}
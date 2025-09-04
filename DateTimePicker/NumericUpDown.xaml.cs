using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DateTimePicker
{
    /// <summary>
    /// NumericUpDown.xaml 的交互逻辑
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        public NumericUpDown()
        {
            InitializeComponent();
            IncreaseCommand = new RelayCommand(_ =>
            {
                if (Value >= Maximum)
                    Value = Minimum;   // 超过最大值 → 循环到最小值
                else
                    Value += 1;
            });

            DecreaseCommand = new RelayCommand(_ =>
            {
                if (Value <= Minimum)
                    Value = Maximum;   // 小于最小值 → 循环到最大值
                else
                    Value -= 1;
            });
            DataContext = this;
        }

        #region Dependency Properties
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumericUpDown),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(NumericUpDown), new PropertyMetadata(0));

        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(NumericUpDown), new PropertyMetadata(100));

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }
        #endregion

        #region Commands
        public ICommand IncreaseCommand { get; }
        public ICommand DecreaseCommand { get; }

        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            public RelayCommand(Action<object> execute) => _execute = execute;
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) => _execute(parameter);
            public event EventHandler CanExecuteChanged { add { } remove { } }
        }
        #endregion

        private void TextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            int delta = e.Delta > 0 ? 1 : -1;
            int newValue = Value + delta;

            if (newValue > Maximum) newValue = Minimum;
            if (newValue < Minimum) newValue = Maximum;

            Value = newValue;
            e.Handled = true;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 只允许输入数字
            if (!int.TryParse(e.Text, out _))
            {
                e.Handled = true; // 非数字输入被阻止
                return;
            }

            // 获取当前 TextBox 的文本（不包括正在输入的字符）
            var textBox = sender as TextBox;
            string currentText = textBox.Text;
            int caretIndex = textBox.CaretIndex;
            string newText = currentText.Remove(caretIndex, textBox.SelectedText.Length).Insert(caretIndex, e.Text);

            // 检查输入后的值是否在范围内
            if (int.TryParse(newText, out int newValue))
            {
                if (newValue < Minimum || newValue > Maximum)
                {
                    e.Handled = true; // 超出范围的输入被阻止
                }
            }
            else
            {
                e.Handled = true; // 无效输入被阻止
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // 失焦时验证并更正输入
            var textBox = sender as TextBox;
            if (int.TryParse(textBox.Text, out int newValue))
            {
                // 限制值在范围内
                Value = Math.Clamp(newValue, Minimum, Maximum);
            }
            else
            {
                // 无效输入时恢复到最小值
                Value = Minimum;
            }
        }
    }
}

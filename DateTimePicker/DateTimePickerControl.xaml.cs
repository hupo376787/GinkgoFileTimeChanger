using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DateTimePicker
{
    public partial class DateTimePickerControl : UserControl, INotifyPropertyChanged
    {
        private int _lastHour; // 缓存上一次的小时
        private int _lastMinute; // 缓存上一次的分钟
        private int _lastSecond; // 缓存上一次的秒

        public DateTimePickerControl()
        {
            InitializeComponent();
            //Hours = new ObservableCollection<int>(Enumerable.Range(0, 24));
            //Minutes = new ObservableCollection<int>(Enumerable.Range(0, 60));
            //Seconds = new ObservableCollection<int>(Enumerable.Range(0, 60));

            DataContext = this;

            // 初始化 SelectedDateTime
            SelectedDateTime = DateTime.Now;

            // 初始化 Hour/Minute/Second 和缓存
            _hour = SelectedDateTime.Hour;
            _minute = SelectedDateTime.Minute;
            _second = SelectedDateTime.Second;
            _lastHour = _hour;
            _lastMinute = _minute;
            _lastSecond = _second;

            OnPropertyChanged(nameof(Hour));
            OnPropertyChanged(nameof(Minute));
            OnPropertyChanged(nameof(Second));

            // 日期选择事件
            PART_DatePicker.SelectedDateChanged += PART_DatePicker_SelectedDateChanged;
        }

        #region SelectedDateTime 依赖属性
        public DateTime SelectedDateTime
        {
            get => (DateTime)GetValue(SelectedDateTimeProperty);
            set => SetValue(SelectedDateTimeProperty, value);
        }

        public static readonly DependencyProperty SelectedDateTimeProperty =
            DependencyProperty.Register(
                nameof(SelectedDateTime),
                typeof(DateTime),
                typeof(DateTimePickerControl),
                new FrameworkPropertyMetadata(DateTime.Now,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedDateTimeChanged));

        private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DateTimePickerControl picker)
            {
                if (picker._isUpdating) return;
                picker._isUpdating = true;

                var newDateTime = (DateTime)e.NewValue;
                if (newDateTime.Year == 1)
                {
                    picker._isUpdating = false;
                    return;
                }

                // 更新日期控件
                if (picker.PART_DatePicker.SelectedDate != newDateTime.Date)
                    picker.PART_DatePicker.SelectedDate = newDateTime.Date;

                // 只有当时间部分非零时才更新 _hour/_minute/_second
                if (newDateTime.Hour != 0 || newDateTime.Minute != 0 || newDateTime.Second != 0)
                {
                    picker._hour = newDateTime.Hour;
                    picker._minute = newDateTime.Minute;
                    picker._second = newDateTime.Second;
                    picker._lastHour = newDateTime.Hour;
                    picker._lastMinute = newDateTime.Minute;
                    picker._lastSecond = newDateTime.Second;

                    picker.OnPropertyChanged(nameof(Hour));
                    picker.OnPropertyChanged(nameof(Minute));
                    picker.OnPropertyChanged(nameof(Second));
                }

                picker._isUpdating = false;
            }
        }
        #endregion

        #region ComboBox 数据源
        //public ObservableCollection<int> Hours { get; }
        //public ObservableCollection<int> Minutes { get; }
        //public ObservableCollection<int> Seconds { get; }
        #endregion

        #region Hour / Minute / Second 属性
        private int _hour;
        public int Hour
        {
            get => _hour;
            set
            {
                if (_hour != value)
                {
                    _hour = value;
                    _lastHour = value; // 更新缓存
                    OnPropertyChanged(nameof(Hour));
                    UpdateSelectedDateTime();
                }
            }
        }

        private int _minute;
        public int Minute
        {
            get => _minute;
            set
            {
                if (_minute != value)
                {
                    _minute = value;
                    _lastMinute = value; // 更新缓存
                    OnPropertyChanged(nameof(Minute));
                    UpdateSelectedDateTime();
                }
            }
        }

        private int _second;
        public int Second
        {
            get => _second;
            set
            {
                if (_second != value)
                {
                    _second = value;
                    _lastSecond = value; // 更新缓存
                    OnPropertyChanged(nameof(Second));
                    UpdateSelectedDateTime();
                }
            }
        }
        #endregion

        private bool _isUpdating = false;

        private void UpdateSelectedDateTime()
        {
            if (_isUpdating) return;
            if (PART_DatePicker.SelectedDate is DateTime date)
            {
                _isUpdating = true;
                SelectedDateTime = new DateTime(
                    date.Year, date.Month, date.Day,
                    _hour, _minute, _second);
                _lastHour = _hour;
                _lastMinute = _minute;
                _lastSecond = _second;
                _isUpdating = false;
            }
        }

        private void PART_DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating) return;

            if (PART_DatePicker.SelectedDate is DateTime date)
            {
                _isUpdating = true;
                // 使用缓存的时间部分
                SelectedDateTime = new DateTime(
                    date.Year, date.Month, date.Day,
                    _lastHour, _lastMinute, _lastSecond);
                _isUpdating = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
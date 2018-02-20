using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 自分で追加したネームスペース
using System.Runtime.InteropServices;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace Pendulum
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>

    public sealed partial class MainPage : Page
    {
        private DispatcherTimer timer;
        private static bool is_start = false;
        private static int count_time = 0;

//        [DllImport("user32.dll")]
//        public static extern IntPtr GetForegroundWindow();

        public MainPage()
        {
            this.InitializeComponent();
            active_window.Text = "0";
            this.timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            this.timer.Tick += (sender, e) =>
            {
                count_time++;
                
                //active_window.Text = GetForegroundWindow().ToString(); 
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (is_start)
            {
                this.timer.Stop();
                is_start = false;
                start_button.Content = "start";
            }
            else
            {
                count_time = 0;
                this.timer.Start();
                is_start = true;
                start_button.Content = "stop";
            }
        }
    }
}

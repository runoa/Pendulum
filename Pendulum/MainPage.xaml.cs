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
using System.Text;

using System.Diagnostics;
using System.ComponentModel;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace Pendulum
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>

    public sealed partial class MainPage : Page
    {
        private DispatcherTimer timer;
        private static int total_time = 0;
        private static Dictionary<string, int> time_counts = new Dictionary<string, int>();
        private static TextBlock[] time_texts = new TextBlock[] { };
        private static TextBlock[] detail_texts = new TextBlock[] { };
        private static Dictionary<string, Dictionary<string, int>> detail_counts = new Dictionary<string, Dictionary<string, int>>();
        private static String[] suffix_appli_names = new String[]
        {
            "Google Chrome",
            "Slack",
            "Kindle",
            "LINE",
            "Mozilla Thunderbird",
            "タスク マネージャー",
            "コマンドプロンプト",
            "Atom",
            "Microsoft Visual Studio",
            "Visual Studio Code",
            "Tera Term",
            "VLCメディアプレイヤー",
            "Pendulum",
            "Windows Ink Workspace",
            "Excel",
            "Skype",
            "Atom",
            "Adobe Acrobat Reader DC",
            "PowerPoint",
            "Microsoft Word",
            "Magic: The Gathering Online",
            "Twitter",
            "Microsoft Store",
        };
        private static String[] prefix_appli_names = new String[]
        {
            "Surface",
            "Slack",
            "JTrim",
            "コマンド プロンプト",
            "選択コマンド プロンプト",
        };
        private static String[] version_no_appli_names = new String[]
        {
            "Honeyview",
            "sakura",
        };
        private static String[] other_appli_names = new String[]
        {
            "VT",
            "Atom",
        };

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", EntryPoint = "GetWindowText", CharSet = CharSet.Ansi)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public MainPage()
        {
            this.InitializeComponent();
            this.timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            this.timer.Tick += (sender, e) =>
            {
                tick_caller();
            };
            this.timer.Start();
        }

        private String get_time_string(int total_second)
        {
            String time_string = "";
            int hour = total_second / 3600;
            int minute = (total_second - 3600 * hour) / 60;
            int second = total_second - 3600 * hour - 60 * minute;
            if (hour > 0)
            {
                time_string += hour.ToString() + "h ";
            }
            if (minute > 0)
            {
                time_string += minute.ToString() + "m ";
            }
            if (second > 0)
            {
                time_string += second.ToString() + 's';
            }
            return time_string;
        }

        private String get_appli_name(String window_title)
        {
            // 大抵のアプリは - 区切りで末尾がアプリ名なのでまず末尾をチェックする
            String[] chars = window_title.Split('-');
            if (chars.Length > 0)
            {
                String suffix_window_title = chars[chars.Length - 1].Trim();
                if (Array.IndexOf(suffix_appli_names, suffix_window_title) != -1)
                {
                    // ただしKindleはKindleの名前が表示されてしまうため置き換える
                    return suffix_window_title;
                }

                // 次に末尾にあるがスペース区切りでバージョン番号が入っている例外を処理する
                String[] chars2 = suffix_window_title.Split(' ');
                String version_no_appli_name = chars2[0];
                if (Array.IndexOf(version_no_appli_names, version_no_appli_name) != -1)
                {
                    return version_no_appli_name;
                }
            }

            // 次に先頭にある例外を処理する
            String prefix_window_title = chars[0].Trim();
            if (Array.IndexOf(prefix_appli_names, prefix_window_title) != -1)
            {
                if (prefix_window_title == "Surface")
                {
                    return "Kindle";
                }
                if (prefix_window_title == "選択コマンド プロンプト")
                {
                    return "コマンド プロンプト";
                }
                return prefix_window_title;
            }

            // 最後にそもそもアプリ名が - で区切られていないアプリを処理する
            String[] chars3 = window_title.Split(' ');
            if (chars3.Length > 0)
            {
                String other_appli_name = chars3[chars3.Length - 1].Trim();
                if (Array.IndexOf(other_appli_names, other_appli_name) != -1)
                {
                    // ただしTera TermはVTと表示されるので置き換える
                    if (other_appli_name == "VT")
                    {
                        return "Tera Term";
                    }
                    return other_appli_name;
                }
            }

            // 全部違うならその他
            return "Other";
        }

        private void tick_caller()
        {
            // 合計の起動時間
            total_time++;
            time_list_view.Header = "合計時間: " + get_time_string(total_time);

            // 今、アクティブなアプリの起動時間
            IntPtr id = GetForegroundWindow();
            StringBuilder sb = new StringBuilder(65535);
            GetWindowText(id, sb, 65535);
            String window_title = sb.ToString();
            String appli_name = get_appli_name(window_title);
            if (!time_counts.ContainsKey(appli_name))
            {
                time_counts[appli_name] = 0;
                detail_counts[appli_name] = new Dictionary<string, int>();
            }
            if (!detail_counts[appli_name].ContainsKey(window_title))
            {
                detail_counts[appli_name][window_title] = 0;
            }
            time_counts[appli_name]++;
            detail_counts[appli_name][window_title]++;
            refresh();
        }

        private void refresh()
        {
            // アプリ名部分の描画
            String selected_appli_name = "";
            int index = 0;
            foreach (var item in time_counts.OrderByDescending((x) => x.Value))
            {
                String appli_name = item.Key;
                int count = item.Value;
                String text = get_time_string(count) + " / " + appli_name;
                if (index >= time_texts.Length)
                {
                    Array.Resize(ref time_texts, index + 1);
                    time_texts[index] = new TextBlock();
                    time_list_view.Items.Add(time_texts[index]);
                }
                if (index == time_list_view.SelectedIndex)
                {
                    selected_appli_name = appli_name;
                }
                time_texts[index].Text = get_time_string(count) + " / " + appli_name;
                index++;
            }
            for (int i = index; i < time_texts.Length; i++)
            {
                time_texts[i].Text = "";
            }

            // ウィンドウ名部分の描画
            if (selected_appli_name == "")
            {
                return;
            }
            index = 0;
            foreach (var item in detail_counts[selected_appli_name].OrderByDescending((x) => x.Value))
            {
                String window_title = item.Key;
                int count = item.Value;
                String text = get_time_string(count) + " / " + selected_appli_name;
                if (index >= detail_texts.Length)
                {
                    Array.Resize(ref detail_texts, index + 1);
                    detail_texts[index] = new TextBlock();
                    detail_list_view.Items.Add(detail_texts[index]);
                }
                detail_texts[index].Text = get_time_string(count) + " / " + window_title;
                index++;
            }
            for (int i = index; i < detail_texts.Length; i++)
            {
                detail_texts[i].Text = "";
            }
        }

        private void reset_button_Click(object sender, RoutedEventArgs e)
        {
            total_time = 0;
            time_counts = new Dictionary<string, int>();
            detail_counts = new Dictionary<string, Dictionary<string, int>>();
        }
    }
}

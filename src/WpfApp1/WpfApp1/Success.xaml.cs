using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Success.xaml 的交互逻辑
    /// </summary>
    public partial class Success : Window
    {
        private NotifyIcon notifyIcon;
        public Success()
        {
            InitializeComponent();

            string textFile = DateTime.Now.ToString("yyyyMMdd") + "Log.txt";
            FileStream fs;
            if (File.Exists(textFile))
            {
                fs = new FileStream(textFile, FileMode.Open, FileAccess.Read);
                using (fs)
                {
                    TextRange text = new TextRange(SuccessInfomation.Document.ContentStart, SuccessInfomation.Document.ContentEnd);
                    text.Load(fs, System.Windows.DataFormats.Text);
                }
            }
            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.BalloonTipText = "系统监控中... ...";
            this.notifyIcon.ShowBalloonTip(2000);
            this.notifyIcon.Text = "系统监控中... ...";
            this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            this.notifyIcon.Visible = true;
            //打开菜单项
            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("打开");
            open.Click += new EventHandler(Show);
            //退出菜单项
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("退出");
            exit.Click += new EventHandler(Close);
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { open, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == MouseButtons.Left) this.Show(o, e);
            });

            this.StateChanged += Success_StateChanged;

        }

        private void Success_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState==WindowState.Minimized)
            {
                this.Hide();
            }
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();

            Verification vf = new Verification();
            vf.ShowDialog();
        }

        public void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Infomation(sender, e);
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(Infomation);

            int fen = Convert.ToInt32(ConfigurationManager.AppSettings["sjjg"]);
            dispatcherTimer.Interval = new TimeSpan(0, 0, fen); //两分钟
            dispatcherTimer.Start();

            MainWindow mw = new MainWindow();
            mw.Button_Click(sender, e);
        }


        private void Infomation(object sender, EventArgs e)
        {
            string textFile = DateTime.Now.ToString("yyyyMMdd") + "Log.txt";
            FileStream fs;
            if (File.Exists(textFile))
            {
                fs = new FileStream(textFile, FileMode.Open, FileAccess.Read);
                using (fs)
                {
                    TextRange text = new TextRange(SuccessInfomation.Document.ContentStart, SuccessInfomation.Document.ContentEnd);
                    text.Load(fs, System.Windows.DataFormats.Text);
                }
            }
        }

        private void Show(object sender, EventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Visible;
            this.ShowInTaskbar = true;
            if (this.Activate())
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void Hide(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Close(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}

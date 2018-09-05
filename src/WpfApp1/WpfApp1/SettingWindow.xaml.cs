using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using WpfApp1.ServiceReference1;
using System.Windows.Threading;
using System.Data;
using System.IO;
using System.Reflection;
using System.Configuration;
using System.Threading;
using WpfApp1.Properties;
using System.Windows.Interop;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();

            fwqdz.Text = ConfigurationManager.AppSettings["fwqdz"];
            sjkmc.Text = ConfigurationManager.AppSettings["sjkmc"];
            sjkyhm.Text = ConfigurationManager.AppSettings["sjkyhm"];
            sjkmm.Password = ConfigurationManager.AppSettings["sjkmm"];
            xkzs.Text = ConfigurationManager.AppSettings["licensekey"];
            scbh.Text = ConfigurationManager.AppSettings["mallid"];
            yhzh.Text = ConfigurationManager.AppSettings["username"];
            mm.Password = ConfigurationManager.AppSettings["password"];
            dph.Text = ConfigurationManager.AppSettings["storecode"];
            sjjg.Text = ConfigurationManager.AppSettings["sjjg"];
            scdz.Text = ConfigurationManager.AppSettings["address"];

        }

        public void OkClick(object sender, RoutedEventArgs e)
        {
            Evaluation();
            MainWindow mainwindow = new MainWindow();
            mainwindow.StartClick(sender, e);

            if (mainwindow.Result)
            {
                this.Close();
                mainwindow.Show();
            }
            else
            {
                MessageBox.Show("数据库配置错误!");
            }
        }
        //取值
        private void Evaluation()
        {
            //获取许可证书 用户名 密码  店铺号 
            Configuration fwqdzM = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            fwqdzM.AppSettings.Settings["fwqdz"].Value = fwqdz.Text;
            fwqdzM.Save();
            Configuration sjkmcM = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            sjkmcM.AppSettings.Settings["sjkmc"].Value = sjkmc.Text;
            sjkmcM.Save();
            Configuration sjkyhmM = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            sjkyhmM.AppSettings.Settings["sjkyhm"].Value = sjkyhm.Text;
            sjkyhmM.Save();
            Configuration sjkmmM = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            sjkmmM.AppSettings.Settings["sjkmm"].Value = sjkmm.Password;
            sjkmmM.Save();
            Configuration licensekeyM = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            licensekeyM.AppSettings.Settings["licensekey"].Value = xkzs.Text;
            licensekeyM.Save();
            Configuration mallidM = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            mallidM.AppSettings.Settings["mallid"].Value = scbh.Text;
            mallidM.Save();
            Configuration usernameM = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            usernameM.AppSettings.Settings["username"].Value = yhzh.Text;
            usernameM.Save();
            Configuration passwordM = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            passwordM.AppSettings.Settings["password"].Value = mm.Password;
            passwordM.Save();
            Configuration dphM = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            dphM.AppSettings.Settings["storecode"].Value = dph.Text;
            dphM.Save();
            Configuration sjjgM = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            sjjgM.AppSettings.Settings["sjjg"].Value = sjjg.Text;
            sjjgM.Save();
            Configuration addressM = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            addressM.AppSettings.Settings["address"].Value = scdz.Text;
            addressM.Save();
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["com_no"]))
            {
                Configuration com_no = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                com_no.AppSettings.Settings["com_no"].Value = "0";
                com_no.Save();
            }
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}

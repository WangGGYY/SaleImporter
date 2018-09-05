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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Verification.xaml 的交互逻辑
    /// </summary>
    public partial class VerificationWindow : Window
    {
        public bool Result;//true: ok, false:
        public VerificationWindow(Window owner)
        {
            InitializeComponent();
            this.Owner = owner;
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            string time = DateTime.Now.ToString("yyyyMMdd");
            if (yhzh.Password == time)
            {
                Result = true;
                Close();
            }
            else
            {
                MessageBox.Show("密码错误!");
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            //Success sc = new Success();
            //sc.ShowDialog();
            Close();
        }
    }
}

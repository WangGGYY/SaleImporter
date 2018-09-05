using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
using WpfApp1.ServiceReference1;
namespace WpfApp1
{
    /// <summary>
    /// Success.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool Result;

        public MainWindow()
        {
            InitializeComponent();
            stopButton.IsEnabled = false;
            //this.StateChanged += Success_StateChanged;
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string textFile = DateTime.Now.ToString("yyyyMMdd") + "Log.txt";
            FileStream fs;
            //读取记事本记录
            if (File.Exists(textFile))
            {
                fs = new FileStream(textFile, FileMode.Open, FileAccess.Read);
                using (fs)
                {
                    int fsLen = (int)fs.Length;
                    byte[] heByte = new byte[fsLen];
                    int r = fs.Read(heByte, 0, heByte.Length);
                    string myStr = System.Text.Encoding.UTF8.GetString(heByte);
                    SuccessInfomation.Text = myStr;
                    SuccessInfomation.ScrollToEnd();

                }
            }

            dispatcherTimer.Tick += new EventHandler(Send);
            int fen = Convert.ToInt32(ConfigurationManager.AppSettings["sjjg"]);
            dispatcherTimer.Interval = new TimeSpan(0, 0, fen); //两分钟
            dispatcherTimer.Start();
            #region 
            //this.notifyIcon = new NotifyIcon();
            //this.notifyIcon.BalloonTipText = "系统监控中... ...";
            //this.notifyIcon.ShowBalloonTip(2000);
            //this.notifyIcon.Text = "系统监控中... ...";
            //this.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            //this.notifyIcon.Visible = true;
            ////打开菜单项
            //System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("打开");
            //open.Click += new EventHandler(Show);
            ////退出菜单项
            //System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("退出");
            //exit.Click += new EventHandler(Close);
            ////关联托盘控件
            //System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { open, exit };
            //notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

            //this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            //{
            //    if (e.Button == MouseButtons.Left) this.Show(o, e);
            //}); 
            #endregion

        }


        //点击设置
        public void SettingClick(object sender, RoutedEventArgs e)
        {
            VerificationWindow vf = new VerificationWindow(this);
            vf.ShowDialog();
            if (vf.Result)
            {
                SettingWindow setWnd = new SettingWindow();
                setWnd.ShowDialog();
                Close();
            }
            else
            {
            }
        }
        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        //点击启动
        public void StartClick(object sender, RoutedEventArgs e)
        {
            stopButton.IsEnabled = true;
            startButton.IsEnabled = false;
            settingtButton.IsEnabled = false;
            Prompt.Content = "连接中";
            Send(sender, e);
        }

        public void Send(object sender, EventArgs e)
        {
            dispatcherTimer.Stop();

            requestheader header = new requestheader();
            header.licensekey = ConfigurationManager.AppSettings["licensekey"].ToString();
            header.username = ConfigurationManager.AppSettings["username"].ToString();
            header.password = ConfigurationManager.AppSettings["password"].ToString();
            header.lang = "中文";
            header.pagerecords = 1;
            header.pageno = 1;
            header.updatecount = 1;
            header.messagetype = "SALESDATA";
            header.messageid = "332";
            header.version = "V332M";

            StringBuilder sb = new StringBuilder();

            DataTable payflow = Obtain("select t_rm_payflow.pay_way,sell_way,com_no,t_rm_payflow.flow_no,t_rm_payflow.sale_man,t_rm_payflow.vip_no,t_rm_payflow.pay_amount  from  t_rm_payflow where pay_way !='CHG' and com_no > " + ConfigurationManager.AppSettings["com_no"]);

            if (payflow == null)
            {
                dispatcherTimer.Start();

                return;
            }
            //去重
            for (int i = payflow.Rows.Count - 2; i > 0; i--)
            {
                DataRow[] rows = payflow.Select(string.Format("{0}='{1}'", "flow_no", payflow.Rows[i]["flow_no"]));
                if (rows.Length > 1)
                {
                    payflow.Rows.RemoveAt(i);
                }
            }
            //一次获取付款表的销售单号 并把当前最大的标识列取出保存
            foreach (DataRow item in payflow.Rows)
            {

                sb.Append(item["flow_no"] + ",");
            }
            //截掉最后一个逗号
            string strS = "";
            if (sb.Length > 0)
            {
                strS = sb.ToString().Remove(sb.ToString().LastIndexOf(","), 1);
            }
            else
            {
                dispatcherTimer.Start();

                return;
            }
            //用来循环
            DataTable saleflowold = new DataTable();
            //查询流水表 用来赋值
            DataTable saleflow = Obtain("select posid, flow_no,sell_way,sale_qnty, CONVERT(varchar(100), t_rm_saleflow.oper_date, 112) as oper_day, replace(CONVERT(varchar(100), oper_date, 8),':','') as oper_time, t_rm_saleflow.oper_id, t_rm_saleflow.sale_qnty, t_rm_saleflow.sale_money,t_rm_saleflow.item_no, (source_price - sale_price) * sale_qnty as salequt,t_rm_saleflow.sale_money from t_rm_saleflow where flow_no  in (" + strS + ") ");
            saleflowold = saleflow;
            if (saleflowold == null)
            {
                dispatcherTimer.Start();

                return;
            }
            //去重
            for (int i = saleflowold.Rows.Count - 2; i > 0; i--)
            {
                DataRow[] rows = saleflowold.Select(string.Format("{0}='{1}'", "flow_no", saleflowold.Rows[i]["flow_no"]));
                if (rows.Length > 1)
                {
                    saleflowold.Rows.RemoveAt(i);
                }
            }
            List<SalesModel> list = new List<SalesModel>();

            list = ConvertToModel(saleflow);

            foreach (DataRow payRow in payflow.Rows)
            {
                foreach (DataRow saleRow in saleflowold.Rows)
                {
                    if (payRow["flow_no"].ToString() == saleRow["flow_no"].ToString() && saleRow["sell_way"].ToString() != "C")
                    {
                        esaleshdr sales = new esaleshdr();

                        sales.mallid = ConfigurationManager.AppSettings["mallid"];//商场编号
                        sales.txdate_yyyymmdd = saleRow["oper_day"].ToString();

                        sales.txtime_hhmmss = saleRow["oper_time"].ToString();
                        sales.storecode = ConfigurationManager.AppSettings["storecode"].ToString();
                        if (saleRow["posid"].ToString() == "")
                        {
                            sales.tillid = "01";//?
                        }
                        else
                        {
                            sales.tillid = saleRow["posid"].ToString();//?
                        }
                        sales.txdocno = payRow["flow_no"].ToString();
                        sales.cashier = saleRow["oper_id"].ToString();
                        sales.vipcode = payRow["vip_no"].ToString();
                        sales.salesman = payRow["sale_man"].ToString();
                        string flow_no = payRow["flow_no"].ToString();
                        decimal net = list.Where(u => u.flow_no == flow_no).Count();

                        ArrayOfEsalestender enderList = new ArrayOfEsalestender();
                        ArrayOfEsalesitem item = new ArrayOfEsalesitem();

                        foreach (DataRow saleflowitem in saleflow.Rows)
                        {
                            esalestender esals = new esalestender();
                            esals.tendercode = "CH";
                            esals.payamount = Convert.ToInt32(payRow["pay_amount"]);
                            esals.baseamount = Convert.ToInt32(payRow["pay_amount"]);
                            esals.excessamount = 0;
                            esals.extendparam = "";
                            esals.remark = "";
                            enderList.Add(esals);

                            esalesitem items = new esalesitem();//销售单货品明细表 多
                            items.itemcode = ConfigurationManager.AppSettings["storecode"].ToString() + "1"; ; //"01L501N011";货号
                            items.bonusearn = 0;
                            items.discountamount = 0;
                            items.extendparam = "";
                            items.salesitemremark = "";
                            if (saleflowitem["sell_way"].ToString() == "A")
                            {
                                items.qty = Convert.ToInt32(saleflowitem["sale_qnty"]);
                                items.netamount = Convert.ToInt32(saleflowitem["salequt"]);
                            }
                            else if (saleflowitem["sell_way"].ToString() == "B")
                            {
                                items.qty = -Convert.ToInt32(saleflowitem["sale_qnty"]);
                                items.netamount = -Convert.ToInt32(saleflowitem["salequt"]);
                            }
                            item.Add(items);

                        }
                        if (saleRow["sell_way"].ToString() == "A")
                        {
                            //总数量
                            sales.netqty = net;
                            //总金额 
                            sales.netamount = Convert.ToInt32(payRow["pay_amount"]);
                        }
                        else if (saleRow["sell_way"].ToString() == "B")
                        {
                            sales.netqty = net;
                            sales.netamount = -Convert.ToInt32(saleRow["sale_money"]);
                        }

                        sales.extendparam = "";

                        postesalescreaterequest postsale = new postesalescreaterequest();
                        postsale.header = header;
                        postsale.esalesitems = item;
                        postsale.esalestenders = enderList;
                        postsale.esalestotal = sales;

                        postesalescreateRequest1Body body = new postesalescreateRequest1Body();
                        body.astr_request = postsale;

                        postesalescreateRequest1 request = new postesalescreateRequest1();
                        request.Body = body;

                        salesSoapClient salesCreate = new salesSoapClient();

                        try
                        {
                            postesalescreateresponse respone = new postesalescreateresponse();
                            respone = salesCreate.postesalescreate(postsale);

                            short code = respone.header.responsecode;
                            string str = respone.header.responsemessage;

                            if (code == 0 || code == 1000)
                            {
                                int num = 0;

                                foreach (DataRow payfRows in payflow.Rows)
                                {
                                    if (Convert.ToInt32(payfRows["com_no"]) > num)
                                    {
                                        num = Convert.ToInt32(payfRows["com_no"]);
                                    }
                                    Configuration com_no = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                                    com_no.AppSettings.Settings["com_no"].Value = num.ToString();
                                    com_no.Save();

                                }
                            }
                            if (code != 1000)
                            {
                                //保存日志
                                Save(DateTime.Now.ToString("yyyyMMdd") + "Log.txt", code.ToString(), str, flow_no);
                            }
                        }
                        catch (Exception ex)
                        {
                            //判断是否是网络问题 是休息一分钟后继续执行
                            if (ex.ToString().Contains("没有终结点在侦听可以接受消息"))
                            {
                                Thread.Sleep(60000);//一分钟
                                Send(sender, e);
                            }
                        }
                    }
                }
            }
            Infomation(sender, e);
           
            dispatcherTimer.Start();
        }
        //获取数据
        public DataTable Obtain(string sql)
        {
            string address = ConfigurationManager.AppSettings["fwqdz"];
            string catalog = ConfigurationManager.AppSettings["sjkmc"];
            string uid = ConfigurationManager.AppSettings["sjkyhm"];
            string pwd = ConfigurationManager.AppSettings["sjkmm"];


            string con = "data source=" + address + ";initial catalog=" + catalog + ";uid=" + uid + ";pwd=" + pwd + ";";
            DataTable dt = new DataTable();

            using (SqlConnection mycon = new SqlConnection(con))
            {
                try
                {
                    mycon.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, mycon);
                    adapter.Fill(dt);
                    Result = true;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return dt;
        }
        //保存记录
        public void Save(string path, string code, string str, string flowNo)
        {
            using (var fs = new FileStream(path, FileMode.Append))
            using (StreamWriter streamWriter = new StreamWriter(fs))
            {
                streamWriter.Write("\r\n");//换行
                if (code == "0")
                {
                    streamWriter.Write("销售单号：" + flowNo + "   传送时间：" + DateTime.Now.ToString("hh:mm:ss")+ "   传送成功");
                }
                else
                {
                    streamWriter.Write("销售单号：" + flowNo + "   传送时间：" + DateTime.Now.ToString("hh:mm:ss") + "   传送失败");
                }
                streamWriter.Write("\r\n");//换行

                //关闭此文件
            }
        }

        public void Infomation(object sender, EventArgs e)
        {
            string textFile = DateTime.Now.ToString("yyyyMMdd") + "Log.txt";
            FileStream fs;
            if (File.Exists(textFile))
            {
                fs = new FileStream(textFile, FileMode.Open, FileAccess.Read);
                using (fs)
                {
                    using (fs)
                    {
                        int fsLen = (int)fs.Length;
                        byte[] heByte = new byte[fsLen];
                        int r = fs.Read(heByte, 0, heByte.Length);
                        string myStr = System.Text.Encoding.UTF8.GetString(heByte);
                        SuccessInfomation.Text = myStr;
                        SuccessInfomation.ScrollToEnd();

                    }
                }
            }
        }

        //datetable 转list
        public static List<SalesModel> ConvertToModel(DataTable dt)
        {
            List<SalesModel> salesList = new List<SalesModel>();// 定义集合
            Type type = typeof(SalesModel); // 获得此模型的类型
            string tempName = "";
            foreach (DataRow dr in dt.Rows)
            {
                SalesModel smodel = new SalesModel();
                PropertyInfo[] propertys = smodel.GetType().GetProperties();// 获得此模型的公共属性
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;
                    if (dt.Columns.Contains(tempName))
                    {
                        if (!pi.CanWrite) continue;
                        object value = dr[tempName];
                        if (value != DBNull.Value)
                            pi.SetValue(smodel, value, null);
                    }
                }
                salesList.Add(smodel);
            }
            return salesList;
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

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = true;
            settingtButton.IsEnabled = true;
            Prompt.Content = "请点击启动导入数据";
            stopButton.IsEnabled = false;
            dispatcherTimer.Stop();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Configuration;
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

            sb.Append(ConfigurationManager.AppSettings["error"]);
            if (sb.Length > 0 )
            {
                strS = sb.ToString().Remove(sb.ToString().LastIndexOf(","), 1);
            }
            else
            {
                dispatcherTimer.Start();

                return;
            }
            //查询流水表 用来赋值
            DataTable saleflow = Obtain("select posid, flow_no,sell_way, CASE a.sell_way WHEN 'B' THEN -sale_qnty ELSE sale_qnty END sale_qnty, CONVERT(varchar(100), oper_date, 112) as oper_day,REPLACE(CONVERT(varchar(100),oper_date, 8),':','') as oper_time,oper_id,"
                                        + "CASE a.sell_way WHEN 'B' THEN - sale_qnty ELSE sale_qnty END sale_qnty,"
                                        + "CASE A.sell_way WHEN 'B' THEN - sale_money ELSE sale_money END sale_money,"
                                        + "item_no,(source_price - sale_price) * sale_qnty as salequt,"
                                        + "(select sum(CASE sell_way WHEN 'B' THEN - sale_money ELSE sale_money END) from t_rm_saleflow where flow_no in (a.flow_no)) as sale_moneyCount,"
                                        + "(select sum(CASE sell_way WHEN 'B' THEN - sale_qnty ELSE sale_qnty END) from t_rm_saleflow where flow_no in (a.flow_no)) as sale_qntyCount"
                                        + " FROM t_rm_saleflow  a"
                                        + " WHERE flow_no  in (" + strS + ") ");
            if (saleflow == null)
            {
                dispatcherTimer.Start();

                return;
            }

            List<SalesModel> list = new List<SalesModel>();

            list = ConvertToModel(saleflow);

            foreach (DataRow payRow in payflow.Rows)
            {
                string oper_id = "";
                string oper_day = "";
                string oper_time = "";
                string posid = "";
                decimal sale_moneyCount = 0;
                decimal sale_qnty = 0;
                decimal sale_money = 0;

                string sell_way = "";
                decimal sale_qntyCount = 0;
                decimal salequt = 0;
                ArrayOfEsalesitem item = new ArrayOfEsalesitem();

                foreach (DataRow saleRow in saleflow.Rows)
                {
                    if (payRow["flow_no"].ToString() == saleRow["flow_no"].ToString() && saleRow["sell_way"].ToString() != "C")
                    {
                        oper_id = saleRow["oper_id"].ToString();
                        oper_day = saleRow["oper_day"].ToString();
                        oper_time = saleRow["oper_time"].ToString();
                        posid = saleRow["posid"].ToString();
                        sale_moneyCount = Convert.ToDecimal(saleRow["sale_moneyCount"]);
                        sale_qnty = Convert.ToDecimal(saleRow["sale_qnty"]);
                        sale_money = Convert.ToDecimal(saleRow["sale_money"]);
                        sell_way = saleRow["sell_way"].ToString();
                        sale_qntyCount = Convert.ToInt32(saleRow["sale_qntyCount"]);
                        salequt = Convert.ToDecimal(saleRow["salequt"]);

                        esalesitem items = new esalesitem();//销售单货品明细表 多
                        items.itemcode = ConfigurationManager.AppSettings["storecode"].ToString() + "1"; ; //"01L501N011";货号
                        items.lineno = Convert.ToInt32(ConfigurationManager.AppSettings["lineno"]) + 1;
                        items.bonusearn = 0;
                        items.discountamount = salequt;
                        items.extendparam = "";
                        items.salesitemremark = "";

                        //数量
                        items.qty = sale_qnty;
                        //单货品金额
                        items.netamount = sale_money;

                        using (var fs = new FileStream(DateTime.Now.ToString("yyyyMMdd") + "DetailData.txt", FileMode.Append))
                        using (StreamWriter streamWriter = new StreamWriter(fs))
                        {
                            streamWriter.Write("\r\n");//换行

                            streamWriter.Write("行号：" + items.lineno + ",商品编号：" + items.itemcode + ",数量：" + items.qty + ",折扣金额：" + items.discountamount + ",净金额:" + items.netamount + ",积分：" + items.bonusearn + ",销售单号：" + payRow["flow_no"].ToString());
                            streamWriter.Write("\r\n");
                            streamWriter.Write("\r\n");//换行
                                                       //关闭此文件
                        }
                        Configuration lineno = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        lineno.AppSettings.Settings["lineno"].Value = items.lineno.ToString();
                        lineno.Save();
                        ConfigurationManager.RefreshSection("appSettings");

                        item.Add(items);
                    }

                }
                //销售
                esaleshdr sales = new esaleshdr();

                sales.mallid = ConfigurationManager.AppSettings["mallid"];//商场编号
                sales.txdate_yyyymmdd = oper_day;

                sales.txtime_hhmmss = oper_time;
                sales.storecode = ConfigurationManager.AppSettings["storecode"].ToString();
                if (posid.ToString() == "")
                {
                    sales.tillid = "01";//?
                }
                else
                {
                    sales.tillid = posid;//?
                }
                sales.txdocno = payRow["flow_no"].ToString();//单号
                sales.cashier = oper_id;
                sales.vipcode = payRow["vip_no"].ToString();
                sales.salesman = payRow["sale_man"].ToString();
                string flow_no = payRow["flow_no"].ToString();

                ArrayOfEsalestender enderList = new ArrayOfEsalestender();
                //付款信息
                esalestender esals = new esalestender();
                esals.lineno = Convert.ToInt32(ConfigurationManager.AppSettings["lineno1"]) + 1;
                esals.tendercode = "CH";
                //支付金额
                esals.payamount = sale_moneyCount;
                esals.baseamount = sale_moneyCount;


                esals.excessamount = 0;
                esals.extendparam = "";
                esals.remark = "";
                enderList.Add(esals);
                Configuration lineno1 = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                lineno1.AppSettings.Settings["lineno1"].Value = esals.lineno.ToString();
                lineno1.Save();
                decimal qty = 0;

                //净数量
                sales.netqty = sale_qntyCount;
                //销售总额
                sales.netamount = sale_moneyCount;

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

                //string endpointConfigurationName = GetEndpointAddress("salesSoap");
                string remoteAddress = ConfigurationManager.AppSettings["address"].ToString();
                salesSoapClient salesCreate = new salesSoapClient("salesSoap", remoteAddress);

                try
                {
                    postesalescreateresponse respone = new postesalescreateresponse();
                    respone = salesCreate.postesalescreate(postsale);

                    //返回码
                    short code = respone.header.responsecode;
                    //错误信息
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

                    //保存日志
                    Save(DateTime.Now.ToString("yyyyMMdd") + "Log.txt", code.ToString(), str, flow_no);
                    using (var fs = new FileStream(DateTime.Now.ToString("yyyyMMdd") + "Data.txt", FileMode.Append))
                    using (StreamWriter streamWriter = new StreamWriter(fs))
                    {
                        streamWriter.Write("\r\n");//换行

                        streamWriter.Write("商场编号：" + sales.mallid + "   交易日期：" + sales.txdate_yyyymmdd + "   交易时间：" + sales.txtime_hhmmss + "   店铺号：" + sales.storecode + "   收银机号：" + sales.tillid + "   销售单号：" + sales.txdocno + "   收银员编号：" + sales.cashier + "   VIP卡号：" + sales.vipcode + "   销售员：" + sales.salesman + "   付款金额：" + sale_moneyCount + "   货号：" + ConfigurationManager.AppSettings["storecode"].ToString() + "1" + "   数量：" + sale_qntyCount + "   折扣金额：0");
                        streamWriter.Write("\r\n");
                        streamWriter.Write("\r\n");//换行

                        //关闭此文件
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
                    throw ex;
                }
                item.Clear();
                oper_id = "";
                oper_day = "";
                oper_time = "";
                posid = "";
                sale_moneyCount = 0;
                sale_qnty = 0;
                sale_money = 0;

                sell_way = "";
                sale_qntyCount = 0;
                salequt = 0;
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
                    streamWriter.Write("销售单号：" + flowNo + "   传送时间：" + DateTime.Now.ToString("HH:mm:ss") + "   传送成功");
                }
                else 
                {
                    if (code != "1000")
                    {
                        Configuration error = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        error.AppSettings.Settings["error"].Value += flowNo + ",";
                        error.Save();
                        ConfigurationManager.RefreshSection("appSettings");
                    }
                    streamWriter.Write("销售单号：" + flowNo + "   传送时间：" + DateTime.Now.ToString("HH:mm:ss") + "   传送失败" + "返回Code" + code + "错误信息" + str);
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
           /// <summary>
          /// 读取EndpointAddress
          /// </summary>
          /// <param name="endpointName"></param>
          /// <returns></returns>
        private string GetEndpointAddress(string endpointName)
        {
            ClientSection clientSection = ConfigurationManager.GetSection("system.serviceModel/client") as ClientSection;
            foreach (ChannelEndpointElement item in clientSection.Endpoints)
            {
                if (item.Name == endpointName)
                    return item.Address.ToString();
            }
            return string.Empty;
        }
    }
}

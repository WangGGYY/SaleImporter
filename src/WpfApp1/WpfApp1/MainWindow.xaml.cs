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

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Send(sender, e);
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(Send);

            int fen = Convert.ToInt32(ConfigurationManager.AppSettings["minute"]);
            dispatcherTimer.Interval = new TimeSpan(0, 0, fen); //两分钟
            dispatcherTimer.Start();
        }

        private void Send(object sender, EventArgs e)
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
            Configuration mallidM= ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
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
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["com_no"]))
            {
                Configuration com_no = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                com_no.AppSettings.Settings["com_no"].Value = "0";
                com_no.Save();
            }
            ConfigurationManager.RefreshSection("appSettings");
            DataTable payflow = Obtain("select t_rm_payflow.pay_way,sell_way,com_no,t_rm_payflow.flow_no,t_rm_payflow.sale_man,t_rm_payflow.vip_no,t_rm_payflow.pay_amount  from  t_rm_payflow where com_no > " + ConfigurationManager.AppSettings["com_no"]);
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
                return;
            }
            //查询流水表
            DataTable saleflow = Obtain("select posid, flow_no,sell_way,sale_qnty, CONVERT(varchar(100), t_rm_saleflow.oper_date, 112) as oper_day,Datename(hour,t_rm_saleflow.oper_date)+Datename(minute,t_rm_saleflow.oper_date)+Datename(second,t_rm_saleflow.oper_date) as oper_time, t_rm_saleflow.oper_id, t_rm_saleflow.sale_qnty, t_rm_saleflow.sale_money,t_rm_saleflow.item_no, (source_price - sale_price) * sale_qnty as salequt,t_rm_saleflow.sale_money from t_rm_saleflow where flow_no in (" + strS + ") ");

            List<SalesModel> list = new List<SalesModel>();

            list = ConvertToModel(saleflow);

            foreach (DataRow payRow in payflow.Rows)
            {
                foreach (DataRow saleRow in saleflow.Rows)
                {
                    if (payRow["flow_no"].ToString() == saleRow["flow_no"].ToString() && saleRow["sell_way"].ToString() != "C")
                    {
                        esaleshdr sales = new esaleshdr();
                        esalesitem items = new esalesitem();
                        sales.mallid = scbh.Text;//商场编号
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

                        if (saleRow["sell_way"].ToString() == "A")
                        {
                            //总数量
                            sales.netqty = net;
                            //总金额 
                            sales.netamount = Convert.ToInt32(payRow["pay_amount"]);
                            items.qty = Convert.ToInt32(saleRow["sale_qnty"]);
                            items.netamount = Convert.ToInt32(saleRow["salequt"]);
                        }
                        else if (saleRow["sell_way"].ToString() == "B")
                        {
                            sales.netqty = net;
                            sales.netamount = -Convert.ToInt32(saleRow["sale_money"]);
                            items.qty = -Convert.ToInt32(saleRow["sale_qnty"]);
                            items.netamount = -Convert.ToInt32(saleRow["salequt"]);
                        }

                        sales.extendparam = "";
                        items.itemcode = ConfigurationManager.AppSettings["storecode"].ToString()+"1"; ; //"01L501N011";货号
                        items.bonusearn = 0;
                        items.discountamount = 0;
                        items.extendparam = "";
                        items.salesitemremark = "";

                        ArrayOfEsalesitem item = new ArrayOfEsalesitem();
                        item.Add(items);

                        esalestender esals = new esalestender();
                        esals.tendercode = "CH";
                        esals.payamount = Convert.ToInt32(payRow["pay_amount"]);
                        esals.baseamount = Convert.ToInt32(payRow["pay_amount"]);
                        esals.excessamount = 0;
                        esals.extendparam = "";
                        esals.remark = "";
                        ArrayOfEsalestender enderList = new ArrayOfEsalestender();
                        enderList.Add(esals);

                        postesalescreaterequest postsale = new postesalescreaterequest();
                        postsale.header = header;
                        postsale.esalesitems = item;
                        postsale.esalestenders = enderList;
                        postsale.esalestotal = sales;
                        //postsale.ExtensionData = sales;


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

                            if (code != 0)
                            { //保存日志
                                Save(@"log.txt", code.ToString(), str, saleRow["flow_no"].ToString());
                            }
                            else
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
                                //保存日志
                                Save(@"daochu.txt", code.ToString(), str, saleRow["flow_no"].ToString());
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

        }
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
        //获取数据
        public DataTable Obtain(string sql)
        {
            string address = ConfigurationManager.AppSettings["fwqdz"];
            string catalog = ConfigurationManager.AppSettings["sjkmc"];
            string uid = ConfigurationManager.AppSettings["sjkyhm"];
            string pwd = ConfigurationManager.AppSettings["sjkmm"];


            string con =  "data source="+address+";initial catalog="+catalog+";uid="+ uid + ";pwd="+pwd+";";

            SqlConnection mycon = new SqlConnection(con);
            mycon.Open();

            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(sql, mycon);
            adapter.Fill(dt);

            return dt; 
        }

        public void Save(string path, string code, string str, string flowNo)
        {
            using (var fs = new FileStream(path, FileMode.Append))
            using (StreamWriter streamWriter = new StreamWriter(fs))
            {
                streamWriter.Write("\r\n");//换行
                //写入内容
                streamWriter.Write("Code:" + code);
                streamWriter.Write("\r\n");//换行
                streamWriter.Write("信息：" + str);
                streamWriter.Write("\r\n");//换行
                streamWriter.Write("flow_no:" + flowNo);
                //关闭此文件
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DNSProject
{
    public partial class Form1 : Form
    {
        //connect directly to NameServers
        private void GenerateConToNameServer(string ipAddr)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(ipAddr);
            stringBuilder.Append(":53053");
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(IPEndPoint.Parse(stringBuilder.ToString()));
            Byte[] sendData = Encoding.ASCII.GetBytes("Test");
            udpClient.Send(sendData, sendData.Length);

        }
        //Connect to resolver
        private void GenerateConToResolver(string hostName)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(IPEndPoint.Parse("127.0.0.10:53053"));


            Dns dnsPacket = new Dns(new Flags(0, 1, 0, 0), new Qry(hostName, 1), new Count(0), "127.0.0.10", "", new Resp(250,""), new Srv("", 53053, "UDP", "", "localhost"));
            string jSONresult = JsonConvert.SerializeObject(dnsPacket);
            Byte[] sendData = Encoding.ASCII.GetBytes(jSONresult);

            Thread senderThread = new Thread(() =>
            {
                //highlight button
                Form1.ActiveForm.Controls["button35"].BackColor = Color.LightBlue;
                Thread.Sleep(350);
                //stop highlight button
                Form1.ActiveForm.Controls["button35"].BackColor = Color.FromArgb(0, 44, 43, 60);
                udpClient.Send(sendData, sendData.Length);
            });
            senderThread.Start();

            Thread answerThread = new Thread(() =>
            {
                Debug.WriteLine("Application is listening for response");
                UdpClient answerListender = new UdpClient(IPEndPoint.Parse("127.0.0.1:51337"));
                IPEndPoint resolverEndpoint = IPEndPoint.Parse("127.0.0.10:53443");
                answerListender.Receive(ref resolverEndpoint);
                Debug.WriteLine("Application reicived Response and is ready to connect to a service");
                //highlight button
                Form1.ActiveForm.Controls["button35"].BackColor = Color.LightBlue;
                Thread.Sleep(350);
                //stop highlight button
                Form1.ActiveForm.Controls["button35"].BackColor = Color.FromArgb(0, 44, 43, 60);
                answerListender.Close();

            });
            answerThread.Start();

        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.Hide();
            //create directory 4 logs
            CreateDirectory();


            //initResolver
            Resolver resolver = new Resolver();

            Thread resolverThread = new Thread(() =>
            {
                resolver.Listen();
            });
            resolverThread.Start();

            //initDnsServers
            List<ServerBase> dnsServerList = new List<ServerBase>();
            ServerBase root = new ServerBase("root", "127.0.0.11", 53053, false);
            root.AddDictEntry(new Tuple<string, int>(".telematik",1), new Tuple<string, int>("127.0.0.97",250));
            root.AddDictEntry(new Tuple<string, int>(".telematik", 2), new Tuple<string, int>("ns.telematik", 250));
            root.AddDictEntry(new Tuple<string, int>(".fuberlin", 1), new Tuple<string, int>("127.0.0.94", 250));
            root.AddDictEntry(new Tuple<string, int>(".fuberlin", 2), new Tuple<string, int>("ns.fuberlin", 250));

            ServerBase telematik = new ServerBase("telematik", "127.0.0.97", 53053, false);
            telematik.AddDictEntry(new Tuple<string, int>(".switch.telematik", 1), new Tuple<string, int>("127.0.0.98", 250));
            telematik.AddDictEntry(new Tuple<string, int>(".switch.telematik", 2), new Tuple<string, int>("ns.switch.telematik", 250));
            telematik.AddDictEntry(new Tuple<string, int>(".router.telematik", 1), new Tuple<string, int>("127.0.0.99", 250));
            telematik.AddDictEntry(new Tuple<string, int>(".router.telematik", 2), new Tuple<string, int>("ns.router.telematik", 250));

            ServerBase fuberlin = new ServerBase("fuberlin", "127.0.0.94", 53053, false);
            fuberlin.AddDictEntry(new Tuple<string, int>(".homework.fuberlin", 1), new Tuple<string, int>("127.0.0.95", 250));
            fuberlin.AddDictEntry(new Tuple<string, int>(".homework.fuberlin", 2), new Tuple<string, int>("ns.homework.fuberlin", 250));
            fuberlin.AddDictEntry(new Tuple<string, int>(".pcpools.fuberlin", 1), new Tuple<string, int>("127.0.0.96", 250));
            fuberlin.AddDictEntry(new Tuple<string, int>(".pcpools.fuberlin", 2), new Tuple<string, int>("ns.pcpools.fuberlin", 250));

            ServerBase switchTelematik = new ServerBase("switch.telematik", "127.0.0.98", 53053, true);
            switchTelematik.AddDictEntry(new Tuple<string, int>("mail.switch.telematik", 1), new Tuple<string, int>("127.0.0.100", 250));
            switchTelematik.AddDictEntry(new Tuple<string, int>("mail.switch.telematik", 2), new Tuple<string, int>("ns.mail.switch.telematik", 250));
            switchTelematik.AddDictEntry(new Tuple<string, int>("www.switch.telematik", 1), new Tuple<string, int>("127.0.0.101", 250));
            switchTelematik.AddDictEntry(new Tuple<string, int>("www.switch.telematik", 2), new Tuple<string, int>("ns.www.switch.telematik", 250));

            ServerBase routerTelematik = new ServerBase("router.telematik", "127.0.0.99", 53053, true);
            routerTelematik.AddDictEntry(new Tuple<string, int>("shop.router.telematik", 1), new Tuple<string, int>("127.0.0.102", 250));
            routerTelematik.AddDictEntry(new Tuple<string, int>("shop.router.telematik", 2), new Tuple<string, int>("ns.shop.router.telematik", 250));
            routerTelematik.AddDictEntry(new Tuple<string, int>("news.router.telematik", 1), new Tuple<string, int>("127.0.0.103", 250));
            routerTelematik.AddDictEntry(new Tuple<string, int>("news.router.telematik", 2), new Tuple<string, int>("ns.news.router.telematik", 250));

            ServerBase homeworkFuberlin = new ServerBase("homework.fuberlin", "127.0.0.95", 53053, true);
            homeworkFuberlin.AddDictEntry(new Tuple<string, int>("easy.homework.fuberlin", 1), new Tuple<string, int>("127.0.0.104", 250));
            homeworkFuberlin.AddDictEntry(new Tuple<string, int>("easy.homework.fuberlin", 2), new Tuple<string, int>("ns.easy.homework.fuberlin", 250));
            homeworkFuberlin.AddDictEntry(new Tuple<string, int>("hard.homework.fuberlin", 1), new Tuple<string, int>("127.0.0.105", 250));
            homeworkFuberlin.AddDictEntry(new Tuple<string, int>("hard.homework.fuberlin", 2), new Tuple<string, int>("ns.hard.homework.fuberlin", 250));

            ServerBase pcpoolsFuberlin = new ServerBase("pcpools.fuberlin", "127.0.0.96", 53053, true);
            pcpoolsFuberlin.AddDictEntry(new Tuple<string, int>("windows.pcpools.fuberlin", 1), new Tuple<string, int>("127.0.0.106", 250));
            pcpoolsFuberlin.AddDictEntry(new Tuple<string, int>("windows.pcpools.fuberlin", 2), new Tuple<string, int>("ns.windows.pcpools.fuberlin", 250));
            pcpoolsFuberlin.AddDictEntry(new Tuple<string, int>("macos.pcpools.fuberlin", 1), new Tuple<string, int>("127.0.0.107", 250));
            pcpoolsFuberlin.AddDictEntry(new Tuple<string, int>("macos.pcpools.fuberlin", 2), new Tuple<string, int>("ns.macos.pcpools.fuberlin", 250));
            pcpoolsFuberlin.AddDictEntry(new Tuple<string, int>("linux.pcpools.fuberlin", 1), new Tuple<string, int>("127.0.0.108", 250));
            pcpoolsFuberlin.AddDictEntry(new Tuple<string, int>("linux.pcpools.fuberlin", 2), new Tuple<string, int>("ns.linux.pcpools.fuberlin", 250));


            //Endpoints -- provide services!
            ServerBase mailSwitchTelematik = new ServerBase("mail.switch.telematik", "127.0.0.100", 53053, false);
            ServerBase wwwSwitchTelematik = new ServerBase("www.switch.telematik", "127.0.0.101", 53053, false);
            ServerBase shopRouterTelematik = new ServerBase("shop.router.telematik", "127.0.0.102", 53053, false);
            ServerBase newsRouterTelematik = new ServerBase("news.router.telematik", "127.0.0.103", 53053, false);
            ServerBase easyHomeworkFuberlin = new ServerBase("easy.homework.fuberlin", "127.0.0.104", 53053, false);
            ServerBase hardHomeworkFuberlin = new ServerBase("hard.homework.fuberlin", "127.0.0.105", 53053, false);
            ServerBase windowsPcpoolsFuberlin = new ServerBase("windows.pcpools.fuberlin", "127.0.0.106", 53053, false);
            ServerBase macosPcpoolsFuberlin = new ServerBase("macos.pcpools.fuberlin", "127.0.0.107", 53053, false);
            ServerBase linuxPcpoolsFuberlin = new ServerBase("linux.pcpools.fuberlin", "127.0.0.108", 53053, false);

            //rootServer
            dnsServerList.Add(root);
            //nameServerOne
            dnsServerList.Add(telematik);
            dnsServerList.Add(fuberlin);
            //nameServersTwo
            dnsServerList.Add(switchTelematik);
            dnsServerList.Add(routerTelematik);
            dnsServerList.Add(homeworkFuberlin);
            dnsServerList.Add(pcpoolsFuberlin);
            //Services .. dont get confused by the name theese do not resolve names, they have open ports on 53053 only cause its less code req for init Servers but theese could run many services on different ports
            dnsServerList.Add(mailSwitchTelematik);
            dnsServerList.Add(wwwSwitchTelematik);
            dnsServerList.Add(shopRouterTelematik);
            dnsServerList.Add(newsRouterTelematik);
            dnsServerList.Add(easyHomeworkFuberlin);
            dnsServerList.Add(hardHomeworkFuberlin);
            dnsServerList.Add(windowsPcpoolsFuberlin);
            dnsServerList.Add(macosPcpoolsFuberlin);
            dnsServerList.Add(linuxPcpoolsFuberlin);

            foreach(ServerBase server in dnsServerList)
            {
                Thread dnsListener = new Thread(() =>
                {
                    server.Listen();
                });
                dnsListener.Start();
            }
        }
        private int CreateDirectory()
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "MyLogs")){
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "MyLogs");
                //if u have to use sudo uncomment the following 4 lines of code.
                //DirectoryInfo logDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "MyLogs");
                //DirectorySecurity securityRules = new DirectorySecurity();
                //securityRules.AddAccessRule(new FileSystemAccessRule("Users", FileSystemRights.Modify, AccessControlType.Allow));
                //logDirectory.SetAccessControl(securityRules);

                return 1;
            }
            else return -1;
        }
        public void CloseAllSockects()
        {
            //TODO close all sockets gracefully
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            
        }
        public void readlog()
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            StringBuilder pathBuilder = new StringBuilder();
            string logString ="";
            //some Quick and dirty stuff, I dont do windows forms frontend alot :*C
            switch (sender.ToString())
            {
                case "System.Windows.Forms.Button, Text: Controller":
                    richTextBox1.Hide();
                    break;
                case "System.Windows.Forms.Button, Text: Resolver":
                    richTextBox1.Show();
                    pathBuilder.Append(AppDomain.CurrentDomain.BaseDirectory);
                    pathBuilder.Append("MyLogs");
                    pathBuilder.Append("\\");
                    pathBuilder.Append("127.0.0.10");
                    pathBuilder.Append(".txt");
                    logString = File.ReadAllText(pathBuilder.ToString());
                    richTextBox1.Text = logString;
                    break;
                case "System.Windows.Forms.Button, Text: DNS\r\nroot":
                    richTextBox1.Show();
                    pathBuilder.Append(AppDomain.CurrentDomain.BaseDirectory);
                    pathBuilder.Append("MyLogs");
                    pathBuilder.Append("\\");
                    pathBuilder.Append("127.0.0.11");
                    pathBuilder.Append(".txt");
                    logString = File.ReadAllText(pathBuilder.ToString());
                    richTextBox1.Text = logString;
                    break;
                case "System.Windows.Forms.Button, Text: DNS\r\ntelematik":
                    richTextBox1.Show();
                    pathBuilder.Append(AppDomain.CurrentDomain.BaseDirectory);
                    pathBuilder.Append("MyLogs");
                    pathBuilder.Append("\\");
                    pathBuilder.Append("127.0.0.97");
                    pathBuilder.Append(".txt");
                    logString = File.ReadAllText(pathBuilder.ToString());
                    richTextBox1.Text = logString;
                    break;
                case "System.Windows.Forms.Button, Text: DNS\r\nfuberlin":
                    richTextBox1.Show();
                    pathBuilder.Append(AppDomain.CurrentDomain.BaseDirectory);
                    pathBuilder.Append("MyLogs");
                    pathBuilder.Append("\\");
                    pathBuilder.Append("127.0.0.94");
                    pathBuilder.Append(".txt");
                    logString = File.ReadAllText(pathBuilder.ToString());
                    richTextBox1.Text = logString;
                    break;
                case "System.Windows.Forms.Button, Text: DNS\r\nswitch.telematik":
                    richTextBox1.Show();
                    pathBuilder.Append(AppDomain.CurrentDomain.BaseDirectory);
                    pathBuilder.Append("MyLogs");
                    pathBuilder.Append("\\");
                    pathBuilder.Append("127.0.0.98");
                    pathBuilder.Append(".txt");
                    logString = File.ReadAllText(pathBuilder.ToString());
                    richTextBox1.Text = logString;
                    break;
                case "System.Windows.Forms.Button, Text: DNS\r\nrouter.telematik":
                    richTextBox1.Show();
                    pathBuilder.Append(AppDomain.CurrentDomain.BaseDirectory);
                    pathBuilder.Append("MyLogs");
                    pathBuilder.Append("\\");
                    pathBuilder.Append("127.0.0.99");
                    pathBuilder.Append(".txt");
                    logString = File.ReadAllText(pathBuilder.ToString());
                    richTextBox1.Text = logString;
                    break;
                case "System.Windows.Forms.Button, Text: DNS\r\nhomework.fuberlin":
                    richTextBox1.Show();
                    pathBuilder.Append(AppDomain.CurrentDomain.BaseDirectory);
                    pathBuilder.Append("MyLogs");
                    pathBuilder.Append("\\");
                    pathBuilder.Append("127.0.0.95");
                    pathBuilder.Append(".txt");
                    logString = File.ReadAllText(pathBuilder.ToString());
                    richTextBox1.Text = logString;
                    break;
                case "System.Windows.Forms.Button, Text: DNS\r\npcpools.fuberlin":
                    richTextBox1.Show();
                    pathBuilder.Append(AppDomain.CurrentDomain.BaseDirectory);
                    pathBuilder.Append("MyLogs");
                    pathBuilder.Append("\\");
                    pathBuilder.Append("127.0.0.96");
                    pathBuilder.Append(".txt");
                    logString = File.ReadAllText(pathBuilder.ToString());
                    richTextBox1.Text = logString;
                    break;
                case "System.Windows.Forms.Button, Text: Service Provider\r\nmail.switch.telematik":
                    richTextBox1.Text = (logString == "") ? "No Services Found" : logString;
                    break;
                case "System.Windows.Forms.Button, Text: Service Provider\r\nwww.switch.telematik":
                    richTextBox1.Text = (logString == "") ? "No Services Found" : logString;
                    break;
                case "System.Windows.Forms.Button, Text: Service Provider\r\nshop.router.telematik":
                    richTextBox1.Text = (logString == "") ? "No Services Found" : logString;
                    break;
                case "System.Windows.Forms.Button, Text: Service Provider\r\nnews.router.telematik":
                    richTextBox1.Text = (logString == "") ? "No Services Found" : logString;
                    break;
                case "System.Windows.Forms.Button, Text: Service Provider\r\neasy.homework.fuberlin":
                    richTextBox1.Text = (logString == "") ? "No Services Found" : logString;
                    break;
                case "System.Windows.Forms.Button, Text: Service Provider\r\nhard.homework.fuberlin":
                    richTextBox1.Text = (logString == "") ? "No Services Found" : logString;
                    break;
                case "System.Windows.Forms.Button, Text: Service Provider\r\nwindows.pcpools.fuberlin":
                    richTextBox1.Text = (logString == "") ? "No Services Found" : logString;
                    break;
                case "System.Windows.Forms.Button, Text: Service Provider\r\nlinux.pcpools.fuberlin":
                    richTextBox1.Text = (logString == "") ? "No Services Found" : logString;
                    break;
                case "System.Windows.Forms.Button, Text: Service Provider\r\nmacos.pcpools.fuberlin":
                    richTextBox1.Text = (logString == "") ? "No Services Found" : logString;
                    break;

            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            //router.telematik
            GenerateConToResolver(".router.telematik");
        }

        private void button26_Click(object sender, EventArgs e)
        {//www.switch.telematik
            GenerateConToResolver("www.switch.telematik");
        }

        private void button33_Click(object sender, EventArgs e)
        {
            //linux.pcpools.fuberlin
            GenerateConToResolver("linux.pcpools.fuberlin");
        }

        private void button25_Click(object sender, EventArgs e)
        {//mail.switch.telematik
            Debug.WriteLine("clicked on mail.switch.telematik");
            GenerateConToResolver("mail.switch.telematik");
        }


        private void button21_Click(object sender, EventArgs e)
        {//switch.telematik
            GenerateConToResolver(".switch.telematik");
        }

        private void button19_Click(object sender, EventArgs e)
        {//telematik
            GenerateConToResolver(".telematik");
        }

        private void button27_Click(object sender, EventArgs e)
        {
            //shop.router.telematik
            GenerateConToResolver("shop.router.telematik");
        }

        private void button28_Click(object sender, EventArgs e)
        {
            //news.router.telematik
            GenerateConToResolver("news.router.telematik");

        }

        private void button29_Click(object sender, EventArgs e)
        {
            //easy.homework.fuberlin
            GenerateConToResolver("easy.homework.fuberlin");
        }

        private void button30_Click(object sender, EventArgs e)
        {
            //hard.homework.fuberlin
            GenerateConToResolver("hard.homework.fuberlin");
        }

        private void button31_Click(object sender, EventArgs e)
        {
            //windows.pcpools.fuberlin
            GenerateConToResolver("windows.pcpools.fuberlin");
        }

        private void button32_Click(object sender, EventArgs e)
        {
            //macos.pcpools.fuberlin
            GenerateConToResolver("macos.pcpools.fuberlin");
        }

        private void button23_Click(object sender, EventArgs e)
        {
            //homework.fuberlin
            GenerateConToResolver(".homework.fuberlin");
        }

        private void button24_Click(object sender, EventArgs e)
        {
            //pcpools.fuberlin
            GenerateConToResolver(".pcpools.fuberlin");
        }

        private void button20_Click(object sender, EventArgs e)
        {
            //fuberlin
            GenerateConToResolver(".fuberlin");
        }

        private void root_btn_Click(object sender, EventArgs e)
        {
            //root
            GenerateConToResolver("root");
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button35_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        public void HighlightButton() => button35.BackColor = Color.Blue;
    }
}

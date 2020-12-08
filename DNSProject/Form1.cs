using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DNSProject
{
    public partial class Form1 : Form
    {
        //P2P
        private void GenerateCon(string ipAddr)
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

            //TODO use frontendcheckbox for query flags
            Dns dnsPacket = new Dns(new Flags(0, 1, 0, 0), new Qry(hostName, 1), new Count(0), "127.0.0.10", "", new Resp(250,""), new Srv("", 53053, "UDP", "", "localhost"));
            string jSONresult = JsonConvert.SerializeObject(dnsPacket);
            Byte[] sendData = Encoding.ASCII.GetBytes(jSONresult);

            //TODO shorten .Start, dont keep ref
            Thread senderThread = new Thread(() =>
            {
                udpClient.Send(sendData, sendData.Length);
            });
            senderThread.Start();
            
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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
            root.AddDictEntry(new Tuple<string, int>(".telematik",1), new Tuple<string, int>("127.0.0.97",99999));
            root.AddDictEntry(new Tuple<string, int>(".telematik", 2), new Tuple<string, int>("ns.telematik", 99999));
            //root.AddDictEntry(new Tuple<string, int>(".fuberlin", 1), new Tuple<string, int>("127.0.0.97", 99999));

            ServerBase telematik = new ServerBase("telematik", "127.0.0.97", 53053, false);
            telematik.AddDictEntry(new Tuple<string, int>(".switch.telematik", 1), new Tuple<string, int>("127.0.0.98", 99999));
            telematik.AddDictEntry(new Tuple<string, int>(".switch.telematik", 2), new Tuple<string, int>("ns.switch.telematik", 99999));

            ServerBase switchTelematik = new ServerBase("switch.telematik", "127.0.0.98", 53053, false);
            switchTelematik.AddDictEntry(new Tuple<string, int>("mail.switch.telematik", 1), new Tuple<string, int>("127.0.0.100", 99999));
            switchTelematik.AddDictEntry(new Tuple<string, int>("mail.switch.telematik", 2), new Tuple<string, int>("ns.mail.switch.telematik", 99999));

            ServerBase mailSwitchTelematik = new ServerBase("mail.switch.telematik", "127.0.0.100", 53053, false);

            //add to server list for less coding
            dnsServerList.Add(root);
            dnsServerList.Add(telematik);
            dnsServerList.Add(switchTelematik);
            dnsServerList.Add(mailSwitchTelematik);

            foreach(ServerBase server in dnsServerList)
            {
                Thread dnsListener = new Thread(() =>
                {
                    server.Listen();
                });
                dnsListener.Start();
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button22_Click(object sender, EventArgs e)
        {

        }

        private void button26_Click(object sender, EventArgs e)
        {//www.switch.telematik
            GenerateConToResolver("www.switch.telematik");
        }

        private void button33_Click(object sender, EventArgs e)
        {

        }

        private void button25_Click(object sender, EventArgs e)
        {//mail.switch.telematik
            Debug.WriteLine("clicked on mail.switch.telematik");
            GenerateConToResolver("mail.switch.telematik");
        }


        private void button21_Click(object sender, EventArgs e)
        {//switch.telematik
            GenerateConToResolver("switch.telematik");
        }

        private void button19_Click(object sender, EventArgs e)
        {//telematik
            GenerateConToResolver("telematik");
        }
    }
}

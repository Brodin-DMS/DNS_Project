using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DNSProject
{
    public class ServerBase
    {
        string name;
        IPAddress ipAddr;
        int port;
        IPEndPoint iPEndPoint;
        Dictionary<Tuple<string, int>, Tuple<string, int>> chache;
        UdpClient server;
        IPEndPoint resolver;
        bool isAuth;
        public static string baseLogsPath = AppDomain.CurrentDomain.BaseDirectory+ "MyLogs";
        //log Data
        int reqSend;
        int reqRcv;
        int respSend;
        int respRcv;

        public ServerBase(string name, string ipAddr, int port, bool isAuth)
        {
            this.name = name;
            this.ipAddr = IPAddress.Parse(ipAddr);
            this.port = port;
            this.chache = new Dictionary<Tuple<string, int>, Tuple<string, int>>();
            iPEndPoint = IPEndPoint.Parse(ipAddr+":53053");
            this.server = new UdpClient(iPEndPoint);
            resolver = IPEndPoint.Parse("127.0.0.10:53053");
            this.isAuth = isAuth;
            Debug.WriteLine("Server_" + name + " initialized");
            File.Create(baseLogsPath + "\\" + ipAddr.ToString()+".txt").Dispose();
            this.reqSend = 0;
            this.reqRcv = 0;
            this.respSend = 0;
            this.respRcv = 0;

        }
        
        public void AddDictEntry(Tuple<string,int> key, Tuple<string,int> value)
        {
            chache.Add(key, value);
        }
        void WriteLog()
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(baseLogsPath + "\\" + ipAddr.ToString() + ".txt", true))
            {
                StringBuilder logBuilder = new StringBuilder();
                logBuilder.Append(DateTime.Now);
                logBuilder.Append("|");
                logBuilder.Append(ipAddr.ToString());
                logBuilder.Append("|");
                logBuilder.Append(reqSend.ToString());
                logBuilder.Append("|");
                logBuilder.Append(reqRcv.ToString());
                logBuilder.Append("|");
                logBuilder.Append(respSend.ToString());
                logBuilder.Append("|");
                logBuilder.Append(respRcv);
                file.WriteLine(logBuilder.ToString());
            }
        }

        public void Listen()
        {
            while (true)
            {


                try
                {
                    IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, port);
                    //Debug.WriteLine("Server_" + name + " receiving");
                    Byte[] recBuffer = server.Receive(ref iPEndPoint);
                    string data = Encoding.ASCII.GetString(recBuffer);
                    Debug.WriteLine("Server_" + name + "received Data_" + data + " from: " + iPEndPoint);
                    Dns packet = JsonConvert.DeserializeObject<Dns>(data);
                    //handleLogData
                    reqRcv++;
                    WriteLog();
                    //process Data
                    ManipulateReq(packet);

                    Send(packet);

                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                finally
                {
                    //server.Close();
                }
            }
        }
        public string getButtonString()
        {
            string notYieldedName = "";
            switch (name)
            {
                case "root":
                    notYieldedName = "root_btn";
                    break;
                case "telematik":
                    notYieldedName = "button19";
                    break;
                case "fuberlin":
                    notYieldedName = "button20";
                    break;
                case "switch.telematik":
                    notYieldedName = "button21";
                    break;
                case "router.telematik":
                    notYieldedName = "button22";
                    break;
                case "homework.fuberlin":
                    notYieldedName = "button23";
                    break;
                case "pcpools.fuberlin":
                    notYieldedName = "button24";
                    break;
                case "mail.switch.telematik":
                    notYieldedName = "button25";
                    break;
                case "www.switch.telematik":
                    notYieldedName = "button26";
                    break;
                case "shop.router.telematik":
                    notYieldedName = "button27";
                    break;
                case "news.router.telematik":
                    notYieldedName = "button28";
                    break;
                case "easy.homework.fuberlin":
                    notYieldedName = "button29";
                    break;
                case "hard.homework.fuberlin":
                    notYieldedName = "button30";
                    break;
                case "windows.pcpools.fuberlin":
                    notYieldedName = "button31";
                    break;
                case "macos.pcpools.fuberlin":
                    notYieldedName = "button32";
                    break;
                case "linux.pcpools.fuberlin":
                    notYieldedName = "button33";
                    break;
            }
            return notYieldedName;
        }
        public void Send(Dns dnsPacket)
        {
            UdpClient senderClient = new UdpClient();
            Byte[] sendData = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(dnsPacket));

            Thread senderThread = new Thread(() =>
            {
                //highlight button
                Form1.ActiveForm.Controls[getButtonString()].BackColor = Color.LightBlue;
                Thread.Sleep(350);
                //stop highlight button
                Form1.ActiveForm.Controls[getButtonString()].BackColor = Color.FromArgb(0, 44, 43, 60);

                senderClient.Send(sendData, sendData.Length, "127.0.0.10", 53053);
                respSend++;
                WriteLog();
            });
            senderThread.Start();
        }
        public void ManipulateReq(Dns packet)
        {
            Debug.WriteLine("entering req manipulation!");
            foreach(Tuple<string,int> key in chache.Keys)
            {
                Debug.WriteLine("keyValue := " + key.Item1);
                Debug.WriteLine("qryName:= " + packet.qry.name);
                Debug.WriteLine("keyType:= " + key.Item2);
                Debug.WriteLine("qryType:= "+ packet.qry.type);
                if (packet.qry.name.Equals(key.Item1) && packet.qry.type == key.Item2){
                    Debug.WriteLine("req Section is equal");
                    //Positiv change req
                    packet.resp.nextIp = null;
                    packet.flags.authorative = isAuth ? 1 : 0;
                    packet.flags.response = 1;
                    if (packet.qry.type == 1)
                    {
                        packet.a = chache[key].Item1;
                        packet.count.rr_list = new List<Tuple<string, string>>();
                        packet.count.rr_list.Add(new Tuple<string, string>("dns.resp.type", packet.qry.type.ToString()));
                        packet.count.rr_list.Add(new Tuple<string, string>("dns.resp.ttl", packet.resp.ttl.ToString()));
                        packet.count.rr_list.Add(new Tuple<string, string>("dns.a", packet.a.ToString()));

                    }
                    if (packet.qry.type == 2)
                    {
                        packet.ns = chache[key].Item1;
                    }
                    return;
                }
                if (packet.qry.name.EndsWith(key.Item1)&&packet.qry.type ==key.Item2)
                {
                    Debug.WriteLine("req section ends with");
                    //Positiv change req
                    packet.resp.nextIp = chache[key].Item1;
                    packet.flags.authorative = isAuth ? 1 : 0;
                    packet.flags.response = 1;
                    if (packet.qry.type == 1)
                    {
                        packet.a = chache[key].Item1;
                    }
                    if (packet.qry.type == 2)
                    {
                        packet.ns = chache[key].Item1;
                    }
                    break;
                }
            }
        }
    }
}

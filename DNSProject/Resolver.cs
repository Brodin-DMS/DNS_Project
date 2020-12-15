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

namespace DNSProject
{

    class Resolver
    {
        IPEndPoint iPEndPoint;
        string name;
        Dictionary<Tuple<string,int>,Tuple<string,int>> chache;
        int port;
        UdpClient server;
        public static string baseLogsPath = AppDomain.CurrentDomain.BaseDirectory + "MyLogs";

        //log Data
        int reqSend;
        int reqRcv;
        int respSend;
        int respRcv;

        public Resolver()
        {
            this.iPEndPoint = IPEndPoint.Parse("127.0.0.10:53053");
            this.name = "MyDnsResolver";
            this.chache = new Dictionary<Tuple<string, int>, Tuple<string, int>>();
            chache.Add(new Tuple<string, int>("root",1), new Tuple<string, int>("127.0.0.11",99999));
            chache.Add(new Tuple<string, int>("root", 2), new Tuple<string, int>("ns", 99999));
            this.port = 53053;
            this.server = new UdpClient(iPEndPoint);
            Debug.WriteLine("Server_" + name + " initialized");
            File.Create(baseLogsPath + "\\" + "127.0.0.10" + ".txt").Dispose();
            //init LogData
            this.reqSend = 0;
            this.reqRcv = 0;
            this.respSend = 0;
            this.respRcv = 0;
            StartDicTtlCountdown();
        }
        public void StartDicTtlCountdown()
        {

            Thread ttlCountdown = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    Dictionary<Tuple<string, int>, Tuple<string, int>> tempDict = new Dictionary<Tuple<string, int>, Tuple<string, int>>();

                    foreach (Tuple<string, int> key in chache.Keys)
                    {
                        if (chache[key].Item2 <= 1)
                        {
                            continue;
                        }
                        int tempint = chache[key].Item2;
                        tempint--;
                        tempDict.Add(key, new Tuple<string, int>(chache[key].Item1, tempint));
                        
                    }
                    chache = tempDict;
                    //DEBUGGING -- displays chache every second. Enable the 4 following lines of code to Write chache to Debug
                    //foreach (Tuple<string, int> testvalues in chache.Values)
                    //{
                    //    Debug.WriteLine(testvalues.Item1 + " : " + testvalues.Item2);
                    //}
                }
            });
            ttlCountdown.Start();
        }
        private void GenerateCon(Dns packet)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(packet.resp.nextIp);
            stringBuilder.Append(":53053");
            UdpClient senderClient = new UdpClient();
            string JsonDns = JsonConvert.SerializeObject(packet);
            Byte[] sendData = Encoding.ASCII.GetBytes(JsonDns);

            Thread senderThread = new Thread(() =>
            {
                //highlight button
                Form1.ActiveForm.Controls["button34"].BackColor = Color.LightBlue;
                Thread.Sleep(350);
                //stop highlight button
                Form1.ActiveForm.Controls["button34"].BackColor = Color.FromArgb(0, 44, 43, 60);
                senderClient.Send(sendData, sendData.Length, packet.resp.nextIp,53053);
                reqSend++;
                WriteLog();
            });
            senderThread.Start();

        }
        public void Listen()
        {
            while (true)
            {

                try
                {
                    IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, port);
                    Byte[] recBuffer = server.Receive(ref iPEndPoint);
                    string data = Encoding.ASCII.GetString(recBuffer);
                    Debug.WriteLine("Server_" + name + "received Data_" + data + " from: " + iPEndPoint);
                    Dns dnsPacket = JsonConvert.DeserializeObject<Dns>(data);
                    
                    string result = CheckCache(dnsPacket);



                }
                catch (SocketException e)
                {
                    Debug.WriteLine(e.StackTrace);
                    //depending if this was clients application or server manipulate rcode.
                    //but i think an error code 4 a  socket crash is not needed in this project.
                }
            }
        }
        void WriteLog()
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(baseLogsPath + "\\" + "127.0.0.10" + ".txt", true))
            {
                StringBuilder logBuilder = new StringBuilder();
                logBuilder.Append(DateTime.Now);
                logBuilder.Append("|");
                logBuilder.Append("127.0.0.10");
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

        public string CheckCache(Dns packet)
        {
            if (packet.resp.nextIp == "")
            {
                reqRcv++;
                WriteLog();
            }
            else
            {
                respRcv++;
                WriteLog();
            }

            Tuple<string,int> key = new Tuple<string, int>(packet.qry.name, packet.qry.type);
            return (chache.ContainsKey(key)) ? AnswerToApplication(chache[key].Item1) : ResolveRecursiv(packet);

        }
        public void StoreInCache(Tuple<string,int> key,Tuple<string,int> value)
        {
            if(!chache.ContainsKey(key)) { chache.Add(key, value); }
        }
        public string ResolveRecursiv(Dns packet)
        {
            if(packet.resp.nextIp == null)
            {
                Debug.WriteLine("Result of " + packet.qry.name + " resolved to " + packet.a);
                StoreInCache(new Tuple<string, int>(packet.qry.name, packet.qry.type), new Tuple<string, int>(packet.a, packet.resp.ttl));
                AnswerToApplication(packet.a);

                return packet.a;
            }
            if (packet.resp.nextIp == "")
            {
                packet.resp.nextIp = "127.0.0.11";
                GenerateCon(packet);
            }
            else
            {
                packet.flags.response = 0;
                GenerateCon(packet);
            }
            return null;
        }
        public void manipulateReq(Dns packet)
        {

        }
        string AnswerToApplication(string serviceData)
        {
            
            Thread answerToApplicationThread = new Thread(() =>
            {
                UdpClient answerListender = new UdpClient(IPEndPoint.Parse("127.0.0.10:53443"));
                IPEndPoint applicationEndpoint = IPEndPoint.Parse("127.0.0.1:51337");
                answerListender.Connect(applicationEndpoint);
                Byte[] sendData = Encoding.ASCII.GetBytes(serviceData);
                //highlight button
                Form1.ActiveForm.Controls["button34"].BackColor = Color.LightBlue;
                Thread.Sleep(350);
                //stop highlight button
                Form1.ActiveForm.Controls["button34"].BackColor = Color.FromArgb(0, 44, 43, 60);
                answerListender.Send(sendData, sendData.Length);
                respSend++;
                WriteLog();
                answerListender.Close();

            });
            answerToApplicationThread.Start();
            return serviceData;
        }


    }
}

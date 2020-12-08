using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                //server.Send(sendData, sendData.Length);
                senderClient.Send(sendData, sendData.Length, packet.resp.nextIp,53053);
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

        public string CheckCache(Dns packet)
        {
            Tuple<string,int> key = new Tuple<string, int>(packet.qry.name, packet.qry.type);
            return (chache.ContainsKey(key)) ? chache[key].Item1 : ResolveRecursiv(packet);

        }
        public void StoreInCache()
        {

        }
        public string ResolveRecursiv(Dns packet)
        {
            //TODO maybe check auth flag instead
            if(packet.resp.nextIp == null)
            {
                packet.a = packet.resp.nextIp;
                Debug.WriteLine("Result of " + packet.qry.name + " resolved to " + packet.a);
                return packet.a;
            }
            //TODO manipulate response
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
            //Todo change response, 
        }
    }
}

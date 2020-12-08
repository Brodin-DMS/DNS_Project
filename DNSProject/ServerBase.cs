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
    public class ServerBase
    {
        string name;
        IPAddress ipAddr;
        int port;
        IPEndPoint iPEndPoint;
        //TODO this is Key,Value with record
        Dictionary<Tuple<string, int>, Tuple<string, int>> chache;
        UdpClient server;
        IPEndPoint resolver;
        bool isAuth;

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
        }
        public void AddDictEntry(Tuple<string,int> key, Tuple<string,int> value)
        {
            chache.Add(key, value);
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

                    //process Data
                    ManipulateReq(packet);
                    //TODO maybe this gets send without maniplulation
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
        public void Send(Dns dnsPacket)
        {
            //TODO change thios to a sender
            //server.Connect(resolver);
            UdpClient senderClient = new UdpClient();
            Byte[] sendData = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(dnsPacket));

            //TODO definetly dont keep ref
            Thread senderThread = new Thread(() =>
            {
                senderClient.Send(sendData, sendData.Length, "127.0.0.10", 53053);
            });
            senderThread.Start();
        }
        public void ManipulateReq(Dns packet)
        {
            Debug.WriteLine("entering req manipulation!");
            //TODO return record
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
                    //well this has to be 1 but it will anyway
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

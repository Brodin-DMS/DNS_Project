using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DNSProject
{
    public class Srv
    {
        public string name;
        public int port;
        public string proto;
        public string service;
        public string target;

        public Srv(string name, int port, string proto, string service, string target)
        {
            this.name = name;
            this.port = port;
            this.proto = proto;
            this.service = service;
            this.target = target;
        }

    }
    public enum Proto
    {
        Udp,
        Tcp,
    }
}

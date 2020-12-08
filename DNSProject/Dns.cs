using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DNSProject
{
    public class Dns
    {
        public Flags flags;
        public Qry qry;
        public Count count;
        public string a;
        public string ns;
        public Resp resp;
        public Srv srv;
        public Dns(Flags flags, Qry qry, Count count, string a, string ns, Resp resp, Srv srv)
        {
            this.flags = flags;
            this.qry = qry;
            this.count = count;
            this.a = a;
            this.ns = ns;
            this.resp = resp;
            this.srv = srv;
        }
    }

}

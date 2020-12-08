using System;
using System.Collections.Generic;
using System.Text;

namespace DNSProject
{
    public class Resp
    {
        public int ttl;
        public string nextIp;

        public Resp(int ttl,string nextIp)
        {
            this.ttl = ttl;
            this.nextIp=nextIp;
        }
    }
}

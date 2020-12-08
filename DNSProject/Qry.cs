using System;
using System.Collections.Generic;
using System.Text;

namespace DNSProject
{
    public class Qry
    {
        public string name;
        public int type;
        public Qry(string name, int type)
        {
            this.name = name;
            this.type = type;

        }
    }
}

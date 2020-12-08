using System;
using System.Collections.Generic;
using System.Text;

namespace DNSProject
{
    public class Flags
    {
        public int response;
        public int recdesired;
        public int rcode;
        public int authorative;
        public Flags(int response, int recdesired, int rcode, int authorative)
        {
            this.response = response;
            this.recdesired = recdesired;
            this.rcode = rcode;
            this.authorative = authorative;
        }
    }
}

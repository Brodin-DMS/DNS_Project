﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DNSProject
{
    public class Count
    {
        public int answers;
        public List<Tuple<string, string>> rr_list;

        public Count(int answers)
        { 
            this.answers = answers;
        }
    }
}

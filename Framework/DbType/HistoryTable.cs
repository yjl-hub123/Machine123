﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine.Framework.DbType
{
    public class HistoryTable
    {
        public DateTime TimeOfOccurrence { get; set; }

        public string Msg { get; set; }

        public string Line { get; set; }
    }
}

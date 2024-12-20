﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Borsdata.Api.Dal.Model
{
    public partial class ReportsRespV1
    {
        public long? Instrument { get; set; }
        public List<ReportYearV1> ReportsYear { get; set; }
        public List<ReportR12V1> ReportsR12 { get; set; }
        public List<ReportQuarterV1> ReportsQuarter { get; set; }
    }
}

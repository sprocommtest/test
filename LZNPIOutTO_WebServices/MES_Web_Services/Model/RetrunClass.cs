using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MES_Web_Services
{
    [Serializable]
    public class RetrunClass
    {
        public int Status { get; set; }
        public Object ReturnContext { get; set; }
    }
}
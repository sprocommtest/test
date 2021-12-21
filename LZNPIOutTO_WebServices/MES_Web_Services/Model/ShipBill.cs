using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MES_Web_Services
{
    [Serializable]
    public class ShipBill
    {
        public string Ship_No { get; set; }
        public string Cust_No { get; set; }
        public string Cust_name { get; set; }
        public DateTime? Ship_Date { get; set; }
        public int Pack_Qty { get; set; }
        public int Qty { get; set; }
        public string User_No { get; set; }
        public int Bill_Type { get; set; }
        public int Isupload { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MES_Web_Services
{
    [Serializable]
    public class IMEI
    {
        public string Zbl_tsn { get; set; }
        public string Mo_No { get; set; }
        public string Lot_number { get; set; }
        public string Sn { get; set; }
        public string M_IMEI { get; set; }
        public string S_IMEI { get; set; }
        public string BT { get; set; }
        public string MAC { get; set; }
        public DateTime? Print_time { get; set; }
        public string Box_color { get; set; }
        public string Packing_No { get; set; }
        public DateTime? Packing_time { get; set; }
        public string Pallet_No { get; set; }
        public DateTime? Pallent_time { get; set; }
        public string Ship_ID { get; set; }
        public DateTime? Ship_Date { get; set; }
        public string Iccid { get; set; }
        public string EICCID { get; set; }
    }
}
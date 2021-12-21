using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MES_Web_Services
{
    [Serializable]
    public class ITEMINFO
    {
        public string ITEM_SN { get; set; }
        public string MO_NUMBER { get; set; }
        public string WORK_STATION { get; set; }
        public string ITEM_EC_STR { get; set; }
        public string ITEM_POINT_STR { get; set; }
        public string ITEM_COUNT_STR { get; set; }
        public string OPERATE_EMP { get; set; }
        public string REMARK1 { get; set; }
        public string REMARK2 { get; set; }
        public string REMARK3 { get; set; }
        public string REMARK4 { get; set; }
        public string REMARK5 { get; set; }
    }
}
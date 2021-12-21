using System;

namespace MES_Web_Services
{
    public class PackingTable : Mono
    {
        public string Id { get; set; }
        public string PackingNo { get; set; }
        public int FStatus { get; set; }

        /// <summary>
        /// 客户订单号
        /// </summary>
        public string Po { get; set; }

        /// <summary>
        /// 装箱数量
        /// </summary>
        public string Fqty { get; set; }

        public DateTime? CreateTime { get; set; }
        public string PalletId { get; set; }
        public string PalletNo { get; set; }
        public DateTime? PalletTime { get; set; }
        public string PalletOperator { get; set; }
        public string ShipNo { get; set; }
        public string ShipStatus { get; set; }
        public DateTime? ShipTime { get; set; }
    }
}
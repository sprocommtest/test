using Do_MIEI_Main.Do;
using SwiftMES.IIL.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MES_Web_Services.Services
{
    public class MESLogin
    {
        public DbSqlOperator _DB = new DbSqlOperator();
        public bool ValidateIPAccess(string ip, string userId)
        {  
        bool result = false;
            List<DoBlackIPInfo> whiteList = _DB.QueryList<DoBlackIPInfo>($@"SELECT a.*,b.userNo FROM T_BlackIP a INNER JOIN T_BlackIP_User b ON a.ID=b.BlackIP_ID where (a.Name='{userId}' or b.userNo='{userId}') and AuthorizeType=1 ;");

            if (whiteList.Count > 0)
            {
                result = IsInList(whiteList, ip);
                return result; //白名单优先于黑名单，在白名单则通过
            }

            List<DoBlackIPInfo> blackList = _DB.QueryList<DoBlackIPInfo>($@"SELECT a.*,b.userNo FROM T_BlackIP a INNER JOIN T_BlackIP_User b ON a.ID=b.BlackIP_ID where (a.Name='{userId}' or b.userNo='{userId}') and AuthorizeType=0 ;");
            if (blackList.Count > 0)
            {
                bool flag = IsInList(blackList, ip);
                return !flag;//不在则通过，在就禁止
            }

            //当黑白名单都为空的时候，那么返回true，则默认不禁止
            return true;
        }


        private bool IsInList(List<DoBlackIPInfo> list, string ip)
        {
            foreach (DoBlackIPInfo info in list)
            {
                if (IsInIp(ip, info.IPStart, info.IPEnd))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检测指定的IP地址是否在两个IP段中
        /// </summary>
        /// <param name="ip">指定的IP地址</param>
        /// <param name="begip">起始ip</param>
        /// <param name="endip">结束ip</param>
        /// <returns></returns>
        public static bool IsInIp(string ip, string begip, string endip)
        {
            int[] inip, begipint, endipint = new int[4];
            inip = GetIp(ip);
            begipint = GetIp(begip);
            endipint = GetIp(endip);
            for (int i = 0; i < 4; i++)
            {
                if (inip[i] < begipint[i] || inip[i] > endipint[i])
                {
                    return false;
                }
                else if (inip[i] > begipint[i] || inip[i] < endipint[i])
                {
                    return true;
                }
            }
            return true;
        }

        /// <summary>
        /// 将ip地址转成整形数组
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        protected static int[] GetIp(string ip)
        {
            int[] retip = new int[4];
            int i, count;
            char c;
            for (i = 0, count = 0; i < ip.Length; i++)
            {
                c = ip[i];
                if (c != '.')
                {
                    retip[count] = retip[count] * 10 + int.Parse(c.ToString());
                }
                else
                {
                    count++;
                }
            }

            return retip;

        }

        public string EncodeByMD5(string text)
        {
            byte[] buffer = Encoding.Default.GetBytes(text);
            MD5 md5 = MD5.Create();
            byte[] bufferNew = md5.ComputeHash(buffer);
            string strNew = string.Empty;
            for (int i = 0; i < bufferNew.Length; i++)
            {
                strNew += bufferNew[i].ToString("x2");
            }
            return strNew.ToUpper();
        }

        public string GetIP()
        {
          string _ip = "";
            using(var _mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                using(var _moc = _mc.GetInstances())
                {
                    foreach(var mo in _moc)
                    {
                        if((bool)mo["IPEnabled"] == true)
                        {
                            //st=mo["IpAddress"].ToString();
                            var _array = (System.Array)(mo.Properties["IpAddress"].Value);
                            _ip = _array.GetValue(0).ToString();
                            break;
                        }
                    }
                }
            }
            return _ip;
        }

        public string GetMac()
        {
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    return BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes());
                }
            }
            catch (Exception)
            {
               
            }
            return "00-00-00-00-00-00";
        }

        public bool AddLoginLog(DoUserInfo info,string IP,string Macaddr,string Note)
        {
            string systemType = "SwiftMES";
            if (_DB.QueryValue<int>($@"select count(*) from t_LoginLog where userNo='{info.UserNo}'") > 0)
            {
                return _DB.Execute($@"update t_LoginLog set note='{Note}',IPaddress='{IP}',MacAddress='{Macaddr}',LastUpdated=getdate(),SystemType_ID='{systemType}' ") > 0;
            }
            else
            {
                return _DB.Execute($@"INSERT INTO [dbo].[t_LoginLog]
           ([userNo]
           ,[LoginName]
           ,[FullName]
           ,[CompanyNo]
           ,[CompanyName]
           ,[Note]
           ,[IPAddress]
           ,[MacAddress]
           ,[LastUpdated]
           ,[SystemType_ID])
     VALUES
           ('{info.UserNo}','{info.UserName}','{info.UserName}','HM','泸州禾苗','{Note}','{IP}','{Macaddr}',getdate(),'{systemType}')") > 0;
            }
        }
    }
}
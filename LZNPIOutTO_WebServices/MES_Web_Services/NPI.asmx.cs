using System;
using System.Configuration;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services.Protocols;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System.Web.Script.Services;
using System.Web;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using SwiftMES.IIL.Client;
using Do_MIEI_Main.Veiw;
using System.Data.Common;
using Do_MIEI_Main;
using Do_MIEI_Main.Do;
using System.Collections;
using System.Web.UI.WebControls;
using MES_Web_Services.Services;

namespace MES_Web_Services
{
    /// <summary>
    /// MES_Services 的摘要说明
    /// </summary>
    [WebService(Namespace = "Mes.Web.Services")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。
    // [System.Web.Script.Services.ScriptService]    : System.Web.Services.WebService
    [SoapDocumentService(RoutingStyle = SoapServiceRoutingStyle.RequestElement)]
    public class MES_Services
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        //private static string connSqlStr = MySecurity.Decrypt(ConfigurationManager.ConnectionStrings["ConnStr"].ToString().Trim());
        public DbSqlOperator _DB = new DbSqlOperator();
        public MESLogin lg = new MESLogin();



        [WebMethod(Description = "获取服务器时间")]
        public string GetServertime()
        {
            return _DB.QueryValue<DateTime>("SELECT  GETDATE()").ToString();
        }

        [WebMethod(Description = "站点检测   SN 产品编码，station 站点(请联系禾苗获取相应配置站点) prodno 工单号(可不填)")]
        public string CheckStation(string sn, string station,string prodno=null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sn))
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "Sn不能为空" }).ModelToJson();

                }
                if (string.IsNullOrWhiteSpace(station))
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "station不能为空" }).ModelToJson();

                }

              // bool bretest = Convert.ToBoolean(string.IsNullOrWhiteSpace(retest)?"false": retest);

               prodno =string.IsNullOrWhiteSpace(prodno)? _DB.QueryValue<string>($@"select prodno from prodbar where pcbseq='{sn}'"): prodno;

                if (string.IsNullOrWhiteSpace(prodno))
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "该条码无法找到相应工单号" }).ModelToJson();
                }
                List<DbParameter> dbParameters = new List<DbParameter>();
                dbParameters.Add(_DB.CreateParameter("pcbSeq", sn));
                dbParameters.Add(_DB.CreateParameter("prodNo", prodno));
                dbParameters.Add(_DB.CreateParameter("routeNo", station));
                dbParameters.Add(_DB.CreateParameter("retest", false));

                var _dt = _DB.QueryTableByProc("CheckRoute", dbParameters);
                if (Convert.ToInt32(_dt.Rows[0][0]) == 1)
                {
                    return (new JsonBasc() { IsSuccess = false, Message = string.Concat(_dt.Rows[0][1]) }).ModelToJson();
                }
                else
                {
                    return (new JsonBasc() { IsSuccess = true }).ModelToJson();
                }

            }
            catch (Exception err)
            {
                return (new Ve_GetSW_BYSN() { IsSuccess = false, Message = err.Message }).ModelToJson();
            }
        }
        [WebMethod(Description = "过站  SN 产品编码，station 站点(请联系禾苗获取相应配置站点)， user 请填写公司名称， status (过站状态: PASS 通过,FAIL 失败，prodno 工单号(可不填)，remark 备注(可不填))")]
        public string SetStation(string sn, string station, string user, string status, string prodno = null, string remark = "", double weight=0,string packNo="",string rmk1="", string rmk2 = "", string rmk3 = "", string rmk4 = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sn))
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "Sn不能为空" }).ModelToJson();

                }
                if (string.IsNullOrWhiteSpace(station))
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "station不能为空" }).ModelToJson();

                }

                if (string.IsNullOrWhiteSpace(prodno))
                {
                    prodno = _DB.QueryValue<string>($@"select prodno from prodbar where pcbseq='{sn}'");
                }

                if (string.IsNullOrWhiteSpace(prodno))
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "该条码无法找到相应工单号" }).ModelToJson();
                }
                List<DbParameter> dbParameters = new List<DbParameter>();
                dbParameters.Add(_DB.CreateParameter("pcbSeq", sn));
                dbParameters.Add(_DB.CreateParameter("prodNo", prodno));
                dbParameters.Add(_DB.CreateParameter("routeNo", station));
                dbParameters.Add(_DB.CreateParameter("result", status));
                dbParameters.Add(_DB.CreateParameter("remark", "{}{}{}"));
                dbParameters.Add(_DB.CreateParameter("testItem", remark));
                dbParameters.Add(_DB.CreateParameter("userNo", string.IsNullOrWhiteSpace(user)?"对外接口":user));
                dbParameters.Add(_DB.CreateParameter("weight", weight));
                dbParameters.Add(_DB.CreateParameter("packNo", packNo));
                dbParameters.Add(_DB.CreateParameter("rmk1", rmk1));
                dbParameters.Add(_DB.CreateParameter("rmk2", rmk2));
                dbParameters.Add(_DB.CreateParameter("rmk3", rmk3));
                dbParameters.Add(_DB.CreateParameter("rmk4", rmk4));
                var _dt = _DB.QueryTableByProc("CreateRmkRoute", dbParameters);
                if (Convert.ToInt32(_dt.Rows[0][0]) == 1)
                {
                    return (new JsonBasc() { IsSuccess = false, Message = string.Concat(_dt.Rows[0][1]) }).ModelToJson();
                }
                else
                {
                    return (new JsonBasc() { IsSuccess = true }).ModelToJson();
                }

            }
            catch (Exception err)
            {
                return (new Ve_GetSW_BYSN() { IsSuccess = false, Message = err.Message }).ModelToJson();
            }
        }

        [WebMethod(Description = "根据工单获取SN ，count为0时获取所有SN，Islaser 获取是否镭已雕数据 true为已镭雕，false为未镭雕")]
        public string GetSNByProdNo(string ProdNo, int count, bool Islaser)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ProdNo))
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "ProdNo不能为空" }).ModelToJson();

                }
                if (_DB.QueryValue<int>($@"select count(*) from prod where ProdNo='{ProdNo}' and LDposted=1") <= 0)
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "该工单品质未镭雕审核" }).ModelToJson();
                }

                if (_DB.QueryValue<int>($@"select count(*) from prodbar where ProdNo='{ProdNo}'")<=0)
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "该工单号无SN条码" }).ModelToJson();
                }
                List<Ve_SN> result = new List<Ve_SN>();
                if (count <= 0)
                {
                     result = _DB.QueryList<Ve_SN>($@" select pcbSeq as SN from prodbar where prodNo='{ProdNo}' and type='SMT' and isnull(islaser,0)={(Islaser == true ? "1" : "0")}");
                }
                else
                {
                    result = _DB.QueryList<Ve_SN>($@" select top {count} pcbSeq as SN from prodbar where prodNo='{ProdNo}' and type='SMT' and isnull(islaser,0)={(Islaser == true ? "1" : "0")} order by pcbSeq");
                }
 
                return (new JsonBasc() { IsSuccess = true, obj= result }).ModelToJson();
                

            }
            catch (Exception err)
            {
                return (new JsonBasc() { IsSuccess = false, Message = err.Message }).ModelToJson();
            }
        }


        [WebMethod(Description = "根据工单获取SN ，StartCount为起始序号，EndCount为结束序号，Islaser 获取是否镭已雕数据 true为已镭雕，false为未镭雕")]
        public string GetBeTweenSNByProdNo(string ProdNo, string StartCount, string EndCount, bool Islaser)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ProdNo))
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "ProdNo不能为空" }).ModelToJson();

                }

                if (_DB.QueryValue<int>($@"select count(*) from prod where ProdNo='{ProdNo}' and LDposted=1") <= 0)
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "该工单品质未镭雕审核" }).ModelToJson();
                }

                if (_DB.QueryValue<int>($@"select count(*) from prodbar where ProdNo='{ProdNo}'") <= 0)
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "该工单号无SN条码" }).ModelToJson();
                }
                List<Ve_SN> result = new List<Ve_SN>();
              
                    result = _DB.QueryList<Ve_SN>($@"select pcbSeq as SN from  (SELECT row_num=ROW_NUMBER() OVER (ORDER BY pcbSeq ),* FROM ProdBar WHERE prodNo='{ProdNo}' and type='SMT'  ) a  WHERE row_num>={StartCount} and row_num<={EndCount}  and isnull(islaser,0)={(Islaser == true ? "1" : "0")}");
                    //result = _DB.QueryList<Ve_SN>($@" select top {count} pcbSeq as SN from prodbar where prodNo='{ProdNo}' and type='SMT' and isnull(islaser,0)={(Islaser == true ? "1" : "0")} order by pcbSeq");
                

                return (new JsonBasc() { IsSuccess = true, obj = result }).ModelToJson();


            }
            catch (Exception err)
            {
                return (new JsonBasc() { IsSuccess = false, Message = err.Message }).ModelToJson();
            }
        }

        [WebMethod(Description = "根据SN 更新镭雕状态")]
        public string UpDateIslaser(string SN)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SN))
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "SN不能为空" }).ModelToJson();

                }

                if (_DB.Execute($@"update prodbar set islaser=1  where pcbseq='{SN}' and type='SMT'") <= 0)
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "更新数据失败" }).ModelToJson();
                }            
                return (new JsonBasc() { IsSuccess = true}).ModelToJson();


            }
            catch (Exception err)
            {
                return (new JsonBasc() { IsSuccess = false, Message = err.Message }).ModelToJson();
            }
        }
        [WebMethod(Description = "根据Json转换成SN的list集合 更新镭雕状态")]
        public string UpDateIslaserByJson(string Jsons)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Jsons))
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "Json不能为空" }).ModelToJson();

                }
                List<Ve_SN> updatelist = Jsons.JsonToModel<List<Ve_SN>>();
                List<Ve_SN> updatefilelist = new List<Ve_SN>();
                if (updatelist==null||(updatelist!=null&&updatelist.Count<=0))
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "Json转化成SN列表为空" }).ModelToJson();
                }
               int SucceseCount = 0;
                string Errmsg = string.Empty;
                foreach (Ve_SN item in updatelist)
                {
                    try
                    {


                        if (_DB.Execute($@"update prodbar set islaser=1  where pcbseq='{item.SN}' and type='SMT'") <= 0)
                        {
                            updatefilelist.Add(new Ve_SN() { SN = item.SN });
                            Errmsg += item.SN;
                        }
                        else
                        {
                            SucceseCount++;
                        }
                    }
                    catch
                    {
                        updatefilelist.Add(new Ve_SN() { SN = item.SN });
                        Errmsg += item.SN;
                    }
                }
                if (SucceseCount == updatelist.Count)
                {
                    return (new JsonBasc() { IsSuccess = true }).ModelToJson();
                }
                else if (SucceseCount <= 0)
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "数据更新失败"  }).ModelToJson();
                }
                else
                {
                    return (new JsonBasc() { IsSuccess = true, Message = $@"数据更新成功{SucceseCount.ToString()}条,失败{(updatelist.Count- SucceseCount).ToString()}条", obj = updatefilelist }).ModelToJson();
                }


            }
            catch (Exception err)
            {
                return (new Ve_GetSW_BYSN() { IsSuccess = false, Message = err.Message }).ModelToJson();
            }
        }


        [WebMethod(Description = "根据SN查询IMEI")]
        public string QueryIMEIBySN(string SN,string Type="")
        {
            try
            {

                if (string.IsNullOrWhiteSpace(SN))
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "SN不能为空" }).ModelToJson();

                }
                string strType = string.IsNullOrWhiteSpace(Type) ? "SMT" : Type;
                int count = _DB.QueryValue<int>($@"select count(*) from prodbar where type='{strType}' and isnull(islaser,0)=0 and ( pcbseq='{SN}' or seq='{SN}' or seq2 ='{SN}') ");
                if (count > 0)
                {
                    Ve_GetIMEI model= _DB.QuerySingle<Ve_GetIMEI>($@"select pcbseq as SN,seq as IMEI,seq2 as IMEI2 from prodbar where type='{strType}' and isnull(islaser,0)=0 and ( pcbseq='{SN}' or seq='{SN}' or seq2 ='{SN}')");
                    return (new JsonBasc() { IsSuccess = true, obj= model }).ModelToJson();
                }
                else
                {
                    return (new JsonBasc() { IsSuccess = false,Message="查无此SN" }).ModelToJson();
                }




            }
            catch (Exception err)
            {
                return (new JsonBasc() { IsSuccess = false, Message = err.Message }).ModelToJson();
            }
        }


        [WebMethod(Description = "根据SN判断是否已镭雕")]
        public string CheckNolaserBySN(string SN)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(SN))
                {
                    return (new JsonBasc() { IsSuccess = false, Message = "SN不能为空" }).ModelToJson();

                }
                int count = _DB.QueryValue<int>($@"select count(*) from prodbar where type='SMT' and isnull(islaser,0)=0 and ( pcbseq='{SN}' or seq='{SN}' or seq2 ='{SN}') ");
                if (count > 0)
                {
                    return (new JsonBasc() { IsSuccess = true }).ModelToJson();
                }
                else
                {
                    return (new JsonBasc() { IsSuccess = false }).ModelToJson();
                }
                         
            }
            catch (Exception err)
            {
                return (new JsonBasc() { IsSuccess = false, Message = err.Message }).ModelToJson();
            }
        }
    }



}
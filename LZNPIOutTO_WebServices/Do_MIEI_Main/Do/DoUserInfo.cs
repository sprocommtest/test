using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Do_MIEI_Main.Do
{
    public class DoUserInfo
    {
        /// <summary>
        /// 默认构造函数（需要初始化属性的在此处理）
        /// </summary>
        public DoUserInfo()
        {
            this.UserNo = System.Guid.NewGuid().ToString();
            this.Suspend = false;
            this.Version = 0;
            this.IsExpire = false;

        }

        #region Properties

        /// <summary> G/S: 用户编号 </summary>
        public string UserNo { get; set; }

        /// <summary> G/S: 父用户编号 </summary>
        public string Pno { get; set; }

        /// <summary> G/S: 用户名称 </summary>
        public string UserName { get; set; }

        /// <summary> G/S: 部门编号 </summary>
        public string DeptNo { get; set; }

        /// <summary> G/S: 默认部门名称 </summary>
        public string DeptName { get; set; }

        /// <summary> G/S: 密码 </summary>
        public string Password { get; set; }

        /// <summary> G/S: 邮件 </summary>
        public string Email { get; set; }

        /// <summary> G/S: 备注 </summary>
        public string Remark { get; set; }

        /// <summary> G/S: 暂停使用 </summary>
        public bool Suspend { get; set; }

        /// <summary> G/S: 手机 </summary>
        public string Mobile { get; set; }

        /// <summary> G/S: 电话 </summary>
        public string Tel { get; set; }

        /// <summary> G/S: 公司编号 </summary>
        public string CompanyNo { get; set; }

        /// <summary> G/S: 所属公司名称 </summary>
        public string CompanyName { get; set; }

        /// <summary> G/S: 新建日期时间 </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary> G/S: 新建用户 </summary>
        public string CreatedUser { get; set; }

        /// <summary> G/S: 创建人ID </summary>
        public string Creator_ID { get; set; }

        /// <summary> G/S: 编辑人ID </summary>
        public string Editor_ID { get; set; }

        /// <summary> G/S: 修改日期时间 </summary>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary> G/S: 修改用户 </summary>
        public string ModifiedUser { get; set; }

        /// <summary> G/S: 版本 </summary>
        public int Version { get; set; }

        /// <summary> G/S: 供应商编号 </summary>
        public string VendNo { get; set; }

        /// <summary> G/S: 客户编号 </summary>
        public string CustNo { get; set; }

        /// <summary> G/S: IPQC巡线密码 </summary>
        public string IpqcPwd { get; set; }

        /// <summary> G/S: 真实姓名 </summary>
        public string FullName { get; set; }

        /// <summary> G/S: 用户呢称 </summary>
        public string Nickname { get; set; }

        /// <summary> G/S: 是否过期 </summary>
        public bool IsExpire { get; set; }

        /// <summary> G/S: 过期时间 </summary>
        public DateTime? ExpireDate { get; set; }

        /// <summary> G/S: 职务头衔 </summary>
        public string Title { get; set; }

        /// <summary> G/S: 身份证号码 </summary>
        public string IdentityCard { get; set; }

        /// <summary> G/S: 办公电话 </summary>
        public string OfficePhone { get; set; }

        /// <summary> G/S: 邮件地址 </summary>
        public string Email1 { get; set; }

        /// <summary> G/S: 住址 </summary>
        public string Address { get; set; }

        /// <summary> G/S: 办公地址 </summary>
        public string WorkAddr { get; set; }

        /// <summary> G/S: 性别 </summary>
        public string Gender { get; set; }

        /// <summary> G/S: 出生日期 </summary>
        public DateTime? Birthday { get; set; }

        /// <summary> G/S: QQ号码 </summary>
        public string QQ { get; set; }

        /// <summary> G/S: 个性签名 </summary>
        public string Signature { get; set; }

        /// <summary> G/S: 审核状态 </summary>
        public string AuditStatus { get; set; }

        /// <summary> G/S: 备注 </summary>
        public string Note { get; set; }

        /// <summary> G/S: 自定义字段 </summary>
        public string CustomField { get; set; }

        /// <summary> G/S: 排序码 </summary>
        public string SortCode { get; set; }

        /// <summary> G/S: 密保：提示问题 </summary>
        public string Question { get; set; }

        /// <summary> G/S: 密保:问题答案 </summary>
        public string Answer { get; set; }

        /// <summary> G/S: 上次登录IP </summary>
        public string LastLoginIP { get; set; }

        /// <summary> G/S: 上次登录时间 </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary> G/S: 上次Mac地址 </summary>
        public string LastMacAddress { get; set; }

        /// <summary> G/S: 当前登录IP </summary>
        public string CurrentLoginIP { get; set; }

        /// <summary> G/S: 当前登录时间 </summary>
        public DateTime? CurrentLoginTime { get; set; }

        /// <summary> G/S: 当前Mac地址 </summary>
        public string CurrentMacAddress { get; set; }

        /// <summary> G/S: 最后修改密码日期 </summary>
        public DateTime? LastPasswordTime { get; set; }

        /// <summary> G/S: 微信用户OpenID </summary>
        public string OpenId { get; set; }

        /// <summary> G/S: 微信多平台应用下的统一ID </summary>
        public string UnionId { get; set; }

        /// <summary> G/S: 微信关注状态 </summary>
        public string Status { get; set; }

        /// <summary> G/S: 公众号 </summary>
        public string SubscribeWechat { get; set; }

        /// <summary> G/S: 科室权限 </summary>
        public string DeptPermission { get; set; }

        /// <summary> G/S: 企业微信UserID </summary>
        public string CorpUserId { get; set; }

        /// <summary> G/S: 企业微信状态 </summary>
        public string CorpStatus { get; set; }

        /// <summary>  G/S:消息推送处理是否要输入备注 </summary>
        public Boolean IsPushMsgRemarkUse { get; set; }
        #endregion

    }
}

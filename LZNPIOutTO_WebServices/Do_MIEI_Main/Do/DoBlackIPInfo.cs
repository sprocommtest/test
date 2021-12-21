using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Do_MIEI_Main.Do
{
    public class DoBlackIPInfo
    {
        /// <summary>
        /// 默认构造函数（需要初始化属性的在此处理）
        /// </summary>
        public DoBlackIPInfo()
        {
            this.ID = System.Guid.NewGuid().ToString();
            this.AuthorizeType = 0;
            this.Forbid = 0;
            this.CreateTime = System.DateTime.Now;
            this.EditTime = System.DateTime.Now;

        }

        #region Property Members

        
        public virtual string ID { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
		 
        public virtual string Name { get; set; }

        /// <summary>
        /// 授权类型
        /// </summary>
		 
        public virtual int AuthorizeType { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
		 
        public virtual int Forbid { get; set; }

        /// <summary>
        /// IP起始地址
        /// </summary>
		 
        public virtual string IPStart { get; set; }

        /// <summary>
        /// IP结束地址
        /// </summary>
		 
        public virtual string IPEnd { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
		 
        public virtual string Note { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
		 
        public virtual string Creator { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
		 
        public virtual string Creator_ID { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
		 
        public virtual DateTime CreateTime { get; set; }

        /// <summary>
        /// 编辑人
        /// </summary>
		 
        public virtual string Editor { get; set; }

        /// <summary>
        /// 编辑人ID
        /// </summary>
		 
        public virtual string Editor_ID { get; set; }

        /// <summary>
        /// 编辑时间
        /// </summary>
		 
        public virtual DateTime EditTime { get; set; }


        #endregion
    }
}

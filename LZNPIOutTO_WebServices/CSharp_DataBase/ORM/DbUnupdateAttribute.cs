using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwiftMES.Core
{

    /// <summary>
    /// 特性：不更新的数据字段映射
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DbUnupdateAttribute : DbAttribute
    {

        #region Properties
        /// <summary> G/S:数据列名 </summary>
        public string ColumnName { get; set; }

        /// <summary> G/S:描述 </summary>
        public string Description { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 创建特性：数据字段映射
        /// </summary>
        public DbUnupdateAttribute()
        {
            this.ColumnName = "";
            this.Description = "";
        }

        /// <summary>
        /// 创建特性：数据字段映射
        /// </summary>
        /// <param name="columnname">数据列名</param>
        /// <param name="description">描述</param>
        public DbUnupdateAttribute(string columnname, string description = "")
        {
            this.ColumnName = columnname;
            this.Description = description;
        }
        #endregion

    }

}

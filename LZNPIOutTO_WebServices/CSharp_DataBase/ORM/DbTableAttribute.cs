using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwiftMES.Core
{

    /// <summary>
    /// 特性：数据表映射
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DbTableAttribute : DbAttribute
    {

        #region Properties

        /// <summary> G/S:数据表名 </summary>
        public string Name { get; set; }

        /// <summary> G/S:描述 </summary>
        public string Description { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 创建特性：数据表映射
        /// </summary>
        public DbTableAttribute()
        {
            this.Name = "";
            this.Description = "";
        }

        /// <summary>
        /// 创建特性：数据表映射
        /// </summary>
        /// <param name="tablename">数据表名</param>
        /// <param name="description">描述</param>
        public DbTableAttribute(string tablename, string description = "")
        {
            this.Name = tablename;
            this.Description = description;
        }

        #endregion


    }

}

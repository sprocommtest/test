using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwiftMES.Core
{

    /// <summary>
    /// 特性：条件数据字段映射
    /// （用于删除、更新）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class DbConditionAttribute : Attribute
    {

        #region Properties

        #endregion

        #region Constructors

        /// <summary>
        /// 创建特性：条件数据字段映射
        /// （用于删除、更新）
        /// </summary>
        public DbConditionAttribute()
        {
        }

        #endregion

    }

}

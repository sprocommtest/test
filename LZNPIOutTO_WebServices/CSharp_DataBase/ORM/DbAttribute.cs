using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwiftMES.Core
{

    /// <summary>
    /// 特性：数据库映射特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class DbAttribute : Attribute
    {


    }

}

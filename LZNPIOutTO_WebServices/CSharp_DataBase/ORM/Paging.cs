using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_DataBase
{
    /// <summary>
    /// 分页信息
    /// </summary>
    [Serializable]
    public struct Paging
    {

        #region Properties

        /// <summary> Get:是否为空值 </summary>
        public bool IsEmpty
        {
            get { return this.PageNumber <= 0 || this.PageSize <= 0; }
        }

        /// <summary> G/S:当前页码 </summary>
        public int PageNumber { get; set; }

        /// <summary> G/S:页面大小（容量） </summary>
        public int PageSize { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 创建分页信息
        /// </summary>
        /// <param name="pageno">页码</param>
        /// <param name="size">页面大小</param>
        public Paging(int pageno, int size)
        {
            this.PageNumber = pageno;
            this.PageSize = size;
        }

        #endregion


    }
}

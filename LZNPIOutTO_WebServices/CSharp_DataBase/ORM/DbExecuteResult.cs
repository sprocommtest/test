using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_DataBase
{
    /// <summary>
    /// Db结果：执行操作后的返回结果
    /// </summary>
    public class DbExecuteResult 
    {

        #region Properties

        /// <summary> G/S:信息 </summary>
        public string Message { get; set; }

        /// <summary> G/S:信息ID </summary>
        public string MessageNO { get; set; }

        /// <summary> G/S:是否成功执行 </summary>
        public bool IsSucceed { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 创建返回的结果
        /// </summary>
        public DbExecuteResult()
        {
            this.Message = "";
            this.MessageNO = "";
            this.IsSucceed = false;
        }

        /// <summary>
        /// 创建返回的结果
        /// </summary>
        /// <param name="isSucceed">是否成功执行</param>
        public DbExecuteResult(bool isSucceed) : this()
        {
            this.IsSucceed = isSucceed;
        }

        /// <summary>
        /// 创建返回的结果
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="isSucceed">是否成功执行</param>
        public DbExecuteResult(bool isSucceed, string message) : this()
        {
            this.Message = message;
            this.IsSucceed = isSucceed;
        }

        /// <summary>
        /// 创建返回的结果
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="messageNO">信息编号</param>
        /// <param name="isSucceed">是否成功执行</param>
        public DbExecuteResult(bool isSucceed, string messageNO, string message)
        {
            this.MessageNO = messageNO;
            this.Message = message;
            this.IsSucceed = isSucceed;
        }

        #endregion

    }
}

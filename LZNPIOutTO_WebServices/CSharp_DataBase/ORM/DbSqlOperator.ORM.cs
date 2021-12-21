using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CSharp_DataBase;
using SwiftMES.Core;
using SwiftMES.Utility.Core;
namespace SwiftMES.IIL.Client
{


    partial class DbSqlOperator
    {

        #region Functions.ORM.Parameter

        /// <summary>
        /// Get::将DB值映射字典转换为List
        /// </summary>
        /// <param name="dict">DB值映射字典</param>
        /// <returns>SQL参数</returns>
        private List<DbParameter> _ToParameterList(Dictionary<string, object> dict)
        {
            var _list = new List<DbParameter>();
            foreach (var kv in dict)
            {
                _list.Add(this.CreateParameter(kv.Key, kv.Value));
            }
            return _list;
        }

        /// <summary>
        /// Get::获取用于新增数据的参数
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="entity">数据实体</param>
        /// <returns>SQL参数</returns>
        public List<DbParameter> GetParameters4Insert<TEntity>(TEntity entity)
            where TEntity : class
        {
            var _dict = entity.GetDbValueDict4Insert();
            return this._ToParameterList(_dict);
        }

        /// <summary>
        /// Get::获取用于更新数据的参数
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="entity">数据实体</param>
        /// <returns>SQL参数</returns>
        public List<DbParameter> GetParameters4Update<TEntity>(TEntity entity)
            where TEntity : class
        {
            var _dict = entity.GetDbValueDict4Update();
            return this._ToParameterList(_dict);
        }

        /// <summary>
        /// Get::获取用于删除数据的参数
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="entity">数据实体</param>
        /// <returns>SQL参数</returns>
        public List<DbParameter> GetParameters4Delete<TEntity>(TEntity entity)
            where TEntity : class
        {
            var _dict = entity.GetDbValueDict4Primary();
            return this._ToParameterList(_dict);
        }

        /// <summary>
        /// Get::获取用于条件筛选数据的参数
        /// （主键为条件，可用于单一数据的删除、更新、获取）
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="entity">数据实体</param>
        /// <returns>SQL参数</returns>
        public List<DbParameter> GetParameters4Condition<TEntity>(TEntity entity)
            where TEntity : class
        {
            var _dict = entity.GetDbValueDict4Primary();
            return this._ToParameterList(_dict);
        }

        #endregion


        #region Functions.Convert

        #region Functions.Private

        /// <summary>
        /// Get:匹配函数（用于映射数据列与实体类的属性、字段）
        /// </summary>
        /// <param name="member">实体类的成员（属性、字段）</param>
        /// <param name="column">数据列</param>
        /// <returns></returns>
        private bool _OnMemberMatch(MemberInfo member, DataColumn column)
        {
            var _fieldAttribute = member.GetFirstAttribute<DbFieldAttribute>();
            if (_fieldAttribute == null)
            {
                return member.Name.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return _fieldAttribute.ColumnName.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Get:匹配函数（用于映射数据列与实体类的属性、字段）
        /// </summary>
        /// <param name="member">实体类的成员（属性、字段）</param>
        /// <param name="columnName">数据列名称</param>
        /// <returns></returns>
        private bool _OnMemberMatch(MemberInfo member, string columnName)
        {
            var _fieldAttribute = member.GetFirstAttribute<DbFieldAttribute>();
            if (_fieldAttribute == null)
            {
                return member.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return _fieldAttribute.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase);
            }
        }

        #endregion

        #region Functions.ToEntity

        /// <summary>
        /// Get:将数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="row">DataTable的行数据</param>
        /// <returns>实体类</returns>
        public TEntity ToEntity<TEntity>(DataRow row)
            where TEntity : class, new()
        {
            if (row == null) { return default(TEntity); }
            if (row.ItemArray == null) { return default(TEntity); }
            if (row.ItemArray.Length < 1) { return default(TEntity); }

            return DataUtility.ToEntity<TEntity>(row, this._OnMemberMatch);
        }

        /// <summary>
        /// Get:将数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="reader">数据集流</param>
        /// <returns>实体类</returns>
        public TEntity ToEntity<TEntity>(IDataReader reader)
            where TEntity : class, new()
        {
            if (reader == null) { return default(TEntity); }
            if (reader.IsClosed) { return default(TEntity); }

            return DataUtility.ToEntity<TEntity>(reader, this._OnMemberMatch);
        }

        #endregion

        #region Functions.ToEntity.First

        /// <summary>
        /// Get:将数据源中的首条数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="table">DataTable数据</param>
        /// <returns>实体类</returns>
        public TEntity ToEntity1st<TEntity>(DataTable table)
            where TEntity : class, new()
        {
            if (table == null) { return default(TEntity); }
            if (table.Rows.Count < 1) { return default(TEntity); }

            var _row = table.Rows[0];
            return ToEntity<TEntity>(_row);
        }

        /// <summary>
        /// Get:将数据源中的首条数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="reader">数据集流</param>
        /// <returns>实体类</returns>
        public TEntity ToEntity1st<TEntity>(IDataReader reader)
            where TEntity : class, new()
        {
            if (reader == null) { return default(TEntity); }
            if (reader.IsClosed) { return default(TEntity); }

            return DataUtility.ToEntity<TEntity>(reader, this._OnMemberMatch);
        }

        #endregion

        #region Functions.ToList

        /// <summary>
        /// Get:将数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="reader">数据集流</param>
        /// <returns>实体类</returns>
        public List<TEntity> ToList<TEntity>(IDataReader reader)
            where TEntity : class, new()
        {
            if (reader == null) { return new List<TEntity>(); }
            if (reader.IsClosed) { return new List<TEntity>(); }

            return DataUtility.ToList<TEntity>(reader, this._OnMemberMatch);
        }

        /// <summary>
        /// Get:将数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="table">DataTable数据</param>
        /// <returns>实体类</returns>
        public List<TEntity> ToList<TEntity>(DataTable table)
            where TEntity : class, new()
        {
            if (table == null) { return new List<TEntity>(); }
            if (table.Rows == null) { return new List<TEntity>(); }
            if (table.Rows.Count < 1) { return new List<TEntity>(); }

            return DataUtility.ToList<TEntity>(table, this._OnMemberMatch);
        }

        /// <summary>
        /// Get:将数据转换为实体类列表
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="rows">DataTable的行数据迭代</param>
        /// <returns>实体类列表</returns>
        public List<TEntity> ToList<TEntity>(IEnumerable<DataRow> rows)
            where TEntity : class, new()
        {
            if (rows == null) { return new List<TEntity>(); }
            return DataUtility.ToList<TEntity>(rows, this._OnMemberMatch);
        }

        #endregion

        #endregion



    }

}

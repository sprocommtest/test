using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SwiftMES.Utility.Core;

namespace SwiftMES.IIL.Client
{

    partial class DbSqlMaker
    {

        #region Functions.Privates

        /// <summary>
        /// Get:转换为查询条件的语句部份
        /// </summary>
        /// <param name="args">内容列表</param>
        /// <returns></returns>
        private string _ToPart4Wheres(IEnumerable<DbParameter> args)
        {
            var _wheres = "";

            #region Wheres
            {
                if (args != null)
                {
                    foreach (var x in args)
                    {
                        if (x == null) { continue; }
                        if (string.IsNullOrWhiteSpace(x.ParameterName)) { continue; }
                        if (string.IsNullOrWhiteSpace(x.SourceColumn)) { continue; }
                        _wheres += $"and {x.SourceColumn}=@{x.ParameterName} ";
                    }
                    if (!string.IsNullOrWhiteSpace(_wheres))
                    {
                        _wheres = $" where {_wheres.Remove(0, 3)} ";
                    }
                }
            }
            #endregion

            return _wheres;
        }

        private DbSqlOperator GetDbOperator()
        {
            return new DbSqlOperator() ;
        }

        /// <summary>
        /// Get:转换为查询列的语句部份
        /// </summary>
        /// <param name="args">内容列表</param>
        /// <returns></returns>
        private string _ToPart4InsertColumns(IEnumerable<DbParameter> args, string tableName = null)
        {
            var _columns = "";

            #region Columns
            {
                if (args != null)
                {
                    List<ColumnName> ColumnNames = new List<ColumnName>();
                    if (tableName != null)
                    {
                        var _db = this.GetDbOperator();
                        ColumnNames = _db.QueryList<ColumnName>("select  syscolumns.name  from sysobjects inner join syscolumns on sysobjects.id=syscolumns.id where sysobjects.name='" + tableName + "';");
                    }

                    foreach (var x in args)
                    {
                        if (ColumnNames != null && ColumnNames.Count > 0 && ColumnNames.Where(a => a.name.Equals(x.ParameterName, StringComparison.OrdinalIgnoreCase)).Count() == 0)
                        {
                            continue;
                        }
                        if (x == null || (x != null && x.Value == DBNull.Value)) { continue; }
                        if (string.IsNullOrWhiteSpace(x.ParameterName)) { continue; }
                        if (string.IsNullOrWhiteSpace(x.SourceColumn)) { continue; }
                        _columns += $"[{x.SourceColumn}],";
                    }
                    _columns = _columns.TrimEnd(',');
                }
            }
            #endregion

            if (string.IsNullOrWhiteSpace(_columns)) { return ""; }
            return $"({_columns})";
        }

        /// <summary>
        /// Get:转换为新增语句中的数据值部份
        /// </summary>
        /// <param name="args">内容列表</param>
        /// <returns></returns>
        private string _ToPart4InsertValues(IEnumerable<DbParameter> args, string tableName = null)
        {
            var _values = "";

            #region Columns
            {
                if (args != null)
                {
                    List<ColumnName> ColumnNames = new List<ColumnName>();
                    if (tableName != null)
                    {
                        var _db = this.GetDbOperator();
                        ColumnNames = _db.QueryList<ColumnName>("select  syscolumns.name  from sysobjects inner join syscolumns on sysobjects.id=syscolumns.id where sysobjects.name='" + tableName + "';");
                    }

                    foreach (var x in args)
                    {
                        if (ColumnNames != null && ColumnNames.Count > 0 && ColumnNames.Where(a => a.name.Equals(x.ParameterName, StringComparison.OrdinalIgnoreCase)).Count() == 0)
                        {
                            continue;
                        }
                        if (x == null || (x != null && x.Value == DBNull.Value)) { continue; }
                        if (string.IsNullOrWhiteSpace(x.ParameterName)) { continue; }
                        if (string.IsNullOrWhiteSpace(x.SourceColumn)) { continue; }

                        _values += $"@{x.ParameterName},";
                    }
                    _values = _values.TrimEnd(',');
                }
            }
            #endregion

            if (string.IsNullOrWhiteSpace(_values)) { return ""; }
            return $"({_values})";
        }

        #endregion

        #region Functions.ORM

        #region Functions.ORM.Query

        /// <summary>
        /// Get:创建查询数据总量的查询语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="conditionParameters">条件参数的迭代</param>
        /// <returns></returns>
        public string CreateSQL4Count(string tablename, IEnumerable<DbParameter> conditionParameters)
        {
            if (string.IsNullOrWhiteSpace(tablename)) { return ""; }
            var _wheres = this._ToPart4Wheres(conditionParameters);

            var _reuslt = $"select count(1) from [{tablename}] {_wheres};";
            return _reuslt;
        }

        /// <summary>
        /// Get:创建单表的查询语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="conditionParameters">条件参数的迭代</param>
        /// <returns></returns>
        public string CreateSQL4Query(string tablename, IEnumerable<DbParameter> conditionParameters)
        {
            if (string.IsNullOrWhiteSpace(tablename)) { return ""; }
            var _wheres = this._ToPart4Wheres(conditionParameters);

            var _reuslt = $"select * from [{tablename}] {_wheres};";
            return _reuslt;
        }

        /// <summary>
        /// Get:创建单表的查询语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="columns">查询列名称列表</param>
        /// <param name="conditionParameters">条件参数的迭代</param>
        /// <returns></returns>
        public string CreateSQL4Query(string tablename, IEnumerable<string> columns, IEnumerable<DbParameter> conditionParameters)
        {
            if (string.IsNullOrWhiteSpace(tablename)) { return ""; }

            var _columns = this._ToPart4Columns(columns);
            var _wheres = this._ToPart4Wheres(conditionParameters);

            var _reuslt = $"select {_columns} from [{tablename}]{_wheres};";
            return _reuslt;
        }

        #endregion

        #region Functions.ORM.Delete

        /// <summary>
        /// Get:创建单表的删除语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="conditionParameters">条件参数的迭代</param>
        /// <returns></returns>
        public string CreateSQL4Delete(string tablename, IEnumerable<DbParameter> conditionParameters)
        {
            if (string.IsNullOrWhiteSpace(tablename)) { return ""; }
            var _wheres = this._ToPart4Wheres(conditionParameters);

            var _reuslt = $"delete from [{tablename}] {_wheres};";
            return _reuslt;
        }

        #endregion

        #region Functions.ORM.Update

        /// <summary>
        /// Get:创建单表的更新语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="updateParameters">更新数据参数的迭代</param>
        /// <param name="conditionParameters">条件参数的迭代</param>
        /// <returns></returns>
        public string CreateSQL4Update(string tablename, IEnumerable<DbParameter> updateParameters, IEnumerable<DbParameter> conditionParameters)
        {
            if (string.IsNullOrWhiteSpace(tablename)) { return ""; }
            var _updates = "";

            #region Updates
            {
                if (updateParameters == null) { return ""; }

                var _db = this.GetDbOperator();
                List<ColumnName> ColumnNames = _db.QueryList<ColumnName>("select  syscolumns.name  from sysobjects inner join syscolumns on sysobjects.id=syscolumns.id where sysobjects.name='" + tablename + "';");
                foreach (var x in updateParameters)
                {
                    if (x == null || (x != null && ColumnNames.Where(a => a.name == x.ParameterName).Count() == 0) || (x != null && ColumnNames.Where(a => a.name.Equals(x.SourceColumn, StringComparison.OrdinalIgnoreCase)).Count() == 0))
                    { continue; }
                    if (string.IsNullOrWhiteSpace(x.ParameterName)) { continue; }
                    if (string.IsNullOrWhiteSpace(x.SourceColumn)) { continue; }

                    _updates += $"[{x.SourceColumn}]=@{x.ParameterName},";
                }
                _updates = _updates.TrimEnd(',');
            }
            #endregion

            if (string.IsNullOrWhiteSpace(_updates)) { return ""; }

            var _wheres = this._ToPart4Wheres(conditionParameters);
            var _result = $"update [{tablename}] set {_updates} {_wheres};";
            return _result;

        }

        #endregion

        #region Functions.ORM.Insert

        /// <summary>
        /// Get:创建单表的新增语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="dbParameters">新增数据参数的迭代</param>
        /// <returns></returns>
        public string CreateSQL4Insert(string tablename, IEnumerable<DbParameter> dbParameters)
        {
            if (string.IsNullOrWhiteSpace(tablename)) { return ""; }

            var _columns = this._ToPart4InsertColumns(dbParameters, tablename);
            var _values = this._ToPart4InsertValues(dbParameters, tablename);

            var _result = $"insert into [{tablename}] {_columns} values {_values};";
            return _result;
        }

        #endregion

        #endregion

    }
    /// <summary>
    /// 获取表格名称实体
    /// </summary>
    /// <summary>
    /// EquipItemInfo
    /// </summary>
    [Serializable]
    public class ColumnName
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string name { get; set; }
    }
}

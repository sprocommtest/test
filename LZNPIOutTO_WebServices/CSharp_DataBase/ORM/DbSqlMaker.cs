using CSharp_DataBase;
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

namespace SwiftMES.IIL.Client
{

    /// <summary>
    /// SQL语句制造器
    /// </summary>
    public partial class DbSqlMaker 
    {

        #region Functions.Privates

        /// <summary>
        /// Get:转换为查询列的语句部份
        /// </summary>
        /// <param name="args">内容列表</param>
        /// <param name="valueIfEmpty">当得到空值时返回的默认值</param>
        /// <returns></returns>
        private string _ToPart4Columns(IEnumerable<string> args, string valueIfEmpty = "*")
        {
            var _columns = "";

            #region Columns
            {
                if (args != null)
                {
                    foreach (var x in args)
                    {
                        var _column = x.Trim();
                        if (string.IsNullOrWhiteSpace(_column)) { continue; }
                        _columns += $"{_column},";
                    }
                    _columns = _columns.TrimEnd(',');
                }
                if (string.IsNullOrWhiteSpace(_columns)) { _columns = valueIfEmpty; }
            }
            #endregion

            return _columns;
        }

        /// <summary>
        /// Get:转换为查询条件的语句部份
        /// </summary>
        /// <param name="args">内容列表</param>
        /// <returns></returns>
        private string _ToPart4Wheres(IEnumerable<string> args)
        {
            var _wheres = "";

            #region Wheres
            {
                if (args != null)
                {
                    foreach (var x in args)
                    {
                        var _condition = x.Trim();
                        if (string.IsNullOrWhiteSpace(_condition)) { continue; }
                        _wheres += $"and {_condition} ";
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

        /// <summary>
        /// Get:转换为分组的语句部份
        /// </summary>
        /// <param name="args">内容列表</param>
        /// <returns></returns>
        private string _ToPart4Groupby(IEnumerable<string> args)
        {
            var _groupbys = "";

            #region Group by
            {
                if (args != null)
                {
                    foreach (var x in args)
                    {
                        var _column = x.Trim();
                        if (string.IsNullOrWhiteSpace(_column)) { continue; }
                        _groupbys += $"{_column},";
                    }
                    _groupbys = _groupbys.TrimEnd(',');
                    if (!string.IsNullOrWhiteSpace(_groupbys))
                    {
                        _groupbys = $" group by {_groupbys} ";
                    }
                }
            }
            #endregion

            return _groupbys;
        }

        /// <summary>
        /// Get:转换为分组条件的语句部份
        /// </summary>
        /// <param name="args">内容列表</param>
        /// <returns></returns>
        private string _ToPart4Having(IEnumerable<string> args)
        {
            var _havings = "";

            #region Having
            {
                if (args != null)
                {
                    foreach (var x in args)
                    {
                        var _condition = x.Trim();
                        if (string.IsNullOrWhiteSpace(_condition)) { continue; }
                        _havings += $"and {_condition} ";
                    }
                    if (!string.IsNullOrWhiteSpace(_havings))
                    {
                        _havings = $" having {_havings.Remove(0, 3)} ";
                    }
                }
            }
            #endregion

            return _havings;
        }

        /// <summary>
        /// Get:转换为排序的语句部份
        /// </summary>
        /// <param name="args">内容列表</param>
        /// <returns></returns>
        private string _ToPart4Orderby(IEnumerable<string> args)
        {
            var _orderbys = "";

            #region Order by 
            {
                if (args != null)
                {
                    foreach (var x in args)
                    {
                        var _column = x.Trim();
                        if (string.IsNullOrWhiteSpace(_column)) { continue; }
                        _orderbys += $"{_column},";
                    }
                    _orderbys = _orderbys.TrimEnd(',');
                    if (!string.IsNullOrWhiteSpace(_orderbys))
                    {
                        _orderbys = $" order by {_orderbys} ";
                    }
                }
            }
            #endregion

            return _orderbys;
        }

        /// <summary>
        /// Get:转换为查询列的语句部份
        /// </summary>
        /// <param name="args">内容列表</param>
        /// <returns></returns>
        private string _ToPart4InsertColumns(IEnumerable<string> args)
        {
            var _columns = "";

            #region Columns
            {
                if (args != null)
                {
                    foreach (var x in args)
                    {
                        var _column = x.Trim();
                        if (string.IsNullOrWhiteSpace(_column)) { continue; }
                        _columns += $"[{_column}],";
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
        private string _ToPart4InsertValues(IEnumerable<string> args)
        {
            var _values = "";

            #region Columns
            {
                if (args != null)
                {
                    foreach (var x in args)
                    {
                        var _value = x.Trim();
                        if (string.IsNullOrWhiteSpace(_value))
                        {
                            _values += $"'{_value}',";
                            continue;
                        }
                        if (_value.StartsWith("@"))
                        {
                            _values += $"{_value},";
                            continue;
                        }
                        else
                        {
                            //函数
                            if (Regex.IsMatch(_value, @"[\S]+\([\S\s]*\)"))
                            {
                                _values += $"{_value},";
                                continue;
                            }
                            _values += $"'{_value}',";
                            continue;
                        }
                    }
                    _values = _values.TrimEnd(',');
                }
            }
            #endregion

            if (string.IsNullOrWhiteSpace(_values)) { return ""; }
            return $"({_values})";
        }

        #endregion

        #region Functions.Query

        /// <summary>
        /// Get:创建单表的查询语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="columns">查询列名称列表</param>
        /// <param name="whereConditions">条件列表</param>
        /// <param name="groupbys">分组列表</param>
        /// <param name="havingConditions">分组条件列表</param>
        /// <param name="orderbys">排序列表</param>
        /// <returns></returns>
        public string CreateSQL4Query(string tablename, IEnumerable<string> columns, IEnumerable<string> whereConditions
            , IEnumerable<string> groupbys = null, IEnumerable<string> havingConditions = null, IEnumerable<string> orderbys = null)
        {
            if (string.IsNullOrWhiteSpace(tablename)) { return ""; }

            var _columns = this._ToPart4Columns(columns);
            var _wheres = this._ToPart4Wheres(whereConditions);
            var _groupbys = this._ToPart4Groupby(groupbys);
            var _havings = string.IsNullOrWhiteSpace(_groupbys) ? "" : this._ToPart4Having(havingConditions);
            var _orderbys = this._ToPart4Orderby(orderbys);

            var _reuslt = $"select {_columns} from [{tablename}]{_wheres}{_groupbys}{_havings}{_orderbys};";
            return _reuslt;
        }

        /// <summary>
        /// Get:创建单表的查询语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="columns">查询列名称列表</param>
        /// <param name="whereConditions">条件列表</param>
        /// <param name="orderbys">排序列表</param>
        /// <returns></returns>
        public string CreateSQL4Query(string tablename, IEnumerable<string> columns, IEnumerable<string> whereConditions
            , IEnumerable<string> orderbys = null)
        {
            var _sql = this.CreateSQL4Query(tablename, columns, whereConditions, default(string[]), default(string[]), orderbys);
            return _sql;
        }

        /// <summary>
        /// Get:创建单表的查询语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="whereConditions">条件列表</param>
        /// <param name="orderbys">排序列表</param>
        /// <returns></returns>
        public string CreateSQL4Query(string tablename, IEnumerable<string> whereConditions, IEnumerable<string> orderbys = null)
        {
            var _sql = this.CreateSQL4Query(tablename, default(string[]), whereConditions, default(string[]), default(string[]), orderbys);
            return _sql;
        }

        /// <summary>
        /// Get:创建单表的查询语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="whereConditions">条件列表</param>
        /// <param name="orderby">排序</param>
        /// <returns></returns>
        public string CreateSQL4Query(string tablename, IEnumerable<string> whereConditions, string orderby)
        {
            var _sql = this.CreateSQL4Query(tablename, default(string[]), whereConditions, default(string[]), default(string[]), new string[] { orderby });
            return _sql;
        }

        #endregion

        #region Functions.Query.Paging

        /// <summary>
        /// Get:创建单表的查询语句
        /// </summary>
        /// <param name="paging">分页数据</param>
        /// <param name="tablename">表名称</param>
        /// <param name="columns">查询列名称列表</param>
        /// <param name="whereConditions">条件列表</param>
        /// <param name="groupbys">分组列表</param>
        /// <param name="havingConditions">分组条件列表</param>
        /// <param name="orderbys">排序列表</param>
        /// <returns></returns>
        public string CreateSQL4Paging(Paging paging, string tablename, IEnumerable<string> columns, IEnumerable<string> whereConditions
            , IEnumerable<string> groupbys = null, IEnumerable<string> havingConditions = null, IEnumerable<string> orderbys = null)
        {
            var _sql = this.CreateSQL4Query(tablename, columns, whereConditions, groupbys, havingConditions, orderbys);
            return this.CreateSQL4Paging(paging, _sql);
        }

        /// <summary>
        /// Get:创建单表的查询语句
        /// </summary>
        /// <param name="paging">分页数据</param>
        /// <param name="tablename">表名称</param>
        /// <param name="columns">查询列名称列表</param>
        /// <param name="whereConditions">条件列表</param>
        /// <param name="orderbys">排序列表</param>
        /// <returns></returns>
        public string CreateSQL4Paging(Paging paging, string tablename, IEnumerable<string> columns, IEnumerable<string> whereConditions
            , IEnumerable<string> orderbys)
        {
            var _sql = this.CreateSQL4Query(tablename, columns, whereConditions, orderbys);
            return this.CreateSQL4Paging(paging, _sql);
        }

        /// <summary>
        /// Get:创建单表的查询语句
        /// </summary>
        /// <param name="paging">分页数据</param>
        /// <param name="tablename">表名称</param>
        /// <param name="whereConditions">条件列表</param>
        /// <param name="orderbys">排序列表</param>
        /// <returns></returns>
        public string CreateSQL4Paging(Paging paging, string tablename, IEnumerable<string> whereConditions, IEnumerable<string> orderbys = null)
        {
            var _sql = this.CreateSQL4Query(tablename, whereConditions, orderbys);
            return this.CreateSQL4Paging(paging, _sql);
        }

        /// <summary>
        /// Get:创建单表的查询语句
        /// </summary>
        /// <param name="paging">分页数据</param>
        /// <param name="tablename">表名称</param>
        /// <param name="whereConditions">条件列表</param>
        /// <param name="orderby">排序</param>
        /// <returns></returns>
        public string CreateSQL4Paging(Paging paging, string tablename, IEnumerable<string> whereConditions, string orderby)
        {
            var _sql = this.CreateSQL4Query(tablename, whereConditions, orderby);
            return this.CreateSQL4Paging(paging, _sql);
        }

        /// <summary>
        /// Get:创建分页的查询语句
        /// </summary>
        /// <param name="sql">表名称</param>
        /// <param name="paging">分页数据</param>
        /// <returns></returns>
        public string CreateSQL4Paging(Paging paging, string sql)
        {
            if (paging.IsEmpty) { return sql; }
            sql = (sql ?? "").Trim(';');
            if (string.IsNullOrWhiteSpace(sql)) { return ""; }

            var _offset = (paging.PageNumber - 1) * paging.PageSize;
            var _offset2 = _offset + paging.PageSize;
            //var _size = paging.PageSize;

            //const string C_REGEX_FORMULA_ORDERBY = @"[\s]*order[\s]+by[\s]+[\S]+";
            //if(Regex.IsMatch(sql, C_REGEX_FORMULA_ORDERBY, RegexOptions.IgnoreCase))
            //{
            //    return $@"{sql} offset {_offset} rows fetch next {_size} rows only;";
            //}

            //return $@"select * from({sql}) as t order by 1 offset {_offset} rows fetch next {_size} rows only;";

            return $@"
 declare @i int;
 set @i=1;
 select * from 
 (select ROW_NUMBER() OVER(ORDER BY @i desc) as counts,*  from ({sql}) as t) as a
 where counts between {_offset} and {_offset2};";
        }

        #endregion

        #region Functions.Query.Count

        /// <summary>
        /// Get:创建查询数据总量的查询语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="whereConditions">条件列表</param>
        /// <param name="groupbys">分组列表</param>
        /// <param name="havingConditions">分组条件列表</param>
        /// <returns></returns>
        public string CreateSQL4Count(string tablename, IEnumerable<string> whereConditions
            , IEnumerable<string> groupbys = null, IEnumerable<string> havingConditions = null)
        {
            var _sql = this.CreateSQL4Query(tablename, new string[] { "count(1)" }, whereConditions, groupbys, havingConditions, default(string[]));
            return _sql;
        }

        /// <summary>
        /// Get:创建查询数据总量的查询语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public string CreateSQL4Count(string sql)
        {
            var _sql = sql.Trim(';');
            var _reuslt = $"select count(1) from ({_sql}) as t;";
            return _reuslt;
        }

        #endregion

        #region Functions.Delete

        /// <summary>
        /// Get:创建单表的删除语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="whereConditions">条件列表</param>
        /// <returns></returns>
        public string CreateSQL4Delete(string tablename, IEnumerable<string> whereConditions)
        {
            if (string.IsNullOrWhiteSpace(tablename)) { return ""; }
            var _wheres = this._ToPart4Wheres(whereConditions);

            var _reuslt = $"delete from [{tablename}] {_wheres};";
            return _reuslt;
        }

        #endregion

        #region Functions.Update

        /// <summary>
        /// Get:创建单表的更新语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="updates">更新数据的迭代</param>
        /// <param name="whereConditions">条件列表</param>
        /// <returns></returns>
        public string CreateSQL4Update(string tablename, IEnumerable<KeyValuePair<string, string>> updates, IEnumerable<string> whereConditions)
        {
            if (string.IsNullOrWhiteSpace(tablename)) { return ""; }
            var _updates = "";

            #region Updates
            {
                if (updates == null) { return ""; }

                foreach (var kv in updates)
                {
                    var _column = kv.Key.Trim();
                    var _value = kv.Value.Trim();
                    if (string.IsNullOrWhiteSpace(_column)) { continue; }
                    if (_value.StartsWith("@"))
                    {
                        _updates += $"[{_column}]={_value},";
                    }
                    else
                    {
                        _updates += $"[{_column}]='{_value}',";
                    }
                }
                _updates = _updates.TrimEnd(',');
            }
            #endregion

            if (string.IsNullOrWhiteSpace(_updates)) { return ""; }

            var _wheres = this._ToPart4Wheres(whereConditions);
            var _result = $"update [{tablename}] set {_updates} {_wheres};";
            return _result;
        }

        #endregion


        #region Functions.Insert

        /// <summary>
        /// Get:创建单表的新增语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="inserts">新增数据的迭代</param>
        /// <returns></returns>
        public string CreateSQL4Insert(string tablename, IEnumerable<KeyValuePair<string, string>> inserts)
        {
            if (string.IsNullOrWhiteSpace(tablename)) { return ""; }
            var _columns = from kv in inserts
                           select kv.Key;
            var _values = from kv in inserts
                          select kv.Value;
            return this.CreateSQL4Insert(tablename, _columns, _values);
        }

        /// <summary>
        /// Get:创建单表的新增语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="columns">新增数据的列名称列表</param>
        /// <param name="values">新增的数据值迭代</param>
        /// <returns></returns>
        public string CreateSQL4Insert(string tablename, IEnumerable<string> columns, IEnumerable<string> values)
        {
            if (string.IsNullOrWhiteSpace(tablename)) { return ""; }
            if (values == null) { return ""; }

            var _columns = this._ToPart4InsertColumns(columns);
            var _values = this._ToPart4InsertValues(values);
            if (string.IsNullOrWhiteSpace(_values)) { return ""; }

            var _result = $"insert into [{tablename}] {_columns} values {_values};";
            return _result;
        }

        /// <summary>
        /// Get:创建单表的新增语句
        /// </summary>
        /// <param name="tablename">表名称</param>
        /// <param name="columns">新增数据的列名称列表</param>
        /// <param name="valuesList">新增的数据值列表迭代</param>
        /// <returns></returns>
        public string CreateSQL4Insert<TValues>(string tablename, IEnumerable<string> columns, IEnumerable<TValues> valuesList)
            where TValues : IEnumerable<string>
        {
            if (string.IsNullOrWhiteSpace(tablename)) { return ""; }
            if (valuesList == null) { return ""; }

            var _columns = this._ToPart4InsertColumns(columns);

            var _values = "";
            foreach (var _item in valuesList)
            {
                if (_item == null) { continue; }
                var _subvalues = this._ToPart4InsertValues(_item);
                if (string.IsNullOrWhiteSpace(_subvalues)) { continue; }
                _values += $"{_subvalues},";
            }
            _values = _values.Trim(',');

            var _result = $"insert into [{tablename}] {_columns} values {_values};";
            return _result;
        }

        #endregion


    }

}

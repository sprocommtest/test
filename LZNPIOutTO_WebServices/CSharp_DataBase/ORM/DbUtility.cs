using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftMES.Utility.Core
{

    /// <summary>
    /// 工具类：数据库工具类
    /// </summary>
    public static class DbUtility
    {

        #region Functions.Command

        /// <summary>
        /// Get:创建SQL命令
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public static IDbCommand CrateCommand(IDbConnection connection, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (connection == null) { return null; }

            var _cmd = connection.CreateCommand();
            _cmd.Connection = connection;
            _cmd.CommandText = sql;
            _cmd.CommandType = cmdType;

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (p == null) { continue; }
                    _cmd.Parameters.Add(p);
                }
            }
            return _cmd;
        }

        /// <summary>
        /// Get:创建SQL命令
        /// </summary>
        /// <param name="dbTrans">事务</param>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public static IDbCommand CrateCommand(ref IDbTransaction dbTrans, IDbConnection connection, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (connection == null) { return null; }

            var _cmd = connection.CreateCommand();
            _cmd.Connection = connection;
            _cmd.CommandText = sql;
            _cmd.CommandType = cmdType;
            _cmd.Transaction = dbTrans;

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (p == null) { continue; }
                    _cmd.Parameters.Add(p);
                }
            }
            return _cmd;
        }

        /// <summary>
        /// Get:创建SQL命令
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public static IDbCommand CrateCommand(IDbConnection connection, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            return CrateCommand(connection, "", parameters, cmdType);
        }

        /// <summary>
        /// Get:创建SQL命令
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>受影响的行数</returns>
        public static IDbCommand CrateCommandByProc(IDbConnection connection, string procName, IEnumerable<DbParameter> parameters = null)
        {
            return CrateCommand(connection, procName, parameters, CommandType.StoredProcedure);
        }

        #endregion

        #region Functions.Execute.001

        /// <summary>
        /// Get:执行SQL语句
        /// </summary>
        /// <param name="cmd">SQL执行命令</param>
        /// <returns>受影响的行数</returns>
        public static int Execute(IDbCommand cmd)
        {
            //if(trans == null) { return 0; }
            if (cmd == null) { return 0; }
            if (string.IsNullOrWhiteSpace(cmd.CommandText)) { return 0; }

            #region Connect
            {
                if (cmd.Connection == null) { return 0; }
                if (string.IsNullOrWhiteSpace(cmd.Connection.ConnectionString)) { return 0; }

                var _connection = cmd.Connection;
                switch (_connection.State)
                {
                    case ConnectionState.Closed:
                    case ConnectionState.Broken:
                        {
                            _connection.Open();
                        }
                        break;
                }
            }
            #endregion

            #region Execute
            {
                var _count = cmd.ExecuteNonQuery();
                //cmd.Parameters.Clear();
                return _count;
            }
            #endregion
        }

        #endregion

        #region Functions.Execute.Transaction

        /// <summary>
        /// Get:执行SQL语句
        /// </summary>
        /// <param name="trans">数据库连接</param>
        /// <param name="cmd">SQL执行命令</param>
        /// <returns>受影响的行数</returns>
        public static int Execute(IDbTransaction trans, IDbCommand cmd)
        {
            //if(trans == null) { return 0; }
            if (cmd == null) { return 0; }
            if (string.IsNullOrWhiteSpace(cmd.CommandText)) { return 0; }

            #region Connect
            {
                if (cmd.Connection == null) { return 0; }
                if (string.IsNullOrWhiteSpace(cmd.Connection.ConnectionString)) { return 0; }

                var _connection = cmd.Connection;
                switch (_connection.State)
                {
                    case ConnectionState.Closed:
                    case ConnectionState.Broken:
                        {
                            _connection.Open();
                        }
                        break;
                }
            }
            #endregion

            #region Execute

            if (trans != null)
            {
                try
                {
                    cmd.Transaction = trans;
                    var _count = cmd.ExecuteNonQuery();
                    //trans.Commit();
                    //cmd.Parameters.Clear();
                    return _count;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    trans.Dispose();
                    //cmd.Parameters.Clear();
                }
                return -1;
            }
            else
            {
                var _count = cmd.ExecuteNonQuery();
                //cmd.Parameters.Clear();
                return _count;
            }

            #endregion
        }

        /// <summary>
        /// Get:通过事务执行SQL语句
        /// </summary>
        /// <param name="trans">事务</param>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public static int ExecuteByTransaction(ref IDbTransaction trans,
            IDbConnection connection, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (connection == null) { return 0; }
            if (string.IsNullOrWhiteSpace(connection.ConnectionString)) { return 0; }
            if (string.IsNullOrWhiteSpace(sql)) { return 0; }

            using (var _cmd = connection.CreateCommand())
            {
                _cmd.Connection = connection;
                _cmd.CommandText = sql;
                _cmd.CommandType = cmdType;

                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        if (p == null) { continue; }
                        _cmd.Parameters.Add(p);
                    }
                }

                if (trans == null) { trans = connection.BeginTransaction(); }
                _cmd.Transaction = trans;

                return Execute(trans, _cmd);
            }
        }

        #endregion

        #region Functions.Execute.003

        /// <summary>
        /// Get:执行SQL语句
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="isTransaction">是否使用事务</param>
        /// <param name="dict">执行的字典</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public static int Execute(IDbConnection connection, bool isTransaction, Dictionary<string, IEnumerable<DbParameter>> dict = null, CommandType cmdType = CommandType.Text)
        {
            if (connection == null) { return 0; }
            if (string.IsNullOrWhiteSpace(connection.ConnectionString)) { return 0; }
            if (dict == null) { return 0; }
            if (dict.Count < 1) { return 0; }

            using (var _cmd = connection.CreateCommand())
            {
                _cmd.Connection = connection;
                _cmd.CommandType = cmdType;

                switch (connection.State)
                {
                    case ConnectionState.Closed:
                    case ConnectionState.Broken:
                        {
                            connection.Open();
                        }
                        break;
                }

                if (isTransaction)
                {
                    using (var _trans = connection.BeginTransaction())
                    {
                        try
                        {
                            _cmd.Transaction = _trans;

                            var _count = 0;
                            foreach (var kv in dict)
                            {
                                var _sql = kv.Key;
                                var _pars = kv.Value;
                                if (string.IsNullOrWhiteSpace(_sql)) { continue; }

                                if (_pars != null)
                                {
                                    foreach (var p in _pars)
                                    {
                                        if (p == null) { continue; }
                                        _cmd.Parameters.Add(p);
                                    }
                                }

                                _cmd.CommandText = _sql;
                                _count += _cmd.ExecuteNonQuery();
                            }
                            _trans.Commit();
                            _cmd.Parameters.Clear();
                            return _count;
                        }
                        catch //(Exception ex)
                        {
                            _trans.Rollback();
                            _cmd.Parameters.Clear();
                        }
                        return -1;
                    }
                }
                else
                {
                    var _count = 0;
                    foreach (var kv in dict)
                    {
                        var _sql = kv.Key;
                        var _pars = kv.Value;
                        if (string.IsNullOrWhiteSpace(_sql)) { continue; }

                        if (_pars != null)
                        {
                            foreach (var p in _pars)
                            {
                                if (p == null) { continue; }
                                _cmd.Parameters.Add(p);
                            }
                        }

                        _cmd.CommandText = _sql;
                        _count += _cmd.ExecuteNonQuery();
                    }
                    _cmd.Parameters.Clear();
                    return _count;
                }
            }
        }

        /// <summary>
        /// Get:执行SQL语句
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="isTransaction">是否使用事务</param>
        /// <param name="sqls">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public static int Execute(IDbConnection connection, bool isTransaction, IEnumerable<string> sqls, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (connection == null) { return 0; }
            if (string.IsNullOrWhiteSpace(connection.ConnectionString)) { return 0; }
            if (sqls == null) { return 0; }
            if (sqls.Count() < 1) { return 0; }

            using (var _cmd = connection.CreateCommand())
            {
                _cmd.Connection = connection;
                _cmd.CommandType = cmdType;

                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        if (p == null) { continue; }
                        _cmd.Parameters.Add(p);
                    }
                }

                switch (connection.State)
                {
                    case ConnectionState.Closed:
                    case ConnectionState.Broken:
                        {
                            connection.Open();
                        }
                        break;
                }

                if (isTransaction)
                {
                    using (var _trans = connection.BeginTransaction())
                    {
                        try
                        {
                            _cmd.Transaction = _trans;

                            var _count = 0;
                            foreach (var _sql in sqls)
                            {
                                if (string.IsNullOrWhiteSpace(_sql)) { continue; }
                                _cmd.CommandText = _sql;
                                _count += _cmd.ExecuteNonQuery();
                            }
                            _trans.Commit();
                            _cmd.Parameters.Clear();
                            return _count;
                        }
                        catch //(Exception ex)
                        {
                            _trans.Rollback();
                            _cmd.Parameters.Clear();
                        }
                        return -1;
                    }
                }
                else
                {
                    var _count = 0;
                    foreach (var _sql in sqls)
                    {
                        if (string.IsNullOrWhiteSpace(_sql)) { continue; }
                        _cmd.CommandText = _sql;
                        _count += _cmd.ExecuteNonQuery();
                    }
                    _cmd.Parameters.Clear();
                    return _count;
                }
            }
        }

        /// <summary>
        /// Get:执行SQL语句
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="isTransaction">是否使用事务</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public static int Execute(IDbConnection connection, bool isTransaction, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (connection == null) { return 0; }
            if (string.IsNullOrWhiteSpace(connection.ConnectionString)) { return 0; }
            if (string.IsNullOrWhiteSpace(sql)) { return 0; }

            using (var _cmd = connection.CreateCommand())
            {
                _cmd.Connection = connection;
                _cmd.CommandText = sql;
                _cmd.CommandType = cmdType;
                _cmd.CommandTimeout = 300;
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        if (p == null) { continue; }
                        _cmd.Parameters.Add(p);
                    }
                }

                if (isTransaction)
                {
                    using (var _trans = connection.BeginTransaction())
                    {
                        try
                        {
                            _cmd.Transaction = _trans;
                            var _count = _cmd.ExecuteNonQuery();
                            _trans.Commit();
                            _cmd.Parameters.Clear();
                            return _count;
                        }
                        catch (Exception ex)
                        {
                            _trans.Rollback();
                            _cmd.Parameters.Clear();
                        }
                        return -1;
                    }
                }
                else
                {
                    var _count = _cmd.ExecuteNonQuery();
                    _cmd.Parameters.Clear();
                    return _count;
                }
            }
        }

        /// <summary>
        /// Get:执行SQL语句
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public static int Execute(IDbConnection connection, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            return Execute(connection, false, sql, parameters, cmdType);
        }

        /// <summary>
        /// Get:执行存储过程
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>受影响的行数</returns>
        public static int ExecuteByProc(IDbConnection connection, string procName, IEnumerable<DbParameter> parameters = null)
        {
            return Execute(connection, false, procName, parameters, CommandType.StoredProcedure);
        }

        #endregion

        #region Functions.Reader

        /// <summary>
        /// Get:查询SQL语句
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <param name="cmdBehavior">命令说明</param>
        /// <returns>数据读取器</returns>
        public static IDataReader QueryReader(IDbConnection connection, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text
            , CommandBehavior cmdBehavior = CommandBehavior.CloseConnection)
        {
            if (connection == null) { return null; }
            if (string.IsNullOrWhiteSpace(sql)) { return null; }

            using (var _cmd = connection.CreateCommand())
            {
                _cmd.Connection = connection;
                _cmd.CommandText = sql;
                _cmd.CommandType = cmdType;

                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        if (p == null) { continue; }
                        _cmd.Parameters.Add(p);
                    }
                }

                var _reader = _cmd.ExecuteReader(cmdBehavior);
                _cmd.Parameters.Clear();
                return _reader;
                //return _cmd.ExecuteReader(cmdBehavior);
            }
        }

        /// <summary>
        /// Get:查询SQL语句
        /// </summary>
        /// <param name="dbTrans">事务</param>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <param name="cmdBehavior">命令说明</param>
        /// <returns>数据读取器</returns>
        public static IDataReader QueryReader(ref IDbTransaction dbTrans, IDbConnection connection, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text
            , CommandBehavior cmdBehavior = CommandBehavior.CloseConnection)
        {

            //if(connection == null) { return 0; }
            //if(string.IsNullOrWhiteSpace(connection.ConnectionString)) { return 0; }
            //if(string.IsNullOrWhiteSpace(sql)) { return 0; }

            //using(var _cmd = connection.CreateCommand())
            //{
            //    _cmd.Connection = connection;
            //    _cmd.CommandText = sql;
            //    _cmd.CommandType = cmdType;

            //    if(parameters != null)
            //    {
            //        foreach(var p in parameters)
            //        {
            //            if(p == null) { continue; }
            //            _cmd.Parameters.Add(p);
            //        }
            //    }

            //    if(trans == null) { trans = connection.BeginTransaction(); }
            //    _cmd.Transaction = trans;

            //    return Execute(trans, _cmd);
            //}


            if (connection == null) { return null; }
            if (string.IsNullOrWhiteSpace(connection.ConnectionString)) { return null; }
            if (string.IsNullOrWhiteSpace(sql)) { return null; }

            using (var _cmd = connection.CreateCommand())
            {
                _cmd.Connection = connection;
                _cmd.CommandText = sql;
                _cmd.CommandType = cmdType;
                //   _cmd.Transaction = dbTrans;
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        if (p == null) { continue; }
                        _cmd.Parameters.Add(p);
                    }
                }
                if (dbTrans == null) { dbTrans = connection.BeginTransaction(); }
                _cmd.Transaction = dbTrans;
                var _reader = _cmd.ExecuteReader(cmdBehavior);
                _cmd.Parameters.Clear();
                return _reader;
                //return _cmd.ExecuteReader(cmdBehavior);
            }



        }

        /// <summary>
        /// Get:查询存储过程
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdBehavior">命令说明</param>
        /// <returns>数据读取器</returns>
        public static IDataReader QueryReaderByProc(IDbConnection connection, string procName, IEnumerable<DbParameter> parameters = null
            , CommandBehavior cmdBehavior = CommandBehavior.CloseConnection)
        {
            return QueryReader(connection, procName, parameters, CommandType.StoredProcedure, cmdBehavior);
        }

        #endregion


    }

}

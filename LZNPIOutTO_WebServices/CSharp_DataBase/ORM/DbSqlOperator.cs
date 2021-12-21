using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Configuration;
using SwiftMES.Utility.Core;
using CSharp_DataBase;

namespace SwiftMES.IIL.Client
{

    /// <summary>
    /// 数据库操作类
    /// </summary>
    public partial class DbSqlOperator 
    {

        #region Properties

        /// <summary> G/S:数据库连接串 </summary>
        public string ConnectionString { get; set; }

        /// <summary> Get:[常量]无效的执行影响数 </summary>
        public const int C_INVALID_EXECUTE_COUNT = 0;

       // public MySecurity security = new MySecurity();
        #endregion

        #region Constructors

        /// <summary>
        /// 创建数据库操作类
        /// </summary>
        public DbSqlOperator()
        {
           // string aa = MySecurity.Encrypt(ConfigurationManager.ConnectionStrings["ConnStr"].ConnectionString);
            string sqlserver = MySecurity.Decrypt(ConfigurationManager.ConnectionStrings["ConnStr"].ConnectionString);
            this.ConnectionString = sqlserver;
        }

        #endregion

        #region Functions.Connection

        /// <summary>
        /// Get:获取数据库连接串
        /// </summary>
        /// <returns></returns>
        public string GetConnectionString()
        {
            return this.ConnectionString;
        }

        /// <summary>
        /// Get:设置数据库连接串
        /// </summary>
        /// <param name="connectionstring">数据库连接串</param>
        /// <returns></returns>
        public void SetConnectionString(string connectionstring)
        {
            this.ConnectionString = connectionstring;
        }

        /// <summary>
        /// Get:创建数据库连接
        /// </summary>
        /// <returns></returns>
        public IDbConnection CreateConnection()
        {
            var _connectionString = this.ConnectionString;
            if (string.IsNullOrWhiteSpace(_connectionString)) { return null; }
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Get:创建数据库连接
        /// </summary>
        /// <param name="connectionstring">数据库连接串</param>
        /// <returns></returns>
        public IDbConnection CreateConnection(string connectionstring)
        {
            if (string.IsNullOrWhiteSpace(connectionstring)) { return null; }
            return new SqlConnection(connectionstring);
        }

        #endregion

        #region Functions.Others

        /// <summary>
        /// Get:检查能否连接到数据库的连接
        /// </summary>
        /// <returns>受影响的行数</returns>
        public bool CheckConnection()
        {
            var _connectionString = $"{this.ConnectionString};Connection Timeout=3;";
            var _connection = this.CreateConnection(_connectionString);
            using (_connection)
            {
                try
                {
                    //只等待3秒
                    var _wait = new ManualResetEvent(false);
                    bool _runok = false;
                    var _task = new Task(new Action(() =>
                    {
                        try
                        {
                            _connection.Open();
                            _runok = true;
                        }
                        catch (Exception)
                        {
                            _runok = false;
                        }
                        _wait.Set();
                    }));
                    _task.Start();
                    _wait.WaitOne(3000);
                    return _runok;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        #endregion

        #region Functions.Action.Transaction

        /// <summary>
        /// Get:通过事务进行操作
        /// </summary>
        /// <param name="action">操作函数</param>
        /// <returns>受影响的行数</returns>
        public DbResult<bool> ActionByTransaction(Func<IDbTransaction, DbResult<bool>> action)
        {
            if (action == null) { return new DbResult<bool>(false); }

            IDbTransaction _dbTrans = null;
            try
            {
                using (var _connection = this.CreateConnection())
                {
                    _connection.Open();
                    _dbTrans = _connection.BeginTransaction();

                    var _result = action(_dbTrans);
                    if (_result == null || !_result.IsSucceed)
                    {
                        if (_dbTrans?.Connection != null)
                        {
                            _dbTrans.Rollback();
                            _dbTrans.Dispose();
                        }
                        return _result;
                    }

                    _dbTrans.Commit();
                    _dbTrans.Dispose();
                    return _result;
                }
            }
            catch (Exception ex)
            {
                if (_dbTrans?.Connection != null)
                {
                    _dbTrans.Rollback();
                    _dbTrans.Dispose();
                }
                return new DbResult<bool>(false, ex.Message);
            }
        }

        #endregion

        #region Functions.Execute.Transaction

        /// <summary>
        /// Get:通过事务执行SQL语句
        /// </summary>
        /// <param name="trans">事务</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public int ExecuteByTransaction(ref IDbTransaction trans,
            string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (string.IsNullOrWhiteSpace(sql)) { return C_INVALID_EXECUTE_COUNT; }

            var _result = DbUtility.ExecuteByTransaction(ref trans, trans.Connection, sql, parameters, cmdType);
            return _result;
        }

        #endregion

        #region Functions.Execute

        /// <summary>
        /// Get:执行SQL语句
        /// </summary>
        /// <param name="isTransaction">是否使用事务</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public int Execute(bool isTransaction, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (string.IsNullOrWhiteSpace(sql)) { return C_INVALID_EXECUTE_COUNT; }
            var _connection = this.CreateConnection();
            using (_connection)
            {
                _connection.Open();
                var _result = DbUtility.Execute(_connection, isTransaction, sql, parameters, cmdType);
                return _result;
            }
        }

        /// <summary>
        /// Get:执行SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public int Execute(string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (string.IsNullOrWhiteSpace(sql)) { return C_INVALID_EXECUTE_COUNT; }
            var _connection = this.CreateConnection();
            using (_connection)
            {
                _connection.Open();
                var _result = DbUtility.Execute(_connection, sql, parameters, cmdType);
                return _result;
            }
        }

        /// <summary>
        /// 添加大数据 100w-8s
        /// </summary>
        /// <param name="Table"></param>
        /// <returns></returns>
        public bool InsertByDataTable(DataTable Table)
        {
            var _connectionString = this.ConnectionString;
            if (string.IsNullOrWhiteSpace(_connectionString)) { return false; }
            if (Table == null || string.IsNullOrWhiteSpace(Table.TableName) || (Table != null && Table.Rows.Count == 0)) { return false; }
            using (SqlConnection _connection = new SqlConnection(_connectionString))
            {
                Stopwatch sw = new Stopwatch();
                SqlBulkCopy bulkCory = new SqlBulkCopy(_connection);
                bulkCory.DestinationTableName = Table.TableName;
                bulkCory.BatchSize = Table.Rows.Count;
                _connection.Open();
                sw.Start();
                bulkCory.WriteToServer(Table);
                sw.Stop();
                return true;
            }
        }

        /// <summary>
        /// 添加大数据 100w-8s
        /// </summary>
        /// <param name="Table"></param>
        /// <returns></returns>
        public ActionReturn InsertByDataTable(DataTable Table, IDbTransaction dbTrans)
        {
            if (dbTrans == null) { return new ActionReturn() { IsSucceed = false, Message = "事务为空" }; }
            if (Table == null || string.IsNullOrWhiteSpace(Table.TableName) || (Table != null && Table.Rows.Count == 0)) { return new ActionReturn() { IsSucceed = false, Message = "数据为空" }; }
            SqlConnection _connection = dbTrans.Connection as SqlConnection;
            SqlBulkCopy bulkCory = new SqlBulkCopy(_connection, SqlBulkCopyOptions.Default, dbTrans as SqlTransaction);
            bulkCory.DestinationTableName = Table.TableName;
            bulkCory.BatchSize = Table.Rows.Count;
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
            bulkCory.WriteToServer(Table);
            return new ActionReturn() { IsSucceed = true, Message = "数据添加成功", Result = dbTrans }; ;

        }



        /// <summary>
        /// Get:执行SQL语句
        /// </summary>
        /// <param name="isTransaction">是否使用事务</param>
        /// <param name="sqls">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public int Execute(bool isTransaction, IEnumerable<string> sqls, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            var _connection = this.CreateConnection();
            using (_connection)
            {
                _connection.Open();
                var _result = DbUtility.Execute(_connection, isTransaction, sqls, parameters, cmdType);
                return _result;
            }
        }

        /// <summary>
        /// Get:执行SQL语句
        /// </summary>
        /// <param name="sqls">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public int Execute(IEnumerable<string> sqls, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            var _connection = this.CreateConnection();
            using (_connection)
            {
                _connection.Open();
                var _result = DbUtility.Execute(_connection, false, sqls, parameters, cmdType);
                return _result;
            }
        }

        /// <summary>
        /// Get:执行存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>受影响的行数</returns>
        public int ExecuteByProc(string procName, IEnumerable<DbParameter> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(procName)) { return C_INVALID_EXECUTE_COUNT; }
            var _connection = this.CreateConnection();
            using (_connection)
            {
                _connection.Open();
                var _result = DbUtility.ExecuteByProc(_connection, procName, parameters);
                return _result;
            }
        }

        #endregion

        #region Functions.ExecuteAndReturn

        /// <summary>
        /// Get:执行SQL语句,并返回结果
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>首行首列的数据值</returns>
        public DbExecuteResult ExecuteAndReturn(string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            var _reader = this.QueryReader(sql, parameters, cmdType);
            if (_reader == null || !_reader.Read()) { return new DbExecuteResult(false, "获取执行结果数据失败！"); }

            var _msgID = _reader.GetInt32(0);
            var _msg = _reader.GetString(1);
            var _isSucceed = _msgID == 0;
            return new DbExecuteResult(_isSucceed, _msgID.ToString(), _msg);
        }

        /// <summary>
        /// Get:执行存储过程,并返回结果
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>首行首列的数据值</returns>
        public DbExecuteResult ExecuteByProcAndReturn(string procName, IEnumerable<DbParameter> parameters = null)
        {
            return this.ExecuteAndReturn(procName, parameters, CommandType.StoredProcedure);
        }

        #endregion

        #region Functions.Reader

        /// <summary>
        /// Get:查询SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <param name="cmdBehavior">命令说明</param>
        /// <returns>数据读取器</returns>
        public IDataReader QueryReader(string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text
            , CommandBehavior cmdBehavior = CommandBehavior.CloseConnection)
        {
            var _connection = this.CreateConnection();
            //using (_connection)
            {
                _connection.Open();
                var _result = DbUtility.QueryReader(_connection, sql, parameters, cmdType, cmdBehavior);
                return _result;
            }
        }

        /// <summary>
        /// Get:查询SQL语句
        /// </summary>
        /// <param name="dbTrans">事务</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <param name="cmdBehavior">命令说明</param>
        /// <returns>数据读取器</returns>
        public IDataReader QueryReader(ref IDbTransaction dbTrans, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text
            , CommandBehavior cmdBehavior = CommandBehavior.CloseConnection)
        {
            //var _connection = this.CreateConnection();
            //var _connection = dbTrans.Connection;
            //if(_connection.State != ConnectionState.Open)
            //{
            //    _connection.Open();
            //}
            ////using (_connection)
            //{
            //    //_connection.Open();
            //    var _result = DbUtility.QueryReader(ref dbTrans, _connection, sql, parameters, cmdType, cmdBehavior);
            //    return _result;
            //}
            if (string.IsNullOrWhiteSpace(sql)) { return null; }

            var _result = DbUtility.QueryReader(ref dbTrans, dbTrans.Connection, sql, parameters, cmdType, cmdBehavior);
            return _result;
        }

        /// <summary>
        /// Get:查询存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdBehavior">命令说明</param>
        /// <returns>数据读取器</returns>
        public IDataReader QueryReaderByProc(string procName, IEnumerable<DbParameter> parameters = null, CommandBehavior cmdBehavior = CommandBehavior.CloseConnection)
        {
            var _connection = this.CreateConnection();
            //using (_connection)
            {
                _connection.Open();
                var _result = DbUtility.QueryReaderByProc(_connection, procName, parameters, cmdBehavior);
                return _result;
            }
        }

        #endregion

        #region Functions.DataSet

        /// <summary>
        /// Get:查询SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns></returns>
        public DataSet QueryTables(string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (string.IsNullOrWhiteSpace(sql)) { return new DataSet(); }

            var _connection = this.CreateConnection();
            using (_connection)
            {
                using (var _cmd = DbUtility.CrateCommand(_connection, sql, parameters, cmdType))
                {
                    using (var _adapter = new SqlDataAdapter(_cmd as SqlCommand))
                    {
                        var _result = new DataSet();
                        _adapter.Fill(_result);
                        _cmd.Parameters.Clear();
                        return _result;
                    }
                }
            }
        }

        /// <summary>
        /// Get:查询SQL语句
        /// </summary>
        /// <param name="dbTrans">事务</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns></returns>
        public DataSet QueryTables(ref IDbTransaction dbTrans, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (string.IsNullOrWhiteSpace(sql)) { return new DataSet(); }

            //var _connection = this.CreateConnection();
            //using(_connection)

            var _connection = dbTrans.Connection;
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            {
                using (var _cmd = DbUtility.CrateCommand(ref dbTrans, _connection, sql, parameters, cmdType))
                {
                    using (var _adapter = new SqlDataAdapter(_cmd as SqlCommand))
                    {
                        var _result = new DataSet();
                        _adapter.Fill(_result);
                        _cmd.Parameters.Clear();
                        return _result;
                    }
                }
            }
        }

        /// <summary>
        /// Get:查询存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns></returns>
        public DataSet QueryTablesByProc(string procName, IEnumerable<DbParameter> parameters = null)
        {
            return this.QueryTables(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Get:查询存储过程
        /// </summary>
        /// <param name="dbTrans">事务</param>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns></returns>
        public DataSet QueryTablesByProc(ref IDbTransaction dbTrans, string procName, IEnumerable<DbParameter> parameters = null)
        {
            return this.QueryTables(ref dbTrans, procName, parameters, CommandType.StoredProcedure);
        }
        #endregion

        #region Functions.Table

        /// <summary>
        /// Get:查询SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns></returns>
        public DataTable QueryTable(string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (string.IsNullOrWhiteSpace(sql)) { return null; }

            var _connection = this.CreateConnection();
            using (_connection)
            {
                using (var _cmd = DbUtility.CrateCommand(_connection, sql, parameters, cmdType))
                {
                    using (var _adapter = new SqlDataAdapter(_cmd as SqlCommand))
                    {
                        var _result = new DataTable();
                        _adapter.Fill(_result);
                        _cmd.Parameters.Clear();
                        return _result;
                    }
                }
            }
        }

        /// <summary>
        /// Get:查询SQL语句
        /// </summary>
        /// <param name="dbTrans">事务</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns></returns>
        public DataTable QueryTable(ref IDbTransaction dbTrans, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (string.IsNullOrWhiteSpace(sql)) { return null; }

            //var _connection = this.CreateConnection();
            //using(_connection)

            var _connection = dbTrans.Connection;
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            {
                using (var _cmd = DbUtility.CrateCommand(ref dbTrans, _connection, sql, parameters, cmdType))
                {
                    using (var _adapter = new SqlDataAdapter(_cmd as SqlCommand))
                    {
                        var _result = new DataTable();
                        _adapter.Fill(_result);
                        _cmd.Parameters.Clear();
                        return _result;
                    }
                }
            }
        }

        /// <summary>
        /// Get:查询存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns></returns>
        public DataTable QueryTableByProc(string procName, IEnumerable<DbParameter> parameters = null)
        {
            return this.QueryTable(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Get:查询存储过程
        /// </summary>
        /// <param name="dbTrans">事务</param>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns></returns>
        public DataTable QueryTableByProc(ref IDbTransaction dbTrans, string procName, IEnumerable<DbParameter> parameters = null)
        {
            return this.QueryTable(ref dbTrans, procName, parameters, CommandType.StoredProcedure);
        }
        #endregion

        #region Functions.Single

        /// <summary>
        /// Get:查询SQL语句,并转换首条数据为数据实体
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>首条数据转换的数据实体</returns>
        public TEntity QuerySingle<TEntity>(string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
            where TEntity : class, new()
        {
            var _reader = this.QueryReader(sql, parameters, cmdType);
            if (_reader == null) { return default(TEntity); }
            using (_reader)
            {
                var _entity = this.ToEntity1st<TEntity>(_reader);
                return _entity;
            }
        }

        /// <summary>
        /// Get:查询SQL语句,并转换首条数据为数据实体
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="dbTrans">事务</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>首条数据转换的数据实体</returns>
        public TEntity QuerySingle<TEntity>(ref IDbTransaction dbTrans, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
            where TEntity : class, new()
        {
            var _reader = this.QueryReader(ref dbTrans, sql, parameters, cmdType, CommandBehavior.SingleRow);
            if (_reader == null) { return default(TEntity); }
            using (_reader)
            {
                var _entity = this.ToEntity1st<TEntity>(_reader);
                return _entity;
            }
        }

        /// <summary>
        /// Get:查询存储过程,并转换首条数据为数据实体
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>首条数据转换的数据实体</returns>
        public TEntity QuerySingleByProc<TEntity>(string procName, IEnumerable<DbParameter> parameters = null)
            where TEntity : class, new()
        {
            return this.QuerySingle<TEntity>(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Get:查询存储过程,并转换首条数据为数据实体
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="dbTrans">事务</param>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>首条数据转换的数据实体</returns>
        public TEntity QuerySingleByProc<TEntity>(ref IDbTransaction dbTrans, string procName, IEnumerable<DbParameter> parameters = null)
            where TEntity : class, new()
        {
            return this.QuerySingle<TEntity>(ref dbTrans, procName, parameters, CommandType.StoredProcedure);
        }

        #endregion

        #region Functions.List

        /// <summary>
        /// Get:查询存储过程,并转换为数据实体
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>数据实体列表</returns>
        public List<TEntity> QueryList<TEntity>(string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
            where TEntity : class, new()
        {
            var _reader = this.QueryReader(sql, parameters, cmdType);
            using (_reader)
            {
                var _list = this.ToList<TEntity>(_reader);
                return _list;
            }
        }

        /// <summary>
        /// Get:查询存储过程,并转换为数据实体
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="dbTrans">事务</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>数据实体列表</returns>
        public List<TEntity> QueryList<TEntity>(ref IDbTransaction dbTrans, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
            where TEntity : class, new()
        {
            var _reader = this.QueryReader(ref dbTrans, sql, parameters, cmdType, CommandBehavior.SingleResult);
            using (_reader)
            {
                var _list = this.ToList<TEntity>(_reader);
                return _list;
            }
        }

        /// <summary>
        /// Get:查询存储过程,并转换为数据实体
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>数据实体列表</returns>
        public List<TEntity> QueryListByProc<TEntity>(string procName, IEnumerable<DbParameter> parameters = null)
            where TEntity : class, new()
        {
            return this.QueryList<TEntity>(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Get:查询存储过程,并转换为数据实体
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="dbTrans">事务</param>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>数据实体列表</returns>
        public List<TEntity> QueryListByProc<TEntity>(ref IDbTransaction dbTrans, string procName, IEnumerable<DbParameter> parameters = null)
            where TEntity : class, new()
        {
            return this.QueryList<TEntity>(ref dbTrans, procName, parameters, CommandType.StoredProcedure);
        }

        #endregion

        #region Functions.Value

        /// <summary>
        /// Get:查询SQL语句,并转换为首个数据值 
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>首行首列的数据值</returns>
        public object QueryValue(string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            var _reader = this.QueryReader(sql, parameters, cmdType);
            if (_reader == null) { return null; }
            using (_reader)
            {
                if (!_reader.Read()) { return null; }
                var _value = _reader.GetValue(0);
                return _value;
            }
        }

        /// <summary>
        /// Get:查询SQL语句,并转换为首个数据值 
        /// </summary>
        /// <param name="dbTrans">事务</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>首行首列的数据值</returns>
        public object QueryValue(ref IDbTransaction dbTrans, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            var _reader = this.QueryReader(ref dbTrans, sql, parameters, cmdType, CommandBehavior.SingleRow);
            if (_reader == null) { return null; }
            using (_reader)
            {
                if (!_reader.Read()) { return null; }
                var _value = _reader.GetValue(0);
                return _value;
            }
        }

        /// <summary>
        /// Get:查询存储过程,并转换为首个数据值
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>首行首列的数据值</returns>
        public object QueryValueByProc(string procName, IEnumerable<DbParameter> parameters = null)
        {
            return this.QueryValue(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Get:查询存储过程,并转换为首个数据值
        /// </summary>
        /// <param name="dbTrans">事务</param>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>首行首列的数据值</returns>
        public object QueryValueByProc(ref IDbTransaction dbTrans, string procName, IEnumerable<DbParameter> parameters = null)
        {
            return this.QueryValue(ref dbTrans, procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Get:查询SQL语句,并转换为首个数据值 
        /// </summary>
        /// <typeparam name="TValue">数据值类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>首行首列的数据值</returns>
        public TValue QueryValue<TValue>(string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            var _reader = this.QueryReader(sql, parameters, cmdType);
            if (_reader == null) { return default(TValue); }
            using (_reader)
            {
                if (!_reader.Read()) { return default(TValue); }

                var _value = _reader.GetValue(0);
                if (_value == null) { return default(TValue); }
                if (_value == DBNull.Value) { return default(TValue); }
                return (TValue)_value;
            }
        }

        /// <summary>
        /// Get:查询SQL语句,并转换为首个数据值 
        /// </summary>
        /// <typeparam name="TValue">数据值类型</typeparam>
        /// <param name="dbTrans">事务</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>首行首列的数据值</returns>
        public TValue QueryValue<TValue>(ref IDbTransaction dbTrans, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            var _reader = this.QueryReader(ref dbTrans, sql, parameters, cmdType, CommandBehavior.SingleRow);
            if (_reader == null) { return default(TValue); }
            using (_reader)
            {
                if (!_reader.Read()) { return default(TValue); }

                var _value = _reader.GetValue(0);
                if (_value == null) { return default(TValue); }
                if (_value == DBNull.Value) { return default(TValue); }
                return (TValue)_value;
            }
        }

        /// <summary>
        /// Get:查询存储过程,并转换为首个数据值
        /// </summary>
        /// <typeparam name="TValue">数据值类型</typeparam>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>首行首列的数据值</returns>
        public TValue QueryValueByProc<TValue>(string procName, IEnumerable<DbParameter> parameters = null)
        {
            return this.QueryValue<TValue>(procName, parameters, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Get:查询存储过程,并转换为首个数据值
        /// </summary>
        /// <typeparam name="TValue">数据值类型</typeparam>
        /// <param name="dbTrans">事务</param>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameters">SQL参数</param>
        /// <returns>首行首列的数据值</returns>
        public TValue QueryValueByProc<TValue>(ref IDbTransaction dbTrans, string procName, IEnumerable<DbParameter> parameters = null)
        {
            return this.QueryValue<TValue>(ref dbTrans, procName, parameters, CommandType.StoredProcedure);
        }

        #endregion

        #region Functions.Parameter

        /// <summary>
        /// Get:创建SQL参数
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns>SQL参数</returns>
        public DbParameter CreateParameter(string parameterName, object value)
        {
            return new SqlParameter(parameterName, value ?? DBNull.Value)
            {
                SourceColumn = parameterName,
            };
        }

        /// <summary>
        /// Get:创建SQL参数
        /// </summary>
        /// <param name="columnName">数据列名</param>
        /// <param name="parameterName">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns>SQL参数</returns>
        public DbParameter CreateParameter(string columnName, string parameterName, object value)
        {
            return new SqlParameter(columnName, value ?? DBNull.Value)
            {
                SourceColumn = columnName,
            };
        }

        /// <summary>
        /// Get:创建SQL参数
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="dbType">参数类型</param>
        /// <param name="value">参数值</param>
        /// <returns>SQL参数</returns>
        public DbParameter CreateParameter(string parameterName, SqlDbType dbType, object value)
        {
            var _parameter = new SqlParameter(parameterName, dbType)
            {
                SourceColumn = parameterName,
                Value = value ?? DBNull.Value,
            };
            return _parameter;
        }

        /// <summary>
        /// Get:创建SQL参数
        /// </summary>
        /// <param name="columnName">数据列名</param>
        /// <param name="parameterName">参数名</param>
        /// <param name="dbType">参数类型</param>
        /// <param name="value">参数值</param>
        /// <returns>SQL参数</returns>
        public DbParameter CreateParameter(string columnName, string parameterName, SqlDbType dbType, object value)
        {
            var _parameter = new SqlParameter(parameterName, dbType)
            {
                SourceColumn = columnName,
                Value = value ?? DBNull.Value,
            };
            return _parameter;
        }

        #endregion

        #region Functions.Parameter.Output

        /// <summary>
        /// Get:创建SQL输出参数
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns>SQL输出参数</returns>
        public DbParameter CreateOutputParameter(string parameterName, object value)
        {
            return new SqlParameter(parameterName, value ?? DBNull.Value)
            {
                SourceColumn = parameterName,
                Direction = ParameterDirection.Output,
            };
        }

        /// <summary>
        /// Get:创建SQL输出参数
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="dbType">参数类型</param>
        /// <param name="value">参数值</param>
        /// <returns>SQL输出参数</returns>
        public DbParameter CreateOutputParameter(string parameterName, SqlDbType dbType, object value)
        {
            var _parameter = new SqlParameter(parameterName, dbType)
            {
                SourceColumn = parameterName,
                Direction = ParameterDirection.Output,
                Value = value ?? DBNull.Value,
            };
            return _parameter;
        }

        #endregion

        #region Functions

        #region Functions.Command

        /// <summary>
        /// Get:创建SQL命令
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public IDbCommand CrateCommand(IDbConnection connection, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (connection == null) { return null; }

            var _cmd = new SqlCommand(sql, connection as SqlConnection);
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
        /// <param name="connection">数据库连接</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public IDbCommand CrateCommand(IDbConnection connection, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
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
        public IDbCommand CrateCommandByProc(IDbConnection connection, string procName, IEnumerable<DbParameter> parameters = null)
        {
            return CrateCommand(connection, procName, parameters, CommandType.StoredProcedure);
        }

        #endregion

        #region Functions.Execute

        /// <summary>
        /// Get:执行SQL语句
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="isTransaction">是否使用事务</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="cmdType">命令解析类型</param>
        /// <returns>受影响的行数</returns>
        public int Execute(IDbConnection connection, bool isTransaction, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
        {
            if (connection == null) { return 0; }
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
        public int Execute(IDbConnection connection, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text)
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
        public int ExecuteByProc(IDbConnection connection, string procName, IEnumerable<DbParameter> parameters = null)
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
        public IDataReader QueryReader(IDbConnection connection, string sql, IEnumerable<DbParameter> parameters = null, CommandType cmdType = CommandType.Text
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
        public IDataReader QueryReaderByProc(IDbConnection connection, string procName, IEnumerable<DbParameter> parameters = null
            , CommandBehavior cmdBehavior = CommandBehavior.CloseConnection)
        {
            return QueryReader(connection, procName, parameters, CommandType.StoredProcedure, cmdBehavior);
        }

        #endregion

        #endregion

    }

}

namespace CSharp_DataBase
{
    public class ActionReturn
    {
        public bool IsSucceed { get; internal set; }
        public string Message { get; internal set; }
        public IDbTransaction Result { get; internal set; }
    }
}
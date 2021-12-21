using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_DataBase
{
    /// <summary>
    /// 工具类：
    /// </summary>
    public static class DataUtility
    {

        #region Consts

        /// <summary> Get:[常量]实体类的反射标志 </summary>
        public const BindingFlags C_BINDINGS_FLAGS_ENTITY = BindingFlags.Instance | BindingFlags.Public;

        /// <summary> Get:[常量]用于转换为DataTable的反射标志 </summary>
        public const BindingFlags C_BINDINGS_FLAGS_4TABLE = BindingFlags.Instance | BindingFlags.Public;

        #endregion

        #region Functions.ToEntity

        /// <summary>
        /// Get:将数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="row">DataTable的行数据</param>
        /// <param name="matchFunc">匹配函数（用于映射数据列与实体类的属性、字段）</param>
        /// <returns>实体类</returns>
        public static TEntity ToEntity<TEntity>(DataRow row, Func<MemberInfo, DataColumn, bool> matchFunc)
            where TEntity : class, new()
        {
            if (row == null) { return default(TEntity); }
            if (row.ItemArray == null) { return default(TEntity); }
            if (row.ItemArray.Length < 1) { return default(TEntity); }

            var _columnCount = row.ItemArray.Length;
            var _columns = row.Table.Columns;

            var _result = new TEntity();

            var _type = typeof(TEntity);
            var _flags = C_BINDINGS_FLAGS_ENTITY;

            var _properties = _type.GetProperties(_flags);
            var _fields = _type.GetFields(_flags);

            if (matchFunc != null)
            {
                #region 通过匹配函数进行赋值

                foreach (DataColumn c in _columns)
                {
                    if (c == null) { continue; }
                    var _columnValue = row[c];

                    #region Field赋值
                    {
                        foreach (var _field in _fields)
                        {
                            if (matchFunc(_field, c))
                            {
                                if (_columnValue == DBNull.Value)
                                {
                                    _field.SetValue(_result, null);
                                }
                                else
                                {
                                    _field.SetValue(_result, _columnValue);
                                }
                                break;
                            }
                        }
                    }
                    #endregion

                    #region Property赋值
                    {
                        foreach (var _property in _properties)
                        {
                            if (matchFunc(_property, c))
                            {
                                if (!_property.CanWrite) { break; }
                                if (_columnValue == DBNull.Value)
                                {
                                    _property.SetValue(_result, null, null);
                                }
                                else if (_columnValue is Array)
                                {
                                    _property.SetValue(_result, _columnValue, new object[0]);
                                }
                                else
                                {
                                    _property.SetValue(_result, _columnValue, null);
                                }
                                break;
                            }
                        }
                    }
                    #endregion

                }

                #endregion
            }
            else
            {
                #region 直接判断并赋值

                foreach (DataColumn c in _columns)
                {
                    if (c == null) { continue; }
                    var _columnValue = row[c];

                    #region Field赋值
                    {
                        foreach (var _field in _fields)
                        {
                            if (_field.Name.Equals(c.ColumnName))
                            {
                                if (_columnValue == DBNull.Value)
                                {
                                    _field.SetValue(_result, null);
                                }
                                else
                                {
                                    _field.SetValue(_result, _columnValue);
                                }
                                break;
                            }
                        }
                    }
                    #endregion

                    #region Property赋值
                    {
                        foreach (var _property in _properties)
                        {
                            if (_property.Name.Equals(c.ColumnName))
                            {
                                if (!_property.CanWrite) { break; }
                                if (_columnValue == DBNull.Value)
                                {
                                    _property.SetValue(_result, null, null);
                                }
                                else if (_columnValue is Array)
                                {
                                    _property.SetValue(_result, _columnValue, new object[0]);
                                }
                                else
                                {
                                    _property.SetValue(_result, _columnValue, null);
                                }
                                break;
                            }
                        }
                    }
                    #endregion

                }

                #endregion
            }

            return _result;
        }

        /// <summary>
        /// Get:将数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="row">DataTable的行数据</param>
        /// <returns>实体类</returns>
        public static TEntity ToEntity<TEntity>(DataRow row)
            where TEntity : class, new()
        {
            if (row == null) { return default(TEntity); }
            if (row.ItemArray == null) { return default(TEntity); }
            if (row.ItemArray.Length < 1) { return default(TEntity); }

            return ToEntity<TEntity>(row
                , (member, column) =>
                {
                    return member.Name.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase);
                });
        }

        /// <summary>
        /// Get:将数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="reader">数据集流</param>
        /// <param name="matchFunc">匹配函数（用于映射数据列与实体类的属性、字段）</param>
        /// <returns>实体类</returns>
        public static TEntity ToEntity<TEntity>(IDataReader reader, Func<MemberInfo, string, bool> matchFunc)
            where TEntity : class, new()
        {
            if (reader == null) { return default(TEntity); }
            if (reader.IsClosed) { return default(TEntity); }
            if (!reader.Read()) { return default(TEntity); }
            //if(reader.Depth==0)

            var _columnCount = reader.FieldCount;
            var _result = new TEntity();

            var _type = typeof(TEntity);
            var _flags = C_BINDINGS_FLAGS_ENTITY;

            var _properties = _type.GetProperties(_flags);
            var _fields = _type.GetFields(_flags);

            if (matchFunc != null)
            {
                #region 通过匹配函数进行赋值

                for (int i = 0; i < _columnCount; i++)
                {
                    var _columnName = reader.GetName(i);
                    var _columnValue = reader.GetValue(i);

                    #region Field赋值
                    {
                        foreach (var _field in _fields)
                        {
                            if (matchFunc(_field, _columnName))
                            {
                                if (_columnValue == DBNull.Value)
                                {
                                    _field.SetValue(_result, null);
                                }
                                else
                                {
                                    _field.SetValue(_result, _columnValue);
                                }
                                break;
                            }
                        }
                    }
                    #endregion

                    #region Property赋值
                    {
                        foreach (var _property in _properties)
                        {
                            if (matchFunc(_property, _columnName))
                            {
                                if (!_property.CanWrite) { break; }
                                if (_columnValue == DBNull.Value)
                                {
                                    _property.SetValue(_result, null, null);
                                }
                                else if (_columnValue is Array)
                                {
                                    _property.SetValue(_result, _columnValue, new object[0]);
                                }
                                else
                                {
                                    _property.SetValue(_result, _columnValue, null);
                                }
                                break;
                            }
                        }
                    }
                    #endregion

                }
                #endregion
            }
            else
            {
                #region 直接判断并赋值

                for (int i = 0; i < _columnCount; i++)
                {
                    var _columnName = reader.GetName(i);
                    var _columnValue = reader.GetValue(i);

                    #region Field赋值
                    {
                        foreach (var _field in _fields)
                        {
                            if (_field.Name.Equals(_columnName))
                            {
                                if (_columnValue == DBNull.Value)
                                {
                                    _field.SetValue(_result, null);
                                }
                                else
                                {
                                    _field.SetValue(_result, _columnValue);
                                }
                                break;
                            }
                        }
                    }
                    #endregion

                    #region Property赋值
                    {
                        foreach (var _property in _properties)
                        {
                            if (_property.Name.Equals(_columnName))
                            {
                                if (!_property.CanWrite) { break; }
                                if (_columnValue == DBNull.Value)
                                {
                                    _property.SetValue(_result, null, null);
                                }
                                else if (_columnValue is Array)
                                {
                                    _property.SetValue(_result, _columnValue, new object[0]);
                                }
                                else
                                {
                                    _property.SetValue(_result, _columnValue, null);
                                }
                                break;
                            }
                        }
                    }
                    #endregion

                }
                #endregion
            }

            return _result;
        }

        /// <summary>
        /// Get:将数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="reader">数据集流</param>
        /// <returns>实体类</returns>
        public static TEntity ToEntity<TEntity>(IDataReader reader)
            where TEntity : class, new()
        {
            if (reader == null) { return default(TEntity); }
            if (reader.IsClosed) { return default(TEntity); }

            return ToEntity<TEntity>(reader
                , (member, columnName) =>
                {
                    return member.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase);
                });
        }

        #endregion

        #region Functions.ToEntity.First

        /// <summary>
        /// Get:将数据源中的首条数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="table">DataTable数据</param>
        /// <param name="matchFunc">匹配函数（用于映射数据列与实体类的属性、字段）</param>
        /// <returns>实体类</returns>
        public static TEntity ToEntity1st<TEntity>(DataTable table, Func<MemberInfo, DataColumn, bool> matchFunc)
            where TEntity : class, new()
        {
            if (table == null) { return default(TEntity); }
            if (table.Rows.Count < 1) { return default(TEntity); }

            var _row = table.Rows[0];
            return ToEntity<TEntity>(_row, matchFunc);
        }

        /// <summary>
        /// Get:将数据源中的首条数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="table">DataTable数据</param>
        /// <returns>实体类</returns>
        public static TEntity ToEntity1st<TEntity>(DataTable table)
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
        public static TEntity ToEntity1st<TEntity>(IDataReader reader)
            where TEntity : class, new()
        {
            return ToEntity<TEntity>(reader);
        }

        /// <summary>
        /// Get:将数据源中的首条数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="reader">数据集流</param>
        /// <param name="matchFunc">匹配函数（用于映射数据列与实体类的属性、字段）</param>
        /// <returns>实体类</returns>
        public static TEntity ToEntity1st<TEntity>(IDataReader reader, Func<MemberInfo, string, bool> matchFunc)
            where TEntity : class, new()
        {
            return ToEntity<TEntity>(reader, matchFunc);
        }

        #endregion

        #region Functions.ToList

        /// <summary>
        /// Get:将数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="reader">数据集流</param>
        /// <param name="matchFunc">匹配函数（用于映射数据列与实体类的属性、字段）</param>
        /// <returns>实体类</returns>
        public static List<TEntity> ToList<TEntity>(IDataReader reader, Func<MemberInfo, string, bool> matchFunc)
            where TEntity : class, new()
        {
            var _result = new List<TEntity>();

            if (reader == null) { return _result; }
            if (reader.IsClosed) { return _result; }
            if (!reader.Read()) { return _result; }
            //if(reader.Depth==0)

            var _propertyDict = new Dictionary<string, PropertyInfo>();
            var _fieldDict = new Dictionary<string, FieldInfo>();

            #region Get Binding Members
            {
                var _type = typeof(TEntity);
                var _flags = C_BINDINGS_FLAGS_ENTITY;

                var _properties = _type.GetProperties(_flags);
                var _fields = _type.GetFields(_flags);

                var _columnCount = reader.FieldCount;

                if (matchFunc != null)
                {
                    #region 通过匹配函数进行赋值

                    for (int i = 0; i < _columnCount; i++)
                    {
                        var _columnName = reader.GetName(i);

                        #region 获取Field
                        {
                            foreach (var _field in _fields)
                            {
                                if (matchFunc(_field, _columnName))
                                {
                                    _fieldDict.Add(_columnName, _field);
                                    break;
                                }
                            }
                        }
                        #endregion

                        #region 获取Property
                        {
                            foreach (var _property in _properties)
                            {
                                if (matchFunc(_property, _columnName))
                                {
                                    if (!_property.CanWrite) { break; }
                                    if (_propertyDict.ContainsKey(_columnName)) { continue; }
                                    _propertyDict.Add(_columnName, _property);
                                    break;
                                }
                            }
                        }
                        #endregion
                    }

                    #endregion
                }
                else
                {
                    #region 直接判断并赋值

                    for (int i = 0; i < _columnCount; i++)
                    {
                        var _columnName = reader.GetName(i);

                        #region 获取Field
                        {
                            foreach (var _field in _fields)
                            {
                                if (_field.Name.Equals(_columnName, StringComparison.OrdinalIgnoreCase))
                                {
                                    _fieldDict.Add(_columnName, _field);
                                    break;
                                }
                            }
                        }
                        #endregion

                        #region 获取Property
                        {
                            foreach (var _property in _properties)
                            {
                                if (_property.Name.Equals(_columnName, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (!_property.CanWrite) { break; }
                                    if (_propertyDict.ContainsKey(_columnName)) { continue; }
                                    _propertyDict.Add(_columnName, _property);
                                    break;
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }

            }
            #endregion

            #region Binding Data
            {
                do
                {
                    var _item = new TEntity();

                    #region Field 赋值
                    foreach (var kv in _fieldDict)
                    {
                        var _columnValue = reader[kv.Key];
                        if (_columnValue == DBNull.Value)
                        {
                            kv.Value.SetValue(_item, null);
                        }
                        else
                        {
                            kv.Value.SetValue(_item, _columnValue);
                        }
                    }
                    #endregion

                    #region Property 赋值

                    foreach (var kv in _propertyDict)
                    {
                        var _columnValue = reader[kv.Key];
                        if (_columnValue == DBNull.Value)
                        {
                            kv.Value.SetValue(_item, null, null);
                        }
                        else if (_columnValue is Array)
                        {
                            kv.Value.SetValue(_item, _columnValue, new object[0]);
                        }
                        else
                        {
                            kv.Value.SetValue(_item, _columnValue, null);
                        }
                    }

                    #endregion

                    _result.Add(_item);
                }
                while (reader.Read());
            }
            #endregion

            return _result;
        }

        /// <summary>
        /// Get:将数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="reader">数据集流</param>
        /// <returns>实体类</returns>
        public static List<TEntity> ToList<TEntity>(IDataReader reader)
            where TEntity : class, new()
        {
            if (reader == null) { return new List<TEntity>(); }
            if (reader.IsClosed) { return new List<TEntity>(); }

            return ToList<TEntity>(reader
                , (member, columnName) =>
                {
                    return member.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase);
                });
        }
        /// <summary>
        /// Get:将数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="table">DataTable数据</param>
        /// <param name="matchFunc">匹配函数（用于映射数据列与实体类的属性、字段）</param>
        /// <returns>实体类</returns>
        public static List<TEntity> ToList<TEntity>(DataTable table, Func<MemberInfo, DataColumn, bool> matchFunc)
            where TEntity : class, new()
        {
            var _result = new List<TEntity>();

            if (table == null) { return _result; }
            if (table.Rows == null) { return _result; }
            if (table.Rows.Count < 1) { return _result; }

            #region 无数据列时
            {
                if (table.Columns.Count < 1)
                {
                    foreach (DataRow _row in table.Rows)
                    {
                        if (_row == null)
                        {
                            _result.Add(default(TEntity));
                            continue;
                        }
                    }
                    return _result;
                }
            }
            #endregion

            var _columnCount = table.Columns.Count;
            var _columns = table.Columns;

            var _propertyDict = new Dictionary<string, PropertyInfo>();
            var _fieldDict = new Dictionary<string, FieldInfo>();

            #region Get Binding Members
            {
                var _type = typeof(TEntity);
                var _flags = C_BINDINGS_FLAGS_ENTITY;

                var _properties = _type.GetProperties(_flags);
                var _fields = _type.GetFields(_flags);

                if (matchFunc != null)
                {
                    #region 通过匹配函数进行判断

                    foreach (DataColumn c in _columns)
                    {
                        if (c == null) { continue; }
                        var _columnName = c.ColumnName;

                        #region 获取Field
                        {
                            foreach (var _field in _fields)
                            {
                                if (matchFunc(_field, c))
                                {
                                    _fieldDict.Add(_columnName, _field);
                                    break;
                                }
                            }
                        }
                        #endregion

                        #region 获取Property
                        {
                            foreach (var _property in _properties)
                            {
                                if (matchFunc(_property, c))
                                {
                                    if (!_property.CanWrite) { break; }
                                    _propertyDict.Add(_columnName, _property);
                                    break;
                                }
                            }
                        }
                        #endregion

                    }

                    #endregion
                }
                else
                {
                    #region 直接判断

                    foreach (DataColumn c in _columns)
                    {
                        if (c == null) { continue; }
                        var _columnName = c.ColumnName;

                        #region 获取Field
                        {
                            foreach (var _field in _fields)
                            {
                                if (_field.Name.Equals(_columnName))
                                {
                                    _fieldDict.Add(_columnName, _field);
                                    break;
                                }
                            }
                        }
                        #endregion

                        #region 获取Property
                        {
                            foreach (var _property in _properties)
                            {
                                if (_property.Name.Equals(_columnName))
                                {
                                    if (!_property.CanWrite) { break; }
                                    _propertyDict.Add(_columnName, _property);
                                    break;
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
            }
            #endregion

            #region Binding Data
            {
                foreach (DataRow _row in table.Rows)
                {
                    if (_row == null)
                    {
                        _result.Add(default(TEntity));
                        continue;
                    }

                    var _item = new TEntity();

                    #region Field 赋值
                    foreach (var kv in _fieldDict)
                    {
                        var _columnValue = _row[kv.Key];
                        if (_columnValue == DBNull.Value)
                        {
                            kv.Value.SetValue(_item, null);
                        }
                        else
                        {
                            kv.Value.SetValue(_item, _columnValue);
                        }
                    }
                    #endregion

                    #region Property 赋值

                    foreach (var kv in _propertyDict)
                    {
                        var _columnValue = _row[kv.Key];
                        if (_columnValue == DBNull.Value)
                        {
                            kv.Value.SetValue(_item, null, null);
                        }
                        else if (_columnValue is Array)
                        {
                            kv.Value.SetValue(_item, _columnValue, new object[0]);
                        }
                        else
                        {
                            kv.Value.SetValue(_item, _columnValue, null);
                        }
                    }

                    #endregion

                    _result.Add(_item);
                }
            }
            #endregion

            return _result;
        }

        /// <summary>
        /// Get:将数据转换为实体类
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="table">DataTable数据</param>
        /// <returns>实体类</returns>
        public static List<TEntity> ToList<TEntity>(DataTable table)
            where TEntity : class, new()
        {
            if (table == null) { return new List<TEntity>(); }
            if (table.Rows == null) { return new List<TEntity>(); }
            if (table.Rows.Count < 1) { return new List<TEntity>(); }

            return ToList<TEntity>(table
                , (member, column) =>
                {
                    return member.Name.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase);
                });
        }

        /// <summary>
        /// Get:将数据转换为实体类列表
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="rows">DataTable的行数据迭代</param>
        /// <param name="matchFunc">匹配函数（用于映射数据列与实体类的属性、字段）</param>
        /// <returns>实体类列表</returns>
        public static List<TEntity> ToList<TEntity>(IEnumerable<DataRow> rows, Func<MemberInfo, DataColumn, bool> matchFunc)
            where TEntity : class, new()
        {
            var _result = new List<TEntity>();
            if (rows == null) { return _result; }

            foreach (var r in rows)
            {
                if (r == null)
                {
                    _result.Add(default(TEntity));
                    continue;
                }

                var _entity = ToEntity<TEntity>(r, matchFunc);
                _result.Add(_entity);
            }
            return _result;
        }

        /// <summary>
        /// Get:将数据转换为实体类列表
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="rows">DataTable的行数据迭代</param>
        /// <returns>实体类列表</returns>
        public static List<TEntity> ToList<TEntity>(IEnumerable<DataRow> rows)
            where TEntity : class, new()
        {
            if (rows == null) { return new List<TEntity>(); }
            return ToList<TEntity>(rows
                , (member, column) =>
                {
                    return member.Name.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase);
                });
        }

        #endregion

        #region Funcitons.Json

        /// <summary>
        /// Get:序列化为Json字符串
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="t">要序列化的实例</param>
        /// <returns></returns>
        public static string ToJson<T>(this T t)
        {
            if (t == null) { return ""; }
            return Newtonsoft.Json.JsonConvert.SerializeObject(t);
        }

        /// <summary>
        /// Get:将Json字符串反序列化为实例
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="json">要序列化的对象</param>
        /// <returns>反序列化得到的实例</returns>
        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) { return default(T); }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        #endregion

        #region Funcitons.CopyValueTo

        /// <summary>
        /// Get:将源实例的值复制至目标实例
        /// </summary>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="tSource">源实例</param>
        /// <param name="tTarget">目标实例</param>
        /// <returns>目标实例</returns>
        public static TTarget CopyTo<TTarget>(this object tSource, TTarget tTarget)
            where TTarget : class
        {
            if (tSource == null) { return tTarget; }
            if (tTarget == null) { return tTarget; }

            var _target = CopyTo(tSource, tTarget, false);
            return tTarget;
        }

        /// <summary>
        /// Get:将源实例的值复制至目标实例
        /// </summary>
        /// <param name="tSource">源实例</param>
        /// <param name="tTarget">目标实例</param>
        /// <param name="isCopyClassMember">是否复制类的成员(即属性、字段是类的)</param>
        /// <returns>目标实例</returns>
        public static object CopyTo(this object tSource, object tTarget, bool isCopyClassMember = false)
        {
            if (tSource == null) { return tTarget; }
            if (tTarget == null) { return tTarget; }

            var _memberDict = GetCommonFieldsAndProperties(tSource.GetType(), tTarget.GetType());

            if (isCopyClassMember)
            {
                #region 

                foreach (var kv in _memberDict)
                {
                    if (kv.Key is FieldInfo)
                    {
                        var f1 = kv.Key as FieldInfo;
                        var f2 = kv.Value as FieldInfo;

                        //var _value = f1.GetValue(tSource);
                        //f2.SetValue(tTarget, _value);

                        if (f1.FieldType.IsValueType || f1.FieldType.Equals(typeof(string)))
                        {
                            var _value = f1.GetValue(tSource);
                            f2.SetValue(tTarget, _value);
                            continue;
                        }
                        if (f1.FieldType.IsClass)
                        {
                            var _value = f1.GetValue(tSource);
                            if (_value == null)
                            {
                                f2.SetValue(tTarget, _value);
                            }
                            else
                            {
                                var _value2 = Activator.CreateInstance(_value.GetType());
                                CopyTo(_value, _value2, isCopyClassMember);
                                f2.SetValue(tTarget, _value2);
                            }
                        }
                        continue;
                    }
                    if (kv.Key is PropertyInfo)
                    {
                        var p1 = kv.Key as PropertyInfo;
                        var p2 = kv.Value as PropertyInfo;

                        if (!p2.CanWrite) { continue; }

                        if (p2.PropertyType.IsValueType || p2.PropertyType.Equals(typeof(string)))
                        {
                            if (p1.PropertyType.IsArray)
                            {
                                var _indexs = new object[0];
                                var _value = p1.GetValue(tSource, _indexs);
                                p2.SetValue(tTarget, _value, _indexs);
                                continue;
                            }
                            else
                            {
                                var _value = p1.GetValue(tSource, null);
                                p2.SetValue(tTarget, _value, null);
                                continue;
                            }
                        }
                        if (p2.PropertyType.IsClass)
                        {
                            if (p1.PropertyType.IsArray)
                            {
                                var _indexs = new object[0];
                                var _value = p1.GetValue(tSource, _indexs);

                                if (_value == null)
                                {
                                    p2.SetValue(tTarget, _value, _indexs);
                                }
                                else
                                {
                                    var _value2 = Activator.CreateInstance(_value.GetType());
                                    CopyTo(_value, _value2, isCopyClassMember);
                                    p2.SetValue(tTarget, _value2, _indexs);
                                }
                                continue;
                            }
                            else
                            {
                                var _value = p1.GetValue(tSource, null);
                                if (_value == null)
                                {
                                    p2.SetValue(tTarget, _value, null);
                                }
                                else
                                {
                                    var _value2 = Activator.CreateInstance(_value.GetType());
                                    CopyTo(_value, _value2, isCopyClassMember);
                                    p2.SetValue(tTarget, _value2, null);
                                }
                                continue;
                            }
                        }

                    }
                }
                #endregion
            }
            else
            {
                #region 

                foreach (var kv in _memberDict)
                {
                    if (kv.Key is FieldInfo)
                    {
                        var f1 = kv.Key as FieldInfo;
                        var f2 = kv.Value as FieldInfo;

                        var _value = f1.GetValue(tSource);
                        f2.SetValue(tTarget, _value);
                        continue;
                    }
                    if (kv.Key is PropertyInfo)
                    {
                        var p1 = kv.Key as PropertyInfo;
                        var p2 = kv.Value as PropertyInfo;

                        if (!p2.CanWrite) { continue; }

                        if (p1.PropertyType.IsArray)
                        {
                            var _indexs = new object[0];
                            var _value = p1.GetValue(tSource, _indexs);
                            p2.SetValue(tTarget, _value, _indexs);
                            continue;
                        }
                        else
                        {
                            var _value = p1.GetValue(tSource, null);
                            p2.SetValue(tTarget, _value, null);
                            continue;
                        }
                    }
                }
                #endregion
            }

            return tTarget;
        }

        #endregion

        #region Funcitons.Member

        /// <summary>
        /// Get:获取类型的字段与属性
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>实体类</returns>
        public static Dictionary<MemberInfo, MemberInfo> GetCommonFieldsAndProperties(Type sourceType, Type targetType)
        {
            var _dict = new Dictionary<MemberInfo, MemberInfo>();
            if (sourceType == null) { return _dict; }
            if (targetType == null) { return _dict; }

            #region Get Members
            {
                var _flags = C_BINDINGS_FLAGS_ENTITY;

                var _sourceProperties = sourceType.GetProperties(_flags);
                var _sourceFields = sourceType.GetFields(_flags);

                var _targetProperties = targetType.GetProperties(_flags);
                var _targetFields = targetType.GetFields(_flags);

                foreach (var f1 in _sourceFields)
                {
                    var f2 = _targetFields.FirstOrDefault(
                        x => x.Name == f1.Name && x.FieldType.Equals(f1.FieldType));
                    if (f2 == null) { continue; }
                    _dict.Add(f1, f2);
                }

                foreach (var p1 in _sourceProperties)
                {
                    if (!p1.CanRead) { continue; }
                    var p2 = _targetProperties.FirstOrDefault(
                        x => x.CanRead && x.Name == p1.Name && x.PropertyType.Equals(p1.PropertyType));
                    if (p2 == null) { continue; }
                    _dict.Add(p1, p2);
                }
            }
            #endregion

            return _dict;
        }

        /// <summary>
        /// Get:获取类型的字段与属性
        /// </summary>
        /// <param name="type">DataTable数据</param>
        /// <returns>实体类</returns>
        public static Dictionary<string, MemberInfo> GetFieldsAndProperties(Type type)
        {
            if (type == null) { return new Dictionary<string, MemberInfo>(); }

            var _dict = new Dictionary<string, MemberInfo>();

            #region Get Members
            {
                var _flags = C_BINDINGS_FLAGS_4TABLE;

                var _properties = type.GetProperties(_flags);
                var _fields = type.GetFields(_flags);

                foreach (var f in _fields)
                {
                    _dict.Add(f.Name, f);
                }

                foreach (var p in _properties)
                {
                    if (!p.CanRead) { continue; }
                    _dict.Add(p.Name, p);
                }
            }
            #endregion

            return _dict;
        }

        /// <summary>
        /// Get:根据实体对象类型创建DataTable
        /// </summary>
        /// <param name="prefix">数据列名前缀</param>
        /// <param name="type">DataTable数据</param>
        /// <param name="dict">DataTable数据</param>
        /// <returns>实体类</returns>
        public static Dictionary<string, MemberInfo> GetFieldsAndProperties(string prefix, Type type, ref Dictionary<string, MemberInfo> dict)
        {
            if (type == null) { return new Dictionary<string, MemberInfo>(); }

            var _type = type;

            if (dict == null) { dict = new Dictionary<string, MemberInfo>(); }
            var _dict = dict;

            #region Get Binding Members
            {
                var _flags = C_BINDINGS_FLAGS_4TABLE;

                var _properties = _type.GetProperties(_flags);
                var _fields = _type.GetFields(_flags);

                #region Field 赋值

                foreach (var f in _fields)
                {
                    if (f.FieldType.IsValueType)
                    {
                        _dict.Add(prefix + f.Name, f);
                        continue;
                    }
                    if (f.FieldType.Equals(typeof(string)))
                    {
                        _dict.Add(prefix + f.Name, f);
                        continue;
                    }
                    if (f.FieldType.IsClass)
                    {
                        var _dict2 = GetColumnDict4Table($"{prefix}{f.Name}.", f.FieldType, ref _dict);
                    }
                }

                #endregion

                #region Property 赋值

                foreach (var p in _properties)
                {
                    if (!p.CanRead) { continue; }

                    if (p.PropertyType.IsValueType)
                    {
                        _dict.Add(prefix + p.Name, p);
                        continue;
                    }
                    if (p.PropertyType.Equals(typeof(string)))
                    {
                        _dict.Add(prefix + p.Name, p);
                        continue;
                    }
                    if (p.PropertyType.IsClass)
                    {
                        var _dict2 = GetColumnDict4Table($"{prefix}{p.Name}.", p.PropertyType, ref _dict);
                    }
                }

                #endregion
            }
            #endregion

            return _dict;
        }

        #endregion

        #region Funcitons.Convert

        /// <summary>
        /// Get:通过Json转换实例
        /// </summary>
        /// <typeparam name="TTarget">转换后的类型</typeparam>
        /// <param name="source">实例</param>
        /// <returns>转换后的实例</returns>
        public static TTarget ConvertToByJson<TTarget>(this object source)
            where TTarget : class, new()
        {
            if (source == null) { return default(TTarget); }
            var _json = Newtonsoft.Json.JsonConvert.SerializeObject(source);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TTarget>(_json);
        }

        /// <summary>
        /// Get:转换实例
        /// </summary>
        /// <typeparam name="TTarget">转换后的类型</typeparam>
        /// <param name="source">实例</param>
        /// <returns>转换后的实例</returns>
        public static TTarget ConvertTo<TTarget>(this object source)
            where TTarget : class, new()
        {
            if (source == null) { return default(TTarget); }
            var _target = new TTarget();
            var _targetObject = CopyTo(source, _target, true);
            return _target;
        }

        /// <summary>
        /// Get:将实例迭代转换指定实例类型的列表
        /// </summary>
        /// <typeparam name="TTarget">转换后的类型</typeparam>
        /// <param name="source">实例</param>
        /// <param name="isCopyClassMember">是否复制类的成员(即属性、字段是类的)</param>
        /// <returns>转换后的实例</returns>
        public static List<TTarget> ConvertToList<TTarget>(this IEnumerable source, bool isCopyClassMember = false)
            where TTarget : class, new()
        {
            return ConvertToList<TTarget>(source, null, isCopyClassMember);
        }
        /// <summary>
        /// Get:将实例迭代转换指定实例类型的列表
        /// </summary>
        /// <typeparam name="TTarget">转换后的类型</typeparam>
        /// <param name="source">实例</param>
        /// <param name="onConverted">转换后触发函数</param>
        /// <param name="isCopyClassMember">是否复制类的成员(即属性、字段是类的)</param>
        /// <returns>转换后的实例</returns>
        public static List<TTarget> ConvertToList<TTarget>(this IEnumerable source, Action<object, TTarget> onConverted, bool isCopyClassMember = false)
            where TTarget : class, new()
        {
            var _list = new List<TTarget>();
            if (source == null) { return _list; }

            Type _sourceType = null;
            if (source.GetType().IsGenericType)
            {
                _sourceType = source.GetType().GetGenericArguments()[0];
            }

            var _hasChild = false;
            foreach (var _source in source)
            {
                _hasChild = true;
                if (_sourceType == null)
                {
                    _sourceType = _source.GetType();
                }
                break;
            }
            if (!_hasChild) { return _list; }

            var _memberDict = GetCommonFieldsAndProperties(_sourceType, typeof(TTarget));

            foreach (var _source in source)
            {
                if (_source == null)
                {
                    _list.Add(null);
                }

                var _target = new TTarget();

                if (isCopyClassMember)
                {
                    #region 

                    foreach (var kv in _memberDict)
                    {
                        if (kv.Key is FieldInfo)
                        {
                            var f1 = kv.Key as FieldInfo;
                            var f2 = kv.Value as FieldInfo;

                            //var _value = f1.GetValue(tSource);
                            //f2.SetValue(tTarget, _value);

                            if (f1.FieldType.IsValueType || f1.FieldType.Equals(typeof(string)))
                            {
                                var _value = f1.GetValue(_source);
                                f2.SetValue(_target, _value);
                                continue;
                            }
                            if (f1.FieldType.IsClass)
                            {
                                var _value = f1.GetValue(_source);
                                if (_value == null)
                                {
                                    f2.SetValue(_target, _value);
                                }
                                else
                                {
                                    var _value2 = Activator.CreateInstance(_value.GetType());
                                    CopyTo(_value, _value2, isCopyClassMember);
                                    f2.SetValue(_target, _value2);
                                }
                            }
                            continue;
                        }
                        if (kv.Key is PropertyInfo)
                        {
                            var p1 = kv.Key as PropertyInfo;
                            var p2 = kv.Value as PropertyInfo;

                            if (!p2.CanWrite) { continue; }

                            if (p2.PropertyType.IsValueType || p2.PropertyType.Equals(typeof(string)))
                            {
                                if (p1.PropertyType.IsArray)
                                {
                                    var _indexs = new object[0];
                                    var _value = p1.GetValue(_source, _indexs);
                                    p2.SetValue(_target, _value, _indexs);
                                    continue;
                                }
                                else
                                {
                                    var _value = p1.GetValue(_source, null);
                                    p2.SetValue(_target, _value, null);
                                    continue;
                                }
                            }
                            if (p2.PropertyType.IsClass)
                            {
                                if (p1.PropertyType.IsArray)
                                {
                                    var _indexs = new object[0];
                                    var _value = p1.GetValue(_source, _indexs);

                                    if (_value == null)
                                    {
                                        p2.SetValue(_target, _value, _indexs);
                                    }
                                    else
                                    {
                                        var _value2 = Activator.CreateInstance(_value.GetType());
                                        CopyTo(_value, _value2, isCopyClassMember);
                                        p2.SetValue(_target, _value2, _indexs);
                                    }
                                    continue;
                                }
                                else
                                {
                                    var _value = p1.GetValue(_source, null);
                                    if (_value == null)
                                    {
                                        p2.SetValue(_target, _value, null);
                                    }
                                    else
                                    {
                                        var _value2 = Activator.CreateInstance(_value.GetType());
                                        CopyTo(_value, _value2, isCopyClassMember);
                                        p2.SetValue(_target, _value2, null);
                                    }
                                    continue;
                                }
                            }

                        }
                    }
                    #endregion
                }
                else
                {
                    #region 

                    foreach (var kv in _memberDict)
                    {
                        if (kv.Key is FieldInfo)
                        {
                            var f1 = kv.Key as FieldInfo;
                            var f2 = kv.Value as FieldInfo;

                            var _value = f1.GetValue(_source);
                            f2.SetValue(_target, _value);
                            continue;
                        }
                        if (kv.Key is PropertyInfo)
                        {
                            var p1 = kv.Key as PropertyInfo;
                            var p2 = kv.Value as PropertyInfo;

                            if (!p2.CanWrite) { continue; }

                            if (p1.PropertyType.IsArray)
                            {
                                var _indexs = new object[0];
                                var _value = p1.GetValue(_source, _indexs);
                                p2.SetValue(_target, _value, _indexs);
                                continue;
                            }
                            else
                            {
                                var _value = p1.GetValue(_source, null);
                                p2.SetValue(_target, _value, null);
                                continue;
                            }
                        }
                    }
                    #endregion
                }

                onConverted?.Invoke(_source, _target);
                _list.Add(_target);
            }

            return _list;
        }

        #endregion

        #region Funcitons.Clone

        /// <summary>
        /// Get:通过Json序列化克隆实例
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="t">克隆实例</param>
        /// <returns>克隆后的实例</returns>
        public static T CloneByJson<T>(this T t)
        {
            if (t == null) { return default(T); }
            var _json = Newtonsoft.Json.JsonConvert.SerializeObject(t);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(_json);
        }

        /// <summary>
        /// Get:克隆实例
        /// </summary>
        /// <typeparam name="TEntity">实例类型</typeparam>
        /// <param name="t">克隆实例</param>
        /// <returns>克隆后的实例</returns>
        public static TEntity Clone<TEntity>(this TEntity t)
            where TEntity : class, new()
        {
            if (t == null) { return default(TEntity); }

            var _new = new TEntity();

            var _type = typeof(TEntity);
            var _flags = C_BINDINGS_FLAGS_ENTITY;

            #region Bind

            var _properties = _type.GetProperties(_flags);
            var _fields = _type.GetFields(_flags);

            #region Field赋值
            {
                foreach (var _field in _fields)
                {
                    var _value = _field.GetValue(t);
                    _field.SetValue(_new, _value);
                }
            }
            #endregion

            #region Property赋值
            {
                foreach (var _property in _properties)
                {
                    if (!_property.CanWrite) { continue; }

                    if (_property.PropertyType.IsArray)
                    {
                        var _indexs = new object[0];
                        var _value = _property.GetValue(t, _indexs);
                        _property.SetValue(_new, _value, _indexs);
                    }
                    else
                    {
                        var _value = _property.GetValue(t, null);
                        _property.SetValue(_new, _value, null);
                    }
                }
            }
            #endregion

            #endregion

            return _new;
        }

        #endregion

        #region Functions.CreateTable

        /// <summary>
        /// Get:根据实体对象类型创建DataTable
        /// </summary>
        /// <param name="prefix">数据列名前缀</param>
        /// <param name="type">DataTable数据</param>
        /// <returns>实体类</returns>
        public static Dictionary<string, MemberInfo> GetColumnDict4Table(string prefix, Type type)
        {
            if (type == null) { return new Dictionary<string, MemberInfo>(); }
            var _dict = new Dictionary<string, MemberInfo>();
            return GetColumnDict4Table(prefix, type, ref _dict);
        }

        /// <summary>
        /// Get:根据实体对象类型创建DataTable
        /// </summary>
        /// <param name="prefix">数据列名前缀</param>
        /// <param name="type">DataTable数据</param>
        /// <param name="dict">DataTable数据</param>
        /// <returns>实体类</returns>
        public static Dictionary<string, MemberInfo> GetColumnDict4Table(string prefix, Type type, ref Dictionary<string, MemberInfo> dict)
        {
            if (type == null) { return new Dictionary<string, MemberInfo>(); }

            var _type = type;

            if (dict == null) { dict = new Dictionary<string, MemberInfo>(); }
            var _dict = dict;

            #region Get Binding Members
            {
                var _flags = C_BINDINGS_FLAGS_4TABLE;

                var _properties = _type.GetProperties(_flags);
                var _fields = _type.GetFields(_flags);

                #region Field 赋值

                foreach (var f in _fields)
                {
                    if (f.FieldType.IsValueType)
                    {
                        _dict.Add(prefix + f.Name, f);
                        continue;
                    }
                    if (f.FieldType.Equals(typeof(string)))
                    {
                        _dict.Add(prefix + f.Name, f);
                        continue;
                    }
                    if (f.FieldType.IsClass)
                    {
                        var _dict2 = GetColumnDict4Table($"{prefix}{f.Name}.", f.FieldType, ref _dict);
                        //foreach(var kv in _dict2)
                        //{
                        //    _dict.Add(kv.Key, kv.Value);
                        //}
                    }
                }

                #endregion

                #region Property 赋值

                foreach (var p in _properties)
                {
                    if (!p.CanRead) { continue; }

                    if (p.PropertyType.IsValueType)
                    {
                        _dict.Add(prefix + p.Name, p);
                        continue;
                    }
                    if (p.PropertyType.Equals(typeof(string)))
                    {
                        _dict.Add(prefix + p.Name, p);
                        continue;
                    }
                    if (p.PropertyType.IsClass)
                    {
                        var _dict2 = GetColumnDict4Table($"{prefix}{p.Name}.", p.PropertyType, ref _dict);
                        //foreach(var kv in _dict2)
                        //{
                        //    _dict.Add(kv.Key, kv.Value);
                        //}
                    }
                }

                #endregion
            }
            #endregion

            return _dict;
        }

        /// <summary>
        /// Get:根据实体对象类型创建DataTable
        /// </summary>
        /// <param name="prefix">数据列名前缀</param>
        /// <param name="type">DataTable数据</param>
        /// <returns>实体类</returns>
        public static List<DataColumn> GetColumns4Table(string prefix, Type type)
        {
            if (type == null) { return new List<DataColumn>(); }

            var _type = type;
            var _table = new DataTable(_type.Name);

            var _columns = new List<DataColumn>();

            #region Get Binding Members
            {
                var _flags = C_BINDINGS_FLAGS_4TABLE;

                var _properties = _type.GetProperties(_flags);
                var _fields = _type.GetFields(_flags);

                #region Field 赋值

                foreach (var f in _fields)
                {
                    if (f.FieldType.IsValueType)
                    {
                        _columns.Add(new DataColumn(prefix + f.Name, f.FieldType));
                        continue;
                    }
                    if (f.FieldType.Equals(typeof(string)))
                    {
                        _columns.Add(new DataColumn(prefix + f.Name, f.FieldType));
                        continue;
                    }
                    if (f.FieldType.IsClass)
                    {
                        var _subcolumns = GetColumns4Table($"{prefix}{f.Name}.", f.FieldType);
                        _columns.AddRange(_subcolumns);
                    }
                }

                #endregion

                #region Property 赋值

                foreach (var p in _properties)
                {
                    if (!p.CanRead) { continue; }
                    if (p.PropertyType.IsValueType)
                    {
                        _columns.Add(new DataColumn(prefix + p.Name, p.PropertyType));
                        continue;
                    }
                    if (p.PropertyType.Equals(typeof(string)))
                    {
                        _columns.Add(new DataColumn(prefix + p.Name, p.PropertyType));
                        continue;
                    }
                    if (p.PropertyType.IsClass)
                    {
                        var _subcolumns = GetColumns4Table($"{prefix}{p.Name}.", p.PropertyType);
                        _columns.AddRange(_subcolumns);
                    }
                }

                #endregion
            }
            #endregion

            return _columns;
        }

        /// <summary>
        /// Get:根据实体对象类型创建DataTable
        /// </summary>
        /// <param name="type">DataTable数据</param>
        /// <param name="dict">DataTable数据</param>
        /// <returns>实体类</returns>
        public static DataTable CreateTable(Type type, ref Dictionary<string, MemberInfo> dict)
        {
            if (type == null) { return new DataTable(); }

            var _table = new DataTable(type.Name);
            var _columnDict = GetColumnDict4Table("", type, ref dict);
            foreach (var kv in _columnDict)
            {
                Type _type = null;
                if (kv.Value is FieldInfo)
                {
                    _type = (kv.Value as FieldInfo).FieldType;
                }
                else if (kv.Value is PropertyInfo)
                {
                    _type = (kv.Value as PropertyInfo).PropertyType;
                }
                if (_type == null) { continue; }
                if (_type.IsGenericType && _type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    _type = _type.GetGenericArguments()[0];
                }
                _table.Columns.Add(new DataColumn(kv.Key, _type));
            }
            return _table;
        }

        /// <summary>
        /// Get:根据实体对象类型创建DataTable
        /// </summary>
        /// <param name="type">DataTable数据</param>
        /// <returns>实体类</returns>
        public static DataTable CreateTable(Type type)
        {
            if (type == null) { return new DataTable(); }

            var _table = new DataTable(type.Name);
            var _dict = new Dictionary<string, MemberInfo>();
            var _columns = GetColumnDict4Table("", type, ref _dict);
            foreach (var kv in _columns)
            {
                _table.Columns.Add(new DataColumn(kv.Key, kv.Value.ReflectedType));
            }
            return _table;
        }

        /// <summary>
        /// Get:根据实体对象类型创建DataTable
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <returns>实体类</returns>
        public static DataTable CreateTable<TEntity>()
        {
            return CreateTable(typeof(TEntity));
        }

        #endregion

        #region Functions.GetValue

        /// <summary>
        /// Get:根据映射路径获取实体对象的值
        /// </summary>
        /// <param name="entity">DataTable数据</param>
        /// <param name="paths">映射路径</param>
        /// <param name="deep">深度</param>
        /// <returns>相应的值</returns>
        public static object GetPropertyOrFieldValue(object entity, string[] paths, int deep = 0)
        {
            var _type = entity.GetType();
            var _rootname = paths[deep];

            var _field = _type.GetField(_rootname, C_BINDINGS_FLAGS_ENTITY);
            if (_field != null)
            {
                var _value = _field.GetValue(entity);
                if (_value == null) { return null; }
                if (_value == DBNull.Value) { return null; }
                if (deep >= paths.Length - 1) { return _value; }
                deep++;
                return GetPropertyOrFieldValue(_value, paths, deep);
            }

            var _property = _type.GetProperty(_rootname, C_BINDINGS_FLAGS_ENTITY);
            if (_property != null)
            {
                var _value = _property.GetValue(entity, null);
                if (_value == null) { return null; }
                if (_value == DBNull.Value) { return null; }
                if (deep >= paths.Length - 1) { return _value; }
                deep++;
                return GetPropertyOrFieldValue(_value, paths, deep);
            }

            return null;
        }

        /// <summary>
        /// 取出实体的属性值或字段值
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="name">属性名或字段名</param>
        /// <returns></returns>
        public static object GetPropertyOrFieldValue(object entity, string name)
        {
            var _flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;

            var _property = entity.GetType().GetProperty(name, _flags);
            if (_property != null)
            {
                return _property.GetValue(entity, null);
            }

            var _field = entity.GetType().GetField(name, _flags);
            if (_field != null)
            {
                return _field.GetValue(entity);
            }

            return default(object);
        }

        #endregion

        #region Functions.SetValue
        /// <summary>
        /// 赋值到对象的属性或字段
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetPropertyOrFieldValue(object entity, string name, object value)
        {
            var _flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;

            var _property = entity.GetType().GetProperty(name, _flags);
            if (_property != null)
            {
                _property.SetValue(entity, value, null);
                return true;
            }

            var _field = entity.GetType().GetField(name, _flags);
            if (_field != null)
            {
                _field.SetValue(entity, value);
                return true;
            }

            return false;
        }
        #endregion

        #region Functions.ToTable

        /// <summary>
        /// Get:将实体对象转换为DataTable
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="entities">DataTable数据</param>
        /// <returns>实体类</returns>
        public static DataTable ToTable<TEntity>(IEnumerable<TEntity> entities)
        {
            var _type = typeof(TEntity);
            var _dict = new Dictionary<string, MemberInfo>();
            var _table = CreateTable(_type, ref _dict);

            if (entities == null) { return _table; }

            foreach (var _entity in entities)
            {
                var _row = _table.NewRow();
                foreach (var kv in _dict)
                {
                    if (kv.Key.Contains('.'))
                    {
                        var _names = kv.Key.Split('.');
                        var _value = GetPropertyOrFieldValue(_entity, _names, 0);
                        _row[kv.Key] = (_value == null) ? DBNull.Value : _value;
                        continue;
                    }
                    else
                    {
                        if (kv.Value is FieldInfo)
                        {
                            var _value = (kv.Value as FieldInfo).GetValue(_entity);
                            _row[kv.Key] = (_value == null) ? DBNull.Value : _value;
                        }
                        else if (kv.Value is PropertyInfo)
                        {
                            var _value = (kv.Value as PropertyInfo).GetValue(_entity, null);
                            _row[kv.Key] = (_value == null) ? DBNull.Value : _value;
                        }
                    }
                }
                _table.Rows.Add(_row);
            }
            return _table;
        }





        /// <summary>
        /// Get:将实体对象转换为DataTable
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="entities"></param>
        /// <param name="columnMaps"></param>
        /// <returns></returns>
        public static DataTable ToTable2(string tableName, IEnumerable<dynamic> entities, Dictionary<string, string> columnMaps)
        {
            var _dict = new Dictionary<string, MemberInfo>();

            var _table = new DataTable(tableName);
            if (entities != null && entities.Count() > 0)
            {
                dynamic Entity = entities.First();
                PropertyInfo[] infos = Entity.GetType().GetProperties();
                //foreach (PropertyInfo info in infos)
                //{
                //    _dict.Add(info.Name, info);
                //    _table.Columns.Add(new DataColumn(info.Name, info.PropertyType));
                //}
                foreach (KeyValuePair<string, string> info in columnMaps)
                {
                    if (infos.Where(a => a.Name.ToUpper() == info.Key.ToUpper()).Count() > 0)
                    {
                        _dict.Add(info.Key, infos.Where(a => a.Name.ToUpper() == info.Key.ToUpper()).First());
                        _table.Columns.Add(new DataColumn(info.Key, infos.Where(a => a.Name.ToUpper() == info.Key.ToUpper()).First().PropertyType));
                    }

                }
            }


            if (entities == null) { return _table; }

            foreach (var _entity in entities)
            {
                var _row = _table.NewRow();
                foreach (var kv in _dict)
                {
                    if (kv.Key.Contains('.'))
                    {
                        var _names = kv.Key.Split('.');
                        var _value = GetPropertyOrFieldValue(_entity, _names, 0);
                        _row[kv.Key] = (_value == null) ? DBNull.Value : _value;
                        continue;
                    }
                    else
                    {
                        if (kv.Value is FieldInfo)
                        {
                            var _value = (kv.Value as FieldInfo).GetValue(_entity);
                            _row[kv.Key] = (_value == null) ? DBNull.Value : _value;
                        }
                        else if (kv.Value is PropertyInfo)
                        {
                            var _value = (kv.Value as PropertyInfo).GetValue(_entity, null);
                            _row[kv.Key] = (_value == null) ? DBNull.Value : _value;
                        }
                        else
                        {
                            var _value = (kv.Value as PropertyInfo).GetValue(_entity, null);
                            _row[kv.Key] = (_value == null) ? DBNull.Value : _value;
                        }
                    }
                }
                _table.Rows.Add(_row);
            }
            return _table;
        }


        /// <summary>
        /// Get:将实体对象转换为DataTable
        /// </summary>
        /// <typeparam name="TEntity">实体类类型</typeparam>
        /// <param name="entity">DataTable数据</param>
        /// <returns>实体类</returns>
        public static DataTable ToTable<TEntity>(TEntity entity)
        {
            var _type = typeof(TEntity);
            var _dict = new Dictionary<string, MemberInfo>();
            var _table = CreateTable(_type, ref _dict);

            if (entity == null) { return _table; }

            var _row = _table.NewRow();
            foreach (var kv in _dict)
            {
                if (kv.Key.Contains('.'))
                {
                    var _names = kv.Key.Split('.');
                    var _value = GetPropertyOrFieldValue(entity, _names, 0);
                    _row[kv.Key] = (_value == null) ? DBNull.Value : _value;
                    continue;
                }
                else
                {
                    if (kv.Value is FieldInfo)
                    {
                        var _value = (kv.Value as FieldInfo).GetValue(entity);
                        _row[kv.Key] = (_value == null) ? DBNull.Value : _value;
                    }
                    else if (kv.Value is PropertyInfo)
                    {
                        var _value = (kv.Value as PropertyInfo).GetValue(entity, null);
                        _row[kv.Key] = (_value == null) ? DBNull.Value : _value;
                    }
                }
            }

            _table.Rows.Add(_row);
            return _table;
        }


        #endregion

    }
}

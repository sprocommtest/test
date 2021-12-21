using SwiftMES.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_DataBase
{
    public static class OrmUtility
    {
        #region Consts

        /// <summary> Get:[常量]实体类的反射标志 </summary>
        public const BindingFlags C_BINDINGS_FLAGS_ENTITY = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        #endregion

        #region Functions.DbAttribute

        /// <summary>
        /// Get:获取映射到数据库的数据表名称
        /// </summary>
        /// <param name="type">数据实体</param>
        /// <returns>DB映射值字典</returns>
        public static string GetDbTableName(this Type type)
        {
            if (type == null) { return ""; }

            var _attr = type.GetFirstAttribute<DbTableAttribute>();
            if (_attr == null) { return type.Name; }
            return _attr.Name;
        }

        /// <summary>
        /// Get:获取映射到数据库的数据表名称
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="entity">数据实体</param>
        /// <returns>DB映射值字典</returns>
        public static string GetDbTableName<TEntity>(this TEntity entity)
            where TEntity : class
        {
            return GetDbTableName(typeof(TEntity));
        }

        /// <summary>
        /// Get:获取映射到数据库的值字典
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="entity">数据实体</param>
        /// <returns>DB映射值字典</returns>
        public static Dictionary<string, object> GetDbValueDict<TEntity>(this TEntity entity)
            where TEntity : class
        {
            if (entity == null) { return new Dictionary<string, object>(); }

            var _dict = new Dictionary<string, object>();
            var _memberdict = entity.GetType().GetDict4MemberHasAttribute<DbFieldAttribute>();
            foreach (var kv in _memberdict)
            {
                var _columnname = kv.Value.ColumnName;
                if (kv.Key is FieldInfo)
                {
                    var _value = (kv.Key as FieldInfo).GetValue(entity);
                    if (_value == DBNull.Value) { _value = null; }
                    _dict.Add(_columnname, _value);
                    continue;
                }
                if (kv.Key is PropertyInfo)
                {
                    var _value = (kv.Key as PropertyInfo).GetValue(entity, null);
                    if (_value == DBNull.Value) { _value = null; }
                    _dict.Add(_columnname, _value);
                    continue;
                }
            }
            return _dict;
        }

        /// <summary>
        /// Get:获取用于新增数据的DB映射值字典
        /// (忽略的字段排除)
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="entity">数据实体</param>
        /// <returns>DB映射值字典</returns>
        public static Dictionary<string, object> GetDbValueDict4Insert<TEntity>(this TEntity entity)
            where TEntity : class
        {
            if (entity == null) { return new Dictionary<string, object>(); }

            var _dict = new Dictionary<string, object>();
            var _memberdict = entity.GetType().GetDict4MemberHasAttribute<DbFieldAttribute>();
            foreach (var kv in _memberdict)
            {
                if (kv.Key.IsExistAttribute<DbIgnoreAttribute>()) { continue; }
                if (kv.Key.IsExistAttribute<DbAutoIncrementAttribute>()) { continue; }

                var _columnname = kv.Value.ColumnName;
                if (kv.Key is FieldInfo)
                {
                    var _value = (kv.Key as FieldInfo).GetValue(entity);
                    if (_value == DBNull.Value) { _value = null; }
                    _dict.Add(_columnname, _value);
                    continue;
                }
                if (kv.Key is PropertyInfo)
                {
                    var _value = (kv.Key as PropertyInfo).GetValue(entity, null);
                    if (_value == DBNull.Value) { _value = null; }
                    _dict.Add(_columnname, _value);
                    continue;
                }
            }
            return _dict;
        }

        /// <summary>
        /// Get:获取用于更新数据的DB映射值字典
        /// (忽略、不更新、主键的字段排除)
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="entity">数据实体</param>
        /// <returns>DB映射值字典</returns>
        public static Dictionary<string, object> GetDbValueDict4Update<TEntity>(this TEntity entity)
            where TEntity : class
        {
            if (entity == null) { return new Dictionary<string, object>(); }

            var _dict = new Dictionary<string, object>();
            var _memberdict = entity.GetType().GetDict4MemberHasAttribute<DbFieldAttribute>();
            foreach (var kv in _memberdict)
            {
                if (kv.Key.IsExistAttribute<DbUnupdateAttribute>()) { continue; }
                if (kv.Key.IsExistAttribute<DbIgnoreAttribute>()) { continue; }
                if (kv.Key.IsExistAttribute<DbPrimaryAttribute>()) { continue; }

                var _columnname = kv.Value.ColumnName;
                if (kv.Key is FieldInfo)
                {
                    var _value = (kv.Key as FieldInfo).GetValue(entity);
                    if (_value == DBNull.Value) { _value = null; }
                    _dict.Add(_columnname, _value);
                    continue;
                }
                if (kv.Key is PropertyInfo)
                {
                    var _value = (kv.Key as PropertyInfo).GetValue(entity, null);
                    if (_value == DBNull.Value) { _value = null; }
                    _dict.Add(_columnname, _value);
                    continue;
                }
            }
            return _dict;
        }

        /// <summary>
        /// Get:获取表示主键的DB映射值字典
        /// （可用于单一数据的删除、更新、获取）
        /// </summary>
        /// <typeparam name="TEntity">数据实体类型</typeparam>
        /// <param name="entity">数据实体</param>
        /// <returns>DB映射值字典</returns>
        public static Dictionary<string, object> GetDbValueDict4Primary<TEntity>(this TEntity entity)
            where TEntity : class
        {
            if (entity == null) { return new Dictionary<string, object>(); }

            var _dict = new Dictionary<string, object>();
            var _memberdict = entity.GetType().GetDict4MemberHasAttribute<DbPrimaryAttribute>();
            foreach (var kv in _memberdict)
            {
                var _attr = kv.Key.GetFirstAttribute<DbFieldAttribute>();
                if (_attr == null) { continue; }
                var _columnname = _attr.ColumnName;
                if (kv.Key is FieldInfo)
                {
                    var _value = (kv.Key as FieldInfo).GetValue(entity);
                    if (_value == DBNull.Value) { _value = null; }
                    _dict.Add(_columnname, _value);
                    continue;
                }
                if (kv.Key is PropertyInfo)
                {
                    var _value = (kv.Key as PropertyInfo).GetValue(entity, null);
                    if (_value == DBNull.Value) { _value = null; }
                    _dict.Add(_columnname, _value);
                    continue;
                }
            }
            return _dict;
        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_DataBase
{
    /// <summary>
    /// 工具类：反射工具类
    /// </summary>
    public static class ReflectionUtility
    {

        #region Consts

        /// <summary> Get:[常量]实体类的反射标志 </summary>
        public const BindingFlags C_BINDINGS_FLAGS_ENTITY = BindingFlags.Instance | BindingFlags.Public;

        #endregion

        #region Fucntions.Attributes

        /// <summary>
        /// Get:获取符合条件的首个特性
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="member">成员</param>
        /// <returns></returns>
        public static TAttribute GetFirstAttribute<TAttribute>(this MemberInfo member)
            where TAttribute : Attribute
        {
            if (member == null) { return default(TAttribute); }

            var _attrs = member.GetCustomAttributes(typeof(TAttribute), true);
            if (_attrs == null) { return default(TAttribute); }
            if (_attrs.Length < 1) { return default(TAttribute); }

            var _attr = _attrs[0] as TAttribute;
            if (_attr == null) { return default(TAttribute); }
            return _attr;
        }

        /// <summary>
        /// Get:是否存在指定类型的特性
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="member">成员</param>
        /// <returns>是否存在指定类型的特性</returns>
        public static bool IsExistAttribute<TAttribute>(this MemberInfo member)
            where TAttribute : Attribute
        {
            if (member == null) { return false; }

            var _attrs = member.GetCustomAttributes(typeof(TAttribute), true);
            if (_attrs == null) { return false; }
            return (_attrs.Length > 0);
        }

        /// <summary>
        /// Get:获取指定类型的特性
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="member">成员</param>
        /// <returns>是否存在指定类型的特性</returns>
        public static int CountAttribute<TAttribute>(this MemberInfo member)
            where TAttribute : Attribute
        {
            if (member == null) { return 0; }

            var _attrs = member.GetCustomAttributes(typeof(TAttribute), true);
            if (_attrs == null) { return 0; }
            return _attrs.Length;
        }

        #endregion

        #region Fucntions.Attributes

        /// <summary>
        /// Get:获取附加了符合指定特性的成员列表
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="type">成员</param>
        /// <returns>附加了符合指定特性的成员列表</returns>
        public static List<MemberInfo> GetMembersHasAttribute<TAttribute>(this Type type)
            where TAttribute : Attribute
        {
            if (type == null) { return new List<MemberInfo>(); }

            var _fields = type.GetFields(C_BINDINGS_FLAGS_ENTITY);
            var _properties = type.GetProperties(C_BINDINGS_FLAGS_ENTITY);

            var _list = new List<MemberInfo>();

            foreach (var f in _fields)
            {
                var _attr = f.GetFirstAttribute<TAttribute>();
                if (_attr == null) { continue; }
                _list.Add(f);
            }

            foreach (var p in _properties)
            {
                var _attr = p.GetFirstAttribute<TAttribute>();
                if (_attr == null) { continue; }
                _list.Add(p);
            }
            return _list;
        }

        /// <summary>
        /// Get:获取附加了符合指定特性的成员字典
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="type">成员</param>
        /// <returns>附加了符合指定特性的成员字典</returns>
        public static Dictionary<MemberInfo, TAttribute> GetDict4MemberHasAttribute<TAttribute>(this Type type)
            where TAttribute : Attribute
        {
            if (type == null) { return new Dictionary<MemberInfo, TAttribute>(); }

            var _fields = type.GetFields(C_BINDINGS_FLAGS_ENTITY);
            var _properties = type.GetProperties(C_BINDINGS_FLAGS_ENTITY);

            var _dict = new Dictionary<MemberInfo, TAttribute>();

            foreach (var f in _fields)
            {
                var _attr = f.GetFirstAttribute<TAttribute>();
                if (_attr == null) { continue; }
                _dict.Add(f, _attr);
            }

            foreach (var p in _properties)
            {
                var _attr = p.GetFirstAttribute<TAttribute>();
                if (_attr == null) { continue; }
                _dict.Add(p, _attr);
            }
            return _dict;
        }

        #endregion

        #region Fucntions.InstanceMember

        /// <summary>
        /// Get:获取类型的字段字典
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>实体类</returns>
        public static Dictionary<string, FieldInfo> GetFieldDict(Type type)
        {
            if (type == null) { return new Dictionary<string, FieldInfo>(); }

            var _dict = new Dictionary<string, FieldInfo>();

            #region Get Members
            {
                var _flags = C_BINDINGS_FLAGS_ENTITY;
                var _fields = type.GetFields(_flags);

                foreach (var f in _fields)
                {
                    _dict.Add(f.Name, f);
                }
            }
            #endregion

            return _dict;
        }

        /// <summary>
        /// Get:获取类型的属性字典
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>实体类</returns>
        public static Dictionary<string, PropertyInfo> GetPropertyDict(Type type)
        {
            if (type == null) { return new Dictionary<string, PropertyInfo>(); }

            var _dict = new Dictionary<string, PropertyInfo>();

            #region Get Members
            {
                var _flags = C_BINDINGS_FLAGS_ENTITY;
                var _properties = type.GetProperties(_flags);

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
        /// Get:获取类型的字段与属性的字典
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>实体类</returns>
        public static Dictionary<string, MemberInfo> GetFieldAndPropertyDict(Type type)
        {
            if (type == null) { return new Dictionary<string, MemberInfo>(); }

            var _dict = new Dictionary<string, MemberInfo>();

            #region Get Members
            {
                var _flags = C_BINDINGS_FLAGS_ENTITY;

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

        #endregion

        #region Fucntions.InstanceMember.T

        /// <summary>
        /// Get:获取类型的字段与属性的字典
        /// </summary>
        /// <typeparam name="T">类型T</typeparam>
        /// <returns>实体类</returns>
        public static Dictionary<string, MemberInfo> GetFieldAndPropertyDict<T>()
        {
            return GetFieldAndPropertyDict(typeof(T));
        }

        /// <summary>
        /// Get:获取类型的字段字典
        /// </summary>
        /// <typeparam name="T">类型T</typeparam>
        /// <returns>实体类</returns>
        public static Dictionary<string, FieldInfo> GetFieldDict<T>()
        {
            return GetFieldDict(typeof(T));
        }

        /// <summary>
        /// Get:获取类型的属性字典
        /// </summary>
        /// <typeparam name="T">类型T</typeparam>
        /// <returns>实体类</returns>
        public static Dictionary<string, PropertyInfo> GetPropertyDict<T>()
        {
            return GetPropertyDict(typeof(T));
        }

        #endregion



    }
}

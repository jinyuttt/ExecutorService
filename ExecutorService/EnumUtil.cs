#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ExecutorTask
* 项目描述 ：
* 类 名 称 ：EnumUtil
* 类 描 述 ：
* 命名空间 ：ExecutorTask
* CLR 版本 ：4.0.30319.42000
* 作    者 ：jinyu
* 创建时间 ：2018
* 更新时间 ：2018
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ jinyu 2018. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace ExecutorService
{
    /* ============================================================================== 
    * 功能描述：EnumUtil  枚举处理
    * 创 建 者：jinyu 
    * 修 改 者：jinyu 
    * 创建日期：2018 
    * 修改日期：2018 
    * ==============================================================================*/

 public  static   class EnumUtil
    {
        private static ConcurrentDictionary<string, Dictionary<string, string>> dicEnum = new ConcurrentDictionary<string, Dictionary<string, string>>();
        public static string FetchDescription(this Enum value)
        {
            string key = value.GetType().Name;
            string key1 = value.ToString();
            string strValue = "";
            Dictionary<string, string> pairs = null;
            if (dicEnum.TryGetValue(key, out pairs))
            {
                if (pairs.TryGetValue(key1, out strValue))
                {
                    return strValue;
                }
            }
            else
            {
                pairs = new Dictionary<string, string>();
                dicEnum[key] = pairs;
            }
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])
            fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            strValue = (attributes.Length > 0) ? attributes[0].Description : value.ToString();
            pairs[key1] = strValue;
            return strValue;
        }
        /// <summary>
        /// 扩展类类型，自定义
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetFriendlyTypeName(this Type t)
        {
            int index = t.Name.LastIndexOf("`");
            string typeName = t.Name;
            if (index > -1)
            {
                typeName = t.Name.Substring(0, t.Name.LastIndexOf("`"));
            }
            var genericArgs = t.GetGenericArguments();
            if (genericArgs.Length > 0)
            {
                typeName += "<";
                foreach (var genericArg in genericArgs)
                {
                    typeName += genericArg.GetFriendlyTypeName() + ", ";
                }
                typeName = typeName.TrimEnd(',', ' ') + ">";
            }
            return typeName;
        }
    }
}

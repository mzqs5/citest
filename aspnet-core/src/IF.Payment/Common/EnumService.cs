using System;
using System.ComponentModel;
using System.Reflection;

namespace IF.Payment.Common
{
    public sealed class EnumService
    {
        public static string GetDescription(Enum obj)
        {
            string objName = obj.ToString();
            Type t = obj.GetType();
            FieldInfo fi = t.GetField(objName);
            DescriptionAttribute[] arrDesc = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return arrDesc[0].Description;
        }

        public static T GetAttribute<T>(Enum obj) where T: Attribute
        {
            string objName = obj.ToString();
            Type t = obj.GetType();
            FieldInfo fi = t.GetField(objName);
            return fi.GetCustomAttribute(typeof(T)) as T;
        }

        public static T GetEnum<T>(string description)
        {
            T t = default(T);
            var array = Enum.GetValues(typeof(T));
            foreach (var item in array)
            {
                if (item.ToString() != description)
                    continue;

                t = (T)Enum.Parse(typeof(T), ((int)item).ToString());
                break;
            }
            return t;
        }
    }
}

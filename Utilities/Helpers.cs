using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Helpers
{
    public static class EnumHelper
    {
        public static List<string> EnumNames<T>()
        {
            return Enum.GetNames(typeof(T)).ToList();
        }

        /// <summary>
        /// Gets an enum's description and retuns a list of the description
        /// </summary>
        /// <typeparam name="T">The type of enum you are converting</typeparam>
        /// <returns></returns>
        public static List<string> GetEnumDescriptionList<T>()
        {
            var enums = Enum.GetValues(typeof(T)).Cast<T>();

            var data = new List<string>();

            enums.ToList().ForEach(m => data.Add(GetEnumDescriptions((T)m)));

            return data;
        }

        public static string GetEnumDescriptions<T>(T enumValue)
        {
            var fi = enumValue.GetType().GetField(((T)enumValue).ToString());

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : enumValue.ToString();

        }

        public static IEnumerable<T> ConvertEnumToList<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }

}

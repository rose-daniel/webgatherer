using System;
using System.Collections;

namespace vip.common
{
    public class KVPair : DictionaryBase
    {
        public void Add(string fieldName, string value)
        {
            Dictionary.Add(fieldName, value);
        }

        public void Remove(string fieldName)
        {
            Dictionary.Remove(fieldName);
        }

        public KVPair()
        {
        }

        public bool ToBool(string name)
        {
            return this[name].ToLower() == "true" || this[name].ToString() == "1";
        }

        public DateTime ToDateTime(string date)
        {
            return Utils.GetDateTime(this[date]);
        }
        public int ToInt32(string number)
        {
            return Utils.GetInt32(this[number]);
        }
        public string this[string fieldName]
        {
            get
            {
                return (string)Dictionary[fieldName];
            }
            set
            {
                Dictionary[fieldName] = value;
            }
        }
        /// <summary>
        ///格式化显示时间 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public string FormatDate(string date, string format)
        {
            return Utils.ConvertDateString(this[date], format);
        }
    }
}

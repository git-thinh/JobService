using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class DictionaryExtension
{
    public static T Get<T>(this Dictionary<string, object> data, string key, T valDef = default(T))
    {
        if (data != null && data.ContainsKey(key))
        {
            try
            {
                var val = data[key];
                if (val != null) return (T)val;
            }
            catch { }
        }
        return valDef;
    }

    public static string Get(this Dictionary<string, object> data, string key)
    {
        if (data != null && data.ContainsKey(key))
        {
            try
            {
                var val = data[key];
                if (val != null) return (string)val;
            }
            catch { }
        }
        return string.Empty;
    }
}

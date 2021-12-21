using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MES_Web_Services
{
    public static class Json
    {
        public static string ModelToJson<T>(this T Model)
        {
            return JsonConvert.SerializeObject(Model);
        }

        public static T JsonToModel<T>(this string Json)
        {
            return JsonConvert.DeserializeObject<T>(Json);
        }
    }
}
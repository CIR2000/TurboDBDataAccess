using System;
using System.Collections.Generic;
using Amica.Model;

namespace Amica.Data
{
    public static class ObjectMapper
    {

        static Dictionary<Type, Dictionary<string, string>> dictMapper = new Dictionary<Type, Dictionary<string, string>>();

        static ObjectMapper()
        {
            Dictionary<string, string> dictObj;
            // Dictionary Key is Property Name of Business Object
            // Dictionary Value is Column Name of DataTable

            // Mapper for Company
            dictObj = new Dictionary<string, string>()
            {
                 { "Id", "Id"},
                 { "Name", "Nome"},
            };
            dictMapper.Add(typeof(Company), dictObj);

            // Mapper for Document
            dictObj = new Dictionary<string, string>()
            {
                 { "Id", "Id"},
                 { "Date", "Data"},
                 { "Number", "NumeroParteNumerica"},
                 { "NumberParam", "NumeroParteTesto"},
                 { "ContacName", ""},
                 { "Taxable", "TotaleImponibile"},
                 { "NotTaxable", "TotaleEsente"},
                 { "Tax", "TotaleImposta"},
                 { "Total", "TotaleFattura"},
            };
            dictMapper.Add(typeof(Document), dictObj);

            // Mapper for others business objects - TODO
            // ...
        }

        static public string GetFieldName(Type t, string propertyName)
        {
            // Alternative implentation -> return dictMapper.ContainsKey(t) ? (dictMapper[t].ContainsKey(propertyName) ? dictMapper[t][propertyName] : null) : null;
            if (dictMapper.ContainsKey(t))
                if (dictMapper[t].ContainsKey(propertyName))
                    return dictMapper[t][propertyName];
            return null;
        }
    }
}

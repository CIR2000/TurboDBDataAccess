using System;
using System.Collections.Generic;
using Amica;
using Amica.Data;
using Amica.Model;
using DataAccess;
using DataWeb.TurboDB;


namespace Amica.Data
{
    public class TurboDbDataReader : SqlDataReader
    {
        private TurboDBConnection defaultConn;

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<Resources, string> Res = new Dictionary<Resources, string>()
        {
            { Resources.Accounts, "Utenti"},
            { Resources.Companies, "Aziende"},
            { Resources.Dashoard, ""},
        };

        /// <summary>
        /// 
        /// </summary>
        public TurboDbDataReader()
        {
            ResourcesToData = Res;
        }

        public TurboDbDataReader(string dataSourceName, Authentication authentication) : this ()
        {
            DataSourceName = dataSourceName;
            Authentication = authentication;
            defaultConn = new TurboDBConnection(dataSourceName);
            defaultConn.ConnectionString = "";
            defaultConn.Exclusive = false;

        }

        /// <summary>
        /// 
        /// </summary>
        protected new Dictionary<Resources, string> ResourcesToData { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Response<T> Get<T>(GetRequest request)
        {
            string s = ParseFilters(request.Filters);
            TurboDBConnection conn = new TurboDBConnection(request.DataSourceName);
            conn.ConnectionString = "";
            conn.Exclusive = false;
            
            // System.Diagnostics.Debug.Print(s);
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Response<T> Get<T>(GetRequestItem request)
        {
            return null;
        }
    }
}

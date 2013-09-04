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
            defaultConn = new TurboDBConnection(DataSourceName);
            defaultConn.ConnectionString = "Datasource=" + DataSourceName + ";Exclusive=False;";
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
            TurboDBConnection conn;
            string s = ParseFilters(request.Filters);
            if (request.DataSourceName != null && request.DataSourceName.Length > 0)
                conn = new TurboDBConnection(request.DataSourceName);
            else
                if (DataSourceName != null && DataSourceName.Length > 0)
                    conn = new TurboDBConnection(DataSourceName);
                else
                    return null;    // TODO errore
            conn.Exclusive = false;
            conn.Open();

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

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
        private TurboDBConnection _defaultConn;
        //private string _currentDatabasePassword;

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
            _defaultConn = new TurboDBConnection(DataSourceName);
            _defaultConn.ConnectionString = "Datasource=" + DataSourceName + ";Exclusive=False;";
            _defaultConn.Exclusive = false;

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
            {
                conn = new TurboDBConnection(request.DataSourceName);
                conn.Exclusive = false;
            }
            else
            {
                if (_defaultConn.State != System.Data.ConnectionState.Open)
                    if (DataSourceName != null && DataSourceName.Length > 0)
                    {
                        _defaultConn = new TurboDBConnection(DataSourceName);
                        _defaultConn.Exclusive = false;
                    }
                    else
                        return null;    // TODO generate error
                conn = _defaultConn;
            }

            
            // Event subscription
            // conn.PasswordNeeded += turboDBConnection_PasswordNeeded;
            conn.Open();

            // System.Diagnostics.Debug.Print(s);
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void turboDBConnection_PasswordNeeded(object sender, DataWeb.TurboDB.TurboDBPasswordNeededEventArgs e)
        //{
        //    if (DataSourcePassword != null && DataSourcePassword.Length > 0)
        //    {
        //        e.Password = DataSourcePassword;
        //        e.Retry = true;
        //    }
        //}
        
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

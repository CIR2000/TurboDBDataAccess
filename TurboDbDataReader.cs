using System;
using System.Collections.Generic;
using Amica;
using Amica.Data;
using Amica.Model;
using DataAccess;
using DataWeb.TurboDB;
using System.Data;

namespace Amica.Data
{
    public class TurboDbDataReader : SqlDataReader
    {
        private TurboDBConnection _defaultConn;
        private string _currentDatabasePassword;

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
        public override Response<T> Get<T>(IGetRequest request)
        {
            TurboDBConnection conn;
            string s = ParseFilters(request.Filters);
            // System.Diagnostics.Debug.Print(s);

            // Set (and open if needed) connection properly from request or class properties
            if (request.DataSourceName != null && request.DataSourceName.Length > 0)
            {
                conn = new TurboDBConnection(request.DataSourceName);
                conn.Exclusive = false;
                conn.Open();
            }
            else
            {
                if (_defaultConn.State != System.Data.ConnectionState.Open)
                    if (DataSourceName != null && DataSourceName.Length > 0)
                    {
                        _defaultConn = new TurboDBConnection(DataSourceName);
                        _defaultConn.Exclusive = false;
                        _defaultConn.Open();
                    }
                    else
                        return null;    // TODO generate error
                conn = _defaultConn;
            }

            // TODO add try to handle if conversion to SqlGetRequest fail (can fail if objest isn't sqlgetrequest)
            // Set password from request or class properties
            _currentDatabasePassword = null;
            try
            {
                SqlGetRequest sqlreq = (SqlGetRequest)request;
                _currentDatabasePassword = sqlreq.DataSourcePassword;                    
            }
            catch {}

            if (_currentDatabasePassword == null)
                _currentDatabasePassword = DataSourcePassword;
            
            // Event subscription
            conn.PasswordNeeded += turboDBConnection_PasswordNeeded;

            // Reading data from database
            DataTable list = new DataTable();
            TurboDBDataAdapter apt = new TurboDBDataAdapter("SELECT * FROM " + request.Resource, conn);
            apt.Fill(list);

            System.Diagnostics.Debug.Print(DateTime.Now.ToString() + " Record letti: " + list.Rows.Count);
            return null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Response<T> Get<T>(IGetRequestItem request)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void turboDBConnection_PasswordNeeded(object sender, DataWeb.TurboDB.TurboDBPasswordNeededEventArgs e)
        {
            if (_currentDatabasePassword != null)
            {
                e.Password = _currentDatabasePassword;
                e.Retry = true;
            }
        }

    }
}

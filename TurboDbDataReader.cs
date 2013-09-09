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
            DataTable list = GetDataTable(request);
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

        private DataTable GetDataTable(IGetRequest request)
        {
            SetPassword(request);
            TurboDBConnection conn = GetConnection(request);
            
            // Event subscription for databases with table password
            conn.PasswordNeeded += turboDBConnection_PasswordNeeded;

            // Reading data from database
            string sqlString = request.GetType() == typeof(SqlGetRequestItem) ? BuildSqlIdString(request) : BuildSqlIdString(request);
            TurboDBDataAdapter apt = new TurboDBDataAdapter(sqlString, conn);
            DataTable list = new DataTable();
            apt.Fill(list);
            return list;
        }

        private void SetPassword(IGetRequest request)
        {
            // Set password from request or class properties
            _currentDatabasePassword = null;
            try
            {
                SqlGetRequest sqlreq = (SqlGetRequest)request;
                _currentDatabasePassword = sqlreq.DataSourcePassword;
            }
            catch { }
            _currentDatabasePassword = _currentDatabasePassword == null ? _currentDatabasePassword = DataSourcePassword : null;
        }

        private TurboDBConnection GetConnection(IGetRequest request)
        {
            TurboDBConnection conn;

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
            return conn;
        }

        /// <summary>
        /// Build SQL string from request info
        /// </summary>
        /// <param name="request">Request from which to construct the SqlString</param>
        private string BuildSqlString(IGetRequest request)
        {
            string sqlSelect = "SELECT * FROM " + request.Resource;
            string sqlFilter = ParseFilters(request.Filters);
            string sqlOrder = ParseSorts(request.Sort);
            sqlSelect += sqlFilter != "" ? " WHERE " + sqlFilter : "";
            sqlSelect += sqlFilter != "" ? " ORDER BY " + sqlOrder : "";
            //System.Diagnostics.Debug.Print(sqlSelect);
            return sqlSelect;
        }

        private string BuildSqlIdString(IGetRequest request)
        {
            string sqlSelect = "SELECT * FROM " + request.Resource + " WHERE id=" + request;
            return sqlSelect;
        }

        /// <summary>
        /// Set password for table when called from PasswordNeeded event
        /// </summary>
        /// <param name="sender">Object that generated the event</param>
        /// <param name="e">Event parameters object</param>
        private void turboDBConnection_PasswordNeeded(object sender, TurboDBPasswordNeededEventArgs e)
        {
            if (_currentDatabasePassword != null)
            {
                e.Password = _currentDatabasePassword;
                e.Retry = true;
            }
        }

    }
}

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
        public TurboDbDataReader()
        {
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
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Response<T> Get<T>(IGetRequest request)
        {
            if (request is SqlGetRequest)
            {
                DataTable list = GetDataTable((SqlGetRequest)request);
                System.Diagnostics.Debug.Print(DateTime.Now.ToString() + " Record letti: " + list.Rows.Count);
                return null;
            }
            else 
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
            if (request is SqlGetRequestItem)
            {
                SqlGetRequest req = (SqlGetRequest)request;
                req.Filters.Add(new Filter("Id", Comparison.Equal, request.Id.ToString()));
                DataTable list = GetDataTable(req);
                System.Diagnostics.Debug.Print(DateTime.Now.ToString() + " Record letti: " + list.Rows.Count);
                return null;
            }
            else
                return null;
        }

        private DataTable GetDataTable (SqlGetRequest request)
        {
            // Reading data from database
            TurboDBConnection conn = GetConnection(request.DataSourceName, request.DataSourcePassword);
            TurboDBDataAdapter apt = new TurboDBDataAdapter(BuildSqlString(request), conn);
            DataTable list = new DataTable();
            apt.Fill(list);
            return list;
        }

        private TurboDBConnection GetConnection(string tbName, string pw)
        {
            TurboDBConnection conn;

            // Set (and open if needed) connection properly from request or class properties
            if (tbName != null && tbName.Length > 0)
            {
                conn = new TurboDBConnection(tbName);
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
            // Set password from request or class properties
            _currentDatabasePassword = pw != null ? pw : DataSourcePassword;
            // Event subscription for databases with table password
            conn.PasswordNeeded += turboDBConnection_PasswordNeeded;

            return conn;
        }

        /// <summary>
        /// Build SQL string from request info
        /// </summary>
        /// <param name="request">Request from which to construct the SqlString</param>
        private string BuildSqlString(SqlGetRequest request)
        {
            string sqlSelect = "SELECT * FROM " + request.Resource;
            string sqlFilter = ParseFilters(request.Filters);
            string sqlOrder = ParseSorts(request.Sort);
            sqlSelect += sqlFilter != "" ? " WHERE " + sqlFilter : "";
            sqlSelect += sqlFilter != "" ? " ORDER BY " + sqlOrder : "";
            //System.Diagnostics.Debug.Print(sqlSelect);
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

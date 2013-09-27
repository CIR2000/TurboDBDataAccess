using System;
using System.Collections.Generic;
using System.Reflection;
using Amica;
using Amica.Data;
using Amica.Model;
using DataAccess;
using DataWeb.TurboDB;
using System.Data;
using System.Linq;

namespace Amica.Data
{
    public class TurboDbDataAccessBase : SqlDataAccessBase
    {
        private TurboDBConnection _defaultConn;     // private variable 
        private string _currentDatabasePassword;

        /// <summary>
        /// Constructor without parameters
        /// </summary>
        public TurboDbDataAccessBase()
        {
        }

        /// <summary>
        /// Constructor with dataSourceName and authentication
        /// </summary>
        /// <param name="dataSourceName">Name of datasource (DataBase) origin of data</param>
        /// <param name="authentication">Authentication data; username and password</param>
        public TurboDbDataAccessBase(string dataSourceName, Authentication authentication) : this ()
        {
            DataSourceName = dataSourceName;
            Authentication = authentication;
            _defaultConn = new TurboDBConnection(DataSourceName);
            
            _defaultConn.ConnectionString = "Datasource=" + DataSourceName + ";Exclusive=False;";
            _defaultConn.Exclusive = false;
        }

        /// <summary>
        /// Execute the specified request.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <typeparam name="T">The expected return type .</typeparam>
        public override Response<T> Execute<T>(IRequest request)
        {
            if (!(request is SqlRequest))
                return null;

            if (request.Method == Methods.Read)
            {
                return ExecuteRead<T>((SqlRequest)request);
            }
            return null;
        }

        /// <summary>
        /// Executes an async request.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <param name="callback">Callback.</param>
        /// <typeparam name="T">The expected return type.</typeparam>
        public override void ExecuteAsync<T>(IRequest request, Action<Response<T>, IRequest> callback)
        {
            // TODO return response: not implemented
        }

        /// <summary>
        /// Method that execute the read request
        /// </summary>
        /// <typeparam name="T">Return type parameter for property Content in response</typeparam>
        /// <param name="request">The request for data from DataSource</param>
        /// <returns></returns>
        private Response<T> ExecuteRead<T>(SqlRequest request) where T : new()
        {
            DataTable tb = GetDataTable(request);
            if (tb.Rows.Count > 0)
            {
                if (request.DocumentId == null)
                {
                    Type t = new T().GetType().GetGenericArguments()[0];
                    var obj = Activator.CreateInstance(t);
                    Response<T> resp = new Response<T>();
                    resp.Content = new T();
                    object[] a = new object[1];

                    foreach (DataRow dr in tb.Rows)
                    {
                        obj = Activator.CreateInstance(t);
                        FillObject(obj, dr);
                        a[0] = obj;
                        resp.Content.GetType().GetMethod("Add").Invoke(resp.Content, a);
                    }
                    resp.StatusCode = StatusCode.Accepted;
                    resp.ResponseStatus = ResponseStatus.Completed;
                    return resp;
                }
                else
                {
                    T obj = new T();
                    FillObject(obj, tb.Rows[0]);
                    Response<T> resp = new Response<T>();
                    resp.Content = obj;
                    resp.StatusCode = StatusCode.Accepted;
                    resp.ResponseStatus = ResponseStatus.Completed;
                    return resp;
                }
            }
            else
            {
                Response<T> resp = new Response<T>();
                resp.StatusCode = StatusCode.Accepted;
                resp.ResponseStatus = ResponseStatus.Error;
                resp.ErrorMessage = "Item not found";
                return resp;
            }
        }

        /// <summary>
        /// Method that fills the object or the contents of the list to be returned 
        /// </summary>
        /// <typeparam name="T">Specified object type to be filled</typeparam>
        /// <param name="obj">Object to fill</param>
        /// <param name="dr">DataRow containing the data to fill the object</param>
        private void FillObject<T>(T obj, DataRow dr)
        {
            IList<PropertyInfo> props = new List<PropertyInfo>(obj.GetType().GetProperties());
            string fieldName = null;
            object value;
            try
            {
                foreach (PropertyInfo prop in props)
                {
                    fieldName = ObjectMapper.GetFieldName(obj.GetType(), prop.Name);
                    if (fieldName != null)
                    {
                        value = dr[fieldName] is DBNull ? null : dr[fieldName];
                        prop.SetValue(obj, (dr[fieldName] is double & prop.GetValue(obj) is decimal) ? Convert.ToDecimal(value) : value, null);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Read datatable from database with request parameters
        /// </summary>
        /// <param name="request">The request with all parameters</param>
        /// <returns>A datatable filled with results</returns>
        private DataTable GetDataTable (SqlRequest request)
        {
            // Reading data from database
            TurboDBConnection conn = GetConnection(request.DataSourceName, request.DataSourcePassword);
            TurboDBDataAdapter apt = new TurboDBDataAdapter(BuildSqlString(request), conn);
            DataTable tb = new DataTable();
            apt.Fill(tb);
            return tb;
        }

        /// <summary>
        /// Select and open if needed the properly connection to database
        /// </summary>
        /// <param name="tbName"></param>
        /// <param name="pw"></param>
        /// <returns></returns>
        private TurboDBConnection GetConnection(string dbName, string pw)
        {
            TurboDBConnection conn;
            
            // Set (and open if needed) connection properly from request or class properties
            if (dbName != null && dbName.Length > 0)
            {
                conn = new TurboDBConnection(dbName);
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

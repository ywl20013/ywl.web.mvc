using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ywl.Data.Entity
{
    /// <summary>
    /// 拦截生成的SQL语句
    /// </summary>
    public class EFInterceptorLogging : DbCommandInterceptor
    {
        private readonly Stopwatch stopWatch = new Stopwatch();
        private readonly String logFilePath = "";
        private readonly String logFileName = "";
        private int _counter = 0;

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            DbInterception.Add(new EFInterceptorLogging());
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public EFInterceptorLogging()
        {
            //string path = System.Configuration.ConfigurationSettings.AppSettings["log_path"];
            string path = System.Configuration.ConfigurationManager.AppSettings["log_path"];
            string name = System.Configuration.ConfigurationManager.AppSettings["log_name"];
            if (path != null && path != "")
            {
                logFilePath = path;
            }
            else
            {
                logFilePath = "~/Logs/";

            }
            if (name != null && name != "")
            {
                logFileName = name;
            }
            else
            {
                logFileName = "db.log";
            }
        }

        /// <summary>
        /// 非读取
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public override void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            var url = "";
            if (HttpContext.Current != null)
            {
                var request = HttpContext.Current.Request;
                if (request != null) url = request.Url.PathAndQuery;
            }
            WriteLog("\r\n时间：" + DateTime.Now + "\r\n");
            WriteLog("Url：" + url + "\r\n");
            WriteLog("线程：" + System.Threading.Thread.CurrentThread.ManagedThreadId + "\r\n");
            WriteLog("事件：NonQueryExecuting\r\n");
            WriteLog("SQL：" + command.CommandText + "\r\n");
            WriteLog("Parameters.Count：" + command.Parameters.Count.ToString() + "\r\n");
            if (command.Parameters.Count > 0)
            {
                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    WriteLog("\tParameters[" + i + "]: " + command.Parameters[i].ParameterName + " = " + command.Parameters[i].Value.ToString() + "\r\n");
                }
            }

            if (interceptionContext.OriginalException != null)
            {
                WriteLog("Exception:{1} \r\n --> Error executing command: {0}", command.CommandText, interceptionContext.OriginalException.ToString());
            }

            if (interceptionContext.Exception != null)
            {
                Trace.TraceError("Exception:{1} \r\n --> Error executing command: {0}", command.CommandText, interceptionContext.Exception.ToString());
                WriteLog("Exception:{1} \r\n --> Error executing command: {0}", command.CommandText, interceptionContext.Exception.ToString());
            }
            else
            {
                for (int i = 0; i < interceptionContext.ObjectContexts.Count(); i++)
                {
                    var context = interceptionContext.ObjectContexts.ElementAt(i);
                    Trace.TraceInformation("\r\nObjectContexts[{0}].name:{1}", i, context.GetType().Name);
                    WriteLog("ObjectContexts[{0}].name:{1}", i, context.GetType().FullName);
                }
            }
            WriteLog("\r\n");
            base.NonQueryExecuting(command, interceptionContext);
            stopWatch.Restart();
        }

        /// <summary>
        /// 非读取
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public override void NonQueryExecuted(System.Data.Common.DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            stopWatch.Stop();
            Trace.WriteLine("线程：" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            var url = "";
            if (HttpContext.Current != null)
            {
                var request = HttpContext.Current.Request;
                if (request != null) url = request.Url.PathAndQuery;
            }
            WriteLog("\r\n时间：" + DateTime.Now + "\r\n");
            WriteLog("Url：" + url + "\r\n");
            WriteLog("线程：" + System.Threading.Thread.CurrentThread.ManagedThreadId + "\r\n");
            WriteLog("事件：NonQueryExecuted\r\n");
            WriteLog("SQL：" + command.CommandText + "\r\n");
            WriteLog("耗时:{0} 毫秒", stopWatch.ElapsedMilliseconds);
            WriteLog("\r\n");
            base.NonQueryExecuted(command, interceptionContext);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public override void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            var url = "";
            if (HttpContext.Current != null)
            {
                var request = HttpContext.Current.Request;
                if (request != null) url = request.Url.PathAndQuery;
            }
            WriteLog("\r\n时间：" + DateTime.Now + "\r\n");
            WriteLog("Url：" + url + "\r\n");
            WriteLog("线程：" + System.Threading.Thread.CurrentThread.ManagedThreadId + "\r\n");
            WriteLog("事件：ReaderExecuting\r\n");
            WriteLog("CommandType：" + command.CommandType.ToString() + "\r\n");
            WriteLog("SQL：\r\n");
            WriteLog("================================\r\n");
            WriteLog(command.CommandText + "\r\n");
            WriteLog("Parameters.Count：" + command.Parameters.Count.ToString() + "\r\n");
            if (command.Parameters.Count > 0)
            {
                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    WriteLog("\tParameters[" + i + "] " + command.Parameters[i].Direction.ToString() + ": " +
                        command.Parameters[i].ParameterName + " = " +
                        (command.Parameters[i].Value == null ? "" : command.Parameters[i].Value.ToString()) + "\r\n");
                }
            }
            WriteLog("================================\r\n");
            bool throwTransientErrors = false;
            if (command.Parameters.Count > 0 && command.Parameters[0].Value.ToString() == "%Throw%")
            {
                throwTransientErrors = true;
                command.Parameters[0].Value = "%an%";
                command.Parameters[1].Value = "%an%";
            }
            if (throwTransientErrors && _counter < 4)
            {
                //_logger.Information("Returning transient error for command: {0}", command.CommandText);
                _counter++;
                interceptionContext.Exception = CreateDummySqlException();
            }
            if (command.Parameters.Count > 0 && command.Parameters[0].Value.ToString() == "%Throw%")
            {
                //command.Parameters[0].Value = "%an%";
                //command.Parameters[1].Value = "%an%";
                WriteLog("Exception:{0} \r\n", command.Parameters[0].Value.ToString());
            }

            if (interceptionContext.OriginalException != null)
            {
                WriteLog("Exception:{0} \r\n", interceptionContext.OriginalException.Message);
            }

            if (interceptionContext.Exception != null)
            {
                //Trace.TraceError("Exception:{1} \r\n --> Error executing command: {0}", command.CommandText, interceptionContext.Exception.ToString());
                //WriteLog("Exception:{1} \r\n --> Error executing command: {0}", command.CommandText, interceptionContext.Exception.ToString());
                WriteLog("Exception:{0} \r\n", interceptionContext.Exception.Message);
            }
            //else
            //{
            //    for (int i = 0; i < interceptionContext.ObjectContexts.Count(); i++)
            //    {
            //        var context = interceptionContext.ObjectContexts.ElementAt(i);
            //        Trace.TraceInformation("\r\nObjectContexts[{0}].name:{1}", i, context.GetType().Name);
            //        WriteLog("ObjectContexts[{0}].name:{1}", i, context.GetType().FullName);
            //    }
            //}
            WriteLog("\r\n");

            base.ReaderExecuting(command, interceptionContext);
            stopWatch.Restart();
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public override void ReaderExecuted(System.Data.Common.DbCommand command, DbCommandInterceptionContext<System.Data.Common.DbDataReader> interceptionContext)
        {
            stopWatch.Stop();
            Trace.WriteLine("线程：" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            var url = "";
            if (HttpContext.Current != null)
            {
                var request = HttpContext.Current.Request;
                if (request != null) url = request.Url.PathAndQuery;
            }
            WriteLog("\r\n时间：" + DateTime.Now + "\r\n");
            WriteLog("Url：" + url + "\r\n");
            WriteLog("线程：" + System.Threading.Thread.CurrentThread.ManagedThreadId + "\r\n");
            WriteLog("事件：ReaderExecuted\r\n");
            WriteLog("CommandType：" + command.CommandType.ToString() + "\r\n");
            WriteLog("SQL：\r\n");
            WriteLog("================================\r\n");
            WriteLog(command.CommandText + "\r\n");
            WriteLog("Parameters.Count：" + command.Parameters.Count.ToString() + "\r\n");
            if (command.Parameters.Count > 0)
            {
                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    WriteLog("\tParameters[" + i + "] " + command.Parameters[i].Direction.ToString() + ": " +
                        command.Parameters[i].ParameterName + " = " +
                        (command.Parameters[i].Value == null ? "" : command.Parameters[i].Value.ToString()) + "\r\n");
                }
            }
            WriteLog("================================\r\n");
            WriteLog("耗时:{0} 毫秒", stopWatch.ElapsedMilliseconds);

            if (interceptionContext.OriginalException != null)
            {
                WriteLog("Exception:{0} \r\n", interceptionContext.OriginalException.Message);
            }
            if (interceptionContext.Exception != null)
            {
                WriteLog("Exception:{0} \r\n", interceptionContext.Exception.Message);
            }

            WriteLog("\r\n");
            base.ReaderExecuted(command, interceptionContext);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public override void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            //System.Diagnostics.Debug.WriteLine("ScalarExecuting System.Threading.Thread.CurrentThread.Name " + command.CommandText);
            var url = "";
            if (HttpContext.Current != null)
            {
                var request = HttpContext.Current.Request;
                if (request != null) url = request.Url.PathAndQuery;
            }
            WriteLog("\r\n时间：" + DateTime.Now + "\r\n");
            WriteLog("Url：" + url + "\r\n");
            WriteLog("线程：" + System.Threading.Thread.CurrentThread.ManagedThreadId + "\r\n");
            WriteLog("事件：ScalarExecuting\r\n");
            WriteLog("SQL：" + command.CommandText + "\r\n");
            WriteLog("Parameters.Count：" + command.Parameters.Count.ToString() + "\r\n");
            if (command.Parameters.Count > 0)
            {
                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    WriteLog("\tParameters[" + i + "]: " + command.Parameters[i].ParameterName + " = " + command.Parameters[i].Value.ToString() + "\r\n");
                }
            }

            WriteLog("耗时:{0} 毫秒", stopWatch.ElapsedMilliseconds);
            WriteLog("\r\n");
            base.ScalarExecuting(command, interceptionContext);
            stopWatch.Restart();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public override void ScalarExecuted(System.Data.Common.DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            stopWatch.Stop();
            var url = "";
            if (HttpContext.Current != null)
            {
                var request = HttpContext.Current.Request;
                if (request != null) url = request.Url.PathAndQuery;
            }
            WriteLog("\r\n时间：" + DateTime.Now + "\r\n");
            WriteLog("Url：" + url + "\r\n");
            WriteLog("线程：" + System.Threading.Thread.CurrentThread.ManagedThreadId + "\r\n");
            WriteLog("事件：ScalarExecuted\r\n");
            WriteLog("CommandText：" + command.CommandText + "\r\n");
            if (interceptionContext.Exception != null)
            {
                Trace.TraceError("Exception:{1} \r\n --> Error executing command: {0}", command.CommandText, interceptionContext.Exception.ToString());
                WriteLog("Exception:{1} \r\n --> Error executing command: {0}", command.CommandText, interceptionContext.Exception.ToString());
            }
            else
            {
                Trace.TraceInformation("\r\n执行时间:{0} 毫秒\r\n-->ScalarExecuted.Command:{1}\r\n", stopWatch.ElapsedMilliseconds, command.CommandText);
                WriteLog("执行时间:{0} 毫秒\r\n-->ScalarExecuted.Command:\r\n{1}\r\n", stopWatch.ElapsedMilliseconds, command.CommandText);

                for (int i = 0; i < interceptionContext.ObjectContexts.Count(); i++)
                {
                    var context = interceptionContext.ObjectContexts.ElementAt(i);
                    Trace.TraceInformation("\r\nObjectContexts[{0}].name:{1}", i, context.GetType().Name);
                    WriteLog("ObjectContexts[{0}].name:{1}", i, context.GetType().FullName);
                }
            }
            WriteLog("\r\n");
            base.ScalarExecuted(command, interceptionContext);
        }


        private bool CreateFolderIfNeeded(string path)
        {
            bool result = true;
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    //TODO：处理异常
                    System.Diagnostics.Debug.WriteLine("Create Folder \"" + path + "\" Error: " + e.Message);
                    result = false;
                }
            }
            return result;
        }
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="msg">消息</param>
        private void WriteLog(string msg)
        {
            var filePath = this.logFilePath + DateTime.Now.ToString("yyyy/MM/");
            if (HttpContext.Current == null) return;
            filePath = HttpContext.Current.Server.MapPath(filePath);
            if (CreateFolderIfNeeded(filePath))
            {
                var fileName = filePath + "db." + DateTime.Now.ToString("yyyy.MM.dd HH") + ".log";
                //指定true表示追加
                lock (this)
                {
                    using (System.IO.TextWriter writer = new System.IO.StreamWriter(fileName, true))
                    {
                        writer.Write(msg);
                    }
                }
            }

        }
        private void WriteLog(string format, params object[] args)
        {
            var filePath = this.logFilePath + DateTime.Now.ToString("yyyy/MM/");
            if (HttpContext.Current == null) return;
            filePath = HttpContext.Current.Server.MapPath(filePath);
            if (CreateFolderIfNeeded(filePath))
            {
                var fileName = filePath + "db." + DateTime.Now.ToString("yyyy.MM.dd HH") + ".log";
                //指定true表示追加
                //using (System.IO.TextWriter writer = new System.IO.StreamWriter(fileName, true))
                //{
                //    writer.Write(format, args);
                //}
                lock (this)
                {
                    using (System.IO.TextWriter writer = new System.IO.StreamWriter(fileName, true))
                    {
                        writer.Write(format, args);
                    }
                }
            }
        }

        private SqlException CreateDummySqlException()
        {
            // The instance of SQL Server you attempted to connect to does not support encryption
            var sqlErrorNumber = 20;

            var sqlErrorCtor = typeof(SqlError).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Where(c => c.GetParameters().Count() == 7).Single();
            var sqlError = sqlErrorCtor.Invoke(new object[] { sqlErrorNumber, (byte)0, (byte)0, "", "", "", 1 });

            var errorCollection = Activator.CreateInstance(typeof(SqlErrorCollection), true);
            var addMethod = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic);
            addMethod.Invoke(errorCollection, new[] { sqlError });

            var sqlExceptionCtor = typeof(SqlException).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Where(c => c.GetParameters().Count() == 4).Single();
            var sqlException = (SqlException)sqlExceptionCtor.Invoke(new object[] { "Dummy", errorCollection, null, Guid.NewGuid() });

            return sqlException;
        }
    }
}

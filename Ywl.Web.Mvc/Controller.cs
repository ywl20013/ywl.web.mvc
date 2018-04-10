using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Web.Configuration;

namespace Ywl.Web.Mvc
{
    public class Controller : System.Web.Mvc.Controller
    {
        #region 基础

        protected readonly String logFilePath = "";
        protected readonly String logFileName = "";
        private readonly Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// 获取当前控制器名称,不包含字符串"Controller"
        /// </summary>
        /// <returns></returns>
        public virtual string GetControllerName()
        {
            string className = this.GetType().Name;
            return className.Replace("Controller", "");
        }

        protected class ObjectDescription
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Namespace { get; set; }
        }

        /// <summary>
        /// 获取所有命名空间
        /// </summary>
        /// <returns>IQueryable<ObjectDescription></returns>
        protected IQueryable<ObjectDescription> NameSpaces()
        {
            List<ObjectDescription> result = new List<ObjectDescription>();
            List<Type> types = new List<Type>();
            List<string> namespaces = new List<string>();

            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (System.Reflection.Assembly assembly in assemblies)
            {
                var _types = assembly.GetTypes();
                foreach (Type typ in _types)
                    namespaces.Add(typ.Namespace);
            }
            var items = namespaces.Distinct();
            foreach (var item in items)
            {
                result.Add(new ObjectDescription { Id = 0, Namespace = item, Name = item, Description = item });
            }
            return result.OrderBy(e => e.Name).AsQueryable();
        }

        /// <summary>
        /// 获取所有已加载模块的控制器
        /// </summary>
        /// <returns></returns>
        protected List<ObjectDescription> Controllers()
        {
            List<ObjectDescription> result = new List<ObjectDescription>();
            List<Type> types = new List<Type>();

            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (System.Reflection.Assembly assembly in assemblies)
            {
                var _types = assembly.GetTypes();
                foreach (Type typ in _types)
                    types.Add(typ);
            }

            var items = types
               .Where(t => t.Name.Contains("Controller"))
               .Select(t => new
               {
                   Namespace = t.Namespace,
                   Name = t.Name.Replace("Controller", ""),
                   Description =
                   (DescriptionAttribute)Attribute.GetCustomAttributes(t).Where(a => a is DescriptionAttribute).FirstOrDefault() == null ? t.Name.Replace("Controller", "") :
                   ((DescriptionAttribute)Attribute.GetCustomAttributes(t).Where(a => a is DescriptionAttribute).FirstOrDefault()).Title
               });

            foreach (var item in items)
            {
                ObjectDescription cd = new ObjectDescription();
                cd.Name = item.Name;
                cd.Description = item.Description;
                cd.Namespace = item.Namespace;
                result.Add(cd);
            }
            return result;
        }

        #endregion

        #region 返回值

        /// <summary>
        /// 返回Json格式数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        protected new JsonResult Json(object data)
        {
            return Json(data, null, null, JsonRequestBehavior.AllowGet);
        }

        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new NewtonsoftJsonResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = int.MaxValue
            };
        }

        public JsonResult OK()
        {
            return Json(new { done = true });
        }
        /// <summary>
        /// 返回正确信息
        /// </summary>
        /// <param name="msg">正确信息</param>
        /// <param name="asPage">返回页面</param>
        /// <returns>网页视图</returns>
        public ActionResult OK(object msg, bool asPage = false)
        {
            if (!asPage)
            {
                return Json(new { done = true, msg = msg });
            }
            else
            {
                return Content("<html><head></head><body><div><h1 style=\"text-align: center;vertical-align: middle;height: 100px;line-height: 100px;color: green;\">" + msg + "</h1></div></body></html>");
            }
        }
        /// <summary>
        /// 返回错误信息，参数msg=""空时，表示正确
        /// </summary>
        /// <param name="msg">错误信息</param>
        /// <param name="asPage">返回页面</param>
        /// <returns>网页视图</returns>
        public ActionResult Error(string msg, bool asPage = false)
        {
            if (!asPage)
            {
                return Json(new { done = msg == "", msg = msg });
            }
            else
            {
                return Content("<html><head></head><body><div><h1 style=\"text-align: center;vertical-align: middle;height: 100px;line-height: 100px;color: red;\">" + msg + "</h1></div></body></html>");
            }
        }
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public Controller()
        {
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
                logFileName = "controller.log";
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
#if DEBUG
            string log = String.Format("--> OnActionExecuting ActionName:{0}.{1}",
                filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                filterContext.ActionDescriptor.ActionName
                );
            System.Diagnostics.Debug.WriteLine(log);
#endif
            //Trace.TraceInformation("\r\n -->OnActionExecuting.Thread.Name:\r\n{0}", System.Threading.Thread.CurrentThread.Name);
            base.OnActionExecuting(filterContext);
            _stopwatch.Restart();
        }
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            _stopwatch.Stop();
            string log = String.Format("--> OnActionExecuted ActionName:{1}.{2}  执行时间:{0} 毫秒",
                _stopwatch.ElapsedMilliseconds,
                filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                filterContext.ActionDescriptor.ActionName);
            ViewBag.Logging = log;
#if DEBUG
            System.Diagnostics.Debug.WriteLine(log);
#endif
            //Trace.TraceInformation("\r\n执行时间:{0} 毫秒 \r\n -->OnActionExecuted.Thread.Name:\r\n{1}", _stopwatch.ElapsedMilliseconds, System.Threading.Thread.CurrentThread.Name);
            base.OnActionExecuted(filterContext);
        }

        /// <summary>
        /// 获取是否通过用户验证
        /// </summary>
        /// <returns>true or false</returns>
        public static bool IsAuthenticated()
        {
            if (System.Web.HttpContext.Current.User == null) return false;
            if (System.Web.HttpContext.Current.User.Identity == null) return false;
            return System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
        }

        public class LoginInfo
        {
            public int id { get; set; }
            public string name { get; set; }
            public string pw { get; set; }
            public bool rem { get; set; }
            public bool expired { get; set; }
        }

        /// <summary>
        /// 获取登录的用户名
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentUserName()
        {
            if (System.Web.HttpContext.Current.User == null) return null;
            if (System.Web.HttpContext.Current.User.Identity == null) return null;
            if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return System.Web.HttpContext.Current.User.Identity.Name;
            }
            var cookieName = "ywl.web.login";
            var config_cookieName = ConfigurationManager.AppSettings["LoginCookieName"];
            if (config_cookieName != null)
            {
                cookieName = config_cookieName;
            }
            var cookie = System.Web.HttpContext.Current.Request.Cookies.Get(cookieName);
            if (cookie != null)
            {
                var value = System.Web.HttpContext.Current.Server.UrlDecode(cookie.Value);
                var loginfo = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginInfo>(value);
                if (loginfo != null) return loginfo.id.ToString();
            }
            return null;
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
        protected void WriteLog(string msg)
        {
            var filePath = this.logFilePath + DateTime.Now.ToString("yyyy/MM/");

            filePath = Server.MapPath(filePath);
            if (CreateFolderIfNeeded(filePath))
            {
                var fileName = filePath + "controller." + DateTime.Now.ToString("yyyy.MM.dd HH") + ".log";
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
        protected void WriteLog(string format, params object[] args)
        {
            var filePath = this.logFilePath + DateTime.Now.ToString("yyyy/MM/");

            filePath = Server.MapPath(filePath);
            if (CreateFolderIfNeeded(filePath))
            {
                var fileName = filePath + "controller." + DateTime.Now.ToString("yyyy.MM.dd HH") + ".log";
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
    }
}

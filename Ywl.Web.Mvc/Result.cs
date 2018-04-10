using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ywl.Web.Mvc
{
    /// <summary>
    /// 带有错误信息的通用函数返回值
    /// </summary>
    public class Result
    {
        private bool? _successful = null;
        public bool successful
        {
            get
            {
                if (this._successful == null)
                {
                    return this.error.Message == "";
                }
                else
                    return this._successful.Value;
            }
            set { this._successful = value; }
        }
        public object Value { get; set; }
        public class Error
        {
            public string Code { get; set; }
            public string Message { get; set; }
        }
        public Error error { get; set; }
        public Result()
        {
            this.error = new Error();
            this.error.Code = "0";
            this.error.Message = "";
        }
    }
}

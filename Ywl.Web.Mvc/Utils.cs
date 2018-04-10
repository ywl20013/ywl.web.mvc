using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Ywl.Web.Mvc
{
    public class Utils
    {
        #region MD5加密
        public static string MD5(string pwd)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.Default.GetBytes(pwd);
            byte[] md5data = md5.ComputeHash(data);
            md5.Clear();
            string str = "";
            for (int i = 0; i < md5data.Length; i++)
            {
                str += md5data[i].ToString("x").PadLeft(2, '0');

            }
            return str;
        }
        #endregion

        #region 类型转换
        /// <summary>
        /// 字符串转换为Int32类型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static int? StrToInt(string expression, int? defValue)
        {
            if (string.IsNullOrEmpty(expression) || expression.Trim().Length >= 11 || !Regex.IsMatch(expression.Trim(), @"^([-]|[0-9])[0-9]*(\.\w*)?$"))
                return defValue;

            int rv;
            if (Int32.TryParse(expression, out rv))
                return rv;

            return null;
        }

        /// <summary>
        /// 将对象转换为Int32类型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static int? ObjToInt(object expression, int? defValue)
        {
            if (expression != null)
                return StrToInt(expression.ToString(), defValue);

            return defValue;
        }
        /// <summary>
        /// 将字符串转换为布尔类型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的结果</returns>
        public static bool? StrToBool(string expression, bool? defValue)
        {
            if (expression != null)
            {
                if (string.Compare(expression, "true", true) == 0)
                    return true;
                else if (string.Compare(expression, "false", true) == 0)
                    return false;
            }
            return defValue;
        }
        /// <summary>
        /// 将对象转换为布尔类型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的结果</returns>
        public static bool? ObjToBool(object expression, bool? defValue)
        {
            if (expression != null)
                return StrToBool(expression.ToString(), defValue);

            return defValue;
        }
        /// <summary>
        /// Object型转换为decimal型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的结果</returns>
        public static decimal? ObjToDecimal(object expression, decimal? defValue)
        {
            if (expression != null)
                return StrToDecimal(expression.ToString(), defValue);

            return defValue;
        }

        /// <summary>
        /// string型转换为decimal型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的decimal类型结果</returns>
        public static decimal? StrToDecimal(string expression, decimal? defValue)
        {
            if ((expression == null) || (expression.Length > 30))
                return defValue;

            decimal intValue = 0;// defValue;
            if (expression != null)
            {
                bool IsDecimal = Regex.IsMatch(expression, @"^([-]|[0-9])[0-9]*(\.\w*)?$");
                if (IsDecimal)
                {
                    decimal.TryParse(expression, out intValue);
                    return intValue;
                }
            }
            return defValue;
        }
        /// <summary>
        /// Object型转换为decimal型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的结果</returns>
        public static double? ObjToDouble(object expression, double? defValue)
        {
            if (expression != null)
                return StrToDouble(expression.ToString(), defValue);

            return defValue;
        }

        /// <summary>
        /// string型转换为decimal型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的decimal类型结果</returns>
        public static double? StrToDouble(string expression, double? defValue)
        {
            if ((expression == null) || (expression.Length > 10))
                return defValue;

            double value = 0;// defValue;
            if (expression != null)
            {
                bool IsDecimal = Regex.IsMatch(expression, @"^([-]|[0-9])[0-9]*(\.\w*)?$");
                if (IsDecimal)
                {
                    double.TryParse(expression, out value);
                    return value;
                }
            }
            return defValue;
        }
        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="expression">要转换的字符串</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的结果</returns>
        public static DateTime? StrToDateTime(string expression, DateTime? defValue)
        {
            if (!string.IsNullOrEmpty(expression))
            {
                DateTime dateTime;
                if (DateTime.TryParse(expression, out dateTime))
                    return dateTime;
            }
            return defValue;
        }
        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="expression">要转换的字符串</param> 
        /// <returns>转换后的结果</returns>
        public static DateTime? StrToDateTime(string expression)
        {
            return StrToDateTime(expression, DateTime.Now);
        }

        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="expression">要转换的字符串</param> 
        /// <returns>转换后的结果</returns>
        public static DateTime? ObjectToDateTime(object expression)
        {
            if (expression == null) return null;
            return StrToDateTime(expression.ToString());
        }

        /// <summary>
        /// 将对象转换为日期时间类型
        /// </summary>
        /// <param name="expression">要转换的对象</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>转换后的int类型结果</returns>
        public static DateTime? ObjectToDateTime(object expression, DateTime? defValue)
        {
            if (expression == null) return defValue;
            return StrToDateTime(expression.ToString(), defValue);
        }
        #endregion

        #region 文件处理
        /// <summary>
        ///  检查是否要创建上传文件夹
        /// </summary>
        /// <param name="path">服务器物理路径</param>
        /// <returns>是否成功</returns>
        public static string CreateFolder(string path)
        {
            string result = "";
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
                    result = e.Message;
                }
            }
            return result;
        }

        /// <summary>
        ///  检查是否要创建上传文件夹
        /// </summary>
        /// <param name="path">服务器物理路径</param>
        /// <returns>是否成功</returns>
        public static bool CreateFolderIfNeeded(string path)
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
        /// 创建缩略图
        /// </summary>
        /// <param name="originalPicture">原图地址</param>
        /// <param name="thumbnail">缩略图地址</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <returns>是否成功</returns>
        public static bool CreateThumbnail(string originalPicture, string thumbnail, int width, int height, int? resolution)
        {
            //原图
            Image _original = Image.FromFile(originalPicture);
            // 原图使用区域
            RectangleF _originalArea = new RectangleF();
            //宽高比
            float _ratio = (float)width / height;
            if (_ratio > ((float)_original.Width / _original.Height))
            {
                _originalArea.X = 0;
                _originalArea.Width = _original.Width;
                _originalArea.Height = _originalArea.Width / _ratio;
                _originalArea.Y = (_original.Height - _originalArea.Height) / 2;
            }
            else
            {
                _originalArea.Y = 0;
                _originalArea.Height = _original.Height;
                _originalArea.Width = _originalArea.Height * _ratio;
                _originalArea.X = (_original.Width - _originalArea.Width) / 2;
            }
            Bitmap _bitmap = new Bitmap(width, height);
            if (resolution != null)
                _bitmap.SetResolution(resolution.Value, resolution.Value);
            Graphics _graphics = Graphics.FromImage(_bitmap);
            //设置图片质量
            _graphics.InterpolationMode = InterpolationMode.High;
            _graphics.SmoothingMode = SmoothingMode.HighQuality;
            //绘制图片
            _graphics.Clear(Color.Transparent);
            _graphics.DrawImage(_original, new RectangleF(0, 0, _bitmap.Width, _bitmap.Height), _originalArea, GraphicsUnit.Pixel);
            //保存
            _bitmap.Save(thumbnail);
            _graphics.Dispose();
            _original.Dispose();
            _bitmap.Dispose();
            return true;
        }

        /// <summary>
        /// 获得当前绝对路径
        /// </summary>
        /// <param name="strPath">指定的路径</param>
        /// <returns>绝对路径</returns>
        public static string GetMapPath(string strPath)
        {
            if (strPath.ToLower().StartsWith("http://"))
            {
                return strPath;
            }
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Server.MapPath(strPath);
            }
            else //非web程序引用
            {
                strPath = strPath.Replace("/", "\\");
                if (strPath.StartsWith("\\"))
                {
                    strPath = strPath.Substring(strPath.IndexOf('\\', 1)).TrimStart('\\');
                }
                return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strPath);
            }
        }
        /// <summary>
        /// 返回文件扩展名，不含“.”
        /// </summary>
        /// <param name="_filepath">文件全名称</param>
        /// <returns>string</returns>
        public static string GetFileExt(string _filepath)
        {
            if (string.IsNullOrEmpty(_filepath))
            {
                return "";
            }
            if (_filepath.LastIndexOf(".") > 0)
            {
                return _filepath.Substring(_filepath.LastIndexOf(".") + 1); //文件扩展名，不含“.”
            }
            return "";
        }

        /// <summary>
        /// 返回文件名，不含路径
        /// </summary>
        /// <param name="_filepath">文件相对路径</param>
        /// <returns>string</returns>
        public static string GetFileName(string _filepath)
        {
            return _filepath.Substring(_filepath.LastIndexOf(@"/") + 1);
        }

        /// <summary>
        /// 返回文件大小KB
        /// </summary>
        /// <param name="_filepath">文件相对路径</param>
        /// <returns>int</returns>
        public static int GetFileSize(string _filepath)
        {
            if (string.IsNullOrEmpty(_filepath))
            {
                return 0;
            }
            string fullpath = GetMapPath(_filepath);
            if (File.Exists(fullpath))
            {
                FileInfo fileInfo = new FileInfo(fullpath);
                return ((int)fileInfo.Length) / 1024;
            }
            return 0;
        }
        #endregion

        #region ===== 获取属性描述 =====
        /// <summary>
        /// 获取属性描述
        /// </summary>
        /// <param name="typ"></param>
        /// <param name="property_name"></param>
        /// <returns></returns>
        public static string ClassPropertyDescription(Type typ, string property_name)
        {
            var prop = typ.GetProperty(property_name);
            if (prop != null)
            {
                var attrs = Attribute.GetCustomAttributes(prop);
                foreach (var attr in attrs)
                {
                    if (attr is System.ComponentModel.DisplayNameAttribute)
                        return ((System.ComponentModel.DisplayNameAttribute)attr).DisplayName;
                    if (attr is System.ComponentModel.DescriptionAttribute)
                        return ((System.ComponentModel.DescriptionAttribute)attr).Description;
                    if (attr is System.ComponentModel.DataAnnotations.DisplayAttribute)
                    {
                        var a = (System.ComponentModel.DataAnnotations.DisplayAttribute)attr;
                        return a.Name != null ? a.Name : a.Description;
                    }
                }
            }
            return "";
        }
        public static string ClassPropertyDescription(Type typ, System.Reflection.PropertyInfo property)
        {
            if (property != null)
            {
                var attrs = Attribute.GetCustomAttributes(property);
                foreach (var attr in attrs)
                {
                    if (attr is System.ComponentModel.DisplayNameAttribute)
                        return ((System.ComponentModel.DisplayNameAttribute)attr).DisplayName;
                    if (attr is System.ComponentModel.DescriptionAttribute)
                        return ((System.ComponentModel.DescriptionAttribute)attr).Description;
                    if (attr is System.ComponentModel.DataAnnotations.DisplayAttribute a)
                    {
                        return a.Name ?? a.Description;
                    }
                }
            }
            return "";
        }
        #endregion

        #region ===== 生成随机码 =====

        /// <summary>
        /// 生成日期随机码
        /// </summary>
        /// <returns></returns>
        public static string GetRamCode()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssffff");
        }

        /// <summary>
        /// 生成随机数字
        /// </summary>
        /// <param name="length">生成长度</param>
        /// <returns></returns>
        public static string Number(int Length)
        {
            return Number(Length, false);
        }

        /// <summary>
        /// 生成随机数字
        /// </summary>
        /// <param name="Length">生成长度</param>
        /// <param name="Sleep">是否要在生成前将当前线程阻止以避免重复</param>
        /// <returns></returns>
        public static string Number(int Length, bool Sleep)
        {
            if (Sleep)
                System.Threading.Thread.Sleep(3);
            string result = "";
            System.Random random = new Random();
            for (int i = 0; i < Length; i++)
            {
                result += random.Next(10).ToString();
            }
            return result;
        }
        /// <summary>
        /// 生成随机字母字符串(数字字母混和)
        /// </summary>
        /// <param name="codeCount">待生成的位数</param>
        public static string GetCheckCode(int codeCount)
        {
            string str = string.Empty;
            int rep = 0;
            long num2 = DateTime.Now.Ticks + rep;
            rep++;
            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> rep)));
            for (int i = 0; i < codeCount; i++)
            {
                char ch;
                int num = random.Next();
                if ((num % 2) == 0)
                {
                    ch = (char)(0x30 + ((ushort)(num % 10)));
                }
                else
                {
                    ch = (char)(0x41 + ((ushort)(num % 0x1a)));
                }
                str = str + ch.ToString();
            }
            return str;
        }

        #endregion
    }
}
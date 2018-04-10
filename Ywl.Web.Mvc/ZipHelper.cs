using System;
using System.Collections.Generic;
using System.Linq;

namespace Ywl.Web.Mvc
{
    public class ZipHelper
    {
        /// <summary>  
        ///  压缩多个文件  
        /// </summary>  
        /// <param name="files">文件名</param>  
        /// <param name="ZipedFileName">压缩包文件名</param>  
        /// <returns></returns>  
        public static void Zip(string[] files, string ZipedFileName)
        {
            Zip(files, ZipedFileName, string.Empty);
        }

        /// <summary>  
        ///  压缩多个文件  
        /// </summary>  
        /// <param name="files">文件名</param>  
        /// <param name="ZipedFileName">压缩包文件名</param>  
        /// <param name="Password">解压码</param>  
        /// <returns></returns>  
        public static void Zip(string[] files, string ZipedFileName, string Password)
        {
            files = files.Where(f => System.IO.File.Exists(f) || System.IO.Directory.Exists(f)).ToArray();
            if (files.Length == 0) throw new System.IO.FileNotFoundException("未找到指定打包的文件");
            ICSharpCode.SharpZipLib.Zip.ZipOutputStream s = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(System.IO.File.Create(ZipedFileName));
            s.SetLevel(6);
            if (!string.IsNullOrEmpty(Password.Trim())) s.Password = Password.Trim();
            Zip(files, s);
            s.Finish();
            s.Close();
        }

        private static void Zip(string[] files, ICSharpCode.SharpZipLib.Zip.ZipOutputStream s)
        {
            List<string> rootPaths = new List<string>();
            ICSharpCode.SharpZipLib.Zip.ZipEntry entry = null;
            System.IO.FileStream fs = null;
            ICSharpCode.SharpZipLib.Checksums.Crc32 crc = new ICSharpCode.SharpZipLib.Checksums.Crc32();
            try
            {
                ////创建当前文件夹  
                //entry = new ICSharpCode.SharpZipLib.Zip.ZipEntry("/");  //加上 “/” 才会当成是文件夹创建  
                //s.PutNextEntry(entry);
                //s.Flush();
                foreach (string file in files)
                {
                    if (System.IO.Directory.Exists(file))
                    {
                        if (file.Split('\\').Count() > 1)
                        {
                            rootPaths.Add(file);
                            entry = new ICSharpCode.SharpZipLib.Zip.ZipEntry(file.Split('\\').Last() + "/");  //加上 “/” 才会当成是文件夹创建  
                            s.PutNextEntry(entry);
                            s.Flush();
                        }
                        continue;
                    }

                    //打开压缩文件  
                    fs = System.IO.File.OpenRead(file);

                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    var zipfilename = System.IO.Path.GetFileName(file);
                    foreach (var rootPath in rootPaths)
                    {
                        var _index = file.IndexOf(rootPath);
                        if (_index >= 0)
                        {
                            zipfilename = rootPath.Split('\\').Last() + "\\" + System.IO.Path.GetFileName(file);
                            break;
                        }
                    }
                    entry = new ICSharpCode.SharpZipLib.Zip.ZipEntry(zipfilename)
                    {
                        DateTime = DateTime.Now,
                        Size = fs.Length
                    };
                    fs.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    s.PutNextEntry(entry);
                    s.Write(buffer, 0, buffer.Length);
                }
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                if (entry != null)
                    entry = null;
                GC.Collect();
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ywl.Web.Mvc.Handlers
{
    public class JsxLoader : IHttpHandler
    {
        private class Module
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public string FileName { get; set; }
            public List<Module> Imports;
            public Module()
            {
                Imports = new List<Module>();
            }
            public Module LoadFromFile(Modules modules, string file)
            {
                this.FileName = file;
                this.Name = System.IO.Path.GetFileNameWithoutExtension(this.FileName);
                this.Code = @"""use strict"";" + "\r\n";
                using (System.IO.StreamReader reader = new System.IO.StreamReader(file))
                {
                    var path = System.IO.Path.GetDirectoryName(file);
                    var msg = reader.ReadLine();
                    while (true)
                    {
                        var varName = "";
                        var subFile = "";
                        var import = false;
                        var export = false;
                        var line = msg.Trim();
                        if (line.Length >= 6 && line.Substring(0, 6).ToLower() == "import")
                        {
                            import = true;
                            //import utils from '../utils';
                            var fromPosition = line.ToLower().IndexOf(" from ");
                            if (fromPosition > 0)
                            {
                                varName = line.Substring(7, fromPosition - 7);
                                subFile = line.Substring(fromPosition + 7, line.Length - fromPosition - 7);
                            }
                            else
                            {
                                subFile = line.Substring(7, line.Length - 7);
                            }

                            if (subFile != "")
                            {
                                subFile = subFile.Replace("'", "").Replace(";", "");
                                subFile = System.IO.Path.Combine(path, subFile);
                                subFile = System.IO.Path.GetFullPath(subFile);
                                if (subFile.Substring(subFile.Length - 1, 1) == "\\") subFile += "index";
                                var exists = System.IO.File.Exists(subFile + ".js");
                                if (exists) subFile += ".js";
                                else
                                {
                                    exists = System.IO.File.Exists(subFile + ".jsx");
                                    if (exists) subFile += ".jsx";
                                }
                                if (exists)
                                {
                                    Module sub = modules.FindByFile(subFile);
                                    if (sub == null)
                                    {
                                        sub = modules.CreateModule(subFile);
                                    }
                                    this.Imports.Add(sub);
                                }
                            }
                        }
                        else
                        {
                            this.Code += msg + "\r\n";
                        }


                        if (reader.EndOfStream) break;
                        msg = reader.ReadLine();
                    }
                }

                return this;
            }
        }
        private class Modules : List<Module>
        {
            public Module CreateModule(string file)
            {
                Module m = new Module
                {
                    Id = this.Count()
                };
                this.Add(m);
                m.LoadFromFile(this, file);
                return m;
            }
            public static Modules LoadFromFile(string file)
            {
                Modules ms = new Modules();
                Module m = ms.CreateModule(file);
                return ms;
            }
            public Module FindByFile(string file)
            {
                return this.Find(delegate (Module m) { return m.FileName == file; });
            }
        }
        public bool IsReusable => false;
        private string LoadJsxFile(string file)
        {
            var result = "";
            var isDebugEnable = true;
            System.Web.Configuration.CompilationSection ds = (System.Web.Configuration.CompilationSection)ConfigurationManager.GetSection("system.web/compilation");
            isDebugEnable = ds.Debug;
            using (System.IO.StreamReader reader = new System.IO.StreamReader(file))
            {
                var path = System.IO.Path.GetDirectoryName(file);
                var msg = reader.ReadLine();
                while (true)
                {
                    var varName = "";
                    var subFile = "";
                    var import = false;
                    var export = false;
                    var line = msg.Trim();
                    if (line.Length >= 6 && line.Substring(0, 6).ToLower() == "import")
                    {
                        import = true;
                        //import utils from '../utils';
                        var fromPosition = line.ToLower().IndexOf(" from ");
                        if (fromPosition > 0)
                        {
                            varName = line.Substring(7, fromPosition - 7);
                            subFile = line.Substring(fromPosition + 7, line.Length - fromPosition - 7);
                        }
                        else
                        {
                            subFile = line.Substring(7, line.Length - 7);
                        }
                    }
                    else if (line.Length >= 15 && line.Substring(0, 15).ToLower() == "export default ")
                    {
                        //export default
                        export = true;
                        result += line.Substring(15, line.Length - 15) + "\r\n";
                    }
                    else if (line.Length >= 8 && line.Substring(0, 8).ToLower() == "console.")
                    {
                        if (isDebugEnable)
                        {
                            result += msg + "\r\n";
                        }
                    }
                    else
                    {
                        result += msg + "\r\n";
                    }
                    if (subFile != "")
                    {
                        subFile = subFile.Replace("'", "").Replace(";", "");
                        subFile = System.IO.Path.Combine(path, subFile);
                        subFile = System.IO.Path.GetFullPath(subFile);
                        if (subFile.Substring(subFile.Length - 1, 1) == "\\") subFile += "index";
                        var exists = System.IO.File.Exists(subFile + ".js");
                        if (exists) subFile += ".js";
                        else
                        {
                            exists = System.IO.File.Exists(subFile + ".jsx");
                            if (exists) subFile += ".jsx";
                        }
                        if (exists)
                        {
                            if (varName == "")
                            {
                                var filecontent = LoadJsxFile(subFile);
                                if (filecontent.Length > 1 && filecontent.Trim().Substring(0, 1) == "{")
                                {
                                    result += "extend(window," + filecontent.Trim() + ");\r\n";
                                }
                                else
                                {
                                    result += filecontent;
                                }
                            }
                            else
                            {
                                result += "var " + varName + " = " + LoadJsxFile(subFile);
                            }
                        }
                        //result += "{ " +
                        //               "name:'" + varName + "'," +
                        //               "subFile:'" + subFile + "'," +
                        //               "exists:'" + exists.ToString() + "'," +
                        //               "export:'" + export.ToString() + "'," +
                        //           "}";
                    }
                    if (reader.EndOfStream) break;
                    msg = reader.ReadLine();
                }
            }
            return result;
        }
        public void ProcessRequest(HttpContext context)
        {
            var result = "";

            var absolutePath = context.Server.MapPath(context.Request.Url.AbsolutePath);

            var define_begin =
@"/******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};
/******/
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/
/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId]) {
/******/ 			return installedModules[moduleId].exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			i: moduleId,
/******/ 			l: false,
/******/ 			exports: {}
/******/ 		};
/******/
/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/
/******/ 		// Flag the module as loaded
/******/ 		module.l = true;
/******/
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/
/******/
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;
/******/
/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;
/******/
/******/ 	// define getter function for harmony exports
/******/ 	__webpack_require__.d = function(exports, name, getter) {
/******/ 		if(!__webpack_require__.o(exports, name)) {
/******/ 			Object.defineProperty(exports, name, {
/******/ 				configurable: false,
/******/ 				enumerable: true,
/******/ 				get: getter
/******/ 			});
/******/ 		}
/******/ 	};
/******/
/******/ 	// getDefaultExport function for compatibility with non-harmony modules
/******/ 	__webpack_require__.n = function(module) {
/******/ 		var getter = module && module.__esModule ?
/******/ 			function getDefault() { return module['default']; } :
/******/ 			function getModuleExports() { return module; };
/******/ 		__webpack_require__.d(getter, 'a', getter);
/******/ 		return getter;
/******/ 	};
/******/
/******/ 	// Object.prototype.hasOwnProperty.call
/******/ 	__webpack_require__.o = function(object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
/******/
/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = """";
/******/
/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(__webpack_require__.s = 0);
/******/ })
/************************************************************************/
/******/ ([
";
            var define_end =
@"/******/ ]);";

            Modules ms = Modules.LoadFromFile(absolutePath);
            result = define_begin;
            foreach (var m in ms)
            {
                string code =
                    "/* " + m.Id + " */" + "\r\n" +
                    "/***/ (function(module, __webpack_exports__, __webpack_require__) {" + "\r\n" +
                    "" + "\r\n";

                int im_index = 0;
                foreach (var im in m.Imports)
                {
                    code += "/* harmony import */ var __WEBPACK_IMPORTED_MODULE_" + im_index + "__" + im.Name + "__ = __webpack_require__(" + im.Id + ");" + "\r\n";
                    im_index++;
                }

                code += m.Code;

                code += "" + "\r\n" +
                     "" + "\r\n" +
                     "/***/" + "\r\n" +
                     "      }),";
                result += code;// code;
            }
            result += "\r\n" + define_end;
            // result = define_begin + LoadJsxFile(absolutePath) + "\r\n" + define_end;
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.Now.AddMinutes(5));
            context.Response.Cache.SetValidUntilExpires(false);
            context.Response.ContentType = "application/javascript";
            context.Response.Write(result);
        }
    }
}

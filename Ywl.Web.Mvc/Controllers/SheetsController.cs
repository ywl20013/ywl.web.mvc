using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;

namespace Ywl.Web.Mvc.Controllers
{
    /// <summary>
    /// 普通单据基础控制器
    /// </summary>
    /// <typeparam name="TContext">数据库上下文类，用于调用对应单据的数据集</typeparam>
    /// <typeparam name="TEntity">单据模型类</typeparam>
    public class SheetsController<TContext, TEntity> : DbController<TContext>
        where TContext : Data.Entity.DbContext, new()
        where TEntity : Models.Entity, new()
    {
        #region ========== 基础变量 ==========

        protected Type EntityClassType = typeof(TEntity);
        protected virtual DbSet<TEntity> _Entities { get; set; }
        protected virtual IQueryable<TEntity> Queryable { get; set; }

        /// <summary>
        /// 单据数据集
        /// </summary>
        protected virtual DbSet<TEntity> Entities
        {
            get
            {
                if (_Entities == null)
                {
                    foreach (var pro in typeof(TContext).GetProperties())
                    {
                        if (pro.PropertyType.FullName == typeof(DbSet<TEntity>).FullName)
                        {
                            return (DbSet<TEntity>)pro.GetValue(this.db);
                        }
                    }
                }
                return _Entities;
            }
            set { this._Entities = value; }
        }

        #endregion ==========基础变量==========

        #region ========== 工具 ==========


        protected Result GetTEntityTitleAttribute()
        {
            var result = new Result();
            var entity_type = typeof(TEntity);
            var attribute = entity_type.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
            if (attribute == null)
            {
                result.error.Message = "模型[" + entity_type.FullName + "]缺少DescriptionAttribute！无法获取DescriptionAttribute.Title用于用户待办任务标题。";
            }
            else
            {
                result.Value = ((DescriptionAttribute)attribute).Title;
            }
            return result;
        }

        #region ========== 从网页POST请求中生成实例 ==========
        /// <summary>
        /// 从网页POST请求中生成实例
        /// </summary>
        /// <returns></returns>
        protected TEntity InternalCreateEntityFromRequest()
        {
            var entity = new TEntity();
            InternalCreateEntityFromRequest(entity);
            return entity;
        }
        protected TEntity InternalCreateEntityFromRequest(TEntity entity)
        {
            foreach (var prop in entity.GetType().GetProperties())
            {
                Type PropertyType = prop.PropertyType;
                var PropertyTypeFullName = PropertyType.FullName;
                var req_value = Request[prop.Name];
                if (req_value != null)
                {
                    //System.TypeCode typeCode = Type.GetTypeCode(PropertyType);
                    //switch (typeCode)
                    //{
                    //    case TypeCode.Boolean:
                    //        //doStuff
                    //        break;
                    //    case TypeCode.String:
                    //    case TypeCode.Int32:
                    //        prop.SetValue(entity, req_value);
                    //        break;
                    //    case TypeCode.DateTime:
                    //        prop.SetValue(entity, Utils.ObjectToDateTime(req_value));
                    //        break;
                    //    default: break;
                    //}
                    if (PropertyTypeFullName == typeof(System.DateTime).FullName)
                    {
                        prop.SetValue(entity, Utils.StrToDateTime(req_value));
                    }
                    else if (PropertyTypeFullName == typeof(Nullable<System.DateTime>).FullName)
                    {
                        prop.SetValue(entity, Utils.StrToDateTime(req_value, null));
                    }
                    else if (PropertyTypeFullName == typeof(System.Int32).FullName)
                    {
                        prop.SetValue(entity, Utils.StrToInt(req_value, 0));
                    }
                    else if (PropertyTypeFullName == typeof(Nullable<System.Int32>).FullName)
                    {
                        prop.SetValue(entity, Utils.StrToInt(req_value, null));
                    }
                    else if (PropertyTypeFullName == typeof(System.Decimal).FullName)
                    {
                        prop.SetValue(entity, Utils.StrToDecimal(req_value, 0));
                    }
                    else if (PropertyTypeFullName == typeof(Nullable<System.Decimal>).FullName)
                    {
                        prop.SetValue(entity, Utils.StrToDecimal(req_value, null));
                    }
                    else if (PropertyTypeFullName == typeof(System.Double).FullName)
                    {
                        prop.SetValue(entity, Utils.StrToDouble(req_value, 0));
                    }
                    else if (PropertyTypeFullName == typeof(Nullable<System.Double>).FullName)
                    {
                        prop.SetValue(entity, Utils.StrToDouble(req_value, null));
                    }
                    else if (PropertyTypeFullName == typeof(System.Boolean).FullName)
                    {
                        prop.SetValue(entity, Utils.StrToBool(req_value, false));
                    }
                    else if (PropertyTypeFullName == typeof(Nullable<System.Boolean>).FullName)
                    {
                        prop.SetValue(entity, Utils.StrToBool(req_value, null));
                    }
                    else
                        prop.SetValue(entity, req_value);

                    //   ret += prop.Name + ":" + prop.PropertyType.Name + " PropertyType.FullName:" + prop.PropertyType.FullName + "<br />";
                }
            }
            return entity;
        }
        #endregion

        #region ========== 获取单个实例 ==========
        /// <summary>
        /// 获取单个实例
        /// </summary>
        /// <returns></returns>
        [Description("获取单个实例", Description = "获取单个实例")]
        public async Task<ActionResult> GetSingle(int? id)
        {
            return Json(await InternalGetSingleAsync(id));
        }

        /// <summary>
        /// 获取单个实例
        /// </summary>
        /// <returns></returns>
        [Description("获取单个实例", Description = "获取单个实例")]
        protected virtual async Task<TEntity> InternalGetSingleAsync(int? id)
        {
            if (id == null)
                return null;

            var entity = await Entities.FindAsync(id);
            return entity;
        }
        #endregion ==========获取单个实例==========

        #endregion ========== 工具 ==========

        #region ========== 首页 ==========
        /// <summary>
        /// 首页重载
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual Task<string> InternalIndexAsync(int? id)
        {
            return Task.FromResult<string>("");
        }
        //[Description("首页", Description = "首页（列表）")]
        //public virtual ActionResult Index(int? id)
        //{
        //    ViewBag.CurrentUserId = CurrentAuthenticatedUserId();
        //    return View();
        //}
        #endregion ========== 首页 ==========

        #region ========== 创建 ==========
        protected virtual async Task<string> InternalBeforeCreateAsync(int? id, TEntity entity)
        {
            return await Task<string>.FromResult("");
        }
        protected virtual async Task<string> InternalAfterCreateSaveAsync(TEntity entity)
        {
            return await Task<string>.FromResult("");
        }
        protected virtual async Task<string> InternalBeforeCreateSaveAsync(TEntity entity)
        {
            return await Task<string>.FromResult("");
        }

        #endregion ==========创建==========

        #region ========== 编辑 ==========

        protected virtual async Task<string> InternalBeforeEditAsync(TEntity entity)
        {
            return await Task<string>.FromResult("");
        }
        protected virtual async Task<string> InternalBeforeEditSaveAsync(TEntity entity)
        {
            return await Task<string>.FromResult("");
        }
        protected virtual async Task<string> InternalAfterEditSaveAsync(TEntity entity)
        {
            return await Task<string>.FromResult("");
        }

        #endregion ==========编辑==========

        #region ========== 删除 ==========
        protected virtual async Task<string> InternalBeforeDeleteAsync(List<int> ids, bool ishide, IQueryable<TEntity> entities)
        {
            return await Task<string>.FromResult("");
        }
        protected virtual async Task<string> InternalBeforeDeleteSaveAsync(List<int> ids, bool ishide, IQueryable<TEntity> entities)
        {
            return await Task<string>.FromResult("");
        }
        #endregion ==========删除==========

        #region ========== 列表 ==========
        public virtual IQueryable<TEntity> InternalBeforeListSearch()
        {
            return from t in Entities
                   where t.Status == Ywl.Web.Mvc.Models.Status.Normal
                   select t;
        }
        public virtual async Task<List<TEntity>> InternalBeforeListReturnDataAsync(List<TEntity> list)
        {
            return list;
        }

        public class OrderColumn
        {
            public enum Direction { Asc, Desc }
            public string Name { get; set; }
            public Direction Dir { get; set; }
            public void SetDir(string value)
            {
                Dir = value.ToLower() == "asc" ? Direction.Asc : Direction.Desc;
            }
        }

        /// <summary>
        /// List排序前
        /// </summary>
        /// <param name="id"></param>
        /// <param name="req"></param>
        /// <param name="Items"></param>
        /// <returns></returns>
        protected virtual string InternalBeforeListOrder(List<OrderColumn> OrderColumns, IQueryable<TEntity> Items)
        {
            return "";
        }
        /// <summary>
        /// 排序后
        /// </summary>
        /// <param name="id"></param>
        /// <param name="req"></param>
        /// <param name="Items"></param>
        /// <returns></returns>
        protected virtual string InternalAfterListOrder(List<OrderColumn> OrderColumns, IQueryable<TEntity> Items)
        {
            return "";
        }

        /// <summary>
        /// 为Action List 准备排序,支持DateTime,String,Int,Double
        /// </summary>
        /// <param name="id"></param>
        /// <param name="req"></param>
        /// <param name="Items"></param>
        /// <returns></returns>
        protected virtual IQueryable<TEntity> PrepareListOrder(List<OrderColumn> OrderColumns, IQueryable<TEntity> Items)
        {
            var ret = InternalBeforeListOrder(OrderColumns, Items);
            if (this.Queryable != null)
                Items = this.Queryable;
            if (ret != "DONE")
            {
                #region ===== 排序 =====
                IOrderedQueryable<TEntity> oq = null;
                //排序
                for (int i = 0; i < OrderColumns.Count; i++)
                {
                    OrderColumn od = OrderColumns[i];

                    foreach (var prop in (typeof(TEntity)).GetProperties())
                    {
                        Type PropertyType = prop.PropertyType;
                        var PropertyTypeFullName = PropertyType.FullName;

                        if (od.Name == prop.Name)
                        {
                            var param = Expression.Parameter(typeof(TEntity), prop.Name);
                            var body = Expression.Property(param, prop.Name);

                            if (PropertyTypeFullName == typeof(System.DateTime).FullName)
                            {
                                var keySelector = Expression.Lambda<Func<TEntity, System.DateTime>>(body, param);

                                if (od.Dir == OrderColumn.Direction.Asc)
                                    oq = oq == null ? Items.OrderBy(keySelector) : oq.ThenBy(keySelector);
                                else
                                    oq = oq == null ? Items.OrderByDescending(keySelector) : oq.ThenByDescending(keySelector);
                            }
                            else if (PropertyTypeFullName == typeof(Nullable<System.DateTime>).FullName)
                            {
                                var keySelector = Expression.Lambda<Func<TEntity, Nullable<System.DateTime>>>(body, param);

                                if (od.Dir == OrderColumn.Direction.Asc)
                                    oq = oq == null ? Items.OrderBy(keySelector) : oq.ThenBy(keySelector);
                                else
                                    oq = oq == null ? Items.OrderByDescending(keySelector) : oq.ThenByDescending(keySelector);
                            }
                            else if (PropertyTypeFullName == typeof(System.String).FullName)
                            {
                                var keySelector = Expression.Lambda<Func<TEntity, System.String>>(body, param);

                                if (od.Dir == OrderColumn.Direction.Asc)
                                    oq = oq == null ? Items.OrderBy(keySelector) : oq.ThenBy(keySelector);
                                else
                                    oq = oq == null ? Items.OrderByDescending(keySelector) : oq.ThenByDescending(keySelector);
                            }
                            else if (PropertyTypeFullName == typeof(System.Int32).FullName)
                            {
                                var keySelector = Expression.Lambda<Func<TEntity, System.Int32>>(body, param);

                                if (od.Dir == OrderColumn.Direction.Asc)
                                    oq = oq == null ? Items.OrderBy(keySelector) : oq.ThenBy(keySelector);
                                else
                                    oq = oq == null ? Items.OrderByDescending(keySelector) : oq.ThenByDescending(keySelector);
                            }
                            else if (PropertyTypeFullName == typeof(System.Double).FullName)
                            {
                                var keySelector = Expression.Lambda<Func<TEntity, System.Double>>(body, param);

                                if (od.Dir == OrderColumn.Direction.Asc)
                                    oq = oq == null ? Items.OrderBy(keySelector) : oq.ThenBy(keySelector);
                                else
                                    oq = oq == null ? Items.OrderByDescending(keySelector) : oq.ThenByDescending(keySelector);
                            }

                            break;
                        }
                    }
                }

                if (oq != null) Items = oq; else Items = Items.OrderByDescending(e => new { e.Id });

                #endregion ===== 排序 =====
            }
            ret = InternalAfterListOrder(OrderColumns, Items);
            return Items;
        }

        public virtual async Task<ActionResult> List(int? pagesize, int? start, string order)
        {
            if (Entities == null) throw new ArgumentNullException("Entities", "没有未模型配置数据集");

            var q = InternalBeforeListSearch();

            var total = await q.CountAsync();

            List<OrderColumn> orderColumns = new List<OrderColumn>();
            if (!String.IsNullOrEmpty(order) && !String.IsNullOrWhiteSpace(order))
            {
                var orders = order.Split(',');
                foreach (var _order in orders)
                {
                    var column = new OrderColumn();
                    var exp = _order.Split('_');

                    column.Name = exp[0];
                    if (exp.Count() > 1)
                        column.SetDir(exp[1]);

                    orderColumns.Add(column);
                }
            }

            q = this.PrepareListOrder(orderColumns, q);

            pagesize = pagesize == null ? 15 : pagesize;
            start = start == null ? 0 : start;

            List<TEntity> list = null;
            if (pagesize == -1)
            {
                list = await Entities.ToListAsync();
            }
            else
            {
                list = await Entities.OrderByDescending(e => e.Id).Skip(start.Value).Take(pagesize.Value).ToListAsync();
            }
            list = await this.InternalBeforeListReturnDataAsync(list);
            return Json(new { data = list, count = total });
        }
        #endregion ========== 列表 ==========

        #region ========== 导出 ==========

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public virtual async Task Export()
        {
            Exporter exporter = new Exporter(Server, Request, Response);

            await InternalBeforeExportAsync(exporter, this.Entities.AsQueryable());
            exporter.DoExport();
        }

        protected virtual Dictionary<string, string> InternalGetExportColumns(Exporter exporter)
        {
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exporter"></param>
        /// <param name="req"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        protected virtual async Task<string> InternalBeforeExportAsync(Exporter exporter, IQueryable<TEntity> entities)
        {
            var columns = this.InternalGetExportColumns(exporter);
            if (columns == null)
            {
                //导出当前模型的所有属性
                columns = new Dictionary<string, string>();
                var props = EntityClassType.GetProperties();
                for (int i = 0; i < props.Count(); i++)
                {
                    var prop = props[i];
                    columns.Add(prop.Name, Utils.ClassPropertyDescription(EntityClassType, prop));
                    exporter.ExcleExporter.SetCellValue(0, i, Utils.ClassPropertyDescription(EntityClassType, prop));
                }
            }
            else
            {
                for (int i = 0; i < columns.Count(); i++)
                {
                    var col = columns.ElementAt(i);
                    exporter.ExcleExporter.SetCellValue(0, i, col.Value);
                }
            }
            var list = exporter.ExcleExporter.TargetData;
            if (list != null)
            {
                for (int i = 0; i < list.Count(); i++)
                {
                    var item = list[i];
                    for (int j = 0; j < columns.Count(); j++)
                    {
                        var col = columns.ElementAt(j);

                        var prop = EntityClassType.GetProperty(col.Key);
                        if (prop != null)
                        {
                            object val = prop.GetValue(item);
                            exporter.ExcleExporter.SetCellValue(i + 1, j, val);
                        }
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// 导出器
        /// </summary>
        public class Exporter : IDisposable
        {
            /// <summary>
            /// 导出文件名
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// 导出文件名后缀
            /// </summary>
            public string FileExt { get; set; }

            /// <summary>
            /// 导出文件名
            /// </summary>
            public List<string> Files { get; set; }

            private HttpServerUtilityBase Server;

            /// <summary>
            /// Controller.Request
            /// </summary>
            private HttpRequestBase Request;

            /// <summary>
            /// Controller.Response
            /// </summary>
            private HttpResponseBase Response;

            public ExcleExporter ExcleExporter { get; private set; }

            public Exporter(HttpServerUtilityBase serverUtilityBase, HttpRequestBase requestBase, HttpResponseBase responseBase)
            {
                this.Server = serverUtilityBase;
                this.Request = requestBase;
                this.Response = responseBase;
                FileExt = ".xls";
                this.ExcleExporter = new ExcleExporter();
                this.ExcleExporter.Init(Response);

                this.Files = new List<string>();
            }

            /// <summary>
            /// 导出
            /// </summary>
            public void DoExport()
            {
                if (this.FileName == null)
                    this.FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}.{1}", FileName, FileExt));

                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ExcleExporter.Book.Write(ms);
                if (FileExt.Substring(0, 1) == ".") FileExt = FileExt.Substring(1);
                if (FileExt.ToLower() == "zip")
                {
                    var path = Server.MapPath("~/Exports/");
                    Utils.CreateFolderIfNeeded(path);

                    if (System.IO.File.Exists(path + FileName + ".xls")) System.IO.File.Delete(path + FileName + ".xls");

                    using (var fileStream = new System.IO.FileStream(path + FileName + ".xls",
                        System.IO.FileMode.CreateNew, System.IO.FileAccess.ReadWrite))
                    {
                        ms.Position = 0;
                        ms.CopyTo(fileStream); // fileStream is not populated
                    }

                    this.Files.Add(path + FileName + ".xls");

                    if (System.IO.File.Exists(path + FileName + ".zip")) System.IO.File.Delete(path + FileName + ".zip");
                    ZipHelper.Zip(this.Files.ToArray(), path + FileName + ".zip");

                    Response.WriteFile(path + FileName + ".zip");
                }
                else
                {
                    Response.BinaryWrite(ms.ToArray());
                }
                ExcleExporter.Book = null;
                ms.Close();
                ms.Dispose();
            }

            public void Dispose()
            {
            }
        }

        /// <summary>
        /// Excle导出器
        /// </summary>
        public class ExcleExporter
        {
            /// <summary>
            /// Book
            /// </summary>
            public NPOI.HSSF.UserModel.HSSFWorkbook Book
            {
                get;
                set;
            }

            /// <summary>
            /// 数据格式
            /// </summary>
            public NPOI.SS.UserModel.IDataFormat DataFormat { get; private set; }

            /// <summary>
            /// 日期格式单元格格式
            /// </summary>
            public NPOI.SS.UserModel.ICellStyle DateCellStyle { get; private set; }

            /// <summary>
            /// 日期时间格式单元格格式
            /// </summary>
            public NPOI.SS.UserModel.ICellStyle DateTimeCellStyle { get; private set; }

            /// <summary>
            /// 时间格式单元格格式
            /// </summary>
            public NPOI.SS.UserModel.ICellStyle TimeCellStyle { get; private set; }

            /// <summary>
            /// 默认Sheet
            /// </summary>
            public NPOI.SS.UserModel.ISheet DefaultSheet
            {
                get;
                private set;
            }

            /// <summary>
            /// Controller.Response
            /// </summary>
            private HttpResponseBase Response;

            /// <summary>
            /// 导出文件名
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// 导出文件名后缀
            /// </summary>
            [DefaultValue(".xls")]
            public string FileExt { get; set; }

            /// <summary>
            /// 最终要生成Excle的数据
            /// </summary>
            public List<TEntity> TargetData { get; set; }

            /// <summary>
            /// Init
            /// </summary>
            /// <param name="Response"></param>
            public void Init(HttpResponseBase Response)
            {
                this.Response = Response;
                if (Book == null)
                {
                    Book = new NPOI.HSSF.UserModel.HSSFWorkbook();

                    this.DataFormat = Book.CreateDataFormat();
                    this.DateCellStyle = Book.CreateCellStyle();
                    this.DateTimeCellStyle = Book.CreateCellStyle();
                    this.TimeCellStyle = Book.CreateCellStyle();
                    this.DateCellStyle.DataFormat = DataFormat.GetFormat("yyyy/MM/dd");
                    this.DateTimeCellStyle.DataFormat = DataFormat.GetFormat("yyyy/MM/dd HH:mm:ss");
                    this.TimeCellStyle.DataFormat = DataFormat.GetFormat("HH:mm:ss");

                    DefaultSheet = Book.CreateSheet("Sheet1");
                }
            }

            /// <summary>
            /// LoadFromTamplateFile
            /// </summary>
            /// <param name="FileName"></param>
            /// <returns></returns>
            public Boolean LoadFromTamplateFile(string FileName)
            {
                using (System.IO.FileStream fs = System.IO.File.OpenRead(FileName))   //打开myxls.xls文件
                {
                    Book = new NPOI.HSSF.UserModel.HSSFWorkbook(fs);

                    if (Book.NumberOfSheets > 0)
                        DefaultSheet = Book.GetSheetAt(0);
                    else
                        DefaultSheet = Book.CreateSheet("Sheet1");
                }
                return true;
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            public ExcleExporter()
            {
            }

            /// <summary>
            /// 导出
            /// </summary>
            public void DoExport()
            {
                if (this.FileName == null)
                    this.FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                Book.Write(ms);
                Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}.{1}", FileName, FileExt));
                Response.BinaryWrite(ms.ToArray());
                Book = null;
                ms.Close();
                ms.Dispose();
            }

            /// <summary>
            /// 保存成文件
            /// </summary>
            /// <param name="Path">物理路径</param>
            /// <returns>文件物理路径</returns>
            public string SaveToFile(string Path)
            {
                if (this.FileName == null)
                    this.FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                Book.Write(ms);

                var buf = ms.ToArray();

                //保存为Excel文件  
                using (System.IO.FileStream fs = new System.IO.FileStream(Path + "\\" + this.FileName + this.FileExt, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    fs.Write(buf, 0, buf.Length);
                    fs.Flush();
                }
                Book = null;
                ms.Close();
                ms.Dispose();

                return Path + "\\" + this.FileName;
            }
            /// <summary>
            /// 设置单元格值，支持合并单元格
            /// </summary>
            /// <param name="firstRow"></param>
            /// <param name="lastRow"></param>
            /// <param name="firstCol"></param>
            /// <param name="lastCol"></param>
            /// <param name="value"></param>
            /// <param name="sheet"></param>
            public void SetCellValue(int firstRow, int lastRow, int firstCol, int lastCol, object value, NPOI.SS.UserModel.ISheet sheet)
            {
                NPOI.SS.UserModel.IRow row = null;
                NPOI.SS.UserModel.ICell cell = null;

                for (int r = firstRow; r <= lastRow; r++)
                {
                    row = sheet.GetRow(r);
                    if (row == null) row = sheet.CreateRow(r);
                    for (int c = firstCol; c <= lastCol; c++)
                    {
                        cell = row.GetCell(c);
                        if (cell == null) cell = row.CreateCell(c);
                    }
                }
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(firstRow, lastRow, firstCol, lastCol));

                row = sheet.GetRow(firstRow);
                cell = row.GetCell(firstCol);
                if (value is bool) cell.SetCellValue((bool)value);
                if (value is string)
                {
                    string svalue = (string)value;
                    if (svalue != null && svalue != "" && svalue.Substring(0, 1) == "=")
                    {
                        cell.SetCellFormula(svalue.Substring(1));
                    }
                    else
                        cell.SetCellValue((string)svalue);
                }
                if (value is NPOI.SS.UserModel.IRichTextString) cell.SetCellValue((NPOI.SS.UserModel.IRichTextString)value);
                if (value is DateTime)
                {
                    if (this.DateTimeCellStyle != null)
                        cell.CellStyle = this.DateTimeCellStyle;
                    cell.SetCellValue((DateTime)value);
                }
                if (value is double) cell.SetCellValue((double)value);
                if (value is int) cell.SetCellValue((int)value);
            }

            /// <summary>
            /// 设置单元格值，支持合并单元格
            /// </summary>
            /// <param name="firstRow"></param>
            /// <param name="lastRow"></param>
            /// <param name="firstCol"></param>
            /// <param name="lastCol"></param>
            /// <param name="value"></param>
            public void SetCellValue(int firstRow, int lastRow, int firstCol, int lastCol, object value)
            {
                SetCellValue(firstRow, lastRow, firstCol, lastCol, value, DefaultSheet);
            }

            /// <summary>
            /// 设置单元格值
            /// </summary>
            /// <param name="irow"></param>
            /// <param name="icol"></param>
            /// <param name="value"></param>
            public void SetCellValue(int irow, int icol, object value)
            {
                SetCellValue(irow, irow, icol, icol, value, DefaultSheet);
            }
        }

        #endregion ========== 工具 ==========
    }
}

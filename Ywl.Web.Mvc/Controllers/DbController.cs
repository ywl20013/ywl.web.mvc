using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ywl.Web.Mvc.Controllers
{
    public class DbController<TContext> : Controller
        where TContext : Data.Entity.DbContext, new()
    {
        protected TContext db = new TContext();

        #region ========== 工具 ==========

        /// <summary>
        /// 异步提交到数据库,返回结果。结果为空字符串则成功，否则结果字符串中是保存时的出错信息。
        /// </summary>
        protected async Task<string> SaveChangesAsync()
        {
            try
            {
                await db.SaveChangesAsync();
                return "";
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)//捕获实体验证异常
            {
                var sb = new System.Text.StringBuilder();
                dbEx.EntityValidationErrors.First().ValidationErrors.ToList().ForEach(i =>
                {
                    sb.AppendFormat("属性为：{0}，信息为：{1}\n\r", i.PropertyName, i.ErrorMessage);
                });

                return sb.ToString() + "处理时间：" + DateTime.Now;

            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendFormat("信息：\n\r{0}", ex.Message);
                if (ex.InnerException != null)
                {
                    sb.AppendFormat("{0}\n\r", ex.InnerException.Message);
                    if (ex.InnerException.InnerException != null)
                    {
                        sb.AppendFormat("{0}\n\r", ex.InnerException.InnerException.Message);
                    }
                }
                return sb.ToString() + "处理时间：" + DateTime.Now;
            }
            //catch (System.Data.OptimisticConcurrencyException)//并发冲突异常
            //{

            //}
            catch (Exception ex)//捕获所有异常
            {
                //if (Logger == null)//如果没有定义日志功能，就把异常抛出来吧
                //    throw new Exception(ex.Message);
                return ex.Message + "处理时间：" + DateTime.Now;
            }
        }

        private string convertOracleType(string source)
        {
            switch (source.ToLower())
            {
                case "datetime":
                    return "Date";
                case "string":
                    return "nvarchar2";
                case "int":
                    return "number";
                case "double":
                    return "number";
                default:
                    return source;
            }
        }
        private string convertMsSqlType(string source)
        {
            switch (source.ToLower())
            {
                case "datetime":
                    return "DateTime";
                case "string":
                    return "nvarchar";
                case "double":
                    return "numeric";
                default:
                    return source;
            }
        }

        #endregion ========== 工具 ==========

        /// <summary>
        /// 检查添加数据库表
        /// </summary>
        /// <param name="Table">表名称</param>
        /// <param name="TableDescription">表描述</param>
        /// <returns></returns>
        protected async Task<string> InternalCheckAddTable(string Table, string TableDescription)
        {
            StringBuilder sb = new StringBuilder();
            if (db.GetDbType() == Ywl.Data.Entity.DbContext.DataBaseType.Oracle)
            {
                sb.AppendLine("");
                sb.AppendLine("declare");
                sb.AppendLine("  v_ret int;");
                sb.AppendLine("begin");
                sb.AppendLine("  select case when exists(select * from user_tables");
                sb.AppendLine("  where lower(table_name) = lower('" + Table + "')) then 1 else 0 end into v_ret from dual;");
                sb.AppendLine("  if v_ret = 0 then");
                sb.AppendLine("    execute immediate 'create table " + Table + " (id number(10))';");
                sb.AppendLine("    execute immediate 'comment on table " + Table + " is ''" + TableDescription + "''';");
                sb.AppendLine("    execute immediate 'create sequence sq_" + Table + "';");
                sb.AppendLine("    execute immediate 'create or replace trigger tr_" + Table + " before insert on " + Table + " for each row begin select sq_" + Table + ".nextval into :new.id from dual;end;';");
                sb.AppendLine("  end if;");
                sb.AppendLine("end;");
            }
            else if (db.GetDbType() == Ywl.Data.Entity.DbContext.DataBaseType.MSSql)
            {
                sb.Clear();
                var sql = @"
declare 
	@TableName varchar(50), @sql nvarchar(max);
begin
	set @TableName = N'{0}';

    if not exists(select * from sysobjects where xtype=N'U' and name = @TableName) begin
		print 'not exists';
		set @sql = 'create table ' + @TableName + ' (id int identity (1, 1) not null,CONSTRAINT [PK_' + @TableName + '] PRIMARY KEY ([id]))';
		execute sp_executesql @stmt = @sql;
		execute sp_addextendedproperty @name = N'MS_Description', @value = N'{1}', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = @TableName;
	end;
end;
";
                sb.Append(String.Format(sql, Table, TableDescription));
            }
            try
            {
                if (sb.Length > 0)
                    await db.Database.ExecuteSqlCommandAsync(sb.ToString());
                return "";
            }
            catch (Exception e)
            {
                return sb.ToString() + "\n" + e.Message;
            }
        }

        /// <summary>
        /// 检查添加数据表字段
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="Field"></param>
        /// <param name="FieldType"></param>
        /// <param name="FieldDescription"></param>
        /// <returns></returns>
        protected async Task<string> InternalCheckAddTableField(string Table, string Field, string FieldType, string DataLength, string DataScale, string FieldDescription)
        {
            var _FieldType = "";
            StringBuilder sb = new StringBuilder();
            if (db.GetDbType() == Ywl.Data.Entity.DbContext.DataBaseType.Oracle)
            {
                _FieldType = convertOracleType(FieldType);
                switch (FieldType)
                {
                    case "string":
                        {
                            _FieldType += "(" + DataLength + ")";
                        }
                        break;
                    case "double":
                        {
                            _FieldType += "(" + DataLength + "," + DataScale + ")";
                        }
                        break;
                }
                sb.AppendLine("");
                sb.AppendLine("declare");
                sb.AppendLine("  v_ret int;");
                sb.AppendLine("begin");
                sb.AppendLine("  select case when exists(select * from user_tab_columns");
                sb.AppendLine("  where lower(table_name) = lower('" + Table + "')");
                sb.AppendLine("    and lower(column_name) = lower('" + Field + "')) then 1 else 0 end into v_ret from dual;");
                sb.AppendLine("  if v_ret = 0 then");
                sb.AppendLine("    execute immediate 'alter table " + Table + " add " + Field + " " + _FieldType + "';");
                sb.AppendLine("    execute immediate 'comment on column " + Table + "." + Field + " is ''" + FieldDescription + "''';");
                sb.AppendLine("  end if;");
                sb.AppendLine("end;");
            }
            else if (db.GetDbType() == Ywl.Data.Entity.DbContext.DataBaseType.MSSql)
            {
                _FieldType = convertMsSqlType(FieldType);
                switch (FieldType)
                {
                    case "string":
                        {
                            _FieldType += "(" + DataLength + ")";
                        }
                        break;
                    case "double":
                        {
                            _FieldType += "(" + DataLength + "," + DataScale + ")";
                        }
                        break;
                }

                sb.Clear();
                var sql = @"
declare 
	@TableName varchar(50), @FieldName varchar(50), @sql nvarchar(max);
begin
	set @TableName = N'{0}';
	set @FieldName = N'{1}';
	if exists(select 1 from sysobjects where xtype=N'U' and name = @TableName) begin
		if not exists(select 1 from syscolumns t1 where t1.name=@FieldName and exists(select 1 from sysobjects t2 where t2.xtype='U' and t2.name=@TableName and t1.id=t2.id))
		begin
			set @sql ='alter table ' + @TableName + ' add [' + @FieldName + '] {2} null;';
			execute sp_executesql @stmt = @sql;
			execute sp_addextendedproperty @name = N'MS_Description', @value = N'{3}', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = @TableName, @level2type = N'COLUMN', @level2name = @FieldName;
		end;
	end;
end;
";
                sb.Append(String.Format(sql, Table, Field, _FieldType, FieldDescription));
            }
            try
            {
                if (sb.Length > 0)
                    await db.Database.ExecuteSqlCommandAsync(sb.ToString());
                return "";
            }
            catch (Exception e)
            {
                return sb.ToString() + "\n" + e.Message;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ywl.Data.Entity
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public class DbContext : System.Data.Entity.DbContext
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public class DataBaseType
        {
            /// <summary>
            /// MSSql
            /// </summary>
            public static string MSSql = "MSSql";
            /// <summary>
            /// Oracle
            /// </summary>
            public static string Oracle = "Oracle";
        }

        /// <summary>
        /// 数据库用户模式
        /// </summary>
        protected string SchemaName = "dbo";

        /// <summary>
        /// 数据库类型
        /// MSSql Oracle
        /// </summary>

        protected string DbType = "MSSql";//用来匹配Models.Mapping中的数据库映射

        private string NameOrConnectionString = "";

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

        private void Init()
        {
            System.Data.Entity.Database.SetInitializer<DbContext>(null);

            var reg = new Regex("(User\\sId)=\\w+(?=;)");
            var m = reg.Match(Database.Connection.ConnectionString);
            if (m.Success)
            {
                SchemaName = m.Value.Split('=')[1].ToUpper();
            }

            if (Database.Connection.GetType().ToString() == "Oracle.ManagedDataAccess.Client.OracleConnection")
            {
                this.DbType = "Oracle";
            }
            else if (Database.Connection.GetType().ToString() == "System.Data.SqlClient.SqlConnection")
            {
                this.SchemaName = "dbo";
                this.DbType = "MSSql";
            }

            string path = System.Configuration.ConfigurationManager.AppSettings["log_path"];

            if (path == null || path == "")
            {
                path = System.Web.HttpContext.Current.Server.MapPath("~/Logs/");
          
            }
            if (path != null && path != "")
            {
                if (CreateFolderIfNeeded(path))
                    Database.Log = log => System.IO.File.AppendAllText(path + "\\ef.log", string.Format("{0}{1}{2}{3}", DateTime.Now, Environment.NewLine, log, Environment.NewLine));
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DbContext() : base("DefaultConnection")
        {
            this.NameOrConnectionString = "DefaultConnection";
            Init();
        }

        /// <summary>
        /// 带有nameOrConnectionString参数的构造函数
        /// </summary>
        /// <param name="nameOrConnectionString"></param>
        public DbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            this.NameOrConnectionString = nameOrConnectionString;
            Init();
        }

        /// <summary>
        /// 创建模型时的事件
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            //一：数据库不存在时重新创建数据库
            //Database.SetInitializer<testContext>(new CreateDatabaseIfNotExists<testContext>());

            //二：每次启动应用程序时创建数据库
            //Database.SetInitializer<testContext>(new DropCreateDatabaseAlways<testContext>());

            //三：模型更改时重新创建数据库
            //Database.SetInitializer<testContext>(new DropCreateDatabaseIfModelChanges<testContext>());

            //四：从不创建数据库
            //Database.SetInitializer<testContext>(null);

            Database.SetInitializer<DbContext>(null);
            //modelBuilder.Conventions.Remove<System.Data.Entity.Infrastructure.IncludeMetadataConvention>()‌​;
#if DEBUG
            System.Diagnostics.Debug.WriteLine("Ywl.Data.Entity.DbContext.OnModelCreating: " + this.Database.Connection.ConnectionString);
#endif
            base.OnModelCreating(modelBuilder);
            ConfigMapping(modelBuilder);
        }

        /// <summary>
        /// 遍历系统内所有Model映射类
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected void ConfigMapping(DbModelBuilder modelBuilder)
        {
            List<Type> types = new List<Type>();

            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (System.Reflection.Assembly assembly in assemblies)
            {
                var _types = assembly.GetTypes();
                foreach (Type typ in _types)
                    types.Add(typ);
            }

            var typesToRegister = types
                .Where(type => !String.IsNullOrEmpty(type.Namespace))
                .Where(type => type.Name.Contains(this.DbType))
                .Where(type => type.BaseType != null && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));

            foreach (var type in typesToRegister)
            {
                // dynamic configurationInstance = Activator.CreateInstance(type);
                dynamic configurationInstance = Activator.CreateInstance(type, new object[] { SchemaName });
                modelBuilder.Configurations.Add(configurationInstance);

#if DEBUG
                System.Diagnostics.Debug.WriteLine("Ywl.Data.Entity.DbContext.OnModelCreating.ConfigMapping: " + type.FullName);
#endif
            }
        }

        /// <summary>
        /// 获取实体主键的值
        /// </summary>
        /// <param name="entry">实体</param>
        /// <returns></returns>
        protected string PrimaryKeyValue(DbEntityEntry entry)
        {
            var objectStateEntry = ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.Entity);
            if (null == objectStateEntry.EntityKey.EntityKeyValues)
            {
                return string.Empty;
            }
            return string.Join(",", objectStateEntry.EntityKey.EntityKeyValues.Select(item => item.Value.ToString()).ToArray());
        }

        /// <summary>
        /// 保存更改
        /// </summary>
        /// <returns></returns>
        public override System.Threading.Tasks.Task<int> SaveChangesAsync()
        {
            /*
            List<AuditLog> AuditLogs = new List<AuditLog>();
            var changeTracker = ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified);
            foreach (var entity in changeTracker)
            {
                if (entity.Entity != null)
                {
                    var entityName = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(entity.Entity.GetType()).Name;
                    switch (entity.State)
                    {
                        case EntityState.Modified:
                            foreach (string prop in entity.OriginalValues.PropertyNames)
                            {
                                object currentValue = entity.CurrentValues[prop];
                                object originalValue = entity.GetDatabaseValues()[prop];//OriginalValues[prop];
                                if (!Object.Equals(currentValue, originalValue) && entity.Property(prop).IsModified == true
                                    && prop.ToLower() != "lastupdateby")
                                {
                                    AuditLogs.Add(new AuditLog
                                    {
                                        EntityName = entityName,
                                        RecordID = PrimaryKeyValue(entity),
                                        State = entity.State,
                                        ColumnName = prop,
                                        OriginalValue = Convert.ToString(originalValue),
                                        NewValue = Convert.ToString(currentValue),
                                    });
                                }
                            }
                            //if (AuditLogs.Count > 0)
                            //{
                            //    TrackingLog.GetXmlForUpdate(doc, AuditLogs);
                            //    trackinglogs.Add(new DataLensTrackingLog
                            //    {
                            //        EntityName = entityName,
                            //        Email = this.Email,
                            //        XmlDoc = CompressionHelper.Compresse("XmlDoc", new UTF8Encoding().GetBytes(doc.OuterXml)),
                            //        CreateTime = DateTime.Now
                            //    });
                            //}
                            break;
                        case EntityState.Added:
                            //entityName = ObjectContext.GetObjectType(entity.Entity.GetType()).Name;
                            foreach (string prop in entity.CurrentValues.PropertyNames)
                            {
                                AuditLogs.Add(new AuditLog
                                {
                                    EntityName = entityName,
                                    RecordID = PrimaryKeyValue(entity),
                                    State = entity.State,
                                    ColumnName = prop,
                                    OriginalValue = string.Empty,
                                    NewValue = entity.CurrentValues[prop],
                                });

                            }
                            //TrackingLog.GetXmlForUpdate(doc, AuditLogs);
                            //trackinglogs.Add(new DataLensTrackingLog
                            //{
                            //    EntityName = entityName,
                            //    Email = this.Email,
                            //    XmlDoc = CompressionHelper.Compresse("XmlDoc", new UTF8Encoding().GetBytes(doc.OuterXml)),
                            //    CreateTime = DateTime.Now
                            //});
                            break;
                        case EntityState.Deleted:
                            foreach (string prop in entity.OriginalValues.PropertyNames)
                            {
                                AuditLogs.Add(new AuditLog
                                {
                                    EntityName = entityName,
                                    RecordID = PrimaryKeyValue(entity),
                                    State = entity.State,
                                    ColumnName = prop,
                                    OriginalValue = entity.OriginalValues[prop],
                                    NewValue = string.Empty,
                                });

                            }
                            //TrackingLog.GetXmlForUpdate(doc, AuditLogs);
                            //trackinglogs.Add(new DataLensTrackingLog
                            //{
                            //    EntityName = entityName,
                            //    Email = this.Email,
                            //    XmlDoc = CompressionHelper.Compresse("XmlDoc", new UTF8Encoding().GetBytes(doc.OuterXml)),
                            //    CreateTime = DateTime.Now
                            //});
                            break;
                        default:
                            break;
                    }
                }
                System.Diagnostics.Debug.WriteLine(entity.Entity.GetType());
            }
            */
            return base.SaveChangesAsync();
        }

        /*
        public override Task<int> SaveChangesAsync()
        {
            List<AuditLog> AuditLogs = new List<AuditLog>();
            List<DataLensTrackingLog> trackinglogs = new List<DataLensTrackingLog>();
            var changeTracker = ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified);
            try
            {
                foreach (var entity in changeTracker)
                {
                    AuditLogs.Clear();
                    XmlDocument doc = new XmlDocument();
                    doc.AppendChild(doc.CreateElement(TrackingLog.Records));
                    if (entity.Entity != null)
                    {
                        var entityName = ObjectContext.GetObjectType(entity.Entity.GetType()).Name;
                        //string entityName = entity.Entity.GetType().Name;
                        EntityState state = entity.State;
                        switch (entity.State)
                        {
                            case EntityState.Modified:
                                //entityName = ObjectContext.GetObjectType(entity.Entity.GetType()).Name;
                                foreach (string prop in entity.OriginalValues.PropertyNames)
                                {
                                    object currentValue = entity.CurrentValues[prop];
                                    object originalValue = entity.GetDatabaseValues()[prop];//OriginalValues[prop];
                                    if (!Object.Equals(currentValue, originalValue) && entity.Property(prop).IsModified == true
                                        && prop.ToLower() != "lastupdateby")
                                    {
                                        AuditLogs.Add(new AuditLog
                                        {
                                            EntityName = entityName,
                                            RecordID = PrimaryKeyValue(entity),
                                            State = state,
                                            ColumnName = prop,
                                            OriginalValue = Convert.ToString(originalValue),
                                            NewValue = Convert.ToString(currentValue),
                                        });
                                    }
                                }
                                if (AuditLogs.Count > 0)
                                {
                                    TrackingLog.GetXmlForUpdate(doc, AuditLogs);
                                    trackinglogs.Add(new DataLensTrackingLog
                                    {
                                        EntityName = entityName,
                                        Email = this.Email,
                                        XmlDoc = CompressionHelper.Compresse("XmlDoc", new UTF8Encoding().GetBytes(doc.OuterXml)),
                                        CreateTime = DateTime.Now
                                    });
                                }
                                break;
                            case EntityState.Added:
                                //entityName = ObjectContext.GetObjectType(entity.Entity.GetType()).Name;
                                foreach (string prop in entity.CurrentValues.PropertyNames)
                                {
                                    AuditLogs.Add(new AuditLog
                                    {
                                        EntityName = entityName,
                                        RecordID = PrimaryKeyValue(entity),
                                        State = state,
                                        ColumnName = prop,
                                        OriginalValue = string.Empty,
                                        NewValue = entity.CurrentValues[prop],
                                    });

                                }
                                TrackingLog.GetXmlForUpdate(doc, AuditLogs);
                                trackinglogs.Add(new DataLensTrackingLog
                                {
                                    EntityName = entityName,
                                    Email = this.Email,
                                    XmlDoc = CompressionHelper.Compresse("XmlDoc", new UTF8Encoding().GetBytes(doc.OuterXml)),
                                    CreateTime = DateTime.Now
                                });
                                break;
                            case EntityState.Deleted:
                                //entityName = ObjectContext.GetObjectType(entity.Entity.GetType()).Name;
                                foreach (string prop in entity.OriginalValues.PropertyNames)
                                {
                                    AuditLogs.Add(new AuditLog
                                    {
                                        EntityName = entityName,
                                        RecordID = PrimaryKeyValue(entity),
                                        State = state,
                                        ColumnName = prop,
                                        OriginalValue = entity.OriginalValues[prop],
                                        NewValue = string.Empty,
                                    });

                                }
                                TrackingLog.GetXmlForUpdate(doc, AuditLogs);
                                trackinglogs.Add(new DataLensTrackingLog
                                {
                                    EntityName = entityName,
                                    Email = this.Email,
                                    XmlDoc = CompressionHelper.Compresse("XmlDoc", new UTF8Encoding().GetBytes(doc.OuterXml)),
                                    CreateTime = DateTime.Now
                                });
                                break;
                            default:
                                break;
                        }
                    }
                }
                DataTable dt = TypeConvert.ToDataTable(trackinglogs);
                SqlDataHelper.SqlBulkCopy(dt, "DataLensTrackingLog", DataBaseType.ConnLogDataStr);
                return base.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        */

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="typ"></param>
        /// <returns></returns>
        public virtual DbSet<object> GetEntities(System.Reflection.TypeInfo typ)
        {
            foreach (var pro in this.GetType().GetProperties())
            {
                if (pro.PropertyType.FullName == typ.FullName)
                {
                    return (DbSet<object>)pro.GetValue(this);
                }
            }
            return null;
        }

        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public virtual DbSet<object> GetEntities(string typename)
        {
            foreach (var pro in this.GetType().GetProperties())
            {
                if (pro.PropertyType.FullName.Contains(typename))
                {
                    return (DbSet<object>)pro.GetValue(this);
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string GetDbType()
        {
            return this.DbType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string GetSchemaName()
        {
            return this.SchemaName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual System.Data.Common.DbConnection GetDbConnection()
        {
            return this.Database.Connection;
        }
    }
}
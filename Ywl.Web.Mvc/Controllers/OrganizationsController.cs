using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ywl.Web.Mvc.Controllers
{
    public class OrganizationsController<TContext, TEntity> : SheetsController<TContext, TEntity>
        where TContext : Data.Entity.DbContext, new()
        where TEntity : Models.BaseOrganization, new()
    {
    }
}

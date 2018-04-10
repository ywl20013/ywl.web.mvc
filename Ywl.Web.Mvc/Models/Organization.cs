using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ywl.Web.Mvc.Models
{
    [Description(Title = "用户组织机构", Description = "用户组织机构")]
    public class BaseOrganization : NamedEntity
    {
        [Display(Name = "父级Id", Description = "")]
        public int PId { get; set; }

        [Display(Name = "是否部门", Description = "")]
        public bool IsDepartment { get; set; }

        [Display(Name = "是否班组", Description = "")]
        public bool IsGroup { get; set; }

        [MaxLength(10)]
        [Display(Name = "排序号", Description = "")]
        public string OrderNumber { get; set; }
    }
}

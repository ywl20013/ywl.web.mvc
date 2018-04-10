using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ywl.Web.Mvc.Models
{
    [Description(Title = "父子文档类型实体", Description = "父子文档类型实体")]
    public class ParentChildEntity : NamedEntity
    {
        [Display(Name = "密码", Description = "")]
        public int? ParentId { get; set; }

        [NotMapped]
        [Display(Name = "父文档", Description = "")]
        public ParentChildEntity Parent { get; set; }

        [NotMapped]
        [Display(Name = "子文档集", Description = "")]
        public List<ParentChildEntity> Children { get; set; }

        [NotMapped]
        [MaxLength(2000)]
        [Display(Name = "层次路径", Description = "")]
        public string HierarchicalPath { get; set; }

        [NotMapped]
        [Display(Name = "是否存在子文档")]
        public bool HasChild { get { return this.Children.Count > 0; } }

        public ParentChildEntity()
        {
            Children = new List<ParentChildEntity>();
        }
    }
}

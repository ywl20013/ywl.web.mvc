using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ywl.Web.Mvc.Models
{
    public class NamedEntity : Entity
    {
        [MaxLength(256)]
        [Display(Name = "名称", Description = "")]
        public string Name { get; set; }
    }
    public class CreatedEntity : NamedEntity
    {
        /// <summary>
        /// 创建人
        /// </summary>
        [Display(Name = "创建人", Description = "")]
        public int? Creator { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Display(Name = "创建人", Description = "")]
        [MaxLength(50)]
        public string CreatorName { get; set; }
    }
}

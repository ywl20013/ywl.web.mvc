using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ywl.Web.Mvc.Models
{
    public class Moudle : ParentChildEntity
    {
        /// <summary>
        /// 是否需要权限
        /// </summary>
        [Display(Name = "是否需要权限")]
        public bool? NeedPower { get; set; }

        /// <summary>
        /// 链接
        /// </summary>
        [MaxLength(256)]
        [Display(Name = "链接")]
        public string Url { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "分类")]
        public string Category { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>
        [MaxLength(256)]
        [Display(Name = "命名空间")]
        public string NameSpace { get; set; }
    }
}

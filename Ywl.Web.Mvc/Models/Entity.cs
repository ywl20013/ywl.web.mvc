using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ywl.Web.Mvc.Models
{
    public class Status
    {
        public static string Normal = "normal";
    }
    public class Entity : Interfaces.IEntity
    {
        /// <summary>
        /// 主键
        /// 必须映射到数据库
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 必须映射到数据库
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "数据状态")]
        public string Status { get; set; }

        public Entity()
        {
            this.Id = 0;
            this.Status = Ywl.Web.Mvc.Models.Status.Normal;
        }
    }
}

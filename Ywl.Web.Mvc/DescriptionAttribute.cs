using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ywl.Web.Mvc
{
    [AttributeUsage(AttributeTargets.All)]
    public class DescriptionAttribute : System.ComponentModel.DescriptionAttribute
    {
        public DescriptionAttribute() { }
        public DescriptionAttribute(string title) { this.Title = title; }
        public DescriptionAttribute(string title, string description) { this.Title = title; this.Description = description; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public new string Description { get { return this.DescriptionValue; } set { this.DescriptionValue = value; } }

        public static DescriptionAttribute getDescription(Type typ)
        {
            var attributes = Attribute.GetCustomAttributes(typ);
            foreach (var attr in attributes)
            {
                if (attr is DescriptionAttribute)
                {
                    return attr as DescriptionAttribute;
                }
            }
            return new DescriptionAttribute { Title = typ.Name, Description = typ.Name };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace RabbitMQTest.Model
{
    [DataContract]
    public class RequestEntity
    {
        /// <summary>
        /// 取号类型,具体是指针对哪个系统取号
        /// </summary>
        [DataMember]
        public string GenNoType { get; set; }
        /// <summary>
        /// 取号完成后要执行的SQL集合
        /// </summary>
        [DataMember]
        public List<string> SQLList { get; set; }
    }
}

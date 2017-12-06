using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace RabbitMQTest.Model
{
    [DataContract]
    public class ResponseEntity
    {
        /// <summary>
        /// 生成的单号
        /// </summary>
        [DataMember]
        public string No { get; set; }
        /// <summary>
        /// 处理结果
        /// </summary>
        [DataMember]
        public bool HandleResult { get; set; }
        /// <summary>
        /// 处理结果信息
        /// </summary>
        [DataMember]
        public string HandleMessage { get; set; }
    }
}

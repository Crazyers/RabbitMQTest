using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQTest.Model;
using System.Data;

namespace RabbitMQTest.Utils
{
    public class GenNoHelper
    {
        public static ResponseEntity Insert4ContactListApply(RequestEntity requestEntity)
        {
            ResponseEntity responseEntity=new ResponseEntity();
            string strNo = GenNo4ContactListApply();
            if (string.IsNullOrEmpty(strNo))
            {
                responseEntity.No = "";
                responseEntity.HandleResult = false;
                responseEntity.HandleMessage = "取号失败";

            }
            else
            {
                int count= OracleHelper.ExecuteNonQuery(OracleHelper.WEBDBConnection, "INSERT INTO tb_no_test(testno) VALUES ('" + strNo + "')");
                if (count > 0)
                {
                    responseEntity.No = strNo;
                    responseEntity.HandleResult = true;
                    responseEntity.HandleMessage = "保存单据成功";
                }
                else
                {
                    responseEntity.No = strNo;
                    responseEntity.HandleResult = false;
                    responseEntity.HandleMessage = "取号成功,执行其他SQL语句失败";
                }
            }
            return responseEntity;
        }

        public static string GenNo4ContactListApply()
        {
            string strYearMonth = DateTime.Now.ToString("yyyyMM");
            string strGetMaxID = "SELECT MAX(testno) id from tb_no_test ";//查询表中的字段   
            string maxID = Convert.ToString(OracleHelper.ExecuteScalar(OracleHelper.WEBDBConnection, CommandType.Text, strGetMaxID));
            string Result = "";
            if (maxID == "ThrowException")
            {
                Result = "";
            }
            else if (maxID == "")//没有最大编号   
            {
                Result = "S" + strYearMonth + "000000001";//S200902001   
            }
            else
            {
                //截取字符   
                string strFirstSix = maxID.Substring(1, 6);
                string strLastThree = maxID.Substring(7, 9);
                if (strYearMonth == strFirstSix)//截取的最大编号（20090225）是否和数据库服务器系统时间相等   
                {
                    string strNewFour = (Convert.ToInt32(strLastThree) + 1).ToString("000000000");//0000+1   
                    Result = "S" + strYearMonth + strNewFour;//CG2009020001   
                }
                else
                {
                    Result = "S" + strYearMonth + "000000001";
                }
            }
            return Result;
        }
    }
}

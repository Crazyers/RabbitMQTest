using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RabbitMQTest.Model;

namespace RabbitMQTest.Web
{
    public partial class WCFTestForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnGenNo_Click(object sender, EventArgs e)
        {
            try
            {
                WCFMQService.MQServiceClient service = new WCFMQService.MQServiceClient();
                service.InnerChannel.OperationTimeout = Convert.ToDateTime("00:10:00").TimeOfDay;

                RequestEntity requestEntity = new RequestEntity();
                requestEntity.GenNoType = "";
                ResponseEntity responseEntity = service.ContactListApply(requestEntity);
                Response.Write(responseEntity.No);
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace WebAPI.Models
{
    public class CommonFunction
    {
        #region String Date 

        public static string ConvertDateToString(DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        public static DateTime ConvertStringToDate(string date)
        {
            var vSplitDate = date.Split('/');

            var vDate = vSplitDate[2] + "-" + vSplitDate[1] + "-" + vSplitDate[0];

            return Convert.ToDateTime(vDate);
        }

        #endregion

        #region Genrate Order No

        public static string GenerateOrderNo()
        {
            var OrderNo = "";

            var perviousOrderNo = Database.getValue("SELECT ORDERNO FROM ORDERMASTER");
            if (string.IsNullOrEmpty(perviousOrderNo))
            {
                OrderNo = "0000000001";
            }
            else
            {
                OrderNo = "";
                var currentOrderNo = Database.getValue("SELECT MAX(ORDERNO) FROM ORDERMASTER");
                var nextOrderNo = NextNo(currentOrderNo);
                OrderNo = nextOrderNo;
            }
            return OrderNo;
        }

        #endregion

        public static string NextNo(string currentId)
        {
            int i = 0;
            if (int.TryParse(currentId, out i))
            {
                i++;
                return (i.ToString().PadLeft(10, '0'));
            }
            throw new System.ArgumentException("Non-numeric string passed as argument");
        }

        #region SendMail

        public static string SendMail(string ToEmailID, string bodyContent, string Subject)
        {
            TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), INDIAN_ZONE);

            string sendMail = "";
            try
            {
                MailMessage mailMessage = new MailMessage("orders@freshfoodz.in", ToEmailID, Subject, bodyContent);
                mailMessage.IsBodyHtml = true;
                SmtpClient smtpClient = new SmtpClient("webmail.freshfoodz.in", 25);
                smtpClient.EnableSsl = false;
                smtpClient.UseDefaultCredentials = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new NetworkCredential("orders@freshfoodz.in", "FoodOrder@123");
                smtpClient.Send(mailMessage);
                sendMail = "Sucess";

                #region Add Log

                string query = "insert into [ActivityLog] values('" + datetime + "', '" + Subject + "', 'Sent Mail to " + ToEmailID + "', 1)";
                string output = Database.insertData(query);

                #endregion
            }
            catch (Exception ex)
            {
                #region Add Log

                string query = "insert into [ActivityLog] values('" + datetime + "', '" + Subject + "', 'Error " + ex.Message + "', 0)";
                string output = Database.insertData(query);

                #endregion

                sendMail = "Error";
            }
            return sendMail;
        }

        #endregion

        #region Render View ToString

        public static string RenderViewToString(ControllerContext context, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = context.RouteData.GetRequiredString("action");

            var viewData = new ViewDataDictionary(model);

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(context, viewName);
                var viewContext = new ViewContext(context, viewResult.View, viewData, new TempDataDictionary(), sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        #endregion
    }
}
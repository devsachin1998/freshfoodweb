using Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Admin.Helpers
{
    public class CommonHelper
    {
        public static List<SelectListItem> GetUnit()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string query = "select * from [Unit] Order by 2";

            DataSet ds = Database.getDataSet(query);
            DataTable dt = ds.Tables[0];

            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Select UOM --"
            });

            if (ds != null || dt.Rows.Count > 0)
            {
                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    list.Add(new SelectListItem()
                    {
                        Value = dt.Rows[n][0].ToString(),
                        Text = dt.Rows[n][1].ToString(),
                    });
                    n++;
                }
            }
            return list;
        }


        public static string serverKey = "AAAAKnm38N4:APA91bEMQNIBKT3fr9luzHmrm5TXlblE46XAtUMNvFEngRYbRk7E1DwWlb1qH0U1lEssc4qd5gK-AgQL2yN_5uWjTsC2H2afXlRbd4Jl1tWVtpxp2WZP_s_nd6jW9RdL9mk-586f40PY";
        public static string senderId = "182430724318";
        public static string webAddr = "https://fcm.googleapis.com/fcm/send";


        public static string SendNotification(string DeviceToken, string title, string msg, string Isimage)
        {
            var result = "-1";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Headers.Add(string.Format("Authorization: key={0}", serverKey));
            httpWebRequest.Headers.Add(string.Format("Sender: id={0}", senderId));
            httpWebRequest.Method = "POST";
            // var payload = "";
            var payload = new Object();

            if (Isimage == "" || Isimage == null)
            {
                payload = new
                {
                    to = DeviceToken,
                    priority = "high",
                    content_available = true,
                    data = new
                    {
                        body = msg,
                        title = title,


                    },
                };
            }
            else
            {
                payload = new
                {
                    to = DeviceToken,
                    priority = "high",
                    content_available = true,
                    data = new
                    {
                        body = msg,
                        title = title,
                        image = Isimage,

                    },
                };
            }


            var serializer = new JavaScriptSerializer();
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = serializer.Serialize(payload);
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            return result;
        }



        public static List<SelectListItem> GetCategory()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string query = "select * from [CategoryMaster] Order by 2";

            DataSet ds = Database.getDataSet(query);
            DataTable dt = ds.Tables[0];

            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Select Category --"
            });

            if (ds != null || dt.Rows.Count > 0)
            {
                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    list.Add(new SelectListItem()
                    {
                        Value = dt.Rows[n][0].ToString(),
                        Text = dt.Rows[n][1].ToString(),
                    });
                    n++;
                }
            }
            return list;
        }

        public static List<SelectListItem> GetItem()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string query = "select * from ITEMMASTER Order by 2";

            DataSet ds = Database.getDataSet(query);
            DataTable dt = ds.Tables[0];

            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Select Item --"
            });

            if (ds != null || dt.Rows.Count > 0)
            {
                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    list.Add(new SelectListItem()
                    {
                        Value = dt.Rows[n][0].ToString(),
                        Text = dt.Rows[n][1].ToString(),
                    });
                    n++;
                }
            }
            return list;
        }

        public static List<SelectListItem> GetPurchaseItem()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string query = "select * from ITEMMASTER Where ItemID not in (select itemid from Item_Purchase) Order by 2";

            DataSet ds = Database.getDataSet(query);
            DataTable dt = ds.Tables[0];

            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Select Item --"
            });

            if (ds != null || dt.Rows.Count > 0)
            {
                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    list.Add(new SelectListItem()
                    {
                        Value = dt.Rows[n][0].ToString(),
                        Text = dt.Rows[n][1].ToString(),
                    });
                    n++;
                }
            }
            return list;
        }

        public static string ConvertDateToString(DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        public static List<SelectListItem> GetCustomerList()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string query = "select * from [tbl_Register_User] where isActive = 1 order by name";

            DataSet ds = Database.getDataSet(query);
            DataTable dt = ds.Tables[0];

            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Select Customer --"
            });

            if (ds != null || dt.Rows.Count > 0)
            {
                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    list.Add(new SelectListItem()
                    {
                        Value = dt.Rows[n][0].ToString(),
                        Text = dt.Rows[n][2].ToString(),
                    });
                    n++;
                }
            }
            return list;
        }

        #region Order Status

        public static List<SelectListItem> OrderStatus()
        {
            List<SelectListItem> typeList = new List<SelectListItem>();

            typeList.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Select Status --",
            });
            typeList.Add(new SelectListItem()
            {
                Value = "Placed",
                Text = "Placed",
            });
            typeList.Add(new SelectListItem()
            {
                Value = "Accepted",
                Text = "Accepted",
            });
            typeList.Add(new SelectListItem()
            {
                Value = "Declined",
                Text = "Declined",
            });
            typeList.Add(new SelectListItem()
            {
                Value = "Ready",
                Text = "Ready",
            });
            typeList.Add(new SelectListItem()
            {
                Value = "Delivered",
                Text = "Delivered",
            });
            typeList.Add(new SelectListItem()
            {
                Value = "Cancelled by Admin",
                Text = "Cancelled by Admin",
            });

            return typeList;
        }

        #endregion

        #region Payment Status

        public static List<SelectListItem> PaymentStatus()
        {
            List<SelectListItem> typeList = new List<SelectListItem>();

            typeList.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Select Status --",
            });
            typeList.Add(new SelectListItem()
            {
                Value = "Pending",
                Text = "Pending",
            });
            typeList.Add(new SelectListItem()
            {
                Value = "Received",
                Text = "Received",
            });

            return typeList;
        }

        #endregion

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

        public static string SendDeliveryMail(string ToEmailID, string bodyContent, string Subject, string orderSummary)
        {
            TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), INDIAN_ZONE);

            string sendMail = "";
            try
            {
                MailMessage mailMessage = new MailMessage("orders@freshfoodz.in", ToEmailID, Subject, bodyContent);
                mailMessage.Attachments.Add(new Attachment(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/OrderSummary/" + orderSummary)));
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

        public static List<SelectListItem> GetOffer()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string query = "select * from offerMaster where IsActive = 1 order by 2";

            DataSet ds = Database.getDataSet(query);
            DataTable dt = ds.Tables[0];

            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Select Offer Type --"
            });

            if (ds != null || dt.Rows.Count > 0)
            {
                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    list.Add(new SelectListItem()
                    {
                        Value = dt.Rows[n][0].ToString(),
                        Text = dt.Rows[n][1].ToString(),
                    });
                    n++;
                }
            }
            return list;
        }

        public static List<SelectListItem> AppCashback()
        {
            List<SelectListItem> typeList = new List<SelectListItem>();

            typeList.Add(new SelectListItem()
            {
                Value = "",
                Text = "-- Select Applicable Cashback --",
            });
            typeList.Add(new SelectListItem()
            {
                Value = "Percentage",
                Text = "Percentage",
            });

            typeList.Add(new SelectListItem()
            {
                Value = "Value",
                Text = "Value",
            });

            return typeList;
        }

    }
}
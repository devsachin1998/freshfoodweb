using Admin.Helpers;
using Admin.Models;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.Services.Description;

namespace Admin.Controllers
{
    public class NotificationController : Controller

    {
        public static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), INDIAN_ZONE);

        #region Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string UserName, string Password)
        {
            try
            {
                int UserID = 0;
                string DisplayName = "";

                string query = "select * from AdminUser Where UserName= '" + UserName + "' and Password= '" + Password + "' and IsActive=1";

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    TempData["ErrorMessage"] = "Incorrect username or password. Please try again !!";
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    UserID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    DisplayName = dt.Rows[n][1].ToString();
                    n++;
                }

                if (UserID > 0 && !string.IsNullOrEmpty(DisplayName))
                {
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, DisplayName, DateTime.Now, DateTime.Now.AddMinutes(80000), true, "Admin", FormsAuthentication.FormsCookiePath);
                    string hash = FormsAuthentication.Encrypt(ticket);
                    HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);

                    if (ticket.IsPersistent)
                    {
                        cookie.Expires = ticket.Expiration;
                    }

                    Response.Cookies["UserID"].Value = UserID.ToString();
                    Response.Cookies["DisplayName"].Value = DisplayName;

                    Response.Cookies.Add(cookie);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "User account has not been created.";
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "error" + e.Message;
                Console.WriteLine(e.Message);
                return View();
            }
        }

        #endregion

        #region LogOut

        public ActionResult Logout()
        {
            Session.Abandon();
            Session.Clear();
            FormsAuthentication.SignOut();
            TempData["ErrorMessage"] = "";
            return RedirectToAction("Login");
        }

        #endregion
        #region Manage Notification

        [HttpGet]
        public ActionResult Notificationlist()
        {
            Notification_Model data = new Notification_Model();
            return View(data);
        }


        [HttpPost]
        public ActionResult Notificationlist(Notification_Model Data)
        {
            string Ttl = Data.Tittle;
            string Msg = Data.Message;
            string img = Data.Image;
            string Isimage = img;
            string notification_query = "";
            int SelectedData = Data.SelectedValues.Length;
           

            if ((Data.SelectedValues.Length == null || Data.SelectedValues.Length == 0))
            {
                notification_query = "insert into Tbl_Notifications values('0', '" + Ttl + "', '" + img + "', '" + Msg + "','"+ datetime + "','1')";
                string output = Database.insertData(notification_query);
               
                if (output == "200")
                {
                   
                
                string query = "select fcmtoken from tbl_register_user where fcmtoken != '' GROUP BY fcmtoken ";
                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string fcmtoken = dt.Rows[i][0].ToString();
                    if (fcmtoken != null && fcmtoken != "")
                    {
                        string data = CommonHelper.SendNotification(fcmtoken, Ttl, Msg, Isimage);
                    }
                }
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occured please try again !!";

                }
            }
            else
            {
                string[] result = Array.ConvertAll(Data.SelectedValues, x => x.ToString());

                string user_id = String.Join(",", result);
                notification_query = "insert into Tbl_Notifications values('"+ user_id + "', '" + Ttl + "', '" + img + "', '" + Msg + "','" + datetime + "','1')";
                string output = Database.insertData(notification_query);

                if (output == "200")
                {

                    for (int i = 0; i < Data.SelectedValues.Length; i++)
                    {
                        string fcmtoken = Database.getValue("select  fcmtoken from tbl_Register_User WHERE UserID = " + Data.SelectedValues[i] + "");
                        if (fcmtoken != null && fcmtoken != "")
                        {
                            string data = CommonHelper.SendNotification(fcmtoken, Ttl, Msg, Isimage);
                        }
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occured please try again !!";

                }
            }
            TempData["SuccessMessage"] = "Notification Sucessfully Send !!";
            return RedirectToAction("NotificationList");

        }

        #endregion
    }
}
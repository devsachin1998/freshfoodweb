using Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Admin.Controllers
{
    public class AccountController : Controller
    {
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
                TempData["ErrorMessage"] = "error"+e.Message;
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

        #region Manage User

        [HttpGet]
        public ActionResult UserList()
        {
            try
            {
                string query = "select * from tbl_Register_User order by UserID DESC";
                    //"select * from tbl_Register_User order by name";


                List<User_Model> userList = new List<User_Model>();

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    userList = new List<User_Model>();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    User_Model userData = new User_Model();

                    userData.UserID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    userData.RoleID = ((dt.Rows[n][1] == null) ? 0 : Convert.ToInt32(dt.Rows[n][1]));
                    userData.Name = dt.Rows[n][2].ToString();
                    userData.UserName = dt.Rows[n][3].ToString();
                    userData.Password = dt.Rows[n][4].ToString();
                    userData.MobileNo = dt.Rows[n][5].ToString();
                    userData.Email = dt.Rows[n][6].ToString();
                    userData.IsActive = Convert.ToBoolean(dt.Rows[n][7]);
                    userData.Address = dt.Rows[0][11].ToString();
                    userData.CreatedDate = dt.Rows[n][9].ToString();
                    userList.Add(userData);
                    n++;
                }

                return View(userList);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = "An error occured please try again !!";
                return RedirectToAction("UserList");
            }
        }

        [HttpGet]
        public ActionResult UserDetail(int UserID = 0)
        {
            try
            {
                string query = "select * from tbl_Register_User where UserID = " + UserID;

                User_Model userData = new User_Model();

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    userData = new User_Model();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    userData = new User_Model();

                    userData.UserID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    userData.RoleID = ((dt.Rows[n][1] == null) ? 0 : Convert.ToInt32(dt.Rows[n][1]));
                    userData.Name = dt.Rows[n][2].ToString();
                    userData.UserName = dt.Rows[n][3].ToString();
                    userData.Password = dt.Rows[n][4].ToString();
                    userData.MobileNo = dt.Rows[n][5].ToString();
                    userData.Email = dt.Rows[n][6].ToString();
                    userData.Address = dt.Rows[0][11].ToString();
                    userData.CreatedDate = dt.Rows[n][9].ToString();
                    userData.IsActive = Convert.ToBoolean(dt.Rows[n][7]);
                    n++;
                }

                return View(userData);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = "An error occured please try again !!";
                return RedirectToAction("UserList");
            }
        }

        [HttpPost]
        public ActionResult SaveUserData(User_Model dataString)
        {
            try
            {
                TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), INDIAN_ZONE);


                string query = "";
                if (dataString.UserID == 0)
                {
                    query = "insert into tbl_Register_User values (" + dataString.RoleID + ",'" + dataString.Name + "','" + dataString.UserName + "','" + dataString.Password + "','" + dataString.MobileNo + "','" + dataString.Email + "', 1, null, '" + datetime + "', null, '"+dataString.Address+"')";

                    TempData["SuccessMessage"] = "New user added Sucessfully !!";
                }
                else
                {
                    query = "update tbl_Register_User set Name ='" + dataString.Name + "', UserName = '" + dataString.UserName + "', Password = '" + dataString.Password + "', MobileNo = '" + dataString.MobileNo + "', Email = '" + dataString.Email + "', UpdatedDate = '" + datetime + "', Address = '" + dataString.Address + "' Where UserID =" + dataString.UserID;

                    TempData["SuccessMessage"] = "Item details updated Sucessfully !!";
                }
                string output = Database.insertData(query);
                if (output != "200")
                {
                    TempData["ErrorMessage"] = "An error occured please try again !!";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return RedirectToAction("UserList");
        }

        [HttpGet]
        public JsonResult CheckNameIsExist(string Name)
        {
            string response = "";
            try
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    string output = Database.getValue("SELECT Name FROM tbl_Register_User WHERE Name = '" + Name + "' ");
                    if (!string.IsNullOrEmpty(output))
                    {
                        response = "Exist";
                    }
                }
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                response = "An error occured please try again !!";
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult DeleteName(int UserID = 0)
        {
            string response = "";
            try
            {
                string output = Database.insertData("Delete from tbl_Register_User where UserID =" + UserID);
                if (output != "200")
                {
                    response = "An error occured please try again !!";
                }
                else
                {
                    response = "User deleted Sucessfully !!";
                }
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
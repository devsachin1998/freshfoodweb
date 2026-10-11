using Admin.Helpers;
using Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
using System.Web.Security;

namespace Admin.Controllers
{
    public class HomeController : Controller
    {
        public static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), INDIAN_ZONE);


        [Route("Dashboard")]
        public ActionResult Index()
        {
            Dashboard_Model data = new Dashboard_Model();

            try
            {
                data.Balance = Database.getValue("select Balance from [Balance]");
                data.ItemCount = Database.getValue("select count(*) ActiveItem from [ItemStock]");
                data.UserCount = Database.getValue("select count(*) as RegisterUsers from [tbl_Register_User] Where RoleID = 4");
                data.OrderCount = Database.getValue("select ISNULL(count(*),0)  as TodayOrders from [OrderMaster] where  OrderStatus!='Delivered' and DATEADD(dd, 0, DATEDIFF(dd, 0, OrderDate)) = DATEADD(dd, 0, DATEDIFF(dd, 0, '" + datetime + "'))");
                data.DeliveredOrederd = Database.getValue("select ISNULL(count(*),0)  as TodayOrders from [OrderMaster] where  OrderStatus ='Delivered' and DATEADD(dd, 0, DATEDIFF(dd, 0, DeliveryDate)) = DATEADD(dd, 0, DATEDIFF(dd, 0, '" + datetime + "'))");
                data.Online = Database.getValue("select ISNULL(sum(Amount),0) as OfflineAmt from UserWalletHistory where Payment_Status='Offline' and Type='Credit' and  DATEADD(dd, 0, DATEDIFF(dd, 0, EntryDate)) = DATEADD(dd, 0, DATEDIFF(dd, 0,'" + datetime + "'))");
                data.Offlineamount = Database.getValue("select ISNULL(sum(Amount),0) as OfflineAmt from UserWalletHistory where Payment_Status!='Offline' and Type='Credit' and  DATEADD(dd, 0, DATEDIFF(dd, 0, EntryDate)) = DATEADD(dd, 0, DATEDIFF(dd, 0,'" + datetime + "'))");

                data.Orderamount = Database.getValue("select ISNULL(Sum(Total),0) as Total from[OrderMaster] where OrderStatus!='Delivered' and  DATEADD(dd, 0, DATEDIFF(dd, 0, OrderDate)) = DATEADD(dd, 0, DATEDIFF(dd, 0,'" + datetime + "'))");
                #region Daily Order List



                data.OnlineAmount = Database.getValue("select ISNULL(SUM(CAST(Amount AS DECIMAL)), 0) as Amount from[Tbl_OnlinePayment] where PaymentStatus='Success' and  DATEADD(dd, 0, DATEDIFF(dd, 0, CreatedDate)) = DATEADD(dd, 0, DATEDIFF(dd, 0,'" + datetime + "'))");



                data.Newcustomer = Database.getValue("select ISNULL(count(*),0)  as Customer from [tbl_Register_User] where DATEADD(dd, 0, DATEDIFF(dd, 0, CreatedDate)) = DATEADD(dd, 0, DATEDIFF(dd, 0, '" + datetime + "'))");
                #region Daily Order List

                string query = "SELECT *  FROM [ORDERMASTER] WHERE OrderStatus!='Delivered' and DATEADD(DD, 0, DATEDIFF(DD, 0, ORDERDATE)) = DATEADD(DD, 0, DATEDIFF(DD, 0,'" + datetime + "')) ORDER BY 1 DESC";

                 data._orderList = new List<Order_Model>();

                //DataSet ds = Database.getDataSet("SELECT *  FROM [ORDERMASTER] WHERE DATEADD(DD, 0, DATEDIFF(DD, 0, ORDERDATE)) = DATEADD(DD, 0, DATEDIFF(DD, 0,'8/30/2022 9:43:52 PM')) ORDER BY 1 DESC");
                DataSet ds = Database.getDataSet(query);

                DataTable dt = ds.Tables[0];

                if (ds == null || dt.Rows.Count == 0)
                {
                    data._orderList = new List<Order_Model>();
                }

                int n = 0;
                int count = dt.Rows.Count;

                #region Order Data

                while (n < count)
                {
                    Order_Model orderDetails = new Order_Model();

                    orderDetails.OrderID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    orderDetails.OrderNo = dt.Rows[n][1].ToString();
                    orderDetails.OrderDate = dt.Rows[n][2].ToString();

                    orderDetails.UserID = ((dt.Rows[n][3] == null) ? 0 : Convert.ToInt32(dt.Rows[n][3]));
                    orderDetails.CustomerName = dt.Rows[n][4].ToString();
                    orderDetails.CustomerMobile = dt.Rows[n][5].ToString();
                    orderDetails.CustomerEmail = dt.Rows[n][6].ToString();
                    orderDetails.CustomerAddress = dt.Rows[n][7].ToString();
                    orderDetails.DeliveryAddress = dt.Rows[n][8].ToString();

                    orderDetails.NoofItem = ((dt.Rows[n][9] == null) ? 0 : Convert.ToInt32(dt.Rows[n][9]));

                    orderDetails.OrderStatus = dt.Rows[n][10].ToString();
                    orderDetails.PaymentStatus = dt.Rows[n][11].ToString();

                    orderDetails.CustomerRemark = dt.Rows[n][12].ToString();
                    orderDetails.AdminRemark = dt.Rows[n][13].ToString();

                    orderDetails.Total = Convert.ToDecimal(dt.Rows[n][14]);

                    orderDetails.SysDeliveryDate = dt.Rows[n][17].ToString();


                    if (!string.IsNullOrEmpty(dt.Rows[n][18].ToString()))
                    {
                        orderDetails.PaidAmount = Convert.ToDecimal(dt.Rows[n][18]);
                    }

                    orderDetails.PaymentDate = dt.Rows[n][19].ToString();

                    #region Item List

                    //orderDetails._itemList = new List<Order_Item>();

                    //string itemQuery = "SELECT * FROM ORDER_ITEMS WHERE ORDERID =" + orderDetails.OrderID;

                    //DataSet item_ds = Database.getDataSet(itemQuery);
                    //DataTable item_dt = item_ds.Tables[0];
                    //if (item_ds == null || item_dt.Rows.Count == 0)
                    //{
                    //    orderDetails._itemList = new List<Order_Item>();
                    //}

                    //int i = 0;
                    //int item_count = item_dt.Rows.Count;

                    //#region Item List

                    //while (i < item_count)
                    //{
                    //    Order_Item itemData = new Order_Item();

                    //    itemData.OrderItemID = Convert.ToInt32(item_dt.Rows[i][0]);
                    //    itemData.OrderID = Convert.ToInt32(item_dt.Rows[i][1]);
                    //    itemData.OrderNo = item_dt.Rows[i][2].ToString();
                    //    itemData.ItemID = Convert.ToInt32(item_dt.Rows[i][3]);
                    //    itemData.ItemName = item_dt.Rows[i][4].ToString();
                    //    itemData.OrderQty = Convert.ToDecimal(item_dt.Rows[i][5]);
                    //    itemData.Price = Convert.ToDecimal(item_dt.Rows[i][6]);

                    //    orderDetails._itemList.Add(itemData);

                    //    i++;
                    //}

                    //#endregion

                    #endregion

                    //string InvoiceFile = OrderSummary.GenerateOrderSummary(orderDetails);
                    //if (InvoiceFile != "Error.pdf")
                    //{
                    //    orderDetails.Invoice = InvoiceFile;
                    //}

                    data._orderList.Add(orderDetails);

                    n++;
                }

                #endregion

                #endregion

                #region Wallet Request List

                string w_query = "SELECT *  FROM USERWALLETHISTORY WHERE STATUS IN ('Request to Credit Balance') ORDER BY 1 DESC";

                data._walletRequest = new List<Wallet_History>();

                DataSet w_ds = Database.getDataSet(w_query);
                DataTable w_dt = w_ds.Tables[0];
                if (w_ds == null || w_dt.Rows.Count == 0)
                {
                    data._walletRequest = new List<Wallet_History>();
                }

                int w_n = 0;
                int w_count = w_dt.Rows.Count;

                #region Credit Request

                while (w_n < w_count)
                {
                    Wallet_History walletData = new Wallet_History();

                    walletData.HistoryID = ((w_dt.Rows[w_n][0] == null) ? 0 : Convert.ToInt32(w_dt.Rows[w_n][0]));
                    walletData.WalletID = ((w_dt.Rows[w_n][1] == null) ? 0 : Convert.ToInt32(w_dt.Rows[w_n][1]));
                    walletData.UserID = ((w_dt.Rows[w_n][2] == null) ? 0 : Convert.ToInt32(w_dt.Rows[w_n][2]));
                    walletData.UserName = Database.getValue("select  name from tbl_Register_User WHERE UserID = " + walletData.UserID + "");
                    walletData.Amount = Convert.ToDecimal(w_dt.Rows[w_n][5]);

                    data._walletRequest.Add(walletData);

                    w_n++;
                }

                #endregion

                #endregion

                #region Online Payment History

                // string O_query = "SELECT *  FROM Tbl_OnlinePayment";

                string O_query = "SELECT *  FROM [Tbl_OnlinePayment] WHERE PaymentStatus='Success' and  DATEADD(DD, 0, DATEDIFF(DD, 0, CreatedDate)) = DATEADD(DD, 0, DATEDIFF(DD, 0,'" + datetime + "')) ORDER BY 1 DESC";

                data._walletData = new List<Wallet_Model>();

                DataSet o_ds = Database.getDataSet(O_query);
                DataTable o_dt = o_ds.Tables[0];
                if (o_ds == null || o_dt.Rows.Count == 0)
                {
                    data._walletData = new List<Wallet_Model>();
                }

                int o_n = 0;
                int o_count = o_dt.Rows.Count;

                #region Credit Request

                while (o_n < o_count)
                {
                    Wallet_Model walletData1 = new Wallet_Model();

                    walletData1.UserID = ((o_dt.Rows[o_n][2] == null) ? 0 : Convert.ToInt32(o_dt.Rows[o_n][2]));
                    walletData1.UserName = o_dt.Rows[o_n][3].ToString();
                    walletData1.Amount = Convert.ToDecimal(o_dt.Rows[o_n][4]);
                    walletData1.CreatedDate = o_dt.Rows[o_n][5].ToString();
                    walletData1.PaymentStatus = o_dt.Rows[o_n][8].ToString();
                    data._walletData.Add(walletData1);
                    o_n++;
                }

                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return View(data);
        }

        #region Wallet Credit Request

        [HttpGet]
        public JsonResult ManageWalletRequest(int HistoryID = 0, int WalletID = 0, int UserID = 0, decimal Amount = 0, string StatusName = "")
        {
            string returnMessage = "", query = "", output = "";
            try
            {
                if (StatusName == "Approved")
                {
                    // check view balance is disabled or not && get pervious balance
                    var is_Display = Database.getValue("select IsDisplay from UserWallet Where WalletID = " + WalletID + " and UserID = " + UserID + "");
                    bool isDisplay = Convert.ToBoolean(is_Display);

                    //var existing_Balance = Database.getValue("select Balance from UserWallet Where WalletID = " + WalletID + " and IsDisplay = 1 and UserID = " + UserID + "");
                    //decimal existingBalance = Convert.ToDecimal(existing_Balance);

                    if (isDisplay == true)
                    {
                        // Update record from UserWalletHistory table
                        //decimal newBalance = existingBalance + Amount;

                        query = "";
                        query = "Update UserWalletHistory Set Status = 'Approved', Balance = + " + Amount + " Where HistoryID = " + HistoryID + " and WalletID = " + WalletID + " and UserID = " + UserID;
                        output = Database.insertData(query);
                        if (output == "200")
                        {
                            // update balance from UserWallet table
                            query = "";
                            query = "Update UserWallet set Balance = Balance + " + Amount + " Where WalletID = " + WalletID + " and UserID = " + UserID + "";
                            output = Database.insertData(query);
                            returnMessage = "Wallet request has been approved successfully !!";
                        }
                        else
                            returnMessage = "An error occured please try again !!";
                    }
                    else
                    {
                        // Update record from UserWalletHistory table
                        // decimal newBalance = existingBalance + Amount;

                        query = "";
                        query = "Update UserWalletHistory Set Status = 'Approved', Balance = Balance + " + Amount + ", ManageBy = 'Admin' Where HistoryID = " + HistoryID + " and WalletID = " + WalletID + " and UserID = " + UserID + " ";
                        output = Database.insertData(query);
                        if (output == "200")
                        {
                            // update balance from UserWallet table
                            query = "";
                            query = "Update UserWallet set Balance = Balance + " + Amount + ", IsDisplay = 1 Where WalletID = " + WalletID + " and UserID = " + UserID + "";
                            output = Database.insertData(query);
                            returnMessage = "Wallet request has been approved successfully !!";
                        }
                        else
                            returnMessage = "An error occured please try again !!";
                    }
                }
                else
                {
                    // Delete record from UserWalletHistory table
                    query = "";
                    query = "Update UserWalletHistory Set Status = 'Rejected', ManageBy = 'Admin' Where HistoryID = " + HistoryID + " and WalletID = " + WalletID + " and UserID = " + UserID + "";
                    output = Database.insertData(query);
                    var is_Display = Database.getValue("select IsDisplay from UserWallet Where WalletID = " + WalletID + " and UserID = " + UserID + "");
                    bool isDisplay = Convert.ToBoolean(is_Display);
                    if (output == "200")
                    {
                        if (isDisplay == false)
                        {
                            // update balance from UserWallet table
                            query = "";
                            query = "Update UserWallet set Balance = Balance - " + Amount + ", IsDisplay = 1 Where WalletID = " + WalletID + " and UserID = " + UserID + "";
                            output = Database.insertData(query);
                        }
                        returnMessage = "Wallet request has been rejected successfully !!";
                    }
                    else
                        returnMessage = "An error occured please try again !!";
                }
            }
            catch (Exception ex)
            {
                returnMessage = "An error occured please try again !!";
                Console.WriteLine(ex.ToString());
            }
            return Json(returnMessage, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Login
        [HttpGet, Route("Login")]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string UserName, string Password)
        {
            try
            {
                //using (_db = new DBEntities())
                //{
                //    var userData = _db.tblUserMaster.Where(m => m.UserName == UserName && m.Password == Password && m.IsActive == true).FirstOrDefault();
                //    if (userData != null)
                //    {
                //        RoleUser roleUser = _db.ValidateUser(UserName, Password).FirstOrDefault();
                //        switch (roleUser.UserId.Value)
                //        {
                //            case -1:
                //                TempData["ErrorMessage"] = "Username or password is incorrect.";
                //                break;
                //            case -2:
                //                TempData["ErrorMessage"] = "User account has not been created.";
                //                break;
                //            default:
                //                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userData.DisplayName, DateTime.Now, DateTime.Now.AddMinutes(80000), true, roleUser.Roles, FormsAuthentication.FormsCookiePath);
                //                string hash = FormsAuthentication.Encrypt(ticket);
                //                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);

                //                if (ticket.IsPersistent)
                //                {
                //                    cookie.Expires = ticket.Expiration;
                //                }

                //                Response.Cookies["LoginUserID"].Value = userData.ID.ToString();
                //                Response.Cookies["LoginUserRole"].Value = roleUser.Roles.ToString();
                //                Response.Cookies["LoginUserName"].Value = userData.DisplayName;

                //                Response.Cookies.Add(cookie);
                //                if (roleUser.Roles == "SuperAdmin")
                //                {
                //                    return RedirectToAction("Dashboard", "Home");
                //                }
                //                else if (roleUser.Roles == "ServiceManager" || roleUser.Roles == "Supervisor")
                //                {
                //                    return RedirectToAction("DataSheetList", "DataSheet");
                //                }
                //                else if (roleUser.Roles == "QuotationHead")
                //                {
                //                    return RedirectToAction("QuotationDashboard", "Home");
                //                }
                //                else
                //                {
                //                    return RedirectToAction("Dashboard", "Home");
                //                }
                //        }
                return RedirectToAction("Index", "Home");
                //}
                //else
                //{
                //    TempData["ErrorMessage"] = "User account has not been created.";
                //    return RedirectToAction("Login", "Account");
                //}
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "An error occured please try again !!";
                Console.WriteLine(e.Message);
                return View();
            }
        }

        #endregion

        #region LogOut
        #endregion
        public ActionResult Logout()
        {
            Session.Abandon();
            Session.Clear();
            FormsAuthentication.SignOut();
            TempData["ErrorMessage"] = "";
            return RedirectToAction("Login");
        }

        #endregion

    }
}
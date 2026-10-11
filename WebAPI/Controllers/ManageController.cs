using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using WebAPI.Helpers;
//using WebAPI.OutputCache.V2;
using WebAPI.Models;
using System.Text.RegularExpressions;


namespace WebAPI.Controllers
{
    [RoutePrefix("api/Manage")]
    public class ManageController : ApiController 
    {
       

        public static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), INDIAN_ZONE);

        #region Global Variable

        public string CreateToken()
        {
            try
            {
                byte[] time = BitConverter.GetBytes(DateTime.Now.AddDays(1).ToBinary());
                byte[] key = Guid.NewGuid().ToByteArray();
                string token = Convert.ToBase64String(time.Concat(key).ToArray());
                Console.WriteLine(token);
                token = "FAdmin" + token.Substring(0, 7) + "garbage" + token.Substring(7);
                byte[] asciiBytes = Encoding.ASCII.GetBytes(token);
                for (int i = 0; i < asciiBytes.Length; i++)
                {
                    ++asciiBytes[i];
                    ++asciiBytes[i];
                }
                var cookie = System.Text.Encoding.Default.GetString(asciiBytes);

                return cookie;
            }
            catch
            {
                return "201";
            }
        }

        public string getToken()
        {
            try
            {
                string sessionId = "";

                CookieHeaderValue Token = Request.Headers.GetCookies("admintoken").FirstOrDefault();
                if (Token != null)
                {
                    sessionId = Token["admintoken"].Value;
                    byte[] asciiBytes = Encoding.ASCII.GetBytes(sessionId);
                    for (int i = 0; i < asciiBytes.Length; i++)
                    {
                        --asciiBytes[i];
                        --asciiBytes[i];
                    }
                    string str = System.Text.Encoding.Default.GetString(asciiBytes);
                    return str;
                }
                else
                {
                    return "201";
                }
            }
            catch
            {
                return "201";
            }
        }

        public string ValidateToken()
        {
            return "200";

            #region token response

            //try
            //{
            //    string sessionId = "";

            //    CookieHeaderValue Token = Request.Headers.GetCookies("admintoken").FirstOrDefault();
            //    if (Token != null)
            //    {
            //        sessionId = Token["tnpatoken"].Value;
            //        byte[] asciiBytes = Encoding.ASCII.GetBytes(sessionId);
            //        for (int i = 0; i < asciiBytes.Length; i++)
            //        {
            //            --asciiBytes[i];
            //            --asciiBytes[i];
            //        }
            //        string str = System.Text.Encoding.Default.GetString(asciiBytes);
            //        string token = str.Substring(8, 7) + str.Substring(22);
            //        Console.WriteLine(token);
            //        byte[] data = Convert.FromBase64String(token);
            //        DateTime time = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            //        if (time < DateTime.Now)
            //        {
            //            return "201";
            //        }
            //        else
            //        {
            //            return "200";
            //        }
            //    }
            //    else
            //    {
            //        return "201";
            //    }
            //}
            //catch
            //{
            //    return "201";
            //}

            #endregion
        }

        #endregion

        #region Registration

        [HttpPost]
        public HttpResponseMessage AddUpdateUser(RegisterUser data)
        {
            string validate = ValidateToken();
            if (validate == "201")
            {
                return Return.returnHttp("215", "Invalid Token or Token Expired.", getToken());
            }

            try
            {
                int UserID = data.UserID, RoleID = data.RoleID; string fcmtoken = data.fcmtoken; ;
                string Name = data.Name, MobileNo = data.MobileNo, Email = data.Email, UserName = data.Name, Password = data.Password;

                if (data.Pincode !="")
                {
                  var pincodeExist =  Database.getValue("select LocationCode from PinMaster WHERE LocationCode = " + data.Pincode + " and  IsActive = 1");


                    if (string.IsNullOrEmpty(pincodeExist))

                    {
                        return Return.returnHttp("201", "Pincode not Exist.", CreateToken());
                    }
                }

                if (RoleID == 0 || string.IsNullOrEmpty(Name)  || string.IsNullOrEmpty(""))
                {
                    return Return.returnHttp("201", "Invalid/Incomplete details. Please try again.", CreateToken());
                }

                var isUserExist = Database.getValue("SELECT UserID FROM tbl_Register_User WHERE IsActive = 1 and RoleID = 4 and ((MobileNo = '" + data.MobileNo + "') OR (Email = '" + data.Email + "') OR (Username = '" + data.UserName + "')) and userID <> " + data.UserID);

                if (!string.IsNullOrEmpty(isUserExist))
                {
                    return Return.returnHttp("201", "Entered mobile no or email or username is already exists.", CreateToken());
                }

                string query = "";

                bool hasEmail = !string.IsNullOrEmpty(Email);
                bool hasPincode = !string.IsNullOrEmpty(data.Pincode);

                if (UserID == 0)
                {
                    if (hasEmail)
                    {
                        query = "INSERT INTO tbl_Register_User " +
                                "VALUES (" + RoleID + ", '" + Name + "', '" + UserName + "', '" + Password + "', '"
                                + MobileNo + "', '" + Email + "', 1, NULL, '" + datetime + "', NULL, '"
                                + data.Address + "', '" + data.fcmtoken + "', '', "
                                + (hasPincode ? "'" + data.Pincode + "'" : "NULL") + ")";
                    }
                    else
                    {
                        query = "INSERT INTO tbl_Register_User " +
                                "VALUES (" + RoleID + ", '" + Name + "', '" + UserName + "', '" + Password + "', '"
                                + MobileNo + "', NULL, 1, NULL, '" + datetime + "', NULL, '"
                                + data.Address + "', '" + data.fcmtoken + "', '', "
                                + (hasPincode ? "'" + data.Pincode + "'" : "NULL") + ")";
                    }
                }
                else
                {
                    query = "UPDATE tbl_Register_User SET " +
                            "Name = '" + Name + "', " +
                            "UserName = '" + UserName + "', " +
                            "MobileNo = '" + MobileNo + "', " +
                            "Address = '" + data.Address + "', " +
                            "UpdatedDate = '" + datetime + "', " +
                            "Pincode = " + (hasPincode ? "'" + data.Pincode + "'" : "NULL");

                    if (hasEmail)
                    {
                        query += ", Email = '" + Email + "'";
                    }

                    query += " WHERE userID = " + UserID;
                }


                string output = Database.insertData(query);

                if (output != "200")
                {
                    return Return.returnHttp("201", "Some Internal Issue Occured while entering your data. Please try again." + output, CreateToken());
                }
                if (UserID == 0)
                {
                    return Return.returnHttp("200", "User Added Successfully", CreateToken());
                }
                else
                {
                    return Return.returnHttp("200", "User details updated Successfully", CreateToken());
                }
            }
            catch (Exception e)
            {
                return Return.returnHttp("201", "Some Internal Issue Occured.Please try again." + e.Message + e.StackTrace, CreateToken());
            }
        }

        #endregion

        #region Get User Details

        [HttpGet]
        public HttpResponseMessage GetUserDetails(int UserID)
        {
            try
            {
                RegisterUser obj = new RegisterUser();

                if (UserID == 0)
                {
                    return Return.returnHttp("201", "Invalid user. Please try again.", "");
                }

                DataSet ds = Database.getDataSet("select * from tbl_Register_User where IsActive = 1 and UserID =" + UserID);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    return Return.returnHttp("201", "User details not found. Please try again", "");
                }

                obj.UserID = (dt.Rows[0][0] == null) ? 0 : Convert.ToInt32(dt.Rows[0][0]);
                obj.RoleID = (dt.Rows[0][1] == null) ? 0 : Convert.ToInt32(dt.Rows[0][1]);
                obj.Name = dt.Rows[0][2].ToString();
                obj.UserName = dt.Rows[0][3].ToString();
                obj.Password = dt.Rows[0][4].ToString();
                obj.MobileNo = dt.Rows[0][5].ToString();
                obj.Email = dt.Rows[0][6].ToString();
                obj.IsActive = Convert.ToBoolean(dt.Rows[0][7]);
                obj.Address = dt.Rows[0][11].ToString();
                var WalletID = Database.getValue("SELECT WalletID FROM UserWallet WHERE UserID = " + obj.UserID + "");
                if (!string.IsNullOrEmpty(WalletID))
                {
                    obj.WalletID = Convert.ToInt32(WalletID);
                }
                var WalletBalance = Database.getValue("SELECT Balance FROM UserWallet WHERE IsDisplay = 1 and UserID = " + obj.UserID + "");
                if (!string.IsNullOrEmpty(WalletBalance))
                {
                    obj.WalletBalance = Convert.ToDecimal(WalletBalance);
                }
                return Return.returnHttp("200", obj, CreateToken());

            }
            catch (Exception e)
            {
                return Return.returnHttp("201", "Some Internal Issue Occured. Please try again." + e.Message + e.StackTrace, "");
            }
        }

        #endregion

        #region Login

        [HttpPost]
        public HttpResponseMessage login(RegisterUser data)
        {
            try
            {
                RegisterUser obj = new RegisterUser();

                string UserName = data.UserName, Password = data.Password ,fcmtoken=data.fcmtoken, version = data.Version;

                if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
                {
                    return Return.returnHttp("201", "InComplete Login details. Please try again.", "");
                }

                DataSet ds = Database.getDataSet("select * from tbl_Register_User where MobileNo='" + UserName + "' and Password='" + Password + "' and IsActive = 1 and RoleID = 4");
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    return Return.returnHttp("201", "Incorrect username or password. Please try again", "");
                }
                if (Password == dt.Rows[0][4].ToString())
                {
                    string Updatetoken = "update tbl_Register_User set fcmtoken = '" + fcmtoken + "' , Version='"+ version + "' where userID = " + dt.Rows[0][0];
                    string output = Database.insertData(Updatetoken);

                    obj.UserID = (dt.Rows[0][0] == null) ? 0 : Convert.ToInt32(dt.Rows[0][0]);
                    obj.RoleID = (dt.Rows[0][1] == null) ? 0 : Convert.ToInt32(dt.Rows[0][1]);
                    obj.Name = dt.Rows[0][2].ToString();
                    obj.UserName = dt.Rows[0][3].ToString();
                    obj.Password = dt.Rows[0][4].ToString();
                    obj.MobileNo = dt.Rows[0][5].ToString();
                    obj.Email = dt.Rows[0][6].ToString();
                    obj.IsActive = Convert.ToBoolean(dt.Rows[0][7]);
                    obj.Address = dt.Rows[0][11].ToString();
                    obj.fcmtoken = dt.Rows[0][12].ToString();

                    var WalletID = Database.getValue("SELECT WalletID FROM UserWallet WHERE UserID = " + obj.UserID + "");
                    if (!string.IsNullOrEmpty(WalletID))
                    {
                        obj.WalletID = Convert.ToInt32(WalletID);
                    }
                    var WalletBalance = Database.getValue("SELECT Balance FROM UserWallet WHERE UserID = " + obj.UserID + "");
                    if (!string.IsNullOrEmpty(WalletBalance))
                    {
                        obj.WalletBalance = Convert.ToDecimal(WalletBalance);
                    }
                    return Return.returnHttp("200", obj, CreateToken());
                }
                else
                {
                    return Return.returnHttp("201", "Incorrect Password. Please try again", "");
                }
            }
            catch (Exception e)
            {
                return Return.returnHttp("201", "Some Internal Issue Occured. Please try again." + e.Message + e.StackTrace, "");
            }
        }

        #endregion

        #region Change Password

        [HttpPost]
        public HttpResponseMessage ChangePassword(RegisterUser data)
        {
            string validate = ValidateToken();
            if (validate == "201")
            {
                return Return.returnHttp("215", "Invalid Token or Token Expired.", getToken());
            }

            try
            {
                int UserID = data.UserID;
                string newPassword = data.Password;

                if (UserID == 0 || newPassword == null || newPassword == "")
                {
                    return Return.returnHttp("201", "Invalid/Incomplete details. Please try again.", CreateToken());
                }

                string output = Database.insertData("update tbl_Register_User set Password='" + newPassword + "', PasswordChangDate='" + datetime + "' where UserID=" + UserID);
                if (output != "200")
                {
                    return Return.returnHttp("201", "Some Internal Issue Occured while entering your data. Please try again.", CreateToken());
                }

                return Return.returnHttp("200", "Password Changed successfully.", CreateToken());

            }
            catch (Exception e)
            {
                return Return.returnHttp("201", "Some Internal Issue Occured. Please try again." + e.Message + e.StackTrace, CreateToken());
            }
        }

        #endregion

        #region Stock Item List

        [HttpGet]
       //[ResponseCache(VaryByHeader = "User-Agent", Duration = 30)]

       // [CacheFilter(TimeDuration = 1000)]
        // [System.Web.Mvc.OutputCache(Duration = 10, VaryByParam = "none")]
        public HttpResponseMessage GetStockItemList()
        {
            string validate = ValidateToken();
            if (validate == "201")
            {
                return Return.returnHttp("215", "Invalid Token or Token Expired.", getToken());
            }

            try
            {
                string strTop = "";
                string query = "SELECT t1.ItemID,t1.ItemName,t1.CategoryID,t1.ImagePath,t1.CreatedDate,t1.UpdatedDate FROM ItemMaster t1 INNER JOIN CategoryMaster t2 ON t1.CategoryID = t2.CategoryID WHERE t2.IsActive = '1' Order by t1.CategoryID";

                List<Item> ItemList = new List<Item>();

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    ItemList = new List<Item>();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    Item itemData = new Item();

                    itemData.ItemID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    itemData.ItemName = dt.Rows[n][1].ToString();
                    itemData.CategoryID = ((dt.Rows[n][2] == null) ? 0 : Convert.ToInt32(dt.Rows[n][2]));
                    itemData.CategoryName = Database.getValue("select " + strTop + " CategoryName from CategoryMaster Where CategoryID=" + itemData.CategoryID);
                    string ItemImage = dt.Rows[n][3].ToString();
                    if (!string.IsNullOrEmpty(ItemImage))
                    {
                        itemData.ImagePath = "http://admin.freshfoodz.in.net/Content/ItemImages/" + ItemImage;
                    }

                    #region Sub Item

                    itemData._subitemList = new List<SubItem>();

                    string itemQuery = "(SELECT " + strTop + " * FROM ITEMSTOCK WHERE ITEMID = " + itemData.ItemID + " and InStock = 1)";

                    DataSet item_ds = Database.getDataSet(itemQuery);
                    DataTable item_dt = new DataTable();
                    if (item_ds!=null && item_ds.Tables.Count>0)
                    {
                        item_dt = item_ds.Tables[0];
                        if (item_ds == null || item_dt.Rows.Count == 0)
                        {
                            itemData._subitemList = new List<SubItem>();
                        }
                    }
                    int i = 0;
                    int item_count = item_dt.Rows.Count;

                    while (i < item_count)
                    {
                        SubItem subItemData = new SubItem();

                        subItemData.StockID = Convert.ToInt32(item_dt.Rows[i][0]);
                        subItemData.MainID = Convert.ToInt32(item_dt.Rows[i][1]);
                        subItemData.ItemID = Convert.ToInt32(item_dt.Rows[i][2]);
                        subItemData.ItemName = Database.getValue("select" + strTop + " ItemName from ITEMMASTER Where ItemID=" + subItemData.ItemID);
                        subItemData.SubItemID = Convert.ToInt32(item_dt.Rows[i][3]);
                        subItemData.Weight = item_dt.Rows[i][4].ToString();
                        subItemData.Price = Convert.ToInt32(item_dt.Rows[i][5]);

                        itemData._subitemList.Add(subItemData);

                        i++;
                    }

                    #endregion

                    if (item_count != 0)
                    {
                        ItemList.Add(itemData);
                    }
                    n++;
                }

                if (ItemList.Count > 0)
                {
                    ItemList = ItemList.OrderBy(a => a.ItemName).ToList();
                }
                return Return.returnHttp("200", ItemList, CreateToken());

            }
            catch (Exception e)
            {
                return Return.returnHttp("201", "Some Internal Issue Occured. Please try again." + e.Message + e.StackTrace, CreateToken());
            }
        }

        #endregion

        #region PlaceOrder

        [HttpPost]
        public HttpResponseMessage PlaceOrder(Order_Model data)
        {
            string validate = ValidateToken();
            if (validate == "201")
            {
                return Return.returnHttp("215", "Invalid Token or Token Expired.", getToken());
            }

            try
            {
                if (data.UserID == 0 || string.IsNullOrEmpty(data.CustomerName) || string.IsNullOrEmpty(data.CustomerAddress) || data.NoofItem < 0 || data._itemList.Count == 0)
                {
                    return Return.returnHttp("201", "Invalid/Incomplete details. Please try again.", CreateToken());
                }

                DateTime orderDate;

                if (!string.IsNullOrEmpty(data.OrderDate))
                {
                    DateTime actionDate = DateTime.ParseExact(data.OrderDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    orderDate = actionDate;
                }
                else
                {
                    orderDate = datetime;
                }

                string query = "";
                var orderNo = CommonFunction.GenerateOrderNo();

                if (data.NoofItem == 0 && data._itemList.Count > 0)
                {
                    data.NoofItem = data._itemList.Count;
                }

                if ((data.Total == 0 || data.Total == null) && data._itemList.Count > 0)
                {
                    data.Total = (from s in data._itemList select (s.Price * s.OrderQty)).Sum();
                }

                if (string.IsNullOrEmpty(data.CustomerName))
                {
                    data.CustomerName = Database.getValue("select  name from tbl_Register_User WHERE UserID = " + data.UserID + "");
                }

                if (string.IsNullOrEmpty(data.CustomerMobile))
                {
                    data.CustomerMobile = Database.getValue("select  MobileNo from tbl_Register_User WHERE UserID = " + data.UserID + "");
                }

                if (string.IsNullOrEmpty(data.CustomerEmail))
                {
                    data.CustomerEmail = Database.getValue("select  Email from tbl_Register_User WHERE UserID = " + data.UserID + "");
                }

                if (string.IsNullOrEmpty(data.DeliveryAddress))
                {
                    data.DeliveryAddress = Database.getValue("select Address from tbl_Register_User WHERE UserID = " + data.UserID + "");
                }

                data.CustomerAddress = data.DeliveryAddress;

                if (data.RangeID != 0)
                {
                    data.TimeRange = Database.getValue("select Range from [DeliveryTimeRange] WHERE RangeID = " + data.RangeID + "");
                }

                if (data.OrderID >= 0)
                {
                    query = "INSERT INTO ORDERMASTER VALUES('" + orderNo + "', '" + orderDate + "'," + data.UserID + ", '" + data.CustomerName + "', '" + data.CustomerMobile + "', '" + data.CustomerEmail + "', '" + data.CustomerAddress + "', '" + data.DeliveryAddress + "', " + data.NoofItem + ", 'Accepted', null, '" + data.CustomerRemark + "', null, " + data.Total + ", '" + datetime + "', null, null, null, null, null, '" + data.TimeRange + "', null)";
                }

                string output = Database.insertData(query);

                if (output != "200")
                {
                    return Return.returnHttp("201", "Some Internal Issue Occured while entering your data. Please try again." + output, CreateToken());
                }
                else
                {
                    #region Manage Item

                    var OrderID = Database.getValue("SELECT ORDERID FROM ORDERMASTER WHERE ORDERNO = '" + orderNo + "'");

                    foreach (var item in data._itemList)
                    {
                        if (string.IsNullOrEmpty(item.ItemName))
                        {
                            item.ItemName = Database.getValue("SELECT ITEMNAME FROM ITEMMASTER WHERE ITEMID = " + item.ItemID);
                        }
                        item.TotalAmount = item.OrderQty * item.Price;

                        if (item.OrderQty > 1)
                        {
                            int QtyCount = Convert.ToInt32(item.OrderQty);
                            for (int i = 0; i < QtyCount; i++)
                            {
                                decimal Qty = 1;
                                query = "INSERT INTO ORDER_ITEMS VALUES(" + OrderID + ", '" + orderNo + "', " + item.ItemID + ", '" + item.ItemName + "'," + item.SubItemID + ", " + item.StockID + ", " + item.MainID + ", '" + item.Weight + "', " + Qty + ", " + item.Price + ", " + item.TotalAmount + ", '" + datetime + "', null, null)";
                                output = Database.insertData(query);
                            }
                        }
                        else
                        {
                            query = "INSERT INTO ORDER_ITEMS VALUES(" + OrderID + ", '" + orderNo + "', " + item.ItemID + ", '" + item.ItemName + "'," + item.SubItemID + ", " + item.StockID + ", " + item.MainID + ", '" + item.Weight + "', " + item.OrderQty + ", " + item.Price + ", " + item.TotalAmount + ", '" + datetime + "', null, null)";
                            output = Database.insertData(query);
                        }
                    }

                    #endregion

                    #region Sent Mail 
/*
                    var emailBody = Accepted_OrderSummry_EmailBody(data.CustomerName, orderNo);
                    string response = CommonFunction.SendMail(data.CustomerEmail, emailBody, "Your FreshFoodz order no : " + orderNo + " has been accepted");

                    if (response == "Sucess")
                    {
                        string orderQuery = "insert into [OrderMailStatus] values(" + OrderID + ", '" + orderNo + "', 1,1,0,0,0,'" + datetime + "')";
                        string orderOutput = Database.insertData(orderQuery);
                    }*/

                    #endregion

                    return Return.returnHttp("200", "Order placed successfully", CreateToken());
                }
            }
            catch (Exception e)
            {
                return Return.returnHttp("201", "Some Internal Issue Occured.Please try again." + e.Message + e.StackTrace, CreateToken());
            }
        }

        private string OrderSummry_EmailBody(string CustomerName, string OrderNo)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Shared/OrderSummary.cshtml")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{CName}", CustomerName);
            body = body.Replace("{OrderNo}", OrderNo);
            return body;
        }
        
        private string Accepted_OrderSummry_EmailBody(string CustomerName, string OrderNo)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Shared/Accepted_OrderSummary.cshtml")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{CName}", CustomerName);
            body = body.Replace("{OrderNo}", OrderNo);
            return body;
        }

        #endregion

        #region Get OrderDetails

        [HttpGet]
        public HttpResponseMessage GetOrderDetails(string OrderNo = "", int OrderID = 0)
        {
            string validate = ValidateToken();
            if (validate == "201")
            {
                return Return.returnHttp("215", "Invalid Token or Token Expired.", getToken());
            }

            try
            {
                string query = "SELECT * FROM ORDERMASTER WHERE (ORDERID = " + OrderID + ") OR (ORDERNO = '" + OrderNo + "')";

                Order_Model orderDetails = new Order_Model();

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    orderDetails = new Order_Model();
                }

                int n = 0;
                int count = dt.Rows.Count;

                #region Order Data

                while (n < count)
                {
                    orderDetails = new Order_Model();

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

                    if (!string.IsNullOrEmpty(dt.Rows[n][20].ToString()))
                    {
                        DateTime DeliveryDate = Convert.ToDateTime(dt.Rows[n][20].ToString());
                        orderDetails.DeliveryDate = DeliveryDate.ToShortDateString();
                    }
                    orderDetails.DeliveryTime = dt.Rows[n][21].ToString();

                    if (!string.IsNullOrEmpty(dt.Rows[n][22].ToString()))
                    {
                        orderDetails.BillingAmount = Convert.ToDecimal(dt.Rows[n][22]);
                    }

                    #region Item List

                    orderDetails._itemList = new List<Order_Item>();

                    string itemQuery = "SELECT * FROM ORDER_ITEMS WHERE ORDERID =" + orderDetails.OrderID;

                    DataSet item_ds = Database.getDataSet(itemQuery);
                    DataTable item_dt = item_ds.Tables[0];
                    if (item_ds == null || item_dt.Rows.Count == 0)
                    {
                        orderDetails._itemList = new List<Order_Item>();
                    }

                    int i = 0;
                    int item_count = item_dt.Rows.Count;

                    #region Item List

                    while (i < item_count)
                    {
                        Order_Item itemData = new Order_Item();

                        itemData.OrderItemID = Convert.ToInt32(item_dt.Rows[i][0]);
                        itemData.OrderID = Convert.ToInt32(item_dt.Rows[i][1]);
                        itemData.OrderNo = item_dt.Rows[i][2].ToString();

                        itemData.ItemID = Convert.ToInt32(item_dt.Rows[i][3]);
                        itemData.ItemName = item_dt.Rows[i][4].ToString();

                        itemData.SubItemID = Convert.ToInt32(item_dt.Rows[i][5]);
                        itemData.StockID = Convert.ToInt32(item_dt.Rows[i][6]);
                        itemData.MainID = Convert.ToInt32(item_dt.Rows[i][7]);

                        itemData.Weight = item_dt.Rows[i][8].ToString();
                        itemData.OrderQty = Convert.ToDecimal(item_dt.Rows[i][9]);

                        itemData.Price = Convert.ToDecimal(item_dt.Rows[i][10]);
                        itemData.TotalAmount = Convert.ToDecimal(item_dt.Rows[i][11]);

                        if (!string.IsNullOrEmpty(item_dt.Rows[i][13].ToString()))
                        {
                            itemData.DeliveryWeight = Convert.ToDecimal(item_dt.Rows[i][13]);
                        }

                        if (!string.IsNullOrEmpty(item_dt.Rows[i][14].ToString()))
                        {
                            itemData.DeliveryPrice = Convert.ToDecimal(item_dt.Rows[i][14]);
                        }
                        else
                        {
                            itemData.DeliveryPrice = Convert.ToDecimal(item_dt.Rows[i][10]);
                        }

                        orderDetails._itemList.Add(itemData);

                        i++;
                    }

                    #endregion

                    #endregion

                    string InvoiceFile = OrderSummary.GenerateOrderSummary(orderDetails);
                    if (InvoiceFile != "Error.pdf")
                    {
                        orderDetails.Invoice = "http://api.freshfoodz.in.net/Content/OrderSummary/" + InvoiceFile;
                    }

                    n++;
                }

                #endregion

                return Return.returnHttp("200", orderDetails, CreateToken());

            }
            catch (Exception e)
            {
                return Return.returnHttp("201", "Some Internal Issue Occured. Please try again." + e.Message + e.StackTrace, CreateToken());
            }
        }

        #endregion

        #region Get User wise Order List

        [HttpGet]
        public HttpResponseMessage GetUserOrderList(int UserID = 0)
        {
            string validate = ValidateToken();
            if (validate == "201")
            {
                return Return.returnHttp("215", "Invalid Token or Token Expired.", getToken());
            }

            try
            {
                string query = "SELECT * FROM ORDERMASTER WHERE UserID = " + UserID + "Order by 1 desc";

                List<Order_Model> orderList = new List<Order_Model>();

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    orderList = new List<Order_Model>();
                }

                int n = 0;
                int count = dt.Rows.Count;

                #region Purchase Data

                while (n < count)
                {
                    Order_Model orderDetails = new Order_Model();

                    orderDetails.OrderID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    orderDetails.OrderNo = ((dt.Rows[n][0] == null) ? 0 : dt.Rows[n][0]).ToString();
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
                    if (orderDetails.OrderStatus == "Accepted" || orderDetails.OrderStatus=="Cancelled by Admin" || orderDetails.OrderStatus == "Cancelled by user")
                    {
                        orderDetails.Total = Convert.ToDecimal(dt.Rows[n][14]);
                    }
                    else
                    {
                        orderDetails.Total = Convert.ToDecimal(dt.Rows[n][22]);
                    }
                    orderDetails.SysDeliveryDate = dt.Rows[n][17].ToString();


                    if (!string.IsNullOrEmpty(dt.Rows[n][18].ToString()))
                    {
                        orderDetails.PaidAmount = Convert.ToDecimal(dt.Rows[n][18]);
                    }

                    orderDetails.PaymentDate = dt.Rows[n][19].ToString();

                    if (!string.IsNullOrEmpty(dt.Rows[n][20].ToString()))
                    {
                        DateTime DeliveryDate = Convert.ToDateTime(dt.Rows[n][20].ToString());
                        orderDetails.DeliveryDate = DeliveryDate.ToShortDateString();
                    }
                    orderDetails.DeliveryTime = dt.Rows[n][21].ToString();

                    if (!string.IsNullOrEmpty(dt.Rows[n][22].ToString()))
                    {
                        orderDetails.BillingAmount = Convert.ToDecimal(dt.Rows[n][22]);
                    }

                    #region Item List

                    orderDetails._itemList = new List<Order_Item>();

                    string itemQuery = "SELECT * FROM ORDER_ITEMS WHERE ORDERID =" + orderDetails.OrderID;

                    DataSet item_ds = Database.getDataSet(itemQuery);
                    DataTable item_dt = item_ds.Tables[0];
                    if (item_ds == null || item_dt.Rows.Count == 0)
                    {
                        orderDetails._itemList = new List<Order_Item>();
                    }

                    int i = 0;
                    int item_count = item_dt.Rows.Count;
                
                    
                    #region Item List

                    while (i < item_count)
                    {
                        Order_Item itemData = new Order_Item();
                        
                        itemData.OrderItemID = Convert.ToInt32(item_dt.Rows[i][0]);
                        itemData.OrderID = Convert.ToInt32(item_dt.Rows[i][1]);
                        itemData.OrderNo = item_dt.Rows[i][2].ToString();

                        itemData.ItemID = Convert.ToInt32(item_dt.Rows[i][3]);
                        itemData.ItemName = item_dt.Rows[i][4].ToString();
                        string itemimg = "SELECT ImagePath FROM ItemMaster WHERE ItemID =" + itemData.ItemID;

                        string ItemImage = Database.getValue(itemimg);
                        if (!string.IsNullOrEmpty(ItemImage))
                        {
                            itemData.ImagePath = "http://admin.freshfoodz.in.net/Content/ItemImages/" + ItemImage;
                        }
                        itemData.SubItemID = Convert.ToInt32(item_dt.Rows[i][5]);
                        itemData.StockID = Convert.ToInt32(item_dt.Rows[i][6]);
                        itemData.MainID = Convert.ToInt32(item_dt.Rows[i][7]);


                      
                        itemData.OrderQty = Convert.ToDecimal(item_dt.Rows[i][9]);
                        string mystr = Regex.Replace(item_dt.Rows[i][8].ToString(), @"\d", "");
                        if (orderDetails.OrderStatus == "Accepted" || orderDetails.OrderStatus == "Cancelled by Admin" || orderDetails.OrderStatus == "Cancelled by user")
                        {
                            itemData.Weight = item_dt.Rows[i][8].ToString();
                            itemData.Price = Convert.ToDecimal(item_dt.Rows[i][10]);
                        }
                        else
                        {
                            itemData.Weight = item_dt.Rows[i][13].ToString() +""+mystr;
                            itemData.Price = Convert.ToDecimal(item_dt.Rows[i][14]);
                        }
                        

                       

                        itemData.TotalAmount = Convert.ToDecimal(item_dt.Rows[i][11]);

                        if (!string.IsNullOrEmpty(item_dt.Rows[i][13].ToString()))
                        {
                            itemData.DeliveryWeight = Convert.ToDecimal(item_dt.Rows[i][13]);
                        }

                        if (!string.IsNullOrEmpty(item_dt.Rows[i][14].ToString()))
                        {
                            itemData.DeliveryPrice = Convert.ToDecimal(item_dt.Rows[i][14]);
                        }
                        else
                        {
                            itemData.DeliveryPrice = Convert.ToDecimal(item_dt.Rows[i][10]);
                        }

                        orderDetails._itemList.Add(itemData);

                        i++;
                    }

                    #endregion

                    #endregion

                    string InvoiceFile = OrderSummary.GenerateOrderSummary(orderDetails);
                    if (InvoiceFile != "Error.pdf")
                    {
                        orderDetails.Invoice = "http://api.freshfoodz.in.net/Content/OrderSummary/" + InvoiceFile;
                    }

                    orderList.Add(orderDetails);

                    n++;
                }

                #endregion

                return Return.returnHttp("200", orderList, CreateToken());

            }
            catch (Exception e)
            {
                return Return.returnHttp("201", "Some Internal Issue Occured. Please try again." + e.Message + e.StackTrace, CreateToken());
            }
        }

        #endregion

        #region Cancel Order by User

        [HttpGet]
        public HttpResponseMessage CancelOrder(int OrderID = 0, int UserID = 0)
        {
            string validate = ValidateToken();
            if (validate == "201")
            {
                return Return.returnHttp("215", "Invalid Token or Token Expired.", getToken());
            }

            try
            {
                string orderStatus = Database.getValue("SELECT OrderStatus FROM ORDERMASTER WHERE ORDERID = " + OrderID +"");


                string query = "";

                if (OrderID == 0)
                {
                    var Order_ID = Database.getValue("SELECT OrderID FROM ORDERMASTER WHERE ORDERID = " + OrderID + "");
                    OrderID = (string.IsNullOrEmpty(Order_ID)) ? 0 : Convert.ToInt32(Order_ID);
                }

                if (OrderID != 0)
                {
                    query = "Update ORDERMASTER SET OrderStatus = 'Cancelled by user', UpdatedDate = '" + datetime + "' Where ORDERID=" + OrderID;
                }

                string output = Database.insertData(query);

                if (output != "200")
                {
                    return Return.returnHttp("201", "Some Internal Issue Occured while entering your data. Please try again." + output, CreateToken());
                }
                else
                {
                    string Msg = "Your Order Id " + OrderID + " Successfully Cancelled.";
                    string notification_query = "insert into Tbl_Notifications values('" + UserID + "', 'Order Cancelled', '', '" + Msg + "','" + datetime + "','')";
                    string output1 = Database.insertData(notification_query);

                    string fcmtoken = Database.getValue("select  fcmtoken from tbl_Register_User WHERE UserID = " + UserID + "");
                    if (fcmtoken != null && fcmtoken != "")
                    {
                       
                        string data1 = CommonHelper.SendNotification(fcmtoken, Msg, "Order Cancelled", "");
                    }
                    return Return.returnHttp("200", "Your order cancelled.", CreateToken());
                }
            }
            catch (Exception e)
            {
                return Return.returnHttp("201", "Some Internal Issue Occured.Please try again." + e.Message + e.StackTrace, CreateToken());
            }
        }

        #endregion

        #region Delivery Time Range

        [HttpGet]
        public HttpResponseMessage GetDeliveryTimeRange()
        {

            string validate = ValidateToken();
            if (validate == "201")
            {
                return Return.returnHttp("215", "Invalid Token or Token Expired.", getToken());
            }

            try
            {
                string query = "SELECT * FROM DELIVERYTIMERANGE ORDER BY RANGE";

                List<DeliveryTimeRange_Model> rangeList = new List<DeliveryTimeRange_Model>();

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    rangeList = new List<DeliveryTimeRange_Model>();
                }

                int n = 0;
                int count = dt.Rows.Count;

                #region Time Range Data

                while (n < count)
                {
                    DeliveryTimeRange_Model range = new DeliveryTimeRange_Model();

                    range.RangeID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    range.Range = dt.Rows[n][1].ToString();

                    rangeList.Add(range);

                    n++;
                }

                #endregion

                return Return.returnHttp("200", rangeList, CreateToken());

            }
            catch (Exception e)
            {
                return Return.returnHttp("201", "Some Internal Issue Occured. Please try again." + e.Message + e.StackTrace, CreateToken());
            }
        }

        #endregion

        #region Get HelpLine No + CategoryList --> Start up API

        [HttpGet]
        public HttpResponseMessage StartUp()
        {
            string validate = ValidateToken();
            if (validate == "201")
            {
                return Return.returnHttp("215", "Invalid Token or Token Expired.", getToken());
            }

            try
            {
                Startup_Model data = new Startup_Model();

                data.HelpLineNo = Database.getValue("SELECT tOP 1 MobileNo FROM [tblHelpLineNo]");
                data.Version = Database.getValue("SELECT tOP 1 Version FROM [tblVersion]"); 
                data.holidaytext = Database.getValue("SELECT tOP 1 Title FROM [tblHeading]");
                string query = "SELECT * FROM CATEGORYMASTER where IsActive=1 ORDER BY CategoryID";

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];

                if (ds != null || dt.Rows.Count == 0)
                {
                    data._categoryList = new List<Category>();
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Category category = new Category();

                    category.CategoryID = Convert.ToInt32(dt.Rows[i][0]);
                    category.CategoryName = dt.Rows[i][1].ToString();

                    data._categoryList.Add(category);
                }
                return Return.returnHttp("200", data, CreateToken());
            }
            catch (Exception e)
            {
                return Return.returnHttp("201", "Some Internal Issue Occured. Please try again." + e.Message + e.StackTrace, CreateToken());
            }
        }

        #endregion

        #region User Wallet

        [HttpPost]
        public HttpResponseMessage AddBalance(Wallet_Model data)
        {
            string validate = ValidateToken();
            if (validate == "201")
            {
                return Return.returnHttp("215", "Invalid Token or Token Expired.", getToken());
            }

            try
            {
                string query = "", output = "";

                if (data.UserID != 0 && data.WalletID == 0)
                {
                    query = "Insert into UserWallet Values(" + data.UserID + ", " + data.Amount + ", 0,'" + datetime + "', " + data.UserID + ")";
                    output = Database.insertData(query);

                    if (output == "200")
                    {
                        int WalletID = 0;
                        var Wallet_ID = Database.getValue("SELECT WalletID FROM UserWallet WHERE UserID = " + data.UserID + "");
                        WalletID = (!string.IsNullOrEmpty(Wallet_ID)) ? Convert.ToInt32(Wallet_ID) : 0;

                        if (WalletID != 0)
                        {
                            query = "Insert into UserWalletHistory Values(" + WalletID + ", " + data.UserID + ", 'Credit', 'Request to Credit Balance', " + data.Amount + ",  " + data.Amount + ", null, '" + datetime + "', null, 'User')";
                            output = Database.insertData(query);
                        }
                    }
                }
                else if (data.UserID != 0 && data.WalletID != 0)
                {
                    var perviousBalance = Database.getValue("SELECT Balance FROM UserWallet WHERE UserID = " + data.UserID + "");
                    query = "Insert into UserWalletHistory Values(" + data.WalletID + ", " + data.UserID + ", 'Credit', 'Request to Credit Balance', " + data.Amount + ",  " + perviousBalance + ",  null, '" + datetime + "', null, 'User')";
                    output = Database.insertData(query);
                }

                if (output != "200")
                {
                    return Return.returnHttp("201", "Some Internal Issue Occured while entering your data. Please try again." + output, CreateToken());
                }
                else
                {
                    return Return.returnHttp("200", "Your balance request sent sucessfully", CreateToken());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Return.returnHttp("201", "Some Internal Issue Occured.Please try again." + ex.Message + ex.StackTrace, CreateToken());
            }
        }

        #endregion

        #region User Wallet History

        [HttpGet]
        public HttpResponseMessage GetWalletHistory(int UserID = 0)
        {
            try
            {
                string validate = ValidateToken();
                if (validate == "201")
                {
                    return Return.returnHttp("215", "Invalid Token or Token Expired.", getToken());
                }

                Wallet_History_Model data = new Wallet_History_Model();
                data._historyList = new List<Wallet_History>();

                var Balance = Database.getValue("SELECT Balance FROM UserWallet WHERE  IsDisplay = 1 and UserID = " + UserID + "");
                var count = Database.getValue("SELECT Balance FROM UserWallet WHERE  IsDisplay = 1 and UserID = " + UserID + "");

                if (!string.IsNullOrEmpty(Balance))
                    data.Balance = Convert.ToDecimal(Balance);

                string query = "select HistoryID, WalletID, UserID, Type, Amount, OrderNo, EntryDate, Balance " +
                               " from [UserWalletHistory] " +
                               " Where  Status in ('Purchase','Approved') and UserID = " + UserID + "" +
                               " Order by 1 desc";

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];

                if (ds == null || dt.Rows.Count == 0)
                {
                    data._historyList = new List<Wallet_History>();
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Wallet_History history = new Wallet_History();

                    history.HistoryID = Convert.ToInt32(dt.Rows[i][0]);
                    history.WalletID = Convert.ToInt32(dt.Rows[i][1]);
                    history.UserID = Convert.ToInt32(dt.Rows[i][2]);
                    history.Type = dt.Rows[i][3].ToString();
                    history.Amount = Convert.ToDecimal(dt.Rows[i][4]);
                    history.OrderNo = dt.Rows[i][5].ToString();
                    history.EntryDate = dt.Rows[i][6].ToString();
                    history.Balance = Convert.ToDecimal(dt.Rows[i][7]);
                    data._historyList.Add(history);
                }

                return Return.returnHttp("200", data, CreateToken());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Return.returnHttp("201", "Some Internal Issue Occured.Please try again." + ex.Message + ex.StackTrace, CreateToken());
            }
        }

        #endregion
        #region Online Payment

        [HttpPost]
        public HttpResponseMessage OnlinePayment(Wallet_Model data)
        {
            Wallet_Model obj = new Wallet_Model();

            string validate = ValidateToken();
            if (validate == "201")
            {
                return Return.returnHttp("215", "Invalid Token or Token Expired.", getToken());
            }

            try
            {
                string query = "", output = "",  output1 = "";

                if (data.UserID != 0)
                {
                    query = "Insert into Tbl_OnlinePayment Values(" + data.TransactionId + ", " + data.UserID + ",'" + data.UserName + "','" + data.Amount + "','" + datetime + "','" + datetime + "','" + data.PaymentMode + "','" + data.PaymentStatus + "')";
                    output = Database.insertData(query);

                    if (output == "200" && data.PaymentStatus == "Success")
                    {
                        var Wallet_ID = Database.getValue("SELECT WalletID FROM UserWallet WHERE UserID = " + data.UserID + "");
                        data.WalletID = (!string.IsNullOrEmpty(Wallet_ID)) ? Convert.ToInt32(Wallet_ID) : 0;


                        if (data.UserID != 0 && data.WalletID == 0)
                        {
                            query = "Insert into UserWallet Values(" + data.UserID + ", " + data.Amount + ", 1,'" + datetime + "', " + data.UserID + ")";
                            output1 = Database.insertData(query);

                            if (output1 == "200")
                            {
                                Wallet_ID = Database.getValue("SELECT WalletID FROM UserWallet WHERE UserID = " + data.UserID + "");
                                data.WalletID = (!string.IsNullOrEmpty(Wallet_ID)) ? Convert.ToInt32(Wallet_ID) : 0;

                                if (data.WalletID != 0)
                                {
                                    query = "Insert into UserWalletHistory Values(" + data.WalletID + ", " + data.UserID + ", 'Credit', 'Approved', " + data.Amount + ",  " + data.Amount + ", null, '" + datetime + "', null, 'Razorpay','Online')";
                                    output1 = Database.insertData(query);
                                }
                            }
                        }
                        else if (data.UserID != 0 && data.WalletID != 0)
                        {
                            var perviousBalance = Database.getValue("SELECT Balance FROM UserWallet WHERE UserID = " + data.UserID + "");
                            query = "Insert into UserWalletHistory Values(" + data.WalletID + ", " + data.UserID + ", 'Credit', 'Approved', " + data.Amount + ",  " + perviousBalance + ",  null, '" + datetime + "', null, 'Razorpay','Online')";
                            output1 = Database.insertData(query);
                            if (output1 == "200")
                            {
                                var existing_Balance = Database.getValue("select Balance from UserWallet Where WalletID = " + data.WalletID + " and UserID = " + data.UserID + "");
                                decimal existingBalance = Convert.ToDecimal(existing_Balance);
                                decimal newBalance = existingBalance + data.Amount;

                                // update balance from UserWallet table
                                query = "";
                                query = "Update UserWallet set Balance = " + newBalance + " Where WalletID = " + data.WalletID + " and UserID = " + data.UserID + "";
                                output1 = Database.insertData(query);
                            }
                        }
                    }
                }
                if (output1 == "200")
                {
                    var newBalance1 = Database.getValue("SELECT Balance FROM UserWallet WHERE UserID = " + data.UserID + "");

                    if (!string.IsNullOrEmpty(newBalance1))
                    {
                        obj.Balance = Convert.ToDecimal(newBalance1);
                        obj.PaymentStatus = "Success";
                    }
                    return Return.returnHttp("200", obj, CreateToken());
                }
                else if (output == "200")
                {
                    var newBalance1 = Database.getValue("SELECT Balance FROM UserWallet WHERE UserID = " + data.UserID + "");

                    if (!string.IsNullOrEmpty(newBalance1))
                    {
                        obj.Balance = Convert.ToDecimal(newBalance1);
                        obj.PaymentStatus = "Fail";
                    }
                    return Return.returnHttp("200", obj, CreateToken());
                }
                else
                {
                    return Return.returnHttp("201", "Some Internal Issue Occured while entering your data. Please try again." + output, CreateToken());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Return.returnHttp("201", "Some Internal Issue Occured.Please try again." + ex.Message + ex.StackTrace, CreateToken());
            }
        }
        #endregion

        #region Notification List

        [HttpGet]
        public HttpResponseMessage GetNotificationList(int UserID=0)
        {

            string validate = ValidateToken();
            if (validate == "201")
            {
                return Return.returnHttp("215", "Invalid Token or Token Expired.", getToken());
            }

            try
            {
                
                string query = "SELECT TOP 10 * FROM Tbl_Notifications WHERE UserId LIKE '%" + UserID + "%' or  UserId='0' order by Id desc";

                List<Notification_Model> NotificationList = new List<Notification_Model>();

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    NotificationList = new List<Notification_Model>();
                }

                int n = 0;
                int count = dt.Rows.Count;

                #region Time Range Data

                while (n < count)
                {
                    Notification_Model range = new Notification_Model();

                 
                    range.Tittle = dt.Rows[n][2].ToString();
                    if (dt.Rows[n][3].ToString() == "")
                    {
                        range.Image = "";
                    }
                    else

                    {
                        range.Image = dt.Rows[n][3].ToString().Replace(" ","");
                    }

                    
                    range.Message = dt.Rows[n][4].ToString();
                    range.Date = dt.Rows[n][5].ToString();

                    NotificationList.Add(range);

                    n++;
                }

                #endregion

                return Return.returnHttp("200", NotificationList, CreateToken());

            }
            catch (Exception e)
            {
                return Return.returnHttp("201", "Some Internal Issue Occured. Please try again." + e.Message + e.StackTrace, CreateToken());
            }
        }

        #endregion
    }
}

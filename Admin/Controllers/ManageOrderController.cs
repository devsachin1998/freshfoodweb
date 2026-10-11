using Admin.Helpers;
using Admin.Models;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;

namespace Admin.Controllers
{
    public class ManageOrderController : Controller
    {
        public static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), INDIAN_ZONE);

        [HttpGet]
        public ActionResult OrderList(string FromDate, string ToDate)
        {
           Dashboard_Model data = new Dashboard_Model();
            try
            {

                DateTime Orddr_date = datetime;

                //if (!string.IsNullOrEmpty(OrderDate))
                //{
                // Orddr_date = DateTime.ParseExact(OrderDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //}


                //string query = "SELECT* FROM[ORDERMASTER] WHERE DATEADD(DD, 0, DATEDIFF(DD, 0, OrderDate)) = DATEADD(DD, 0, DATEDIFF(DD, 0, '" + OrderDate + "')";
                string query = "";
                DateTime Report_Date = datetime;

                DateTime To_Date = datetime;
                if (string.IsNullOrEmpty(FromDate) && string.IsNullOrEmpty(ToDate))
                {

                    ToDate = Report_Date.ToString("dd/MM/yyyy");

                    FromDate = Report_Date.AddDays(-1).ToString("dd/MM/yyyy");
                }

                if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
                {
                    Report_Date = DateTime.ParseExact(FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    To_Date = DateTime.ParseExact(ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    ToDate = To_Date.ToString("MM/dd/yyyy");

                    FromDate = Report_Date.ToString("MM/dd/yyyy");

                }

                if (FromDate == ToDate)
                {

                    data.DeliveredOrederd = Database.getValue("select ISNULL(count(*),0)  as TotalOrders from [OrderMaster] where  OrderStatus='Delivered'  and  DATEADD(DD, 0, DATEDIFF(DD, 0, OrderDate)) = DATEADD(DD, 0, DATEDIFF(DD, 0, '" + Report_Date + "'))");
                    data.OrderCount = Database.getValue("select ISNULL(count(*),0)  as TotalDelOrders from [OrderMaster] where OrderStatus!='Cancelled by Admin'  and  DATEADD(DD, 0, DATEDIFF(DD, 0, OrderDate)) = DATEADD(DD, 0, DATEDIFF(DD, 0, '" + Report_Date + "'))");
                    data.Balance = Database.getValue("select ISNULL(sum(Total),0)  as TotalAmt from [OrderMaster] where  OrderStatus='Delivered'  and  DATEADD(DD, 0, DATEDIFF(DD, 0, OrderDate)) = DATEADD(DD, 0, DATEDIFF(DD, 0, '" + Report_Date + "'))");
                    data.OnlineAmount = Database.getValue("select ISNULL(sum(BillingAmount),0)  as TotalDelAmt from [OrderMaster] where OrderStatus!='Cancelled by Admin'  and  DATEADD(DD, 0, DATEDIFF(DD, 0, OrderDate)) = DATEADD(DD, 0, DATEDIFF(DD, 0, '" + Report_Date + "'))");
                    query = "SELECT* FROM[ORDERMASTER] WHERE DATEADD(DD, 0, DATEDIFF(DD, 0, OrderDate)) = DATEADD(DD, 0, DATEDIFF(DD, 0, '" + Report_Date + "'))  ORDER BY 1 DESC";

                    //  string query = "SELECT* FROM[ORDERMASTER] WHERE DATEADD(DD, 0, DATEDIFF(DD, 0, OrderDate)) = DATEADD(DD, 0, DATEDIFF(DD, 0, '" + Report_Date + "')  ORDER BY 1 DESC";

                }
                else
                {
                    data.DeliveredOrederd = Database.getValue("select ISNULL(count(*),0)  as TotalOrders from [OrderMaster] where  OrderDate between '" + FromDate + " 00:00:00" + "' and '" + ToDate + " 23:59:59" + "' and OrderStatus='Delivered'");
                    data.OrderCount = Database.getValue("select ISNULL(count(*),0)  as TotalDelOrders from [OrderMaster] where    OrderDate between '" + FromDate + " 00:00:00" + "' and '" + ToDate + " 23:59:59" + "'  and  OrderStatus!='Cancelled by Admin' ");
                    data.Balance = Database.getValue("select ISNULL(sum(Total),0)  as TotalAmt from [OrderMaster] where  OrderDate between '" + FromDate + " 00:00:00" + "' and '" + ToDate + " 23:59:59" + "' and OrderStatus='Delivered'");
                    data.OnlineAmount = Database.getValue("select ISNULL(sum(BillingAmount),0)  as TotalDelAmt from [OrderMaster] where   OrderDate between '" + FromDate + " 00:00:00" + "' and '" + ToDate + " 23:59:59" + "' and OrderStatus='Delivered' and OrderStatus !='Cancelled by Admin'");

                    query = "SELECT* FROM[ORDERMASTER] WHERE  OrderDate between '" + FromDate + " 00:00:00" + "' and '" + ToDate + " 23:59:59" + "'  ORDER BY 1 DESC";

                }






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
                        if (!string.IsNullOrEmpty(dt.Rows[n][22].ToString()))
                        {
                            orderDetails.BillingAmount = Convert.ToDecimal(dt.Rows[n][22]);
                        }
                        else
                        {
                            orderDetails.BillingAmount = Convert.ToDecimal(dt.Rows[n][14]);

                        }
                        orderDetails.SysDeliveryDate = dt.Rows[n][17].ToString();


                        if (!string.IsNullOrEmpty(dt.Rows[n][18].ToString()))
                        {
                            orderDetails.PaidAmount = Convert.ToDecimal(dt.Rows[n][18]);
                        }

                        orderDetails.PaymentDate = dt.Rows[n][19].ToString();


                        data._orderList.Add(orderDetails);

                        n++;

                    }
                
                return View(data);

                   

            }


            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = ex.ToString();
                return RedirectToAction("OrderList");
            }
        }

        [HttpGet]
        public ActionResult OrderDetail(int OrderID,
    string FromDate = "",
    string ToDate = "")
        {
            try
            {
                ViewBag.FromDate = FromDate;
                ViewBag.ToDate = ToDate;
                string query = "SELECT * FROM ORDERMASTER WHERE ORDERID = " + OrderID;

                Order_Model orderDetails = new Order_Model();

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    orderDetails = new Order_Model();
                }

                int n = 0;
                int count = dt.Rows.Count;

                #region Purchase Data

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
                    orderDetails.PaymentStatus = string.IsNullOrEmpty(dt.Rows[n][11].ToString()) ? "Pending" : dt.Rows[n][11].ToString();

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
                        orderDetails.DeliveryDate = dt.Rows[n][20].ToString();
                    }
                    else
                    {
                        orderDetails.DeliveryDate = datetime.ToShortDateString();
                    }

                    if (!string.IsNullOrEmpty(dt.Rows[n][21].ToString()))
                    {
                        orderDetails.DeliveryTime = dt.Rows[n][21].ToString();
                    }
                    else
                    {
                        orderDetails.DeliveryTime = "08:00 AM - 10:00 AM";
                    }

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

                        string Weight = "", UnitID = "", UnitName = "";

                        Weight = Database.getValue("Select Weight from Item_Purchase where ItemID=" + itemData.ItemID);

                        UnitID = Database.getValue("Select UnitID from Item_Purchase where ItemID=" + itemData.ItemID);
                        if (!string.IsNullOrEmpty(UnitID))
                        {
                            int Unit_ID = Convert.ToInt32(UnitID);
                            UnitName = Database.getValue("Select Unit from Unit where UnitID=" + Unit_ID);
                        }

                        if (!string.IsNullOrEmpty(Weight))
                        {
                            itemData.AvailableStock = Weight + " " + UnitName;
                        }

                        itemData.SubItemID = Convert.ToInt32(item_dt.Rows[i][5]);
                        itemData.StockID = Convert.ToInt32(item_dt.Rows[i][6]);
                        itemData.MainID = Convert.ToInt32(item_dt.Rows[i][7]);

                        itemData.Weight = item_dt.Rows[i][8].ToString();
                        itemData.OrderQty = Convert.ToDecimal(item_dt.Rows[i][9]);

                        itemData.Price = Convert.ToDecimal(item_dt.Rows[i][10]);
                        itemData.TotalAmount = Convert.ToDecimal(item_dt.Rows[i][11]);

                        UnitID = Database.getValue("Select UOMID from SubItem where SubItemID=" + itemData.SubItemID);
                        if (!string.IsNullOrEmpty(UnitID))
                        {
                            int Unit_ID = Convert.ToInt32(UnitID);
                            itemData.SubItemUnitID = UnitID;

                            itemData.SubItemUnitName = Database.getValue("Select UOMName from SubItem where SubItemID=" + itemData.SubItemID);
                        }

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

                    if (orderDetails._itemList.Count != 0)
                    {
                        orderDetails._itemList = orderDetails._itemList.OrderBy(a => a.ItemName).ToList();
                    }

                    n++;
                }

                #endregion

                return View(orderDetails);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = "An error occured please try again !! check"+ ex;
                return RedirectToAction("OrderList");
            }
        }

        [HttpPost]
        public ActionResult ManageOrder(Order_Model data, string FromDate = "",
    string ToDate = "")
        {
            try
            {
                string query = "", output = ""; decimal billingAmount = 0;



                if (data.OrderID == 0 || string.IsNullOrEmpty(data.OrderNo))
                {
                    TempData["ErrorMessage"] = "Please try again !!";
                    return RedirectToAction("OrderList");
                }
                string checkorder = "SELECT OrderStatus FROM ORDERMASTER WHERE ORDERID =" + data.OrderID;

                string chk_ds = Database.getValue(checkorder);

                if (chk_ds == "Delivered")
                {
                    TempData["SuccessMessage"] = "Order " + data.OrderNo + " has already delivered !!";

                }
                else
                {

                    if (data.OrderID != 0 && !string.IsNullOrEmpty(data.OrderNo))
                    {
                        if (data.BillingAmount == null)
                        {
                            data.BillingAmount = 0;
                        }

                        query = "update OrderMaster set OrderStatus = '" + data.OrderStatus + "', UpdatedDate = '" + datetime + "', DeliveryDate ='" + data.DeliveryDate + "', DeliveryTime ='" + data.DeliveryTime + "' Where OrderID=" + data.OrderID;
                        output = Database.insertData(query);

                        if (data.OrderStatus == "Delivered")
                        {
                            if (string.IsNullOrEmpty(data.DeliveryDate))
                            {
                                data.DeliveryDate = datetime.AddDays(1).ToShortDateString();
                            }

                            if (string.IsNullOrEmpty(data.DeliveryTime))
                            {
                                data.DeliveryTime = "08:00 AM - 10:00 AM";
                            }

                            query = "update OrderMaster set OrderStatus = '" + data.OrderStatus + "', UpdatedDate = '" + datetime + "', SysDeliveryDate = '" + datetime + "' Where OrderID=" + data.OrderID;
                            output = Database.insertData(query);
                        }

                        if (data.PaymentStatus == "Received")
                        {
                            query = "update OrderMaster set  UpdatedDate = '" + datetime + "', PaidPayment = '" + data.Total + "', PaymentDate = '" + datetime + "', BillingAmount = '" + data.BillingAmount + "' Where OrderID=" + data.OrderID;
                            output = Database.insertData(query);
                        }
                    }

                    if (output == "200")
                    {
                        string stockUpdate_Query, stockUpdate_Result;

                        #region  // Update DeliveryWeight

                        foreach (var item in data._itemList)
                        {
                            // Update DeliveryWeight

                            decimal perviousWeight = 0;
                            var existweight = Database.getValue("Select DeliveryWeight from Order_Items where OrderItemID=" + item.OrderItemID);
                            if (!string.IsNullOrEmpty(existweight))
                            {
                                perviousWeight = Convert.ToDecimal(existweight);
                            }

                            if (item.DeliveryWeight != 0 && (perviousWeight != item.DeliveryWeight))
                            {
                                var TotalStock = "";
                                TotalStock = Database.getValue("Select Weight from Item_Purchase where ItemID=" + item.ItemID);

                                var subItemUnit = Database.getValue("select UOMName from SubItem where subitemid = " + item.SubItemID);
                                var subItemWeight = Database.getValue("select WeightType from SubItem where subitemid = " + item.SubItemID);

                                if (perviousWeight != item.DeliveryWeight)
                                {
                                    var stoclUpdatequery = "update Item_Purchase set Weight = Weight + " + perviousWeight + " Where ItemID=" + item.ItemID;
                                    var stoclUpdateoutput = Database.insertData(stoclUpdatequery);
                                }

                                if (!string.IsNullOrEmpty(TotalStock))
                                {
                                    decimal availableStockInGrams = Convert.ToDecimal(TotalStock) * 1000;
                                    decimal itemWeightInGrams = Convert.ToDecimal(item.DeliveryWeight);

                                    if (subItemUnit == "kg")
                                    {
                                        itemWeightInGrams = itemWeightInGrams * 1000;
                                    }

                                    decimal Weight = availableStockInGrams - itemWeightInGrams;

                                    decimal Weight_in_KG = Weight / 1000;

                                    if (Weight_in_KG != 0)
                                    {
                                        stockUpdate_Query = "Update Item_Purchase Set Weight = " + Weight_in_KG + " Where ItemID = " + item.ItemID;
                                        stockUpdate_Result = Database.insertData(stockUpdate_Query);
                                    }

                                    //decimal itemWeight_Grams = 0, pricePerGrms = 0, subItemAmount = 0;

                                    //if (subItemUnit == "kg")
                                    //{
                                    //    itemWeight_Grams = Convert.ToDecimal(subItemWeight) * 1000;
                                    //    pricePerGrms = item.Price * 100 / itemWeight_Grams;
                                    //    subItemAmount = pricePerGrms * itemWeightInGrams / 100;

                                    //}
                                    //else if (subItemUnit == "gms")
                                    //{
                                    //    itemWeight_Grams = Convert.ToDecimal(subItemWeight);
                                    //    pricePerGrms = item.Price * 100 / itemWeight_Grams;
                                    //    subItemAmount = pricePerGrms * item.DeliveryWeight / 100;
                                    //}
                                    //else if (Convert.ToDecimal(subItemWeight) == item.DeliveryWeight)
                                    //{
                                    //    subItemAmount = item.Price;
                                    //}

                                    //if (item.DeliveryPrice == 0)
                                    //{
                                    //    subItemAmount = 0;
                                    //}

                                    var updateItem = Database.insertData("Update Order_Items Set DeliveryWeight =" + item.DeliveryWeight + ", DeliveryPrice = " + item.DeliveryPrice + " Where OrderItemID = " + item.OrderItemID);

                                    billingAmount += item.DeliveryPrice;
                                }
                            }
                            else
                            {
                                var updateItem = Database.insertData("Update Order_Items Set DeliveryWeight =" + item.DeliveryWeight + ", DeliveryPrice = " + item.DeliveryPrice + " Where OrderItemID = " + item.OrderItemID);

                                var perviousDeliveryPrice = Database.getValue("Select DeliveryPrice from Order_Items where OrderItemID=" + item.OrderItemID);
                                if (!string.IsNullOrEmpty(perviousDeliveryPrice))
                                {
                                    decimal DeliveryPrice = Convert.ToDecimal(perviousDeliveryPrice);
                                    billingAmount += DeliveryPrice;
                                }
                            }
                        }

                        #endregion

                        query = "update OrderMaster set BillingAmount = " + billingAmount + " Where OrderID=" + data.OrderID;
                        output = Database.insertData(query);

                        if (data.OrderStatus == "Accepted")
                        {
                            #region Accepted Mail

                            /* var isMailSent = Database.getValue("SELECT AcceptMail FROM OrderMailStatus WHERE ORDERNO = '" + data.OrderNo + "' and OrderID = " + data.OrderID);
                             if (isMailSent != "True")
                             {
                                 var emailBody = Accepted_OrderSummry_EmailBody(data.CustomerName, data.OrderNo);
                                 string response = CommonHelper.SendMail(data.CustomerEmail, emailBody, "Your FreshFoodz order no : " + data.OrderNo + " has been accepted");

                                 if (response == "Sucess")
                                 {
                                     string orderQuery = "Update [OrderMailStatus] Set AcceptMail = 1 where ORDERNO = '" + data.OrderNo + "' and OrderID =  " + data.OrderID;
                                     string orderOutput = Database.insertData(orderQuery);
                                 }
                             }
     */
                            TempData["SuccessMessage"] = "Order " + data.OrderNo + " has been marked as accepted !!";

                            #endregion
                        }
                        if (data.OrderStatus == "Declined")
                        {
                            #region Declined mail

                            /* var isMailSent = Database.getValue("SELECT CancelledMail FROM OrderMailStatus WHERE ORDERNO = '" + data.OrderNo + "' and OrderID = " + data.OrderID);
                             if (isMailSent != "True")
                             {
                                 var emailBody = Declined_OrderSummary_EmailBody(data.CustomerName);
                                 string response = CommonHelper.SendMail(data.CustomerEmail, emailBody, "Your FreshFoodz order no : " + data.OrderNo + " has been declined");

                                 if (response == "Sucess")
                                 {
                                     string orderQuery = "Update [OrderMailStatus] Set CancelledMail = 1 where ORDERNO = '" + data.OrderNo + "' and OrderID =  " + data.OrderID;
                                     string orderOutput = Database.insertData(orderQuery);
                                 }
                             }*/

                            #endregion

                            TempData["SuccessMessage"] = "Order " + data.OrderNo + " has been marked as declined !!";
                        }
                        if (data.OrderStatus == "Ready")
                        {
                            #region Dispatch Mail

                            // var isMailSent = Database.getValue("SELECT DispatchMail FROM OrderMailStatus WHERE ORDERNO = '" + data.OrderNo + "' and OrderID = " + data.OrderID);
                            /* if (isMailSent != "True")
                             {
                                 DateTime deliveryDate = Convert.ToDateTime(data.DeliveryDate);
                                 var emailBody = Dispatched_OrderSummry_EmailBody(data.CustomerName, data.OrderNo, deliveryDate.ToShortDateString(), data.DeliveryTime, billingAmount);
                                 string response = CommonHelper.SendMail(data.CustomerEmail, emailBody, "Your FreshFoodz order no : " + data.OrderNo + " has been ready");

                                 if (response == "Sucess")
                                 {
                                     string orderQuery = "Update [OrderMailStatus] Set DispatchMail = 1 where ORDERNO = '" + data.OrderNo + "' and OrderID =  " + data.OrderID;
                                     string orderOutput = Database.insertData(orderQuery);
                                 }
                             }*/
                            #endregion

                            TempData["SuccessMessage"] = "Order " + data.OrderNo + " has been mark as ready !!";
                        }

                        if (data.OrderStatus == "Delivered")
                        {
                            var FileName = "";

                            #region Order data

                            query = "SELECT * FROM ORDERMASTER WHERE ORDERID = " + data.OrderID;

                            Order_Model orderDetails = new Order_Model();

                            DataSet ds = Database.getDataSet(query);
                            DataTable dt = ds.Tables[0];
                            if (ds == null || dt.Rows.Count == 0)
                            {
                                orderDetails = new Order_Model();
                            }

                            int n = 0;
                            int count = dt.Rows.Count;

                            #region Purchase Data

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
                                orderDetails.PaymentStatus = string.IsNullOrEmpty(dt.Rows[n][11].ToString()) ? "Pending" : dt.Rows[n][11].ToString();

                                orderDetails.CustomerRemark = dt.Rows[n][12].ToString();
                                orderDetails.AdminRemark = dt.Rows[n][13].ToString();

                                orderDetails.Total = Convert.ToDecimal(dt.Rows[n][14]);

                                orderDetails.SysDeliveryDate = dt.Rows[n][17].ToString();


                                if (!string.IsNullOrEmpty(dt.Rows[n][18].ToString()))
                                {
                                    orderDetails.PaidAmount = Convert.ToDecimal(dt.Rows[n][18]);
                                }

                                orderDetails.PaymentDate = dt.Rows[n][19].ToString();

                                orderDetails.DeliveryDate = dt.Rows[n][20].ToString();
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

                                    string Weight = "", UnitID = "", UnitName = "";

                                    Weight = Database.getValue("Select Weight from Item_Purchase where ItemID=" + itemData.ItemID);

                                    UnitID = Database.getValue("Select UnitID from Item_Purchase where ItemID=" + itemData.ItemID);
                                    if (!string.IsNullOrEmpty(UnitID))
                                    {
                                        int Unit_ID = Convert.ToInt32(UnitID);
                                        UnitName = Database.getValue("Select Unit from Unit where UnitID=" + itemData.ItemID);
                                    }

                                    if (!string.IsNullOrEmpty(Weight))
                                    {
                                        itemData.AvailableStock = Weight + " " + UnitName;
                                    }

                                    itemData.SubItemID = Convert.ToInt32(item_dt.Rows[i][5]);
                                    itemData.StockID = Convert.ToInt32(item_dt.Rows[i][6]);
                                    itemData.MainID = Convert.ToInt32(item_dt.Rows[i][7]);

                                    itemData.Weight = item_dt.Rows[i][8].ToString();
                                    itemData.OrderQty = Convert.ToDecimal(item_dt.Rows[i][9]);

                                    itemData.Price = Convert.ToDecimal(item_dt.Rows[i][10]);
                                    itemData.TotalAmount = Convert.ToDecimal(item_dt.Rows[i][11]);

                                    UnitID = Database.getValue("Select UOMID from SubItem where SubItemID=" + itemData.SubItemID);
                                    if (!string.IsNullOrEmpty(UnitID))
                                    {
                                        int Unit_ID = Convert.ToInt32(UnitID);
                                        itemData.SubItemUnitID = UnitID;

                                        itemData.SubItemUnitName = Database.getValue("Select UOMName from SubItem where SubItemID=" + itemData.SubItemID);
                                    }

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

                                string InvoiceFile = OrderSummary.GenerateOrderSummary(orderDetails);
                                if (InvoiceFile != "Error.pdf")
                                {
                                    FileName = InvoiceFile;
                                    orderDetails.Invoice = "http://api.freshfoodz.in/Content/OrderSummary/" + InvoiceFile;
                                }

                                #endregion

                                n++;
                            }

                            #endregion

                            #endregion

                            #region delivered mail

                            /* var isMailSent = Database.getValue("SELECT deliveredmail FROM OrderMailStatus WHERE ORDERNO = '" + data.OrderNo + "' and OrderID = " + data.OrderID);
                             if (isMailSent != "True")
                             {
                                 var emailBody = Delivered_Order_EmailBody(data.CustomerName, data.OrderNo);
                                 string response = CommonHelper.SendDeliveryMail(data.CustomerEmail, emailBody, "Your FreshFoodz order no : " + data.OrderNo + " has been delivered", FileName);

                                 if (response == "Sucess")
                                 {
                                     string orderQuery = "Update [OrderMailStatus] Set deliveredmail = 1 where ORDERNO = '" + data.OrderNo + "' and OrderID =  " + data.OrderID;
                                     string orderOutput = Database.insertData(orderQuery);
                                 }
                             }*/

                            #endregion

                            #region --> Manage Wallet

                            string Wallet_query = "", Wallet_output = "";
                            var Wallet_ID = Database.getValue("SELECT WalletID FROM UserWallet WHERE UserID = " + data.UserID + "");
                            decimal WalletID = (!string.IsNullOrEmpty(Wallet_ID)) ? Convert.ToInt32(Wallet_ID) : 0;

                            if (data.UserID != 0 && WalletID == 0)
                            {
                                var balanceAmount = 0 - orderDetails.BillingAmount;
                                Wallet_query = "Insert into UserWallet Values(" + data.UserID + ", " + balanceAmount + ", 1,'" + datetime + "', " + data.UserID + ")";
                                Wallet_output = Database.insertData(Wallet_query);

                                if (Wallet_output == "200")
                                {
                                    Wallet_ID = Database.getValue("SELECT WalletID FROM UserWallet WHERE UserID = " + data.UserID + "");
                                    WalletID = (!string.IsNullOrEmpty(Wallet_ID)) ? Convert.ToInt32(Wallet_ID) : 0;

                                    if (WalletID != 0)
                                    {
                                        Wallet_query = "Insert into UserWalletHistory Values(" + WalletID + ", " + data.UserID + ", 'Debit', 'Purchase', " + balanceAmount + ",  " + balanceAmount + ", '" + data.OrderNo + "', '" + datetime + "', null, 'Admin','Offline')";
                                        Wallet_output = Database.insertData(Wallet_query);
                                    }
                                }
                            }
                            else if (data.UserID != 0 && WalletID != 0)
                            {
                                var perviousBalance = Database.getValue("SELECT Balance FROM UserWallet WHERE UserID = " + data.UserID + "");
                                var ExistHistoryID = Database.getValue("select HistoryID from UserWalletHistory where userID =  " + data.UserID + " and WalletID = " + WalletID + " and OrderNo = '" + data.OrderNo + "'");

                                if (!string.IsNullOrEmpty(ExistHistoryID))
                                {
                                    Wallet_query = "Update UserWalletHistory Set Amount =  " + orderDetails.BillingAmount + ", Balance =  " + orderDetails.BillingAmount + " Where UserID =  " + data.UserID + " and WalletID = " + WalletID + " and HistoryID = " + ExistHistoryID;
                                    Wallet_output = Database.insertData(Wallet_query);
                                    if (Wallet_output == "200")
                                    {
                                        var existing_Balance = Database.getValue("select Balance from UserWallet Where WalletID = " + WalletID + " and UserID = " + data.UserID + "");
                                        decimal existingBalance = Convert.ToDecimal(existing_Balance);
                                        if (existingBalance != -orderDetails.BillingAmount)
                                        {
                                            decimal newBalance = existingBalance - Convert.ToDecimal(orderDetails.BillingAmount);

                                            // update balance from UserWallet table
                                            Wallet_query = "";
                                            Wallet_query = "Update UserWallet set Balance = " + newBalance + " Where WalletID = " + WalletID + " and UserID = " + data.UserID + "";
                                            Wallet_output = Database.insertData(Wallet_query);
                                        }
                                    }
                                }
                                else
                                {
                                    var balanceAmount = Convert.ToDecimal(perviousBalance) - orderDetails.BillingAmount;
                                    Wallet_query = "Insert into UserWalletHistory Values(" + WalletID + ", " + data.UserID + ", 'Debit', 'Purchase', " + orderDetails.BillingAmount + ",  " + balanceAmount + ", '" + data.OrderNo + "', '" + datetime + "', null, 'Admin','Offline')";
                                    Wallet_output = Database.insertData(Wallet_query);
                                    if (Wallet_output == "200")
                                    {
                                        var existing_Balance = Database.getValue("select Balance from UserWallet Where WalletID = " + WalletID + " and UserID = " + data.UserID + "");
                                        decimal existingBalance = Convert.ToDecimal(existing_Balance);
                                        decimal newBalance = existingBalance - Convert.ToDecimal(orderDetails.BillingAmount);

                                        // update balance from UserWallet table
                                        Wallet_query = "";
                                        Wallet_query = "Update UserWallet set Balance = " + newBalance + " Where WalletID = " + WalletID + " and UserID = " + data.UserID + "";
                                        Wallet_output = Database.insertData(Wallet_query);
                                    }
                                }
                            }


                      
                        var perviousBalance1 = Database.getValue("SELECT Balance FROM UserWallet WHERE UserID = " + data.UserID + "");

                        string Msg = "";
                        if (Convert.ToDecimal(perviousBalance1)>0)
                        {

                             Msg = "Your Order Id:" + data.OrderNo + " is successfully delivered on " + datetime + " and your available Balance is "+ perviousBalance1 + ".";

                        }
                        else
                        {

                            Msg = "Your Order Id:" + data.OrderNo + " is successfully delivered on " + datetime + " and your outstanding Balance is " + perviousBalance1 + ".";

                        }


                        string Ttl = "Order Delivered";
                        string notification_query = "insert into Tbl_Notifications values('" + data.UserID + "', '" + Ttl + "', '', '" + Msg + "','" + datetime + "','')";
                        string output1 = Database.insertData(notification_query);

                        if (output1 == "200")
                        {


                            string fcmtoken = Database.getValue("select  fcmtoken from tbl_Register_User WHERE UserID = " + data.UserID + "");
                            if (fcmtoken != null && fcmtoken != "")
                            {
                                string data1 = CommonHelper.SendNotification(fcmtoken, Msg, Ttl, "");
                            }

                        }
                        else
                        {
                            TempData["ErrorMessage"] = "An error occured please try again !!";

                        }
                        TempData["SuccessMessage"] = "Order " + data.OrderNo + " has been marked as delivered !!";
                      
                    
                    
                    
                    }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "An error occured please try again !! extra";
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = "An error occured please try again !!"+ ex;
            }
            if (!string.IsNullOrEmpty(FromDate) &&
      !string.IsNullOrEmpty(ToDate))
            {
                return RedirectToAction("OrderList", new
                {
                    FromDate = FromDate,
                    ToDate = ToDate
                });
            }

            return RedirectToAction("OrderList");
        }

        [HttpGet]
        public JsonResult PrintOrder(int OrderID)
        {
            try
            {
                string query = "SELECT * FROM ORDERMASTER WHERE ORDERID = " + OrderID;

                Order_Model orderDetails = new Order_Model();

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    orderDetails = new Order_Model();
                }

                int n = 0;
                int count = dt.Rows.Count;

                #region Purchase Data

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
                    orderDetails.PaymentStatus = string.IsNullOrEmpty(dt.Rows[n][11].ToString()) ? "Pending" : dt.Rows[n][11].ToString();

                    orderDetails.CustomerRemark = dt.Rows[n][12].ToString();
                    orderDetails.AdminRemark = dt.Rows[n][13].ToString();

                    orderDetails.Total = Convert.ToDecimal(dt.Rows[n][14]);

                    orderDetails.SysDeliveryDate = dt.Rows[n][17].ToString();


                    if (!string.IsNullOrEmpty(dt.Rows[n][18].ToString()))
                    {
                        orderDetails.PaidAmount = Convert.ToDecimal(dt.Rows[n][18]);
                    }

                    orderDetails.PaymentDate = dt.Rows[n][19].ToString();

                    orderDetails.DeliveryDate = dt.Rows[n][20].ToString();
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

                        string Weight = "", UnitID = "", UnitName = "";

                        Weight = Database.getValue("Select Weight from Item_Purchase where ItemID=" + itemData.ItemID);

                        UnitID = Database.getValue("Select UnitID from Item_Purchase where ItemID=" + itemData.ItemID);
                        if (!string.IsNullOrEmpty(UnitID))
                        {
                            int Unit_ID = Convert.ToInt32(UnitID);
                            UnitName = Database.getValue("Select Unit from Unit where UnitID=" + Unit_ID);
                        }

                        if (!string.IsNullOrEmpty(Weight))
                        {
                            itemData.AvailableStock = Weight + " " + UnitName;
                        }

                        itemData.SubItemID = Convert.ToInt32(item_dt.Rows[i][5]);
                        itemData.StockID = Convert.ToInt32(item_dt.Rows[i][6]);
                        itemData.MainID = Convert.ToInt32(item_dt.Rows[i][7]);

                        itemData.Weight = item_dt.Rows[i][8].ToString();
                        itemData.OrderQty = Convert.ToDecimal(item_dt.Rows[i][9]);

                        itemData.Price = Convert.ToDecimal(item_dt.Rows[i][10]);
                        itemData.TotalAmount = Convert.ToDecimal(item_dt.Rows[i][11]);

                        UnitID = Database.getValue("Select UOMID from SubItem where SubItemID=" + itemData.SubItemID);
                        if (!string.IsNullOrEmpty(UnitID))
                        {
                            int Unit_ID = Convert.ToInt32(UnitID);
                            itemData.SubItemUnitID = UnitID;

                            itemData.SubItemUnitName = Database.getValue("Select UOMName from SubItem where SubItemID=" + itemData.SubItemID);
                        }

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

                    if (orderDetails._itemList.Count != 0)
                    {
                        orderDetails._itemList = orderDetails._itemList.OrderBy(a => a.ItemName).ToList();
                    }

                    string InvoiceFile = OrderSummary.GenerateOrderSummary(orderDetails);
                    if (InvoiceFile != "Error.pdf")
                    {
                        orderDetails.Invoice = "https://api.freshfoodz.in/Content/OrderSummary/" + InvoiceFile;
                    }

                    n++;
                }

                #endregion

                Print_Receipt printFunction = new Print_Receipt(orderDetails);
                printFunction.Print("");

                return Json("Success", JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult POSPrint(int OrderID)
        {
            Order_Model orderDetails = new Order_Model();

            try
            {
                string query = "SELECT * FROM ORDERMASTER WHERE ORDERID = " + OrderID;

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    orderDetails = new Order_Model();
                }

                int n = 0;
                int count = dt.Rows.Count;

                #region Purchase Data

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
                    orderDetails.PaymentStatus = string.IsNullOrEmpty(dt.Rows[n][11].ToString()) ? "Pending" : dt.Rows[n][11].ToString();

                    orderDetails.CustomerRemark = dt.Rows[n][12].ToString();
                    orderDetails.AdminRemark = dt.Rows[n][13].ToString();

                    orderDetails.Total = Convert.ToDecimal(dt.Rows[n][14]);

                    orderDetails.SysDeliveryDate = dt.Rows[n][17].ToString();


                    if (!string.IsNullOrEmpty(dt.Rows[n][18].ToString()))
                    {
                        orderDetails.PaidAmount = Convert.ToDecimal(dt.Rows[n][18]);
                    }

                    orderDetails.PaymentDate = dt.Rows[n][19].ToString();

                    orderDetails.DeliveryDate = dt.Rows[n][20].ToString();
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

                        string Weight = "", UnitID = "", UnitName = "";

                        Weight = Database.getValue("Select Weight from Item_Purchase where ItemID=" + itemData.ItemID);

                        UnitID = Database.getValue("Select UnitID from Item_Purchase where ItemID=" + itemData.ItemID);
                        if (!string.IsNullOrEmpty(UnitID))
                        {
                            int Unit_ID = Convert.ToInt32(UnitID);
                            UnitName = Database.getValue("Select Unit from Unit where UnitID=" + Unit_ID);
                        }

                        if (!string.IsNullOrEmpty(Weight))
                        {
                            itemData.AvailableStock = Weight + " " + UnitName;
                        }

                        itemData.SubItemID = Convert.ToInt32(item_dt.Rows[i][5]);
                        itemData.StockID = Convert.ToInt32(item_dt.Rows[i][6]);
                        itemData.MainID = Convert.ToInt32(item_dt.Rows[i][7]);

                        itemData.Weight = item_dt.Rows[i][8].ToString();
                        itemData.OrderQty = Convert.ToDecimal(item_dt.Rows[i][9]);

                        itemData.Price = Convert.ToDecimal(item_dt.Rows[i][10]);
                        itemData.TotalAmount = Convert.ToDecimal(item_dt.Rows[i][11]);

                        UnitID = Database.getValue("Select UOMID from SubItem where SubItemID=" + itemData.SubItemID);
                        if (!string.IsNullOrEmpty(UnitID))
                        {
                            int Unit_ID = Convert.ToInt32(UnitID);
                            itemData.SubItemUnitID = UnitID;

                            itemData.SubItemUnitName = Database.getValue("Select UOMName from SubItem where SubItemID=" + itemData.SubItemID);
                        }

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

                    if (orderDetails._itemList.Count != 0)
                    {
                        orderDetails._itemList = orderDetails._itemList.OrderBy(a => a.ItemName).ToList();
                    }

                    n++;
                }

                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
            }
            return View(orderDetails);
        }

        [HttpGet]
        public JsonResult CalculateOrderPrice(decimal DeliveryWeight = 0, int ItemID = 0, int SubItemID = 0, decimal Price = 0)
        {
            var TotalStock = "";
            TotalStock = Database.getValue("Select Weight from Item_Purchase where ItemID=" + ItemID);

            var subItemUnit = Database.getValue("select UOMName from SubItem where subitemid = " + SubItemID);
            var subItemWeight = Database.getValue("select WeightType from SubItem where subitemid = " + SubItemID);

            decimal availableStockInGrams = Convert.ToDecimal(TotalStock) * 1000;
            decimal itemWeightInGrams = Convert.ToDecimal(DeliveryWeight);

            if (subItemUnit == "kg")
            {
                itemWeightInGrams = itemWeightInGrams * 1000;
            }

            decimal Weight = availableStockInGrams - itemWeightInGrams;

            decimal Weight_in_KG = Weight / 1000;

            decimal itemWeight_Grams = 0, pricePerGrms = 0, subItemAmount = 0;

            if (subItemUnit == "kg")
            {
                itemWeight_Grams = Convert.ToDecimal(subItemWeight) * 1000;
                pricePerGrms = Price * 100 / itemWeight_Grams;
                subItemAmount = pricePerGrms * itemWeightInGrams / 100;

            }
            else if (subItemUnit == "gms")
            {
                itemWeight_Grams = Convert.ToDecimal(subItemWeight);
                pricePerGrms = Price * 100 / itemWeight_Grams;
                subItemAmount = pricePerGrms * DeliveryWeight / 100;
            }
            else if (Convert.ToDecimal(subItemWeight) == DeliveryWeight)
            {
                subItemAmount = Price;
            }
            else
            {
                subItemAmount = Price;
            }
            if (DeliveryWeight == 0)
            {
                subItemAmount = 0;
            }

            return Json(subItemAmount, JsonRequestBehavior.AllowGet);
        }

        private string Accepted_OrderSummry_EmailBody(string CustomerName, string OrderNo)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Views/ManageOrder/Accepted_OrderSummary.cshtml")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{CName}", CustomerName);
            body = body.Replace("{OrderNo}", OrderNo);
            return body;
        }

        private string Declined_OrderSummary_EmailBody(string CustomerName)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Views/ManageOrder/Declined_OrderSummary.cshtml")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{CName}", CustomerName);
            return body;
        }

        private string Dispatched_OrderSummry_EmailBody(string CustomerName, string OrderNo, string DeliveryDate, string DeliveryTime, decimal BillAmount)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Views/ManageOrder/Dispatched_OrderSummary.cshtml")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{CName}", CustomerName);
            body = body.Replace("{OrderNo}", OrderNo);
            body = body.Replace("{DeliveryDate}", DeliveryDate);
            body = body.Replace("{DeliveryTime}", DeliveryTime);
            body = body.Replace("{BillAmount}", BillAmount.ToString("0.00"));
            return body;
        }

        private string Delivered_Order_EmailBody(string CustomerName, string OrderNo)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Views/ManageOrder/Delivered_Order.cshtml")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{CName}", CustomerName);
            body = body.Replace("{OrderNo}", OrderNo);
            return body;
        }
    }
}
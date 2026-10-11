using Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using static Admin.Models.Report_Model;

namespace Admin.Controllers
{
    public class ReportController : Controller
    {
        public static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), INDIAN_ZONE);

        #region PurchaseSummary

        [HttpGet]
        public ActionResult PurchaseSummary(string FromDate, string ToDate, int CategoryID = 0)
        {
            List<Report_Model> reportList = new List<Report_Model>();

            try
            {
                if (!string.IsNullOrEmpty(FromDate) && CategoryID != 0)
                {
                    DateTime Report_Date = datetime;

                    DateTime To_Date = datetime;
                    if (!string.IsNullOrEmpty(FromDate))
                    {
                        Report_Date = DateTime.ParseExact(FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        To_Date = DateTime.ParseExact(ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        ToDate = To_Date.ToString("MM/dd/yyyy");

                        FromDate = Report_Date.ToString("MM/dd/yyyy");
                    }

                    string query = "select * from ItemMaster Where ItemID in (Select ItemID from Order_Items Where OrderID in (SELECT OrderID FROM [ORDERMASTER]  WHERE  OrderStatus not in ('Cancelled by Admin','Declined','Accepted','Cancelled by user') and OrderDate between '" + FromDate + " 00:00:00" + "' and '" + ToDate + " 23:59:59" + "')) Order by ItemName";

                    DataSet ds = Database.getDataSet(query);
                    DataTable dt = ds.Tables[0];
                    if (ds == null || dt.Rows.Count == 0)
                    {
                        reportList = new List<Report_Model>();
                    }

                    int n = 0;
                    int count = dt.Rows.Count;

                    int perviousItemID = 0;

                    while (n < count)
                    {
                        Report_Model itemData = new Report_Model();
                        string subItemUOMList = ""; decimal Weight_IN_KG = 0;

                        itemData.ItemID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                        itemData.ItemName = dt.Rows[n][1].ToString();
                        itemData.CategoryID = ((dt.Rows[n][2] == null) ? 0 : Convert.ToInt32(dt.Rows[n][2]));
                        itemData.CategoryName = Database.getValue("select CategoryName from CategoryMaster Where CategoryID=" + itemData.CategoryID);

                        string UOMList_Query = "SELECT Distinct UOMName FROM SUBITEM WHERE ITEMID =  " + itemData.ItemID + " and SubItemID in (select distinct SubItemID from [Order_Items] Where OrderID in (SELECT OrderID  FROM [ORDERMASTER] WHERE  OrderStatus not in ('Cancelled by Admin','Declined','Accepted','Cancelled by user') and  OrderDate between '" + FromDate + " 00:00:00" + "' and '" + ToDate + " 23:59:59" + "')) and ITEMID = " + itemData.ItemID + "";
                        DataSet UOMList_ds = Database.getDataSet(UOMList_Query);
                        DataTable UOMList_dt = UOMList_ds.Tables[0];
                        if (UOMList_ds == null || UOMList_dt.Rows.Count == 0)
                        {
                            itemData._itemOrderList = new List<ItemOrder>();
                        }
                        int k = 0;
                        int UOMList_count = UOMList_dt.Rows.Count;
                        while (k < UOMList_count)
                        {
                            //if (UOMList_dt.Rows[k][0].ToString() == "kg" || UOMList_dt.Rows[k][0].ToString() == "Kg" || UOMList_dt.Rows[k][0].ToString() == "kg" || UOMList_dt.Rows[k][0].ToString() == "gms")
                            //{
                            if (itemData.ItemID != perviousItemID && !subItemUOMList.Contains(UOMList_dt.Rows[k][0].ToString()))
                            {
                                if (!subItemUOMList.Contains(UOMList_dt.Rows[k][0].ToString()))
                                {
                                    subItemUOMList += UOMList_dt.Rows[k][0].ToString() + "|";
                                    perviousItemID = itemData.ItemID;
                                }
                            }
                            else if (itemData.ItemID == perviousItemID && !subItemUOMList.Contains(UOMList_dt.Rows[k][0].ToString()))
                            {
                                if (!subItemUOMList.Contains(UOMList_dt.Rows[k][0].ToString()))
                                {
                                    subItemUOMList += UOMList_dt.Rows[k][0].ToString() + "|";
                                    perviousItemID = itemData.ItemID;
                                }
                            }
                            //}
                            k++;
                        }

                        #region Order Item

                        itemData._itemOrderList = new List<ItemOrder>();

                        string itemQuery = "select * from [Order_Items] Where OrderID in (SELECT OrderID  FROM [ORDERMASTER] WHERE  OrderStatus not in ('Cancelled by Admin','Declined','Accepted','Cancelled by user') and  OrderDate between '" + FromDate + " 00:00:00" + "' and '" + ToDate + " 23:59:59" + "') and ITEMID =" + itemData.ItemID + " order by SubItemID ";

                        DataSet item_ds = Database.getDataSet(itemQuery);
                        DataTable item_dt = item_ds.Tables[0];
                        if (item_ds == null || item_dt.Rows.Count == 0)
                        {
                            itemData._itemOrderList = new List<ItemOrder>();
                        }

                        int i = 0;
                        int item_count = item_dt.Rows.Count;

                        while (i < item_count)
                        {
                            ItemOrder item = new ItemOrder();

                            item.ItemID = Convert.ToInt32(item_dt.Rows[i][3]);
                            item.SubItemID = Convert.ToInt32(item_dt.Rows[i][5]);
                            var numbers = Regex.Replace((string)item_dt.Rows[i][8], "[^0-9]+", string.Empty);
                            var numbers2 = (decimal)item_dt.Rows[i][13];
                            if (numbers2>= Convert.ToDecimal(numbers))
                            {
                                item.Weight = Convert.ToDecimal(numbers);

                            }
                            else
                            {
                                item.Weight = (decimal)item_dt.Rows[i][13];

                            }

                            item.OrderQty = Convert.ToInt32(item_dt.Rows[i][9]);

                            itemData._itemOrderList.Add(item);

                            #region Order Item Data

                            int SubItemID = item.SubItemID;
                            int ItemID = item.ItemID;
                            int UOMID = Convert.ToInt32(Database.getValue("select UOMID from SubItem where subitemid = " + SubItemID));
                            string UOMName = Database.getValue("select UOMName from SubItem where subitemid = " + SubItemID);
                            decimal Weight =  item.Weight;

                            if (UOMName == "kg" || UOMName == "Kg")
                            {
                                Weight = Weight * 1000;
                                itemData.ItemTotal += Weight;
                            }
                            else if (UOMName == "gms")
                            {
                                itemData.ItemTotal += Weight;
                            }

                            #endregion

                            #region Sub Item

                            //string SUBITEMQuery = "SELECT * FROM SUBITEM WHERE ITEMID =" + itemData.ItemID + " and SubItemID =" + item.SubItemID;

                            //DataSet SUBITEM_ds = Database.getDataSet(SUBITEMQuery);
                            //DataTable SUBITEM_dt = SUBITEM_ds.Tables[0];
                            //if (SUBITEM_ds == null || SUBITEM_dt.Rows.Count == 0)
                            //{
                            //}

                            //int j = 0;
                            //int j_count = SUBITEM_dt.Rows.Count;


                            //decimal Weight_IN_KG = 0, Weight_IN_Gms = 0;
                            //decimal GmsTotal = 0;

                            //while (j < j_count)
                            //{
                            //    int SubItemID = Convert.ToInt32(SUBITEM_dt.Rows[j][0]);
                            //    int ItemID = Convert.ToInt32(SUBITEM_dt.Rows[j][1]);
                            //    int UOMID = Convert.ToInt32(SUBITEM_dt.Rows[j][2]);
                            //    string UOMName = SUBITEM_dt.Rows[j][3].ToString();
                            //    decimal Weight = Convert.ToDecimal(SUBITEM_dt.Rows[j][4].ToString());

                            //    if (UOMName == "kg" || UOMName == "Kg")
                            //    {
                            //        Weight_IN_Gms = Convert.ToDecimal(Weight) * 1000;
                            //        Weight_IN_KG = Weight_IN_Gms / 1000;
                            //        itemData.ItemTotal = Weight_IN_KG;
                            //    }
                            //    else if (subItemUOMList.Contains("kg|gms"))
                            //    {
                            //        Weight_IN_Gms = Convert.ToDecimal(Weight) * 1000;
                            //        Weight_IN_KG = Weight_IN_Gms / 1000;
                            //        itemData.ItemTotal += Weight_IN_KG;
                            //    }
                            //    else if (subItemUOMList.Contains("gms") && itemData._itemOrderList.Count() > 1)
                            //    {
                            //        Weight_IN_Gms = Convert.ToDecimal(Weight) * 1000;
                            //        Weight_IN_KG = Weight_IN_Gms / 1000;
                            //        itemData.ItemTotal += Weight_IN_KG;
                            //    }
                            //    else
                            //    {
                            //        itemData.ItemTotal+= Weight;
                            //    }

                            //    j++;
                            //}

                            #endregion

                            i++;
                        }


                        #endregion

                        if (subItemUOMList.Contains("kg|gms") || subItemUOMList.Contains("gms|kg"))
                        {
                            Weight_IN_KG = itemData.ItemTotal / 1000;
                            itemData.ItemTotal = Weight_IN_KG;
                            itemData.ItemTotal_UOM = "kg";
                        }
                        else if (subItemUOMList.Contains("kg") || subItemUOMList.Contains("Kg"))
                        {
                            Weight_IN_KG = itemData.ItemTotal / 1000;
                            itemData.ItemTotal = Weight_IN_KG;
                            itemData.ItemTotal_UOM = "kg";
                        }
                        else if (subItemUOMList.Contains("gms") && itemData._itemOrderList.Count() > 1)
                        {
                            Weight_IN_KG = itemData.ItemTotal / 1000;
                            itemData.ItemTotal = Weight_IN_KG;
                            itemData.ItemTotal_UOM = "kg";
                        }
                        //else if (subItemUOMList.Contains("gms"))
                        //{
                        //    Weight_IN_KG = itemData.ItemTotal / 1000;
                        //    itemData.ItemTotal = Weight_IN_KG;
                        //}
                        else
                        {
                            if (subItemUOMList.Contains("gms"))
                            {
                                itemData.ItemTotal = itemData.ItemTotal;
                                itemData.ItemTotal_UOM = "gms";
                            }
                            if (subItemUOMList.Contains("Box"))
                            {
                                itemData.ItemTotal = itemData.ItemTotal;
                                itemData.ItemTotal_UOM = "Box";
                            }
                            if (subItemUOMList.Contains("ltr"))
                            {
                                itemData.ItemTotal = itemData.ItemTotal;
                                itemData.ItemTotal_UOM = "ltr";
                            }
                            if (subItemUOMList.Contains("pcs"))
                            {
                                itemData.ItemTotal = itemData.ItemTotal;
                                itemData.ItemTotal_UOM = "pcs";
                            }
                        }

                        if (CategoryID == itemData.CategoryID)
                        {
                            reportList.Add(itemData);
                        }
                        n++;
                    }
                }
                return View(reportList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = "An error occured please try again !!";
                return RedirectToAction("PurchaseSummary");
            }
        }

        #endregion

        #region Order Summary

        [HttpGet]
        public ActionResult OrderSummary(string ReportDate)
        {
            Report_Model reportList = new Report_Model();
            reportList._OrderDetails = new List<OrderDetails>();
            try
            {
                if (!string.IsNullOrEmpty(ReportDate))
                {
                    DateTime Report_Date = datetime;
                    if (!string.IsNullOrEmpty(ReportDate))
                    {
                        Report_Date = DateTime.ParseExact(ReportDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    }

                    string query = "SELECT * FROM [ORDERMASTER] WHERE DATEADD(DD, 0, DATEDIFF(DD, 0, OrderDate)) = DATEADD(DD, 0, DATEDIFF(DD, 0, '" + Report_Date + "')) and OrderStatus in ('Delivered') Order by 1 desc";

                    DataSet ds = Database.getDataSet(query);
                    DataTable dt = ds.Tables[0];
                    if (ds == null || dt.Rows.Count == 0)
                    {
                        reportList = new Report_Model();
                       
                        reportList._OrderDetails = new List<OrderDetails>();
                    }

                    int n = 0;


                    int count = dt.Rows.Count;

                    while (n < count)
                    {
                        OrderDetails OrderData = new OrderDetails();

                        OrderData.OrderID = Convert.ToInt32(dt.Rows[n][0].ToString());
                        OrderData.OrderNo = dt.Rows[n][0].ToString();
                         OrderData.CustomerName = dt.Rows[n][4].ToString();
                        OrderData.ReceviedAmount = Convert.ToDecimal(dt.Rows[n][22].ToString());

                        OrderData.Invoice = "http://admin.freshfoodz.in.net/Content/OrderSummary/OrderSummary_" + OrderData.OrderNo + ".pdf";

                        reportList._OrderDetails.Add(OrderData);
                        string query1 = "SELECT  Sum(BillingAmount) as Total FROM [ORDERMASTER] WHERE DATEADD(DD, 0, DATEDIFF(DD, 0, OrderDate)) = DATEADD(DD, 0, DATEDIFF(DD, 0, '" + Report_Date + "')) and OrderStatus in ('Delivered') Order by 1 desc";

                        reportList.ItemTotal_UOM = Database.getValue(query1);

                        n++;
                    }
                }
                return View(reportList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = "An error occured please try again !!";
                return RedirectToAction("OrderSummary");

              
            }
        }

        #endregion


        #region Pin

        [HttpGet]
        public ActionResult PinCodeList()
        {
            Pincode_Modal pinlist_data = new Pincode_Modal();
            pinlist_data._locationList = new List<Pincode_Modal>();


            try
            {
                string query = "select * from [PinMaster] order by 2 ";

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    pinlist_data._locationList = new List<Pincode_Modal>();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    Pincode_Modal pin_data = new Pincode_Modal();
                    pin_data.PinID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    pin_data.LocationName = dt.Rows[n][1].ToString();
                    pin_data.LocationCode = dt.Rows[n][2].ToString();
                    pin_data.IsActive = dt.Rows[n][3].ToString();
                    pinlist_data._locationList.Add(pin_data);
                    n++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return View(pinlist_data);
        }

        // Add | Update Category
        [HttpPost]
        public ActionResult SavePincode(Pincode_Modal data)
        {
            try
            {
                // Step 1: Check if LocationCode already exists
                string checkQuery = data.PinID == 0
                    ? "SELECT LocationCode FROM PinMaster WHERE LocationCode = '" + data.LocationCode + "'"
                    : "SELECT LocationCode FROM PinMaster WHERE LocationCode = '" + data.LocationCode + "' AND PinID <> " + data.PinID;

                string existingCode = Database.getValue(checkQuery); // returns null or empty if not found

                if (!string.IsNullOrEmpty(existingCode))
                {
                    TempData["ErrorMessage"] = "Location code already exists!";
                    return RedirectToAction("PinCodeList");
                }

                // Step 2: Insert or Update
                string query = "";
                if (data.PinID == 0)
                {
                    query = "INSERT INTO PinMaster (LocationName, LocationCode, IsActive) " +
                            "VALUES ('" + data.LocationName + "', '" + data.LocationCode + "', 1)";
                }
                else
                {
                    query = "UPDATE PinMaster SET " +
                            "LocationName = '" + data.LocationName + "', " +
                            "LocationCode = '" + data.LocationCode + "' " +
                            "WHERE PinID = " + data.PinID;
                }

                string output = Database.insertData(query);

                if (output != "200")
                {
                    TempData["ErrorMessage"] = "An error occurred, please try again!";
                }
                else
                {
                    TempData["SuccessMessage"] = data.PinID == 0
                        ? "Location has been added successfully!"
                        : "Location details updated successfully!";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occurred, please try again!";
            }

            return RedirectToAction("PinCodeList");
        }

        public JsonResult GetCategorys()
        {
            List<Category_Model> categoryData = new List<Category_Model>();
            try
            {
                string query = "select * from [PinMaster] order by 2";

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    categoryData = new List<Category_Model>();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    Category_Model category = new Category_Model();
                    category.CategoryID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    category.CategoryName = dt.Rows[n][1].ToString();
                    category.IsActive = dt.Rows[n][2].ToString();
                    categoryData.Add(category);
                    n++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return Json(categoryData, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CheckCategoryIsExist(string CategoryName)
        {
            string response = "";
            try
            {
                if (!string.IsNullOrEmpty(CategoryName))
                {
                    string output = Database.getValue("SELECT CategoryName,IsActive FROM [CategoryMaster] WHERE CategoryName = '" + CategoryName + "' ");
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

        public JsonResult ManageLocation(int PinID, string Status)
        {
            try
            {
                DateTime EntryDate = datetime;
                string query, output;

                if (PinID != 0)
                {
                    if (Status.Equals("1"))
                    {
                        query = "update PinMaster set IsActive =1 Where PinID=" + PinID;
                    }
                    else
                    {
                        query = "update PinMaster set IsActive = 0 Where PinID=" + PinID;
                    }
                    output = Database.insertData(query);

                    if (output == "200")
                    {
                        if (Status.Equals("1"))
                        {
                            return Json("PinCode Active successfully", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json("PinCode InActive successfully", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion




        [HttpGet]
        public JsonResult DeleteLocation(int PinID = 0)
        {
            string response = "";
            try
            {
                string output = Database.insertData("Delete from PinMaster where PinID =" + PinID);
                if (output != "200")
                {
                    response = "An error occured please try again !!";
                }
                else
                {
                    response = "PincCode deleted Sucessfully !!";
                }
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }
    }



}
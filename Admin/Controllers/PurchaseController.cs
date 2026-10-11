using Admin.Helpers;
using Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace Admin.Controllers
{
    public class PurchaseController : Controller
    {
        public static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), INDIAN_ZONE);

        [HttpGet]
        public ActionResult PurchaseList()
        {
            try
            {
                string query = "SELECT * FROM [ITEM_PURCHASE]";

                List<Purchase> purchaseList = new List<Purchase>();

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    purchaseList = new List<Purchase>();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    Purchase purchase = new Purchase();
                    purchase.PurchaseID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    purchase.ItemID = ((dt.Rows[n][1] == null) ? 0 : Convert.ToInt32(dt.Rows[n][1]));
                    purchase.ItemName = Database.getValue("select ItemName from ITEMMASTER Where ItemID=" + purchase.ItemID);
                    purchase.UOMID = ((dt.Rows[n][2] == null) ? 0 : Convert.ToInt32(dt.Rows[n][2]));
                    purchase.UOMName = Database.getValue("SELECT Unit FROM UNIT WHERE UnitID = " + purchase.UOMID + "");
                    purchase.Weight = ((dt.Rows[n][3] == null) ? 0 : Convert.ToDecimal(dt.Rows[n][3]));
                    purchase.Price = ((dt.Rows[n][4] == null) ? 0 : Convert.ToInt32(dt.Rows[n][4]));
                    purchase.GivenDate = (Convert.ToDateTime(dt.Rows[n][5])).ToShortDateString();

                    purchaseList.Add(purchase);
                    n++;
                }

                if (purchaseList.Count > 0)
                {
                    purchaseList = purchaseList.OrderBy(a => a.ItemName).ToList();
                }

                return View(purchaseList);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = "An error occured please try again !!";
                return RedirectToAction("PurchaseList");
            }

        }

        [HttpGet]
        public ActionResult ManagePurchase(int PurchaseID = 0)
        {
            try
            {
                string query = "SELECT * FROM [ITEM_PURCHASE] WHERE PURCHASEID=" + PurchaseID;

                Purchase purchaseData = new Purchase();

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    purchaseData = new Purchase();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    purchaseData.PurchaseID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    purchaseData.ItemID = ((dt.Rows[n][1] == null) ? 0 : Convert.ToInt32(dt.Rows[n][1]));
                    purchaseData.ItemName = Database.getValue("select ItemName from ITEMMASTER Where ItemID=" + purchaseData.ItemID);
                    purchaseData.UOMID = ((dt.Rows[n][2] == null) ? 0 : Convert.ToInt32(dt.Rows[n][2]));
                    purchaseData.UOMName = Database.getValue("SELECT Unit FROM UNIT WHERE UnitID = " + purchaseData.UOMID + "");
                    purchaseData.Weight = ((dt.Rows[n][3] == null) ? 0 : Convert.ToInt32(dt.Rows[n][3]));
                    purchaseData.Price = ((dt.Rows[n][4] == null) ? 0 : Convert.ToInt32(dt.Rows[n][4]));
                    DateTime EntryDate = Convert.ToDateTime(dt.Rows[n][5]);
                    purchaseData.EntryDate = CommonHelper.ConvertDateToString(EntryDate);
                    n++;
                }

                return View(purchaseData);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = "An error occured please try again !!";
                return RedirectToAction("PurchaseList");
            }

        }

        [HttpPost]
        public ActionResult SavePurchase(Purchase dataString)
        {
            try
            {
                int PurchaseID = dataString.PurchaseID, ItemID = dataString.ItemID;
                decimal Weight = dataString.Weight, Price = dataString.Price, AvailableBalance = 0, PerviousPrice = 0;
                DateTime? GivenDate;
                var Pervious_Price = "";
                if (!string.IsNullOrEmpty(dataString.EntryDate))
                {
                    DateTime actionDate = DateTime.ParseExact(dataString.EntryDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    GivenDate = actionDate;
                }
                else
                {
                    GivenDate = datetime;
                }

                string query = "";

                var existQuery = "select PurchaseID from Item_Purchase Where PurchaseID=" + PurchaseID + " and ItemID = " + ItemID;

                var isExist = Database.getValue(existQuery);

                if (!string.IsNullOrEmpty(isExist))
                {
                    PurchaseID = Convert.ToInt32(isExist);
                }

                if (PurchaseID == 0)
                {
                    query = "insert into Item_Purchase values (" + ItemID + "," + dataString.UOMID + "," + Weight + "," + Price + ",'" + GivenDate + "',1)";
                }

                else
                {
                    Pervious_Price = Database.getValue("select Price from Item_Purchase Where PurchaseID=" + PurchaseID + " ");
                    query = "update Item_Purchase set ItemID = " + ItemID + ",UnitID = " + dataString.UOMID + ", Weight ='" + Weight + "', Price = '" + Price + "', EntryDate = '" + GivenDate + "' Where PurchaseID=" + PurchaseID;
                }
                string output = Database.insertData(query);
                if (output != "200")
                {
                    TempData["ErrorMessage"] = "An error occured please try again !!";
                }
                else
                {
                    if (Price > 0)
                    {
                        var exsitBalance = Database.getValue("select balance from Balance");

                        if (!string.IsNullOrEmpty(exsitBalance))
                        {
                            AvailableBalance = Convert.ToDecimal(exsitBalance);
                        }
                        if (AvailableBalance > 0)
                        {
                            if (PurchaseID > 0 && Price > 0)
                            {
                                if (!string.IsNullOrEmpty(Pervious_Price))
                                {
                                    PerviousPrice = Convert.ToDecimal(Pervious_Price);
                                    AvailableBalance = AvailableBalance + PerviousPrice;
                                    query = "update Balance set balance=" + AvailableBalance;
                                    output = Database.insertData(query);
                                    if (output != "200")
                                    {
                                        TempData["ErrorMessage"] = "An error occured please try again !!";
                                    }
                                    exsitBalance = Database.getValue("select balance from Balance");
                                    if (!string.IsNullOrEmpty(exsitBalance))
                                    {
                                        AvailableBalance = Convert.ToDecimal(exsitBalance);
                                    }
                                }
                            }

                            AvailableBalance = AvailableBalance - Price;
                            query = "update Balance set balance=" + AvailableBalance;
                            output = Database.insertData(query);
                            if (output != "200")
                            {
                                TempData["ErrorMessage"] = "An error occured please try again !!";
                            }
                            query = "insert into Balanace_Transaction values(0, '" + (Price * Weight) + "', '" + AvailableBalance + "','" + GivenDate + "','Purchase Department')";
                            output = Database.insertData(query);
                            if (output != "200")
                            {
                                TempData["ErrorMessage"] = "An error occured please try again !!";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return RedirectToAction("PurchaseList");
        }

        [HttpGet]
        public JsonResult GetItemUOMName(int ItemID)
        {
            string UOMName = "";
            try
            {
                if (ItemID > 0)
                {
                    string Unit = Database.getValue("SELECT UOM FROM ITEMMASTER WHERE ItemID = " + ItemID + "");
                    if (!string.IsNullOrEmpty(Unit))
                    {
                        int UOMID = Convert.ToInt32(Unit);
                        string UnitName = Database.getValue("SELECT Unit FROM Unit WHERE UnitID = " + UOMID + "");
                        if (!string.IsNullOrEmpty(UnitName))
                        {
                            UOMName = UnitName;
                        }
                    }
                }
                return Json(UOMName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                return Json(UOMName, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult DeletePurchase(int PurchaseID = 0)
        {
            string response = "";
            decimal AvailableBalance = 0;
            try
            {
                var balance = Database.insertData("Select (Price * Weight) as Total from Item_Purchase where PurchaseID =" + PurchaseID);
                string output = Database.insertData("Delete from Item_Purchase where PurchaseID =" + PurchaseID);
                if (output != "200")
                {
                    response = "An error occured please try again !!";
                }
                else
                {
                    var exsitBalance = Database.getValue("select balance from Balance");
                    if (!string.IsNullOrEmpty(exsitBalance))
                    {
                        AvailableBalance = Convert.ToDecimal(exsitBalance);
                    }
                    AvailableBalance = AvailableBalance + Convert.ToDecimal(balance);
                    string query = "update Balance set balance=" + AvailableBalance;
                    output = Database.insertData(query);
                    if (output != "200")
                    {
                        TempData["ErrorMessage"] = "An error occured please try again !!";
                    }
                    response = "Purchase details deleted Sucessfully !!";
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
    }
}
using Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace Admin.Controllers
{
    public class BalanceController : Controller
    {
        [HttpGet]
        public ActionResult GetBalanceList()
        {
            Balance_Model balanceData = new Balance_Model();
            balanceData.balanceList = new List<BalanceList>();

            balanceData.GivenDate = null;
            try
            {
                var exsitBalance = Database.getValue("select balance from Balance");
                if (!string.IsNullOrEmpty(exsitBalance))
                {
                    balanceData.AvailableBalance += Convert.ToDecimal(exsitBalance);
                }

                string query = "select * from Balanace_Transaction Order by 1 DESC";

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    balanceData.balanceList = new List<BalanceList>();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    BalanceList balance = new BalanceList();
                    balance.PayID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    balance.BalanceAmt = ((dt.Rows[n][2] == null) ? 0 : Convert.ToInt32(dt.Rows[n][2]));
                    if (dt.Rows[n][4] != null)
                    {
                        balance.GivenDate = Convert.ToDateTime(dt.Rows[n][4]);
                    }
                    balance.GivenBy = dt.Rows[n][5].ToString();
                    balanceData.balanceList.Add(balance);
                    n++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return View(balanceData);
        }

        // Add | Update Balance
        [HttpPost]
        public ActionResult SaveBalance(Balance_Model dataString)
        {
            try
            {
                int PayID = dataString.PayID;
                decimal Balance = dataString.BalanceAmt, AvailableBalance = 0;
                string GivenBy = dataString.GivenBy, exsitBalance;
                DateTime? GivenDate = dataString.GivenDate;

                TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), INDIAN_ZONE);

                if (GivenDate == null)
                {
                    GivenDate = datetime;
                }

                // check available balance
                exsitBalance = Database.getValue("select balance from Balance");
                if (!string.IsNullOrEmpty(exsitBalance))
                {
                    AvailableBalance = Convert.ToDecimal(exsitBalance) + Balance;
                }

                string query = "";
                // table Balance
                if (AvailableBalance == 0)
                {
                    query = "insert into Balance values('" + Balance + "', '" + GivenBy + "', '" + GivenDate + "', '" + datetime + "',null)";
                }
                else
                {
                    query = "update Balance set Balance = '" + AvailableBalance + "', GivenBy =  '" + GivenBy + "', GivenDate= '" + GivenDate + "', UpdatedDate='" + datetime + "'";
                }

                string output = Database.insertData(query);
                if (output != "200")
                {
                    TempData["ErrorMessage"] = "An error occured please try again !!";
                    return RedirectToAction("GetBalanceList");
                }
                else
                {
                    if (PayID == 0)
                    {
                        string pID = Database.getValue("select max(payID) from balance");
                        PayID = (!string.IsNullOrEmpty(pID)) ? Convert.ToInt32(pID) : 0;
                    }
                    // table Balanace Transaction
                    if (PayID > 0)
                    {
                        query = "insert into Balanace_Transaction values(" + PayID + ", '" + Balance + "', '" + AvailableBalance + "','" + GivenDate + "','" + GivenBy + "')";
                        output = Database.insertData(query);
                        if (output != "200")
                        {
                            TempData["ErrorMessage"] = "An error occured please try again !!";
                            return RedirectToAction("GetBalanceList");
                        }
                    }
                    TempData["SuccessMessage"] = "Balance added Sucessfully !!";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
                return RedirectToAction("GetBalanceList");
            }
            return RedirectToAction("GetBalanceList");
        }


        public JsonResult DeleteBalance(int PayID = 0)
        {
            var response = "";
            try
            {
                string output = Database.insertData("Delete from Balance where PayID = " + PayID);
                if (output != "200")
                {
                    TempData["ErrorMessage"] = "An error occured please try again !!";
                }
                response = "Balance deleted Sucessfully !!";
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                response = "An error occured please try again !!";
                return Json(response, JsonRequestBehavior.AllowGet);

            }
        }

    }
}

using Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Admin.Controllers
{
    public class ManageUnitController : Controller
    {
        #region Unit

        [HttpGet]
        public ActionResult UnitList()
        {
            Unit_Model unitData = new Unit_Model();
            unitData._unitList = new List<Unit_Model>();

            try
            {
                string query = "select * from [Unit] Order by unit ";

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    unitData._unitList = new List<Unit_Model>();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    Unit_Model unit = new Unit_Model();
                    unit.UnitID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    unit.UnitName = dt.Rows[n][1].ToString();
                    unitData._unitList.Add(unit);
                    n++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return View(unitData);
        }

        // Add | Update Unit
        [HttpPost]
        public ActionResult SaveUnit(Unit_Model dataString)
        {
            try
            {

                string query = "";
                // table Balance
                if (dataString.UnitID == 0)
                {
                    query = "insert into Unit values('" + dataString.UnitName + "')";
                }
                else
                {
                    query = "update Unit set Unit = '" + dataString.UnitName + "' Where UnitID=" + dataString.UnitID;
                }

                string output = Database.insertData(query);
                if (output != "200")
                {
                    TempData["ErrorMessage"] = "An error occured please try again !!";
                    return RedirectToAction("UnitList");
                }
                else
                {
                    if (dataString.UnitID == 0)
                    {
                        TempData["SuccessMessage"] = "Unit has been added Sucessfully !!";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Unit detail updated Sucessfully !!";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
                return RedirectToAction("UnitList");
            }
            return RedirectToAction("UnitList");
        }

        public JsonResult GetUnitName()
        {
            List<Unit_Model> unitData = new List<Unit_Model>();
            try
            {
                string query = "select * from [Unit]";

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    unitData = new List<Unit_Model>();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    Unit_Model unit = new Unit_Model();
                    unit.UnitID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    unit.UnitName = dt.Rows[n][1].ToString();
                    unitData.Add(unit);
                    n++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return Json(unitData, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CheckUnitNameIsExist(string UnitName)
        {
            string response = "";
            try
            {
                if (!string.IsNullOrEmpty(UnitName))
                {
                    string output = Database.getValue("SELECT Unit FROM Unit WHERE Unit = '" + UnitName + "' ");
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

        #endregion
    }
}
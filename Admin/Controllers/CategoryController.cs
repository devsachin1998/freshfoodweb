using Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace Admin.Controllers
{
    public class CategoryController : Controller
    {
        public static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), INDIAN_ZONE);

        #region Category

        [HttpGet]
        public ActionResult CategoryList()
        {
            Category_Model categoryData = new Category_Model();
            categoryData._categoryList = new List<Category_Model>();

            try
            {
                string query = "select * from [CategoryMaster] order by 2 ";

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    categoryData._categoryList = new List<Category_Model>();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    Category_Model category = new Category_Model();
                    category.CategoryID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    category.CategoryName = dt.Rows[n][1].ToString();
                    category.IsActive = dt.Rows[n][2].ToString();
                    categoryData._categoryList.Add(category);
                    n++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return View(categoryData);
        }

        // Add | Update Category
        [HttpPost]
        public ActionResult SaveCategory(Category_Model data)
        {
            try
            {

                string query = "";
                // table Balance
                if (data.CategoryID == 0)
                {
                    query = "insert into CategoryMaster values('" + data.CategoryName + "',1)";
                }
                else
                {
                    query = "update CategoryMaster set CategoryName = '" + data.CategoryName + "' Where CategoryID=" + data.CategoryID;
                }

                string output = Database.insertData(query);
                if (output != "200")
                {
                    TempData["ErrorMessage"] = "An error occured please try again !!";
                    return RedirectToAction("CategoryList");
                }
                else
                {
                    if (data.CategoryID == 0)
                    {
                        TempData["SuccessMessage"] = "Category has been added Sucessfully !!";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Category detail updated Sucessfully !!";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
                return RedirectToAction("CategoryList");
            }
            return RedirectToAction("CategoryList");
        }

        public JsonResult GetCategorys()
        {
            List<Category_Model> categoryData = new List<Category_Model>();
            try
            {
                string query = "select * from [CategoryMaster] order by 2";

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

        public JsonResult ManageCategory(int Cat_Id, string Status)
        {
            try
            {
                DateTime EntryDate = datetime;
                string query, output;

                if (Cat_Id != 0)
                {
                    if (Status.Equals("1"))
                    {
                        query = "update CategoryMaster set IsActive =1 Where CategoryID=" + Cat_Id;
                    }
                    else
                    {
                        query = "update CategoryMaster set IsActive = 0 Where CategoryID=" + Cat_Id;
                    }
                    output = Database.insertData(query);

                    if (output == "200")
                    {
                        if (Status.Equals("1"))
                        {
                            return Json("Category Active successfully", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json("Category InActive successfully", JsonRequestBehavior.AllowGet);
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
        public JsonResult DeleteCategory(int CategoryId = 0)
        {
            string response = "";
            try
            {
                string output = Database.insertData("Delete from CategoryMaster where CategoryId =" + CategoryId);
                if (output != "200")
                {
                    response = "An error occured please try again !!";
                }
                else
                {
                    response = "Category deleted Sucessfully !!";
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
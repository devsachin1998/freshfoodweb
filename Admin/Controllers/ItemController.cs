using Admin.Helpers;
using Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;


namespace Admin.Controllers
{
    public class ItemController : Controller
    {
        [HttpGet]
        public ActionResult ItemList(int CategoryID = 0)
        {
            string query = "";
            try
            {
                if(CategoryID == 0)
                {
                    query = "select * from ItemMaster Order by ItemName";
                }
                else
                {
                    query = "select * from ItemMaster where CategoryID='"+ CategoryID + "' Order by ItemName";
                }
                 

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
                    itemData.CategoryName = Database.getValue("select CategoryName from CategoryMaster Where CategoryID=" + itemData.CategoryID);
                    itemData.ImagePath = dt.Rows[n][3].ToString();

                    #region Sub Item

                    itemData._subitemList = new List<SubItem>();

                    string itemQuery = "SELECT * FROM SUBITEM WHERE ITEMID =" + itemData.ItemID;

                    DataSet item_ds = Database.getDataSet(itemQuery);
                    DataTable item_dt = item_ds.Tables[0];
                    if (item_ds == null || item_dt.Rows.Count == 0)
                    {
                        itemData._subitemList = new List<SubItem>();
                    }

                    int i = 0;
                    int item_count = item_dt.Rows.Count;

                    while (i < item_count)
                    {
                        SubItem subItemData = new SubItem();

                        subItemData.SubItemID = Convert.ToInt32(item_dt.Rows[i][0]);
                        subItemData.ItemID = Convert.ToInt32(item_dt.Rows[i][1]);
                        subItemData.UOMID = Convert.ToInt32(item_dt.Rows[i][2]);
                        subItemData.UOMName = item_dt.Rows[i][3].ToString();
                        subItemData.Weight = item_dt.Rows[i][4].ToString();

                        itemData._subitemList.Add(subItemData);

                        i++;
                    }

                    #endregion

                    ItemList.Add(itemData);
                    n++;
                }


                return View(ItemList);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = "An error occured please try again !!";
                return RedirectToAction("ItemList");
            }

        }

        [HttpGet]
        public ActionResult AddItem(int ItemID = 0)
        {
            try
            {
                string query = "select * from ItemMaster Where ItemID =" + ItemID;

                Item ItemDetail = new Item();

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    ItemDetail = new Item();
                    ItemDetail._subitemList = new List<SubItem>();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    ItemDetail.ItemID = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    ItemDetail.ItemName = dt.Rows[n][1].ToString();
                    ItemDetail.CategoryID = ((dt.Rows[n][2] == null) ? 0 : Convert.ToInt32(dt.Rows[n][2]));
                    ItemDetail.CategoryName = Database.getValue("select CategoryName from CategoryMaster Where CategoryID=" + ItemDetail.CategoryID);
                    ItemDetail.ImagePath = dt.Rows[n][3].ToString();

                    #region Sub Item

                    ItemDetail._subitemList = new List<SubItem>();

                    string itemQuery = "SELECT * FROM SUBITEM WHERE ITEMID =" + ItemID;

                    DataSet item_ds = Database.getDataSet(itemQuery);
                    DataTable item_dt = item_ds.Tables[0];
                    if (item_ds == null || item_dt.Rows.Count == 0)
                    {
                        ItemDetail._subitemList = new List<SubItem>();
                    }

                    int i = 0;
                    int item_count = item_dt.Rows.Count;

                    while (i < item_count)
                    {
                        SubItem subItemData = new SubItem();

                        subItemData.SubItemID = Convert.ToInt32(item_dt.Rows[i][0]);
                        subItemData.ItemID = Convert.ToInt32(item_dt.Rows[i][1]);
                        subItemData.UOMID = Convert.ToInt32(item_dt.Rows[i][2]);
                        subItemData.UOMName = item_dt.Rows[i][3].ToString();
                        subItemData.Weight = item_dt.Rows[i][4].ToString();

                        ItemDetail._subitemList.Add(subItemData);

                        i++;
                    }

                    #endregion

                    n++;
                }

                return View(ItemDetail);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = "An error occured please try again !!";
                return RedirectToAction("ItemList");
            }

        }

        [HttpPost]
        public ActionResult SaveItem(Item dataString)
        {
            try
            {
                TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), INDIAN_ZONE);

                int ItemID = dataString.ItemID, UOMID = dataString.UOMID, CategoryID = dataString.CategoryID;
                string ItemName = dataString.ItemName, ImagePath = "", EntryBy = dataString.EntryBy;

                #region Upload Image

                if (dataString.ImagePath_File != null)
                {
                    var fileName = Path.GetExtension(dataString.ImagePath_File.FileName);
                    Guid guid = Guid.NewGuid();
                    ImagePath = guid + fileName;
                    var filepath = Path.GetFullPath(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/ItemImages/" + ImagePath));
                    dataString.ImagePath_File.SaveAs(filepath);
                }

                #endregion

                string query = "";
                if (ItemID == 0)
                {
                    query = "insert into ItemMaster values ('" + ItemName + "'," + CategoryID + ",'" + ImagePath + "','" + datetime + "',null , '" + EntryBy + "')";

                    TempData["SuccessMessage"] = "New item added Sucessfully !!";
                }
                else
                {
                    if (!string.IsNullOrEmpty(ImagePath))
                    {
                        query = "update ItemMaster set ItemName ='" + ItemName + "', CategoryID = " + CategoryID + ",  ImagePath = '" + ImagePath + "', UpdatedDate = '" + datetime + "' Where ItemID=" + ItemID;
                    }
                    else
                    {
                        query = "update ItemMaster set ItemName ='" + ItemName + "', CategoryID = " + CategoryID + ", UpdatedDate = '" + datetime + "' Where ItemID=" + ItemID;
                    }
                    TempData["SuccessMessage"] = "Item details updated Sucessfully !!";
                }
                string output = Database.insertData(query);
                if (output != "200")
                {
                    TempData["ErrorMessage"] = "An error occured please try again !!";
                }
                else
                {
                    #region Manage Item

                    if (dataString.ItemID == 0)
                    {
                        var itemID = Database.getValue("SELECT MAX(ITEMID) FROM [ITEMMASTER]");
                        if (!string.IsNullOrEmpty(itemID))
                        {
                            dataString.ItemID = Convert.ToInt32(itemID);
                        }
                    }

                    foreach (var item in dataString._subitemList)
                    {
                        var subItemID = Database.getValue("SELECT SubItemID FROM SubItem WHERE SubItemID = " + item.SubItemID);
                        if (!string.IsNullOrEmpty(subItemID))
                        {
                            item.SubItemID = Convert.ToInt32(subItemID);
                            if (!string.IsNullOrEmpty(item.Weight))
                            {
                                item.UOMName = Database.getValue("SELECT Unit FROM Unit WHERE UnitID = " + item.UOMID);

                                query = "Update SubItem Set UOMID= " + item.UOMID + ", UOMName = '" + item.UOMName + "', WeightType = '" + item.Weight + "', UpdatedDate = '" + datetime + "' Where SubItemID = " + item.SubItemID;
                                output = Database.insertData(query);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(item.Weight))
                            {
                                item.UOMName = Database.getValue("SELECT Unit FROM Unit WHERE UnitID = " + item.UOMID);

                                query = "INSERT INTO SubItem VALUES(" + dataString.ItemID + ", " + item.UOMID + ", '" + item.UOMName + "', '" + item.Weight + "','" + datetime + "',null)";
                                output = Database.insertData(query);
                            }
                        }
                        if (string.IsNullOrEmpty(item.Weight) && item.UOMID == 0)
                        {
                            output = Database.insertData("Delete from SubItem where SubItemID =" + item.SubItemID);
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return RedirectToAction("ItemList");
        }

        [HttpGet]
        public JsonResult CheckItemIsExist(string ItemName)
        {
            string response = "";
            try
            {
                if (!string.IsNullOrEmpty(ItemName))
                {
                    string output = Database.getValue("SELECT ITEMNAME FROM ITEMMASTER WHERE ITEMNAME = '" + ItemName + "' ");
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
        public JsonResult DeleteItem(int ItemID = 0)
        {
            string response = "";
            try
            {
                string output = Database.insertData("Delete from ItemMaster where ItemID =" + ItemID);
                if (output != "200")
                {
                    response = "An error occured please try again !!";
                }
                else
                {
                    response = "Item deleted Sucessfully !!";
                }
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetUOMList()
        {
            var uomList = CommonHelper.GetUnit();
            return Json(uomList, JsonRequestBehavior.AllowGet);
        }
    }
}
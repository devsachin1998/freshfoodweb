using Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace Admin.Controllers
{
    public class OfferController : Controller
    {
        public static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime datetime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), INDIAN_ZONE);

        #region Offer

        [HttpGet]
        public ActionResult OfferMaster()
        {
            Offer_Model OfferData = new Offer_Model();
            OfferData._OfferList = new List<Offer_Model>();

            try
            {
                string query = "select * from [OfferMaster] order by 2 ";

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    OfferData._OfferList = new List<Offer_Model>();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    Offer_Model Offer = new Offer_Model();
                    Offer.OfferId = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    Offer.OfferName = dt.Rows[n][1].ToString();
                    Offer.IsActive = dt.Rows[n][2].ToString();
                    Offer.Date = dt.Rows[n][3].ToString();
                    OfferData._OfferList.Add(Offer);
                    n++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return View(OfferData);
        }

        // Add | Update Offer

        [HttpPost]
        public ActionResult SaveOffer(Offer_Model data)
        {
            try
            {

                string query = "";
                // table Balance
                if (data.OfferId == 0)
                {
                    query = "insert into OfferMaster values('" + data.OfferName + "',1,'" + datetime + "')";
                }
                else
                {
                    query = "update OfferMaster set OfferName = '" + data.OfferName + "' Where OfferId=" + data.OfferId;
                }

                string output = Database.insertData(query);
                if (output != "200")
                {
                    TempData["ErrorMessage"] = "An error occured please try again !!";
                    return RedirectToAction("OfferMaster");
                }
                else
                {
                    if (data.OfferId == 0)
                    {
                        TempData["SuccessMessage"] = "Offer has been added Sucessfully !!";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Offer detail updated Sucessfully !!";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
                return RedirectToAction("OfferMaster");
            }
            return RedirectToAction("OfferMaster");
        }

        public JsonResult GetOffers()
        {
            List<Offer_Model> OfferData = new List<Offer_Model>();
            try
            {
                string query = "select * from [OfferMaster] order by 2";

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    OfferData = new List<Offer_Model>();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    Offer_Model Offer = new Offer_Model();
                    Offer.OfferId = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    Offer.OfferName = dt.Rows[n][1].ToString();
                    Offer.IsActive = dt.Rows[n][2].ToString();
                    Offer.Date = dt.Rows[n][3].ToString();
                    OfferData.Add(Offer);
                    n++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return Json(OfferData, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CheckOfferIsExist(string OfferName)
        {
            string response = "";
            try
            {
                if (!string.IsNullOrEmpty(OfferName))
                {
                    string output = Database.getValue("SELECT OfferName,IsActive FROM [OfferMaster] WHERE OfferName = '" + OfferName + "' ");
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

        public JsonResult ManageOffer(int Off_Id, string Status)
        {
            try
            {
                DateTime EntryDate = datetime;
                string query, output;

                if (Off_Id != 0)
                {
                    if (Status.Equals("1"))
                    {
                        query = "update OfferMaster set IsActive =1 Where OfferId=" + Off_Id;
                    }
                    else
                    {
                        query = "update OfferMaster set IsActive = 0 Where OfferId=" + Off_Id;
                    }
                    output = Database.insertData(query);

                    if (output == "200")
                    {
                        if (Status.Equals("1"))
                        {
                            return Json("Offer Active successfully", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json("Offer InActive successfully", JsonRequestBehavior.AllowGet);
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
        public JsonResult DeleteOffer(int OfferId = 0)
        {
            string response = "";
            try
            {
                string output = Database.insertData("Delete from OfferMaster where OfferId =" + OfferId);
                if (output != "200")
                {
                    response = "An error occured please try again !!";
                }
                else
                {
                    response = "Offer deleted Sucessfully !!";
                }
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpGet]
        public ActionResult AddOffer()
        {
            Offer_Model offerData = new Offer_Model();
            offerData._OfferDetails = new List<Offer_details>();

            try
            {
                string query = "select * from [Offer_details] order by 2 ";

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    offerData._OfferDetails = new List<Offer_details>();
                }
                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    Offer_details Offer = new Offer_details();
                    Offer.Id = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    Offer.OfferName = dt.Rows[n][1].ToString();
                    Offer.Type = dt.Rows[n][2].ToString();
                    Offer.MinimumAmont = dt.Rows[n][3].ToString();
                    Offer.ApplicableCashBack = dt.Rows[n][4].ToString();
                    Offer.Amount = dt.Rows[n][5].ToString();
                    Offer.TotalCashBack = dt.Rows[n][6].ToString();
                    Offer.CreatedDate = dt.Rows[n][7].ToString();
                    Offer.UpdatedDate = dt.Rows[n][8].ToString();
                    Offer.IsActive = dt.Rows[n][9].ToString();
                    offerData._OfferDetails.Add(Offer);
                    n++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return View(offerData);

        }


        // Add | Update Offer
        [HttpPost]
        public ActionResult SaveOffers(Offer_details data)
        {
            try
            {
                string query = "";
                if (data.Id == 0)
                {
                    query = "insert into Offer_details values('" + data.OfferName + "','" + data.Type + "','" + data.MinimumAmont + "','" + data.ApplicableCashBack + "','" + data.Amount + "', '" + data.TotalCashBack + "', '" + data.CreatedDate + "','" + data.UpdatedDate + "',1)";
                    // Id OfferName   Type MinimumAmont    ApplicableCashBack Amount  TotalCashBack CreatedDate UpdatedDate IsActive
                }
                else
                {
                    query = "update Offer_details set OfferName = '" + data.OfferName + "' Where Id=" + data.Id;
                }

                string output = Database.insertData(query);
                if (output != "200")
                {
                    TempData["ErrorMessage"] = "An error occured please try again !!";
                    return RedirectToAction("AddOffer");
                }
                else
                {
                    if (data.Id == 0)
                    {
                        TempData["SuccessMessage"] = "Offer has been added Sucessfully !!";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Offer detail updated Sucessfully !!";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
                return RedirectToAction("AddOffer");
            }
            return RedirectToAction("AddOffer");
        }

        public JsonResult GetOffersdata()
        {
            List<Offer_details> OfferData = new List<Offer_details>();
            try
            {
                string query = "select * from [Offer_details] order by 2 ";

                DataSet ds = Database.getDataSet(query);
                DataTable dt = ds.Tables[0];
                if (ds == null || dt.Rows.Count == 0)
                {
                    OfferData = new List<Offer_details>();
                }

                int n = 0;
                int count = dt.Rows.Count;
                while (n < count)
                {
                    Offer_details Offer = new Offer_details();
                    Offer.Id = ((dt.Rows[n][0] == null) ? 0 : Convert.ToInt32(dt.Rows[n][0]));
                    Offer.OfferName = dt.Rows[n][1].ToString();
                    Offer.Type = dt.Rows[n][2].ToString();
                    Offer.MinimumAmont = dt.Rows[n][3].ToString();
                    Offer.ApplicableCashBack = dt.Rows[n][4].ToString();
                    Offer.Amount = dt.Rows[n][5].ToString();
                    Offer.TotalCashBack = dt.Rows[n][6].ToString();
                    Offer.CreatedDate = dt.Rows[n][7].ToString();
                    Offer.UpdatedDate = dt.Rows[n][8].ToString();
                    Offer.IsActive = dt.Rows[n][9].ToString();
                    OfferData.Add(Offer);
                    n++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TempData["ErrorMessage"] = "An error occured please try again !!";
            }
            return Json(OfferData, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ManageOfferData(int OffId, string Status)
        {
            try
            {
                DateTime EntryDate = datetime;
                string query, output;

                if (OffId != 0)
                {
                    if (Status.Equals("1"))
                    {
                        query = "update Offer_details set IsActive =1 Where Id=" + OffId;
                    }
                    else
                    {
                        query = "update Offer_details set IsActive = 0 Where Id=" + OffId;
                    }
                    output = Database.insertData(query);

                    if (output == "200")
                    {
                        if (Status.Equals("1"))
                        {
                            return Json("Offer Active successfully", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json("Offer InActive successfully", JsonRequestBehavior.AllowGet);
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

        [HttpGet]

        public JsonResult DeleteOfferData(int Id = 0)
        {
            string response = "";
            try
            {
                string output = Database.insertData("Delete from Offer_details where Id =" + Id);
                if (output != "200")
                {
                    response = "An error occured please try again !!";
                }
                else
                {
                    response = "Offer deleted Sucessfully !!";
                }
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult CalculateOfferPrice(decimal MinimumAmont, string ApplicableCashBack, decimal Amount)
        {
            decimal TotalCashBack = 0;
            {
                if (ApplicableCashBack == "Value")
                {
                    TotalCashBack = Amount; 
                }
                else
                {
                   
                    TotalCashBack = ( Amount/MinimumAmont) * 100;
                }
            }
            return Json(TotalCashBack, JsonRequestBehavior.AllowGet);

        }
    }
}

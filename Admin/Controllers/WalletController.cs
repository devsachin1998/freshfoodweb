using Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Admin.Helpers;

namespace Admin.Controllers
{
    public class WalletController : Controller
    {
        private static readonly TimeZoneInfo INDIAN_ZONE =
            TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        private DateTime IndianDateTime
        {
            get
            {
                return TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    INDIAN_ZONE
                );
            }
        }

        #region Wallet Summary

        [HttpGet]
        public ActionResult TransactionHistory(string FromDate, string ToDate)
        {
            Wallet_Model reportList = new Wallet_Model
            {
                _transactionHistory_Models =
                    new List<TransactionHistory_Model>()
            };

            try
            {
                DateTime today = IndianDateTime.Date;

                DateTime fromDate = today;
                DateTime toDate = today;

                if (!string.IsNullOrEmpty(FromDate) &&
                    !string.IsNullOrEmpty(ToDate))
                {
                    fromDate = DateTime.ParseExact(
                        FromDate,
                        "dd/MM/yyyy",
                        CultureInfo.InvariantCulture
                    ).Date;

                    toDate = DateTime.ParseExact(
                        ToDate,
                        "dd/MM/yyyy",
                        CultureInfo.InvariantCulture
                    ).Date;
                }

                DateTime toDateExclusive =
                    toDate.AddDays(1);

                string fromDateSql =
                    fromDate.ToString("yyyy-MM-dd HH:mm:ss");

                string toDateSql =
                    toDateExclusive.ToString("yyyy-MM-dd HH:mm:ss");

                string query = @"
SELECT
    w.HistoryID,
    w.UserID,
    w.Amount,
    w.EntryDate,
    w.PaymentMode,
    r.Name AS UserName
FROM UserWalletHistory w
LEFT JOIN tbl_Register_User r
    ON r.UserID = w.UserID
WHERE w.Type = 'Credit'
  AND w.Status = 'Approved'
  AND w.Amount <> 0
  AND w.EntryDate >= '" + fromDateSql + @"'
  AND w.EntryDate < '" + toDateSql + @"'
ORDER BY w.EntryDate DESC";

                DataSet ds =
                    Database.getDataSet(query);

                if (ds != null &&
                    ds.Tables.Count > 0 &&
                    ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    foreach (DataRow row in dt.Rows)
                    {
                        TransactionHistory_Model transaction =
                            new TransactionHistory_Model();

                        transaction.TransactionId =
                            row["HistoryID"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(
                                    row["HistoryID"]);

                        transaction.CreatedDate =
                            row["EntryDate"] == DBNull.Value
                                ? ""
                                : row["EntryDate"].ToString();

                        transaction.UserId =
                            row["UserID"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(
                                    row["UserID"]);

                        transaction.OnlinePayUserName =
                            row["UserName"] == DBNull.Value
                                ? ""
                                : row["UserName"].ToString();

                        transaction.OnlineAmount =
                            row["Amount"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(
                                    row["Amount"]);

                        transaction.PaymentMode =
                            row["PaymentMode"] == DBNull.Value
                                ? ""
                                : row["PaymentMode"].ToString();

                        reportList
                            ._transactionHistory_Models
                            .Add(transaction);
                    }
                }

                string totalQuery = @"
SELECT ISNULL(SUM(Amount), 0)
FROM UserWalletHistory
WHERE Type = 'Credit'
  AND Status = 'Approved'
  AND EntryDate >= '" + fromDateSql + @"'
  AND EntryDate < '" + toDateSql + "'";

                reportList.payTotal =
                    Database.getValue(totalQuery);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                TempData["ErrorMessage"] =
                    "An error occured please try again !!";

                return RedirectToAction(
                    "TransactionHistory");
            }

            return View(reportList);
        }

        [HttpGet]
        public ActionResult AddMoney()
        {
            Wallet_Model walletData =
                new Wallet_Model
                {
                    _walletHistory =
                        new List<Wallet_History>()
                };

            return View(walletData);
        }

        [HttpGet]
        public ActionResult WalletDetail()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetWalletDetails()
        {
            try
            {
                string query = @"
SELECT
    r.UserID,
    r.Name,
    LEFT(ISNULL(r.Address, ''), 38) AS Address,
    ISNULL(r.MobileNo, '') AS MobileNo,
    u.Balance,
    u.WalletID
FROM TBL_REGISTER_USER r
INNER JOIN
(
    SELECT
        UserID,
        MAX(WalletID) AS WalletID
    FROM UserWallet
    WHERE IsDisplay = 1
    GROUP BY UserID
) latest
    ON latest.UserID = r.UserID
INNER JOIN UserWallet u
    ON u.WalletID = latest.WalletID
WHERE r.ISACTIVE = 1
  AND ISNULL(u.Balance, 0) <> 0
ORDER BY r.Name";

                DataSet ds =
                    Database.getDataSet(query);

                var results =
                    new List<object>();

                if (ds != null &&
                    ds.Tables.Count > 0 &&
                    ds.Tables[0].Rows.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    foreach (DataRow row in dt.Rows)
                    {
                        int userId =
                            row["UserID"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(
                                    row["UserID"]);

                        string name =
                            row["Name"] == DBNull.Value
                                ? ""
                                : row["Name"].ToString();

                        string address =
                            row["Address"] == DBNull.Value
                                ? ""
                                : row["Address"].ToString();

                        string mobile =
                            row["MobileNo"] == DBNull.Value
                                ? ""
                                : row["MobileNo"].ToString();

                        int walletId =
                            row["WalletID"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(
                                    row["WalletID"]);

                        decimal balance =
                            row["Balance"] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(
                                    row["Balance"]);

                        results.Add(new
                        {
                            UserID = userId,
                            UserName = name,
                            Address = address,
                            Mobile = mobile,
                            WalletID = walletId,
                            Balance = balance
                        });
                    }
                }

                return Json(
                    new
                    {
                        success = true,
                        data = results
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Unable to load wallet details."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        #region Add Money AJAX

        [HttpGet]
        public JsonResult GetCustomers(string term = "")
        {
            try
            {
                term = (term ?? "").Trim();

                if (term.Length < 2)
                {
                    return Json(
                        new
                        {
                            results =
                                new List<object>()
                        },
                        JsonRequestBehavior.AllowGet
                    );
                }

                term =
                    term.Replace("'", "''");

                string where =
                    "r.ISACTIVE = 1";

                where +=
                    " AND (r.Name LIKE '%" +
                    term +
                    "%' " +
                    "OR CAST(r.UserID AS VARCHAR(20)) LIKE '%" +
                    term +
                    "%' " +
                    "OR r.MobileNo LIKE '%" +
                    term +
                    "%')";

                string query = @"
SELECT TOP 50
    r.UserID,
    r.Name,
    r.MobileNo
FROM TBL_REGISTER_USER r
WHERE " + where + @"
ORDER BY r.Name";

                DataSet ds =
                    Database.getDataSet(query);

                var results =
                    new List<object>();

                if (ds != null &&
                    ds.Tables.Count > 0 &&
                    ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        int userId =
                            row["UserID"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(
                                    row["UserID"]);

                        string name =
                            row["Name"] == DBNull.Value
                                ? ""
                                : row["Name"].ToString();

                        string mobile =
                            row["MobileNo"] == DBNull.Value
                                ? ""
                                : row["MobileNo"].ToString();

                        results.Add(new
                        {
                            id = userId,
                            text =
                                string.IsNullOrEmpty(mobile)
                                    ? name +
                                      " (ID: " +
                                      userId +
                                      ")"
                                    : name +
                                      " (" +
                                      mobile +
                                      ") - ID: " +
                                      userId
                        });
                    }
                }

                return Json(
                    new
                    {
                        results = results
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return Json(
                    new
                    {
                        results =
                            new List<object>(),
                        error =
                            "Unable to load customers."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        [HttpGet]
        public JsonResult GetCustomerWallet(int UserID)
        {
            try
            {
                if (UserID <= 0)
                {
                    return Json(
                        new
                        {
                            success = false,
                            message =
                                "Please select a customer."
                        },
                        JsonRequestBehavior.AllowGet
                    );
                }

                string query = @"
SELECT TOP 1
    r.UserID,
    r.Name,
    r.MobileNo,
    LEFT(r.Address, 38) AS Address,
    ISNULL(u.WalletID, 0) AS WalletID,
    ISNULL(u.Balance, 0) AS Balance
FROM TBL_REGISTER_USER r
LEFT JOIN UserWallet u
    ON u.UserID = r.UserID
    AND u.IsDisplay = 1
WHERE r.UserID = " + UserID + @"
  AND r.ISACTIVE = 1";

                DataSet ds =
                    Database.getDataSet(query);

                if (ds == null ||
                    ds.Tables.Count == 0 ||
                    ds.Tables[0].Rows.Count == 0)
                {
                    return Json(
                        new
                        {
                            success = false,
                            message =
                                "Customer not found."
                        },
                        JsonRequestBehavior.AllowGet
                    );
                }

                DataRow row =
                    ds.Tables[0].Rows[0];

                int userId =
                    Convert.ToInt32(
                        row["UserID"]);

                string name =
                    row["Name"] == DBNull.Value
                        ? ""
                        : row["Name"].ToString();

                string mobile =
                    row["MobileNo"] == DBNull.Value
                        ? ""
                        : row["MobileNo"].ToString();

                string address =
                    row["Address"] == DBNull.Value
                        ? ""
                        : row["Address"].ToString();

                int walletId =
                    row["WalletID"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(
                            row["WalletID"]);

                decimal balance =
                    row["Balance"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            row["Balance"]);

                return Json(
                    new
                    {
                        success = true,
                        UserID = userId,
                        UserName = name,
                        Mobile = mobile,
                        Address = address,
                        WalletID = walletId,
                        Balance = balance
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Unable to load customer wallet."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        #endregion

        [HttpGet]
        public ActionResult PaymentHistory(int UserID = 0)
        {
            Wallet_Model data =
                new Wallet_Model
                {
                    _walletHistory =
                        new List<Wallet_History>()
                };

            try
            {
                if (UserID == 0)
                {
                    return View(data);
                }

                string userQuery = @"
SELECT TOP 1
    u.WalletID,
    u.Balance,
    r.Name
FROM UserWallet u
LEFT JOIN tbl_Register_User r
    ON r.UserID = u.UserID
WHERE u.IsDisplay = 1
  AND u.UserID = " + UserID;

                DataSet userDs =
                    Database.getDataSet(userQuery);

                if (userDs == null ||
                    userDs.Tables.Count == 0 ||
                    userDs.Tables[0].Rows.Count == 0)
                {
                    return View(data);
                }

                DataRow userRow =
                    userDs.Tables[0].Rows[0];

                data.UserID = UserID;

                data.UserName =
                    userRow["Name"] == DBNull.Value
                        ? ""
                        : userRow["Name"].ToString();

                data.WalletBalance =
                    userRow["Balance"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            userRow["Balance"]);

                DateTime toDate =
                    IndianDateTime.Date.AddDays(1);

                DateTime fromDate =
                    toDate.AddMonths(-1);

                string fromDateSql =
                    fromDate.ToString(
                        "yyyy-MM-dd HH:mm:ss");

                string toDateSql =
                    toDate.ToString(
                        "yyyy-MM-dd HH:mm:ss");

                string historyQuery = @"
SELECT
    HistoryID,
    WalletID,
    UserID,
    Type,
    Status,
    Amount,
    Balance,
    OrderNo,
    EntryDate,
    ManageBy,
    PaymentMode,
    Payment_Status
FROM UserWalletHistory
WHERE UserID = " + UserID + @"
  AND EntryDate >= '" + fromDateSql + @"'
  AND EntryDate < '" + toDateSql + @"'
  AND Type IN ('Credit', 'Debit')
  AND Status IN ('Approved', 'Purchase')
ORDER BY HistoryID DESC";

                DataSet historyDs =
                    Database.getDataSet(historyQuery);

                if (historyDs == null ||
                    historyDs.Tables.Count == 0 ||
                    historyDs.Tables[0].Rows.Count == 0)
                {
                    return View(data);
                }

                DataTable historyDt =
                    historyDs.Tables[0];

                List<string> orderNumbers =
                    new List<string>();

                foreach (DataRow row in historyDt.Rows)
                {
                    string type =
                        row["Type"] == DBNull.Value
                            ? ""
                            : row["Type"].ToString();

                    string orderNo =
                        row["OrderNo"] == DBNull.Value
                            ? ""
                            : row["OrderNo"].ToString();

                    if (type == "Debit" &&
                        !string.IsNullOrWhiteSpace(orderNo))
                    {
                        if (!orderNumbers.Contains(orderNo))
                        {
                            orderNumbers.Add(orderNo);
                        }
                    }
                }

                Dictionary<string, Order_Model> orders =
                    new Dictionary<string, Order_Model>();

                if (orderNumbers.Count > 0)
                {
                    string orderInClause =
                        string.Join(
                            ",",
                            orderNumbers.Select(
                                x =>
                                    "'" +
                                    x.Replace("'", "''") +
                                    "'"
                            )
                        );

                    string orderQuery = @"
SELECT
    OrderID,
    OrderNo,
    OrderDate,
    UserID,
    CustomerName,
    CustomberMobile,
    CustomberEmail,
    CustomberAddress,
    DeliveryAddress,
    NoofItem,
    OrderStatus,
    PaymentStatus,
    CustomberRemark,
    AdminRemark,
    Total,
    EntryDate,
    UpdatedDate,
    SysDeliveryDate,
    PaidPayment,
    PaymentDate,
    DeliveryDate,
    DeliveryTime,
    BillingAmount
FROM OrderMaster
WHERE OrderNo IN (" +
                        orderInClause +
                        ")";

                    DataSet orderDs =
                        Database.getDataSet(orderQuery);

                    if (orderDs != null &&
                        orderDs.Tables.Count > 0 &&
                        orderDs.Tables[0].Rows.Count > 0)
                    {
                        DataTable orderDt =
                            orderDs.Tables[0];

                        foreach (DataRow row in orderDt.Rows)
                        {
                            Order_Model order =
                                new Order_Model();

                            order.OrderID =
                                row["OrderID"] == DBNull.Value
                                    ? 0
                                    : Convert.ToInt32(
                                        row["OrderID"]);

                            order.OrderNo =
                                row["OrderNo"] == DBNull.Value
                                    ? ""
                                    : row["OrderNo"].ToString();

                            order.OrderDate =
                                row["OrderDate"] == DBNull.Value
                                    ? ""
                                    : row["OrderDate"].ToString();

                            order.UserID =
                                row["UserID"] == DBNull.Value
                                    ? 0
                                    : Convert.ToInt32(
                                        row["UserID"]);

                            order.CustomerName =
                                row["CustomerName"] == DBNull.Value
                                    ? ""
                                    : row["CustomerName"].ToString();

                            order.CustomerMobile =
                                row["CustomberMobile"] == DBNull.Value
                                    ? ""
                                    : row["CustomberMobile"].ToString();

                            order.CustomerEmail =
                                row["CustomberEmail"] == DBNull.Value
                                    ? ""
                                    : row["CustomberEmail"].ToString();

                            order.CustomerAddress =
                                row["CustomberAddress"] == DBNull.Value
                                    ? ""
                                    : row["CustomberAddress"].ToString();

                            order.DeliveryAddress =
                                row["DeliveryAddress"] == DBNull.Value
                                    ? ""
                                    : row["DeliveryAddress"].ToString();

                            order.NoofItem =
                                row["NoofItem"] == DBNull.Value
                                    ? 0
                                    : Convert.ToInt32(
                                        row["NoofItem"]);

                            order.OrderStatus =
                                row["OrderStatus"] == DBNull.Value
                                    ? ""
                                    : row["OrderStatus"].ToString();

                            order.PaymentStatus =
                                row["PaymentStatus"] == DBNull.Value
                                    ? "Pending"
                                    : row["PaymentStatus"].ToString();

                            order.CustomerRemark =
                                row["CustomberRemark"] == DBNull.Value
                                    ? ""
                                    : row["CustomberRemark"].ToString();

                            order.AdminRemark =
                                row["AdminRemark"] == DBNull.Value
                                    ? ""
                                    : row["AdminRemark"].ToString();

                            order.Total =
                                row["Total"] == DBNull.Value
                                    ? 0
                                    : Convert.ToDecimal(
                                        row["Total"]);

                            order.SysDeliveryDate =
                                row["SysDeliveryDate"] == DBNull.Value
                                    ? ""
                                    : row["SysDeliveryDate"].ToString();

                            order.PaidAmount =
                                row["PaidPayment"] == DBNull.Value
                                    ? 0
                                    : Convert.ToDecimal(
                                        row["PaidPayment"]);

                            order.PaymentDate =
                                row["PaymentDate"] == DBNull.Value
                                    ? ""
                                    : row["PaymentDate"].ToString();

                            order.DeliveryDate =
                                row["DeliveryDate"] == DBNull.Value
                                    ? ""
                                    : row["DeliveryDate"].ToString();

                            order.DeliveryTime =
                                row["DeliveryTime"] == DBNull.Value
                                    ? ""
                                    : row["DeliveryTime"].ToString();

                            order.BillingAmount =
                                row["BillingAmount"] == DBNull.Value
                                    ? 0
                                    : Convert.ToDecimal(
                                        row["BillingAmount"]);

                            order._itemList =
                                new List<Order_Item>();

                            if (!string.IsNullOrWhiteSpace(
                                order.OrderNo))
                            {
                                orders[order.OrderNo] =
                                    order;
                            }
                        }
                    }
                }

                if (orders.Count > 0)
                {
                    List<int> orderIds =
                        orders.Values
                            .Select(x => x.OrderID)
                            .Where(x => x > 0)
                            .Distinct()
                            .ToList();

                    if (orderIds.Count > 0)
                    {
                        string orderIdClause =
                            string.Join(
                                ",",
                                orderIds);

                        string itemQuery = @"
SELECT
    oi.OrderItemID,
    oi.OrderID,
    oi.OrderNo,
    oi.ItemID,
    oi.ItemName,
    oi.SubItemID,
    oi.StockID,
    oi.MainID,
    oi.Weight,
    oi.OrderQty,
    oi.Price,
    oi.TotalAmount,
    oi.EntryDate,
    oi.DeliveryWeight,
    oi.DeliveryPrice,

    ip.Weight AS PurchaseWeight,
    ip.UnitID AS PurchaseUnitID,

    un.Unit AS PurchaseUnitName,

    si.UOMID AS SubItemUOMID,
    si.UOMName AS SubItemUOMName

FROM Order_Items oi

OUTER APPLY
(
    SELECT TOP 1
        p.Weight,
        p.UnitID
    FROM Item_Purchase p
    WHERE p.ItemID = oi.ItemID
    ORDER BY p.PurchaseID DESC
) ip

LEFT JOIN Unit un
    ON un.UnitID = ip.UnitID

LEFT JOIN SubItem si
    ON si.SubItemID = oi.SubItemID

WHERE oi.OrderID IN (" +
                            orderIdClause +
                            @")
ORDER BY oi.OrderID, oi.OrderItemID";

                        DataSet itemDs =
                            Database.getDataSet(
                                itemQuery);

                        if (itemDs != null &&
                            itemDs.Tables.Count > 0 &&
                            itemDs.Tables[0].Rows.Count > 0)
                        {
                            DataTable itemDt =
                                itemDs.Tables[0];

                            foreach (DataRow row
                                in itemDt.Rows)
                            {
                                int orderId =
                                    row["OrderID"] ==
                                    DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(
                                            row["OrderID"]);

                                Order_Model order =
                                    orders.Values
                                        .FirstOrDefault(
                                            x =>
                                                x.OrderID ==
                                                orderId
                                        );

                                if (order == null)
                                {
                                    continue;
                                }

                                Order_Item item =
                                    new Order_Item();

                                item.OrderItemID =
                                    row["OrderItemID"] ==
                                    DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(
                                            row["OrderItemID"]);

                                item.OrderID =
                                    orderId;

                                item.OrderNo =
                                    row["OrderNo"] ==
                                    DBNull.Value
                                        ? ""
                                        : row["OrderNo"]
                                            .ToString();

                                item.ItemID =
                                    row["ItemID"] ==
                                    DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(
                                            row["ItemID"]);

                                item.ItemName =
                                    row["ItemName"] ==
                                    DBNull.Value
                                        ? ""
                                        : row["ItemName"]
                                            .ToString();

                                item.SubItemID =
                                    row["SubItemID"] ==
                                    DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(
                                            row["SubItemID"]);

                                item.StockID =
                                    row["StockID"] ==
                                    DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(
                                            row["StockID"]);

                                item.MainID =
                                    row["MainID"] ==
                                    DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(
                                            row["MainID"]);

                                item.Weight =
                                    row["Weight"] ==
                                    DBNull.Value
                                        ? ""
                                        : row["Weight"]
                                            .ToString();

                                item.OrderQty =
                                    row["OrderQty"] ==
                                    DBNull.Value
                                        ? 0
                                        : Convert.ToDecimal(
                                            row["OrderQty"]);

                                item.Price =
                                    row["Price"] ==
                                    DBNull.Value
                                        ? 0
                                        : Convert.ToDecimal(
                                            row["Price"]);

                                item.TotalAmount =
                                    row["TotalAmount"] ==
                                    DBNull.Value
                                        ? 0
                                        : Convert.ToDecimal(
                                            row["TotalAmount"]);

                                if (row["PurchaseWeight"] !=
                                    DBNull.Value)
                                {
                                    string weight =
                                        row["PurchaseWeight"]
                                            .ToString();

                                    string unit =
                                        row["PurchaseUnitName"] ==
                                        DBNull.Value
                                            ? ""
                                            : row[
                                                "PurchaseUnitName"
                                            ].ToString();

                                    item.AvailableStock =
                                        string.IsNullOrEmpty(unit)
                                            ? weight
                                            : weight +
                                              " " +
                                              unit;
                                }

                                if (row["SubItemUOMID"] !=
                                    DBNull.Value)
                                {
                                    item.SubItemUnitID =
                                        row["SubItemUOMID"]
                                            .ToString();
                                }

                                if (row["SubItemUOMName"] !=
                                    DBNull.Value)
                                {
                                    item.SubItemUnitName =
                                        row["SubItemUOMName"]
                                            .ToString();
                                }

                                if (row["DeliveryWeight"] !=
                                    DBNull.Value)
                                {
                                    item.DeliveryWeight =
                                        Convert.ToDecimal(
                                            row[
                                                "DeliveryWeight"
                                            ]);
                                }

                                if (row["DeliveryPrice"] !=
                                    DBNull.Value)
                                {
                                    item.DeliveryPrice =
                                        Convert.ToDecimal(
                                            row[
                                                "DeliveryPrice"
                                            ]);
                                }
                                else
                                {
                                    item.DeliveryPrice =
                                        item.Price;
                                }

                                order._itemList.Add(item);
                            }
                        }
                    }
                }

                foreach (DataRow row
                    in historyDt.Rows)
                {
                    Wallet_History historyData =
                        new Wallet_History();

                    historyData.HistoryID =
                        row["HistoryID"] ==
                        DBNull.Value
                            ? 0
                            : Convert.ToInt32(
                                row["HistoryID"]);

                    historyData.WalletID =
                        row["WalletID"] ==
                        DBNull.Value
                            ? 0
                            : Convert.ToInt32(
                                row["WalletID"]);

                    historyData.UserID =
                        row["UserID"] ==
                        DBNull.Value
                            ? 0
                            : Convert.ToInt32(
                                row["UserID"]);

                    historyData.Amount =
                        row["Amount"] ==
                        DBNull.Value
                            ? 0
                            : Convert.ToDecimal(
                                row["Amount"]);

                    historyData.Type =
                        row["Type"] ==
                        DBNull.Value
                            ? ""
                            : row["Type"].ToString();

                    historyData.OrderNo =
                        row["OrderNo"] ==
                        DBNull.Value
                            ? ""
                            : row["OrderNo"].ToString();

                    historyData.EntryDate =
                        row["EntryDate"] ==
                        DBNull.Value
                            ? ""
                            : row["EntryDate"].ToString();

                    historyData.PaymentMode =
                        row["PaymentMode"] ==
                        DBNull.Value
                            ? ""
                            : row["PaymentMode"].ToString();

                    if (historyData.Type == "Debit" &&
                        !string.IsNullOrWhiteSpace(
                            historyData.OrderNo))
                    {
                        Order_Model orderDetails = null;

                        orders.TryGetValue(
                            historyData.OrderNo,
                            out orderDetails
                        );

                        if (orderDetails != null)
                        {
                            string invoiceFile =
                                OrderSummary
                                    .GenerateOrderSummary(
                                        orderDetails
                                    );

                            if (invoiceFile !=
                                "Error.pdf")
                            {
                                historyData.Invoice =
                                    "http://admin.freshfoodz.in.net/Content/OrderSummary/"
                                    + invoiceFile;
                            }
                        }
                    }

                    data._walletHistory
                        .Add(historyData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    ex.ToString());
            }

            return View(data);
        }

        #endregion

        #region Add Money

        [HttpPost]
        public ActionResult AddMoney(
            Wallet_Model data)
        {
            try
            {
                if (data == null ||
                    data.UserID == 0)
                {
                    TempData["ErrorMessage"] =
                        "Please select a customer.";

                    return RedirectToAction(
                        "AddMoney");
                }

                decimal amount = data.Amount;

                if (amount == 0)
                {
                    TempData["ErrorMessage"] =
                        "Please enter a valid amount.";

                    return RedirectToAction(
                        "AddMoney");
                }

                // ---------------------------------------------------------
                // Get wallet
                // ---------------------------------------------------------

                string walletQuery = @"
SELECT TOP 1
    WalletID,
    Balance
FROM UserWallet
WHERE UserID = " + data.UserID + @"
  AND IsDisplay = 1";

                DataSet walletDs =
                    Database.getDataSet(
                        walletQuery);

                int walletId = 0;

                decimal existingBalance = 0;

                if (walletDs != null &&
                    walletDs.Tables.Count > 0 &&
                    walletDs.Tables[0].Rows.Count > 0)
                {
                    DataRow walletRow =
                        walletDs.Tables[0].Rows[0];

                    walletId =
                        walletRow["WalletID"] ==
                        DBNull.Value
                            ? 0
                            : Convert.ToInt32(
                                walletRow["WalletID"]);

                    existingBalance =
                        walletRow["Balance"] ==
                        DBNull.Value
                            ? 0
                            : Convert.ToDecimal(
                                walletRow["Balance"]);
                }

                data.WalletID =
                    walletId;

                string output = "";

                // ---------------------------------------------------------
                // New wallet
                // ---------------------------------------------------------

                if (data.WalletID == 0)
                {
                    string insertWalletQuery =
                        "INSERT INTO UserWallet " +
                        "(UserID, Balance, IsDisplay, AddedDate, AddedBy) VALUES (" +
                        data.UserID + ", " +
                        amount.ToString(
                            CultureInfo.InvariantCulture) +
                        ", 1, '" +
                        IndianDateTime.ToString(
                            "yyyy-MM-dd HH:mm:ss") +
                        "', " +
                        data.UserID +
                        ")";

                    output =
                        Database.insertData(
                            insertWalletQuery);

                    if (output == "200")
                    {
                        string walletIdQuery =
                            "SELECT TOP 1 WalletID " +
                            "FROM UserWallet " +
                            "WHERE UserID = " +
                            data.UserID +
                            " AND IsDisplay = 1 " +
                            "ORDER BY WalletID DESC";

                        string walletIdValue =
                            Database.getValue(
                                walletIdQuery);

                        if (!string.IsNullOrEmpty(
                            walletIdValue))
                        {
                            data.WalletID =
                                Convert.ToInt32(
                                    walletIdValue);
                        }

                        if (data.WalletID != 0)
                        {
                            // -------------------------------------------------
                            // IMPORTANT:
                            // Payment_Status explicitly saved as Offline
                            // -------------------------------------------------

                            string historyQuery =
                                "INSERT INTO UserWalletHistory " +
                                "(WalletID, UserID, Type, Status, Amount, Balance, OrderNo, EntryDate, ManageBy, PaymentMode, Payment_Status) VALUES (" +
                                data.WalletID + ", " +
                                data.UserID + ", " +
                                "'Credit', " +
                                "'Approved', " +
                                amount.ToString(
                                    CultureInfo.InvariantCulture) +
                                ", " +
                                amount.ToString(
                                    CultureInfo.InvariantCulture) +
                                ", " +
                                "NULL, '" +
                                IndianDateTime.ToString(
                                    "yyyy-MM-dd HH:mm:ss") +
                                "', " +
                                "'Admin', " +
                                "'Offline', " +
                                "'Offline')";

                            output =
                                Database.insertData(
                                    historyQuery);
                        }
                    }
                }
                else
                {
                    // ---------------------------------------------------------
                    // Existing wallet
                    // ---------------------------------------------------------

                    decimal newBalance =
                        existingBalance +
                        amount;

                    // ---------------------------------------------------------
                    // IMPORTANT:
                    // Payment_Status explicitly saved as Offline
                    // ---------------------------------------------------------

                    string historyQuery =
                        "INSERT INTO UserWalletHistory " +
                        "(WalletID, UserID, Type, Status, Amount, Balance, OrderNo, EntryDate, ManageBy, PaymentMode, Payment_Status) VALUES (" +
                        data.WalletID + ", " +
                        data.UserID + ", " +
                        "'Credit', " +
                        "'Approved', " +
                        amount.ToString(
                            CultureInfo.InvariantCulture) +
                        ", " +
                        newBalance.ToString(
                            CultureInfo.InvariantCulture) +
                        ", " +
                        "NULL, '" +
                        IndianDateTime.ToString(
                            "yyyy-MM-dd HH:mm:ss") +
                        "', " +
                        "'Admin', " +
                        "'Offline', " +
                        "'Offline')";

                    output =
                        Database.insertData(
                            historyQuery);

                    if (output == "200")
                    {
                        string updateQuery =
                            "UPDATE UserWallet SET Balance = " +
                            newBalance.ToString(
                                CultureInfo.InvariantCulture) +
                            " WHERE WalletID = " +
                            data.WalletID +
                            " AND UserID = " +
                            data.UserID;

                        output =
                            Database.insertData(
                                updateQuery);

                        existingBalance =
                            newBalance;
                    }
                }

                if (output != "200")
                {
                    TempData["ErrorMessage"] =
                        "An error occured please try again !!";

                    return RedirectToAction(
                        "AddMoney");
                }

                // ---------------------------------------------------------
                // Notification
                // ---------------------------------------------------------

                decimal finalBalance =
                    existingBalance;

                if (data.WalletID != 0 &&
                    data.WalletID != walletId)
                {
                    string balanceQuery =
                        "SELECT Balance FROM UserWallet " +
                        "WHERE WalletID = " +
                        data.WalletID +
                        " AND UserID = " +
                        data.UserID;

                    string balanceValue =
                        Database.getValue(
                            balanceQuery);

                    if (!string.IsNullOrEmpty(
                        balanceValue))
                    {
                        finalBalance =
                            Convert.ToDecimal(
                                balanceValue);
                    }
                }

                string msg;

                if (finalBalance > 0)
                {
                    msg =
                        "Your Amount Rs." +
                        amount +
                        " is successfully added in your wallet and your available Balance is " +
                        finalBalance +
                        ".";
                }
                else
                {
                    msg =
                        "Your Amount Rs." +
                        amount +
                        " is successfully added in your wallet and your available outstanding is " +
                        finalBalance +
                        ".";
                }

                string title =
                    "Amount Added";

                string notificationQuery =
                    "INSERT INTO Tbl_Notifications " +
                    "VALUES('" +
                    data.UserID +
                    "', '" +
                    title +
                    "', '', '" +
                    msg.Replace(
                        "'",
                        "''") +
                    "','" +
                    IndianDateTime.ToString(
                        "yyyy-MM-dd HH:mm:ss") +
                    "','')";

                string notificationOutput =
                    Database.insertData(
                        notificationQuery);

                if (notificationOutput != "200")
                {
                    TempData["ErrorMessage"] =
                        "An error occured please try again !!";
                }

                TempData["SuccessMessage"] =
                    "Amount added Sucessfully !!";
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    ex.ToString());

                TempData["ErrorMessage"] =
                    "An error occured please try again !!";
            }

            return RedirectToAction(
                "AddMoney");
        }

        #endregion
    }
}
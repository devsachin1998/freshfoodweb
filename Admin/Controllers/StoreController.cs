using Admin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.Mvc;

namespace Admin.Controllers
{
    public class StoreController : Controller
    {
        // =====================================================
        // STOCK LIST - PAGE SHELL
        // =====================================================
        [HttpGet]
        public ActionResult StockList()
        {
            return View(new List<Stock_SubItem>());
        }

        // =====================================================
        // STOCK LIST - SERVER-SIDE DATA (OPTIMIZED)
        // Page data + filtered count ek hi query mein.
        // Total count sirf search hone par alag se.
        // =====================================================
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult StockListData(
            int draw = 1,
            int start = 0,
            int length = 25,
            string search = "",
            int orderColumn = 1,
            string orderDir = "asc")
        {
            // Jaanch ke liye: server pe kitna time laga
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                start = Math.Max(0, start);

                if (length != 10 && length != 25 &&
                    length != 50 && length != 100)
                {
                    length = 25;
                }

                // 0 = Sr.No, 1 = Item Name, 2 = Weight,
                // 3 = Selling Price, 4 = In Stock, 5 = Action.
                string[] sortableColumns =
                {
                    "I.ItemName",
                    "I.ItemName",
                    "ISNULL(SI.WeightType, S.WeightType)",
                    "S.Price",
                    "S.InStock",
                    "S.StockID"
                };

                if (orderColumn < 0 || orderColumn >= sortableColumns.Length)
                {
                    orderColumn = 1;
                }

                string sortColumn = sortableColumns[orderColumn];
                string direction =
                    string.Equals(orderDir, "desc", StringComparison.OrdinalIgnoreCase)
                    ? "DESC"
                    : "ASC";

                // Item Master se delete hue items ki stock rows nahi dikhani
                const string baseWhere = " WHERE I.ItemID IS NOT NULL ";

                string searchWhere = BuildStockSearchWhere(search);
                bool hasSearch = searchWhere.Length > 0;
                string where = baseWhere + searchWhere;

                const string fromClause = @"
                    FROM ITEMSTOCK S
                    LEFT JOIN ITEMMASTER I
                        ON I.ItemID = S.ItemID
                    LEFT JOIN SUBITEM SI
                        ON SI.SubItemID = S.SubItemID
                        AND SI.ItemID = S.ItemID ";

                string dataQuery = @"
                    SELECT
                        S.StockID,
                        S.MainID,
                        S.ItemID,
                        S.SubItemID,
                        I.ItemName,
                        -- Weight Item Master (SUBITEM) se, jaise 500 + gms
                        ISNULL(SI.WeightType, S.WeightType) AS WeightType,
                        S.Price,
                        S.InStock,
                        SI.UOMName,
                        COUNT(*) OVER() AS FilteredTotal
                    " + fromClause + where + @"
                    ORDER BY " + sortColumn + " " + direction + @", S.StockID DESC
                    OFFSET " + start + @" ROWS
                    FETCH NEXT " + length + @" ROWS ONLY";

                DataSet dataDs = Database.getDataSet(dataQuery);

                List<object> rows = new List<object>();
                int filteredRecords = 0;

                if (dataDs != null && dataDs.Tables.Count > 0)
                {
                    DataTable table = dataDs.Tables[0];

                    if (table.Rows.Count > 0)
                    {
                        filteredRecords = GetInt(table.Rows[0], "FilteredTotal");
                    }

                    foreach (DataRow row in table.Rows)
                    {
                        rows.Add(new
                        {
                            StockID = GetInt(row, "StockID"),
                            MainID = GetInt(row, "MainID"),
                            ItemID = GetInt(row, "ItemID"),
                            SubItemID = GetInt(row, "SubItemID"),
                            ItemName = GetString(row, "ItemName"),
                            Weight = GetString(row, "WeightType"),
                            UOMName = GetString(row, "UOMName"),
                            Price = GetDecimal(row, "Price").ToString(
                                CultureInfo.InvariantCulture),
                            InStock = GetBool(row, "InStock")
                        });
                    }
                }

                // Khaali page (jaise last page ka record delete hua)
                if (rows.Count == 0 && start > 0)
                {
                    filteredRecords = GetCount(
                        "SELECT COUNT(*) AS Total " + fromClause + where);
                }

                int totalRecords = hasSearch
                    ? GetCount("SELECT COUNT(*) AS Total " + fromClause + baseWhere)
                    : filteredRecords;

                sw.Stop();
                Response.AddHeader("X-Server-Time-Ms", sw.ElapsedMilliseconds.ToString());

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords,
                    serverMs = sw.ElapsedMilliseconds,
                    dbOk = dataDs != null,
                    data = rows
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("StockListData Error: " + ex);

                Response.StatusCode = 500;

                return Json(new
                {
                    draw = draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new object[0],
                    error = "Unable to load stock list."
                }, JsonRequestBehavior.AllowGet);
            }
        }


        // =====================================================
        // MANAGE STOCK - GET (OPTIMIZED)
        // Ek hi query mein lot + master ke SAARE sub items.
        // Jo sub item lot banne ke baad master mein add hua
        // (jaise Annar 250 gms), woh bhi khaali price ke saath dikhega.
        // =====================================================
        [HttpGet]
        public ActionResult ManageStock(int MainID = 0)
        {
            Stock_Model model = new Stock_Model();

            model.MainID = 0;
            model.ItemID = 0;
            model.ItemName = "";
            model._subitemList = new List<Stock_SubItem>();

            try
            {
                if (MainID > 0)
                {
                    DataSet ds = Database.getDataSet(
                        LotRowsQuery(MainID.ToString(CultureInfo.InvariantCulture)));

                    if (ds != null &&
                        ds.Tables.Count > 0 &&
                        ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow first = ds.Tables[0].Rows[0];

                        model.MainID = GetInt(first, "LotID");
                        model.ItemID = GetInt(first, "LotItemID");
                        model.ItemName = GetString(first, "LotItemName");

                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            // Lot hai par koi sub item nahi (OUTER APPLY se NULL row)
                            if (GetInt(row, "SubItemID") <= 0)
                                continue;

                            model._subitemList.Add(MapStockSubItem(row));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ManageStock GET Error: " + ex);
            }

            return View(model);
        }


        // =====================================================
        // LOT ROWS QUERY (ManageStock GET aur GetStockForItem dono use karte hain)
        // - Master (SUBITEM) ke saare sub items + lot ka saved stock.
        //   Stock mein nahi hai to StockID = 0, Price khaali.
        // - Jo stock row ka sub item master se hat gaya ho,
        //   woh bhi dikhti hai (taaki save pe data na chhoote).
        // =====================================================
        private static string LotRowsQuery(string mainIdSql)
        {
            return @"
                SELECT
                    M.ID        AS LotID,
                    M.ItemID    AS LotItemID,
                    I.ItemName  AS LotItemName,
                    R.StockID,
                    R.MainID,
                    R.ItemID,
                    R.SubItemID,
                    I.ItemName,
                    R.WeightType,
                    R.Price,
                    R.InStock,
                    R.UOMID,
                    R.UOMName
                FROM Main_item_stock M
                LEFT JOIN ITEMMASTER I
                    ON I.ItemID = M.ItemID
                OUTER APPLY
                (
                    SELECT
                        ISNULL(S.StockID, 0)                AS StockID,
                        M.ID                                AS MainID,
                        SI.ItemID,
                        SI.SubItemID,
                        SI.WeightType                       AS WeightType,
                        S.Price,
                        ISNULL(S.InStock, 0)                AS InStock,
                        SI.UOMID,
                        SI.UOMName
                    FROM SUBITEM SI
                    LEFT JOIN ITEMSTOCK S
                        ON S.MainID = M.ID
                        AND S.SubItemID = SI.SubItemID
                    WHERE SI.ItemID = M.ItemID

                    UNION ALL

                    SELECT
                        S.StockID,
                        S.MainID,
                        S.ItemID,
                        S.SubItemID,
                        S.WeightType,
                        S.Price,
                        S.InStock,
                        SI.UOMID,
                        SI.UOMName
                    FROM ITEMSTOCK S
                    LEFT JOIN SUBITEM SI
                        ON SI.SubItemID = S.SubItemID
                        AND SI.ItemID = S.ItemID
                    WHERE S.MainID = M.ID
                      AND NOT EXISTS (
                          SELECT 1 FROM SUBITEM X
                          WHERE X.SubItemID = S.SubItemID
                            AND X.ItemID = M.ItemID)
                ) R
                WHERE M.ID = " + mainIdSql + @"
                ORDER BY R.SubItemID";
        }


        // =====================================================
        // ITEM SEARCH FOR SELECT2 (NEW)
        // Wallet page ke customer dropdown jaisa:
        // page load pe saare items nahi aate, user type kare
        // tab sirf matching (max 50) items aate hain.
        // =====================================================
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult GetItemsForSelect(string term = "")
        {
            List<object> results = new List<object>();

            try
            {
                string t = (term ?? "").Trim();

                if (t.Length > 50)
                    t = t.Substring(0, 50);

                // NOTE: ITEMMASTER mein IsActive / IsDeleted jaisa column ho
                // to yahan filter jod do.
                string where = t.Length > 0
                    ? " WHERE ItemName LIKE '%" + EscapeLike(t) + "%'"
                    : "";

                string query = @"
                    SELECT TOP 50 ItemID, ItemName
                    FROM ITEMMASTER" + where + @"
                    ORDER BY ItemName";

                DataSet ds = Database.getDataSet(query);

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        int id = GetInt(row, "ItemID");

                        if (id <= 0)
                            continue;

                        results.Add(new
                        {
                            id = id,
                            text = GetString(row, "ItemName")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetItemsForSelect Error: " + ex);
            }

            return Json(new { results = results }, JsonRequestBehavior.AllowGet);
        }


        // =====================================================
        // GET STOCK FOR ITEM (OPTIMIZED)
        // Ek hi call: latest lot dhoondho.
        //  - Lot hai  -> master ke saare sub items + lot ka saved stock
        //  - Lot nahi -> sirf master sub items
        // =====================================================
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult GetStockForItem(int ItemID)
        {
            List<Stock_SubItem> list = new List<Stock_SubItem>();

            try
            {
                if (ItemID <= 0)
                {
                    return Json(new { success = true, MainID = 0, data = list },
                        JsonRequestBehavior.AllowGet);
                }

                string query = @"
                    SET NOCOUNT ON;

                    DECLARE @ItemID INT = " + ItemID + @";

                    DECLARE @MainID INT = ISNULL((
                        SELECT TOP 1 ID
                        FROM Main_item_stock
                        WHERE ItemID = @ItemID
                        ORDER BY ID DESC), 0);

                    IF @MainID > 0
                    BEGIN
                        " + LotRowsQuery("@MainID") + @";
                    END
                    ELSE
                    BEGIN
                        SELECT
                            0 AS LotID,
                            0 AS StockID,
                            0 AS MainID,
                            SI.ItemID,
                            SI.SubItemID,
                            I.ItemName,
                            SI.WeightType,
                            NULL AS Price,
                            0 AS InStock,
                            SI.UOMID,
                            SI.UOMName
                        FROM SUBITEM SI
                        LEFT JOIN ITEMMASTER I
                            ON I.ItemID = SI.ItemID
                        WHERE SI.ItemID = @ItemID
                        ORDER BY SI.SubItemID;
                    END";

                DataSet ds = Database.getDataSet(query);
                int mainID = 0;

                if (ds != null &&
                    ds.Tables.Count > 0 &&
                    ds.Tables[0].Rows.Count > 0)
                {
                    mainID = GetInt(ds.Tables[0].Rows[0], "LotID");

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        if (GetInt(row, "SubItemID") <= 0)
                            continue;

                        list.Add(MapStockSubItem(row));
                    }
                }

                return Json(new { success = true, MainID = mainID, data = list },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetStockForItem Error: " + ex);

                return Json(new
                {
                    success = false,
                    MainID = 0,
                    message = "Unable to load stock details.",
                    data = list
                }, JsonRequestBehavior.AllowGet);
            }
        }


        // =====================================================
        // GET LATEST MAIN ID BY ITEM
        // (Purana action - naya page GetStockForItem use karta hai.
        //  Kisi aur page pe use ho raha ho isliye rakha hai.)
        // =====================================================
        [HttpGet]
        public JsonResult GetLatestMainIDByItem(int ItemID)
        {
            try
            {
                if (ItemID <= 0)
                {
                    return Json(new { success = false, MainID = 0 },
                        JsonRequestBehavior.AllowGet);
                }

                string query = @"
                    SELECT TOP 1 ID AS MainID
                    FROM Main_item_stock
                    WHERE ItemID = " + ItemID + @"
                    ORDER BY ID DESC";

                DataSet ds = Database.getDataSet(query);

                int mainID = 0;

                if (ds != null &&
                    ds.Tables.Count > 0 &&
                    ds.Tables[0].Rows.Count > 0)
                {
                    mainID = GetInt(ds.Tables[0].Rows[0], "MainID");
                }

                return Json(new { success = true, MainID = mainID, ItemID = ItemID },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetLatestMainIDByItem Error: " + ex);

                return Json(new { success = false, MainID = 0 },
                    JsonRequestBehavior.AllowGet);
            }
        }


        // =====================================================
        // GET SUB ITEMS
        // (Purana action - naya page GetStockForItem use karta hai.)
        // =====================================================
        [HttpGet]
        public JsonResult GetSubItemList(int ItemID, int MainID = 0)
        {
            List<Stock_SubItem> list = new List<Stock_SubItem>();

            try
            {
                if (ItemID <= 0)
                {
                    return Json(new { success = true, data = list },
                        JsonRequestBehavior.AllowGet);
                }

                string query;

                if (MainID > 0)
                {
                    query = @"
                        SELECT
                            S.StockID, S.MainID, S.ItemID, S.SubItemID,
                            I.ItemName, S.WeightType, S.Price, S.InStock,
                            SI.UOMID, SI.UOMName
                        FROM ITEMSTOCK S
                        LEFT JOIN ITEMMASTER I ON I.ItemID = S.ItemID
                        LEFT JOIN SUBITEM SI
                            ON SI.SubItemID = S.SubItemID
                            AND SI.ItemID = S.ItemID
                        WHERE S.ItemID = " + ItemID + @"
                          AND S.MainID = " + MainID + @"
                        ORDER BY S.StockID";
                }
                else
                {
                    query = @"
                        SELECT
                            0 AS StockID, 0 AS MainID, SI.ItemID, SI.SubItemID,
                            I.ItemName, SI.WeightType, NULL AS Price, 0 AS InStock,
                            SI.UOMID, SI.UOMName
                        FROM SUBITEM SI
                        LEFT JOIN ITEMMASTER I ON I.ItemID = SI.ItemID
                        WHERE SI.ItemID = " + ItemID + @"
                        ORDER BY SI.SubItemID";
                }

                DataSet ds = Database.getDataSet(query);

                if (ds != null &&
                    ds.Tables.Count > 0 &&
                    ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        list.Add(MapStockSubItem(row));
                    }
                }

                return Json(new { success = true, data = list },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetSubItemList Error: " + ex);

                return Json(new { success = false, message = ex.Message, data = list },
                    JsonRequestBehavior.AllowGet);
            }
        }


        // =====================================================
        // SAVE STOCK (OPTIMIZED)
        // Pehle: UPDATE + DELETE + har row ka alag INSERT + MAX(ID)
        //        = 10 rows ke liye ~13 database round-trips.
        // Ab:    sab kuch ek SQL batch, ek transaction mein.
        //        Beech mein error aaye to poora rollback.
        //        MAX(ID) ki jagah SCOPE_IDENTITY() (2 log ek saath
        //        save karein to bhi sahi MainID milega).
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveStock(Stock_Model dataString)
        {
            try
            {
                if (dataString == null)
                {
                    return Json(new { success = false, message = "Invalid data." });
                }

                if (dataString.ItemID <= 0)
                {
                    return Json(new { success = false, message = "Please select item." });
                }

                if (dataString._subitemList == null ||
                    dataString._subitemList.Count == 0)
                {
                    return Json(new { success = false, message = "No stock item found." });
                }

                // Stock rows ki VALUES list
                List<string> valueRows = new List<string>();

                foreach (Stock_SubItem item in dataString._subitemList)
                {
                    if (item == null || item.SubItemID <= 0)
                        continue;

                    // (SubItemID, Weight, Price, InStock)
                    valueRows.Add(
                        "(" + item.SubItemID + ", '" +
                        EscapeSql(item.Weight) + "', " +
                        item.Price.ToString(CultureInfo.InvariantCulture) + ", " +
                        (item.InStock ? "1" : "0") + ")");
                }

                if (valueRows.Count == 0)
                {
                    return Json(new { success = false, message = "No valid stock item found." });
                }

                StringBuilder sql = new StringBuilder();

                sql.Append(@"
                    SET NOCOUNT ON;
                    SET XACT_ABORT ON;

                    BEGIN TRANSACTION;

                    DECLARE @ItemID INT = " + dataString.ItemID + @";
                    DECLARE @MainID INT = " + Math.Max(0, dataString.MainID) + @";

                    -- Aaya hua lot kisi AUR item ka ho to use mat chhuo
                    -- (warna Banana ki rows Masroom ke lot mein chali jaati hain).
                    IF @MainID > 0 AND NOT EXISTS (
                        SELECT 1 FROM Main_item_stock
                        WHERE ID = @MainID AND ItemID = @ItemID)
                    BEGIN
                        SET @MainID = 0;
                    END

                    -- Naya lot tabhi banao jab is item ka koi lot hai hi nahi.
                    -- (Warna ek hi item ke 2 lot ban jaate hain, jaise Amba 39 / 100)
                    IF @MainID <= 0
                    BEGIN
                        SELECT TOP 1 @MainID = ID
                        FROM Main_item_stock
                        WHERE ItemID = @ItemID
                        ORDER BY ID DESC;

                        SET @MainID = ISNULL(@MainID, 0);
                    END

                    IF @MainID > 0 AND EXISTS (
                        SELECT 1 FROM Main_item_stock WHERE ID = @MainID)
                    BEGIN
                        UPDATE Main_item_stock
                        SET ItemID = @ItemID
                        WHERE ID = @MainID;

                        DELETE FROM ITEMSTOCK
                        WHERE MainID = @MainID;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO Main_item_stock (ItemID)
                        VALUES (@ItemID);

                        SET @MainID = CAST(SCOPE_IDENTITY() AS INT);
                    END

                    IF @MainID IS NULL OR @MainID <= 0
                    BEGIN
                        ;THROW 50001, 'Unable to create stock lot.', 1;
                    END
                ");

                // SQL Server ek INSERT mein max 1000 rows leta hai.
                const int chunkSize = 500;

                for (int i = 0; i < valueRows.Count; i += chunkSize)
                {
                    int take = Math.Min(chunkSize, valueRows.Count - i);

                    // Weight Item Master (SUBITEM) se, UNIT KE SAATH ("500 gms").
                    // Mobile app (GetStockItemList) ITEMSTOCK.WeightType seedha
                    // dikhata hai, aur order history usi se unit nikalti hai,
                    // isliye unit saath mein save karna zaroori hai.
                    sql.Append(@"
                    INSERT INTO ITEMSTOCK
                        (MainID, ItemID, SubItemID, WeightType, Price, InStock, EntryDate)
                    SELECT
                        @MainID, @ItemID, V.SubItemID,
                        CASE
                            WHEN SI.SubItemID IS NULL
                                THEN V.Weight
                            WHEN ISNULL(LTRIM(RTRIM(SI.UOMName)), '') = ''
                                THEN LTRIM(RTRIM(SI.WeightType))
                            WHEN RIGHT(LTRIM(RTRIM(SI.WeightType)), LEN(LTRIM(RTRIM(SI.UOMName))))
                                 = LTRIM(RTRIM(SI.UOMName))
                                THEN LTRIM(RTRIM(SI.WeightType))
                            ELSE LTRIM(RTRIM(SI.WeightType)) + ' ' + LTRIM(RTRIM(SI.UOMName))
                        END,
                        V.Price, V.InStock, GETDATE()
                    FROM (VALUES
                        ");
                    sql.Append(string.Join(",\n                        ",
                        valueRows.GetRange(i, take)));
                    sql.Append(@"
                    ) AS V (SubItemID, Weight, Price, InStock)
                    LEFT JOIN SUBITEM SI
                        ON SI.SubItemID = V.SubItemID
                        AND SI.ItemID = @ItemID;");
                }

                sql.Append(@"
                    COMMIT TRANSACTION;

                    SELECT @MainID AS MainID;");

                DataSet ds = Database.getDataSet(sql.ToString());

                int savedMainID = 0;

                if (ds != null &&
                    ds.Tables.Count > 0 &&
                    ds.Tables[0].Rows.Count > 0)
                {
                    savedMainID = GetInt(ds.Tables[0].Rows[0], "MainID");
                }

                if (savedMainID <= 0)
                {
                    return Json(new { success = false, message = "Unable to save stock." });
                }

                return Json(new
                {
                    success = true,
                    MainID = savedMainID,
                    isNew = savedMainID != dataString.MainID,
                    message = "Stock saved successfully."
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SaveStock Error: " + ex);

                return Json(new { success = false, message = ex.Message });
            }
        }


        // =====================================================
        // UPDATE STOCK STATUS
        // =====================================================
        [HttpPost]
        public JsonResult ManageStock(int StockID, bool Status)
        {
            try
            {
                if (StockID <= 0)
                {
                    return Json(new { success = false, message = "Invalid stock ID." });
                }

                string query = @"
                    UPDATE ITEMSTOCK
                    SET InStock = " + (Status ? "1" : "0") + @"
                    WHERE StockID = " + StockID;

                // insertData error pe exception nahi deta, message string deta hai
                string output = Database.insertData(query);

                if (output != "200")
                {
                    return Json(new { success = false, message = "Unable to update stock status." });
                }

                return Json(new { success = true, message = "Stock status updated." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ManageStock Status Error: " + ex);

                return Json(new { success = false, message = ex.Message });
            }
        }


        // =====================================================
        // DELETE STOCK
        // =====================================================
        [HttpPost]
        public JsonResult DeleteStock(int StockID)
        {
            try
            {
                if (StockID <= 0)
                {
                    return Json(new { success = false, message = "Invalid stock ID." });
                }

                string query = @"
                    DELETE FROM ITEMSTOCK
                    WHERE StockID = " + StockID;

                string output = Database.insertData(query);

                if (output != "200")
                {
                    return Json(new { success = false, message = "Unable to delete stock." });
                }

                return Json(new { success = true, message = "Stock deleted successfully." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DeleteStock Error: " + ex);

                return Json(new { success = false, message = ex.Message });
            }
        }


        // =====================================================
        // SEARCH WHERE BUILDER (Stock List)
        // =====================================================
        private string BuildStockSearchWhere(string search)
        {
            string raw = (search ?? "").Trim();

            if (raw.Length == 0)
                return "";

            if (raw.Length > 100)
                raw = raw.Substring(0, 100);

            string like = EscapeLike(raw);

            StringBuilder sb = new StringBuilder();
            sb.Append(" AND (");
            sb.Append("I.ItemName LIKE '%").Append(like).Append("%'");
            sb.Append(" OR S.WeightType LIKE '%").Append(like).Append("%'");
            sb.Append(" OR (ISNULL(SI.WeightType, '') + ' ' + ISNULL(SI.UOMName, '')) LIKE '%").Append(like).Append("%'");

            int idNumber;
            if (int.TryParse(raw, NumberStyles.None,
                    CultureInfo.InvariantCulture, out idNumber))
            {
                sb.Append(" OR S.StockID = ").Append(idNumber);
            }

            decimal priceNumber;
            if (decimal.TryParse(raw, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out priceNumber))
            {
                sb.Append(" OR S.Price = ")
                  .Append(priceNumber.ToString(CultureInfo.InvariantCulture));
            }

            sb.Append(")");

            return sb.ToString();
        }

        private int GetCount(string query)
        {
            DataSet ds = Database.getDataSet(query);

            if (ds != null &&
                ds.Tables.Count > 0 &&
                ds.Tables[0].Rows.Count > 0)
            {
                return GetInt(ds.Tables[0].Rows[0], "Total");
            }

            return 0;
        }


        // =====================================================
        // MAP DATA
        // =====================================================
        private Stock_SubItem MapStockSubItem(DataRow row)
        {
            Stock_SubItem item = new Stock_SubItem();

            item.StockID = GetInt(row, "StockID");
            item.MainID = GetInt(row, "MainID");
            item.ItemID = GetInt(row, "ItemID");
            item.SubItemID = GetInt(row, "SubItemID");
            item.ItemName = GetString(row, "ItemName");
            item.Weight = GetString(row, "WeightType");
            item.Price = GetDecimal(row, "Price");
            item.InStock = GetBool(row, "InStock");
            item.UOMID = GetInt(row, "UOMID");
            item.UOMName = GetString(row, "UOMName");
            item.Weight_UOM = GetString(row, "WeightType");

            return item;
        }


        // =====================================================
        // SAFE GETTERS
        // =====================================================
        private int GetInt(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) ||
                row[column] == DBNull.Value)
                return 0;

            int value;

            return int.TryParse(row[column].ToString(), out value) ? value : 0;
        }

        private string GetString(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) ||
                row[column] == DBNull.Value)
                return "";

            return row[column].ToString();
        }

        private decimal GetDecimal(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) ||
                row[column] == DBNull.Value)
                return 0;

            decimal value;

            return decimal.TryParse(row[column].ToString(), out value) ? value : 0;
        }

        private bool GetBool(DataRow row, string column)
        {
            if (!row.Table.Columns.Contains(column) ||
                row[column] == DBNull.Value)
                return false;

            bool value;

            if (bool.TryParse(row[column].ToString(), out value))
                return value;

            int intValue;

            if (int.TryParse(row[column].ToString(), out intValue))
                return intValue == 1;

            return false;
        }

        private string EscapeSql(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return value.Replace("'", "''");
        }

        private string EscapeLike(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return value
                .Replace("'", "''")
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("_", "[_]");
        }
    }
}
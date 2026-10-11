using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Admin.Models
{
    public class Report_Model
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }

        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public string ReportDate { get; set; }

        public decimal ItemTotal { get; set; }
        public string ItemTotal_UOM { get; set; }

        public List<Report_Details> _Report_Details { get; set; }

        public List<ItemOrder> _itemOrderList { get; set; }

        public List<OrderDetails> _OrderDetails { get; set; }

    }

    public class Report_Details
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }

        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public string ReportDate { get; set; }

        public decimal ItemTotal { get; set; }

        public List<ItemOrder> _itemOrderList { get; set; }
    }

    public class ItemOrder
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int SubItemID { get; set; }
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public int UOMID { get; set; }
        public string UOMName { get; set; }
        public decimal Weight { get; set; }
        public int OrderID { get; set; }
        public int OrderItemID { get; set; }
        public int OrderQty { get; set; }
    }

    public class OrderDetails
    {
        public int OrderID { get; set; }
        public string OrderNo { get; set; }
        public string CustomerName { get; set; }
        public decimal ReceviedAmount { get; set; }
        public string Invoice { get; set; }
    }
}
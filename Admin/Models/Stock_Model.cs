using System.Collections.Generic;

namespace Admin.Models
{
    public class Stock_Model
    {
        public int MainID { get; set; }

        public int ItemID { get; set; }
        public string ItemName { get; set; }


        public List<Stock_SubItem> _subitemList { get; set; }
    }

    public class Stock_SubItem
    {
        public int MainID { get; set; }
        public int StockID { get; set; }
        public int SubItemID { get; set; }
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public int UOMID { get; set; }
        public string UOMName { get; set; }
        public string Weight { get; set; }
        public decimal Price { get; set; }
        public bool InStock { get; set; }
        public string Weight_UOM { get; set; }
    }
}
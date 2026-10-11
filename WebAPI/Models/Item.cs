using System.Collections.Generic;

namespace WebAPI.Models
{
    public class Item
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }

        //public int UOMID { get; set; }
        //public string UOMName { get; set; }

        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public string ImagePath { get; set; }
        //public string EntryBy { get; set; }

        public List<SubItem> _subitemList { get; set; }
    }

    public class SubItem
    {
        public int StockID { get; set; }
        public int MainID { get; set; }
        public int SubItemID { get; set; }
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public string Weight { get; set; }
        public decimal Price { get; set; }
    }
}
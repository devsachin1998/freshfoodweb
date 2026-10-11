using System.Collections.Generic;

namespace Admin.Models
{
    public class Purchase
    {
        public int PurchaseID { get; set; }

        public int ItemID { get; set; }
        public string ItemName { get; set; }

        public int UOMID { get; set; }
        public string UOMName { get; set; }

        public decimal Weight { get; set; }
        public decimal Price { get; set; }

        public string ActionByName { get; set; }

        public string EntryDate { get; set; }
        public string GivenDate { get; set; }
    }
}
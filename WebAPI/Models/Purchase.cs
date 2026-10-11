using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class Purchase
    {
        public int PurchaseID { get; set; }
        public int ItemID { get; set; }
        public int Weight { get; set; }
        public int Price { get; set; }
        public List<Purchase> purchaseList { get; set; }
     
    }
}
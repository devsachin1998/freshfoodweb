using System.Collections.Generic;

namespace Admin.Models
{
    public class Dashboard_Model
    {
        public string ItemCount { get; set; }
        public string OrderCount { get; set; }
        public string UserCount { get; set; }
        public string Balance { get; set; }
        public string Orderamount { get; set; }

        public string DeliveredOrederd { get; set; }


        public string OnlineAmount { get; set; }
        public string Online { get; set; }
        public string Offlineamount { get; set; }





        public string Newcustomer { get; set; }

        public List<Order_Model> _orderList { get; set; }

        public List<Wallet_History> _walletRequest { get; set; }


        public List<Wallet_Model> _walletData { get; set; }
    }
}
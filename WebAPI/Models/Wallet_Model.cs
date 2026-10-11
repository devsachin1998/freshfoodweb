using System;
using System.Collections.Generic;
using System.Web.Util;

namespace WebAPI.Models
{
    public class Wallet_Model
    {
        public int WalletID { get; set; }

        public int UserID { get; set; }

        public string UserName { get; set; }

        public decimal Amount { get; set; }

        public decimal Balance { get; set; }

        public string AddedDate { get; set; }



        public int TransactionId { get; set; }

        //public int UserId { get; set; } 

        //public string UserName { get; set; }

        //public decimal Amount { get; set; }

        //public string CreatedDate { get; set; }

        //public string UpdatedDate { get; set; }

        public string PaymentMode { get; set; }

        public string PaymentStatus { get; set; }
        public string Notification_count { get; set; }

    }

    public class Wallet_History_Model
    {
        public decimal Balance { get; set; }
        public List<Wallet_History> _historyList { get; set; }
    }

    public class Wallet_History
    {
        public int HistoryID { get; set; }
        public int WalletID { get; set; }

        public int UserID { get; set; }

        public string Type { get; set; }

        public decimal Amount { get; set; }

        public string OrderNo { get; set; }

        public string EntryDate { get; set; }
        public decimal Balance { get; set; }
    }
}
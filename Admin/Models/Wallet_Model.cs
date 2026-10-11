using System;
using System.Collections.Generic;


namespace Admin.Models
{
    public class Wallet_Model
    {
        public int WalletID { get; set; }

        public int UserID { get; set; }
        public string UserName { get; set; }

        public decimal Amount { get; set; }
        public decimal WalletBalance { get; set; }
        public decimal DueAmount { get; set; }
        public decimal DepositAmount { get; set; }

        public string CreatedDate { get; set; }

        public string UpdatedDate { get; set; }

        public string PaymentStatus { get; set; }


        public string payTotal { get; set; }


        public List<Wallet_History> _walletHistory { get; set; }


        public List<TransactionHistory_Model> _transactionHistory_Models { get; set; }

    }

    public class Wallet_History
    {
        public int HistoryID { get; set; }
        public int WalletID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }

        public string Type { get; set; }

        public decimal Amount { get; set; }

        public string Invoice { get; set; }
        public string Mobile { get; set; }

        public string OrderNo { get; set; }

        public string EntryDate { get; set; }

        public string Address { get; set; }
        public string PaymentMode { get; set; }
    }


    public class TransactionHistory_Model
    {
        public int id { get; set; }

        public int TransactionId { get; set; }

        public int UserId { get; set; }

        public string OnlinePayUserName { get; set; }

        public decimal OnlineAmount { get; set; }
        public string CreatedDate { get; set; }

        public string UpdatedDate { get; set; }

        public string PaymentMode { get; set; }

        public string PaymentStatus { get; set; }

 }



}
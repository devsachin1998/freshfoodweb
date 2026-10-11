using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class Balance_Model
    {
        public int PayID { get; set; }
        public decimal BalanceAmt {get; set; }
        public string GivenBy { get; set; }
        public DateTime? GivenDate { get; set; }
        public decimal AvailableBalance { get; set; }
        public List<BalanceList> balanceList { get; set; }
        
    }

    public class BalanceList
    {
        public int PayID { get; set; }
        public decimal BalanceAmt { get; set; }
        public string GivenBy { get; set; }
        public DateTime GivenDate { get; set; }
    }
}
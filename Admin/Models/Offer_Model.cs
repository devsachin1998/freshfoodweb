using System;
using System.Collections.Generic;

namespace Admin.Models
{
    public class Offer_Model
    {
        public int OfferId { get; set; }
     
        public string OfferName { get; set; }
        
        public string IsActive { get; set; }
    
        public string Date { get; set; }

        public string Type { get; set; }

        public string MinimumAmont { get; set; }

        public string ApplicableCashBack { get; set; }

        public string Amount { get; set; }

        public string TotalCashBack { get; set; }

        public string CreatedDate { get; set; }

        public string UpdatedDate { get; set; }

        public List<Offer_Model> _OfferList { get; set; }

        public List<Offer_details> _OfferDetails { get; set; }

    }

    public class Offer_details
    {

       public int Id { get; set; }

        public string OfferName { get; set; }
        public string Type { get; set; }

        public string MinimumAmont { get; set; }

        public string ApplicableCashBack { get; set; }

        public string Amount { get; set; }

        public string TotalCashBack { get; set; }

        public string CreatedDate { get; set; }

        public string UpdatedDate { get; set; }

        public string IsActive { get; set; }

    }
}
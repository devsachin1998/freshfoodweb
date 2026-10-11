using System.Collections.Generic;

namespace Admin.Models
{
    public class Order_Model
    {
        public int OrderID { get; set; }
        public string OrderNo { get; set; }
        public string OrderDate { get; set; }

        public int UserID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerAddress { get; set; }
        public string DeliveryAddress { get; set; } 
        


        public int NoofItem { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string CustomerRemark { get; set; }
        public string AdminRemark { get; set; }
        public decimal? Total { get; set; }
        public string SysDeliveryDate { get; set; }
        public string PaymentDate { get; set; }
        public decimal? PaidAmount { get; set; }

        public string Invoice { get; set; }

        public string DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }

        public decimal? BillingAmount { get; set; }


        public decimal? OrderAmount { get; set; }
        public decimal? OrderTotal { get; set; }


        public List<Order_Item> _itemList { get; set; }
    }

    public class Order_Item
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public string OrderNo { get; set; }

        public int ItemID { get; set; }
        public string ItemName { get; set; }

        public int SubItemID { get; set; }
        public int StockID { get; set; }
        public int MainID { get; set; }


        public string Weight { get; set; }

        public decimal OrderQty { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }

        public decimal DeliveryWeight { get; set; }
        public decimal DeliveryPrice { get; set; }

        public string AvailableStock { get; set; }

        public string SubItemUnitID  {get; set; }
        public string SubItemUnitName { get; set; }
    }
}
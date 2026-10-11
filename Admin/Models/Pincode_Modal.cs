using System.Collections.Generic;

namespace Admin.Models
{
    public class Pincode_Modal
    {
        public int PinID { get; set; }
        public string LocationName { get; set; }
        public string LocationCode { get; set; }
        public string IsActive { get; set; }

        public List<Pincode_Modal> _locationList { get; set; }
    }
}
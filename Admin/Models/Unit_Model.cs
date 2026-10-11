using System.Collections.Generic;

namespace Admin.Models
{
    public class Unit_Model
    {
        public int UnitID { get; set; }
        public string UnitName { get; set; }

        public List<Unit_Model> _unitList { get; set; }
    }
}
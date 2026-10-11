using System.Collections.Generic;

namespace Admin.Models
{
    public class Category_Model
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string IsActive { get; set; }

        public List<Category_Model> _categoryList { get; set; }
    }
}
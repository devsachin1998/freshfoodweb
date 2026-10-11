using System.Collections.Generic;

namespace WebAPI.Models
{
    public class Startup_Model
    {
        public string HelpLineNo { get; set; }
        public string Version { get; set; }
        public string holidaytext { get; set; }
        public List<Category> _categoryList { get; set; }
    }

    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
    }
}
namespace Admin.Models
{
    public class User_Model
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool? IsActive { get; set; }
        public string PasswordChangeDate { get; set; }

        public string CreatedDate { get; set; }
    }
}
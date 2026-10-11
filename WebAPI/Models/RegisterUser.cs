namespace WebAPI.Models
{
    public class RegisterUser
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; } = "";
        public bool IsActive { get; set; }
        public string Address { get; set; }
        public string fcmtoken { get; set; } = "";
        public string Version { get; set; } = "";
        public int WalletID { get; set; }
        public decimal WalletBalance { get; set; }

        public string Pincode { get; set; } = "";
    }
}
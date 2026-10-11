using System.Collections.Generic;

namespace WebAPI.Models
{
    public class Notification_Model
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Tittle { get; set; }

        public string Image { get; set; }

        public string Message { get; set; }

        public int[] SelectedValues { get; set; } = new int[0];
        

        public string Date { get; set; }


    }
}
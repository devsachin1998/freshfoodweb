using Admin.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Web;

namespace Admin.Helpers
{
    public class Print_Receipt
    {
        private PrintDocument PrintDocument;
        private Graphics graphics;
        private Order_Model order { set; get; }
        private int InitialHeight = 360;

        public Print_Receipt(Order_Model order)
        {
            this.order = order;
            AdjustHeight();
        }
        private void AdjustHeight()
        {
            var capacity = 5 * 5;
            InitialHeight += capacity;

            capacity = 5 * 5;
            InitialHeight += capacity;
        }
        public void Print(string printername)
        {
            PrintDocument = new PrintDocument();
           PrintDocument.PrinterSettings.PrinterName = "Microsoft XPS Document Writer";

            PrintDocument.PrintPage += new PrintPageEventHandler(FormatPage);
            PrintDocument.DocumentName = order.OrderNo;
            PrintDocument.PrinterSettings.PrintToFile = true;
            PrintDocument.PrinterSettings.PrintFileName = order.OrderNo;
            PrintDocument.Print();
        }
        void DrawAtStart(string text, int Offset)
        {
            int startX = 10;
            int startY = 5;
            Font minifont = new Font("Arial", 5);

            graphics.DrawString(text, minifont,
                     new SolidBrush(Color.Black), startX + 5, startY + Offset);
        }
        void InsertItem(string key, decimal? value, decimal? key1, decimal? value1, int Offset)
        {
            Font minifont = new Font("Arial", 5);
            int startX = 10;
            int startY = 5;

            graphics.DrawString(key, minifont,
                         new SolidBrush(Color.Black), startX + 5, startY + Offset);

            graphics.DrawString(value.ToString(), minifont,
                     new SolidBrush(Color.Black), startX + 130, startY + Offset);

            graphics.DrawString(key1.ToString(), minifont,
                         new SolidBrush(Color.Black), startX + 220, startY + Offset);

            graphics.DrawString(value1.ToString(), minifont,
                     new SolidBrush(Color.Black), startX + 270, startY + Offset);
        }
        void InsertHeaderStyleItem(string key, string value,string key1, string value1, int Offset)
        {
            int startX = 10;
            int startY = 5;
            Font itemfont = new Font("Arial", 5, FontStyle.Bold);

            graphics.DrawString(key, itemfont,
                         new SolidBrush(Color.Black), startX + 5, startY + Offset);

            graphics.DrawString(value, itemfont,
                     new SolidBrush(Color.Black), startX + 130, startY + Offset);

            graphics.DrawString(key1, itemfont,
             new SolidBrush(Color.Black), startX + 220, startY + Offset);

            graphics.DrawString(value1, itemfont,
                     new SolidBrush(Color.Black), startX + 270, startY + Offset);
        }
        void DrawLine(string text, Font font, int Offset, int xOffset)
        {
            int startX = 10;
            int startY = 5;
            graphics.DrawString(text, font,
                     new SolidBrush(Color.Black), startX + xOffset, startY + Offset);
        }
        void DrawSimpleString(string text, Font font, int Offset, int xOffset)
        {
            int startX = 10;
            int startY = 5;
            graphics.DrawString(text, font,
                     new SolidBrush(Color.Black), startX + xOffset, startY + Offset);
        }
        private void FormatPage(object sender, PrintPageEventArgs e)
        {
            graphics = e.Graphics;
            Font minifont = new Font("Arial", 5);
            Font itemfont = new Font("Arial", 6);
            Font smallfont = new Font("Arial", 8);
            Font mediumfont = new Font("Arial", 10);
            Font largefont = new Font("Arial", 12);
            int Offset = 10;
            int smallinc = 10, mediuminc = 12, largeinc = 15;


            graphics.DrawString("FreshFoodz", smallfont,
                  new SolidBrush(Color.Black), 12, Offset);

            Offset = Offset + largeinc;

            String underLine = "--------------------------------------------------------------";
            DrawLine(underLine, largefont, Offset, 0);

            Offset = Offset + mediuminc;
            DrawAtStart(" ", Offset);

            Offset = Offset + mediuminc;
            DrawAtStart("Order No: " + order.OrderNo, Offset);

            if (string.IsNullOrEmpty(order.CustomerName))
            {
                Offset = Offset + mediuminc;
                DrawAtStart("Customer Name: " + order.CustomerName, Offset);
            }

            if (string.IsNullOrEmpty(order.CustomerMobile))
            {
                Offset = Offset + mediuminc;
                DrawAtStart("Mob: " + order.CustomerMobile, Offset);
            }

            if (string.IsNullOrEmpty(order.CustomerAddress))
            {
                Offset = Offset + mediuminc;
                DrawAtStart("Address: " + order.CustomerAddress, Offset);
            }

            Offset = Offset + mediuminc;
            DrawAtStart("Order Date: " + order.OrderDate, Offset);

            Offset = Offset + mediuminc;
            DrawAtStart("Order Status: " + order.OrderStatus, Offset);

            Offset = Offset + smallinc;
            underLine = "--------------------------------------------------------------";
            DrawLine(underLine, largefont, Offset, 0);

            Offset = Offset + largeinc;

            InsertHeaderStyleItem("Item ", "Delivery Weight ", "Actual Price ","Price ", Offset);

            Offset = Offset + largeinc;
            foreach (var item in order._itemList)
            {
                InsertItem(item.ItemName + " x " + item.OrderQty, item.DeliveryWeight, item.Price, item.DeliveryPrice, Offset);
                Offset = Offset + smallinc;
            }

            underLine = "--------------------------------------------------------------";
            DrawLine(underLine, largefont, Offset, 0);

            Offset = Offset + largeinc;

            InsertItem(" Item Total: ", null, null, order.Total, Offset);

            Offset = Offset + smallinc;
            InsertHeaderStyleItem(" Amount Payable: ","","",order.BillingAmount.ToString(), Offset);

            Offset = Offset + largeinc;

            //Offset = Offset + smallinc;
            //String number = "Tel: " + "12345" + " - OR - " + "120";
            //DrawSimpleString(number, minifont, Offset, 35);

            Offset = Offset + 7;
            underLine = "--------------------------------------------------------------";
            DrawLine(underLine, largefont, Offset, 0);

            Offset = Offset + mediuminc;
            String greetings = "Thanks for using FreshFoodz! ";
            DrawSimpleString(greetings, mediumfont, Offset, 28);

            Offset = Offset + mediuminc;
            underLine = "--------------------------------------------------------------";
            DrawLine(underLine, largefont, Offset, 0);

            Offset += (2 * mediuminc);

            //Offset = Offset + largeinc;
            //string DrawnBy = "Meganos Softwares: 0312-0459491 - OR - 0321-6228321";
            //DrawSimpleString(DrawnBy, minifont, Offset, 15);
        }

        #region MyRegion

        //public void Print()
        //{
        //    var doc = new PrintDocument();
        //    doc.PrintPage += new PrintPageEventHandler(ProvideContent);
        //    doc.Print();
        //}

        //public void ProvideContent(object sender, PrintPageEventArgs e)
        //{
        //    Graphics graphics = e.Graphics;
        //    Font font = new Font("Courier New", 10);

        //    float fontHeight = font.GetHeight();

        //    int startX = 0;
        //    int startY = 0;
        //    int Offset = 20;

        //    e.PageSettings.PaperSize.Width = 50;
        //    graphics.DrawString("Welcome to MSST", new Font("Courier New", 8),
        //                        new SolidBrush(Color.Black), startX, startY + Offset);
        //    Offset = Offset + 20;

        //    graphics.DrawString("Ticket No:" + "4525554654545",
        //                new Font("Courier New", 14),
        //                new SolidBrush(Color.Black), startX, startY + Offset);
        //    Offset = Offset + 20;


        //    graphics.DrawString("Ticket Date :" + "21/12/215",
        //                new Font("Courier New", 14),
        //                new SolidBrush(Color.Black), startX, startY + Offset);

        //    Offset = Offset + 20;
        //    String underLine = "------------------------------------------";

        //    graphics.DrawString(underLine, new Font("Courier New", 14),
        //                new SolidBrush(Color.Black), startX, startY + Offset);

        //    Offset = Offset + 20;
        //    String Grosstotal = "Total Amount to Pay = " + "2566";

        //    Offset = Offset + 20;
        //    underLine = "------------------------------------------";
        //    graphics.DrawString(underLine, new Font("Courier New", 14),
        //                new SolidBrush(Color.Black), startX, startY + Offset);
        //    Offset = Offset + 20;

        //    graphics.DrawString(Grosstotal, new Font("Courier New", 14),
        //                new SolidBrush(Color.Black), startX, startY + Offset);

        //}


        #endregion
    }

}
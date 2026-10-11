using Admin.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;

namespace Admin.Helpers
{
    public class OrderSummary
    {
        #region Generate Order summary

        public static string GenerateOrderSummary(Order_Model data)
        {
            string fileName = "OrderSummary_" + data.OrderNo;

            string pdfPath = Path.GetFullPath(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/OrderSummary/" + fileName + ".pdf"));
            using (FileStream msReport = new FileStream(pdfPath, FileMode.Create))
            {
                //step 1
                using (Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 0f, 0f))
                {
                    try
                    {
                        PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, msReport);
                        pdfWriter.PageEvent = new ITextEvents();
                        Font ptext = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, 9);

                        pdfDoc.Open();

                        #region logo

                        Paragraph p = new Paragraph("\n");
                        pdfDoc.Add(p);

                        PdfPTable pdfTab = new PdfPTable(1)
                        {
                            WidthPercentage = 100
                        };
                        pdfTab.SetWidths(new float[] { 1.0f });
                        pdfTab.DefaultCell.Border = Rectangle.NO_BORDER;

                        //Row 1
                        Image logo = Image.GetInstance(Path.GetFullPath(System.Web.Hosting.HostingEnvironment.MapPath("~/Content/") + "ff.png"));
                        logo.ScalePercent(20f);
                        logo.Alignment = iTextSharp.text.Image.ALIGN_CENTER;

                        BaseColor white = BaseColor.WHITE;
                        BaseColor grey = BaseColor.WHITE;

                        PdfPCell cell = new PdfPCell(logo);
                        cell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;

                        cell.BorderWidthBottom = 0;
                        cell.BorderWidthTop = 0;
                        cell.BorderWidthLeft = 0;
                        cell.BorderWidthRight = 0;
                        pdfTab.AddCell(cell);


                        pdfDoc.Add(pdfTab);

                        #endregion

                        #region Company Details

                        Font fntBold = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, 9);
                        Font fntFontHdr = FontFactory.GetFont(BaseFont.HELVETICA, 7);

                        p = new Paragraph("\n\nThanks for choosing Freshfoodz, " + data.CustomerName + " ! Here are your order details:");
                        p.Alignment = Element.ALIGN_CENTER;
                        pdfDoc.Add(p);

                        #region To Details

                        p = new Paragraph("\n");
                        pdfDoc.Add(p);

                        pdfTab = new PdfPTable(1)
                        {
                            WidthPercentage = 100
                        };
                        pdfTab.SetWidths(new float[] { 1.5f });

                        fntBold = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, 9);
                        fntFontHdr = FontFactory.GetFont(BaseFont.HELVETICA, 9);
                        Font tableTitleFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, 10);
                        tableTitleFont.Color = BaseColor.BLACK.Darker();

                        p = new Paragraph(" Order Summary", tableTitleFont);
                        cell = new PdfPCell(new Phrase(p));
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.PaddingTop = 5;
                        cell.PaddingBottom = 5;
                        cell.VerticalAlignment = Element.ALIGN_TOP;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        pdfTab.AddCell(cell);

                        p = new Paragraph(" Order No: " + data.OrderNo, fntBold);
                        cell = new PdfPCell(new Phrase(p));
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.PaddingTop = 5;
                        cell.PaddingBottom = 5;
                        cell.BorderWidthBottom = 0;
                        cell.VerticalAlignment = Element.ALIGN_TOP;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        pdfTab.AddCell(cell);

                        p = new Paragraph(" To: " + data.CustomerName, fntFontHdr);
                        cell = new PdfPCell(new Phrase(p));
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.PaddingTop = 5;
                        cell.PaddingBottom = 5;
                        cell.BorderWidthBottom = 0;
                        cell.BorderWidthTop = 0;
                        cell.VerticalAlignment = Element.ALIGN_TOP;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        pdfTab.AddCell(cell);

                        p = new Paragraph(" Mob. No: " + data.CustomerMobile, fntFontHdr);
                        cell = new PdfPCell(new Phrase(p));
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.PaddingTop = 5;
                        cell.PaddingBottom = 5;
                        cell.BorderWidthBottom = 0;
                        cell.BorderWidthTop = 0;
                        cell.VerticalAlignment = Element.ALIGN_TOP;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        pdfTab.AddCell(cell);

                        p = new Paragraph(" Address: " + data.CustomerAddress, fntFontHdr);
                        cell = new PdfPCell(new Phrase(p));
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.PaddingTop = 5;
                        cell.PaddingBottom = 5;
                        cell.BorderWidthBottom = 0;
                        cell.BorderWidthTop = 0;
                        cell.VerticalAlignment = Element.ALIGN_TOP;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        pdfTab.AddCell(cell);

                        p = new Paragraph(" Order Status: " + data.OrderStatus, fntFontHdr);
                        cell = new PdfPCell(new Phrase(p));
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.PaddingTop = 5;
                        cell.PaddingBottom = 5;
                        cell.BorderWidthTop = 0;
                        cell.VerticalAlignment = Element.ALIGN_TOP;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        pdfTab.AddCell(cell);


                        pdfDoc.Add(pdfTab);

                        #endregion

                        p = new Paragraph("\n\n");
                        pdfDoc.Add(p);

                        pdfTab = new PdfPTable(5)
                        {
                            WidthPercentage = 100
                        };
                        pdfTab.SetWidths(new float[] { 1.5f, 0.5f, 0.5f, 0.5f, 0.5f });

                        p = new Paragraph(" Item Name", fntBold);
                        cell = new PdfPCell(new Phrase(p));
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.PaddingTop = 5;
                        cell.PaddingBottom = 5;
                        cell.VerticalAlignment = Element.ALIGN_TOP;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        pdfTab.AddCell(cell);

                        p = new Paragraph(" Quantity", fntBold);
                        cell = new PdfPCell(new Phrase(p));
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.PaddingTop = 5;
                        cell.PaddingBottom = 5;
                        cell.VerticalAlignment = Element.ALIGN_TOP;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        pdfTab.AddCell(cell);

                        p = new Paragraph(" Delivery Weight", fntBold);
                        cell = new PdfPCell(new Phrase(p));
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.PaddingTop = 5;
                        cell.PaddingBottom = 5;
                        cell.VerticalAlignment = Element.ALIGN_TOP;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        pdfTab.AddCell(cell);

                        p = new Paragraph(" Actual Price", fntBold);
                        cell = new PdfPCell(new Phrase(p));
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.PaddingTop = 5;
                        cell.PaddingBottom = 5;
                        cell.BorderColorLeft = BaseColor.BLACK;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        pdfTab.AddCell(cell);

                        p = new Paragraph(" Price", fntBold);
                        cell = new PdfPCell(new Phrase(p));
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.PaddingTop = 5;
                        cell.PaddingBottom = 5;
                        cell.BorderColorLeft = BaseColor.BLACK;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        pdfTab.AddCell(cell);

                        foreach (var item in data._itemList)
                        {
                            p = new Paragraph(" " + item.ItemName + " " + item.Weight, fntFontHdr);
                            cell = new PdfPCell(new Phrase(p));
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            cell.PaddingTop = 5;
                            cell.PaddingBottom = 5;
                            cell.VerticalAlignment = Element.ALIGN_TOP;
                            cell.HorizontalAlignment = Element.ALIGN_LEFT;
                            pdfTab.AddCell(cell);

                            p = new Paragraph(" " + item.OrderQty, fntFontHdr);
                            cell = new PdfPCell(new Phrase(p));
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.PaddingTop = 5;
                            cell.PaddingBottom = 5;
                            cell.VerticalAlignment = Element.ALIGN_TOP;
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            pdfTab.AddCell(cell);

                            p = new Paragraph(" " + item.DeliveryWeight, fntFontHdr);
                            cell = new PdfPCell(new Phrase(p));
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.PaddingTop = 5;
                            cell.PaddingBottom = 5;
                            cell.VerticalAlignment = Element.ALIGN_TOP;
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            pdfTab.AddCell(cell);

                            p = new Paragraph(" " + "INR " + item.Price, fntFontHdr);
                            cell = new PdfPCell(new Phrase(p));
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.PaddingTop = 5;
                            cell.PaddingBottom = 5;
                            cell.VerticalAlignment = Element.ALIGN_TOP;
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            pdfTab.AddCell(cell);

                            p = new Paragraph(" " + "INR " + item.DeliveryPrice, fntFontHdr);
                            cell = new PdfPCell(new Phrase(p));
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.PaddingTop = 5;
                            cell.PaddingBottom = 5;
                            cell.VerticalAlignment = Element.ALIGN_TOP;
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            pdfTab.AddCell(cell);
                        }

                        pdfDoc.Add(pdfTab);


                        #region Total Amount Details

                        p = new Paragraph("\n");
                        pdfDoc.Add(p);

                        pdfTab = new PdfPTable(2)
                        {
                            WidthPercentage = 100
                        };
                        pdfTab.SetWidths(new float[] { 1.5f, 0.2f });

                        fntBold = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, 10);
                        fntFontHdr = FontFactory.GetFont(BaseFont.HELVETICA, 10);
                        tableTitleFont = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, 11);
                        tableTitleFont.Color = BaseColor.BLACK.Darker();

                        //p = new Paragraph(" Item Total :", fntBold);
                        //cell = new PdfPCell(new Phrase(p));
                        //cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        //cell.PaddingTop = 5;
                        //cell.PaddingBottom = 5;
                        //cell.BorderWidthRight = 0;
                        //cell.BorderWidthLeft = 0;
                        //cell.BorderWidthRight = 0;
                        //cell.BorderWidthTop = 0;
                        //cell.BorderWidthBottom = 0;
                        //cell.VerticalAlignment = Element.ALIGN_TOP;
                        //cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        //pdfTab.AddCell(cell);

                        //p = new Paragraph(" INR " + data.Total, fntFontHdr);
                        //cell = new PdfPCell(new Phrase(p));
                        //cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        //cell.PaddingTop = 5;
                        //cell.PaddingBottom = 5;
                        //cell.BorderWidthLeft = 0;
                        //cell.BorderWidthRight = 0;
                        //cell.BorderWidthTop = 0;
                        //cell.BorderWidthBottom = 0;
                        //cell.VerticalAlignment = Element.ALIGN_TOP;
                        //cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        //pdfTab.AddCell(cell);

                        p = new Paragraph(" Order Total: ", fntBold);
                        cell = new PdfPCell(new Phrase(p));
                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        cell.PaddingTop = 5;
                        cell.PaddingBottom = 5;
                        cell.BorderWidthLeft = 0;
                        cell.BorderWidthRight = 0;
                        cell.BorderWidthTop = 0;
                        cell.BorderWidthBottom = 0;
                        cell.VerticalAlignment = Element.ALIGN_TOP;
                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        pdfTab.AddCell(cell);

                        p = new Paragraph(" INR " + data.BillingAmount, fntBold);
                        cell = new PdfPCell(new Phrase(p));
                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        cell.PaddingTop = 5;
                        cell.PaddingBottom = 5;
                        cell.BorderWidthLeft = 0;
                        cell.BorderWidthRight = 0;
                        cell.BorderWidthTop = 0;
                        cell.BorderWidthBottom = 0;
                        cell.VerticalAlignment = Element.ALIGN_TOP;
                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        pdfTab.AddCell(cell);


                        pdfDoc.Add(pdfTab);

                        #endregion

                        #endregion

                        pdfDoc.Close();
                    }
                    catch (Exception ex)
                    {
                        fileName = "Error";
                        Console.WriteLine(ex.ToString());
                    }
                }
                return fileName + ".pdf";
            }
        }

        #endregion

        #region PDF Content

        public class ITextEvents : PdfPageEventHelper
        {
            // This is the contentbyte object of the writer
            //PdfContentByte cb;

            // we will put the final number of pages in a template
            //PdfTemplate headerTemplate, footerTemplate;

            // this is the BaseFont we are going to use for the header / footer
            //BaseFont bf = null;

            // This keeps track of the creation time

            DateTime PrintTime = DateTime.Now;

            #region Fields
            private string _header;
            #endregion

            #region Properties
            public string Header
            {
                get { return _header; }
                set { _header = value; }
            }
            #endregion
        }

        public static PdfPCell getTableCell(string content, int rowspan, int colspan, Font f, BaseColor color, int alignDirection)
        {
            PdfPCell cell = new PdfPCell(new Phrase(content, f));

            if (alignDirection == 1)
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
            else if (alignDirection == 2)
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            else if (alignDirection == 0)
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Colspan = colspan;
            cell.Rowspan = rowspan;
            cell.BackgroundColor = color;
            return cell;
        }

        public static PdfPCell getTableCell(Paragraph content, int rowspan, int colspan, Font f, BaseColor color, int alignDirection)
        {
            PdfPCell cell = new PdfPCell(new Phrase(content));

            if (alignDirection == 1)
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
            else if (alignDirection == 2)
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            else if (alignDirection == 0)
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Colspan = colspan;
            cell.Rowspan = rowspan;
            cell.BackgroundColor = color;
            return cell;
        }

        #endregion
    }
}
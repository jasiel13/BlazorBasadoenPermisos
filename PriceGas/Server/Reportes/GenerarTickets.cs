//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Runtime.CompilerServices;
//using System.Diagnostics;
//using System.Globalization;
//using QRCodeEncoderLibrary;
//using BarcodeLib;
//using PriceGas.Server.Datos;
//using PriceGas.Shared.Entidades;
//using Microsoft.AspNetCore.Hosting;
//using PriceGas.Server.Helpers;

//namespace PriceGas.Server.Reportes
//{
//    public class GenerarTickets
//    {
//        private static PdfPCell ImageCell(string path, float scale, int align)
//        {
//            iTextSharp.text.Image instance = iTextSharp.text.Image.GetInstance(path);
//            return ImageToCell(scale, align, instance);
//        }
//        private static PdfPCell LogoCell(string path, int px)
//        {
//            iTextSharp.text.Image instance = iTextSharp.text.Image.GetInstance(path);

//            float width = instance.Width;
//            float height = instance.Height;
//            float targetwidth = 68;
//            float scale = (targetwidth * px) / width;
//            return ImageToCell(scale, 0, instance);
//        }
//        private static PdfPCell ImageToCell(float scale, int align, iTextSharp.text.Image image)
//        {
//            image.ScalePercent(scale);
//            return new PdfPCell(image)
//            {
//                BorderColor = BaseColor.White,
//                VerticalAlignment = 5,
//                HorizontalAlignment = align,
//                PaddingBottom = 0f,
//                PaddingTop = 0f
//            };
//        }     
//        public class PdfPageEvents : PdfPageEventHelper
//        {            
//            [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
//            private List<Equipo> datosparaeldocumento;
//            public override void OnEndPage(PdfWriter writer, Document document)
//            {
//                PdfPTable table = new PdfPTable(1);
//                Phrase phrase = null;
//                PdfPCell cell = null;
//                BaseColor color = new BaseColor(0x15, 0x7b, 0xff);
//                BaseColor fontColor = new BaseColor(0xa9, 0xa9, 0xa9);
//                BaseColor color3 = new BaseColor(220, 220, 220);
//                BaseColor bLACK = BaseColor.Black;
//                int px = 40;

//                table = new PdfPTable(2)
//                {
//                    DefaultCell = { Border = 0 },
//                    TotalWidth = (document.PageSize.Width - document.LeftMargin) - document.RightMargin,
//                    LockedWidth = true
//                };
//                float[] relativeWidths = new float[] { 0.15f, 0.85f };
//                table.SetWidths(relativeWidths);

//                //alternativa de usar Path.Combine(environment.WebRootPath, "logo_simsa.png");   
//                string logo = $"{Directory.GetCurrentDirectory()}{@"\wwwroot\ImgServer\logo_simsa.png"}";                              
//                if (File.Exists(logo))
//                {
//                    table.AddCell(LogoCell(logo, px));
//                }
//                else
//                {                    
//                    table.AddCell(ImageCell(logo, 8f, 1));
//                }

//                PdfPTable table2 = new PdfPTable(4)
//                {
//                    DefaultCell = { Border = 0 },
//                    TotalWidth = table.TotalWidth * 0.80f,
//                    LockedWidth = true
//                };
//                table2.SetWidths(new float[] { 0.25f, 0.25f, 0.25f, 0.25f });

//                phrase = new Phrase();
//                cell = this.PhraseCell(phrase, 8);
//                cell.VerticalAlignment = 4;
//                cell.Colspan = 4;
//                table2.AddCell(cell);
//                table2.AddCell(cell);
//                table.AddCell(table2);
//                table.WriteSelectedRows(0, -1, document.LeftMargin, document.PageSize.Height, writer.DirectContent);
//            }

//            private PdfPCell PhraseCell(Phrase phrase, int align) =>
//                new PdfPCell(phrase)
//                {
//                    BorderColor = BaseColor.White,
//                    VerticalAlignment = 4,
//                    HorizontalAlignment = align,
//                    PaddingBottom = 2f,
//                    PaddingTop = 0f
//                };
//            public List<Equipo> _equipo
//            {
//                [CompilerGenerated]
//                get =>
//                    this.datosparaeldocumento;
//                [CompilerGenerated]
//                set =>
//                    datosparaeldocumento = value;
//            }
//        }
//        private static PdfPCell PhraseCell(Phrase phrase, int align) =>
//             new PdfPCell(phrase)
//             {
//                 BorderColor = BaseColor.White,
//                 VerticalAlignment = 4,
//                 HorizontalAlignment = align,
//                 PaddingBottom = 2f,
//                 PaddingTop = 0f
//             };
//        public static void Detalle(ref PdfPTable table, BaseColor FontColor, string Value, int Align, float Size = 8f, int Tipo = 0, int ColSpan = 1, int VerticalAlignment = 4)
//        {
//            PdfPCell cell = PhraseCell(new Phrase(Value, FontFactory.GetFont("Arial", Size, Tipo, FontColor)), Align);
//            if (ColSpan > 1)
//            {
//                cell.Colspan = ColSpan;
//            }
//            cell.VerticalAlignment = VerticalAlignment;
//            //Quitamos el Borde de cada celda de la Tabla
//            cell.Border = Rectangle.NO_BORDER;
//            table.AddCell(cell);
//        }
//        public static void TitDetalle(ref PdfPTable table, BaseColor FontColor, BaseColor BackColor, BaseColor LineColor, string Title, int Align, float Size = 8f, int Tipo = 0, int ColSpan = 1, int VerticalAlignment = 4)
//        {
//            PdfPCell cell = PhraseCell(new Phrase(Title, FontFactory.GetFont("Arial", Size, Tipo, FontColor)), Align);
//            cell.BorderColorBottom = LineColor;
//            cell.BorderColorLeft = BackColor;
//            cell.BorderColorRight = BackColor;
//            cell.BorderColorTop = BackColor;
//            cell.BorderWidthBottom = 0.4f;
//            cell.BackgroundColor = BackColor;
//            if (ColSpan > 1)
//            {
//                cell.Colspan = ColSpan;
//            }
//            cell.VerticalAlignment = VerticalAlignment;
//            table.AddCell(cell);
//        }
        
//        #region Etiqueta
//        public static byte[] Etiqueta(List<Equipo> equipos)
//        {            
//            Rectangle tamaño = new Rectangle(252, 79);       
//            Document document = new Document(tamaño, 0f, 0f, 5f, 0f);          
//            iTextSharp.text.Font font = FontFactory.GetFont("Arial", 8f, 0, BaseColor.Black);
//            using (MemoryStream stream = new MemoryStream())
//            {
//                PdfWriter instance = PdfWriter.GetInstance(document, stream);
//                PdfPageEvents events = new PdfPageEvents
//                {
//                    _equipo = equipos
//                };
//                instance.PageEvent = events;
//                Phrase phrase = null;
//                PdfPTable table = null;
//                PdfPCell cell = null;
//                BaseColor lineColor = new BaseColor(0x15, 0x7b, 0xff);
//                BaseColor fontColor = new BaseColor(0xff, 0xff, 0xff);
//                BaseColor plata = new BaseColor(0xa9, 0xa9, 0xa9);
//                BaseColor backColor = new BaseColor(220, 220, 220);
//                BaseColor bLACK = BaseColor.Black;
//                document.Open();                

//                table = new PdfPTable(5);
//                table.SetWidths(new float[] { 0.10f, 0.20f, 0.25f, 0.25f, 0.20f });

//                table.TotalWidth = 248f;
//                table.LockedWidth = true;               
//                table.HorizontalAlignment = 0;              

//                foreach (var equipo in equipos)
//                {
//                    //string logo = @"C:\Users\innovacion2\Pictures\logo_simsa_chico.png";
//                    //MemoryStream ms = new MemoryStream();
//                    //using (Stream input = File.OpenRead(logo))
//                    //{
//                    //    input.CopyTo(ms);
//                    //}
//                    //ms.Position = 0;
//                    //iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(ms);
//                    //table.AddCell(ImageToCell(20f, 1, image));

//                    Detalle(ref table, bLACK, "\n", 0, 9f, 0, 5, 4);
//                    Detalle(ref table, bLACK, "\n" + "Inventario", 0, 9f, 0, 5, 4);
//                    Detalle(ref table, bLACK, "Sistemas: Mi Gasolina", 0, 9f, 0, 5, 4);
//                    Detalle(ref table, bLACK, $"Código:{equipo.EquipoId}", 0, 9f, 1, 5, 4);

//                    string nombrecat = "";
//                    foreach (var categoria in equipo.CategoriaEquipo)
//                    {
//                        nombrecat = categoria.Categoria.Nombre;                       
//                    }
//                    Detalle(ref table, bLACK, $"Categoría:{nombrecat}", 0, 9f, 0, 5, 4);
//                }

//                document.Add(table);
//                document.Close();

//                byte[] arch = stream.ToArray();
//                stream.Close();
//                return arch;
//            }
//        }
//        #endregion Etiqueta      
//    }
//}

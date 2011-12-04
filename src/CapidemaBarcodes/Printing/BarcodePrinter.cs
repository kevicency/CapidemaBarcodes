using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using codesomnia.CapidemaBarcodes.Model;
using codesomnia.CapidemaBarcodes.Properties;
using codesomnia.CapidemaBarcodes.Views;
using Size = System.Windows.Size;

namespace codesomnia.CapidemaBarcodes.Printing
{
    [Export]
    public class BarcodePrinter
    {
        const int RowCount = 13;
        const int ColCount = 5;

        private Settings Settings { get { return Settings.Default; } }

        public BarcodePrinter()
        {
            Ean13Settings = new Ean13Settings
                                          {
                                              BarCodeHeight = Settings.BarHeight,
                                              BarWidth = Settings.BarWidth,
                                              TopMargin = 0,
                                              LeftMargin = 0,
                                              RightMargin = 0,
                                              BottomMargin = 0,
                                          };
        }

        protected Ean13Settings Ean13Settings { get; private set; }

        public Barcode[] Barcodes { get; private set; }
        public int GlobalIndex { get; set; }

        public void Print(IEnumerable<Barcode> barcodes)
        {
            if (!barcodes.Any()) return;

            Barcodes = Flatten(barcodes.Where(x => !x.HasError));
            GlobalIndex = 0;

            var pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                var availableSize = new Size(pd.PrintableAreaWidth, pd.PrintableAreaHeight);
                var grids = CreateGrids(availableSize, Settings.Default.PageMarginLR, Settings.Default.PageMarginTB);

                var paginator = new GridPaginator(grids, availableSize);
                pd.PrintDocument(paginator, "Barcodes");
            }
        }

        private Barcode[] Flatten(IEnumerable<Barcode> barcodes)
        {
            var count = barcodes.Sum(x => x.Quantity);
            var array = new Barcode[count];
            int global = 0;
            foreach (var barcode in barcodes)
            {
                for (int i = 0; i < barcode.Quantity; i++)
                {
                    array[global] = barcode;

                    global += 1;
                }
            }

            return array;
        }

        private Grid[] CreateGrids(Size availableSize, double marginLR, double marginTB)
        {
            var gridSize = new Size(availableSize.Width - 2 * marginLR, availableSize.Height - 2 * marginTB);
            var grids = new List<Grid>();

            while (GlobalIndex < Barcodes.Length)
            {
                var grid = CreateGrid(gridSize);
                grid.Margin = new Thickness(marginLR, marginTB, marginLR, marginTB);

                for (int y = 0; y < RowCount; y++)
                {
                    for (int x = 0; x < ColCount; x++)
                    {
                        if (GlobalIndex < Barcodes.Length)
                        {
                            var barcode = Barcodes[GlobalIndex];
                            var template = GetPrintTemplate(barcode);

                            template.SetValue(Grid.ColumnProperty, x);
                            template.SetValue(Grid.RowProperty, y);

                            grid.Children.Add(template);

                            GlobalIndex++;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                grid.Measure(availableSize);
                grid.Arrange(new Rect(availableSize));
                grid.UpdateLayout();

                grids.Add(grid);
            }

            return grids.ToArray();
        }


        private static Grid CreateGrid(Size availableSize)
        {
            var rowHeight = availableSize.Height / RowCount;
            var columnWidth = availableSize.Width / ColCount;

            var grid = new Grid();
            //grid.ShowGridLines = true;
            //grid.Background = new SolidColorBrush(Colors.DodgerBlue);
            for (int x = 0; x < ColCount; x++) grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(columnWidth, GridUnitType.Pixel) });
            for (int y = 0; y < RowCount; y++) grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(rowHeight, GridUnitType.Pixel) });

            return grid;
        }

        public BarcodePrintTemplate GetPrintTemplate(Barcode barcode)
        {
            dynamic printData = new ExpandoObject();
            printData.ProductCode = barcode.ProductCode;
            printData.Image = CreateBarcodeImage(barcode.EanCode);

            var template = new BarcodePrintTemplate();
            template.DataContext = printData;
            template.Margin = new Thickness(Settings.CodeMarginLR, Settings.CodeMarginTB, Settings.CodeMarginLR, Settings.CodeMarginTB);
            return template;
        }

        private BitmapImage CreateBarcodeImage(string eanCode)
        {
            var ean13 = new Ean13(eanCode, null, Ean13Settings);
            var img = ean13.Paint();

            return ConvertImage(img);
        }

        public static BitmapImage ConvertImage(System.Drawing.Image img)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                var bImg = new BitmapImage();
                bImg.BeginInit();
                bImg.StreamSource = new MemoryStream(ms.ToArray());
                bImg.EndInit();

                return bImg;
            }
        }
    }
}
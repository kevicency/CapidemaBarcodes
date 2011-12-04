using System.ComponentModel.Composition;
using Caliburn.Micro;
using codesomnia.CapidemaBarcodes.Model;
using codesomnia.CapidemaBarcodes.Properties;

namespace codesomnia.CapidemaBarcodes.ViewModels
{
    [Export]
    public class ConfigViewModel : Screen
    {
        public double CodeMarginLR { get; set; }
        public double CodeMarginTB { get; set; }
        public double PageMarginTB { get; set; }
        public double PageMarginLR { get; set; }
        public int ProductCodeSize { get; set; }

        public ConfigViewModel()
        {
            DisplayName = "Konfiguration";
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            CodeMarginLR = Settings.Default.CodeMarginLR;
            CodeMarginTB = Settings.Default.CodeMarginTB;
            PageMarginTB = Settings.Default.PageMarginTB;
            PageMarginLR = Settings.Default.PageMarginLR;
            ProductCodeSize = Settings.Default.ProductCodeSize;
        }

        public void Save()
        {
            Settings.Default.CodeMarginLR = CodeMarginLR;
            Settings.Default.CodeMarginTB = CodeMarginTB;
            Settings.Default.PageMarginTB = PageMarginTB;
            Settings.Default.PageMarginLR = PageMarginLR;
            Settings.Default.ProductCodeSize = ProductCodeSize;

            Settings.Default.Save();

            TryClose();
        }
    }

    [Export]
    public class BarcodeManagerViewModel : Screen
    {
        public Barcode Editable { get; private set; }
        public IObservableCollection<Barcode> Barcodes { get; private set; }

        public BarcodeManagerViewModel()
        {
            Barcodes = new BindableCollection<Barcode>();
            Editable = new Barcode();
        }

        public void AddBarcode()
        {
            Barcodes.Insert(0, Editable);

            Editable = new Barcode() { Quantity = 1 };
        }

        public void RemoveBarcode(Barcode barcode)
        {
            Barcodes.Remove(barcode);
        }

        public void Import(Barcode barcode)
        {
            Barcodes.Add(barcode);
        }

        public void Clear()
        {
            Barcodes.Clear();
        }
    }
}


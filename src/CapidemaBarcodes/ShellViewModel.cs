using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Caliburn.Micro.Contrib;
using Caliburn.Micro.Contrib.Interaction;
using Caliburn.Micro.Contrib.Results;
using codesomnia.CapidemaBarcodes.Model;
using codesomnia.CapidemaBarcodes.Printing;
using codesomnia.CapidemaBarcodes.ViewModels;

namespace codesomnia.CapidemaBarcodes
{
    [Export(typeof(IShell))]
    public class ShellViewModel : Screen, IShell
    {
        public BarcodeManagerViewModel BarcodeManager { get; set; }
        public BarcodePrinter BarcodePrinter { get; set; }
        public CsvImporter CsvImporter { get; set; }

        [ImportingConstructor]
        public ShellViewModel(BarcodeManagerViewModel barcodeManager, BarcodePrinter barcodePrinter, CsvImporter csvImporter)
        {
            BarcodeManager = barcodeManager;
            BarcodePrinter = barcodePrinter;
            CsvImporter = csvImporter;

            barcodeManager.ConductWith(this);

            DisplayName = "Capidema Barcodes";
        }

        public IEnumerable<IResult> Print()
        {
            if (BarcodeManager.Barcodes.Any(x => x.HasError))
                yield return new Warning("Fehlerhafte Barcodes werden nicht gedruckt !", Answer.Ok)
                        .AsResult();

            BarcodePrinter.Print(BarcodeManager.Barcodes);
        }

        public IEnumerable<IResult> Export()
        {
            var saveFileResult = new SaveFileResult()
                    .FilterFiles(x => x.AddFilter("csv", isDefault: true).WithDescription("CSV Datei"))
                    .In(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));

            yield return saveFileResult;

            var exportResult = new DelegateResult(() => CsvImporter.Export(BarcodeManager.Barcodes, saveFileResult.FileName));

            yield return exportResult
                .Rescue().With(coroutine: ExportRescue);
        }

        public IEnumerable<IResult> Import()
        {
            var openFileResult = new OpenFileResult()
                    .FilterFiles(x => x.AddFilter("csv", isDefault: true).WithDescription("CSV Datei"))
                    .In(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));

            yield return openFileResult
                .Rescue().With(coroutine: ImportRescue);

            var clearBeforeQuestion = new Question("Frage", "Aktuelle Liste vor dem Import verwerfen?", Answer.Yes, Answer.No);
            yield return clearBeforeQuestion.AsResult();


            var importResult = new DelegateResult(() =>
                                                 {
                                                     if (clearBeforeQuestion.GivenResponse == Answer.Yes)
                                                     {
                                                         BarcodeManager.Clear();
                                                     }
                                                     foreach (var barcode in CsvImporter.Import(openFileResult.FileName))
                                                     {
                                                         BarcodeManager.Import(barcode);
                                                     }
                                                 });

            yield return importResult
                .Rescue().With(coroutine: ImportRescue);
        }

        private IEnumerable<IResult> ExportRescue(Exception ex)
        {
            yield return new Error(string.Format("Fehler beim exportieren.\n{0}", ex.Message), Answer.Ok)
                .AsResult();
        }

        private IEnumerable<IResult> ImportRescue(Exception ex)
        {
            yield return new Error(string.Format("Fehler beim importieren.\n{0}", ex.Message), Answer.Ok)
                .AsResult();
        }

        public void ShowConfig()
        {
            var configVm = IoC.Get<ConfigViewModel>();
            IoC.Get<IWindowManager>().ShowDialog(configVm);
        }
    }
}

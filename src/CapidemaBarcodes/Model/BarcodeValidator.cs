using System.Collections.Generic;
using codesomnia.CapidemaBarcodes.Printing;

namespace codesomnia.CapidemaBarcodes.Model
{
    public class BarcodeValidator
    {
        private static BarcodeValidator _instance;

        public static BarcodeValidator Instance
        {
            get { return _instance ?? (_instance = new BarcodeValidator()); }
        }

        public ValidationResult[] Validate(Barcode barcode)
        {
            var results = new List<ValidationResult>();

            foreach (var validationResult in ValidateEan(barcode))
            {
                results.Add(validationResult);
            }
            foreach (var validationResult in ValidateQuantity(barcode))
            {
                results.Add(validationResult);
            }

            return results.ToArray();
        }

        private IEnumerable<ValidationResult> ValidateEan(Barcode barcode)
        {
            var length = barcode.EanCode != null
                                 ? barcode.EanCode.Length
                                 : 0;

            if (length != 13)
            {
                yield return new ValidationResult
                             {
                                 Error = string.Format("Ean Code muss 13 Stellen besitzen, hat aber {0} Stellen.", length),
                                 Property = "EanCode",
                                 Value = barcode.EanCode
                             };
            }
            else
            {
                if (!Ean13.HasValidChecksum(barcode.EanCode))
                {
                    yield return new ValidationResult
                    {
                        Error = "Ean nicht gültig, da Prüfziffer inkorrekt.",
                        Property = "EanCode",
                        Value = barcode.EanCode
                    };
                }
            }
        }

        private IEnumerable<ValidationResult> ValidateQuantity(Barcode barcode)
        {
            if (barcode.Quantity <= 0)
            {
                yield return new ValidationResult
                             {
                                 Error = "Anzahl muss >= 1 sein",
                                 Property = "Quantity",
                                 Value = barcode.EanCode
                             };
            }
        }
    }
}
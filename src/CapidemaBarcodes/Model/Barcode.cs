using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;

namespace codesomnia.CapidemaBarcodes.Model
{
    public class Barcode : PropertyChangedBase, IDataErrorInfo
    {
        private BarcodeValidator Validator { get { return BarcodeValidator.Instance; } }

        public string ProductCode { get; set; }
        public string EanCode { get; set; }
        public int Quantity { get; set; }

        public string this[string columnName]
        {
            get
            {
                var errors = Validator.Validate(this);
                HasError = errors.Any();
                var error = errors.SingleOrDefault(x => x.Property == columnName);
                return error != null ? error.Error : null;
            }
        }

        public string Error { get; set; }

        private bool? _hasError;
        public bool HasError
        {
            get
            {
                if (!_hasError.HasValue)
                    _hasError = Validator.Validate(this).Any();

                return _hasError.Value;
            }
            set { _hasError = value; }
        }
    }
}
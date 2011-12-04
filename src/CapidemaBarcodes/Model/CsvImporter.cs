using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace codesomnia.CapidemaBarcodes.Model
{
    class CaseInsesitiveComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == null || y == null) return false;
            return x.Equals(y, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return obj != null ? obj.GetHashCode() : 0;
        }
    }
    [Export]
    public class CsvImporter
    {
        IEqualityComparer<string> _comparer = new CaseInsesitiveComparer();

        public IEnumerable<Barcode> Import(string file)
        {
            var lines = File.ReadAllLines(file);

            for (int i = 0; i < lines.Length; i++)
            {
                var data = lines[i].Split(',', ';', '\t');

                if (i == 0 && data.Length > 1 && data[1].Equals("EAN", StringComparison.InvariantCultureIgnoreCase)) continue;

                int quantity = 0;
                if (data.Length >= 3)
                {
                    int.TryParse(data[2], out quantity);
                }

                var barcode = new Barcode
                              {
                                  EanCode = data.Length > 1 ? data[1] : null,
                                  ProductCode = data.Length > 0 ? data[0] : null,
                                  Quantity = quantity,
                              };

                yield return barcode;
            }
        }

        public void Export(IEnumerable<Barcode> barcodes, string file)
        {
            var csvData = barcodes
                    .Select(x => string.Format("{0};{1};{2}", x.ProductCode, x.EanCode, x.Quantity))
                    .ToArray();
            var csv = string.Join(Environment.NewLine, csvData);

            File.WriteAllText(file, csv);
        }
    }
}
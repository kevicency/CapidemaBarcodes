using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace codesomnia.CapidemaBarcodes.Printing
{
    public class Ean13
    {
        #region Static initialization

        static Dictionary<int, Pattern> codes = new Dictionary<int, Pattern>();
        static Dictionary<int, Parity> parity = new Dictionary<int, Parity>();

        static Ean13()
        {
            //      #  LEFT ODD   LEFT EVEN  RIGHT
            AddCode(0, "0001101", "0100111", "1110010");
            AddCode(1, "0011001", "0110011", "1100110");
            AddCode(2, "0010011", "0011011", "1101100");
            AddCode(3, "0111101", "0100001", "1000010");
            AddCode(4, "0100011", "0011101", "1011100");
            AddCode(5, "0110001", "0111001", "1001110");
            AddCode(6, "0101111", "0000101", "1010000");
            AddCode(7, "0111011", "0010001", "1000100");
            AddCode(8, "0110111", "0001001", "1001000");
            AddCode(9, "0001011", "0010111", "1110100");

            AddParity(0, "ooooo");
            AddParity(1, "oeoee");
            AddParity(2, "oeeoe");
            AddParity(3, "oeeeo");
            AddParity(4, "eooee");
            AddParity(5, "eeooe");
            AddParity(6, "eeeoo");
            AddParity(7, "eoeoe");
            AddParity(8, "eoeeo");
            AddParity(9, "eeoeo");
        }

        static void AddCode(int digit, string lhOdd, string lhEven, string rh)
        {
            Pattern p = new Pattern();
            p.LhOdd = lhOdd; p.LhEven = lhEven; p.Rh = rh;
            codes.Add(digit, p);
        }

        static void AddParity(int digit, string par)
        {
            parity.Add(digit, new Parity(par));
        }

        #endregion

        private Ean13Settings settings;
        private string code;
        private string title;

        public Ean13(string code, string title)
            : this(code, title, new Ean13Settings())
        {
        }

        public Ean13(string code, string title, Ean13Settings settings)
        {
            this.settings = settings;
            this.code = code;
            this.title = title;

            if (!CheckCode(code))
                throw new ArgumentException("Invalid EAN-13 code specified.");
        }

        public static bool CheckCode(string code)
        {
            if (code == null || code.Length != 13)
                return false;

            int res;
            foreach (char c in code)
                if (!int.TryParse(c.ToString(), out res))
                    return false;

            return HasValidChecksum(code);
        }

        public static bool HasValidChecksum(string code)
        {
            char check = (char)('0' + CalculateChecksum(code.Substring(0, 12)));

            return code[12] == check;
        }

        public static int CalculateChecksum(string code)
        {
            if (code == null || code.Length != 12)
                throw new ArgumentException("Code length should be 12, i.e. excluding the checksum digit");

            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int v;
                if (!int.TryParse(code[i].ToString(), out v))
                    throw new ArgumentException("Invalid character encountered in specified code.");
                sum += (i % 2 == 0 ? v : v * 3);
            }
            int check = 10 - (sum % 10);
            return check % 10;
        }

        private int top;

        public Image Paint()
        {
            top = settings.TopMargin;

            Graphics g = Graphics.FromImage(new Bitmap(1, 1));

            int width = (3 + 6 * 7 + 5 + 6 * 7 + 3) * settings.BarWidth + settings.LeftMargin + settings.RightMargin + (int)g.MeasureString(code[0].ToString(), settings.Font).Width;
            int height = settings.BarCodeHeight;

            if (title != null)
            {
                int h = (int)g.MeasureString(title, settings.Font).Height;
                height += h;
                top += h;
            }

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(bmp);

            int left = settings.LeftMargin;

            //LEFT GUARD
            left = DrawLeftGuard(settings, g, code[0].ToString(), left);

            //LEFT GROUP
            int first = int.Parse(code[0].ToString());
            Parity par = parity[first];
            string digit = code[1].ToString();
            left = Draw(settings, g, left, digit, codes[int.Parse(digit)].LhOdd); //Odd
            for (int i = 2; i <= 6; i++)
            {
                digit = code[i].ToString();
                Pattern p = codes[int.Parse(digit)];
                left = Draw(settings, g, left, digit, (par.IsOdd(i - 2) ? p.LhOdd : p.LhEven));
            }

            //MIDDLE GUARD
            left = DrawCenterGuard(settings, g, left);

            //RIGHT GROUP
            for (int i = 7; i <= 12; i++)
            {
                digit = code[i].ToString();
                Pattern p = codes[int.Parse(digit)];
                left = Draw(settings, g, left, digit, p.Rh);
            }

            //RIGHT GUARD
            left = DrawRightGuard(settings, g, left);

            return bmp;
        }

        private static Pen pen = new Pen(Color.Black);
        private static Brush brush = Brushes.Black;

        private int Draw(Ean13Settings settings, Graphics g, int left, string digit, string s)
        {
            int h = (int)(settings.BarCodeHeight * 0.8);
            g.DrawString(digit, settings.Font, brush, left, h + top);

            foreach (char c in s)
            {
                if (c == '1')
                    g.FillRectangle(brush, left, top, settings.BarWidth, h);

                left += settings.BarWidth;
            }

            return left;
        }

        private int DrawLeftGuard(Ean13Settings settings, Graphics g, string digit, int left)
        {
            int h = (int)(settings.BarCodeHeight * 0.8);
            g.DrawString(digit, settings.Font, brush, left, h + top);
            left += (int)g.MeasureString(digit, settings.Font).Width;

            //TITLE
            if (title != null)
                g.DrawString(title, settings.Font, brush, left, settings.TopMargin);

            g.FillRectangle(brush, left, top, settings.BarWidth, settings.BarCodeHeight); //1
            left += settings.BarWidth;

            left += settings.BarWidth;                                                                   //0

            g.FillRectangle(brush, left, top, settings.BarWidth, settings.BarCodeHeight); //1
            left += settings.BarWidth;

            return left;
        }

        private int DrawRightGuard(Ean13Settings settings, Graphics g, int left)
        {
            g.FillRectangle(brush, left, top, settings.BarWidth, settings.BarCodeHeight); //1
            left += settings.BarWidth;

            left += settings.BarWidth;                                                                   //0

            g.FillRectangle(brush, left, top, settings.BarWidth, settings.BarCodeHeight); //1
            left += settings.BarWidth;

            return left;
        }

        private int DrawCenterGuard(Ean13Settings settings, Graphics g, int left)
        {
            left += settings.BarWidth;                                                                   //0

            g.FillRectangle(brush, left, top, settings.BarWidth, settings.BarCodeHeight); //1
            left += settings.BarWidth;

            left += settings.BarWidth;                                                                   //0

            g.FillRectangle(brush, left, top, settings.BarWidth, settings.BarCodeHeight); //1
            left += settings.BarWidth;

            left += settings.BarWidth;                                                                   //0

            return left;
        }

        class Pattern
        {
            private string lhOdd;

            public string LhOdd
            {
                get { return lhOdd; }
                set { lhOdd = value; }
            }

            private string lhEven;

            public string LhEven
            {
                get { return lhEven; }
                set { lhEven = value; }
            }

            private string rh;

            public string Rh
            {
                get { return rh; }
                set { rh = value; }
            }
        }

        class Parity
        {
            private string par;

            internal Parity(string par)
            {
                this.par = par;
            }

            internal bool IsOdd(int i)
            {
                return par[i] == 'o';
            }

            internal bool IsEven(int i)
            {
                return par[i] == 'e';
            }
        }
    }
}

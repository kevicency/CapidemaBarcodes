using System.Drawing;

namespace codesomnia.CapidemaBarcodes.Printing
{
    public class Ean13Settings
    {
        private int height = 120;

        public int BarCodeHeight
        {
            get { return height; }
            set { height = value; }
        }

        private int leftMargin = 10;

        public int LeftMargin
        {
            get { return leftMargin; }
            set { leftMargin = value; }
        }

        private int rightMargin = 10;

        public int RightMargin
        {
            get { return rightMargin; }
            set { rightMargin = value; }
        }

        private int topMargin = 10;

        public int TopMargin
        {
            get { return topMargin; }
            set { topMargin = value; }
        }

        private int bottomMargin = 10;

        public int BottomMargin
        {
            get { return bottomMargin; }
            set { bottomMargin = value; }
        }

        private int barWidth = 2;

        public int BarWidth
        {
            get { return barWidth; }
            set { barWidth = value; }
        }

        private Font font = new Font(FontFamily.GenericSansSerif, 10);

        internal Font Font
        {
            get { return font; }
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace codesomnia.CapidemaBarcodes
{
    internal class GridPaginator : DocumentPaginator, IDocumentPaginatorSource
    {
        public Grid[] Grids { get; private set; }

        public GridPaginator(Grid[] grids, Size pageSize)
        {
            Grids = grids;
            PageSize = pageSize;
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            return new DocumentPage(Grids[pageNumber]);
        }

        public override bool IsPageCountValid
        {
            get { return true; }
        }

        public override int PageCount
        {
            get { return Grids.Length; }
        }

        public override Size PageSize { get; set; }

        public override IDocumentPaginatorSource Source
        {
            get { return this; }
        }

        public DocumentPaginator DocumentPaginator
        {
            get { return this; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Szakdoga
{
    public class MultiPagePaginator : DocumentPaginator
    {
        private readonly List<Visual> _pages;
        public MultiPagePaginator(List<Visual> pages) => _pages = pages;

        public override DocumentPage GetPage(int pageNumber)
            => new DocumentPage(_pages[pageNumber]);

        public override bool IsPageCountValid => true;
        public override int PageCount => _pages.Count;
        public override Size PageSize { get; set; } = new Size(1000, 1414); // A4
        public override IDocumentPaginatorSource Source => null;
    }
}

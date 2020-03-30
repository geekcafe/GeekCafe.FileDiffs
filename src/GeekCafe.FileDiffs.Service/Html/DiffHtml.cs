using System;
using System.IO;
using System.Threading.Tasks;
using DiffPlex.DiffBuilder.Model;
namespace GeekCafe.FileDiffs.Service.Html
{
    internal class DiffHtml
    {
        public async Task BuildAsync(SideBySideDiffModel sideBySideDiffModel, StreamWriter streamWriter, bool includeUnchanged)
        {
            var diffPane = new DiffPane(includeUnchanged);

            await diffPane.BuildAsync(sideBySideDiffModel, streamWriter);

        }
    }
}

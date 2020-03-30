using System;
using System.Collections.Generic;

namespace GeekCafe.FileDiffs.Service.Model
{
    public class FileCollection
    {

        public Dictionary<string, FileCompareModel> Files { get; set; } = null;

        public FileCollection()
        {
            Files = new Dictionary<string, FileCompareModel>();
        }

        

    }
}

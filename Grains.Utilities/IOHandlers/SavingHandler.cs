using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grains.Utilities.IOHandlers
{
    public class SavingHandler : IOHandler
    {
        public SavingHandler(string filter): base(filter)
        {
        }

        public override string GetPath()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = this.filter;
            string path = string.Empty;

            if (saveFileDialog.ShowDialog() == true)
            {
                path = saveFileDialog.FileName;

                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }
            }

            return path;
        }
    }
}

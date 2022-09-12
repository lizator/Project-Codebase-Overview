using Project_Codebase_Overview.DataCollection.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Codebase_Overview.TestDocs
{
    internal class DummyDataSummoner
    {
        public static PCOFolder SummonDummyData()
        {
            PCOFolder rootFolder = new PCOFolder("Dummy-root", null);

            var f1 = new PCOFolder("F1", rootFolder);
            var f11 = new PCOFolder("F11", f1);
            var file111 = new PCOFile("File111", f11);
            var file112 = new PCOFile("File112", f11);
            var f12 = new PCOFolder("F12", f1);
            var file121 = new PCOFile("File121", f12);
            var file122 = new PCOFile("File122", f12);
            var file11 = new PCOFile("File11", f1);


            f11.AddChildren(new IExplorerItem[] { file111, file112 });
            f12.AddChildren(new IExplorerItem[] { file121, file122 });
            f1.AddChildren(new IExplorerItem[] { f11, f12, file11 });

            var f2 = new PCOFolder("F2", rootFolder);
            var f21 = new PCOFolder("F21", f2);
            var file211 = new PCOFile("File211", f21);
            var f22 = new PCOFolder("F22", f2);
            var file221 = new PCOFile("File221", f21);
            var file21 = new PCOFile("File21", f2);
            var file22 = new PCOFile("File22", f2);

            f21.AddChild(file211);
            f22.AddChild(file221);
            f2.AddChildren(new IExplorerItem[] { f21, f22, file21, file22});


            var file1 = new PCOFile("File1", rootFolder);
            var file2 = new PCOFile("File2", rootFolder);

            rootFolder.AddChildren(new IExplorerItem[] { f1, f2, file1, file2 });

            return rootFolder;
        }
    }
}

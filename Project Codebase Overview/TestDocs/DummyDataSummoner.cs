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
        public static Folder SummonDummyData()
        {
            Folder rootFolder = new Folder("Dummy-root", null);

            var f1 = new Folder("F1", rootFolder);
            var f11 = new Folder("F11", f1);
            var file111 = new File("File111", f11);
            var file112 = new File("File112", f11);
            var f12 = new Folder("F12", f1);
            var file121 = new File("File121", f12);
            var file122 = new File("File122", f12);
            var file11 = new File("File11", f1);


            f11.addChildren(new IExplorerItem[] { file111, file112 });
            f12.addChildren(new IExplorerItem[] { file121, file122 });
            f1.addChildren(new IExplorerItem[] { f11, f12, file11 });

            var f2 = new Folder("F2", rootFolder);
            var f21 = new Folder("F21", f2);
            var file211 = new File("File211", f21);
            var f22 = new Folder("F22", f2);
            var file221 = new File("File221", f21);
            var file21 = new File("File21", f2);
            var file22 = new File("File22", f2);

            f21.addChild(file211);
            f22.addChild(file221);
            f2.addChildren(new IExplorerItem[] { f21, f22, file21, file22});


            var file1 = new File("File1", rootFolder);
            var file2 = new File("File2", rootFolder);

            rootFolder.addChildren(new IExplorerItem[] { f1, f2, file1, file2 });

            return rootFolder;
        }
    }
}

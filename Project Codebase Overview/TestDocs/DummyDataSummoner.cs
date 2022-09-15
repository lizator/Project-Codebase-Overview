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


            f11.AddChild(file111);
            f11.AddChild(file112);
            f12.AddChild(file121);
            f12.AddChild(file122);
            f1.AddChild(f11);
            f1.AddChild(f12);
            f1.AddChild(file11);

            var f2 = new PCOFolder("F2", rootFolder);
            var f21 = new PCOFolder("F21", f2);
            var file211 = new PCOFile("File211", f21);
            var f22 = new PCOFolder("F22", f2);
            var file221 = new PCOFile("File221", f21);
            var file21 = new PCOFile("File21", f2);
            var file22 = new PCOFile("File22", f2);

            f21.AddChild(file211);
            f22.AddChild(file221);
            f2.AddChild(f21);
            f2.AddChild(f22);
            f2.AddChild(file21);
            f2.AddChild(file22);


            var file1 = new PCOFile("File1", rootFolder);
            var file2 = new PCOFile("File2", rootFolder);

            rootFolder.AddChild(f1);
            rootFolder.AddChild(f2);
            rootFolder.AddChild(file1);
            rootFolder.AddChild(file2);

            return rootFolder;
        }
    }
}

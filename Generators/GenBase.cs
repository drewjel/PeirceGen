using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace PeirceGen.Generators
{
    using System.IO;

    public abstract class GenBase
    {
        public string HeaderFile { get; protected set; }

        public string CppFile { get; protected set; }

        public GenBase() 
        {
            GenHeader();
            GenCpp();
            if (!Directory.Exists(PeirceGen.MonoConfigurationManager.Instance["GenPath"]))
                Directory.CreateDirectory(PeirceGen.MonoConfigurationManager.Instance["GenPath"]);
            System.IO.File.WriteAllText(this.GetHeaderLoc(), this.HeaderFile);
            System.IO.File.WriteAllText(this.GetCPPLoc(), this.CppFile);
        }

        public abstract void GenHeader();//set headerfile property

        public abstract void GenCpp();//set cpp file property

        public abstract string GetHeaderLoc();

        public abstract string GetCPPLoc();
    }
}

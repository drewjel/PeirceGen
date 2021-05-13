using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen.Generators
{
    public class GenInterpDelegator : GenBase
    {
        public override void GenCpp()
        {
            throw new NotImplementedException();
        }

        public override void GenHeader()
        {
            throw new NotImplementedException();
        }

        public override string GetCPPLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["GenPath"] + "/NewBackendInterp.cpp";
        }

        public override string GetHeaderLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["GenPath"] + "/Interp.h";
        }
    }
}

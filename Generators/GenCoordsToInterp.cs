using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen.Generators
{
    public class GenCoordsToInterp : GenBase
    {
        public override string GetCPPLoc()
        {
            return @"C:\Users\msfti\source\repos\givemeros\PeirceGen\symlinkme\CoordsToInterp.cpp";
        }

        public override string GetHeaderLoc()
        {
            return @"C:\Users\msfti\source\repos\givemeros\PeirceGen\symlinkme\CoordsToInterp.h";
        }

        public override void GenCpp()
        {
            var header = @"
#include ""CoordsToInterp.h""

#include <iostream>

#include <g3log/g3log.hpp>


using namespace coords2interp;
";
            var file = header;

            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                if (prod.ProductionType != Grammar.ProductionType.Single)
                {
                    var getintprod = @"interp::" + prod.Name + @" *CoordsToInterp::get" + prod.Name + @"(coords::" + prod.Name + @" *c) const
    {
        interp::" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @"*interp = NULL;
        try {
            interp = coords2interp_" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @".at(c);
        }
        catch (std::out_of_range &e) {
            interp = NULL;
        }
        return (interp::" + (prod.Name) + @"*)interp;
    }";
                    var getcooprod = @"coords::" + prod.Name + @" *CoordsToInterp::get" + prod.Name + @"(interp::" + prod.Name + @" *i) const
    {
        coords::" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @" *coords = NULL;
        try {
            coords = interp2coords_" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @".at(i);
        }
        catch (std::out_of_range &e) {
            coords = NULL;
        }
        return (coords::" + (prod.Name) + @"*)coords;
    }";
                    file += "\n" + getcooprod + "\n" + getintprod + "\n";
                }

                foreach (var pcase in prod.Cases)
                {

                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    else if (prod.ProductionType == Grammar.ProductionType.Single)
                    {
                        var put = @"void CoordsToInterp::put" + prod.Name + @"(coords::" + prod.Name + @"* c, interp::" + prod.Name + @"* i)
{
    coords2interp_" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @"[c] = (interp::" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @"*)i;
    interp2coords_" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @"[(interp::" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @"*)i] = c;
}";
                        var getcoo = @"coords::" + prod.Name + @"* CoordsToInterp::get" + prod.Name + @"(interp::" + prod.Name + @"* i) const
{
    coords::" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @"* coo = NULL;
    try {
        coo = interp2coords_" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @".at((interp::" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @"*)i);
    }
    catch (std::out_of_range &e) {
        coo = NULL;
    }
    return static_cast<coords::" + prod.Name + @"*>(coo);
}";
                        var getint = @"interp::" + prod.Name + @"* CoordsToInterp::get" + prod.Name + @"(coords::" + prod.Name + @"* c) const
{
    interp::" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @" *interp = NULL;
    try {
        interp = coords2interp_" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @".at(c);
    }
    catch (std::out_of_range &e) {
        interp = NULL;
    }
    return static_cast<interp::" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Name) + @"*>(interp);
}";
                        file += "\n" + put + "\n" + getcoo + "\n" + getint + "\n";
                    }
                    else
                    {

                        var put = @"void CoordsToInterp::put" + pcase.Name + @"(coords::" + pcase.Name + @"* c, interp::" + pcase.Name + @"* i)
{
    coords2interp_" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name :  prod.Name) + @"[c] = (interp::" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name :  prod.Name) + @"*)i;
    interp2coords_" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name :  prod.Name) + @"[(interp::" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name :  prod.Name) + @"*)i] = c;
}";
                        var getcoo = @"coords::" + pcase.Name + @"* CoordsToInterp::get" + pcase.Name + @"(interp::" + pcase.Name + @"* i) const
{
    coords::" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name :  prod.Name) + @"* coo = NULL;
    try {
        coo = interp2coords_" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name :  prod.Name) + @".at((interp::" + prod.Name + @"*)i);
    }
    catch (std::out_of_range &e) {
        coo = NULL;
    }
    return static_cast<coords::" + pcase.Name + @"*>(coo);
}";
                        var getint = @"interp::" + pcase.Name + @"* CoordsToInterp::get" + pcase.Name + @"(coords::" + pcase.Name + @"* c) const
{
    interp::" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name :  prod.Name) + @" *interp = NULL;
    try {
        interp = coords2interp_" + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name :  prod.Name) + @".at(c);
    }
    catch (std::out_of_range &e) {
        interp = NULL;
    }
    return static_cast<interp::" + pcase.Name + @"*>(interp);
}";
                        file += "\n" + put + "\n" + getcoo + "\n" + getint + "\n";
                    }
                }
            }

            this.CppFile = file;
        }

        public override void GenHeader()
        {
            var header = @"#ifndef COORDSTOINTERP_H
#define COORDSTOINTERP_H

# include <iostream>
# include ""Coords.h""
# include ""Interp.h""

# include <unordered_map>

namespace coords2interp{

class CoordsToInterp
{
public:

";
            var file = header;


            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                if (prod.ProductionType != Grammar.ProductionType.Single)
                {

                    var getintprod = @"interp::" + prod.Name + @"* get" + prod.Name + "(coords::" + prod.Name + "* c) const;";
                    var getcooprod = @"coords::" + prod.Name + "* get" + prod.Name + "(interp::" + prod.Name + @"* i) const;";

                    file += "\n\t" + getintprod + "\n\t" + getcooprod + "\n";
                }

                foreach (var pcase in prod.Cases)
                {

                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    else if(pcase.CaseType == Grammar.CaseType.Ident)
                    {


                        var put = @"void put" + prod.Name + "(coords::" + prod.Name + "* key, interp::" + prod.Name + "* val);";
                        var getint = @"interp::" + prod.Name + "* get" + prod.Name + "(coords::" + prod.Name + "* c) const;";
                        var getcoo = @"coords::" + prod.Name + "* get" + prod.Name + "(interp::" + prod.Name + "* i) const;";

                        file += "\n\t" + put + "\n\t" + getint + "\n\t" + getcoo + "\n";
                    }
                    else
                    {

                        var put = @"void put" + pcase.Name + "(coords::" + pcase.Name + "* key, interp::" + pcase.Name + "* val);";
                        var getint = @"interp::" + pcase.Name + "* get" + pcase.Name + "(coords::" + pcase.Name + "* c) const;";
                        var getcoo = @"coords::" + pcase.Name + "* get" + pcase.Name + "(interp::" + pcase.Name + "* i) const;";

                        file += "\n\t" + put + "\n\t" + getint + "\n\t" + getcoo + "\n";
                    }
                }
            }

            file += "\nprivate:\n";

            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                //foreach (var pcase in prod.Cases)
                // {
                var mapc2i = @"std::unordered_map <coords::" + prod.Name + "*,	interp::" + prod.Name + @"*	> 	coords2interp_" + prod.Name + ";";
                var mapi2c = @"std::unordered_map <interp::" + prod.Name + "*,	coords::" + prod.Name + "*	> 	interp2coords_" + prod.Name + ";";

                file += "\n\t" + mapc2i + "\n\t" + mapi2c + "\n";
                //}
            }

            file += @"};

} // namespace

#endif";
            this.HeaderFile = file;

        }
    }
}

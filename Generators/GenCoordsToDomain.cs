using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace PeirceGen.Generators
{
    public class GenCoordsToDomain : GenBase
    {
        public override string GetCPPLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["GenPath"] + "CoordsToDomain.cpp";
        }

        public override string GetHeaderLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["GenPath"] + "CoordsToDomain.h";
        }
        public override void GenCpp()
        {
            /*
             * void CoordsToDomain::putTransformIdent(coords::TransformIdent *c, domain::TransformIdent *d)
{
    coords2dom_TransformIdent[c] = d;
    dom2coords_TransformIdent[d] = c;
}

// TODO: Decide whether or not these maps can be partial on queried keys
// As defined here, yes, and asking for a missing key returns NULL
//
domain::VecIdent *CoordsToDomain::getVecIdent(coords::VecIdent *c) const
{
    std::unordered_map<coords::VecIdent*, domain::VecIdent*>::iterator it;
    domain::VecIdent *dom = NULL;
    try {
        dom = coords2dom_VecIdent.at(c);
    }
    catch (std::out_of_range &e) {
        dom = NULL;
    }
    return dom;
}

coords::VecIdent *CoordsToDomain::getVecIdent(domain::VecIdent *d) const
{
    std::unordered_map<domain::VecIdent*, coords::VecIdent*>::iterator it;
    coords::VecIdent *coords = NULL;
    try {
        coords = dom2coords_VecIdent.at(d);
    }
    catch (std::out_of_range &e) {
        coords = NULL;
    }
    return coords;
}
             * */

            var header = @"
#include ""CoordsToDomain.h""

# include <iostream>

//# include <g3log/g3log.hpp>



//using namespace std;
using namespace coords2domain;
";
            var file = header;


            foreach(var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                if (prod.ProductionType != Grammar.ProductionType.Single && prod.ProductionType != Grammar.ProductionType.CaptureSingle)
                {
                    var getdomprod = @"domain::DomainObject *CoordsToDomain::get" + prod.Name + @"(coords::" + prod.Name + @" *c) const
    {
        domain::DomainObject *dom = NULL;
        try {
            dom = coords2dom_" + prod.GetTopPassthrough().Name + @".at((coords::" + prod.GetTopPassthrough().Name + @"*)c);
        }
        catch (std::out_of_range &e) {
            dom = NULL;
        }
        return dom;
    }";
                    var getcooprod = @"coords::" + prod.Name + @" *CoordsToDomain::get" + prod.Name + @"(domain::DomainObject *d) const
    {
        coords::" + prod.GetTopPassthrough().Name + @" *coords = NULL;
        try {
            coords = dom2coords_" + prod.GetTopPassthrough().Name + @".at(d);
        }
        catch (std::out_of_range &e) {
            coords = NULL;
        }
        return (coords::" + prod.Name + @" *)coords;
    }";
                    file += "\n" + getcooprod + "\n" + getdomprod + "\n";
                }

                foreach (var pcase in prod.Cases)
                {

                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    else if (prod.ProductionType == Grammar.ProductionType.Single || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                    {
                        var put = @"void CoordsToDomain::put" + prod.Name + @"(coords::" + prod.Name + @"* c, domain::DomainObject *d)
{
    coords2dom_" + prod.GetTopPassthrough().Name + @"[(coords::" + prod.GetTopPassthrough().Name + @"*)c] = d;
    dom2coords_" + prod.GetTopPassthrough().Name + @"[d] = c;
}";
                        var erase = @"void CoordsToDomain::erase" + prod.Name + @"(coords::" + prod.Name + @"* c, domain::DomainObject *d)
{
    coords2dom_" + prod.GetTopPassthrough().Name + @".erase((coords::" + prod.GetTopPassthrough().Name + @"*)c);
    dom2coords_" + prod.GetTopPassthrough().Name + @".erase(d);
}";
                        var getcoo = @"domain::DomainObject* CoordsToDomain::get" + prod.Name + @"(coords::" + prod.Name + @"* c) const
{
    domain::DomainObject* dom = NULL;
    try {
        dom = coords2dom_" + prod.GetTopPassthrough().Name + @".at((coords::" + prod.GetTopPassthrough().Name + @"*)c);
    }
    catch (std::out_of_range &e) {
        dom = NULL;
    }
    return static_cast<domain::DomainObject*>(dom);
}";
                        var getdom = @"coords::" + prod.Name + @"* CoordsToDomain::get" + prod.Name + @"(domain::DomainObject* d) const
{
    coords::" + prod.GetTopPassthrough().Name + @" *coords = NULL;
    try {
        coords = dom2coords_" + prod.GetTopPassthrough().Name + @".at(d);
    }
    catch (std::out_of_range &e) {
        coords = NULL;
    }
    return static_cast<coords::" + prod.Name + @"*>(coords);
}";
                        file += "\n" + put + "\n" + getcoo + "\n" + getdom + "\n" + erase + "\n";
                    }
                    else
                    {
                        var put = @"void CoordsToDomain::put" + pcase.Name + @"(coords::" + pcase.Name + @"* c, domain::DomainObject *d)
{
    coords2dom_" + prod.GetTopPassthrough().Name + @"[(coords::" + prod.GetTopPassthrough().Name + @"*)c] = d;
    dom2coords_" + prod.GetTopPassthrough().Name + @"[d] = (coords::" + prod.GetTopPassthrough().Name + @"*)c;
}";

                        var erase = @"void CoordsToDomain::erase" + pcase.Name + @"(coords::" + pcase.Name + @"* c, domain::DomainObject *d)
{
    coords2dom_" + prod.GetTopPassthrough().Name + @".erase((coords::" + prod.GetTopPassthrough().Name + @"*)c);
    dom2coords_" + prod.GetTopPassthrough().Name + @".erase(d);
}";
                        var getcoo = @"domain::DomainObject* CoordsToDomain::get" + pcase.Name + @"(coords::" + pcase.Name + @"* c) const
{
    domain::DomainObject* dom = NULL;
    try {
        dom = coords2dom_" + prod.GetTopPassthrough().Name + @".at((coords::" + prod.GetTopPassthrough().Name + @"*)c);
    }
    catch (std::out_of_range &e) {
        dom = NULL;
    }
    return static_cast<domain::DomainObject*>(dom);
}";
                        var getdom = @"coords::" + pcase.Name + @"* CoordsToDomain::get" + pcase.Name + @"(domain::DomainObject* d) const
{
    coords::" + prod.GetTopPassthrough().Name + @" *coords = NULL;
    try {
        coords = dom2coords_" + prod.GetTopPassthrough().Name + @".at(d);
    }
    catch (std::out_of_range &e) {
        coords = NULL;
    }
    return static_cast<coords::" + pcase.Name + @"*>(coords);
}";
                        file += "\n" + put + "\n" + erase + "\n" + getcoo + "\n" + getdom + "\n";
                    }
                }

            }

            this.CppFile = file;
        }

        public override void GenHeader()
        {
            var header = @"
#ifndef COORDSTODOMAIN_H
#define COORDSTODOMAIN_H

#include <iostream>
#include ""Coords.h""
#include ""Domain.h""

#include <unordered_map>

/*
	When putting, we know precise subclass, so we don't include
	putters for Expr and Vector super-classes. When getting, we 
	generally don't know, so we can return superclass pointers.
*/

/*
We currently require client to create domain nodes, which we 
then map to and from the given coordinates. From coordinates 
is currently implement as unordered map. From domain object is
currently implemented as domain object method. This enables us
to return precisely typed objects without having to maintain a
lot of separate mapping tables.
*/

namespace coords2domain{

class CoordsToDomain
{
public:

";
            var file = header;


            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                if (prod.ProductionType != Grammar.ProductionType.Single && prod.ProductionType != Grammar.ProductionType.CaptureSingle)
                {
                    var getdomprod = @"domain::DomainObject* get" + prod.Name + "(coords::" + prod.Name + "* c) const;";
                    var getcooprod = @"coords::" + prod.Name + "* get" + prod.Name + "(domain::DomainObject* d) const;";

                    file += "\n\t" + getdomprod + "\n\t" + getcooprod + "\n";
                }
                else
                {
                    var getdomprod = @"domain::DomainObject* get" + prod.Name + "(coords::" + prod.Name + "* c) const;";
                    var getcooprod = @"coords::" + prod.Name + "* get" + prod.Name + "(domain::DomainObject* d) const;";

                    file += "\n\t" + getdomprod + "\n\t" + getcooprod + "\n";

                    var put = @"void put" + prod.Name + "(coords::" + prod.Name + "* key, domain::DomainObject* val);";
                    var erase = @"void erase" + prod.Name + "(coords::" + prod.Name + "* key, domain::DomainObject* val);";

                    file += "\n\t" + put + "\n" + erase + "\n";

                    continue;
                }


                foreach (var pcase in prod.Cases)
                {

                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    else if (pcase.CaseType == Grammar.CaseType.Ident)
                    {
                        var put = @"void put" + prod.Name + "(coords::" + prod.Name + "* key, domain::DomainObject* val);";
                        var getdom = @"domain::DomainObject* get" + prod.Name + "(coords::" + prod.Name + "* c) const;";
                        var getcoo = @"coords::" + prod.Name + "* get" + prod.Name + "(domain::DomainObject* d) const;";
                        var erase = @"void erase" + prod.Name + "(coords::" + prod.Name + "* key, domain::DomainObject* val);";

                        file += "\n\t" + put + "\n\t" + getdom + "\n\t" + getcoo + "\n" + erase + "\n";
                    }
                    else
                    {
                        var put = @"void put" + pcase.Name + "(coords::" + pcase.Name + "* key, domain::DomainObject* val);";
                        var getdom = @"domain::DomainObject* get" + pcase.Name + "(coords::" + pcase.Name + "* c) const;";
                        var getcoo = @"coords::" + pcase.Name + "* get" + pcase.Name + "(domain::DomainObject* d) const;";
                        var erase = @"void erase" + pcase.Name + "(coords::" + pcase.Name + "* key, domain::DomainObject* val);";

                        file += "\n\t" + put + "\n\t" + getdom + "\n\t" + getcoo + "\n" + erase + "\n";
                    }
                }
            }

            file += "\nprivate:\n";

            foreach(var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                //foreach (var pcase in prod.Cases)
                // {
                var mapc2d = @"std::unordered_map <coords::" + prod.Name + "*,	domain::DomainObject*	> 	coords2dom_" + prod.Name + ";";
                var mapd2c = @"std::unordered_map <domain::DomainObject*,	coords::" + prod.Name + "*	> 	dom2coords_" + prod.Name + ";";

                file += "\n\t" + mapc2d + "\n\t" + mapd2c + "\n";
                //}
            }

            file += @"};

} // namespace

#endif";
            this.HeaderFile = file;
        }
    }
}

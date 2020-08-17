using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen.Generators
{
    public class GenInterpToDomain : GenBase
    {
        public override string GetCPPLoc()
        {
            return "/peirce/PeirceGen/symlinkme/InterpToDomain.cpp";
        }

        public override string GetHeaderLoc()
        {
            return "/peirce/PeirceGen/symlinkme/InterpToDomain.h";
        }
        public override void GenCpp()
        {
            var header = @"
#include ""InterpToDomain.h""

#include <iostream>

#include <g3log/g3log.hpp>

using namespace interp2domain;
";
            var file = header;

            var putspace = @"void InterpToDomain::putSpace(interp::Space* key, domain::Space* val){
    interp2dom_Spaces[key] = val;
    dom2interp_Spaces[val] = key;
}";
            var getdomspace = @"domain::Space* InterpToDomain::getSpace(interp::Space* i) const{
    domain::Space* dom = NULL;
    try {
        dom = interp2dom_Spaces.at(i);
    }
    catch (std::out_of_range &e) {
        dom = NULL;
    }
    return dom;
}";
            var getintspace = @"interp::Space* InterpToDomain::getSpace(domain::Space* d) const{
    interp::Space *interp = NULL;
    try {
        interp = dom2interp_Spaces.at(d);
    }
    catch (std::out_of_range &e) {
        interp = NULL;
    }
    return interp;
}";

            file += "\n\t" + putspace + "\n\t" + getdomspace + "\n\t" + getintspace + "\n";

            var putFrame = @"void InterpToDomain::putFrame(interp::Frame* key, domain::Frame* val){
    interp2dom_Frames[key] = val;
    dom2interp_Frames[val] = key;
}";
            var getdomFrame = @"domain::Frame* InterpToDomain::getFrame(interp::Frame* i) const{
    domain::Frame* dom = NULL;
    try {
        dom = interp2dom_Frames.at(i);
    }
    catch (std::out_of_range &e) {
        dom = NULL;
    }
    return dom;
}";
            var getintFrame = @"interp::Frame* InterpToDomain::getFrame(domain::Frame* d) const{
    interp::Frame *interp = NULL;
    try {
        interp = dom2interp_Frames.at(d);
    }
    catch (std::out_of_range &e) {
        interp = NULL;
    }
    return interp;
}";

            file += "\n\t" + putFrame + "\n\t" + getdomFrame + "\n\t" + getintFrame + "\n";



            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                if (prod.ProductionType != Grammar.ProductionType.Single && prod.ProductionType != Grammar.ProductionType.CaptureSingle)
                {
                    var getdomprod = @"domain::DomainObject *InterpToDomain::get" + prod.Name + @"(interp::" + prod.Name + @" *i) const
    {
        domain::DomainObject *dom = NULL;
        try {
            dom = interp2dom_" + prod.GetTopPassthrough().Name + @".at(i);
        }
        catch (std::out_of_range &e) {
            dom = NULL;
        }
        return dom;
    }";
                    var getintprod = @"interp::" + prod.Name + @" *InterpToDomain::get" + prod.Name + @"(domain::DomainObject *d) const
    {
        interp::" + prod.GetTopPassthrough().Name + @" *interp = NULL;
        try {
            interp = dom2interp_" + prod.GetTopPassthrough().Name + @".at(d);
        }
        catch (std::out_of_range &e) {
            interp = NULL;
        }
        return (interp::" + (prod.Name) + @"*)interp;
    }";
                    file += "\n" + getintprod + "\n" + getdomprod + "\n";
                }
                else
                {
                    var put = @"void InterpToDomain::put" + prod.Name + @"(interp::" + prod.Name + @"* i, domain::DomainObject* d)
{
    interp2dom_" + prod.GetTopPassthrough().Name + @"[i] = d;
    dom2interp_" + prod.GetTopPassthrough().Name + @"[d] = i;
}";
                    var erase = @"void InterpToDomain::erase" + prod.Name + @"(interp::" + prod.Name + @"* i, domain::DomainObject* d)
{
    interp2dom_" + prod.GetTopPassthrough().Name + @".erase(i);
    dom2interp_" + prod.GetTopPassthrough().Name + @".erase(d);
}";
                    var getint = @"domain::DomainObject* InterpToDomain::get" + prod.Name + @"(interp::" + prod.Name + @"* i) const
{
    domain::DomainObject* dom = NULL;
    try {
        dom = interp2dom_" + prod.GetTopPassthrough().Name + @".at(i);
    }
    catch (std::out_of_range &e) {
        dom = NULL;
    }
    return static_cast<domain::DomainObject*>(dom);
}";
                    var getdom = @"interp::" + prod.Name + @"* InterpToDomain::get" + prod.Name + @"(domain::DomainObject* d) const
{
    interp::" + prod.GetTopPassthrough().Name + @" *interp = NULL;
    try {
        interp = dom2interp_" + prod.GetTopPassthrough().Name + @".at(d);
    }
    catch (std::out_of_range &e) {
        interp = NULL;
    }
    return static_cast<interp::" + prod.Name + @"*>(interp);
}";
                    file += "\n" + put + "\n" + erase + "\n" + getint + "\n" + getdom + "\n";

                    continue;
                }

                foreach (var pcase in prod.Cases)
                {

                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    else if (pcase.CaseType == Grammar.CaseType.Ident)
                    {
                        break;
                        var put = @"void InterpToDomain::put" + prod.Name + @"(interp::" + prod.Name + @"* i, domain::DomainObject* d)
{
    interp2dom_" + prod.GetTopPassthrough().Name + @"[i] = d;
    dom2interp_" + prod.GetTopPassthrough().Name + @"[d] = i;
}";
                        var erase = @"void InterpToDomain::erase" + prod.Name + @"(interp::" + prod.Name + @"* i, domain::DomainObject* d)
{
    interp2dom_" + prod.GetTopPassthrough().Name + @".erase(i);
    dom2interp_" + prod.GetTopPassthrough().Name + @".erase(d);
}";
                        var getint = @"domain::DomainObject* InterpToDomain::get" + prod.Name + @"(interp::" + prod.Name + @"* i) const
{
    domain::DomainObject* dom = NULL;
    try {
        dom = interp2dom_" + prod.GetTopPassthrough().Name + @".at(i);
    }
    catch (std::out_of_range &e) {
        dom = NULL;
    }
    return static_cast<domain::DomainObject*>(dom);
}";
                        var getdom = @"interp::" + prod.Name + @"* InterpToDomain::get" + prod.Name + @"(domain::DomainObject* d) const
{
    interp::" + prod.GetTopPassthrough().Name + @" *interp = NULL;
    try {
        interp = dom2interp_" + prod.GetTopPassthrough().Name + @".at(d);
    }
    catch (std::out_of_range &e) {
        interp = NULL;
    }
    return static_cast<interp::" + prod.Name + @"*>(interp);
}";
                        file += "\n" + put + "\n" + erase + "\n" + getint + "\n" + getdom + "\n";
                    }
                    else
                    {
                        var put = @"void InterpToDomain::put" + pcase.Name + @"(interp::" + pcase.Name + @"* i, domain::DomainObject* d)
{
    interp2dom_" + prod.GetTopPassthrough().Name + @"[i] = d;
    dom2interp_" + prod.GetTopPassthrough().Name + @"[d] = i;
}";
                        var erase = @"void InterpToDomain::erase" + pcase.Name + @"(interp::" + pcase.Name + @"* i, domain::DomainObject* d)
{
    interp2dom_" + prod.GetTopPassthrough().Name + @".erase(i);
    dom2interp_" + prod.GetTopPassthrough().Name + @".erase(d);
}";
                        var getint = @"domain::DomainObject* InterpToDomain::get" + pcase.Name + @"(interp::" + pcase.Name + @"* i) const
{
    domain::DomainObject* dom = NULL;
    try {
        dom = interp2dom_" + prod.GetTopPassthrough().Name + @".at(i);
    }
    catch (std::out_of_range &e) {
        dom = NULL;
    }
    return static_cast<domain::DomainObject*>(dom);
}";
                        var getdom = @"interp::" + pcase.Name + @"* InterpToDomain::get" + pcase.Name + @"(domain::DomainObject* d) const
{
    interp::" + prod.GetTopPassthrough().Name + @" *interp = NULL;
    try {
        interp = dom2interp_" + prod.GetTopPassthrough().Name + @".at(d);
    }
    catch (std::out_of_range &e) {
        interp = NULL;
    }
    return static_cast<interp::" + pcase.Name + @"*>(interp);
}";
                        file += "\n" + put + "\n" + erase + "\n" + getint + "\n" + getdom + "\n";
                    }

                    
                }
            }
            this.CppFile = file;
        }

        public override void GenHeader()
        {
            var header = @"#ifndef INTERPTODOMAIN_H
#define INTERPTODOMAIN_H

#include <iostream>
#include ""Domain.h""
#include ""Interp.h""

#include <unordered_map>

namespace interp2domain{

class InterpToDomain
{
    public:";


            var file = header;

            var putspace = @"void putSpace(interp::Space* key, domain::Space* val);";
            var getdomspace = @"domain::Space* getSpace(interp::Space* c) const;";
            var getintspace = @"interp::Space* getSpace(domain::Space* d) const;";

            file += "\n\t" + putspace + "\n\t" + getdomspace + "\n\t" + getintspace + "\n";
            var putFrame = @"void putFrame(interp::Frame* key, domain::Frame* val);";
            var getdomFrame = @"domain::Frame* getFrame(interp::Frame* c) const;";
            var getintFrame = @"interp::Frame* getFrame(domain::Frame* d) const;";

            file += "\n\t" + putFrame + "\n\t" + getdomFrame + "\n\t" + getintFrame + "\n";

            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                if (prod.ProductionType != Grammar.ProductionType.Single && prod.ProductionType != Grammar.ProductionType.CaptureSingle)
                {


                    var getdomprod = @"domain::DomainObject* get" + prod.Name + "(interp::" + prod.Name + "* c) const;";
                    var getintprod = @"interp::" + prod.Name + "* get" + prod.Name + "(domain::DomainObject* d) const;";

                    file += "\n\t" + getdomprod + "\n\t" + getintprod + "\n\t";
                }
                else
                {

                    var put = @"void put" + prod.Name + "(interp::" + prod.Name + "* key, domain::DomainObject* val);";
                    var erase = @"void erase" + prod.Name + "(interp::" + prod.Name + "* key, domain::DomainObject* val);";
                    var getdom = @"domain::DomainObject* get" + prod.Name + "(interp::" + prod.Name + "* c) const;";
                    var getint = @"interp::" + prod.Name + "* get" + prod.Name + "(domain::DomainObject* d) const;";

                    file += "\n\t" + put + "\n\t" + getdom + "\n\t" + getint + "\n" + erase + "\n";

                    continue;

                }

                foreach (var pcase in prod.Cases)
                {

                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    else if (pcase.CaseType == Grammar.CaseType.Ident)
                    {
                        break;
                        var put = @"void put" + prod.Name + "(interp::" + prod.Name + "* key, domain::DomainObject* val);";
                        var erase = @"void erase" + prod.Name + "(interp::" + prod.Name + "* key, domain::DomainObject* val);";
                        var getdom = @"domain::DomainObject* get" + prod.Name + "(interp::" + prod.Name + "* c) const;";
                        var getint = @"interp::" + prod.Name + "* get" + prod.Name + "(domain::DomainObject* d) const;";

                        file += "\n\t" + put + "\n\t" + getdom + "\n\t" + getint + "\n" + erase + "\n";
                    }
                    else
                    {
                        var put = @"void put" + pcase.Name + "(interp::" + pcase.Name + "* key, domain::DomainObject* val);";
                        var erase = @"void erase" + pcase.Name + "(interp::" + pcase.Name + "* key, domain::DomainObject* val);";
                        var getdom = @"domain::DomainObject* get" + pcase.Name + "(interp::" + pcase.Name + "* c) const;";
                        var getint = @"interp::" + pcase.Name + "* get" + pcase.Name + "(domain::DomainObject* d) const;";

                        file += "\n\t" + put + "\n\t" + getdom + "\n\t" + getint + "\n" + erase + "\n";
                    }
                }
            }

            file += "\nprivate:\n";

            file += "\nstd::unordered_map<interp::Space*, domain::Space*> interp2dom_Spaces;\n";
            file += "\nstd::unordered_map<domain::Space*, interp::Space*> dom2interp_Spaces;\n";
            file += "\nstd::unordered_map<interp::Frame*, domain::Frame*> interp2dom_Frames;\n";
            file += "\nstd::unordered_map<domain::Frame*, interp::Frame*> dom2interp_Frames;\n";

            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                //foreach (var pcase in prod.Cases)
               // {
                    var mapi2d = @"std::unordered_map <interp::" + prod.Name + "*,	domain::DomainObject*	> 	interp2dom_" + prod.Name + ";";
                    var mapd2i = @"std::unordered_map <domain::DomainObject*,	interp::" + prod.Name + "*	> 	dom2interp_" + prod.Name + ";";

                    file += "\n\t" + mapi2d + "\n\t" + mapd2i + "\n";
                //}
            }

            file += @"};

} // namespace

#endif";
            this.HeaderFile = file;
        }
    }
}

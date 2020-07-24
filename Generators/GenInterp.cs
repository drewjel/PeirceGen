using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen.Generators
{
    public class GenInterp : GenBase
    {
        public override string GetCPPLoc()
        {
            return @"C:\Users\msfti\source\repos\givemeros\PeirceGen\symlinkme\Interp.cpp";
        }

        public override string GetHeaderLoc()
        {
            return @"C:\Users\msfti\source\repos\givemeros\PeirceGen\symlinkme\Interp.h";
        }
        public override void GenCpp()
        {
            var header = @"#include ""Interp.h""

#include ""Domain.h""

#include <g3log/g3log.hpp>

#include <algorithm>

using namespace g3; 

namespace interp{

int GLOBAL_INDEX = 0;

Interp::Interp(coords::Coords* c, domain::DomainObject* d) : coords_(c), dom_(d){
}

std::string Space::toString() const {
    std::string retval = """";
    bool found = false;
" + string.Join("",ParsePeirce.Instance.Spaces.Select(sp_ => {

    var hasName = sp_.MaskContains(Space.FieldType.Name);
    var hasDim = sp_.MaskContains(Space.FieldType.Dimension);
                //PhysSpaceExpression.ClassicalTimeLiteral (ClassicalTimeSpaceExpression.ClassicalTimeLiteral
                return "\n\tif(auto dc = dynamic_cast<domain::" + sp_.Name + @"*>(s_)){
        found = true;
        retval += ""def "" + dc->getName() + ""var : " + sp_.Prefix + @"SpaceVar := (!"" + std::to_string(++GLOBAL_INDEX) + "")"" + ""\n"";
        retval += " + ((hasName && hasDim) ? @"""def "" + dc->getName() + ""sp := (" + sp_.Name + @""" + std::to_string(dc->getDimension()) +  ""SpaceExpression." + sp_.Name + @""" + std::to_string(dc->getDimension()) +  ""Literal ( Build" + sp_.Name +@"Space \"""" + dc->getName() + ""\"" "" + std::to_string(dc->getDimension()) +  ""))\n"";; " : 
                        hasName ? @"""def "" + dc->getName() + ""sp :=  (" + sp_.Name + @"SpaceExpression." + sp_.Name + @"Literal ( Build" + sp_.Name + @"Space \"""" + dc->getName() + ""\"" ))\n"";; " : "") + @"
        retval += ""def "" + dc->getName() + "" := PhysGlobalCommand.GlobalSpace (⊢"" + dc->getName() + ""var) (⊢"" + dc->getName() + ""sp)\n"";
    }"; })) + @"

    if(!found){
        //retval = ""--Unknown space type - Translation Failed!"";
    }

    return retval;
};

std::string Frame::toString() const {
    std::string retval = """";
    bool found = false;
    bool isStandard = this->f_->getName() == ""Standard"";
    if(!isStandard)
        return retval;
" + string.Join("", ParsePeirce.Instance.Spaces.Select(sp_ => {

                            var hasName = sp_.MaskContains(Space.FieldType.Name);
                            var hasDim = sp_.MaskContains(Space.FieldType.Dimension);
                            //PhysSpaceExpression.ClassicalTimeLiteral (ClassicalTimeSpaceExpression.ClassicalTimeLiteral
                            return "\n\tif(auto dc = dynamic_cast<domain::" + sp_.Name + @"*>(f_->getSpace())){
        found = true;
        retval += ""def "" +dc->getName()+"".""+f_->getName() + ""var : " + sp_.Prefix+@"FrameVar := (!"" + std::to_string(++GLOBAL_INDEX) + "")"" + ""\n"";
        if(!isStandard){
            retval += ""def "" + dc->getName()+"".""+f_->getName() + ""fr := " + sp_.Prefix + @"FrameExpression.FrameLiteral ( Build" + sp_.Name + @"Frame ""+ dc->getName()+""))"";
        }
        else{
            retval += ""def "" + dc->getName()+"".""+f_->getName() + ""fr := " + sp_.Prefix + @"FrameExpression.FrameLiteral ( Get" + sp_.Name + @"StandardFrame (Eval"+sp_.Prefix+@"SpaceExpression "" + dc->getName()+""sp))\n"";
    
        }
        retval += ""def "" + dc->getName()+"".""+f_->getName() + "" := PhysGlobalCommand.GlobalFrame (⊢"" + dc->getName()+"".""+f_->getName() + ""var) (⊢"" + dc->getName()+"".""+f_->getName() + ""fr)\n"";
    }";
                        })) + @"

    if(!found){
        //retval = ""--Unknown Frame type - Translation Failed!"";
    }

    return retval;

};

";
            /*
             * with PhysFrameVar : Type
            | EuclideanGeometry3 : EuclideanGeometry3FrameVar → PhysFrameVar
            | ClassicalTime : ClassicalTimeFrameVar → PhysFrameVar
            | ClassicalVelocity3 : ClassicalVelocity3FrameVar → PhysFrameVar
            with EuclideanGeometry3FrameVar : Type
            | mk : ℕ → EuclideanGeometry3FrameVar
            with ClassicalTimeFrameVar : Type
            | mk : ℕ → ClassicalTimeFrameVar
            with ClassicalVelocity3FrameVar : Type
            | mk : ℕ → ClassicalVelocity3FrameVar

            with EuclideanGeometry3FrameExpression: Type
            | FrameLiteral (sp : EuclideanGeometrySpace 3) : EuclideanGeometryFrame sp → EuclideanGeometry3FrameExpression
            with ClassicalTimeFrameExpression: Type
            | FrameLiteral (sp : ClassicalTimeSpace) : ClassicalTimeFrame sp → ClassicalTimeFrameExpression
            with ClassicalVelocity3FrameExpression : Type
            | FrameLiteral (sp : ClassicalVelocitySpace 3) : ClassicalVelocityFrame sp → ClassicalVelocity3FrameExpression
             * */


            var file = header;
            /*
             * VecIdent::VecIdent(coords::VecIdent* c, domain::VecIdent* d) : Interp(c,d) {}

std::string VecIdent::toString() const {
  std::string ret = "";
//  ret += "( ";
  ret += "def ";
  ret += coords_->toString() + "_var";
  ret += " : @peirce.vector_variable " + ident_->getSpaceContainer()->toString();
  ret += " := @peirce.vector_variable.mk ";
  ret += ident_->getSpaceContainer()->toString() + " " + std::to_string(++index);
//  ret += " )";
  return ret; 
}

             * */
            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                if (true || prod.ProductionType != Grammar.ProductionType.Single)
                {
                    var prodcons = "\n" +
    @"" + prod.Name + @"::" + prod.Name + @"(coords::" + prod.Name + @"* c, domain::DomainObject* d) : " + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Interp") + @"(c,d) {}
                    ";
                    file += prodcons;

                    var prodstr = @"
std::string " + prod.Name + @"::toString() const {
    std::string retval = """";
    bool found = false;
    
    //  ret += ""("";
    //ret += ""def var_"" + std::to_string(++index) + "":= 1"";" +
    string.Join("",
    ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ? 
    ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
    {
        var spInstances = ParsePeirce.Instance.SpaceInstances;

        var retval = @"
    if(auto dc = dynamic_cast<domain::" + sppair.Item1.Prefix + sppair.Item2.Name + @"*>(this->dom_)){
        found = true;
        " + (prod.ProductionType == Grammar.ProductionType.Single ? prod.Cases[0].InterpTranslation(prod, sppair.Item1, sppair.Item2, ParsePeirce.Instance.SpaceInstances[0]) : @"std::cout<<""Warning - Calling toString on a production rather than a case\n;"";") + @"
    }
";      return retval;


    }
    ) : new List<string>())
                    + @"
    if (auto cont = dynamic_cast<domain::DomainContainer*>(this->dom_)){
        if(cont->hasValue()){

                        " +
                        (
                        string.Join("",
                    ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                    ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair_ =>
                    {
                        var retval_ = @"
            if(auto dc = dynamic_cast<domain::" + sppair_.Item1.Prefix + sppair_.Item2.Name + @"*>(cont->getValue())){
                found = true;
                " + (prod.ProductionType == Grammar.ProductionType.Single ? prod.Cases[0].InterpTranslation(prod, sppair_.Item1, sppair_.Item2, ParsePeirce.Instance.SpaceInstances[0]) : @"std::cout<<""Warning - Calling toString on a production rather than a case\n;"";") + @"
            }";
                        return retval_;
                    }
                    ) : new List<string>())) +
                        @"
        }
    }

    if(!found){
        //ret = """";
        " + (prod.ProductionType == Grammar.ProductionType.Single ? prod.Cases[0].InterpTranslation(prod, null, null, ParsePeirce.Instance.SpaceInstances[0]) : @"std::cout<<""Warning - Calling toString on a production rather than a case\n;"";") + @"
    }
    std::replace(retval.begin(), retval.end(), '_', '.');
    int index;
    string sub_str = "": _"";
    string singleperiod = "".a"";
    while ((index = retval.find("": ."")) != string::npos)
    {    
        retval.replace(index, sub_str.length(), sub_str); 
    }
    while ((index = retval.find("": ^"")) != string::npos)
    {    
        retval.replace(index, sub_str.length(), sub_str); 
    }
    while ((index = retval.find("".."")) != string::npos)
    {    
        retval.replace(index, singleperiod.length(), singleperiod); 
    }
    
    
    return retval;
}
                ";
                    file += prodstr;
                }

                foreach (var pcase in prod.Cases)
                {
                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                            continue;

                    switch(pcase.CaseType)
                    {
                        case Grammar.CaseType.ArrayOp:
                            {
                                int i = 0, j = 0, k = 0;
                                var cons = "\n" + pcase.Name + "::" + pcase.Name + "(coords::" + pcase.Name + @"* c, domain::DomainObject* d, std::vector<interp::" + pcase.Productions[0].Name + @"*> operands)  :" + prod.Name + "(c, d)" + @" {
    for(auto& op : operands){
        this->operands_.push_back(op);
    }

};";

                                file += cons;
                                i = 0; j = 0;
                                //foreach (var casep in pcase.Productions)
                                //{
                                //   var opgetter = "\n" + "coords::" + casep.Name + "* " + pcase.Name + "::getOperand" + ++i + "() { return this->operand" + i + ";}";
                                //  file += opgetter;
                                //}
                                file += "\nstd::string " + pcase.Name + @"::toString() const{ 
    std::string retval = """";
    string cmdval = ""[]"";
    for(auto op: this->operands_){ 
        retval += ""\n"" + op->toString() + ""\n"";
        cmdval = op->coords_->toString() + ""::"" + cmdval;
    }
    cmdval = ""("" + cmdval + "")"";

    cmdval += """"; " + (
        pcase.Command is Grammar.Command && prod.Command is Grammar.Command ? @"
    cmdval = ""\ndef "" + this->coords_->toString() + """ + pcase.Command.NameSuffix + "" + @" : " + pcase.Command.Production + @" := " + pcase.Command.Production + '.' + pcase.Command.Case + @" "" + cmdval;

" + @"
    cmdval += ""\ndef "" + this->coords_->toString() + """ + "" + @" : " + prod.Command.Production + @" := " + prod.Command.Production + '.' + prod.Command.Case + @" "" + this->coords_->toString() + """ + pcase.Command.NameSuffix + "\"" + @";

"
        :
    ((
        pcase.Command is Grammar.Command ? @"
    cmdval = ""\ndef "" + this->coords_->toString() + """ + "" + @" : " + pcase.Command.Production + @" := " + pcase.Command.Production + '.' + pcase.Command.Case + @" "" + cmdval;

" : ""
    
    ) + (
        prod.Command is Grammar.Command ? @"
    cmdval = ""\ndef "" + this->coords_->toString() + """ + "" + @" : " + prod.Command.Production + @" := " + prod.Command.Production + '.' + prod.Command.Case + @" ("" + cmdval + "")"";


" : ""
    ))) + (
        (pcase.Command is Grammar.Command || prod.Command is Grammar.Command) ? @"
    retval += ""\n"" + cmdval + ""\n"";
    " : "") + @"

    //std::replace(retval.begin(), retval.end(), '_', '.');
    int index;
    string sub_str = "": _"";
    string singleperiod = "".a"";
    while ((index = retval.find("": ."")) != string::npos)
    {    
        retval.replace(index, sub_str.length(), sub_str); 
    }
    while ((index = retval.find("": ^"")) != string::npos)
    {    
        retval.replace(index, sub_str.length(), sub_str); 
    }
    while ((index = retval.find("".."")) != string::npos)
    {   
        retval.replace(index, singleperiod.length(), singleperiod); 
    }
    
    
    return retval;
}";
                                file += "\nstd::string " + pcase.Name + @"::toStringLinked(std::vector<interp::Space*> links, std::vector<std::string> names, std::vector<interp::Frame*> framelinks, std::vector<string> framenames, bool before) { 
    //std::string toStr = this->toString();
    std::string retval = """";
        string cmdvalstart = ""::[]"";
        string cmdval = """";
    int i = 0;
    if(before)
    {
        
        for(auto op: links){
            retval += ""\n"" + op->toString() + ""\n"";
            cmdval = names[i++] + ""::"" + cmdval;
            
        }
        i = 0;
        for(auto op: framelinks){
            retval += ""\n"" + op->toString() + ""\n"";
            cmdval = framenames[i++] + ""::"" + cmdval;
        }

        bool start = true;
        for(auto op: this->operands_){ 
            retval += ""\n"" + op->toString() + ""\n"";
            cmdval = cmdval + (!start?""::"":"""") + op->coords_->toString();
            start = false;
        }
    }
    else
    {
        for(auto op: this->operands_){ 
            retval += ""\n"" + op->toString() + ""\n"";
            cmdval = op->coords_->toString() + ""::"" + cmdval;
        }
        bool start = true;
        for(auto op: links){
            retval += ""\n"" + op->toString() + ""\n"";
            cmdval = cmdval + (!start?""::"":"""") + names[i++];
            start = false;
            
        }
        i = 0;
        for(auto op: framelinks){
            retval += ""\n"" + op->toString() + ""\n"";
            cmdval = framenames[i++] + ""::"" + cmdval;
        }

    }
    cmdval += """"; " + (
        pcase.Command is Grammar.Command && prod.Command is Grammar.Command ? @"
    cmdval = ""\ndef "" + this->coords_->toString() + """ + pcase.Command.NameSuffix + "" + @" : " + pcase.Command.Production + @" := " + pcase.Command.Production + '.' + pcase.Command.Case + @" ("" + cmdval + cmdvalstart + "")"";

" + @"
    cmdval += ""\ndef "" + this->coords_->toString() + """ + "" + @" : " + prod.Command.Production + @" := " + prod.Command.Production + '.' + prod.Command.Case + @" "" + this->coords_->toString() + """ + pcase.Command.NameSuffix + "\"" + @";

"
        :
    ((
        pcase.Command is Grammar.Command ? @"
    cmdval = ""\ndef "" + this->coords_->toString() + """ + "" + @" : " + pcase.Command.Production + @" := " + pcase.Command.Production + '.' + pcase.Command.Case + @" "" + cmdval;

" : ""

    ) + (
        prod.Command is Grammar.Command ? @"
    cmdval = ""\ndef "" + this->coords_->toString() + """ + "" + @" : " + prod.Command.Production + @" := " + prod.Command.Production + '.' + prod.Command.Case + @" "" + cmdval;


" : ""
    ))) + (
        (pcase.Command is Grammar.Command || prod.Command is Grammar.Command) ? @"
    retval += ""\n"" + cmdval + ""\n"";
    " : "") + @"

    //std::replace(retval.begin(), retval.end(), '_', '.');
    int index;
    string sub_str = "": _"";
    string singleperiod = "".a"";
    while ((index = retval.find("": ."")) != string::npos)
    {    
        retval.replace(index, sub_str.length(), sub_str); 
    }
    while ((index = retval.find("": ^"")) != string::npos)
    {    
        retval.replace(index, sub_str.length(), sub_str); 
    }
    while ((index = retval.find("".."")) != string::npos)
    {   
        retval.replace(index, singleperiod.length(), singleperiod); 
    }
    if(before)
    {
        
    }
    else
    {

    }

    return retval;
}";

                                var hm = (pcase.Command is Grammar.Command || prod.Command is Grammar.Command);
                                file += "\n\n";
                                break;
                            }
                        case Grammar.CaseType.Ident:
                            {
                                break;
                                var i = 0;
                                var j = 0;
                                var k = 0;

                                var casecons =
            "\n" + prod.Name + @"::" + prod.Name + @"(coords::" + prod.Name + @"* c, domain::DomainObject* d" + (pcase.Productions.Count > 0 ? "," + string.Join(",", pcase.Productions.Select(p_ => "interp::" + p_.Name + " * operand" + ++i)) : "") + @" ) : " + prod.Name + @"(c,d)
   " + (pcase.Productions.Count > 0 ? "," + string.Join(",", pcase.Productions.Select(p_ => "operand_" + ++k + "(operand" + k + ")")) : "") + @" {}
";
                                var casestr = @"
std::string " + prod.Name + @"::toString() const {
    std::string ret = """";
    //  ret += ""("";
    ret += ""def var_"" + std::to_string(++index) + "":= 1"";
     return ret;
}

";

                                file += "\n" + casecons;
                                file += casestr + "\n";
                                break;
                            }
                        default:
                            {
                                var i = 0;
                                var j = 0;
                                var k = 0;
                                var l = 0;

                                var casecons =
            "\n" + pcase.Name + @"::" + pcase.Name + @"(coords::" + pcase.Name + @"* c, domain::DomainObject* d" + (pcase.Productions.Count > 0 ? "," + string.Join(",", pcase.Productions.Select(p_ => "interp::" + p_.Name + " * operand" + ++i)) : "") + @" ) : " + prod.Name + @"(c,d)
   " + (pcase.Productions.Count > 0 ? "," + string.Join(",", pcase.Productions.Select(p_ => "operand_" + ++k + "(operand" + k + ")")) : "") + @" {}
";
                                var casestr = @"
std::string " + pcase.Name + @"::toString() const {
    bool found = false;
    std::string retval = """";" +
    (pcase.CaseType != Grammar.CaseType.Hidden ? string.Join("", pcase.Productions.Select(p_ => "\n\tretval += \"\\n\"+ operand_" + ++l + "->toString() + \"\\n\";")) : "")

    + @"
    //  ret += ""("";
    //ret += ""def var_"" + std::to_string(++index) + "":= 1"";" +
   string.Join("",
    ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
    ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
    {
        var spInstances = ParsePeirce.Instance.SpaceInstances;

        var retval = @"
    if(auto dc = dynamic_cast<domain::" + sppair.Item1.Prefix + sppair.Item2.Name + @"*>(this->dom_)){
        found = true;
        " + pcase.InterpTranslation(prod, sppair.Item1, sppair.Item2, ParsePeirce.Instance.SpaceInstances[0]) + @"
    }
"; return retval;


    }
    ) : new List<string>())
                    + @"
    if (auto cont = dynamic_cast<domain::DomainContainer*>(this->dom_)){
        if(cont->hasValue()){

                        " +
                        (
                        string.Join("",
                    ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                    ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair_ =>
                    {
                        var retval_ = @"
            if(auto dc = dynamic_cast<domain::" + sppair_.Item1.Prefix + sppair_.Item2.Name + @"*>(cont->getValue())){
                found = true;
                " + pcase.InterpTranslation(prod, sppair_.Item1, sppair_.Item2, ParsePeirce.Instance.SpaceInstances[0]) + @"
            }";
                        return retval_;
                    }
                    ) : new List<string>())) +
                        @"
        }
    }

    if(!found){
        //retval = """";
        " + (prod.ProductionType == Grammar.ProductionType.Single ? prod.Cases[0].InterpTranslation(prod, null, null, ParsePeirce.Instance.SpaceInstances[0]) : pcase.InterpTranslation(prod, null, null, null)/*@"std::cout<<""Warning - Calling toString on a production rather than a case\n;"";") */)+ @"
    }
    std::replace(retval.begin(), retval.end(), '_', '.');
    int index;
    string sub_str = "": _"";
    string singleperiod = "".a"";
    while ((index = retval.find("": ."")) != string::npos)
    {    
        retval.replace(index, sub_str.length(), sub_str); 
    }
    while ((index = retval.find("": ^"")) != string::npos)
    {    
        retval.replace(index, sub_str.length(), sub_str); 
    }
    while ((index = retval.find("".."")) != string::npos)
    {    
        retval.replace(index, singleperiod.length(), singleperiod);
    }
    

    return retval;
}
                ";

                                file += "\n" + casecons;
                                file += casestr + "\n";
                                break;
                            }
                    }
                }
            }
            file += "} // namespace coords";

            this.CppFile = file;
        }

        public override void GenHeader()
        {
            var header = @"#ifndef INTERP_H
#define INTERP_H

#include <cstddef>
#include <iostream> // for cheap logging only

#include ""Coords.h""
#include ""AST.h""
#include ""Domain.h""

namespace interp{

class Interp;
class Space;
class Frame;
";
            var file = header;

            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                if (prod.ProductionType != Grammar.ProductionType.Single)
                {
                    file += "\n";
                    file += "class " + prod.Name + ";";
                }

                foreach (var pcase in prod.Cases)
                {
                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    else if (pcase.CaseType == Grammar.CaseType.Ident)
                    {

                        file += "\n";
                        file += "class " + prod.Name + ";";
                    }
                    else
                    {
                        file += "\n";
                        file += "class " + pcase.Name + ";";
                    }
                }
            }

            var interp = @"
class Interp
{
public:
  Interp(coords::Coords *c, domain::DomainObject *d);
  Interp(){};
  std::string toString() const { return ""Not Implemented -- don't call this!!"";};
  //friend class Interp;
//protected:
  coords::Coords *coords_;
  domain::DomainObject *dom_;
};


class Space : public Interp
{
public:
    Space(domain::Space* s) : s_(s) {};
    std::string toString() const;
protected:
    domain::Space* s_;
};

class Frame : public Interp
{
public:
    Frame(domain::Frame* f) : f_(f) {};
    std::string toString() const;
protected:
    domain::Frame* f_;
};

";
            file += "\n\n" + interp + "\n\n";

            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                if (true || prod.ProductionType != Grammar.ProductionType.Single)
                {
                    var prodStr =
@"

class " + prod.Name + @" : public " + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Interp") + @" {
public:
    " + prod.Name + @"(coords::" + prod.Name + @"* coords, domain::DomainObject* dom);
    virtual std::string toString() const;
    //friend class Interp;              
};

";
                    file += prodStr;
                }
                foreach (var pcase in prod.Cases)
                {
                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    var i = 0;
                    var j = 0;

                    switch (pcase.CaseType)
                    {
                        case Grammar.CaseType.ArrayOp:
                            {
                                var caseStr =
    @"

class " + pcase.Name + @" : public " + prod.Name + @" {
public:
    " + pcase.Name + "(coords::" + pcase.Name + @"* coords, domain::DomainObject* dom, std::vector<" + pcase.Productions[0].Name + @"*> operands);
    virtual std::string toString() const;
    virtual std::string toStringLinked(std::vector<interp::Space*> links, std::vector<std::string> names, std::vector<interp::Frame*> framelinks, std::vector<string> framenames, bool before);
    void link(std::vector<" + pcase.Productions[0].Name + @"*> operands);
    //friend class Interp;              
    " +

    "\nprotected:\n\t" + @"
    std::vector<interp::" + pcase.Productions[0].Name + @"*> operands_;

};

";
                                file += caseStr;
                                break;
                            }
                        case Grammar.CaseType.Ident:
                            {
                                break;
                                var caseStr = @"

class " + prod.Name + @" : public " + prod.Name + @" {
public:
    " + prod.Name + @"(coords::" + prod.Name + @"* coords, domain::DomainObject* dom " + (pcase.Productions.Count > 0 ? "," + string.Join(",", pcase.Productions.Select(p_ => "interp::" + p_.Name + " *operand" + ++i)) : "") + @" );
    virtual std::string toString() const;
    " +

            // string.Join("\n", pcase.Productions.Select(p_ => "Interp::" + p_.Name + " *getOperand" + ++i) + "(); ")


            "\nprotected:\n\t" +
            string.Join("\n\t", pcase.Productions.Select(p_ => "interp::" + p_.Name + " *operand_" + ++j + ";"))
            +
            @"
};

";
                                file += caseStr;
                                break;
                            }
                        default:
                            {


                                var caseStr = @"

class " + pcase.Name + @" : public " + prod.Name + @" {
public:
    " + pcase.Name + @"(coords::" + pcase.Name + @"* coords, domain::DomainObject* dom " + (pcase.Productions.Count > 0 ? "," + string.Join(",", pcase.Productions.Select(p_ => "interp::" + p_.Name + " *operand" + ++i)) : "") + @" );
    virtual std::string toString() const override ;
    //friend class Interp;              
    " +

            // string.Join("\n", pcase.Productions.Select(p_ => "Interp::" + p_.Name + " *getOperand" + ++i) + "(); ")


            "\nprotected:\n\t" +
            string.Join("\n\t", pcase.Productions.Select(p_ => "interp::" + p_.Name + " *operand_" + ++j + ";"))
            +
            @"
};

";
                                file += caseStr;
                                break;
                            }
                    
                    }
                }
            }

            file += @"
} // namespace coords

#endif";
            this.HeaderFile = file;
        }
    }
}

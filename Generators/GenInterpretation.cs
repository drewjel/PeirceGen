using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen.Generators
{
    public class GenInterpretation : GenBase
    {
        public override string GetCPPLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["GenPath"] + "Interpretation.cpp";
        }

        public override string GetHeaderLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["GenPath"] + "Interpretation.h";
        }
        public override void GenCpp()
        {
            var header = @"
/*
Establish interpretations for AST nodes:

-  get any required information from oracle 
-  create coordinates for object
-  update ast-coord bijections
-  create corresponding domain object
-  update coord-domain bijection
-  create element-wise inter object
-  update maps: coords-interp, interp-domain
*/

// TODO: These two can be integrated
#include ""Coords.h""
#include ""Interpretation.h""
#include ""Interp.h""
#include ""CoordsToInterp.h""
#include ""CoordsToDomain.h""
#include ""InterpToDomain.h""
#include ""ASTToCoords.h""
#include ""Oracle_AskAll.h""    // default oracle
//#include ""Space.h""
#include ""Checker.h""

//#include <g3log/g3log.hpp>
#include <unordered_map>
#include <memory>
using namespace interp;

Interpretation::Interpretation() { 
    domain_ = new domain::Domain();
    oracle_ = new oracle::Oracle_AskAll(domain_); 
    /* 
    context_ can only be set later, once Clang starts parse
    */
    ast2coords_ = new ast2coords::ASTToCoords(); 
    coords2dom_ = new coords2domain::CoordsToDomain();
    coords2interp_ = new coords2interp::CoordsToInterp();
    interp2domain_ = new interp2domain::InterpToDomain();
    checker_ = new Checker(this);
}

std::string Interpretation::toString_AST(){
    //this should technically be in Interp but OKAY this second
    std::string math = """";

    math += ""import .lang.imperative_DSL.physlang\n\n"";
    math += ""noncomputable theory\n\n"";
            math += ""def "" + interp::getEnvName() + "" := init_env"";
            //math += interp->toString_Spaces();
            //math += interp->toString_PROGRAMs();
            math += this->toString_COMPOUND_STMTs();

            return math;
        };


";
            /*
             * 
             * void Interpretation::mkVecIdent(ast::VecIdent *ast)
                {
                  coords::VecIdent *coords = ast2coords_->mkVecIdent(ast, context_);
                  LOG(DBUG) << "Interpretation::mkVecIdent. ast=" << std::hex << ast << ", " << coords->toString() << "\n";
                  //domain::Space &space = oracle_->getSpaceForVecIdent(coords);
                  domain::VecIdent *dom = domain_->mkVecIdent();
                  coords2dom_->putVecIdent(coords, dom);
                  interp::VecIdent *interp = new interp::VecIdent(coords, dom);
                  coords2interp_->putVecIdent(coords, interp);
                  interp2domain_->putVecIdent(interp, dom);
                }
             * */


            var file = header;


            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                foreach (var pcase in prod.Cases)
                {
                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;

                    switch (prod.ProductionType)
                    {
                        case Grammar.ProductionType.CaptureSingle:
                        case Grammar.ProductionType.Single:
                            {
                                int i = 0, j = 0, k = 0, l = 0, m = 0, n = 0, p = 0;
                                var mkstr =

            @"void Interpretation::mk" + prod.Name + @"(const ast::" + prod.Name + @" * ast " + (pcase.Productions.Count > 0 ? "," +
            string.Join(",", pcase.Productions.Select(p_ => "ast::" + p_.Name + "* operand" + ++i)) : "") + (prod.HasValueContainer() ? "," +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "std::shared_ptr<" + prod.GetPriorityValueContainer().ValueType + "> value" + v /*+ "=nullptr"*/) : "") + @") {
" + (pcase.Productions.Count > 0 ? "\n\t" +
            string.Join(";\n\t", pcase.Productions.Select(p_ => "coords::" + p_.Name + "* operand" + ++j + "_coords = static_cast<coords::" + p_.Name + "*>(ast2coords_->" + ( p_.ProductionType == Grammar.ProductionType.Single || (p_.IsVarDeclare || p_.IsTranslationDeclare || p_.IsFuncDeclare) ? "getDeclCoords" : "getStmtCoords" ) + @"(operand" + j + "));")) : "") + @"

    coords::" + prod.Name + @"* coords = ast2coords_->mk" + prod.Name + @"(ast, context_ " + (pcase.Productions.Count > 0 ? "," +
            string.Join(",", pcase.Productions.Select(p_ => "operand" + ++k + "_coords")) : "") + (prod.HasValueContainer() ? "," +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "value" + v) : "") + @");
" + (pcase.Productions.Count > 0 ? "\n\t" +
            string.Join("\n\t", pcase.Productions.Select(p_ => "domain::DomainObject* operand" + ++l + "_dom = coords2dom_->get" + p_.Name + "(operand" + l + "_coords);")) : "") + @"
    domain::DomainObject* dom = domain_->mkDefaultDomainContainer({" + string.Join(",", pcase.Productions.Select(p_ => "operand" + ++p + "_dom")) + @"});
    coords2dom_->put" + prod.Name + @"(coords, dom);
" + (pcase.Productions.Count > 0 ? "\n\t" +
            string.Join(";\n\t", pcase.Productions.Select(p_ => "interp::" + p_.Name + @"* operand" + ++m + "_interp = coords2interp_->get" + p_.Name + @"(operand" + m + "_coords);")) : "") + @"

    interp::" + prod.Name + @"* interp = new interp::" + prod.Name + @"(coords, dom" + (pcase.Productions.Count > 0 ? ", " +
            string.Join(",", pcase.Productions.Select(p_ => "operand" + ++n + "_interp")) : "") + @");
    coords2interp_->put" + prod.Name + @"(coords, interp);
    interp2domain_->put" + prod.Name + @"(interp, dom); " + "\n\tthis->" + prod.Name + @"_vec.push_back(coords);

} ";
                                file += mkstr + "\n\n";
                                break;
                            }
                        default:
                            {
                                switch (pcase.CaseType)
                                {
                                    case Grammar.CaseType.Passthrough:
                                        continue;
                                    case Grammar.CaseType.Inherits:
                                        continue;
                                    case Grammar.CaseType.Op:
                                    case Grammar.CaseType.Hidden:
                                    case Grammar.CaseType.Pure:
                                        {
                                            int i = 0, j = 0, k = 0, l = 0, m = 0, n = 0, p = 0;
                                            var mkstr =

                        @"void Interpretation::mk" + pcase.Name + @"(const ast::" + pcase.Name + @" * ast " + (pcase.Productions.Count > 0 ? "," +
                        string.Join(",", pcase.Productions.Select(p_ => "ast::" + p_.Name + "* operand" + ++i)) : "") +
                        (prod.HasValueContainer() ? "," +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "std::shared_ptr<" + prod.GetPriorityValueContainer().ValueType + "> value" + v /* + "=nullptr"*/) : "") + @") {
"   /*+
    (prod.HasValueContainer() ? 
                        Peirce.Join("", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => prod.GetPriorityValueContainer().ValueType + "value" + v + "=value"+v+"?value"+v+":new "+ prod.GetPriorityValueContainer().ValueType+"();") : "")
                        */

    + (pcase.Productions.Count > 0 ? "\n\t" +
                        string.Join(";\n\t", pcase.Productions.Select(p_ => "coords::" + p_.Name + "* operand" + ++j + "_coords = static_cast<coords::" 
                            + p_.Name + "*>(ast2coords_->" + ( p_.ProductionType == Grammar.ProductionType.Single || p_.ProductionType == Grammar.ProductionType.CaptureSingle || (p_.IsVarDeclare || p_.IsTranslationDeclare || p_.IsFuncDeclare)  ? "getDeclCoords" : "getStmtCoords" ) + @"(operand" + j + "));")) : "") + @"

    coords::" + pcase.Name + @"* coords = ast2coords_->mk" + pcase.Name + @"(ast, context_ " + (pcase.Productions.Count > 0 ? "," +
                        string.Join(",", pcase.Productions.Select(p_ => "operand" + ++k + "_coords")) : "") +
                        (prod.HasValueContainer() ? "," +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "value" + v) : "") + @");
" + (pcase.Productions.Count > 0 ? "\n\t" +
                        string.Join("\n\t", pcase.Productions.Select(p_ => "domain::DomainObject* operand" + ++l + "_dom = coords2dom_->get" + p_.Name + "(operand" + l + "_coords);")) : "") + @"
    domain::DomainObject* dom =  domain_->mkDefaultDomainContainer({" + string.Join(",", pcase.Productions.Select(p_ => "operand" + ++p + "_dom")) + @"});
    coords2dom_->put" + pcase.Name + @"(coords, dom);
" + (pcase.Productions.Count > 0 ? "\n\t" +
                        string.Join(";\n\t", pcase.Productions.Select(p_ => "interp::" + p_.Name + @"* operand" + ++m + "_interp = coords2interp_->get" + p_.Name + @"(operand" + m + "_coords);")) : "") + @"

    interp::" + pcase.Name + @"* interp = new interp::" + pcase.Name + @"(coords, dom" + (pcase.Productions.Count > 0 ? ", " +
                        string.Join(",", pcase.Productions.Select(p_ => "operand" + ++n + "_interp")) : "") + @");
    coords2interp_->put" + pcase.Name + @"(coords, interp);
    interp2domain_->put" + pcase.Name + @"(interp, dom); " + "\n\tthis->" + prod.Name + @"_vec.push_back(coords);

} ";

                                            file += mkstr + "\n\n";
                                            break;
                                        }
                                    case Grammar.CaseType.Ident:
                                        {
                                            break;
                                        }
                                    case Grammar.CaseType.ArrayOp:
                                        {
                                            //int i = 0,  k = 0, l = 0, m = 0, n = 0, o = 0;

                                            var mkStr = @"

void Interpretation::mk" + pcase.Name + @"(const ast::" + pcase.Name + @" * ast , std::vector <ast::" + pcase.Productions[0].Name + @"*> operands) {
//const ast::COMPOUND_STMT * ast , std::vector < ast::STMT *> operands 
	//coords::" + pcase.Productions[0].Name + @"* operand1_coords = static_cast<coords::" + pcase.Productions[0].Name + @"*>(ast2coords_->" 
+ (pcase.Productions[0].ProductionType == Grammar.ProductionType.Single || pcase.Productions[0].ProductionType == Grammar.ProductionType.CaptureSingle || (pcase.IsVarDeclare || pcase.IsTranslationDeclare || pcase.IsFuncDeclare) ? "getDeclCoords" : "getStmtCoords") + @"(operands));

    vector<coords::" + pcase.Productions[0].Name + @"*> operand_coords;

    for(auto op : operands)
    {
        " + ( pcase.Productions[0].IsFuncDeclare || pcase.Productions[0].IsTranslationDeclare || pcase.Productions[0].IsVarDeclare ? @"
        if(ast2coords_->existsDeclCoords(op)){
            operand_coords.push_back(static_cast<coords::" + pcase.Productions[0].Name + @"*>(ast2coords_->getDeclCoords(op)));
        } " : @"
        if(ast2coords_->existsStmtCoords(op)){
            operand_coords.push_back(static_cast<coords::" + pcase.Productions[0].Name + @"*>(ast2coords_->getStmtCoords(op)));
        } ") + @"
    }

    coords::" + pcase.Name + @"* coords = ast2coords_->mk" + pcase.Name + @"(ast, context_ ,operand_coords);

	//domain::DomainObject* operand1_dom = coords2dom_->get" + pcase.Productions[0].Name + @"(operand_coords);

    vector<domain::DomainObject*> operand_domain;

    for(auto op : operand_coords)
    {
        operand_domain.push_back(coords2dom_->get" + pcase.Productions[0].Name + @"(op));
    }

    domain::DomainObject* dom = domain_->mkDefaultDomainContainer(operand_domain);
    coords2dom_->put" + pcase.Name + @"(coords, dom);

	//interp::" + pcase.Productions[0].Name + @"* operand1_interp = coords2interp_->get" + pcase.Productions[0].Name + @"(operand1_coords);

    vector<interp::" + pcase.Productions[0].Name + @"*> operand_interp;

    for(auto op : operand_coords)
    {
        //auto p = coords2interp_->get" + pcase.Productions[0].Name + @"(op);
        operand_interp.push_back(coords2interp_->get" + pcase.Productions[0].Name + @"(op));
    }

    interp::" + pcase.Name + @"* interp = new interp::" + pcase.Name + @"(coords, dom, operand_interp);
    coords2interp_->put" + pcase.Name + @"(coords, interp);
    interp2domain_->put" + pcase.Name + @"(interp, dom); 
	this->" + prod.Name + @"_vec.push_back(coords);
	this->" + pcase.Name + @"_vec.push_back(coords);
};";
                                            file += mkStr + "\n\n";

                                            var printpcase = @" std::string Interpretation::toString_" + pcase.Name + @"s(){ 
    std::vector<interp::" + pcase.Name + @"*> interps;
    for(auto coord : this->" + pcase.Name + @"_vec){
        interps.push_back((interp::" + pcase.Name + @"*)this->coords2interp_->get" + prod.Name + @"(coord));
    }
    std::string retval = """";
    for(auto interp_ : interps){" +
        (pcase.LinkSpace ? @"
        retval += ""\n"" + interp_->toStringLinked(this->getSpaceInterps(), this->getSpaceNames(), this->getFrameInterps(), this->getFrameNames(), true) + ""\n"";
" : 
     @"
        retval += ""\n"" + interp_->toString() + ""\n"";") + @"
    }
    return retval;
}";
                                            file += "\n" + printpcase+ "\n";

                                            break;
                                        }
                                    /*case Grammar.CaseType.Value:
                                        {
                                            int i = 0, j = 0, k = 0, l = 0, m = 0, n = 0, o = 0, p = 0;
                                            var mkstr =

                        @"void Interpretation::mk" + pcase.Name + @"(const ast::" + pcase.Name + @" * ast " + (pcase.ValueCount > 0 ? "," +
                        Peirce.Join(",",Enumerable.Range(0, pcase.ValueCount),v=>pcase.ValueType + " operand"+v ):"") + @") {
" + (pcase.Productions.Count > 0 ? "\n\t" +
                        string.Join(";\n\t", pcase.Productions.Select(p_ => "coords::" + p_.Name + "* operand" + ++j + "_coords = static_cast<coords::" + p_.Name + "*>(ast2coords_->" 
                        + (p_.ProductionType == Grammar.ProductionType.Single || 
                        p_.ProductionType == Grammar.ProductionType.CaptureSingle ||
                        (p_.IsVarDeclare || p_.IsTranslationDeclare || p_.IsFuncDeclare) ? "getDeclCoords" : "getStmtCoords") + @"(operand" + j + "));")) : "") + @"

    coords::" + pcase.Name + @"* coords = ast2coords_->mk" + pcase.Name + @"(ast, context_ " + (pcase.ProductionRefs.Count > 0 ? "," +
                        Peirce.Join(",", Enumerable.Range(0, pcase.ValueCount), v =>"operand"+v) : "") + @");
" + (pcase.Productions.Count > 0 ? "\n\t" +
                        string.Join("\n\t", pcase.Productions.Select(p_ => "domain::DomainObject* operand" + ++l + "_dom = coords2dom_->get" + p_.Name + "(operand" + l + "_coords);")) : "") + @"
    domain::DomainObject* dom = domain_->mkDefaultDomainContainer({" + string.Join(",", pcase.Productions.Select(p_ => "operand" + ++p + "_dom")) + @"});
    coords2dom_->put" + pcase.Name + @"(coords, dom);
" + (pcase.Productions.Count > 0 ? "\n\t" +
                        string.Join(";\n\t", pcase.Productions.Select(p_ => "interp::" + p_.Name + @"* operand" + ++m + "_interp = coords2interp_->get" + p_.Name + @"(operand" + m + "_coords);")) : "") + @"

    interp::" + pcase.Name + @"* interp = new interp::" + pcase.Name + @"(coords, dom" + (pcase.Productions.Count > 0 ? ", " +
                        string.Join(",", pcase.Productions.Select(p_ => "operand" + ++n + "_interp")) : "") + @");
    coords2interp_->put" + pcase.Name + @"(coords, interp);
    interp2domain_->put" + pcase.Name + @"(interp, dom); " + "\n\tthis->" + prod.Name + @"_vec.push_back(coords);

} ";

                                            file += mkstr + "\n\n";
                                            break;
                                        }*/
                                }

                                break;
                            }
                    }

                    

                    //domain::DomainObject* dom = domain_->mk" + pcase.Name + @"({" + (pcase.Productions.Count > 0 ? "" +
                    //string.Join(",", pcase.Productions.Select(p_ => "operand" + ++o + "_dom")) : "") +@"});
                    //
                }
                var printprod = @" std::string Interpretation::toString_" + prod.Name + @"s(){ 
    std::vector<interp::"+ prod.Name + @"*> interps;
    for(auto coord : this->" + prod.Name + @"_vec){
        interps.push_back(this->coords2interp_->get" + prod.Name + @"(coord));
    }
    std::string retval = """";
    for(auto interp_ : interps){
        retval += ""\n"" + interp_->toString() + ""\n"";
    }
    return retval;
}";
                file += "\n" + printprod + "\n";
            }
            //int q = 0;
            var printspace = @"

std::string Interpretation::toString_Spaces() {
      //  int index = 0;
    std::string retval = """";
    //std::vector<domain::Space*> & s = domain_->getSpaces();
    //for (std::vector<domain::Space*>::iterator it = s.begin(); it != s.end(); ++it)
     //   retval = retval.append(""def "")
     //                   .append((*it)->toString())
     //                   .append("" : peirce.vector_space := peirce.vector_space.mk "")
     //                   .append(std::to_string(index++))
     //                   .append(""\n"");
     //auto spaces = domain_->getSpaces();
    " + string.Join("",ParsePeirce.Instance.Spaces.Select(sp_ => "\n\tauto " + sp_.Name + @"s = domain_->get" + sp_.Name + @"Spaces();
    for (auto it = " + sp_.Name + @"s.begin(); it != " + sp_.Name + @"s.end(); it++)
    {
        auto sp = interp2domain_->getSpace(*it);
        retval.append(""\n"" + (sp->toString()) + ""\n"");
    }
            ")) + @"

    return retval;
}   

std::vector<interp::Space*> Interpretation::getSpaceInterps() {
    std::vector<interp::Space*> interps;
    " + string.Join("", ParsePeirce.Instance.Spaces.Select(sp_ => "\n\tauto " + sp_.Name + @"s = domain_->get" + sp_.Name + @"Spaces();
    for (auto it = " + sp_.Name + @"s.begin(); it != " + sp_.Name + @"s.end(); it++)
    {
        auto sp = interp2domain_->getSpace(*it);
        interps.push_back(sp);
    }
            ")) + @"

    return interps;
}   

std::vector<std::string> Interpretation::getSpaceNames() {
    std::vector<std::string> names;
    " + string.Join("", ParsePeirce.Instance.Spaces.Select(sp_ => "\n\tauto " + sp_.Name + @"s = domain_->get" + sp_.Name + @"Spaces();
    for (auto it = " + sp_.Name + @"s.begin(); it != " + sp_.Name + @"s.end(); it++)
    {
        //auto sp = interp2domain_->getSpace(*it);
        names.push_back((*it)->getName());
    }
            ")) + @"

    return names;
}

std::vector<interp::Frame*> Interpretation::getFrameInterps() {
    std::vector<interp::Frame*> interps;
    " + string.Join("", ParsePeirce.Instance.Spaces.Select(sp_ => "\n\tauto " + sp_.Name + @"s = domain_->get" + sp_.Name + @"Spaces();
    for (auto it = " + sp_.Name + @"s.begin(); it != " + sp_.Name + @"s.end(); it++)
    {
        auto frs = (*it)->getFrames();

        for(auto fr : frs){
            auto intfr = interp2domain_->getFrame(fr);
            interps.push_back(intfr);
        }
    }
            ")) + @"

    return interps;
}   

std::vector<std::string> Interpretation::getFrameNames() {
    std::vector<std::string> names;
    " + string.Join("", ParsePeirce.Instance.Spaces.Select(sp_ => "\n\tauto " + sp_.Name + @"s = domain_->get" + sp_.Name + @"Spaces();
    for (auto it = " + sp_.Name + @"s.begin(); it != " + sp_.Name + @"s.end(); it++)
    {
        auto frs = (*it)->getFrames();

        for(auto fr : frs){
            //auto intfr = interp2domain_->getFrame(fr);
            names.push_back((*it)->getName()+"".""+fr->getName());
        }
    }
            ")) + @"

    return names;
}




";



            file += printspace;
            //string.Join("\n\t", (ParsePeirce.Instance.SpaceInstances.Select(inst => "domain::"+inst.TypeName + " " + inst.InstanceName + string.Join(",",inst.FieldValues)))) 
            file += @"    
void Interpretation::buildDefaultSpaces(){
    " +/* string.Join("\n\t", (ParsePeirce.Instance.SpaceInstances.Select(inst => {
                var fv = inst.FieldValues.Prepend(inst.InstanceName).ToArray();//ANDREW -- FIX YOUR CODE ! WOW. WHAT ON EARTH... 

                fv[0] = @""""+ fv[0] + @"""";
                fv[1] = @"""" + fv[1] + @"""";

                return "auto " + inst.FieldValues[0] + "= domain_->mk" + inst.TypeName + "(" + string.Join(",", fv) + @");
    auto i" + inst.FieldValues[0] + @" = new interp::Space(" + inst.FieldValues[0] + @");
    interp2domain_->putSpace(i" + inst.FieldValues[0] + @", " + inst.FieldValues[0] + @");
    auto standard_frame" + inst.FieldValues[0] + @" = " + inst.FieldValues[0] + @"->getFrames()[0];
    auto interp_frame" + inst.FieldValues[0] + @" = new interp::Frame(standard_frame" + inst.FieldValues[0] + @");
    interp2domain_->putFrame(interp_frame" + inst.FieldValues[0] + @", " + inst.FieldValues[0] + @"->getFrames()[0]);";
            }))) +*/
    @"


}

void Interpretation::buildSpace(){
    int index = 0;
    int choice = 0;
    int size = " + ParsePeirce.Instance.Spaces.Count + @";
    if (size == 0){
        std::cout<<""Warning: No Available Spaces to Build"";
        return;
    }
    while((choice <= 0 or choice > size)){ 
        std::cout<<""Available types of Spaces to build:\n"";
        " +
        string.Join("\n\t\t", (ParsePeirce.Instance.Spaces.Select(sp_=> "std::cout <<\"(\"<<std::to_string(++index)<<\")\"<<\"" +sp_.Name + "\\n\";")))
        +   @"
        std::cin>>choice;
    }
    index = 0;
    " + 
    string.Join("\n\t", ParsePeirce.Instance.Spaces.Select(sp_=>
    {
        //bool hasName = sp_.MaskContains(Space.FieldType.Name);
        //bool hasDim = sp_.MaskContains(Space.FieldType.Dimension);
        if (sp_.IsDerived)
        {
          var str = @"
        if(choice==++index){
            std::string name;
            domain::Space *base1,*base2;
            std::cout<<""Enter Name (string):\n"";
            std::cin>>name;
            int index = 0;
            std::unordered_map<int, domain::Space*> index_to_sp;
        " + string.Join("", ParsePeirce.Instance.Spaces.Select(sp__ => "\n\tauto " + sp__.Name + @"s = domain_->get" + sp__.Name + @"Spaces();
            for (auto it = " + sp__.Name + @"s.begin(); it != " + sp__.Name + @"s.end(); it++)
            {
                std::cout<<""(""<<std::to_string(++index)<<"")""<<(*it)->toString() + ""\n"";
                index_to_sp[index] = *it;
            }")) + @"

            if(index==0){
                std::cout<<""Unable to Proceed - No Existing Spaces\n"";
                return;
            }
            int choice;
            label1st:
            std::cout<<""Select First Base Space : ""<<""\n"";
            std::cin>>choice;
            if(choice >0 and choice <=index){
                base1 = index_to_sp[choice];
            }
            else
                goto label1st;
            
            label2nd:
            std::cout<<""Select Second Base Space : ""<<""\n"";
            std::cin>>choice;
            if(choice >0 and choice <=index){
                base2 = index_to_sp[choice];
            }
            else
                goto label2nd;
            auto sp = this->domain_->mk" + sp_.Name + @"(name, name, base1, base2);
            auto ib1 = this->interp2domain_->getSpace(base1);
            auto ib2 = this->interp2domain_->getSpace(base2);

            auto isp = new interp::DerivedSpace(sp, ib1, ib2);
            interp2domain_->putSpace(isp, sp);
            auto standard_framesp = sp->getFrames()[0];
            auto interp_framesp = new interp::Frame(standard_framesp);
            interp2domain_->putFrame(interp_framesp, sp->getFrames()[0]);
        }
";
            return str;
        }
        else {
            var str = @"
        if(choice==++index){
            std::string name;
            std::cout<<""Enter Name (string):\n"";
            std::cin>>name;
            "
            +
            (sp_.DimensionType == Space.DimensionType_.ANY ? @"
            int dimension;
            std::cout<<""Enter Dimension (integer):\n"";
            std::cin>>dimension;
            auto sp = this->domain_->mk" + sp_.Name + @"(name, name, dimension);
    " : @"
            auto sp = this->domain_->mk" + sp_.Name + @"(name, name);")
            +
            @"
            auto isp = new interp::Space(sp);
            interp2domain_->putSpace(isp, sp);
            auto standard_framesp = sp->getFrames()[0];
            auto interp_framesp = new interp::Frame(standard_framesp);
            interp2domain_->putFrame(interp_framesp, sp->getFrames()[0]);
        }

    " +
        @"";
            return str;
        }
    })) + 
    @"
}

void Interpretation::buildFrame(){
    while(true){
        std::cout<<""Select Space : ""<<""\n"";
        int index = 0;
        std::unordered_map<int, domain::Space*> index_to_sp;
    " + string.Join("", ParsePeirce.Instance.Spaces.Select(sp_ => "\n\tauto " + sp_.Name + @"s = domain_->get" + sp_.Name + @"Spaces();
        for (auto it = " + sp_.Name + @"s.begin(); it != " + sp_.Name + @"s.end(); it++)
        {
            std::cout<<""(""<<std::to_string(++index)<<"")""<<(*it)->toString() + ""\n"";
            index_to_sp[index] = *it;
        }")) + @"
        int choice;
        std::cin>>choice;
        if(choice >0 and choice <=index){
            auto chosen = index_to_sp[choice];
            std::cout<<""Building Frame For : ""<<chosen->toString()<<""\n"";
            auto frames = chosen->getFrames();
            std::cout<<""Select Parent Frame : ""<<""\n"";
            index = 0;
            std::unordered_map<int, domain::Frame*> index_to_fr;
        
            auto frs = chosen->getFrames();
            for(auto fr : frs){
            std::cout<<""(""<<std::to_string(++index)<<"")""<<(fr)->toString()<<""\n"";
            index_to_fr[index] = fr;
            }
            choice = 0;
            std::cin>>choice;
            if(choice > 0 and choice<= index){
                auto parent = index_to_fr[index];
                std::cout<<""Enter Name of Frame:\n"";
                std::string name;
                std::cin>>name;
                auto child = domain_->mkFrame(name, chosen, parent);
                interp::Frame* interp = new interp::Frame(child);
                interp2domain_->putFrame(interp, child);
                return;
            }
            
        }

    }
}

void Interpretation::printSpaces(){
    int index = 0;
    " + string.Join("", ParsePeirce.Instance.Spaces.Select(sp_ => "\n\tauto " + sp_.Name + @"s = domain_->get" + sp_.Name + @"Spaces();
    for (auto it = " + sp_.Name + @"s.begin(); it != " + sp_.Name + @"s.end(); it++)
    {
        std::cout<<""(""<<std::to_string(++index)<<"")""<<(*it)->toString() + ""\n"";
    }")) + @"
}

void Interpretation::printFrames(){
    int index = 0;
    " + string.Join("", ParsePeirce.Instance.Spaces.Select(sp_ => "\n\tauto " + sp_.Name + @"s = domain_->get" + sp_.Name + @"Spaces();
    for (auto it = " + sp_.Name + @"s.begin(); it != " + sp_.Name + @"s.end(); it++)
    {
        std::cout<<""Printing Frames For : "" + (*it)->toString() + ""\n"";
        auto frs = (*it)->getFrames();
        index = 0;
        for(auto fr : frs){
            std::cout<<""(""<<std::to_string(++index)<<"")""<<fr->toString() + ""\n"";
        }
    }")) + @"

}

void Interpretation::mkVarTable(){" +
    (ParsePeirce.Instance.Grammar.Productions.Where(p => p.ProductionType == Grammar.ProductionType.Capture || p.ProductionType == Grammar.ProductionType.CaptureSingle).ToList().Count > 0 ? @"
    int idx = 0; " : "")
    + @"
  
" + string.Join("\n\t", ParsePeirce.Instance.Grammar.Productions.Where(p => p.ProductionType == Grammar.ProductionType.Capture || p.ProductionType == Grammar.ProductionType.CaptureSingle).Select(p => @"
    for(auto it = this->" + p.Name + @"_vec.begin(); it != this->" + p.Name + @"_vec.end(); it++){
        this->index2coords_[++idx] = *it;
        (*it)->setIndex(idx);
    }
")) + @"

}

//print the indexed variable table for the user
void Interpretation::printVarTable(){ " +
    (ParsePeirce.Instance.Grammar.Productions.Where(p => p.ProductionType == Grammar.ProductionType.Capture || p.ProductionType == Grammar.ProductionType.CaptureSingle).ToList().Count > 0 ? @"
    
  int sz = this->index2coords_.size();

  for(int i = 1; i<=sz;i++)
  {
    coords::Coords* coords = this->index2coords_[i];
    if(false){}
" +
        string.Join("\n", ParsePeirce.Instance.Grammar.Productions.Where(p_ => p_.ProductionType == Grammar.ProductionType.Capture || p_.ProductionType == Grammar.ProductionType.CaptureSingle).SelectMany(p_ => (p_.Cases.Where(c_ => c_.CaseType != Grammar.CaseType.Passthrough).Select(c_ => @"
    else if(auto dc = dynamic_cast<coords::" + (p_.ProductionType == Grammar.ProductionType.Single || 
        p_.ProductionType == Grammar.ProductionType.CaptureSingle ? p_.Name : c_.Name) + @"*>(this->index2coords_[i])){
        auto dom = (domain::DomainContainer*)this->coords2dom_->get" 
+ (p_.ProductionType == Grammar.ProductionType.Single || p_.ProductionType == Grammar.ProductionType.CaptureSingle ? p_.Name : c_.Name) + @"(dc);
        std::cout<<""Index: ""<<i<<"",""<<" + (string.IsNullOrEmpty(c_.Description) ? "" : @"""" + c_.Description + @",""<<") + @"dc->toString()<<"", SourceLocation:""<<dc->getSourceLoc()<<""\nExisting Interpretation: ""<<dom->toString()<<std::endl;

    }"))))  : "{");

            file += @"
    
  }

}//make a printable, indexed table of variables that can have their types assigned by a user or oracle

//void Interpretation::printVarTable(){}//print the indexed variable table for the user
//while loop where user can select a variable by index and provide a physical type for that variable
void Interpretation::updateVarTable(){
  auto sz = (int)this->index2coords_.size()+1;
  int choice;
  try{
        checker_->CheckPoll();
        //std::cout << ""********************************************\n"";
        std::cout << ""********************************************\n"";
        std::cout << ""********************************************\n"";
        std::cout << ""See type-checking output in /peirce/phys/deps/orig/PeirceOutput.lean\n"";
        std::cout << ""********************************************\n"";
        //std::cout << ""********************************************\n"";
        std::cout << ""********************************************\n"";
        std::cout<<""Enter -1 to print Available Spaces\n"";
        std::cout<<""Enter -2 to create a New Space\n"";
        std::cout<<""Enter -3 to print Available Frames\n"";
        std::cout<<""Enter -4 to create a New Frame\n"";
        std::cout<<""Enter 0 to print the Variable Table again.\n"";
        std::cout << ""Enter the index of a Variable to update its physical type. Enter "" << sz << "" to exit and check."" << std::endl;
        std::cin >> choice;
        std::cout << std::to_string(choice) << ""\n"";


        while ((choice == -3 || choice == -2 || choice == -1 || choice == 0 || this->index2coords_.find(choice) != this->index2coords_.end()) && choice != sz)
        {
            if (choice == -4)
            {
                this->buildFrame();
            }
            else if(choice == -3)
            {
                this->printFrames();
            }
            else if(choice == -2)
            {
                this->buildSpace();
            }
            else if (choice == -1)
            {
                this->printSpaces();
            }
            else if (choice == 0)
            {
                this->printVarTable();
            }
            else
            {
                if(false){}
" +
        string.Join("\n", ParsePeirce.Instance.Grammar.Productions.Where(p_ => p_.ProductionType == Grammar.ProductionType.Capture || p_.ProductionType == Grammar.ProductionType.CaptureSingle).SelectMany(p_ => (p_.Cases.Where(c_ => c_.CaseType != Grammar.CaseType.Passthrough).Select(c_ => @"
                else if(auto dc = dynamic_cast<coords::" + (p_.ProductionType == Grammar.ProductionType.Single || p_.ProductionType == Grammar.ProductionType.CaptureSingle ? p_.Name : c_.Name) + @"*>(this->index2coords_[choice])){
                    auto dom = this->coords2dom_->get" + (p_.ProductionType == Grammar.ProductionType.Single || p_.ProductionType == Grammar.ProductionType.CaptureSingle ? p_.Name : c_.Name) + @"(dc);
                    auto interp = this->coords2interp_->get" + (p_.ProductionType == Grammar.ProductionType.Single || p_.ProductionType == Grammar.ProductionType.CaptureSingle ? p_.Name : c_.Name) + @"(dc);
                    //this->coords2dom_->erase" + (p_.ProductionType == Grammar.ProductionType.Single || p_.ProductionType == Grammar.ProductionType.CaptureSingle ? p_.Name : c_.Name) + @"(dc, dom);
                    //this->interp2domain_->erase" + (p_.ProductionType == Grammar.ProductionType.Single || p_.ProductionType == Grammar.ProductionType.CaptureSingle ? p_.Name : c_.Name) + @"(interp, dom);
                    auto upd_dom = this->oracle_->getInterpretationFor" + p_.Name + @"(dc, dom);
                    if(upd_dom){//remap, hopefully everything works fine from here
                        //this->coords2dom_->erase" + (p_.ProductionType == Grammar.ProductionType.Single || p_.ProductionType == Grammar.ProductionType.CaptureSingle ? p_.Name : c_.Name) + @"(dc, dom);
                        //this->interp2domain_->erase" + (p_.ProductionType == Grammar.ProductionType.Single || p_.ProductionType == Grammar.ProductionType.CaptureSingle ? p_.Name : c_.Name) + @"(interp, dom);
                        //upd_dom->setOperands(dom->getOperands());
                        ((domain::DomainContainer*)dom)->setValue(upd_dom);
                        //this->coords2dom_->put" + (p_.ProductionType == Grammar.ProductionType.Single || p_.ProductionType == Grammar.ProductionType.CaptureSingle ? p_.Name : c_.Name) + @"(dc, upd_dom);
                        //this->interp2domain_->put" + (p_.ProductionType == Grammar.ProductionType.Single || p_.ProductionType == Grammar.ProductionType.CaptureSingle ? p_.Name : c_.Name) + @"(interp, upd_dom);
                        //delete dom;
                    }
                }")))) + @"
            }
            checker_->CheckPoll();
            std::cout << ""********************************************\n"";
            std::cout << ""********************************************\n"";
            //std::cout << ""********************************************\n"";
            std::cout << ""See type-checking output in /peirce/phys/deps/orig/PeirceOutput.lean\n"";
            std::cout << ""********************************************\n"";
            std::cout << ""********************************************\n"";
            //std::cout << ""********************************************\n"";
            std::cout<<""Enter -1 to print Available Spaces\n"";
            std::cout<<""Enter -2 to create a New Space\n"";
            std::cout<<""Enter -3 to print Available Frames\n"";
            std::cout<<""Enter -4 to create a New Frame\n"";
            std::cout<<""Enter 0 to print the Variable Table again.\n"";
            std::cout << ""Enter the index of a Variable to update its physical type. Enter "" << sz << "" to exit and check."" << std::endl;
            std::cin >> choice;
            std::cout << std::to_string(choice) << ""\n"";
        }
    }
    catch(std::exception ex){
        std::cout<<ex.what()<<""\n"";
    }
};

void remap(coords::Coords c, domain::DomainObject newinterp){
    return;
};

";



            this.CppFile = file;
        }

        public override void GenHeader()
        {
            var header = @"
#include ""Checker.h""
class Checker;

#ifndef INTERPRETATION_H
#define INTERPRETATION_H

#include <iostream>
#include ""AST.h""
#include ""Coords.h""
#include ""Domain.h""
//#include ""Space.h""
#include ""ASTToCoords.h""
#include ""CoordsToDomain.h""
#include ""Oracle.h""
#include ""CoordsToInterp.h""
#include ""InterpToDomain.h""
#include <g3log/g3log.hpp> 
#include <memory>


#include <unordered_map>

namespace interp {

// TODO: Take clang::ASTContext
class Interpretation
{
public:
    Interpretation();

    std::string toString_AST();

    void setOracle(oracle::Oracle* oracle)
    {
        oracle_ = oracle;
    }

    void setASTContext(clang::ASTContext* context)
    {
        context_ = context;
    }

    //void addSpace(std::string type, std::string name, int dimension)
    //{
    //    domain_->mkSpace(type, name, dimension);
    //}

    domain::Domain* getDomain()
    {
        return domain_;
    }

    void setSources(std::vector<std::string> sources)
    {
        this->sources_ = sources;
    }

    std::vector<std::string> getSources()
    {
        return this->sources_;
    }

";
            var file = header;

            foreach(var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                if(prod.ProductionType == Grammar.ProductionType.Single || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                {
                    int j = 0;
                    var mkStr = @"void mk" + prod.Name + @"(const ast::" + prod.Name + @" * ast " + (prod.Cases[0].Productions.Count > 0 ? "," +
            string.Join(",", prod.Cases[0].Productions.Select(p_ => "ast::" + p_.Name + "* operand" + ++j)) : "") +
                        (prod.HasValueContainer() ? "," +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "std::shared_ptr<" + prod.GetPriorityValueContainer().ValueType + "> value" + v + "=nullptr") : "") + @");
                    ";
                    file += "\n\t" + mkStr;
                }

                foreach(var pcase in prod.Cases)
                {
                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    else if (prod.ProductionType == Grammar.ProductionType.Single || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                        continue;
                    int i = 0;
                    switch (pcase.CaseType)
                    {
                        case Grammar.CaseType.Passthrough:
                            continue;
                        case Grammar.CaseType.Op:
                        case Grammar.CaseType.Hidden:
                        case Grammar.CaseType.Pure:
                            {
                                var mkStr = @"void mk" + pcase.Name + @"(const ast::" + pcase.Name + @" * ast " + (pcase.Productions.Count > 0 ? "," +
                        string.Join(",", pcase.Productions.Select(p_ => "ast::" + p_.Name + "* operand" + ++i)) : "") +
                        (prod.HasValueContainer() ? "," +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "std::shared_ptr<" + prod.GetPriorityValueContainer().ValueType + "> value" + v + "=nullptr") : "") + @");
                    ";
                                file += "\n\t" + mkStr;
                                break;
                            }
                        case Grammar.CaseType.Ident:
                            {
                                break;/*
                                var mkStr = @"void mk" + prod.Name + @"(const ast::" + prod.Name + @" * ast " + (pcase.Productions.Count > 0 ? "," +
                        string.Join(",", pcase.Productions.Select(p_ => "ast::" + p_.Name + "* operand" + ++i)) : "") + @");
                    ";
                                file += "\n\t" + mkStr;
                                break;*/
                            }
                        case Grammar.CaseType.ArrayOp:
                        {
                        var mkStr = @"void mk" + pcase.Name + @"(const ast::" + pcase.Name + @" * ast , std::vector < ast::" + pcase.Productions[0].Name + @" *> operands );
                    ";
                        var pstr = @"std::string toString_" + pcase.Name + "s();";

                        file += "\n\t" + pstr + "\n";
                                file += "\n\t" + mkStr;
                            break;
                        }

                        /*case Grammar.CaseType.Value:
                            {
                                var mkStr = @"void mk" + pcase.Name + @"(const ast::" + pcase.Name + @" * ast " + (pcase.ValueCount > 0 ? "," +
                        Peirce.Join(",", Enumerable.Range(0, pcase.ValueCount), v => pcase.ValueType + " operand" + v + (!string.IsNullOrEmpty(pcase.ValueDefault) ? "=" + pcase.ValueDefault : "")) : "") + @");
                    ";
                                file += "\n\t" + mkStr;
                                break;
                            }
                            */
                    }
                }
                var printstr = @"std::string toString_"+prod.Name+"s();";

                file += "\n\t" + printstr + "\n";
            }

            file += "\n\tstd::string toString_Spaces();\n\tstd::vector<interp::Space*> getSpaceInterps();\n\tstd::vector<std::string> getSpaceNames();";
            file += "\n\tstd::vector<interp::Frame*> getFrameInterps();\n\tstd::vector<std::string> getFrameNames();\n\t";
            var footer = @"    void buildDefaultSpaces();

    void buildSpace();
    void buildFrame();

    //void setAll_Spaces();
    void printSpaces();
    void printFrames();
    void mkVarTable();//make a printable, indexed table of variables that can have their types assigned by a user or oracle
    void printVarTable();//print the indexed variable table for the user
    void updateVarTable();//while loop where user can select a variable by index and provide a physical type for that variable
    void remap(coords::Coords c, domain::DomainObject newinterp);

    /*
    * Builds a list of variables that have a type either assigned or inferred.
    * Used for runtime constraint generation/logging 
    */
    //void buildTypedDeclList();
    
    
    /*
    used for generating dynamic constraints.
    given a variable, determine whether or not it does not have a type available (if so, a constraint must be registered)
    */
    //bool needsConstraint(clang::VarDecl* var);

// TODO: Make private
    domain::Domain *domain_;
    oracle::Oracle *oracle_;
    clang::ASTContext *context_;
    coords2domain::CoordsToDomain *coords2dom_;
    ast2coords::ASTToCoords *ast2coords_;
    coords2interp::CoordsToInterp *coords2interp_;
    interp2domain::InterpToDomain *interp2domain_; 
    Checker *checker_;
    std::vector<std::string> sources_;
"
+
string.Join("\n",ParsePeirce.Instance.Grammar.Productions.Select(p_ => "\tstd::vector<coords::" + p_.Name + "*> " + p_.Name + "_vec;"))
+
string.Join("\n",ParsePeirce.Instance.Grammar.Productions.SelectMany(p_ => p_.Cases).Where(p_=>p_.CaseType == Grammar.CaseType.ArrayOp)
                                                        .Select(p_ => "\tstd::vector<coords::" + p_.Name + "*> " + p_.Name + "_vec;"))
+
@"

    std::unordered_map<int, coords::Coords*> index2coords_;
    std::unordered_map<int, void*> index2dom_;

    //populated after initial pass of AST
    //list of scalars/vecs that do not have any assigned or inferred type
   // std::vector<ast::VecIdent*> unconstrained_vecs;
    //std::vector<std::string> unconstrained_vec_names;
    //std::vector<ast::ScalarIdent*> unconstrained_floats;
    //std::vector<std::string> unconstrained_float_names;
    //std::vector<ast::TransformIdent*> unconstrained_transforms;
   // std::vector<std::string> unconstrained_transform_names;
}; 

} // namespaceT

#endif
";
            file += footer;
            this.HeaderFile = file;
        }

        
    }
}

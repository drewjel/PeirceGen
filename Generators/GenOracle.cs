using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen.Generators
{
    public class GenOracle : GenBase
    {
        public GenOracle() : base()
        {
            GenBaseHeaderFile();

            System.IO.File.WriteAllText(this.GetBaseHeaderLoc(), this.BaseHeaderFile);
        }

        public string BaseHeaderFile { get; protected set; }

        public override void GenCpp()
        {
            var header = @"
// Oracle_AskAll.cpp. An oracle that asks interactively for
// information on every vector-valued term.

#include ""Oracle_AskAll.h""

# include <string>
# include <iostream>
# include <g3log/g3log.hpp>
# include <vector>
#include <memory>

//using namespace std;
using namespace oracle;


";
            var file = header;

            var getFrame = @"domain::Frame* Oracle_AskAll::getFrame(domain::Space* space){

    auto frames = space->getFrames();
    auto sz = (int)frames.size();
            
    while(true){
        int i = 0;
        std::cout<<""Available Frames For : "" << space->toString() << ""\n"";
        for(auto fr : frames){
            std::cout<<""(""+std::to_string((i++))+"") ""<<fr->toString()<<""\n"";
        }
        int choice = 0;
        std::cin>>choice;
        if(choice > 0 and choice <= sz){
            return frames[choice-1];
        }
    }
    return nullptr;
}";
            file += "\n" + getFrame + "\n";


            var getinterpretation = @"
domain::DomainObject* Oracle_AskAll::getInterpretation(coords::Coords* coords, domain::DomainObject* dom){

";

            var ifstub = "\tif(false){}";

           // file += getinterpretation + "\n\t" + ifstub + "\n\t";

            var prodcopy = default(List<Grammar.Production>);

            prodcopy = new List<Grammar.Production>(ParsePeirce.Instance.Grammar.Productions);
            file += getinterpretation + ifstub;
            var getters = "";
            foreach(var prod in ParsePeirce.Instance.Grammar.Productions.Where(p_ => p_.ProductionType == Grammar.ProductionType.Capture || p_.ProductionType == Grammar.ProductionType.CaptureSingle))
            {
                var cur = prod;

                while(cur != default(Grammar.Production) && prodcopy.Any(p_ => p_.Name == cur.Name)
                    && (cur.ProductionType == Grammar.ProductionType.Capture || cur.ProductionType == Grammar.ProductionType.CaptureSingle))
                {
                    prodcopy.Remove(cur);

                    var ifcase = @"
    else if(auto dc = dynamic_cast<coords::" + cur.Name + @"*>(coords)){";
                    var detected = @"
    
        return this->getInterpretationFor" + cur.Name + @"(dc, dom);
    }";
                    var choices = ""; 
                    var cases = "";
                    int i = 0, j = 0;
                    var getterbegin = @"

domain::DomainObject* Oracle_AskAll::getInterpretationFor" + cur.Name + @"(coords::" + cur.Name + @" * coords, domain::DomainObject * dom){
    std::cout << ""Provide new interpretation for : "" << """ + cur.Description + @""";
    std::cout << ""\nExisting interpretation:   "";
    std::cout << dom->toString();
    std::cout << ""\nAt location:  "";
    std::cout << coords->getSourceLoc();
    int choice;
    choose:
    std::cout<<""\nAvailable Interpretations (Enter numeral choice) : \n"";
    
    //return getInterpretation(coords);

                    ";


                    if (!ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.Keys.Contains(cur))
                    {
                        getters += getterbegin + @"
    std::cout<<""None available!\n"";
    return this->domain_->mkDefaultDomainContainer();
}
";
                        goto END;
                    }

                    foreach (var sppair in ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[cur])
                    {/*
                        spInstances.Where(inst => inst.TypeName == sppair.Item1.Name).ToList().ForEach(inst =>
                        {
                            var selectFrame = 

                            choices += "" + @"
    std::cout<<""(" + ++i + @")""<<""@@" + sppair.Item1.Name + sppair.Item2.Name + "(" + inst.InstanceName + @")\n"";";
                        cases += @"
            case " + ++j + @" : 
            {
            domain::" + sppair.Item1.Name + @"* " + inst.InstanceName + @" = (domain::"+sppair.Item1.Name+@"*)this->domain_->getSpace(""" + inst.InstanceName + @""");
            auto ret = this->domain_->mk" + sppair.Item1.Name + sppair.Item2.Name + "(" + inst.InstanceName + @");" + 
                (sppair.Item2.HasFrame ? @"
            auto frame = (domain::" + sppair.Item1.Name + @"Frame*)this->getFrame(" + inst.InstanceName + @"); 
            ret->setFrame(frame);" : "")
            + @"
            }"; 
                        });*/

                        choices += @"
    std::cout<<""(" + ++i + @")""<<""@@" + sppair.Item1.Name + sppair.Item2.Name + "(" + @")\n"";";
                        cases += @"
            case " + ++j + @" : 
            {
                std::vector<domain::" + sppair.Item1.Name + @"*> spaces = this->domain_->get" + sppair.Item1.Name + @"Spaces();
                while(spaces.size()>0){
                    int sp_choice = 0;
                    int index = 0;

                    std::unordered_map<int,domain::" + sppair.Item1.Name + @"*> index_to_sp;

                    std::cout<<""Choose " + sppair.Item1.Name + @" Space to Attach to This Annotation : \n"";

                    for(auto sp : spaces){
                        index_to_sp[++index] = sp;
                        std::cout<<""(""<<std::to_string(index)<<"") ""<<sp->toString()<<""\n"";
                
                    }
                    std::cin>>sp_choice;
                    if(sp_choice >0 and sp_choice <= index){
                        auto sp = index_to_sp[sp_choice];" +
                            (sppair.Item2.HasFrame ? @"
                        " + (cur.HasValueContainer() ? "std::shared_ptr<"+cur.GetPriorityValueContainer().ValueType + "> cp[" + cur.GetPriorityValueContainer().ValueCount + @"];
                                auto vals = ((coords::ValueCoords<" + cur.GetPriorityValueContainer().ValueType + "," + cur.GetPriorityValueContainer().ValueCount + @">*)coords)->getValues();
                                for(int idx = 0;idx < " + cur.GetPriorityValueContainer().ValueCount + @";idx++){
                                    cp[idx] = vals[idx] ? std::make_shared<" + cur.GetPriorityValueContainer().ValueType + @">(*vals[idx]) : nullptr;
                                }
                    " : "") + @"

                        auto ret = this->domain_->mk" + sppair.Item1.Name + sppair.Item2.Name + 
                            (cur.HasValueContainer() ? 
                                "<" + cur.GetPriorityValueContainer().ValueType + "," + cur.GetPriorityValueContainer().ValueCount + ">" : "") 
                                + @"(sp" + (cur.HasValueContainer() ? ",cp" : "") + @");
                        //delete[] cp;
                        auto frame = (domain::" + sppair.Item1.Name + @"Frame*)this->getFrame(sp); 
                        ret->setFrame(frame);"
+ (cur.HasValueContainer() ? new List<string>() {"one" }.Select(x =>
{
                            var query = @"
                        std::cout<<""Provide Values For Interpretation? (1) Yes(2) No\n"";
                        try{
                            int vchoice = 0;
                            std::cin >> vchoice;
                            if (vchoice == 1)
                            {
                                for (int i = 0; i < " + cur.GetPriorityValueContainer().ValueCount + @"; i++)
                                {
                                    std::cout << ""Enter Value "" << i << "":\n"";
                                    " + cur.GetPriorityValueContainer().ValueType + @" val = 4;
                                    std::cin >> val;
                                    //" + cur.GetPriorityValueContainer().ValueType + @"* vc = new float(valvc);
                                    ret->setValue(val, i);
                                    //delete vc;
                                }
                            }
                        }
                        catch(std::exception ex){
                            return ret;
                        }
/*
    while (true){
                            std::cout<<""Provide Values For Interpretation? (1) Yes (2) No\n"";
                            int vchoice = 0;
                            std::cin>>vchoice;
                            if(vchoice == 1){
                                for(int i = 0; i<" + cur.GetPriorityValueContainer().ValueCount + @";i++){
                                    std::cout<<""Enter Value ""<<i<<"":\n"";
                                    " + cur.GetPriorityValueContainer().ValueType + @" valvc;
                                    std::cin>>valvc;
                                    " + cur.GetPriorityValueContainer().ValueType + @"* vc;
                                    ret->setValue(vc, i);
                                    delete vc;
                                }
                                break;
                            }
                            else if(vchoice == 0){
                                break;
                            }
                            else if(vchoice != 0)
                                continue;
                        }*/
                        ";


                            return query;
                        }).First() : "") +@"
                        return ret;" : 
                            sppair.Item2.IsTransform ? @"
                        while(true){
                            auto frs = sp->getFrames();
                            std::cout<<""Enter Frame of Transform Domain : \n"";
                            std::unordered_map<int, domain::Frame*> index_to_dom;
                            int dom_index = 0,
                                cod_index = 0;
                            int dom_choice = 0, 
                                cod_choice = 0;
                            for(auto fr: frs){
                                index_to_dom[++dom_index] = fr;
                                std::cout<<""(""<<std::to_string(index)<<"") ""<<fr->toString()<<""\n"";
                            }
                            std::cin>>dom_choice;

                        
                            std::cout<<""Enter Frame of Transform Co-Domain : \n"";
                            std::unordered_map<int, domain::Frame*> index_to_cod;
                            for(auto fr: frs){
                                index_to_cod[++cod_index] = fr;
                                std::cout<<""(""<<std::to_string(index)<<"") ""<<fr->toString()<<""\n"";
                            }
                            std::cin>>cod_choice;

                            if(dom_choice >0 and dom_choice <= dom_index and cod_choice >0 and cod_choice <= cod_index){
                                auto mapsp = this->domain_->mkMapSpace(sp, index_to_dom[dom_choice], index_to_cod[cod_index]);
                                " + (cur.HasValueContainer() ? "std::shared_ptr<" + cur.GetPriorityValueContainer().ValueType + "> cp[" + cur.GetPriorityValueContainer().ValueCount + @"];
                                auto vals = ((coords::ValueCoords<" + cur.GetPriorityValueContainer().ValueType + "," + cur.GetPriorityValueContainer().ValueCount + @">*)coords)->getValues();
                                for(int idx = 0;idx < " + cur.GetPriorityValueContainer().ValueCount + @";idx++){
                                    cp[idx] = vals[idx] ? std::make_shared<" + cur.GetPriorityValueContainer().ValueType + @">(*vals[idx]) : nullptr;
                                }" : "") + @"

                                auto ret = this->domain_->mk" + sppair.Item1.Name + sppair.Item2.Name + (cur.HasValueContainer() ?
                                "<" + cur.GetPriorityValueContainer().ValueType + "," + cur.GetPriorityValueContainer().ValueCount + ">" : "") + @"(mapsp" 
                                    + (cur.HasValueContainer() ?
                                        ",cp" : "") + @");
                               // delete[] cp;
"
+ (cur.HasValueContainer() ? new List<string>() { "one" }.Select(x =>
{
    var query = @"
                        std::cout<<""Provide Values For Interpretation? (1) Yes(2) No\n"";
                        try{
                            int vchoice = 0;
                            std::cin >> vchoice;
                            if (vchoice == 1)
                            {
                                for (int i = 0; i < " + cur.GetPriorityValueContainer().ValueCount + @"; i++)
                                {
                                    std::cout << ""Enter Value "" << i << "":\n"";
                                    " + cur.GetPriorityValueContainer().ValueType + @" val = 4;
                                    std::cin >> val;
                                    //" + cur.GetPriorityValueContainer().ValueType + @"* vc = new float(valvc);
                                    ret->setValue(val, i);
                                    //delete vc;
                                }
                            }
                        }
                        catch(std::exception ex){
                            return ret;
                        }
/*
    while (true){
                            std::cout<<""Provide Values For Interpretation? (1) Yes (2) No\n"";
                            int vchoice = 0;
                            std::cin>>vchoice;
                            if(vchoice == 1){
                                for(int i = 0; i<" + cur.GetPriorityValueContainer().ValueCount + @";i++){
                                    std::cout<<""Enter Value ""<<i<<"":\n"";
                                    " + cur.GetPriorityValueContainer().ValueType + @" valvc;
                                    std::cin>>valvc;
                                    " + cur.GetPriorityValueContainer().ValueType + @"* vc;
                                    ret->setValue(vc, i);
                                    delete vc;
                                }
                                break;
                            }
                            else if(vchoice == 0){
                                break;
                            }
                            else if(vchoice != 0)
                                continue;
                        }*/
                                ";


    return query;
}).First() : "") + @"
                                return ret;

                            }
                        
                        }
                        " : @"
                        " + (cur.HasValueContainer() ? "std::shared_ptr<" + cur.GetPriorityValueContainer().ValueType + "> cp[" + cur.GetPriorityValueContainer().ValueCount + @"];
                                auto vals = ((coords::ValueCoords<" + cur.GetPriorityValueContainer().ValueType + "," + cur.GetPriorityValueContainer().ValueCount + @">*)coords)->getValues();
                                for(int idx = 0;idx < " + cur.GetPriorityValueContainer().ValueCount + @";idx++){
                                    cp[idx] = vals[idx] ? std::make_shared<" + cur.GetPriorityValueContainer().ValueType + @">(*vals[idx]) : nullptr;
                                }" : "") + @"

                        auto ret = this->domain_->mk" + sppair.Item1.Name + sppair.Item2.Name +
                            (cur.HasValueContainer() ? 
                                "<" + cur.GetPriorityValueContainer().ValueType + "," + cur.GetPriorityValueContainer().ValueCount + ">" : "") 
                                     + @"(sp" + (cur.HasValueContainer() ?
                                        ",cp" : "") + @"); 
                        //delete[] cp;
" 
                        
+ (cur.HasValueContainer() ? new List<string>() {"one" }.Select(x =>
{
    var query = @"
                        std::cout<<""Provide Values For Interpretation? (1) Yes(2) No\n"";
                        try{
                            int vchoice = 0;
                            std::cin >> vchoice;
                            if (vchoice == 1)
                            {
                                for (int i = 0; i < " + cur.GetPriorityValueContainer().ValueCount + @"; i++)
                                {
                                    std::cout << ""Enter Value "" << i << "":\n"";
                                    " + cur.GetPriorityValueContainer().ValueType + @" val = 4;
                                    std::cin >> val;
                                    //" + cur.GetPriorityValueContainer().ValueType + @"* vc = new float(valvc);
                                    ret->setValue(val, i);
                                    //delete vc;
                                }
                            }
                        }
                        catch(std::exception ex){
                            return ret;
                        }
/*
    while (true){
                            std::cout<<""Provide Values For Interpretation? (1) Yes (2) No\n"";
                            int vchoice = 0;
                            std::cin>>vchoice;
                            if(vchoice == 1){
                                for(int i = 0; i<" + cur.GetPriorityValueContainer().ValueCount + @";i++){
                                    std::cout<<""Enter Value ""<<i<<"":\n"";
                                    " + cur.GetPriorityValueContainer().ValueType + @" valvc;
                                    std::cin>>valvc;
                                    " + cur.GetPriorityValueContainer().ValueType + @"* vc;
                                    ret->setValue(vc, i);
                                    delete vc;
                                }
                                break;
                            }
                            else if(vchoice == 0){
                                break;
                            }
                            else if(vchoice != 0)
                                continue;
                        }*/
                        ";


    return query;
}).First() : "") + @"
                        
                        return ret;")
                        + @"
            
                    }
                }
                if(spaces.size() == 0){
                    std::cout<<""Invalid Annotation: No Available " + sppair.Item1.Name + @" Spaces!\n"";
                    return nullptr;

                    std::cout<<""Provide Another Intepretation\n"";
                    goto choose;
                }
            }";
                    }


                    var ifclose = @"
    std::cin>>choice;
    if(choice < 1 or choice > " + j + @") {
        goto choose;
    } else {
        switch(choice){" + "\n" + string.Join("\n", cases) + @"

        }
    }
  
";
                    getters += getterbegin +choices +  ifclose + "\n}";
                    file += ifcase + detected;
                   // file += detected;
                    //file += choices;
                    //file += ifclose;
                    END:
                    cur = cur.GetTopPassthrough();
                }


            }

            file += "\n\treturn nullptr;\n}";

            file += getters;



            /*
var p = @"
    std::vector<domain::Space*>& spaces = this->domain_->getSpaces();
	if (spaces.size() == 0) {
		LOG(FATAL) <<""Oracle_AskAll::getSpace:: No abstract spaces available for interpretation. Bye!\n"";
		exit(1);
    }
    printSpaces(spaces);
    int whichSpace = selectSpace(spaces);
	if (whichSpace< 0 || whichSpace >= (int) spaces.size())
	{
		domain::Space* resultptr = nullptr;
		return * resultptr;
}
	else{
		domain::Space& result = * spaces[whichSpace];
		return result;
	}
}
";
            */

            this.CppFile = file;

        }

        public override void GenHeader()
        {
            var header = @"
#ifndef ORACLE_ASKALL_H
#define ORACLE_ASKALL_H

#include ""Oracle.h""
#include ""Domain.h""

namespace oracle{

class Oracle_AskAll : public Oracle 
{
public:
	Oracle_AskAll(domain::Domain* d) " + (ParsePeirce.Instance.Grammar.Productions.Where(p_ => p_.ProductionType == Grammar.ProductionType.Capture || p_.ProductionType == Grammar.ProductionType.CaptureSingle).ToList().Count > 0 ? @" : domain_(d)" : "") + @" { };

    domain::DomainObject* getInterpretation(coords::Coords* coords, domain::DomainObject* dom);

    domain::Frame* getFrame(domain::Space* space);

    //domain::Space &getSpace();
    //domain::MapSpace &getMapSpace();";

            var file = header;

            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
  //              foreach (var pcase in prod.Cases)
  //              {
 //                   if (pcase.CaseType == Grammar.CaseType.Passthrough)
  //                      continue;

//                    var getstr = @"

    
//";
                    //file += getstr;
                //}
            }

            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                //foreach(var pcase in prod.Cases)
                //{
                //if (pcase.CaseType == Grammar.CaseType.Passthrough)
                //  continue;

                if (prod.ProductionType == Grammar.ProductionType.Capture || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                {


                    var getstr = @"
    virtual domain::DomainObject* getInterpretationFor"+ prod.Name +@"(coords::"+ prod.Name +@"* coords, domain::DomainObject* dom);
";
                    file += getstr;
                }
                else
                {
                    continue;
                    /*var getstr = @"
    virtual domain::DomainObject* getInterpretationFor" + prod.Name + @"(coords::" + prod.Name + @"* coords, domain::DomainObject* dom);
";
                    file += getstr;*/
                    // }
                }
            }

            var footer = (ParsePeirce.Instance.Grammar.Productions.Where(p_=>p_.ProductionType == Grammar.ProductionType.Capture || p_.ProductionType == Grammar.ProductionType.CaptureSingle).ToList().Count > 0 ? @"
protected:
	domain::Domain* domain_;
};

} // namespace

#endif
" : @"
};
} // namespace

#endif"
);
            file += footer;

            this.HeaderFile = file;
        }

        public override string GetCPPLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["GenPath"] + "Oracle_AskAll.cpp";
        }

        public override string GetHeaderLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["GenPath"] + "Oracle_AskAll.h";
        }

        public void GenBaseHeaderFile()
        {
            var header = @"
#ifndef ORACLE_H
#define ORACLE_H

#include <string>
#include <iostream>
#include ""Coords.h""
#include ""Domain.h""

namespace oracle {

class Oracle {
public:";
            var file = header;

            var footer = @"

};

} // namespace

#endif
";
            foreach(var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                //foreach(var pcase in prod.Cases)
                //{
                //if (pcase.CaseType == Grammar.CaseType.Passthrough)
                //  continue;
                if (prod.ProductionType ==  Grammar.ProductionType.Capture || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                {
                    var getstr = @"
    virtual domain::DomainObject* getInterpretationFor" + prod.Name + @"(coords::" + prod.Name + @"* coords, domain::DomainObject* dom) = 0;
";
                    file += getstr;
                    // }
                }
                else
                {
                    continue;
                    /*var getstr = @"
    virtual domain::DomainObject* getInterpretationFor" + prod.Name + @"(coords::" + prod.Name + @"* coords, domain::DomainObject* dom) = 0;
";
                    file += getstr;*/
                }
            }
            file += footer;
            /*
            var spInsts = ParsePeirce.Instance.SpaceInstances;
            foreach(var sp in ParsePeirce.Instance.Spaces)
            {
                foreach(var spobj in sp.Inherits.Objects)
                {

                }
            }*/
            this.BaseHeaderFile = file;
        }

        public string GetBaseHeaderLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["GenPath"] + "Oracle.h";
        }
    }
}

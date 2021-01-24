using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen.Generators
{
    public class GenInterp : GenBase
    {
        public override string GetCPPLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["GenPath"] + "Interp.cpp";
        }

        public override string GetHeaderLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["GenPath"] + "Interp.h";
        }
        public override void GenCpp()
        {
            var header = @"#include ""Interp.h""

#include ""Domain.h""
#include ""InterpToDomain.h""

#include <g3log/g3log.hpp>

#include <algorithm>
#include <unordered_map>

using namespace g3; 

namespace interp{

int GLOBAL_INDEX = 0;
std::unordered_map<Interp*,int> GLOBAL_IDS;
int ENV_INDEX = 0;
//this will get removed in the future once physlang stabilizes
interp2domain::InterpToDomain* i2d_;


std::string getEnvName(){
    return ""env"" + std::to_string(++ENV_INDEX);
};

std::string getLastEnv(){
    return ""env"" + std::to_string(ENV_INDEX - 1);
};

Interp::Interp(coords::Coords* c, domain::DomainObject* d) : coords_(c), dom_(d){
}

std::string Space::toString() const {
    std::string retval = """";
    bool found = false; if (found) {}
    
    int id = GLOBAL_IDS.count(const_cast<Space*>(this)) ? GLOBAL_IDS[const_cast<Space*>(this)] : GLOBAL_IDS[const_cast<Space*>(this)] = (GLOBAL_INDEX += 2); 
    
" + string.Join("",ParsePeirce.Instance.Spaces.Where(sp_=>!sp_.IsDerived).Select(sp_ => {

                    return "\n\tif(auto dc = dynamic_cast<domain::" + sp_.Name + @"*>(s_)){
            found = true;
           // retval += ""def "" + dc->getName() + ""var : lang." + sp_.Prefix + @".var := lang." + sp_.Prefix + @".var.mk "" + std::to_string(id) + """" + ""\n"";
            //retval += " + (sp_.DimensionType == Space.DimensionType_.ANY ? @"""\ndef "" + dc->getName() + ""sp := lang." + sp_.Prefix + @".expr.lit (" + sp_.Prefix + @".mk "" + std::to_string(id-1) + "" "" + std::to_string(dc->getDimension()) + "")""; " :
                             @"""\ndef "" + dc->getName() + ""sp := lang." + sp_.Prefix + @".expr.lit (" + sp_.Prefix + @".mk "" + std::to_string(id) + "")""; ") + @"
            //retval += ""\ndef "" + dc->getName() + "" := cmd." + sp_.Prefix + @"Assmt (⟨⟨"" + std::to_string(id) + ""⟩⟩) (" + (sp_.DimensionType == Space.DimensionType_.ANY ? @"lang." + sp_.Prefix + @".spaceExpr.lit(" + sp_.Prefix + @".build "" + std::to_string(id) + "" "" + std::to_string(dc->getDimension()) + "")""" : @"lang." + sp_.Prefix + @".spaceExpr.lit(" + sp_.Prefix + @".build "" + std::to_string(id-1) + "")""") + @""")\n"";
            " + 
             (
                sp_.DimensionType == Space.DimensionType_.ANY ? 
                "" 
                :
                @"
            retval += ""\ndef "" + dc->getName() + "" := \n\tlet var := (⟨⟨"" + std::to_string(id) + ""⟩⟩ : lang." + sp_.Prefix + @".spaceVar) in\n"";
            retval += ""\tlet spaceLiteral := lang." + sp_.Prefix + @".spaceExpr.lit(" + sp_.Prefix + @".build "" + std::to_string(id-1) + "") in\n"";
            retval += ""\tvar=spaceLiteral"";"
             )  
            +@"
            retval += ""\ndef "" + getEnvName() + "" := cmdEval "" + dc->getName() + "" "" + getLastEnv();
    }";
            })) + @"

    if(!found){
        //retval = ""--Unknown space type - Translation Failed!"";
    }

    return retval;
};

std::string Space::toDefString() const {
    return this->toString();
};

std::string Space::getVarExpr() const {
    " + string.Join("", ParsePeirce.Instance.Spaces.Select(sp_ => {
                return "\n\tif(auto dc = dynamic_cast<domain::" + sp_.Name + @"*>(s_)){
            int id = GLOBAL_IDS.count(const_cast<Space*>(this)) ? GLOBAL_IDS[const_cast<Space*>(this)] : GLOBAL_IDS[const_cast<Space*>(this)] = (GLOBAL_INDEX += 2); 
    
            return ""lang." + sp_.Prefix + @".expr.var (lang." + sp_.Prefix + @".spaceVar.mk "" + std::to_string(id) + "")"";

    }";

            })) + @"
    return """";
}

std::string Space::getEvalExpr() const {
    auto lastEnv = getLastEnv();

" + string.Join("", ParsePeirce.Instance.Spaces.Select(sp_ => {
                return "\n\tif(auto dc = dynamic_cast<domain::" + sp_.Name + @"*>(s_)){
            int id = GLOBAL_IDS.count(const_cast<Space*>(this)) ? GLOBAL_IDS[const_cast<Space*>(this)] : GLOBAL_IDS[const_cast<Space*>(this)] = (GLOBAL_INDEX += 2); 
    
            return ""(" + sp_.Prefix + @"Eval (lang." + sp_.Prefix + @".spaceExpr.var (lang." + sp_.Prefix + @".spaceVar.mk "" + std::to_string(id) + "")) (" +  @" "" + lastEnv + "" ))"";

    }";

            })) + @"
    return """";
}

std::string DerivedSpace::toString() const {
    std::string retval = """";
    bool found = false; if (found) {}
    
    int id = GLOBAL_IDS.count(const_cast<DerivedSpace*>(this)) ? GLOBAL_IDS[const_cast<DerivedSpace*>(this)] : GLOBAL_IDS[const_cast<DerivedSpace*>(this)] = (GLOBAL_INDEX += 2); 
    
" + string.Join("", ParsePeirce.Instance.Spaces.Where(sp_ => sp_.IsDerived).Select(sp_ => {
                    return "\n\tif(auto dc = dynamic_cast<domain::" + sp_.Name + @"*>(s_)){
            found = true;
            auto currentEnv = getEnvName();
            //retval += ""def "" + dc->getName() + ""var : lang." + sp_.Prefix + @".var := lang." + sp_.Prefix + @".var.mk "" + std::to_string(id) + """" + ""\n"";
            //retval += " + (sp_.DimensionType == Space.DimensionType_.ANY ? @"""\ndef "" + dc->getName() + ""sp := lang." + sp_.Prefix + @".expr.lit (" + sp_.Prefix + @".mk "" + std::to_string(id) + "" "" + dc->getBase1()->getName() + "" "" + dc->getBase2()->getName() +  "")""; " :
                             @"""\ndef "" + dc->getName() + ""sp := lang." + sp_.Prefix + @".expr.lit (" + sp_.Prefix + @".mk "" + std::to_string(id) + "")""; ") + @"
            retval += ""\ndef "" + dc->getName() + "" := cmd." + sp_.Prefix + @"FrameAssmt \n\t\t(lang." + sp_.Prefix + @".var.mk "" + std::to_string(id) + "") \n\t\t(lang." + sp_.Prefix + @".expr.lit (" + sp_.Prefix + @".mk "" + std::to_string(id-1) + "" \n\t\t\t"" + this->base_1->getEvalExpr() + "" \n\t\t\t"" + this->base_2->getEvalExpr() +  ""))\n"";
            retval += ""\n def "" + currentEnv + "" := cmdEval "" + dc->getName() + "" "" + getLastEnv();
    }";

              
            })) + @"

    if(!found){
        //retval = ""--Unknown space type - Translation Failed!"";
    }

    return retval;


};

std::string DerivedSpace::toDefString() const {
    return this->toString();
};

std::string MeasurementSystem::toString() const {
    std::string retval = """";
    
    int id = GLOBAL_IDS.count(const_cast<MeasurementSystem*>(this)) ? GLOBAL_IDS[const_cast<MeasurementSystem*>(this)] : GLOBAL_IDS[const_cast<MeasurementSystem*>(this)] = (GLOBAL_INDEX += 2); 
    
    if(((domain::SIMeasurementSystem*)this->ms_)){
        //retval += ""def "" + this->ms_->getName() + "" := cmd.measurementSystemAssmt (⟨⟨"" + std::to_string(id) + ""⟩⟩) (lang.measurementSystem.measureExpr.lit measurementSystem.si_measurement_system)"";
        //retval += ""\ndef "" + getEnvName() + "" := cmdEval "" + this->ms_->getName() + "" "" + getLastEnv();
        retval += ""\tlet var := (⟨⟨"" + std::to_string(id) + ""⟩⟩ : lang.measurementSystem.measureVar) in\n\tlet measurementSystemLiteral := (lang.measurementSystem.measureExpr.lit measurementSystem.si_measurement_system) in\n\tvar=measurementSystemLiteral"";
    }
    else if((domain::ImperialMeasurementSystem*)this->ms_){
        //retval += ""def "" + this->ms_->getName() + "" :=  cmd.measurementSystemAssmt (⟨⟨"" + std::to_string(id) + ""⟩⟩) (lang.measurementSystem.measureExpr.lit measurementSystem.imperial_measurement_system)"";
        //retval += ""\ndef "" + getEnvName() + "" := cmdEval "" + this->ms_->getName() + "" "" + getLastEnv();
        retval += ""\tlet var := (⟨⟨"" + std::to_string(id) + ""⟩⟩ : lang.measurementSystem.measureVar) in\n\tlet measurementSystemLiteral := (lang.measurementSystem.measureExpr.lit measurementSystem.imperial_measurement_system) in\n\tvar=measurementSystemLiteral"";

    }
        return retval;


};

std::string MeasurementSystem::toDefString() const {
    std::string retval = """";
    
    int id = GLOBAL_IDS.count(const_cast<MeasurementSystem*>(this)) ? GLOBAL_IDS[const_cast<MeasurementSystem*>(this)] : GLOBAL_IDS[const_cast<MeasurementSystem*>(this)] = (GLOBAL_INDEX += 2); 

    retval += ""def "" + this->ms_->getName() + "" := "" + this->toString();
    retval += ""\ndef "" + getEnvName() + "" := cmdEval "" + this->ms_->getName() + "" "" + getLastEnv();
    return retval;
};

std::string AxisOrientation::toString() const {
    std::string retval = """";
    
    int id = GLOBAL_IDS.count(const_cast<AxisOrientation*>(this)) ? GLOBAL_IDS[const_cast<AxisOrientation*>(this)] : GLOBAL_IDS[const_cast<AxisOrientation*>(this)] = (GLOBAL_INDEX += 2); 
    
    if(((domain::NWUOrientation*)this->ax_)){
        //retval += ""def "" + this->ax_->getName() + "" := cmd.axisOrientationAssmt (⟨⟨"" + std::to_string(id) + ""⟩⟩) (lang.axisOrientation.orientationExpr.lit orientation.NWU)"";
        //retval += ""\n def "" + getEnvName() + "" := cmdEval "" + this->ax_->getName() + "" "" + getLastEnv();
        retval += ""\n\tlet var := (⟨⟨"" + std::to_string(id) + ""⟩⟩ : lang.axisOrientation.orientationVar) in\n\tlet axisOrientationLiteral := (lang.axisOrientation.orientationExpr.lit orientation.NWU) in\n\tvar=axisOrientationLiteral"";
    }
    else if((domain::NEDOrientation*)this->ax_){
        //retval += ""def "" + this->ax_->getName() + "" := cmd.axisOrientationAssmt (⟨⟨"" + std::to_string(id) + ""⟩⟩) (lang.axisOrientation.orientationExpr.lit orientation.NED)"";
        //retval += ""\n def "" + getEnvName() + "" := cmdEval "" + this->ax_->getName() + "" "" + getLastEnv();
        retval += ""\n\tlet var := (⟨⟨"" + std::to_string(id) + ""⟩⟩ : lang.axisOrientation.orientationVar) in\n\tlet axisOrientationLiteral := (lang.axisOrientation.orientationExpr.lit orientation.NED) in\n\tvar=axisOrientationLiteral"";

    }
    else if((domain::ENUOrientation*)this->ax_){
        //retval += ""def "" + this->ax_->getName() + "" := cmd.axisOrientationAssmt (⟨⟨"" + std::to_string(id) + ""⟩⟩) (lang.axisOrientation.orientationExpr.lit orientation.ENU)"";
        //retval += ""\n def "" + getEnvName() + "" := cmdEval "" + this->ax_->getName() + "" "" + getLastEnv();
        retval += ""\n\tlet var := (⟨⟨"" + std::to_string(id) + ""⟩⟩ : lang.axisOrientation.orientationVar) in\n\tlet axisOrientationLiteral := (lang.axisOrientation.orientationExpr.lit orientation.ENU) in\n\tvar=axisOrientationLiteral"";

    }
        return retval;
};

std::string AxisOrientation::toDefString() const {
    std::string retval = """";
    
    int id = GLOBAL_IDS.count(const_cast<AxisOrientation*>(this)) ? GLOBAL_IDS[const_cast<AxisOrientation*>(this)] : GLOBAL_IDS[const_cast<AxisOrientation*>(this)] = (GLOBAL_INDEX += 2); 
    
    retval += ""def "" + this->ax_->getName() + "" := "" + this->toString();
    retval += ""\ndef "" + getEnvName() + "" := cmdEval "" + this->ax_->getName() + "" "" + getLastEnv();
    return retval;
};


std::string Frame::toString() const {
    std::string retval = """";
    
    int id = GLOBAL_IDS.count(const_cast<Frame*>(this)) ? GLOBAL_IDS[const_cast<Frame*>(this)] : GLOBAL_IDS[const_cast<Frame*>(this)] = (GLOBAL_INDEX += 2); 
    
    int sid = GLOBAL_IDS.count(const_cast<Space*>(sp_)) ? GLOBAL_IDS[const_cast<Space*>(sp_)] : GLOBAL_IDS[const_cast<Space*>(sp_)] = (GLOBAL_INDEX += 2); 
    
    int mid = GLOBAL_IDS.count(const_cast<MeasurementSystem*>(ms_)) ? GLOBAL_IDS[const_cast<MeasurementSystem*>(ms_)] : GLOBAL_IDS[const_cast<MeasurementSystem*>(ms_)] = (GLOBAL_INDEX += 2); 
        
    int aid = GLOBAL_IDS.count(const_cast<AxisOrientation*>(ax_)) ? GLOBAL_IDS[const_cast<AxisOrientation*>(ax_)] : GLOBAL_IDS[const_cast<AxisOrientation*>(ax_)] = (GLOBAL_INDEX += 2); 
    
    bool found = false; if (found) {}
    //bool isStandard = this->f_->getName() == ""Standard"";
    //if(!isStandard)
    //    return retval;

    if(auto af = dynamic_cast<domain::AliasedFrame*>(f_)){

" + string.Join("", ParsePeirce.Instance.Spaces.Select(sp_ => {

                         //   var hasName = sp_.MaskContains(Space.FieldType.Name);
                           // var hasDim = sp_.MaskContains(Space.FieldType.Dimension);
                            //PhysSpaceExpression.ClassicalTimeLiteral (ClassicalTimeSpaceExpression.ClassicalTimeLiteral
                            return "\n\tif(auto dc = dynamic_cast<domain::" + sp_.Name + @"Frame*>(f_)){
        found = true;
        auto df = dynamic_cast<domain::" + sp_.Name + @"AliasedFrame*>(f_);
        //auto env = getEnvName();
        //retval += ""\ndef "" + ((domain::AliasedFrame*)df)->getName() + "" := \n"";
        retval += ""    let sp := (" + sp_.Prefix + @"Eval (lang." + sp_.Prefix + @".spaceExpr.var ⟨⟨"" + std::to_string(sid) +""⟩⟩) "" + getLastEnv() + "") in\n"";
        retval += ""    let ms := (measurementSystemEval (lang.measurementSystem.measureExpr.var ⟨⟨"" + std::to_string(mid) + ""⟩⟩) "" + getLastEnv() + "") in\n"";
        if(auto std = dynamic_cast<domain::StandardFrame*>(af->getAliased())){
            retval += ""    let parent := (" + sp_.Prefix + @".stdFrame sp) in\n"";
        }
        else{
            auto interpFr = i2d_->getFrame(af->getAliased());
            int fid = GLOBAL_IDS.count(const_cast<Frame*>(interpFr)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr)] : GLOBAL_IDS[const_cast<Frame*>(interpFr)] = (GLOBAL_INDEX += 2); 
            retval += ""    let parent := (" + sp_.Prefix + @"FrameEval (lang." + sp_.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid) +""⟩⟩) "" + getLastEnv() + "") in\n"";
        } 
        retval += ""    let axis := (axisOrientationEval (lang.axisOrientation.orientationExpr.var ⟨⟨"" + std::to_string(aid) + ""⟩⟩) "" + getLastEnv() + "") in\n"";
        retval += ""    let var : lang." + sp_.Prefix + @".frameVar := (⟨⟨"" + std::to_string(id) + ""⟩⟩) in\n"";
        retval += ""    let frame_alias_literal := \n\t\t\tlang." + sp_.Prefix + @".frameExpr.lit (" + sp_.Prefix + @"Frame.interpret parent ms axis) in\n"";
        retval += ""    var=frame_alias_literal\n"";      


        //retval += ""\n def "" + env + "" := cmdEval "" + ((domain::AliasedFrame*)df)->getName() + "" "" + getLastEnv();
    }";
                        })) + @"
    }
    else if(auto df = dynamic_cast<domain::DerivedFrame*>(f_)){

" + string.Join("", ParsePeirce.Instance.Spaces.Select(sp_ => {


                            return "\n\tif(auto dc = dynamic_cast<domain::" + sp_.Name + @"DerivedFrame*>(f_)){
        found = true;
        //auto df = (domain::DerivedFrame*)f_;
        auto interpFr = i2d_->getFrame(dc->getParent());
        int fid = GLOBAL_IDS.count(const_cast<Frame*>(interpFr)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr)] : GLOBAL_IDS[const_cast<Frame*>(interpFr)] = (GLOBAL_INDEX += 2); 
        //auto dom_sp = ((domain::" + sp_.Name + @"Frame*)dc)->getSpace();    
        int dim = " + (sp_.DimensionType == Space.DimensionType_.Fixed ? sp_.FixedDimension : 1) +@"; 

        //retval += ""\ndef "" + ((domain::DerivedFrame*)dc)->getName() + "" := \n"";
        retval += ""    let sp := (" + sp_.Prefix + @"Eval (lang." + sp_.Prefix + @".spaceExpr.var ⟨⟨"" + std::to_string(sid) +""⟩⟩) "" + getLastEnv() + "") in\n"";
        if(auto std = dynamic_cast<domain::StandardFrame*>(dc->getParent())){
            retval += ""    let fr := (" + sp_.Prefix + @".stdFrame sp) in\n"";
        }
        else{
            retval += ""    let fr := (" + sp_.Prefix + @"FrameEval (lang." + sp_.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid) +""⟩⟩) "" + getLastEnv() + "") in\n"";
        }
        retval += ""    let ms := (measurementSystemEval (lang.measurementSystem.measureExpr.var ⟨⟨"" + std::to_string(mid) + ""⟩⟩) "" + getLastEnv() + "") in\n"";
        retval += ""    let axis := (axisOrientationEval (lang.axisOrientation.orientationExpr.var ⟨⟨"" + std::to_string(aid) + ""⟩⟩) "" + getLastEnv() + "") in\n"";
        retval += ""    let var : lang." + sp_.Prefix + @".frameVar := (⟨⟨"" + std::to_string(id) + ""⟩⟩) in\n"";
        retval += ""    let o := aff_lib.affine_coord_space.std_origin_vector ℝ "" + std::to_string(dim) + "" in\n"";
        for(auto i = 0; i < dim" + @";i++)
            retval += ""    let b"" + std::to_string(i)+"" := aff_lib.affine_coord_space.std_basis_vector ℝ "" + std::to_string(dim) + "" "" + std::to_string(i)+"" in\n"";
        //retval += ""    cmd." + sp_.Prefix + @"FrameAssmt (⟨⟨"" + std::to_string(id) + ""⟩⟩) (\n"";
        retval += ""    let derived_frame_literal := \n\t\t\tlang." + sp_.Prefix + @".frameExpr.lit (\n\t\t\t\t" + sp_.Prefix + @"Frame.build_derived_from_coords fr o "";
        for(auto i = 0; i < dim" + @";i++)
            retval += "" b"" + std::to_string(i) + "" "";
        retval += "" ms axis ) in\n"";
        retval += ""    var=derived_frame_literal \n"";
        //retval += ""        lang." + sp_.Prefix + @".frameExpr.lit (" + sp_.Prefix + @"Frame.build_derived_from_coords fr \n"";
        //retval += ""        "";
        /*retval += ""   (⟨[]"";
        for(auto i = 0; i < dim" + @";i++)
            retval += std::string(""++["") + std::to_string(0) + ""]"";
        retval += std::string(""\n\t\t,by refl⟩ : vector ℝ "") + std::to_string(dim)" + @" +  "")"";
        for(auto j = 0; j < dim" + @"; j++){
            retval += ""   (⟨[]"";
            for(auto i = 0; i < dim" + @";i++)
                retval += std::string(""++["") + std::to_string(1) + ""]"";
            retval += std::string(""\n\t\t,by refl⟩ : vector ℝ "") + std::to_string(dim)" + @" +  "")"";
        }*/
        //retval += ""    ms axis   ))\n"";
        //retval += ""\n def "" + env + "" := cmdEval "" + ((domain::AliasedFrame*)df)->getName() + "" "" + getLastEnv();
    }";
                        })) + @"
    }

    if(!found){
        //retval = ""--Unknown Frame type - Translation Failed!"";
    }

    return retval;

};

std::string Frame::toDefString() const {
    auto found = false;
    std::string retval = """";
    
    int id = GLOBAL_IDS.count(const_cast<Frame*>(this)) ? GLOBAL_IDS[const_cast<Frame*>(this)] : GLOBAL_IDS[const_cast<Frame*>(this)] = (GLOBAL_INDEX += 2); 
    
    int sid = GLOBAL_IDS.count(const_cast<Space*>(sp_)) ? GLOBAL_IDS[const_cast<Space*>(sp_)] : GLOBAL_IDS[const_cast<Space*>(sp_)] = (GLOBAL_INDEX += 2); 
    
    int mid = GLOBAL_IDS.count(const_cast<MeasurementSystem*>(ms_)) ? GLOBAL_IDS[const_cast<MeasurementSystem*>(ms_)] : GLOBAL_IDS[const_cast<MeasurementSystem*>(ms_)] = (GLOBAL_INDEX += 2); 
        
    int aid = GLOBAL_IDS.count(const_cast<AxisOrientation*>(ax_)) ? GLOBAL_IDS[const_cast<AxisOrientation*>(ax_)] : GLOBAL_IDS[const_cast<AxisOrientation*>(ax_)] = (GLOBAL_INDEX += 2); 
    if(auto sf = dynamic_cast<domain::StandardFrame*>(f_)){
        return retval;
    }
    else if(auto af = dynamic_cast<domain::AliasedFrame*>(f_)){
        found = true;
        auto env = getEnvName();
        retval += ""\ndef "" + (af)->getName() + "" := \n"" + this->toString();
        retval += ""\n def "" + env + "" := cmdEval "" + ((domain::AliasedFrame*)af)->getName() + "" "" + getLastEnv();
        
    }
    else if(auto df = dynamic_cast<domain::DerivedFrame*>(f_)){
        found = true;
        auto env = getEnvName();
        retval += ""\ndef "" + (df)->getName() + "" := \n"" + this->toString();
        retval += ""\n def "" + env + "" := cmdEval "" + ((domain::AliasedFrame*)df)->getName() + "" "" + getLastEnv();
        
    }

    return retval;
}

";

            var file = header;


            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                if (prod.ProductionType != Grammar.ProductionType.Single && prod.ProductionType != Grammar.ProductionType.CaptureSingle)
                {
                    var prodcons = "\n" +
    @"" + prod.Name + @"::" + prod.Name + @"(coords::" + prod.Name + @"* c, domain::DomainObject* d) : " 
        + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Interp") + @"(c,d) {}
                    ";
                    file += prodcons;

                    var prodstr = @"
std::string " + prod.Name + @"::toString() const {
    std::string retval = """";
    bool found = false; if (found) {}
    
    retval = ""Calling toString on a production, rather than a case."";
    
    
    return retval;
}

std::string " + prod.Name + @"::toDefString() const {
    std::string retval = """";
    bool found = false; if (found) {}
    
    retval = ""Calling toString on a production, rather than a case."";
    
    
    return retval;
}
                ";
                    file += prodstr;
                }

                if(prod.ProductionType == Grammar.ProductionType.Single || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                {
                    var i = 0;
                   // var j = 0;
                    var k = 0;

                    var casecons =
"\n" + prod.Name + @"::" + prod.Name + @"(coords::" + prod.Name + @"* c, domain::DomainObject* d" 
+ (prod.Cases[0].Productions.Count > 0 ? "," + string.Join(",", prod.Cases[0].Productions.Select(p_ => "interp::" + p_.Name + " * operand" + ++i)) : "") 
+ @" ) : " + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Interp") + @"(c,d)
   " + (prod.Cases[0].Productions.Count > 0 ? "," + string.Join(",", prod.Cases[0].Productions.Select(p_ => "operand_" + ++k + "(operand" + k + ")")) : "")
   + @" {}
";
                    
                    var prodtostr = @"
std::string " + prod.Name + @"::toString() const {
                        std::string retval = """";
                        bool found = false; if (found) {}

                ";
                    switch(prod.InterpType_)
                    {
                        case Grammar.Production.InterpType.Decl:
                            {
                                prodtostr +=
                                string.Join("",
                              ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                              ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                              {
                                  //  var spInstances = ParsePeirce.Instance.SpaceInstances;

                                  var retval = @"
    if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
        (prod.HasValueContainer() ?
            "<" + prod.GetPriorityValueContainer().ValueType + "," +
                prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>") + @"*>(this->operand_1->dom_)){
        found = true;
        retval += ""def "" + this->coords_->toString() + "" := cmd." + sppair.Item1.Prefix + sppair.Item2.Name + @"Assmt ("" + this->operand_1->toString() + "") ("" + this->operand_2->toString() +"")\n"";
            
    }";
                                  return retval;
                              }) : new List<string>()) + @"
    if (auto cont = dynamic_cast<domain::DomainContainer*>(this->operand_1->dom_)){
        if(cont->hasValue()){
                        " + (
                            string.Join("",
                        ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                        ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                        {
                            var retval_ = @"
            if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                (prod.HasValueContainer() ?
                                    "<" + prod.GetPriorityValueContainer().ValueType + "," 
                                        + prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>") 
                                        + @"*>(cont->getValue())){
                retval += ""def "" + this->coords_->toString() + "" := cmd." + sppair.Item1.Prefix + sppair.Item2.Name + @"Assmt ("" + this->operand_1->toString() + "") ("" + this->operand_2->toString() +"")\n"";
                
                retval += ""def "" + env + "" := cmdEval "" + this->coords_->toString() + "" "" + getLastEnv();
            }";
                            return retval_;
                        }) : new List<string>())) + @"
        }
    }";
                              break;
                            }
                        case Grammar.Production.InterpType.Expr:
                            {
                                prodtostr +=
                                string.Join("",
                              ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                              ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                              {
                                  //  var spInstances = ParsePeirce.Instance.SpaceInstances;

                                  var retval = @"
    if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
        (prod.HasValueContainer() ?
            "<" + prod.GetPriorityValueContainer().ValueType + "," +
                prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>") + @"*>(this->operand_1->dom_)){
        found = true;
        //auto env = getEnvName();
        int id = GLOBAL_IDS.count(const_cast< " + prod.Name + @"*>(this)) ? GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] : GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] = (GLOBAL_INDEX += 2); 
        auto interpSp = i2d_->getSpace(dc->getSpace());
        int sid = GLOBAL_IDS.count(const_cast<Space*>(interpSp)) ? GLOBAL_IDS[const_cast<Space*>(interpSp)] : GLOBAL_IDS[const_cast<Space*>(interpSp)] = (GLOBAL_INDEX += 2); 
        " + (sppair.Item2.HasFrame ? "auto interpFr = i2d_->getFrame(dc->getFrame());\n" +
        "int fid = GLOBAL_IDS.count(const_cast<Frame*>(interpFr)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr)] : GLOBAL_IDS[const_cast<Frame*>(interpFr)] = (GLOBAL_INDEX += 2);"
        : "") + @"
        " + (sppair.Item2.IsTransform ? "auto interpFr1 = i2d_->getFrame(dc->getFrom());\n" +
        "int fid1 = GLOBAL_IDS.count(const_cast<Frame*>(interpFr1)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr1)] : GLOBAL_IDS[const_cast<Frame*>(interpFr1)] = (GLOBAL_INDEX += 2);"
        : "") + @"
        " + (sppair.Item2.IsTransform ? "auto interpFr2 = i2d_->getFrame(dc->getTo());\n" +
        "int fid2 = GLOBAL_IDS.count(const_cast<Frame*>(interpFr2)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr2)] : GLOBAL_IDS[const_cast<Frame*>(interpFr2)] = (GLOBAL_INDEX += 2);"
        : "") + @"
        retval += "" (lang." + sppair.Item1.Prefix + @"." + sppair.Item2.Name + @"Expr.lit \n"";
        retval += ""   (" + sppair.Item1.Prefix + sppair.Item2.Name + @".build "";
        retval += std::string(""     (" + sppair.Item1.Prefix + @"Eval "") + ""(lang." + sppair.Item1.Prefix + @".spaceExpr.var ⟨⟨"" + std::to_string(sid) +""⟩⟩) ""+getLastEnv() + "")\n"";
        " + (sppair.Item2.HasFrame ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
        " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid1) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
        " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid2) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
        retval += ""   (⟨[]"";
        for(auto i = 0; i < " + prod.GetPriorityValueContainer().ValueCount + @";i++)
            retval += ""++["" + std::to_string(*dc->getValue(i)) + ""]"";
        retval += ""\n\t\t,by refl⟩ : vector ℝ " + prod.GetPriorityValueContainer().ValueCount + @" )))\n"";
        //retval += ""def "" + env + "" := cmdEval "" + this->coords_->toString() + "" "" + getLastEnv();
    }";
                                  return retval;
                              }) : new List<string>()) + @"
    if (auto cont = dynamic_cast<domain::DomainContainer*>(this->operand_1->dom_)){
        if(cont->hasValue()){
                        " + (
                            string.Join("",
                        ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                        ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                        {
                            var retval_ = @"
            if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                (prod.HasValueContainer() ?
                                    "<" + prod.GetPriorityValueContainer().ValueType + ","
                                        + prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>")
                                        + @"*>(cont->getValue())){
                found = true;
                //auto env = getEnvName();
                int id = GLOBAL_IDS.count(const_cast< " + prod.Name + @"*>(this)) ? GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] : GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] = (GLOBAL_INDEX += 2); 
                auto interpSp = i2d_->getSpace(dc->getSpace());
                int sid = GLOBAL_IDS.count(const_cast<Space*>(interpSp)) ? GLOBAL_IDS[const_cast<Space*>(interpSp)] : GLOBAL_IDS[const_cast<Space*>(interpSp)] = (GLOBAL_INDEX += 2); 
                " + (sppair.Item2.HasFrame ? "\t\t\t\tauto interpFr = i2d_->getFrame(dc->getFrame());\n\t\t\t\t" +
                "int fid = GLOBAL_IDS.count(const_cast<Frame*>(interpFr)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr)] : GLOBAL_IDS[const_cast<Frame*>(interpFr)] = (GLOBAL_INDEX += 2);"
                : "") + @"
                " + (sppair.Item2.IsTransform ? "auto interpFr1 = i2d_->getFrame(dc->getFrom());\n" +
                "int fid1 = GLOBAL_IDS.count(const_cast<Frame*>(interpFr1)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr1)] : GLOBAL_IDS[const_cast<Frame*>(interpFr1)] = (GLOBAL_INDEX += 2);"
                : "") + @"
                " + (sppair.Item2.IsTransform ? "auto interpFr2 = i2d_->getFrame(dc->getTo());\n" +
                "int fid2 = GLOBAL_IDS.count(const_cast<Frame*>(interpFr2)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr2)] : GLOBAL_IDS[const_cast<Frame*>(interpFr2)] = (GLOBAL_INDEX += 2);"
                : "") + @"
                retval += "" (lang." + sppair.Item1.Prefix + @"." + sppair.Item2.Name + @"Expr.lit \n"";
                retval += ""   (" + sppair.Item1.Prefix + sppair.Item2.Name + @".build "";
                retval += std::string(""     (" + sppair.Item1.Prefix + @"Eval "") + ""(lang." + sppair.Item1.Prefix + @".spaceExpr.var ⟨⟨"" + std::to_string(sid) +""⟩⟩) ""+getLastEnv() + "")\n"";
                " + (sppair.Item2.HasFrame ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
                " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid1) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
                " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid2) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
                retval += ""   (⟨[]"";
                for(auto i = 0; i < " + prod.GetPriorityValueContainer().ValueCount + @";i++)
                    retval += ""++["" + std::to_string(*dc->getValue(i)) + ""]"";
                retval += ""\n\t\t,by refl⟩ : vector ℝ " + prod.GetPriorityValueContainer().ValueCount + @" )))\n"";
            }";
                            return retval_;
                        }) : new List<string>()));
                                break;
                            }
                        case Grammar.Production.InterpType.Var:
                            {
                                prodtostr += @"
                                int id = GLOBAL_IDS.count(const_cast < " + prod.Name + @" *> (this)) ? GLOBAL_IDS[const_cast < " + prod.Name + @" *> (this)] : GLOBAL_IDS[const_cast < " + prod.Name + @" *> (this)] = (GLOBAL_INDEX += 2);
                                retval += ""⟨⟨"" + std::to_string(id) + ""⟩⟩"";
    
                                ";                            
    
                                break;
                            }
                        case Grammar.Production.InterpType.While:
                            {
                                prodtostr += @"
                retval += this->operand_2->toString() + ""\n"";
                //auto env = getEnvName();
                retval += ""def "" + this->coords_->toString() + "" := while (bExpr.blit tt),"" + this->operand_2->coords_->toString();
            
                //retval += ""def "" + env + "" := cmdEval "" + this->coords_->toString() + "" "" + getLastEnv();
                                ";
                                break;
                            }
                        case Grammar.Production.InterpType.TryCatch:
                            {
                                prodtostr += @"
                retval += this->operand_1->toString() + ""\n"";
                //auto env = getEnvName();
                retval += ""def "" + this->coords_->toString() + "" := try "" + this->operand_1->coords_->toString() + "", catch skip"";
            
                //retval += ""def "" + env + "" := cmdEval "" + this->coords_->toString() + "" "" + getLastEnv();
                                ";
                                break;
                            }
                        case Grammar.Production.InterpType.Func:
                            {
                                prodtostr += @"
                //auto env = getEnvName();
                //retval += ""def "" + this->coords_->toString() + "" := while (bExpr.blit tt),"" + this->operand_2->toString();
            
                //retval += ""def "" + env + "" := cmdEval "" + this->coords_->toString() + "" "" + getLastEnv();
                if(auto dc = dynamic_cast<interp::COMPOUND_STMT*>(this->operand_1)){
                    
                }
                else{
                }
                    
                                ";
                                break;
                            }
                        case Grammar.Production.InterpType.Unk:
                            {
                                break;
                            }
                    }

                    prodtostr += @"

    if (!found){
                        //ret = """";
                        " + @"
    }
    //std::replace(retval.begin(), retval.end(), '_', '.');
    std::size_t index;
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

std::string " + prod.Name + @"::toDefString() const {
    return this->toString();
};


                ";
                    if (prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                    {
                        var prodalgstr = @"
std::string " + prod.Name + @"::toAlgebraString() const {
                        std::string retval = """";
                        bool found = false; if (found) {}

                        //  ret += ""("";
                        //ret += ""def var_"" + std::to_string(++GLOBAL_INDEX) + "":= 1"";" +
                           string.Join("",
                           ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                           ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                           {
                           //  var spInstances = ParsePeirce.Instance.SpaceInstances;

                           var retval = @"
    if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                                                       (prod.HasValueContainer() ?
                                                                           "<" + prod.GetPriorityValueContainer().ValueType + "," + prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>") + @"*>(this->dom_)){
        found = true;
        auto env = getLastEnv();
        //int id = GLOBAL_IDS.count(const_cast< " + prod.Name + @"*>(this)) ? GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] : GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] = (GLOBAL_INDEX += 2); 
        return ""(" + sppair.Item1.Prefix + sppair.Item2.Name + @".algebra "" + this->toEvalString() + "")"";
        
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
            if(auto dc = dynamic_cast<domain::" + sppair_.Item1.Name + sppair_.Item2.Name + @"" +
                                                   (prod.HasValueContainer() ?
                                                       "<" + prod.GetPriorityValueContainer().ValueType + "," + prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>") + @"*>(cont->getValue())){
                found = true;
        auto env = getLastEnv();
        //int id = GLOBAL_IDS.count(const_cast< " + prod.Name + @"*>(this)) ? GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] : GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] = (GLOBAL_INDEX += 2); 
        return ""(" + sppair_.Item1.Prefix + sppair_.Item2.Name + @".algebra "" + this->toEvalString() + "")"";
        
            }";
                                               return retval_;
                                           }
                                           ) : new List<string>())) +
                                               @"
        }
    }

    if(!found){
        //ret = """";
        " + @"
    }
    //std::replace(retval.begin(), retval.end(), '_', '.');
    std::size_t index;
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

                        var prodevalstr = @"
std::string " + prod.Name + @"::toEvalString() const {
                        std::string retval = """";
                        bool found = false; if (found) {}

                        //  ret += ""("";
                        //ret += ""def var_"" + std::to_string(++GLOBAL_INDEX) + "":= 1"";" +
                           string.Join("",
                           ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                           ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                           {
                           //  var spInstances = ParsePeirce.Instance.SpaceInstances;

                           var retval = @"
    if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                                                   (prod.HasValueContainer() ?
                                                                       "<" + prod.GetPriorityValueContainer().ValueType + "," + prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>") + @"*>(this->dom_)){
        found = true;
        auto env = getLastEnv();
        int id = GLOBAL_IDS.count(const_cast< " + prod.Name + @"*>(this)) ? GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] : GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] = (GLOBAL_INDEX += 2); 
        return std::string(""(" + sppair.Item1.Prefix + sppair.Item2.Name + @"Eval (lang." + sppair.Item1.Prefix + "."
        + sppair.Item2.Name + @"Expr.var "") + ""⟨⟨"" + std::to_string(id) +""⟩⟩) "" + getLastEnv() + "")"";
        
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
            if(auto dc = dynamic_cast<domain::" + sppair_.Item1.Name + sppair_.Item2.Name + @"" +
                                                   (prod.HasValueContainer() ?
                                                       "<" + prod.GetPriorityValueContainer().ValueType + "," + prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>") + @"*>(cont->getValue())){
                found = true;
        auto env = getLastEnv();
        int id = GLOBAL_IDS.count(const_cast< " + prod.Name + @"*>(this)) ? GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] : GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] = (GLOBAL_INDEX += 2); 
        return std::string(""(" + sppair_.Item1.Prefix + sppair_.Item2.Name + @"Eval (lang." + sppair_.Item1.Prefix + "."
                + sppair_.Item2.Name + @"Expr.var "") + ""⟨⟨"" + std::to_string(id) +""⟩⟩) "" + getLastEnv() + "")"";
        
            }";
                                               return retval_;
                                           }
                                           ) : new List<string>())) +
                                               @"
        }
    }

    if(!found){
        //ret = """";
        " + @"
    }
    //std::replace(retval.begin(), retval.end(), '_', '.');
    std::size_t index;
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
                        file += "\n" + prodalgstr;
                        file += "\n" + prodevalstr;
                    }
                    file += "\n" + casecons;
                    file += prodtostr + "\n";
                }
                if (prod.ProductionType == Grammar.ProductionType.CaptureSingle || prod.ProductionType == Grammar.ProductionType.Single)
                    continue;

                foreach (var pcase in prod.Cases)
                {
                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                            continue;

                    switch(pcase.CaseType)
                    {
                        case Grammar.CaseType.ArrayOp:
                            {
                                //int i = 0, j = 0, k = 0;
                                var cons = "\n" + pcase.Name + "::" + pcase.Name + "(coords::" + pcase.Name + @"* c, domain::DomainObject* d, std::vector<interp::" + pcase.Productions[0].Name + @"*> operands)  :" + prod.Name + "(c, d)" + @" {
    for(auto& op : operands){
        this->operands_.push_back(op);
    }

};";

                                file += cons;
                              //  i = 0; j = 0;
                                //foreach (var casep in pcase.Productions)
                                //{
                                //   var opgetter = "\n" + "coords::" + casep.Name + "* " + pcase.Name + "::getOperand" + ++i + "() { return this->operand" + i + ";}";
                                //  file += opgetter;
                                //}
                                file += "\nstd::string " + pcase.Name + @"::toString() const{ 
    std::string retval = """";
    string cmdval = ""\n\tcmd.skip"";//""[]"";
    for(auto op: this->operands_){ 
        retval += ""\n"" + op->toDefString() + ""\n"";
        //cmdval = op->coords_->toString() + ""::"" + cmdval;
        cmdval = cmdval + "";\n\t"" + op->coords_->toString();
    }
    //cmdval = ""("" + cmdval + "")"";

    cmdval += """"; " + (
        pcase.Command is Grammar.Command && prod.Command is Grammar.Command ? @"
    //cmdval = ""\ndef "" + this->coords_->toString() + """ + pcase.Command.NameSuffix + "" + @" : " + pcase.Command.Production + @" := " + pcase.Command.Production + '.' + pcase.Command.Case + @" "" + cmdval;
    
" + @"
    //cmdval += ""\ndef "" + this->coords_->toString() + """ + "" + @" : " + prod.Command.Production + @" := " + prod.Command.Production + '.' + prod.Command.Case + @" "" + this->coords_->toString() + """ + pcase.Command.NameSuffix + "\"" + @";
    cmdval = ""\ndef "" + this->coords_->toString() + """ + pcase.Command.NameSuffix + "" + @" : " + pcase.Command.Production + @" := "" + cmdval;
"
        :
    ((
        pcase.Command is Grammar.Command ? @"
    //cmdval = ""\ndef "" + this->coords_->toString() + """ + "" + @" : " + pcase.Command.Production + @" := " + pcase.Command.Production + '.' + pcase.Command.Case + @" "" + cmdval;
    cmdval = ""\ndef "" + this->coords_->toString() + """ + @" : " + pcase.Command.Production + @" := "" + cmdval;

" : ""
    
    ) + (
        prod.Command is Grammar.Command ? @"
    //cmdval = ""\ndef "" + this->coords_->toString() + """ + "" + @" : " + prod.Command.Production + @" := " + prod.Command.Production + '.' + prod.Command.Case + @" ("" + cmdval + "")"";
    cmdval = ""\ndef "" + this->coords_->toString() + """ + @" : " + prod.Command.Production + @" := "" + cmdval;


" : ""
    ))) + (
        (pcase.Command is Grammar.Command || prod.Command is Grammar.Command) ? @"
    retval += ""\n"" + cmdval + ""\n"";
    " : "") + @"

    ////std::replace(retval.begin(), retval.end(), '_', '.');
    std::size_t index;
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

std::string " + pcase.Name + @"::toDefString() const{ 
    return this->toString();
}
";
                                file += "\nstd::string " + pcase.Name + @"::toStringLinked(
        std::vector<interp::Space*> links, 
        std::vector<std::string> names, 
        std::vector<interp::MeasurementSystem*> msystems,
        std::vector<std::string> msnames,
        std::vector<interp::AxisOrientation*> axes,
        std::vector<std::string> axisnames,
        std::vector<interp::Frame*> framelinks, 
        std::vector<string> framenames, 
        interp2domain::InterpToDomain* i2d,
        bool before) { 
    //std::string toStr = this->toString();
    i2d_ = i2d;
    std::string retval = """";
    //string cmdvalstart = ""::[]"";
    string cmdval = ""\n\tcmd.skip"";
    int i = 0;

    std::string cmdwrapper = """ + pcase.Command.Production + '.' + pcase.Command.Case + @""";

    std::string concat = "";\n\t"";

    //int count = this->operands_.size() + links.size() + framelinks.size();
    int actualcount = 0;
    if(true)
    {
        bool prev;

        for(auto op: links){
            if(prev){
                retval += ""\n"" + op->toDefString() + ""\n"";
                //cmdval = ""("" + cmdwrapper + "" "" + names[i++] + "" "" + cmdval + "")"";
                cmdval = cmdval + concat + names[i++];
            }
            else{
                retval += ""\n"" + op->toDefString() + ""\n"";
                //cmdval = names[i++];
                cmdval = cmdval + concat + names[i++];
                prev = true;
                
            }
            actualcount++;
        }
        i = 0;
        for(auto& ms : msystems){
            retval += ""\n"" + ms->toDefString() + ""\n"";
            //cmdval = ""("" + cmdwrapper + "" "" + msnames[i++] + "" "" + cmdval + "")"";
            cmdval = cmdval + concat + msnames[i++];
            actualcount++;
        }
        i = 0;
        for(auto& ax : axes){
            retval += ""\n"" + ax->toDefString() + ""\n"";
            //cmdval = ""("" + cmdwrapper + "" "" + axisnames[i++] + "" "" + cmdval + "")"";
            cmdval = cmdval + concat + axisnames[i++];
            actualcount++;
        }

        i = 0;
        for(auto op: framelinks){
            if(prev){ 
                if(auto df = dynamic_cast<domain::" + @"AliasedFrame*>(op->f_)){
                retval += ""\n"" + op->toDefString() + ""\n"";
                //cmdval = ""("" + cmdwrapper + "" "" + df->getName() + "" "" + cmdval + "")"";
                cmdval = cmdval + concat + df->getName();

                i++;
                }
                else if(auto df = dynamic_cast<domain::" + @"DerivedFrame*>(op->f_)){
                retval += ""\n"" + op->toDefString() + ""\n"";
                
                //cmdval = ""("" + cmdwrapper + "" "" + df->getName() + "" "" + cmdval + "")"";
                cmdval = cmdval + concat + df->getName();
                i++;
                }
            }
            else {
                if(auto df = dynamic_cast<domain::" + @"AliasedFrame*>(op->f_)){
                retval += ""\n"" + op->toDefString() + ""\n"";
                cmdval = cmdval + concat + framenames[i++];
                prev = true;
                }
                else if(auto df = dynamic_cast<domain::" + @"DerivedFrame*>(op->f_)){
                retval += ""\n"" + op->toDefString() + ""\n"";
                cmdval = cmdval + concat + framenames[i++];
                prev = true;
                }
            }
        }

        //bool start = true;
        for(auto op: this->operands_){ 
            if(prev and op->coords_->codegen()){
                retval += ""\n"" + op->toDefString() + ""\n"";
                //cmdval = ""("" + cmdwrapper + "" "" + op->coords_->toString() + "" "" + cmdval + "")"";
                cmdval = cmdval + concat + op->coords_->toString();
                actualcount++;
            }
            else if (op->coords_->codegen()){
                retval += ""\n"" + op->toDefString() + ""\n"";
                //cmdval = op->coords_->toString();
                cmdval = cmdval + concat + op->coords_->toString();
                prev = true;
                actualcount++;
            }
            //retval += ""\n"" + op->toString() + ""\n"";
            //cmdval = cmdval + (!start?""::"":"""") + op->coords_->toString();
            //start = false;
        }
        
    }

    //cmdval += """"; " + (
        pcase.Command is Grammar.Command && prod.Command is Grammar.Command ? @"
    //cmdval = ""\ndef "" + this->coords_->toString() + """ + pcase.Command.NameSuffix + "" + @" : " + pcase.Command.Production + @" := " + pcase.Command.Production + '.' + pcase.Command.Case + @" ("" + cmdval + cmdvalstart + "")"";

" + @"
    //cmdval += ""\ndef "" + this->coords_->toString() + """ + "" + @" : " + prod.Command.Production + @" := " + prod.Command.Production + '.' + prod.Command.Case + @" "" + this->coords_->toString() + """ + pcase.Command.NameSuffix + "\"" + @";

"
        :
    ((
        pcase.Command is Grammar.Command ? @"
    //cmdval = ""\ndef "" + this->coords_->toString() + """ + "" + @" : " + pcase.Command.Production + @" := " + pcase.Command.Production + '.' + pcase.Command.Case + @" "" + cmdval;

" : ""

    ) + (
        prod.Command is Grammar.Command ? @"
    //cmdval = ""\ndef "" + this->coords_->toString() + """ + "" + @" : " + prod.Command.Production + @" := " + prod.Command.Production + '.' + prod.Command.Case + @" "" + cmdval;


" : ""
    ))) + (
        (pcase.Command is Grammar.Command || prod.Command is Grammar.Command) ? @"
    if(actualcount>1)
        retval += ""\ndef "" + this->coords_->toString() + """ + "" + @" : " + pcase.Command.Production + @" :="" + cmdval + ""\n"";
    " : "") + @"

    ////std::replace(retval.begin(), retval.end(), '_', '.');
    std::size_t index;
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
}

/*
std::string " + pcase.Name + @"::toDefStringLinked(
        std::vector<interp::Space*> links, 
        std::vector<std::string> names, 
        std::vector<interp::MeasurementSystem*> msystems,
        std::vector<std::string> msnames,
        std::vector<interp::AxisOrientation*> axes,
        std::vector<std::string> axisnames,
        std::vector<interp::Frame*> framelinks, 
        std::vector<string> framenames, 
        interp2domain::InterpToDomain* i2d,
        bool before) { 
    return this->
};
*/
";

                                var hm = (pcase.Command is Grammar.Command || prod.Command is Grammar.Command);
                                file += "\n\n";
                                break;
                            }
                        case Grammar.CaseType.Ident:
                            {
                                break;
                            }
                        default:
                            {
                                var i = 0;
                               // var j = 0;
                                var k = 0;
                                var l = 0;

                                var casecons =
            "\n" + pcase.Name + @"::" + pcase.Name + @"(coords::" + pcase.Name + @"* c, domain::DomainObject* d" + (pcase.Productions.Count > 0 ? "," + string.Join(",", pcase.Productions.Select(p_ => "interp::" + p_.Name + " * operand" + ++i)) : "") + @" ) : " + prod.Name + @"(c,d)
   " + (pcase.Productions.Count > 0 ? "," + string.Join(",", pcase.Productions.Select(p_ => "operand_" + ++k + "(operand" + k + ")")) : "") + @" {}
";

                                var casetostr = @"
std::string " + pcase.Name + @"::toString() const {
    bool found = false; if (found) {}
    std::string retval = """";";
                                switch (prod.InterpType_)
                                {
                                    case Grammar.Production.InterpType.Decl:
                                        {
                                            if (pcase.Productions.Count < 2)
                                                break;
                                            casetostr +=
                                            string.Join("",
                                          ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(pcase.Productions[1]) ?
                                          ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[pcase.Productions[1]].Select(sppair =>
                                          {
                                              //  var spInstances = ParsePeirce.Instance.SpaceInstances;

                                              var retval = @"
    if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                (pcase.Productions[1].HasValueContainer() ?
                                    "<" + pcase.Productions[1].GetPriorityValueContainer().ValueType + "," +
                                        pcase.Productions[1].GetPriorityValueContainer().ValueCount + ">" : "<float,1>") + @"*>(this->operand_1->dom_)){
        found = true;
        //auto env = getEnvName();
        //retval += ""def "" + this->coords_->toString() + "" := cmd." + sppair.Item1.Prefix + sppair.Item2.Name + @"Assmt ("" + this->operand_1->toString() + "") ("" + this->operand_2->toString() +"")\n"";
        
        retval += std::string(""("") + this->operand_1->toString() + "" : lang." + sppair.Item1.Prefix + @"." + sppair.Item2.Name + @"Var)=("" + this->operand_2->toString() +"")\n"";
            

        //retval += ""def "" + env + "" := cmdEval "" + this->coords_->toString() + "" "" + getLastEnv();
    }";
                                              return retval;
                                          }) : new List<string>()) + @"
    if (auto cont = dynamic_cast<domain::DomainContainer*>(this->operand_1->dom_)){
        if(cont->hasValue()){
                        " + (
                                        string.Join("",
                                    ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(pcase.Productions[1]) ?
                                    ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[pcase.Productions[1]].Select(sppair =>
                                    {
                                        var retval_ = @"
            if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                            (pcase.Productions[1].HasValueContainer() ?
                                                "<" + pcase.Productions[1].GetPriorityValueContainer().ValueType + ","
                                                    + pcase.Productions[1].GetPriorityValueContainer().ValueCount + ">" : "<float,1>")
                                                    + @"*>(cont->getValue())){
                found = true;
                //auto env = getEnvName();
                //retval += ""def "" + this->coords_->toString() + "" := cmd." + sppair.Item1.Prefix + sppair.Item2.Name + @"Assmt ("" + this->operand_1->toString() + "") ("" + this->operand_2->toString() +"")\n"";
                
                //retval += ""def "" + this->coords_->toString() + "" := "" + this->operand_1->toString() + ""="" + this->operand_2->toString() + ""\n"";
                retval += std::string(""("") + this->operand_1->toString() + "" : lang." + sppair.Item1.Prefix + @"." + sppair.Item2.Name + @"Var)="" + this->operand_2->toString() + ""\n"";
                //retval += ""def "" + env + "" := cmdEval "" + this->coords_->toString() + "" "" + getLastEnv();
            }";
                                        return retval_;
                                    }) : new List<string>())) + @"
        }
    }
    if(!found){
        //retval += ""def "" + this->coords_->toString() + "" := "" + this->operand_1->toString() + ""=_\n"";
        retval += this->operand_1->toString() + ""=_\n"";
    }
    ";
                                            break;
                                        }
                                    case Grammar.Production.InterpType.Expr:
                                        {
                                            if (pcase.Interp_.PrintType_ == Grammar.Case.Interp.PrintType.Var)
                                            {
                                                casetostr += string.Join("",
                                              ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                                              ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                                              {
                                                  //  var spInstances = ParsePeirce.Instance.SpaceInstances;

                                                  var retval = (@"
    if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                    (prod.HasValueContainer() ?
                                        "<" + prod.GetPriorityValueContainer().ValueType + "," +
                                            prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>") + @"*>(this->dom_)){
        found = true;
        retval += ""( lang." + sppair.Item1.Prefix + "." + sppair.Item2.Name + @"Expr.var "";
        retval += this->operand_1->toString();
        retval += "")"";
    }");
                                                  return retval;
                                              }) : new List<string>()) + @"
    if (auto cont = dynamic_cast<domain::DomainContainer*>(this->dom_)){
        if(cont->hasValue()){
                        " +
                            (
                                            string.Join("",
                                        ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                                        ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                                        {
                                            var retval_ = @"
            if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                                (prod.HasValueContainer() ?
                                                    "<" + prod.GetPriorityValueContainer().ValueType + ","
                                                        + prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>")
                                                        + @"*>(cont->getValue())){
                found = true;
                retval += ""( lang." + sppair.Item1.Prefix + "." + sppair.Item2.Name + @"Expr.var "";
                retval += this->operand_1->toString();
                retval += "")"";
            }";
                                            return retval_;
                                        }) : new List<string>())) + @"
        }
    }

";
                                            }
                                            else
                                            {
                                                casetostr +=
                                                string.Join("",
                                              ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                                              ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                                              {
                                                  var retval = (@"
    if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                (prod.HasValueContainer() ?
                                    "<" + prod.GetPriorityValueContainer().ValueType + "," +
                                        prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>") + @"*>(this->dom_)){
        found = true;
        //auto env = getEnvName();
        //int id = GLOBAL_IDS.count(const_cast< " + pcase.Name + @"*>(this)) ? GLOBAL_IDS[const_cast<" + pcase.Name + @"*>(this)] : GLOBAL_IDS[const_cast<" + pcase.Name + @"*>(this)] = (GLOBAL_INDEX += 2); 
        auto interpSp = i2d_->getSpace(dc->getSpace());
        int sid = GLOBAL_IDS.count(const_cast<Space*>(interpSp)) ? GLOBAL_IDS[const_cast<Space*>(interpSp)] : GLOBAL_IDS[const_cast<Space*>(interpSp)] = (GLOBAL_INDEX += 2);
        " + (sppair.Item2.HasFrame ? "\t\tauto interpFr = i2d_->getFrame(dc->getFrame());\n" +
        "\t\tint fid = GLOBAL_IDS.count(const_cast<Frame*>(interpFr)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr)] : GLOBAL_IDS[const_cast<Frame*>(interpFr)] = (GLOBAL_INDEX += 2); "
            : "") + @"
            " + (sppair.Item2.IsTransform ? "auto interpFr1 = i2d_->getFrame(dc->getFrom());\n" +
            "int fid1 = GLOBAL_IDS.count(const_cast<Frame*>(interpFr1)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr1)] : GLOBAL_IDS[const_cast<Frame*>(interpFr1)] = (GLOBAL_INDEX += 2);"
            : "") + @"
            " + (sppair.Item2.IsTransform ? "auto interpFr2 = i2d_->getFrame(dc->getTo());\n" +
            "int fid2 = GLOBAL_IDS.count(const_cast<Frame*>(interpFr2)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr2)] : GLOBAL_IDS[const_cast<Frame*>(interpFr2)] = (GLOBAL_INDEX += 2);"
            : "") + @"
        retval += "" (lang." + sppair.Item1.Prefix + @"." + sppair.Item2.Name + @"Expr.lit \n"";" +
                    (pcase.Interp_.PrintType_ == Grammar.Case.Interp.PrintType.Unk ? @" 
        
        retval += ""(" + sppair.Item1.Prefix + sppair.Item2.Name + @".build "";
        retval += std::string(""(" + sppair.Item1.Prefix + @"Eval "") + ""(lang." + sppair.Item1.Prefix + @".spaceExpr.var ⟨⟨"" + std::to_string(sid) + ""⟩⟩) "" + getLastEnv() + "")\n"";
        " + (sppair.Item2.HasFrame ? @"retval += std::string(""(" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid) + ""⟩⟩) "" + getLastEnv() + "")\n""; " : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid1) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid2) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
        
        " + (prod.HasValueContainer() ? @"   retval += ""(⟨[]"";
        for (auto i = 0; i < " + prod.GetPriorityValueContainer().ValueCount + @"; i++)
            retval += ""++["" + std::to_string(*dc->getValue(i)) + ""]"";
        retval += ""\n\t\t,by refl⟩ : vector ℝ " + prod.GetPriorityValueContainer().ValueCount + @")" : @"retval += """) + @"))"";
        " : @"
        retval += ""(" + sppair.Item1.Prefix + sppair.Item2.Name + @".fromalgebra "";
        retval += std::string(""(" + sppair.Item1.Prefix + @"Eval "") + ""(lang." + sppair.Item1.Prefix + @".spaceExpr.var ⟨⟨"" + std::to_string(sid) + ""⟩⟩) "" + getLastEnv() + "")\n"";
        " + (sppair.Item2.HasFrame ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid1) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid2) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
        retval += std::string(""("") + this->operand_1->toAlgebraString() + """ + pcase.Interp_.Symbol + @""" + this->operand_2->toAlgebraString() + ""))"";
        ") + @"
    }");
                                                  return retval;
                                              }) : new List<string>()) + @"
    if (auto cont = dynamic_cast<domain::DomainContainer*>(this->dom_)){
        if(cont->hasValue()){
                        " +
                            (
                                            string.Join("",
                                        ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                                        ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                                        {
                                            var retval_ = @"
            if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                                (prod.HasValueContainer() ?
                                                    "<" + prod.GetPriorityValueContainer().ValueType + ","
                                                        + prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>")
                                                        + @"*>(cont->getValue())){
                found = true;
                //auto env = getEnvName();
                //int id = GLOBAL_IDS.count(const_cast< " + prod.Name + @"*>(this)) ? GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] : GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] = (GLOBAL_INDEX += 2); 
                auto interpSp = i2d_->getSpace(dc->getSpace());
                int sid = GLOBAL_IDS.count(const_cast<Space*>(interpSp)) ? GLOBAL_IDS[const_cast<Space*>(interpSp)] : GLOBAL_IDS[const_cast<Space*>(interpSp)] = (GLOBAL_INDEX += 2); 
                " + (sppair.Item2.HasFrame ? "\n\t\t\t\tauto interpFr = i2d_->getFrame(dc->getFrame());\n" +
                                "\t\t\t\tint fid = GLOBAL_IDS.count(const_cast<Frame*>(interpFr)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr)] : GLOBAL_IDS[const_cast<Frame*>(interpFr)] = (GLOBAL_INDEX += 2);"
            : "") + @"
            " + (sppair.Item2.IsTransform ? "auto interpFr1 = i2d_->getFrame(dc->getFrom());\n" +
            "int fid1 = GLOBAL_IDS.count(const_cast<Frame*>(interpFr1)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr1)] : GLOBAL_IDS[const_cast<Frame*>(interpFr1)] = (GLOBAL_INDEX += 2);"
            : "") + @"
            " + (sppair.Item2.IsTransform ? "auto interpFr2 = i2d_->getFrame(dc->getTo());\n" +
            "int fid2 = GLOBAL_IDS.count(const_cast<Frame*>(interpFr2)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr2)] : GLOBAL_IDS[const_cast<Frame*>(interpFr2)] = (GLOBAL_INDEX += 2);"
            : "") + @"
                retval += "" (lang." + sppair.Item1.Prefix + @"." + sppair.Item2.Name + @"Expr.lit \n"";" +
            (pcase.Interp_.PrintType_ == Grammar.Case.Interp.PrintType.Unk ? @"
                
        retval += ""(" + sppair.Item1.Prefix + sppair.Item2.Name + @".build "";
        retval += std::string(""(" + sppair.Item1.Prefix + @"Eval "") + ""(lang." + sppair.Item1.Prefix + @".spaceExpr.var ⟨⟨"" + std::to_string(sid) + ""⟩⟩) "" + getLastEnv() + "")\n"";
        " + (sppair.Item2.HasFrame ? @"retval += std::string(""(" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid) + ""⟩⟩) "" + getLastEnv() + "")\n""; " : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid1) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid2) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
         " + (prod.HasValueContainer() ? @"   retval += ""(⟨[]"";
        for (auto i = 0; i < " + prod.GetPriorityValueContainer().ValueCount + @"; i++)
            retval += ""++["" + std::to_string(*dc->getValue(i)) + ""]"";
        retval += ""\n\t\t,by refl⟩ : vector ℝ " + prod.GetPriorityValueContainer().ValueCount + @")" : @"retval += """) + @"))"";
        "
              : @"
        retval += ""(" + sppair.Item1.Prefix + sppair.Item2.Name + @".fromalgebra "";
        retval += std::string(""(" + sppair.Item1.Prefix + @"Eval "") + ""(lang." + sppair.Item1.Prefix + @".spaceExpr.var ⟨⟨"" + std::to_string(sid) + ""⟩⟩) "" + getLastEnv() + "")\n"";
        " + (sppair.Item2.HasFrame ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid1) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid2) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
        retval += std::string(""("") + this->operand_1->toAlgebraString() + """ + pcase.Interp_.Symbol + @""" + this->operand_2->toAlgebraString() + ""))"";
        ") + @"
            }";
                                            return retval_;
                                        }) : new List<string>())) + @"
        }
    }

";
                                            }
                                            break;
                                        }
                                    case Grammar.Production.InterpType.Var:
                                        {
                                            casetostr += @"
                                int id = GLOBAL_IDS.count(const_cast < " + pcase.Name + @" *> (this)) ? GLOBAL_IDS[const_cast < " + prod.Name + @" *> (this)] : GLOBAL_IDS[const_cast < " + prod.Name + @" *> (this)] = (GLOBAL_INDEX += 2);
                                retval += ""⟨⟨"" + std::to_string(id) + ""⟩⟩"";
    
                                ";
                                            break;
                                        }
                                    case Grammar.Production.InterpType.While:
                                        {
                                            casetostr += @"
    retval += this->operand_2->toDefString() + ""\n"";
    auto env = getEnvName();
    retval += ""def "" + this->coords_->toString() + "" := while (bExpr.BLit tt),"" + this->operand_2->coords_->toString() + ""\n"";
    //retval += ""while (bExpr.BLit tt),"" + this->operand_2->coords_->toString() + ""\n"";
    //retval += ""while (bExpr.BLit tt),"" + this->operand_2->coords_->toString() + ""\n"";
    retval += ""def "" + env + "" := cmdEval "" + this->coords_->toString() + "" "" + getLastEnv();
                                ";
                                            break;
                                        }
                                    case Grammar.Production.InterpType.TryCatch:
                                        {
                                            casetostr += @"
    retval += this->operand_1->toDefString() + ""\n"";
    auto env = getEnvName();
    retval += ""def "" + this->coords_->toString() + "" := try "" + this->operand_1->coords_->toString() + "", catch skip\n"";
    //retval += ""try "" + this->operand_1->coords_->toString() + "", skip"";
            
    retval += ""def "" + env + "" := cmdEval "" + this->coords_->toString() + "" "" + getLastEnv();
                                ";
                                            break;
                                        }
                                    case Grammar.Production.InterpType.Unk:
                                        {
                                            break;
                                        }
                                }

                                casetostr += @"
    //    }
    //}

    if(!found){" +
    (
    prod.InterpType_ == Grammar.Production.InterpType.Expr ?
    @"
    return ""_"";"
    :
    @"
    "
    ) + @"
        //retval = """";
    }
    //std::replace(retval.begin(), retval.end(), '_', '.');
    std::size_t index;
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

std::string " + pcase.Name + @"::toDefString() const {
    std::string retval = """"; " + 
    (
    prod.InterpType_ == Grammar.Production.InterpType.Expr ?
    @"
    auto env = getEnvName();
    retval += ""def "" + this->coords_->toString() + "" := expr ("" + this->toString() + "")\n"";
    retval += ""\ndef "" + env + "" := cmdEval "" + this->coords_->toString() + "" "" + getLastEnv();
    return retval;"
    :
    prod.InterpType_ == Grammar.Production.InterpType.TryCatch ? @"
    
    return this->toString();
    " :
    prod.InterpType_ == Grammar.Production.InterpType.While ? 
    @"
    return this->toString();
    "
    :
    @"
    auto env = getEnvName();
    retval += ""def "" + this->coords_->toString() + "" := "" + this->toString() + ""\n"";
    retval += ""\ndef "" + env + "" := cmdEval "" + this->coords_->toString() + "" "" + getLastEnv();
    return retval;"
    ) + @"       
}
                                ";

                                if (prod.ProductionType == Grammar.ProductionType.Capture 
                                   )// && pcase.Interp_.PrintType_ == Grammar.Case.Interp.PrintType.Child)
                                {
                                    var casealgstr = @"
std::string " + pcase.Name + @"::toAlgebraString() const {
                        std::string retval = """";
                        bool found = false; if (found) {}

                        //  ret += ""("";
                        //ret += ""def var_"" + std::to_string(++GLOBAL_INDEX) + "":= 1"";" +
                           string.Join("",
                           ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                           ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                           {
                           //  var spInstances = ParsePeirce.Instance.SpaceInstances;

                           var retval = @"
    if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                                                       (prod.HasValueContainer() ?
                                                                           "<" + prod.GetPriorityValueContainer().ValueType + "," + prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>") + @"*>(this->dom_)){
        found = true;
        auto env = getLastEnv();
        //int id = GLOBAL_IDS.count(const_cast< " + pcase.Name + @"*>(this)) ? GLOBAL_IDS[const_cast<" + pcase.Name + @"*>(this)] : GLOBAL_IDS[const_cast<" + pcase.Name + @"*>(this)] = (GLOBAL_INDEX += 2); 
        return ""(" + sppair.Item1.Prefix + sppair.Item2.Name + @".algebra "" + this->toEvalString() + "")"";
        
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
            if(auto dc = dynamic_cast<domain::" + sppair_.Item1.Name + sppair_.Item2.Name + @"" +
                                                   (prod.HasValueContainer() ?
                                                       "<" + prod.GetPriorityValueContainer().ValueType + "," + prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>") + @"*>(cont->getValue())){
                found = true;
        auto env = getLastEnv();
        //int id = GLOBAL_IDS.count(const_cast< " + pcase.Name + @"*>(this)) ? GLOBAL_IDS[const_cast<" + pcase.Name + @"*>(this)] : GLOBAL_IDS[const_cast<" + pcase.Name + @"*>(this)] = (GLOBAL_INDEX += 2); 
        return ""(" + sppair_.Item1.Prefix + sppair_.Item2.Name + @".algebra "" + this->toEvalString() + "")"";
        
            }";
                                               return retval_;
                                           }
                                           ) : new List<string>())) +
                                               @"
        }
    }

    if(!found){
        //ret = """";
        " + @"
    }
    //std::replace(retval.begin(), retval.end(), '_', '.');
    std::size_t index;
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

                                    var caseevalstr = @"
std::string " + pcase.Name + @"::toEvalString() const {
                        std::string retval = """";
                        bool found = false; if (found) {}";

                                    if (pcase.Interp_.PrintType_ == Grammar.Case.Interp.PrintType.Var)
                                    {
                                        caseevalstr += @"
        ";
                                        caseevalstr += string.Join("",
                                              ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                                              ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                                              {
                                                  //  var spInstances = ParsePeirce.Instance.SpaceInstances;

                                                  var retval = (@"
    if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                    (prod.HasValueContainer() ?
                                        "<" + prod.GetPriorityValueContainer().ValueType + "," +
                                            prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>") + @"*>(this->dom_)){
        found = true;
        auto env = getLastEnv();
        retval += ""(" + sppair.Item1.Prefix + sppair.Item2.Name + @"Eval (lang." + sppair.Item1.Prefix + @"." + sppair.Item2.Name + @"Expr.var "";
        retval += this->operand_1->toString();
        retval += "") "" + env +"")"";
    }");
                                                  return retval;
                                              }) : new List<string>()) + @"
    if (auto cont = dynamic_cast<domain::DomainContainer*>(this->dom_)){
        if(cont->hasValue()){
                        " +
                            (
                                            string.Join("",
                                        ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                                        ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                                        {
                                            var retval_ = @"
            if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                                (prod.HasValueContainer() ?
                                                    "<" + prod.GetPriorityValueContainer().ValueType + ","
                                                        + prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>")
                                                        + @"*>(cont->getValue())){
                
                auto env = getLastEnv();
                retval += ""(" + sppair.Item1.Prefix + sppair.Item2.Name + @"Eval (lang." + sppair.Item1.Prefix + @"." + sppair.Item2.Name + @"Expr.var "";
                retval += this->operand_1->toString();
                retval += "") "" + env +"")"";
            }";
                                            return retval_;
                                        }) : new List<string>())) + @"
        }
    }

";
                                    }
                                    else
                                    {


                                        caseevalstr +=
                                                string.Join("",
                                              ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                                              ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                                              {
                                                  //  var spInstances = ParsePeirce.Instance.SpaceInstances;

                                                  var retval = (@"
    if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                    (prod.HasValueContainer() ?
                                        "<" + prod.GetPriorityValueContainer().ValueType + "," +
                                            prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>") + @"*>(this->dom_)){
        found = true;
        //auto env = getEnvName();
        //int id = GLOBAL_IDS.count(const_cast< " + pcase.Name + @"*>(this)) ? GLOBAL_IDS[const_cast<" + pcase.Name + @"*>(this)] : GLOBAL_IDS[const_cast<" + pcase.Name + @"*>(this)] = (GLOBAL_INDEX += 2); 
        auto interpSp = i2d_->getSpace(dc->getSpace());
        int sid = GLOBAL_IDS.count(const_cast<Space*>(interpSp)) ? GLOBAL_IDS[const_cast<Space*>(interpSp)] : GLOBAL_IDS[const_cast<Space*>(interpSp)] = (GLOBAL_INDEX += 2); " +
         (sppair.Item2.HasFrame ? "auto interpFr = i2d_->getFrame(dc->getFrame());\n" +
            "\t\tint fid = GLOBAL_IDS.count(const_cast<Frame*>(interpFr)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr)] : GLOBAL_IDS[const_cast<Frame*>(interpFr)] = (GLOBAL_INDEX += 2);"
                                    : "") + @"
        " + (sppair.Item2.IsTransform ? "auto interpFr1 = i2d_->getFrame(dc->getFrom());\n" +
        "int fid1 = GLOBAL_IDS.count(const_cast<Frame*>(interpFr1)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr1)] : GLOBAL_IDS[const_cast<Frame*>(interpFr1)] = (GLOBAL_INDEX += 2);"
        : "") + @"
        " + (sppair.Item2.IsTransform ? "auto interpFr2 = i2d_->getFrame(dc->getTo());\n" +
        "int fid2 = GLOBAL_IDS.count(const_cast<Frame*>(interpFr2)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr2)] : GLOBAL_IDS[const_cast<Frame*>(interpFr2)] = (GLOBAL_INDEX += 2);"
        : "") +
                    (pcase.Interp_.PrintType_ == Grammar.Case.Interp.PrintType.Unk ? @"


        retval += ""(" + sppair.Item1.Prefix + sppair.Item2.Name + @".build "";
        retval += std::string(""(" + sppair.Item1.Prefix + @"Eval "") + ""(lang." + sppair.Item1.Prefix + @".spaceExpr.var ⟨⟨"" + std::to_string(sid) + ""⟩⟩) "" + getLastEnv() + "")\n"";
        " + (sppair.Item2.HasFrame ? @"retval += std::string(""(" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid) + ""⟩⟩) "" + getLastEnv() + "")\n""; " : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid1) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid2) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
        " + (prod.HasValueContainer() ? @"   retval += ""(⟨[]"";
        for (auto i = 0; i < " + prod.GetPriorityValueContainer().ValueCount + @"; i++)
            retval += ""++["" + std::to_string(*dc->getValue(i)) + ""]"";
        retval += ""\n\t\t,by refl⟩ : vector ℝ " + prod.GetPriorityValueContainer().ValueCount + @")" : @"retval += """) + @"))"";
        " : @"
        retval += ""(" + sppair.Item1.Prefix + sppair.Item2.Name + @".fromalgebra "";
        retval += std::string(""(" + sppair.Item1.Prefix + @"Eval "") + ""(lang." + sppair.Item1.Prefix + @".spaceExpr.var ⟨⟨"" + std::to_string(sid) + ""⟩⟩) "" + getLastEnv() + "")\n"";
        " + (sppair.Item2.HasFrame ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid1) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid2) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
        retval += std::string(""("") + this->operand_1->toAlgebraString() + """ + pcase.Interp_.Symbol + @""" + this->operand_2->toAlgebraString() + ""))"";
        ") + @"
    }");
                                                  return retval;
                                              }) : new List<string>()) + @"
    if (auto cont = dynamic_cast<domain::DomainContainer*>(this->dom_)){
        if(cont->hasValue()){
                        " +
                            (
                                            string.Join("",
                                        ParsePeirce.Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ?
                                        ParsePeirce.Instance.GrammarRuleToSpaceObjectMap[prod].Select(sppair =>
                                        {
                                            var retval_ = @"
            if(auto dc = dynamic_cast<domain::" + sppair.Item1.Name + sppair.Item2.Name + @"" +
                                                (prod.HasValueContainer() ?
                                                    "<" + prod.GetPriorityValueContainer().ValueType + ","
                                                        + prod.GetPriorityValueContainer().ValueCount + ">" : "<float,1>")
                                                        + @"*>(cont->getValue())){
                //auto env = getEnvName();
                //int id = GLOBAL_IDS.count(const_cast< " + prod.Name + @"*>(this)) ? GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] : GLOBAL_IDS[const_cast<" + prod.Name + @"*>(this)] = (GLOBAL_INDEX += 2); 
                auto interpSp = i2d_->getSpace(dc->getSpace());
                int sid = GLOBAL_IDS.count(const_cast<Space*>(interpSp)) ? GLOBAL_IDS[const_cast<Space*>(interpSp)] : GLOBAL_IDS[const_cast<Space*>(interpSp)] = (GLOBAL_INDEX += 2); 
        " + (sppair.Item2.HasFrame ? "auto interpFr = i2d_->getFrame(dc->getFrame());\n" +
        "int fid = GLOBAL_IDS.count(const_cast<Frame*>(interpFr)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr)] : GLOBAL_IDS[const_cast<Frame*>(interpFr)] = (GLOBAL_INDEX += 2);"
        : "") + @"
        " + (sppair.Item2.IsTransform ? "auto interpFr1 = i2d_->getFrame(dc->getFrom());\n" +
        "int fid1 = GLOBAL_IDS.count(const_cast<Frame*>(interpFr1)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr1)] : GLOBAL_IDS[const_cast<Frame*>(interpFr1)] = (GLOBAL_INDEX += 2);"
        : "") + @"
        " + (sppair.Item2.IsTransform ? "auto interpFr2 = i2d_->getFrame(dc->getTo());\n" +
        "int fid2 = GLOBAL_IDS.count(const_cast<Frame*>(interpFr2)) ? GLOBAL_IDS[const_cast<Frame*>(interpFr2)] : GLOBAL_IDS[const_cast<Frame*>(interpFr2)] = (GLOBAL_INDEX += 2);"
        : "") +
                (pcase.Interp_.PrintType_ == Grammar.Case.Interp.PrintType.Unk ? @"
                
        retval += ""(" + sppair.Item1.Prefix + sppair.Item2.Name + @".build "";
        retval += std::string(""(" + sppair.Item1.Prefix + @"Eval "") + ""(lang." + sppair.Item1.Prefix + @".spaceExpr.var ⟨⟨"" + std::to_string(sid) + ""⟩⟩) "" + getLastEnv() + "")\n"";
        " + (sppair.Item2.HasFrame ? @"retval += std::string(""(" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid) + ""⟩⟩) "" + getLastEnv() + "")\n""; " : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid1) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid2) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
         " + (prod.HasValueContainer() ? @"   retval += ""(⟨[]"";
        for (auto i = 0; i < " + prod.GetPriorityValueContainer().ValueCount + @"; i++)
            retval += ""++["" + std::to_string(*dc->getValue(i)) + ""]"";
        retval += ""\n\t\t,by refl⟩ : vector ℝ " + prod.GetPriorityValueContainer().ValueCount + @")" : @"retval += """) + @"))"";
        "
              : @"
        retval += ""(" + sppair.Item1.Prefix + sppair.Item2.Name + @".fromalgebra "";
        retval += std::string(""(" + sppair.Item1.Prefix + @"Eval "") + ""(lang." + sppair.Item1.Prefix + @".spaceExpr.var ⟨⟨"" + std::to_string(sid) + ""⟩⟩) "" + getLastEnv() + "")\n"";
        " + (sppair.Item2.HasFrame ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid1) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
            " + (sppair.Item2.IsTransform ? @"retval += std::string(""     (" + sppair.Item1.Prefix + @"FrameEval "") + ""(lang." + sppair.Item1.Prefix + @".frameExpr.var ⟨⟨"" + std::to_string(fid2) +""⟩⟩) ""+getLastEnv() + "")\n"";" : "") + @"
        retval += std::string(""("") + this->operand_1->toAlgebraString() + """ + pcase.Interp_.Symbol + @""" + this->operand_2->toAlgebraString() + ""))"";
        ") + @"
            }";
                                            return retval_;
                                        }) : new List<string>())) + @"
        }
    }

";
                                    }


                                    caseevalstr += @"
    if (!found)
                                    {
                                        //ret = """";
                                        " + @"
    }
                                    //std::replace(retval.begin(), retval.end(), '_', '.');
                                    std::size_t index;
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

                                    file += "\n" + caseevalstr;
                                    file += "\n" + casealgstr;
                                }
                                file += "\n" + casecons;
                                file += casetostr + "\n";
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


namespace interp2domain
{
    class InterpToDomain;
}
#ifndef INTERP2DOMAIN_H
#include ""InterpToDomain.h""
#endif

namespace interp{

std::string getEnvName();
std::string getLastEnv();

class Interp;
class Space;
class MeasurementSystem;
class AxisOrientation;
class DerivedSpace;
class Frame;
";
            var file = header;

            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
               // if (true || prod.ProductionType != Grammar.ProductionType.Single && prod.ProductionType != Grammar.ProductionType.CaptureSingle)
                //{
                    file += "\n";
                    file += "class " + prod.Name + ";";
              //  }

                if (prod.ProductionType == Grammar.ProductionType.Single || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                    continue;

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
  virtual std::string toString() const { return ""Not Implemented -- don't call this!!"";};
  virtual std::string toDefString() const { return ""Don't call this either!"";};
  //friend class Interp;
//protected:
  coords::Coords *coords_;
  domain::DomainObject *dom_;
};


class Space : public Interp
{
public:
    Space(domain::Space* s) : s_(s) {};
    virtual std::string toString() const override;
    virtual std::string toDefString() const override;
    virtual std::string getVarExpr() const;
    virtual std::string getEvalExpr() const;
//protected:
    domain::Space* s_;
};

class DerivedSpace : public Space
{
public:
    DerivedSpace(domain::DerivedSpace* s, Space* base1, Space* base2) : Space(s), base_1(base1), base_2(base2) {};
    virtual std::string toString() const override;
    virtual std::string toDefString() const override;

    Space* getBase1() const {
        return this->base_1;
    }

    Space* getBase2() const {
        return this->base_2;
    }

protected:
    interp::Space *base_1,*base_2;

};

class MeasurementSystem : public Interp
{
public :
    MeasurementSystem(domain::MeasurementSystem* ms) : ms_(ms){};
    virtual std::string toString() const override;
    virtual std::string toDefString() const override;
protected:
    domain::MeasurementSystem* ms_;
};

class AxisOrientation : public Interp
{
public :
    AxisOrientation(domain::AxisOrientation* ax) : ax_(ax){};
    virtual std::string toString() const override;
    virtual std::string toDefString() const override;
protected:
    domain::AxisOrientation* ax_;
};

class Frame : public Interp
{
public:
    Frame(domain::Frame* f, interp::Space* sp) : f_(f), sp_(sp) {};
    Frame(domain::Frame* f, interp::Space* sp, interp::MeasurementSystem* ms, interp::AxisOrientation* ax) : f_(f), sp_(sp), ms_(ms), ax_(ax) {};
    virtual std::string toString() const override;
    virtual std::string toDefString() const override;
//protected:
    domain::Frame* f_;
    interp::Space* sp_;
    interp::MeasurementSystem* ms_;
    interp::AxisOrientation* ax_;
};

";
            file += "\n\n" + interp + "\n\n";

            foreach (var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                if (prod.ProductionType != Grammar.ProductionType.Single && prod.ProductionType != Grammar.ProductionType.CaptureSingle)
                {
                    var prodStr =
@"

class " + prod.Name + @" : public " + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Interp") + @" {
public:
    " + prod.Name + @"(coords::" + prod.Name + @"* coords, domain::DomainObject* dom);
    virtual std::string toString() const " + (prod.Passthrough != null || prod.Inherits != null ? "override" :"override" ) + @";
    virtual std::string toDefString() const override;
    //friend class Interp;  
    " + (prod.ProductionType == Grammar.ProductionType.Capture? @"virtual std::string toEvalString() const" + ((prod.Passthrough != null || prod.Inherits != null) 
                                                                                                                    && (prod.Passthrough != null ? prod.Passthrough.ProductionType == Grammar.ProductionType.Capture : prod.Inherits != null ? prod.Inherits.ProductionType == Grammar.ProductionType.Capture : false)? " override" : "") + @"{ return """";};" : "") + @" 
    " + (prod.ProductionType == Grammar.ProductionType.Capture? @"virtual std::string toAlgebraString() const" + ((prod.Passthrough != null || prod.Inherits != null)
                                                                                                                    && (prod.Passthrough != null ? prod.Passthrough.ProductionType == Grammar.ProductionType.Capture : prod.Inherits != null ? prod.Inherits.ProductionType == Grammar.ProductionType.Capture : false) ? " override" : "") + @"{ return """";};" : "") + @"             
};

";
                    file += prodStr;
                }
                else
                {
                    int x = 0;
                    int p = 0;
                    int k = 0;
                    var prodStr = @"

class " + prod.Name + @" : public " + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Interp") + @" {
public:
    " + prod.Name + @"(coords::" + prod.Name + @"* coords, domain::DomainObject* dom " + (prod.Cases[0].Productions.Count > 0 ? "," 
        + string.Join(",", prod.Cases[0].Productions.Select(p_ => "interp::" + p_.Name + " *operand" + ++p)) : "") + @" );
    virtual std::string toString() const override;
    virtual std::string toDefString() const override;
    " + (prod.ProductionType == Grammar.ProductionType.CaptureSingle ? @"virtual std::string toEvalString() const" + " " + @";" : "") + @" 
    " + (prod.ProductionType == Grammar.ProductionType.CaptureSingle? @"virtual std::string toAlgebraString() const" + " " + @";" : "") + @" 

    " +

            string.Join("\n", prod.Cases[0].Productions.Select(p_ => "interp::" + p_.Name + " *getOperand" + ++k + "() const { return operand_" + k + @"; }; ")) +


           // string.Join("\n", pcase.Productions.Select(p_ => "Interp::" + p_.Name + " *getOperand" + ++i) + "(); ")


           "\nprotected:\n\t" +
           string.Join("\n\t", prod.Cases[0].Productions.Select(p_ => "interp::" + p_.Name + " *operand_" + ++x + ";"))
           +
           @"
};

";
                    file += prodStr;
                }

                if (prod.ProductionType == Grammar.ProductionType.Single || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                    continue;

                foreach (var pcase in prod.Cases)
                {
                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    var i = 0;
                    var j = 0;
                    var k = 0;

                    switch (pcase.CaseType)
                    {
                        case Grammar.CaseType.ArrayOp:
                            {
                                var caseStr =
    @"

class " + pcase.Name + @" : public " + prod.Name + @" {
public:
    " + pcase.Name + "(coords::" + pcase.Name + @"* coords, domain::DomainObject* dom, std::vector<" + pcase.Productions[0].Name + @"*> operands);
    virtual std::string toString() const override;
    virtual std::string toDefString() const override;
    virtual std::string toStringLinked(
        std::vector<interp::Space*> links, 
        std::vector<std::string> names, 
        std::vector<interp::MeasurementSystem*> msystems,
        std::vector<std::string> msnames,
        std::vector<interp::AxisOrientation*> axes,
        std::vector<std::string> axisnames,
        std::vector<interp::Frame*> framelinks, 
        std::vector<string> framenames, 
        interp2domain::InterpToDomain* i2d,
        bool before);
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
                            }
                        default:
                            {

                                var caseStr = @"

class " + pcase.Name + @" : public " + prod.Name + @" {
public:
    " + pcase.Name + @"(coords::" + pcase.Name + @"* coords, domain::DomainObject* dom " + (pcase.Productions.Count > 0 ? "," + string.Join(",", pcase.Productions.Select(p_ => "interp::" + p_.Name + " *operand" + ++i)) : "") + @" );
    virtual std::string toString() const override ;
    virtual std::string toDefString() const override;
    " + (prod.ProductionType == Grammar.ProductionType.Capture ? @"virtual std::string toEvalString() const" + @" override;" : "") + @" 
    " + (prod.ProductionType == Grammar.ProductionType.Capture ? @"virtual std::string toAlgebraString() const" + @" override;" : "") + @" 
    
    " +

            string.Join("\n", pcase.Productions.Select(p_ => "interp::" + p_.Name + " *getOperand" + ++k + "() const { return operand_" + k + @"; }; ")) +


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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace PeirceGen.Generators
{
    public class GenCoords : GenBase
    {
        public override string GetCPPLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["GenPath"] + "Coords.cpp";
        }

        public override string GetHeaderLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["GenPath"] + "Coords.h";
        }

        public GenCoords()
        {
            //var ParsePeirce.Instance


        }

        public override void GenHeader()
        {
            var header = @"
#ifndef COORDS_H
#define COORDS_H

#include ""clang/AST/AST.h""
#include ""clang/ASTMatchers/ASTMatchFinder.h""
#include <cstddef>
#include <iostream> // for cheap logging only
#include <string>
#include <memory>

#include ""AST.h""


/*
Code coordinate objects wrap AST 
objects to provide inherited behavior
necessary and appropriate for each
referenced code object. They give
AST objects types in our domain's
ontology. 

We maintain a bijection betwen AST and 
Coord objects: specifically between the
memory addresses of these objects. It is
thus critical not to map one AST node
address to more than one Coord object.

Code coordinates provide for ontology 
translation, between the Clang's AST 
types and our domain elements (id, 
var expr, function application expr, 
constructed vector, and definition).
*/

namespace coords
{

    // Ontology of Clang object types that can be 
    // coordinatized. We do not currently use 
    // clang::Decl but we include it here to 
    // establish a path togeneralizability
    //
    //enum ast_type { CLANG_AST_STMT, CLANG_AST_DECL }; 

    struct ASTState
    {
    public:
        ASTState(
        std::string file_id,
        std::string file_name,
        std::string file_path,
        std::string name,
        std::string code,
        int begin_line_no,
        int begin_col_no,
        int end_line_no,
        int end_col_no
        );

        std::string file_id_;
        std::string file_name_;
        std::string file_path_;

        std::string name_; //only used for Decl. possibly subclass this, or else this property is unused elsewhere
        std::string code_;

        int begin_line_no_;
        int begin_col_no_;
        int end_line_no_;
        int end_col_no_;

        bool operator ==(const ASTState& other) const {
return 
    file_id_ == other.file_id_ and
            file_name_ == other.file_name_ and
            file_path_ == other.file_path_ and
            begin_line_no_ == other.begin_line_no_ and
            begin_col_no_ == other.begin_col_no_ and
            end_line_no_ == other.end_line_no_ and
            end_col_no_ == other.end_col_no_;
}
};

class Coords
{
public:
    Coords() {};

    virtual bool operator ==(const Coords &other) const;
    virtual std::string toString() const;
    virtual std::string getSourceLoc() const;
    int getIndex() const { return index_; };
    void setIndex(int index);
    
    virtual bool codegen() const {
        return false;
    }

    ASTState* state_; //maybe  change this to a constructor argument
protected:
    int index_;
};
template <class ValueType, int ValueCount>
class ValueCoords
{
public: 
   // ValueCoords() {};
    ValueCoords() {
        for(int i = 0; i<ValueCount;i++){
            this->values_[i] = nullptr;
        }
    };//, value_len_(len) { this->values_ = new ValueType[value_len_]; };
    //ValueCoords(ValueType* values, int len) : values_(values), value_len_(len) {};
    ValueCoords(std::shared_ptr<ValueType> values...)  {
        
        int i = 0;
        for(auto val : {values}){
            if(i == ValueCount)
                throw ""Out of Range"";
            this->values_[i++] = val ? std::make_shared<ValueType>(*val) : nullptr;
            
        }
    }

    ValueCoords(std::initializer_list<std::shared_ptr<ValueType>> values){
        
        int i = 0;
        for(auto val : values){
            if(i == ValueCount)
                throw ""Out of Range"";
            this->values_[i++] = val ? std::make_shared<ValueType>(*val) : nullptr;
            
        }
    }

    std::shared_ptr<ValueType> getValue(int index) const {
        if(index< 0 or index >= ValueCount)
            throw ""Invalid Index"";
        return this->values_[index];
    };


    void setValue(ValueType value, int index)
    {
        if (index < 0 or index >= ValueCount)
            throw ""Invalid Index"";
        if (this->values_[index])
            *this->values_[index] = value;
        else
            this->values_[index] = std::make_shared<ValueType>(value);
        //this->values_[index] = new ValueType(value)
    };

    void setValue(std::shared_ptr<ValueType> value, int index)
    {
        if (index < 0 or index >= ValueCount)
            throw ""Invalid Index"";
        if (this->values_[index])
            if(value)
                *this->values_[index] = *value;
            else{
                this->values_[index] = std::make_shared<ValueType>(*value);
            }
        else
            this->values_[index] = value ? std::make_shared<ValueType>(*value) : nullptr;
        //this->values_[index] = value ? new ValueType(*value) : nullptr;
    };

    std::shared_ptr<ValueType>* getValues() const {
        return const_cast<std::shared_ptr<ValueType>*>(this->values_);
    }

protected:
    std::shared_ptr<ValueType> values_[ValueCount];
    //int value_len_;
    //std::Vector<ValueType*> values_;

};


";

            var grammar = ParsePeirce.Instance.Grammar;

            var file = header;

            foreach(var prod in grammar.Productions)
            {
                if (true)
                {
                    file += "\n";
                    file += "class " + prod.Name + ";";
                }

                if (prod.ProductionType == Grammar.ProductionType.Single || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                    continue;

                foreach(var pcase in prod.Cases)
                {
                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    
                    file += "\n";
                    file += "class " + pcase.Name + ";";
                }
            }


            foreach (var prod in grammar.Productions)
            {
                if (prod.ProductionType != Grammar.ProductionType.Single &&  prod.ProductionType != Grammar.ProductionType.CaptureSingle)
                {
                    var prodStr =
    @"

class " + prod.Name + @" : public " 
        + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Coords") 
        + (prod.HasValueContainer() && !prod.InheritsContainer() ? 
            ", public ValueCoords<" + prod.GetPriorityValueContainer().ValueType + "," + prod.GetPriorityValueContainer().ValueCount + ">" : "") 
        + @" {
public:
    " + prod.Name + @"(" + (prod.HasValueContainer() ?
                    Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "std::shared_ptr<" + prod.GetPriorityValueContainer().ValueType + "> value" + v) : "") + @") : " 
        + ( prod.HasValueContainer() && !prod.InheritsContainer() ? "ValueCoords < " + prod.GetPriorityValueContainer().ValueType + ", " + prod.GetPriorityValueContainer().ValueCount + " >::ValueCoords" :
            prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Coords") + 
            @"(" + (prod.HasValueContainer() ? "{" +
                    Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "value" + v) + "}" : "") + @") {};
    std::string virtual toString() const override { return ""Do not call this""; };
    bool operator==(const " + prod.Name + @" &other) const {
        return ((Coords*)this)->state_ == ((Coords)other).state_;
    };
    virtual bool codegen() const override {
        return " + (prod.ProductionType != Grammar.ProductionType.Hidden ? "true" : "false") + @";
    }
};

    ";

                    file += prodStr;
                }
                else if(prod.ProductionType == Grammar.ProductionType.Single || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                {
                    var prodStr =
    @"

class " + prod.Name + @" : public "
        + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Coords")
        + (prod.HasValueContainer() && !prod.InheritsContainer() ?
            ", public ValueCoords<" + prod.GetPriorityValueContainer().ValueType + "," + prod.GetPriorityValueContainer().ValueCount + ">" : "")
        + @" {
public:
    " + prod.Name 
    + @"(" + Peirce.Join(",",Enumerable.Range(0, prod.Cases[0].Productions.Count),v=>"coords::" + prod.Cases[0].Productions[v].Name + " * operand" +v) 
    + (prod.HasValueContainer() ?
                    (prod.Cases[0].Productions.Count > 0 ? "," : "") + Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "std::shared_ptr<" + prod.GetPriorityValueContainer().ValueType + "> value" + v) : "")
    + @") : " 
                + (     prod.HasValueContainer() ? "ValueCoords < " + prod.GetPriorityValueContainer().ValueType + ", " + prod.GetPriorityValueContainer().ValueCount + " >::ValueCoords" :
                        prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : 
                        prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Coords") + @"(" + (prod.HasValueContainer() ? "{" +
                    Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "value" + v) + "}" : "") + @")
        " + (prod.Cases[0].Productions.Count > 0 ? "," :"") + Peirce.Join(",", Enumerable.Range(0, prod.Cases[0].Productions.Count), v => " operand_" + v + "(operand" + v + ")") + @"{};
    std::string virtual toString() const override;
    " + Peirce.Join("\n\t", Enumerable.Range(0, prod.Cases[0].Productions.Count), v => "coords::" + prod.Cases[0].Productions[v].Name + " * getOperand" + v + "(){ return this->operand_" + v +@";};") + @"
    bool operator==(const " + prod.Name + @" &other) const {
        return ((Coords*)this)->state_ == ((Coords)other).state_;
    };
    virtual bool codegen() const override {
        return " + (prod.ProductionType != Grammar.ProductionType.Hidden ? "true" : "false") + @";
    }
protected:
    " + Peirce.Join("\n\t", Enumerable.Range(0, prod.Cases[0].Productions.Count), v => "coords::" + prod.Cases[0].Productions[v].Name + " * operand_" + v + ";") + @"
};

    ";

                    file += prodStr;
                    continue;
                }

                foreach(var pcase in prod.Cases)
                {
                    switch(pcase.CaseType)
                    {
                        case Grammar.CaseType.Passthrough :
                            continue;
                        case Grammar.CaseType.Inherits:
                            continue;
                        case Grammar.CaseType.Ident:
                            {
                                break;

                                /*int i = 0, j = 0, k = 0;
                                var caseStr =
    @"

class " + prod.Name + "_"+ pcase.Name + @" : public Coords {
public:
    " + prod.Name + @"(" + string.Join(", ", pcase.Productions.Select(p_ => "coords::" + p_.Name + " * operand_" + ++k)) + @");
    virtual std::string toString() const;
    bool operator==(const " + prod.Name + @" &other) const {
        return this->state_ == other.state_;
    };" + "\n\t" +
        string.Join("\n\t", pcase.Productions.Select(p_ => "coords::" + p_.Name + " *getOperand" + ++i + "(); ").ToList())
        +
    "\nprotected:\n\t" +
    string.Join("\n\t", pcase.Productions.Select(p_ => "coords::" + p_.Name + " *operand" + ++j + ";"))
    +
    @"
};

";
                                break;*/
                            }
                        case Grammar.CaseType.Op:
                        case Grammar.CaseType.Hidden:
                        case Grammar.CaseType.Pure:
                            {
                            int i = 0, j = 0, k = 0;
                            var caseStr =
@"

class " + pcase.Name + @" : public " + prod.Name +  @" {
public:
    " + pcase.Name + @"(" + string.Join(", ", pcase.Productions.Select(p_ => "coords::" + p_.Name + " * operand_" + ++k)) +
    (prod.HasValueContainer() ? (pcase.Productions.Count > 0 ? "," : "") +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "std::shared_ptr<" + prod.GetPriorityValueContainer().ValueType + "> value" + v) : "") +
    @");
    virtual std::string toString() const override;
    bool operator==(const " + prod.Name + @" &other) const {
        return ((Coords*)this)->state_ == ((Coords)other).state_;
    };" + "\n\t" +
    string.Join("\n\t", pcase.Productions.Select(p_ => "coords::"+p_.Name + " *getOperand" + ++i + "(); ").ToList())
    +
"\nprotected:\n\t" + 
string.Join("\n\t", pcase.Productions.Select(p_ => "coords::"+p_.Name + " *operand" + ++j + ";"))
+
@"
};

";
                                file += caseStr;
                            break;
                        }
                        case Grammar.CaseType.ArrayOp:
                        {
                                //int i = 0, j = 0;//, k = 0;
                                var caseStr =
    @"

class " + pcase.Name + @" : public " + prod.Name + @" {
public:
    " + pcase.Name + @"(std::vector<"+pcase.Productions[0].Name + @"*> operands);
    virtual std::string toString() const override;
    bool operator==(const " + prod.Name + @" &other) const {
        return ((Coords*)this)->state_ == ((Coords)other).state_;
    };

    std::vector<" + pcase.Productions[0].Name + @"*> getOperands() const { return this->operands_; };

    coords::" + pcase.Productions[0].Name + @"* getOperand(int i) const {
        return ((int)this->operands_.size()) >= i ? this->operands_[i-1] : nullptr;
    }"+

    "\nprotected:\n\t" + @"
    std::vector<" + pcase.Productions[0].Name + @"*> operands_;

};

";
                                file += caseStr;
                                break;

                            }
                       /* case Grammar.CaseType.Value:
                        {
                            int i = 0, j = 0, k = 0;
                            var caseStr =
@"

class " + pcase.Name + @" : public " + prod.Name +  @" {
public:
    " + pcase.Name + @"(" + (pcase.ValueCount > 0 ?
                       Peirce.Join(",", Enumerable.Range(0, pcase.ValueCount), v => pcase.ValueType + " value" + v ) : "") + @");
    virtual std::string toString() const;
    bool operator==(const " + prod.Name + @" &other) const {
    return this->state_ == other.state_;
    }" + "\n" +
    
    (pcase.ValueCount > 0 ?
                       Peirce.Join("\n", Enumerable.Range(0, pcase.ValueCount), v => pcase.ValueType + " getOperand" + v + "() const { return this->value_" + v + "; }") : "")
    +
@"
protected:" + "\n\t" + 
 (pcase.ValueCount > 0 ?
       Peirce.Join("\n\t", Enumerable.Range(0, pcase.ValueCount), v => pcase.ValueType + " value_" + v+";") : "")
+
@"
};

";
                                file += caseStr;
                            break;
                        }*/
                    }
                }

            }
            file += @"
} // namespace coords

#endif";
            this.HeaderFile = file;
        }

        public override void GenCpp()
        {
            var header = @"
#include ""Coords.h""

#include <g3log/g3log.hpp>
#include <memory>


namespace coords {

/*
Code coordinates provide for ontology translation, between the 
concrete types used to represent pertinent code elements in a 
given programming language and system (code language), and the 
abstract terms of a domain language. Here the code language is
Clang as used to map applications built on our simple vector
class (Vec). The domain language is one of simple vector space
expressions and objects. 
*/

// Ontology of code object types that can be coordinatized
// clang::Decl unused by Peirce, here for generalizability
//


ASTState::ASTState(
    std::string file_id,
    std::string file_name,
    std::string file_path,
    std::string name,
    std::string code,
    int begin_line_no,
    int begin_col_no,
    int end_line_no,
    int end_col_no) 
    : file_id_{file_id}, file_name_{file_name}, file_path_{file_path}, name_{name}, code_{code}, begin_line_no_{begin_line_no}, begin_col_no_{begin_col_no}, end_line_no_{end_line_no}, end_col_no_{end_col_no} {}

//Coords::Coords(){
//}

void Coords::setIndex(int index){
    this->index_ = index;
}

bool Coords::operator==(const Coords &other) const {
    return this->state_ == other.state_;
}

std::string Coords::toString() const {
    LOG(FATAL) << ""Coords::toString. Error. Should not be called. Abstract.\n"";
    return NULL;
}

std::string Coords::getSourceLoc() const {
    /*clang::FullSourceLoc FullLocation;
    if (ast_type_tag_ == CLANG_AST_STMT)
    {
      FullLocation = context_->getFullLoc(clang_stmt_->getSourceRange().getEnd());
    } else {
      FullLocation = context_->getFullLoc(clang_decl_->getLocation());
    }*/
    //std::cout<<this->toString()<<std::endl;
    std::string retval = ""Begin: line "";
    retval += std::to_string(this->state_->begin_line_no_);
    retval +=  "", column "";
    retval +=  std::to_string(this->state_->begin_col_no_);
    retval += ""\tEnd: line "";
    retval += std::to_string(this->state_->end_line_no_);
    retval += "", column "";
    retval += std::to_string(this->state_->end_col_no_);

    return retval;
}

/*************************************************************
* Coordinate subclasses, for type checking, override behaviors
*************************************************************/

";
            var file = header;


            foreach(var prod in ParsePeirce.Instance.Grammar.Productions)
            {
               // var prodcons = "\n" + prod.Name + "::" + prod.Name + "() : " + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : "Coords") + "(){}\n";

                //file += prodcons;
                if(prod.ProductionType == Grammar.ProductionType.Single || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                {
                    
                    file += "\nstd::string " + prod.Name + "::toString() const{ return " + "std::string(\"\")" + @" + state_->name_" + @"+" + @" "".B.L""+ std::to_string(state_->begin_line_no_) + ""C"" + std::to_string(state_->begin_col_no_) + "".E.L"" + std::to_string(state_->end_line_no_) + ""C"" + std::to_string(state_->end_col_no_)" + @";}";
                    continue;
                }


                foreach(var pcase in prod.Cases)
                {
                    
                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    switch (pcase.CaseType)
                    {
                        case Grammar.CaseType.Passthrough:
                            continue;
                        case Grammar.CaseType.Inherits:
                            continue;
                        case Grammar.CaseType.Ident:
                            break;
                        case Grammar.CaseType.Op:
                        case Grammar.CaseType.Hidden:
                        case Grammar.CaseType.Pure:
                            {
                                int i = 0, j = 0;//, k = 0;
                                var cons = "\n" + pcase.Name + "::" + pcase.Name + "(" 
                                    + string.Join(",", pcase.Productions.Select(p_ => "coords::" + p_.Name + " *operand_" + ++j)) +
                                    (prod.HasValueContainer() ? (pcase.Productions.Count > 0 ? "," : "") +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "std::shared_ptr<" + prod.GetPriorityValueContainer().ValueType + "> value" + v) : "") + ") : ";

                                cons += "\n\t\t" + prod.Name + "(" +
                                    (prod.HasValueContainer() ?
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => " value" + v) : "") + ")" + (pcase.Productions.Count > 0 ? "," + string.Join(",", pcase.Productions.Select(p_ => "operand" + ++i + "(operand_" + i + ")")) : "") + "{}";

                                file += cons;
                                i = 0; j = 0;
                                foreach (var casep in pcase.Productions)
                                {
                                    var opgetter = "\n" + "coords::" + casep.Name + "* " + pcase.Name + "::getOperand" + ++i + "() { return this->operand" + i + ";}";
                                    file += opgetter;
                                }
                                file += "\nstd::string " + pcase.Name + "::toString() const{ return " + (prod.ProductionType == Grammar.ProductionType.Hidden ? "\"\""
                                    :
                                        prod.ProductionType == Grammar.ProductionType.Capture || prod.ProductionType == Grammar.ProductionType.CaptureSingle ?
                                        "std::string(\"\")" + @" + state_->name_" + @"+" + @" "".B.L""+ std::to_string(state_->begin_line_no_) + ""C"" + std::to_string(state_->begin_col_no_) + "".E.L"" + std::to_string(state_->end_line_no_) + ""C"" + std::to_string(state_->end_col_no_)" :
                                        pcase.Productions.Count > 0 ? "operand1->toString()" : @""""""
                                    )
                                    
                                    
                                    + @";}";
                                file += "\n\n";
                                break;
                            }
                        //case Grammar.CaseType.Ident:
                        //{

                        //}
                        case Grammar.CaseType.ArrayOp:
                            {
                                /*
                                var caseStr =
                                    @"

                                class " + pcase.Name + @" : public " + prod.Name + @" {
                                public:
                                    " + pcase.Name + @"(std::vector<"+pcase.Productions[0].Name + @"*> operands);
                                    virtual std::string toString() const;
                                    bool operator==(const " + prod.Name + @" &other) const {
                                        return this->state_ == other.state_;
                                    };" + "\n\t" +
                                        string.Join("\n\t", pcase.Productions.Select(p_ => "coords::" + p_.Name + " *getOperand" + ++i + "(); ").ToList())
                                        +
                                    "\nprotected:\n\t" + @"
                                    std::vector<" + pcase.Name + @"*> operands_;

                                };

                                ";
                                */


                                //int i = 0, j = 0;//, k = 0;
                                var cons = "\n" + pcase.Name + "::" + pcase.Name  + "(std::vector<" + pcase.Productions[0].Name + @"*> operands) :" + prod.Name + "()" +@" {
    for(auto& op : operands){
        this->operands_.push_back(op);
    }

};";

                                file += cons;
                                //i = 0; j = 0;
                                //foreach (var casep in pcase.Productions)
                                //{
                                //   var opgetter = "\n" + "coords::" + casep.Name + "* " + pcase.Name + "::getOperand" + ++i + "() { return this->operand" + i + ";}";
                                //  file += opgetter;
                                //}

                                /*
                                 * 
                                    var retval = "std::string(\"\")";
                                    if (toParse.Contains("$NAME"))
                                        retval += @" + state_->name_";
                                    if (toParse.Contains("$LOC"))
                                        retval += @"+" + @" "".B.L""+ std::to_string(state_->begin_line_no_) + ""C"" + std::to_string(state_->begin_col_no_) + "".E.L"" + std::to_string(state_->end_line_no_) + ""C"" + std::to_string(state_->end_col_no_)";
                                    // this.CoordsToString = (prod) => { return @"(this->getIndex() > 0 ? ""INDEX""+std::to_string(this->getIndex())+""."":"""")+" + "\"" + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name).Replace("_", ".") + "\" + " + retval; };

                                 * 
                                 * */
                                //(this->getIndex() > 0 ? ""INDEX""+std::to_string(this->getIndex())+""."":"""")+" + "\"" 
                                //+ (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name).Replace("_", ".") + "\" + " + retval;
                                file += "\nstd::string " + pcase.Name + "::toString() const{ return " 
                                    + @"(this->getIndex() > 0 ? ""INDEX"" + std::to_string(this->getIndex()) + ""."":"""")+" + "\"" +
                                    (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name).Replace("_", ".") + "\" + "+ "std::string(\"\")" + @";}";
                                file += "\n\n";
                                break;

                            }
                        /*case Grammar.CaseType.Value:
                            {

                                int i = 0, j = 0, k = 0;
                                var cons = "\n" + pcase.Name + "::" + pcase.Name + "(" + Peirce.Join(",", Enumerable.Range(0, pcase.ValueCount), v => pcase.ValueType+ " operand" + v) + ") : ";

                                //if (pcase.CaseType == Grammar.CaseType.Real)
                                //   cons = @"(" + string.Join(", ", pcase.ProductionRefs.Select(p_ => "double value_" + ++k)) + @");";
                                
                                cons += "\n\t\t" + prod.Name + "()" + (pcase.ValueCount > 0 ? "," +
                       Peirce.Join(",", Enumerable.Range(0, pcase.ValueCount), v => "value_" + v + "(operand" + v + ")") : "") + "{}";

                                file += cons;
                                i = 0; j = 0;
                                foreach (var casep in pcase.Productions)
                                {
                                    var opgetter = "\n" + "coords::" + casep.Name + "* " + pcase.Name + "::getOperand" + ++i + "() { return this->operand" + i + ";}";
                                    file += opgetter;
                                }
                                file += "\nstd::string " + pcase.Name + "::toString() const{ return " + pcase.CoordsToString(prod) + @";}";
                                file += "\n\n";
                                break;
                            }*/
                    }
                }
            }

            file += "\n} // namespace codecoords";

            this.CppFile = file;
        }
    }
}

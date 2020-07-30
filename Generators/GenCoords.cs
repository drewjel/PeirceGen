using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen.Generators
{
    public class GenCoords : GenBase
    {
        public override string GetCPPLoc()
        {
            return @"/peirce/PeirceGen/symlinkme/Coords.cpp";
        }

        public override string GetHeaderLoc()
        {
            return @"/peirce/PeirceGen/symlinkme/Coords.h";
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
        int begin_line_no,
        int begin_col_no,
        int end_line_no,
        int end_col_no
        );

        std::string file_id_;
        std::string file_name_;
        std::string file_path_;

        std::string name_; //only used for Decl. possibly subclass this, or else this property is unused elsewhere

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

    ASTState* state_; //maybe  change this to a constructor argument
    protected:
        int index_;
};
";

            var grammar = ParsePeirce.Instance.Grammar;

            var file = header;

            foreach(var prod in grammar.Productions)
            {
                if (true || prod.ProductionType != Grammar.ProductionType.Single)
                {
                    file += "\n";
                    file += "class " + prod.Name + ";";
                }

                foreach(var pcase in prod.Cases)
                {
                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits || pcase.CaseType == Grammar.CaseType.Ident)
                        continue;
                    
                    file += "\n";
                    file += "class " + pcase.Name + ";";
                }
            }


            foreach (var prod in grammar.Productions)
            {
                if (prod.ProductionType != Grammar.ProductionType.Single)
                {
                    var prodStr =
    @"

    class " + prod.Name + @" : public " + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Coords") + @" {
    public:
        " + prod.Name + @"() : " + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Coords") + @"() {};
        std::string virtual toString() const { return ""Do not call this""; };
        bool operator==(const " + prod.Name + @" &other) const {
        return this->state_ == other.state_;
        };
    };

    ";

                    file += prodStr;
                }
                else if(prod.ProductionType == Grammar.ProductionType.Single)
                {
                    var prodStr =
    @"

    class " + prod.Name + @" : public " + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Coords") + @" {
    public:
        " + prod.Name + @"() : " + (prod.Passthrough is Grammar.Production ? prod.Passthrough.Name : prod.Inherits is Grammar.Production ? prod.Inherits.Name : "Coords") + @"() {};
        std::string virtual toString() const;
        bool operator==(const " + prod.Name + @" &other) const {
        return this->state_ == other.state_;
        };
    };

    ";

                    file += prodStr;
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

                                int i = 0, j = 0, k = 0;
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
                                break;
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
    " + pcase.Name + @"(" + string.Join(", ", pcase.Productions.Select(p_ => "coords::" + p_.Name + " * operand_" + ++k)) + @");
    virtual std::string toString() const;
    bool operator==(const " + prod.Name + @" &other) const {
        return this->state_ == other.state_;
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
                                int i = 0, j = 0, k = 0;
                                var caseStr =
    @"

class " + pcase.Name + @" : public " + prod.Name + @" {
public:
    " + pcase.Name + @"(std::vector<"+pcase.Productions[0].Name + @"*> operands);
    virtual std::string toString() const;
    bool operator==(const " + prod.Name + @" &other) const {
        return this->state_ == other.state_;
    };

    coords::" + pcase.Productions[0].Name + @"* getOperand(int i) const {
        return this->operands_.size() >= i ? this->operands_[i-1] : nullptr;
    }"+

    "\nprotected:\n\t" + @"
    std::vector<" + pcase.Productions[0].Name + @"*> operands_;

};

";
                                file += caseStr;
                                break;

                            }
                        case Grammar.CaseType.Real:
                        {
                            int i = 0, j = 0, k = 0;
                            var caseStr =
@"

class " + pcase.Name + @" : public " + prod.Name +  @" {
public:
    " + pcase.Name + @"(" + string.Join(", ", pcase.ProductionRefs.Select(p_ => "double value_" + ++k)) + @");
    virtual std::string toString() const;
    bool operator==(const " + prod.Name + @" &other) const {
    return this->state_ == other.state_;
    }" + "\n" +
                      
    string.Join("\n", pcase.ProductionRefs.Select(p_ => "\tdouble getOperand" + ++i + "() const { return this->value"+i+"; }"))

    +
@"
protected:" + "\n\t" + 
string.Join("\n\t", pcase.ProductionRefs.Select(p_ => "double" + " value" + ++j + ";"))
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

        public override void GenCpp()
        {
            var header = @"
#include ""Coords.h""

#include <g3log/g3log.hpp>


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
    int begin_line_no,
    int begin_col_no,
    int end_line_no,
    int end_col_no) 
    : file_id_{file_id}, file_name_{file_name}, file_path_{file_path}, name_{name}, begin_line_no_{begin_line_no}, begin_col_no_{begin_col_no}, end_line_no_{end_line_no}, end_col_no_{end_col_no} {}

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
    retval += "" End: line "";
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
                if(prod.ProductionType == Grammar.ProductionType.Single)
                {

                    file += "\nstd::string " + prod.Name + "::toString() const{ return " + prod.Cases[0].CoordsToString(prod) + @";}";
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
                                int i = 0, j = 0, k = 0;
                                var cons = "\n" + pcase.Name + "::" + pcase.Name + "(" + string.Join(",", pcase.Productions.Select(p_ => "coords::" + p_.Name + " *operand_" + ++j)) + ") : ";

                                cons += "\n\t\t" + prod.Name + "()" + (pcase.Productions.Count > 0 ? "," + string.Join(",", pcase.Productions.Select(p_ => "operand" + ++i + "(operand_" + i + ")")) : "") + "{}";

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


                                int i = 0, j = 0, k = 0;
                                var cons = "\n" + pcase.Name + "::" + pcase.Name  + "(std::vector<" + pcase.Productions[0].Name + @"*> operands) :" + prod.Name + "()" +@" {
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
                                file += "\nstd::string " + pcase.Name + "::toString() const{ return " + pcase.CoordsToString(prod) + @";}";
                                file += "\n\n";
                                break;

                            }
                        case Grammar.CaseType.Real:
                            {

                                int i = 0, j = 0, k = 0;
                                var cons = "\n" + pcase.Name + "::" + pcase.Name + "(" + string.Join(",", pcase.ProductionRefs.Select(p_ => "double value_" + ++k)) + ") : ";

                                //if (pcase.CaseType == Grammar.CaseType.Real)
                                 //   cons = @"(" + string.Join(", ", pcase.ProductionRefs.Select(p_ => "double value_" + ++k)) + @");";

                                cons += "\n\t\t" + prod.Name + "()" +(pcase.Productions.Count > 0 ? "," + string.Join(",", pcase.Productions.Select(p_ => "operand" + ++i + "(operand_" + i + ")")) : "") + "{}";

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
                            }
                    }
                }
            }

            file += "\n} // namespace codecoords";

            this.CppFile = file;
        }
    }
}

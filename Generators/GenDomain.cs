using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen.Generators
{
    public class GenDomain : GenBase
    {
        public override string GetCPPLoc()
        {
            return @"C:\Users\msfti\source\repos\givemeros\PeirceGen\symlinkme\Domain.cpp";
        }

        public override string GetHeaderLoc()
        {
            return @"C:\Users\msfti\source\repos\givemeros\PeirceGen\symlinkme\Domain.h";
        }
        public override void GenCpp()
        {
            var header = @"
#include <vector>
#include <iostream>
#include <string>
#include <utility>

// DONE: Separate clients from Domain
// #include ""Checker.h""

#include ""Domain.h""

#include <g3log/g3log.hpp>

#ifndef leanInferenceWildcard
#define leanInferenceWildcard ""_""
#endif

using namespace std;
using namespace domain;

";
            var file = header;


            /*
             * 
Space &Domain::mkSpace(const string &type, const string &name, const int dimension)
{
    Space *s = new Space(type, name, dimension);
    spaces.push_back(s);
    return *s;
}


std::vector<Space*> &Domain::getSpaces() 
{
    return spaces; 
}


            VecIdent* Domain::mkVecIdent(Space * s)
{
                VecIdent* id = new VecIdent(*s);
                idents.push_back(id);
                return id;
            }
            * 
             * */

            var domcons = @"DomainObject::DomainObject(std::initializer_list<DomainObject*> args) {
    for(auto dom : args){
        operands_.push_back(dom);
    }
    operand_count = this->operands_.size();
}";
            var domget = @"DomainObject* DomainObject::getOperand(int i) { return this->operands_.at(i); };";

            var domset = @"void DomainObject::setOperands(std::vector<DomainObject*> operands) { this->operands_ = operands; };";

            var domstr = @"std::string DomainObject::toString(){return ""A generic, default DomainObject""; };";

            file += "\n" + domcons + "\n" + domget + "\n" + domset + "\n" + domstr + "\n";

            var getSpaces = @"std::vector<Space*> &Domain::getSpaces() 
{
    return Space_vec; 
};
";
            file += getSpaces;

            var domaincontainerfns = @"

DomainContainer::DomainContainer(std::initializer_list<DomainObject*> operands) : DomainObject(operands) {
    this->inner_ = nullptr;
};

DomainContainer::DomainContainer(std::vector<DomainObject*> operands) : DomainObject(operands) {
    this->inner_ = nullptr;
};


void DomainContainer::setValue(DomainObject* dom_){

    //dom_->setOperands(this->inner_->getOperands());
    
    /*
    WARNING - THIS IS NOT CORRECT CODE. YOU ALSO NEED TO UNMAP/ERASE FROM THE ""OBJECT_VEC"". DO THIS SOON! 7/12
    */

    delete this->inner_;

    this->inner_ = dom_;
};

bool DomainContainer::hasValue(){
    return (bool)this->inner_;
};

std::string DomainContainer::toString(){
    if(this->hasValue()){
        return this->inner_->toString();
    }
    else{
        return ""No Interpretation provided"";
    }
};

";
            file += "\n" + domaincontainerfns + "\n";

            var getspace = @"
Space* Domain::getSpace(std::string key){
    return this->Space_map[key];
};

void Space::addFrame(Frame* frame){
    this->frames_.push_back(frame);
};

void Frame::setParent(Frame* parent){
    this->parent_ = parent;
};

DomainObject* Domain::mkDefaultDomainContainer(){
    return new domain::DomainContainer();
};

DomainObject* Domain::mkDefaultDomainContainer(std::initializer_list<DomainObject*> operands){
    return new domain::DomainContainer(operands);
};

DomainObject* Domain::mkDefaultDomainContainer(std::vector<DomainObject*> operands){
    return new domain::DomainContainer(operands);
};

Frame* Domain::mkFrame(std::string name, Space* space, Frame* parent){
    " + string.Join("", ParsePeirce.Instance.Spaces.Select(sp_ => {

                var hasName = sp_.MaskContains(Space.FieldType.Name);
                var hasDim = sp_.MaskContains(Space.FieldType.Dimension);
                //PhysSpaceExpression.ClassicalTimeLiteral (ClassicalTimeSpaceExpression.ClassicalTimeLiteral
                return "\n\tif(auto dc = dynamic_cast<domain::" + sp_.Name + @"*>(space)){
            if(auto df = dynamic_cast<domain::" + sp_.Prefix + @"Frame*>(parent)){
            auto child = this->mk" + sp_.Prefix + @"Frame(name, dc, df);
            return child;
        }
    }";
            })) + @"
};


";
            file += getspace;


            var mkTransformMap = @"MapSpace* Domain::mkMapSpace(Space* space, Frame* dom, Frame* cod){
    return new MapSpace(space, dom, cod);
};";

            file += "\n" + mkTransformMap + "\n";

            foreach (var sp in ParsePeirce.Instance.Spaces)
            {
                var hasName = sp.MaskContains(Space.FieldType.Name);
                var hasDim = sp.MaskContains(Space.FieldType.Dimension);

                var mkSpace = sp.Name + "* Domain::mk" + sp.Name + "(std::string key" +
                       (hasName && hasDim ? ",std::string name_, int dimension_" : hasName ? ",std::string name_" : hasDim ? ",int dimension_" : "") + @"){
    " + sp.Name + @"* s = new " + sp.Name + @"(" + (hasName && hasDim ? "name_, dimension_" : hasName ? "name_" : hasDim ? "dimension_" : "") + @");
    s->addFrame(new domain::" + sp.Prefix + @"Frame(""Standard"", s, nullptr));
    this->" + sp.Name + @"_vec.push_back(s);
    this->Space_vec.push_back(s);
    this->Space_map[key] = s;
    
    return s;
};";/*
                var addFrame = @"
    void addFrame(Frame* frame);";*/


                var getSVec = "//std::vector<" + sp.Name + "*> &Domain::get" + sp.Name + "Spaces() { return " + sp.Name + "_vec; }";

                file += "\n" + mkSpace + "\n\n" + getSVec + "\n";


                var mkFrame = sp.Prefix + "Frame* Domain::mk" + sp.Prefix + "Frame(std::string name, domain::" + sp.Name + @"* space, domain::" + sp.Prefix + @"Frame* parent){
    " + sp.Prefix + @"Frame* child = new domain::" + sp.Prefix + @"Frame(name, space, parent);
    space->addFrame(child);
    return child;
}
            ";
                file += "\n" + mkFrame + "\n";



                var addFrame = "void " + sp.Name + "::addFrame(" + sp.Prefix + @"Frame* frame){
    ((Space*)this)->addFrame(frame);
}";

                file += "\n" + addFrame + "\n";

                foreach (var spObj in sp.Category.Objects)
                {
                    var mkWithSpace = "";

                    if (spObj.IsTransform)
                    {
                        mkWithSpace = sp.Prefix + spObj.Name + "* Domain::mk" + sp.Prefix + spObj.Name + @"(MapSpace* sp){
    " + sp.Prefix + spObj.Name + @"* dom_ = new " + sp.Prefix + spObj.Name + @"(sp, {});
    this->" + sp.Prefix + spObj.Name + @"_vec.push_back(dom_);
    return dom_;
}
                ";

                    }
                    else
                    {
                        mkWithSpace = sp.Prefix + spObj.Name + "* Domain::mk" + sp.Prefix + spObj.Name + "(" + sp.Name + @"* sp){
    " + sp.Prefix + spObj.Name + @"* dom_ = new " + sp.Prefix + spObj.Name + @"(sp, {});
    this->" + sp.Prefix + spObj.Name + @"_vec.push_back(dom_);
    return dom_;
}
                ";
                    }

                    var mkSansSpace = sp.Prefix + spObj.Name + "* Domain::mk" + sp.Prefix + spObj.Name + @"(){
    " + sp.Prefix + spObj.Name + @"* dom_ = new " + sp.Prefix + spObj.Name + @"({});
    this->" + sp.Prefix + spObj.Name + @"_vec.push_back(dom_);   
    return dom_;
}";

                    file += "\n" + mkWithSpace + "\n" + mkSansSpace + "\n" ;
                    if (spObj.HasFrame)
                    {
                        var setFrame = "void " + sp.Prefix + spObj.Name + "::setFrame(" + sp.Prefix + @"Frame* frame){
    this->frame_ = frame;
};";
                        file += "\n" + setFrame;
                    }
                }
            }

            this.CppFile = file;
        }

        public override void GenHeader()
        {
            var header = @"
#ifndef BRIDGE_H
#define BRIDGE_H

#ifndef leanInferenceWildcard
#define leanInferenceWildcard ""_""
#endif

#include <cstddef>  
#include ""clang/AST/AST.h""
#include <vector>
#include <string>

#include ""AST.h""
#include ""Coords.h""

#include <g3log/g3log.hpp>


using namespace std;

/*
- Space
- Ident
- Expr
- Value
- Defn
*/

namespace domain{


";
            var file = header;

            //print all classes

            file += "\nclass Space;\nclass MapSpace;\nclass Frame;\nclass DomainObject;\nclass DomainContainer;\n";

            foreach (var sp in ParsePeirce.Instance.Spaces)
            {
                file += "\nclass " + sp.Name + ";\n";

                file += "\nclass " + sp.Prefix + "Frame;\n";

                foreach (var spObj in sp.Category.Objects)
                {
                    file += "\nclass " + sp.Prefix +  spObj.Name + ";\n";
                }
            }

            file += @"
            
// Definition for Domain class 

class Domain {
public:
// Space
	std::vector<Space*>& getSpaces();

";
            /*
             * ANDREW ! YOU NEED TO COME BACK HERE LATER AND CREATE "MAKE SPACE" APPROPRIATE FUNCTIONS!
             * */


            /*
             * 
	VecIdent* mkVecIdent(Space* s);
	VecIdent* mkVecIdent();
	std::vector<VecIdent *> &getVecIdents() { return idents;  }

             * */

            var getSpace = @"
    Space* getSpace(std::string key);

    DomainObject* mkDefaultDomainContainer();
    DomainObject* mkDefaultDomainContainer(std::initializer_list<DomainObject*> operands);
    DomainObject* mkDefaultDomainContainer(std::vector<DomainObject*> operands);
    Frame* mkFrame(std::string name, Space* space, Frame* parent);
";
            file += getSpace;

            var mkTransformMap = @"
    MapSpace* mkMapSpace(Space* space, Frame* dom, Frame* cod);";

            file += "\n" + mkTransformMap + "\n";

            foreach (var sp in ParsePeirce.Instance.Spaces)
            {
                var hasName = sp.MaskContains(Space.FieldType.Name);
                var hasDim = sp.MaskContains(Space.FieldType.Dimension);

                var mkSpace = sp.Name + "* mk" + sp.Name + "(std::string key " +
                       (hasName && hasDim ? ",std::string name_, int dimension_" : hasName ? ",std::string name_" : hasDim ? ",int dimension_" : "") + ");";
                var getSVec = "std::vector<" + sp.Name + "*> &get" + sp.Name + "Spaces() { return " + sp.Name + "_vec; }";

                file += "\n\t" + mkSpace + "\n\t" + getSVec + "\n";

                var mkFrame = sp.Prefix + "Frame* mk" + sp.Prefix + "Frame(std::string name," + (@" domain::" + sp.Name + @"* space") + @", domain::" + sp.Prefix + @"Frame* parent);";

                file += "\n\t" + mkFrame;

            

                foreach (var spObj in sp.Category.Objects)
                {
                    var mkWithSpace = "";
                    if (spObj.IsTransform)
                    {
                        mkWithSpace = sp.Prefix + spObj.Name + " * mk" + sp.Prefix + spObj.Name + "(MapSpace* sp);";

                    }
                    else
                    {
                        mkWithSpace = sp.Prefix + spObj.Name + " * mk" + sp.Prefix + spObj.Name + "(" + sp.Name + "* sp);";
                    }
                    var mkSansSpace = sp.Prefix + spObj.Name + "* mk" + sp.Prefix + spObj.Name + "();";
                    var getVec = "std::vector<" + sp.Prefix + spObj.Name + "*> &get" + sp.Prefix + spObj.Name + "s() { return " + sp.Prefix + spObj.Name + "_vec; }";

                    file += "\n\t" + mkWithSpace + "\n\t" + mkSansSpace + "\n\t" + getVec + "\n";
                }
            }

            file += "\nprivate:\n";

            var smap = "std::unordered_map<std::string, Space*> Space_map;";
            var gets = "std::vector<Space*> Space_vec;";

            file += "\n\t" + smap + "\n\t"+gets;

            foreach (var sp in ParsePeirce.Instance.Spaces)
            {
                var getSVec = "std::vector<" + sp.Name + "*> " + sp.Name + "_vec;";

                file += "\n\t" + getSVec;

                foreach (var spObj in sp.Category.Objects)
                {
                    var getVec = "std::vector<" + sp.Prefix + spObj.Name + "*>" + sp.Prefix + spObj.Name + "_vec;";

                    file += "\n\t" + getVec;
                }
            }

            file += "\n};";

            /*
             * 
             * /*
            class Space {
            public:
	            Space() {};
	            std::string toString() const {
		            return "This is a mixin interface"; 
	            }

              private:
            };

            class MapSpace {
            public:
	            MapSpace() {}
	            MapSpace(domain::Space domain, domain::Space codomain) : domain_{domain}, codomain_{codomain} {}
	            std::string getName() const;
	            std::string toString() const {
		            return getName(); 
	            }
	            domain::Space domain_;
	            domain::Space codomain_;
            };*/

            file += "\n\n" + @"
class Space {
public:
	Space() {};
    virtual ~Space(){};
	virtual std::string toString() const {
		return ""Not implemented""; 
	}
    virtual std::string getName() const {
        return ""Not implemented"";
    }

    std::vector<Frame*> getFrames() const { return this->frames_; };
    void addFrame(Frame* frame);

protected:
    std::vector<Frame*> frames_;
};

class Frame {
public:
    Frame(std::string name, Space* space, Frame* parent) : name_(name), space_(space), parent_(parent) {};
    Frame() {};
    virtual ~Frame(){};
    virtual std::string toString() const {
        return ""This is a mixin interface"";
    }

    Frame* getParent() const{ return parent_; };
    void setParent(Frame* parent);

    std::string getName() const { return name_; };

    Space* getSpace() const { return space_; };

protected:
    Frame* parent_;
    Space* space_;
    std::string name_;

};
";

            file += "\n\n" + @"
//pretend this is a union
class MapSpace : public Space {
public:
	MapSpace() {}
	MapSpace(domain::Space* domain, domain::Space* codomain) : domain_(domain), codomain_(codomain), change_space_{true}, change_frame_{true} {};

    MapSpace(domain::Space* domain, domain::Space* codomain, Frame* domain_frame, Frame* codomain_frame) 
        : domain_(domain), codomain_(codomain), domain_frame_(domain_frame), codomain_frame_(codomain_frame), change_space_{true}, change_frame_{true} {};

    MapSpace(domain::Space* domain, Frame* domain_frame, Frame* codomain_frame)
        : domain_(domain), codomain_(nullptr), domain_frame_(domain_frame), codomain_frame_(codomain_frame), change_space_{false}, change_frame_{true} {};
	std::string toString() const {
        return ""@@Map("" + this->getName() + "")"";
    };
    std::string getName() const{
        if(change_space_){
            if(change_frame_){
                return domain_->getName()+"".""+domain_frame_->getName()+""->""+codomain_->getName()+"".""+codomain_frame_->getName();
            }
            else{
                return domain_->getName()+""->""+codomain_->getName();
            }
        }
        else{
            if(change_frame_){
                return domain_->getName()+"".""+domain_frame_->getName()+""->""+domain_->getName()+"".""+codomain_frame_->getName();
            }
        }
        return """";
    }
        

	domain::Space* domain_;
	domain::Frame* domain_frame_;

    domain::Space* codomain_;
    domain::Frame* codomain_frame_;
    
    bool change_space_;
    bool change_frame_;
};";

            file += @"
class DomainObject {
public:
    DomainObject(std::initializer_list<DomainObject*> args);
    DomainObject(std::vector<DomainObject*> operands) : operands_(operands) {};
    DomainObject(){};
    DomainObject* getOperand(int i);
    std::vector<DomainObject*> getOperands() const { return operands_; };
    void setOperands(std::vector<DomainObject*> operands);
    virtual std::string toString();
    friend class DomainObject; 
  
protected:
    std::vector<DomainObject*> operands_;
    int operand_count;
};

class DomainContainer : public DomainObject{
public:
        DomainContainer() : DomainObject(), inner_(nullptr) {};
        DomainContainer(DomainObject* inner) : inner_(inner) {};
        DomainContainer(std::initializer_list<DomainObject*> operands);
        DomainContainer(std::vector<DomainObject*> operands);
        virtual std::string toString() override;// { this->hasValue() ? this->inner_->toString() : ""No Provided Interpretation""; }
        DomainObject* getValue() const { return this->inner_; }
        void setValue(DomainObject* obj);
        bool hasValue();
        

private:
DomainObject* inner_;
};
";


            foreach (var sp in ParsePeirce.Instance.Spaces)
            {
                var hasName = sp.MaskContains(Space.FieldType.Name);
                var hasDim = sp.MaskContains(Space.FieldType.Dimension);
                var spclass = "\n\nclass " + sp.Name + @" : public Space {
public:
	" + sp.Name + @"() : name_("""") {};
	" + (hasName ? sp.Name + @"(std::string name) : name_(name) {};" : "") + @"
	" + (hasName && hasDim ? sp.Name + @"(std::string name, int dimension) : name_(name), dimension_(dimension) {};" : "" ) + @"
	" + (hasName ? "std::string getName() const override { return name_; }; " : "") + @"
	" + (hasDim ? "int getDimension() const { return dimension_; }; " : "") + @"
    void addFrame(" + sp.Prefix + @"Frame* frame);
	std::string toString() const override {
		return ""@@" + sp.Name + @"  "" " + (hasName ? "+ getName() " : "") + @"  + ""(""" + (hasDim ? "+ std::to_string(getDimension())" : "") + @" + "")"";" + @" 
	}

private:
    " + (hasName ? "std::string name_;" : "") + @"
    " + (hasDim ? "int dimension_;" : "") + @"
};";

                file += spclass;
                
                var spframeclass = "\n\nclass " + sp.Prefix + @"Frame : public Frame {
public:
	" + sp.Prefix + @"Frame(std::string name,  " + sp.Name + @"* space, " + sp.Prefix + @"Frame* parent) : Frame(name, space, parent) {};
	std::string toString() const override {
        std::string parentName = ((" + sp.Name + @"*)this->space_)->getName();
		return ""@@" + sp.Prefix + @"Frame  "" + this->getName() + ""("" + parentName + (this->parent_? "","" + parentName + ""."" + this->parent_->getName() : """") + "")"";
	}

private:
};";

                file += "\n" + spframeclass + "\n";

                foreach (var spObj in sp.Category.Objects)
                {
                    var spobjclass = @"class " + sp.Prefix +  spObj.Name + @" : public DomainObject {
public:
    " + sp.Prefix + spObj.Name + @"(" + (spObj.IsTransform ? "MapSpace" : sp.Name) + @"* s, std::initializer_list<DomainObject*> args) : 
			domain::DomainObject(args), space_(s)  {}
    " + sp.Prefix + spObj.Name + @"(std::initializer_list<DomainObject*> args ) :
	 		domain::DomainObject(args) {}
	virtual ~" + sp.Prefix + spObj.Name + @"(){}
    std::string toString() override {
        return ""@@" + sp.Prefix + spObj.Name + @"("" + " + (sp.MaskContains(Space.FieldType.Name) ? @"(space_?space_->getName():""Missing Space"")" : "") + (spObj.HasFrame ? "+\",\"+(frame_?frame_->getName():\"\")" : "") + @" + "")"";
    }
    
    " + (spObj.HasFrame ? (sp.Prefix + @"Frame* getFrame() const { return this->frame_; };") : "") + @"
    " + (spObj.HasFrame ? (@"void setFrame(" + sp.Prefix + @"Frame* frame);") : "") + @"
private:
    " + (!spObj.IsMap && !spObj.IsTransform ? sp.Name + @"* space_;" : "") + @" 
    " + (spObj.HasFrame?(sp.Prefix + @"Frame* frame_;"):"") + @"
    " + (spObj.IsMap || spObj.IsTransform ? (@"MapSpace* space_;") : "") + @"
};
";
                    file += "\n\n" +  spobjclass;
                }
            }


            file += @"
} // end namespace

#endif";
            this.HeaderFile = file;
        }
    }
}

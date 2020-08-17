using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen.Generators
{
    public class GenDomain : GenBase
    {
        public override string GetCPPLoc()
        {
            return Directory.GetParent(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName).FullName + @"\symlinkme\Domain.cpp";
        }

        public override string GetHeaderLoc()
        {
            return Directory.GetParent(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName).FullName + @"\symlinkme\Domain.h";
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

                //var hasName = sp_.MaskContains(Space.FieldType.Name);
               // var hasDim = sp_.MaskContains(Space.FieldType.Dimension);
                //PhysSpaceExpression.ClassicalTimeLiteral (ClassicalTimeSpaceExpression.ClassicalTimeLiteral
                return "\n\tif(auto dc = dynamic_cast<domain::" + sp_.Name + @"*>(space)){
            if(auto df = dynamic_cast<domain::" + sp_.Name + @"Frame*>(parent)){
            auto child = this->mk" + sp_.Name + @"Frame(name, dc, df);
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
                //var hasName = sp.MaskContains(Space.FieldType.Name);
                //var hasDim = sp.MaskContains(Space.FieldType.Dimension);
                if(sp.IsDerived)
                {
                    var mkSpace = sp.Name + "* Domain::mk" + sp.Name + @"(std::string key,std::string name_, Space* base1, Space* base2){
        " + sp.Name + @"* s = new " + sp.Name + @"(name_, base1, base2);
        s->addFrame(new domain::" + sp.Name + @"Frame(""Standard"", s, nullptr));
        this->" + sp.Name + @"_vec.push_back(s);
        this->Space_vec.push_back(s);
        this->Space_map[key] = s;
    
        return s;
    };";/*
                var addFrame = @"
    void addFrame(Frame* frame);";*/

                    var getSVec = "//std::vector<" + sp.Name + "*> &Domain::get" + sp.Name + "Spaces() { return " + sp.Name + "_vec; }";

                    file += "\n" + mkSpace + "\n\n" + getSVec + "\n";
                }
                else
                {
                    var mkSpace = sp.Name + "* Domain::mk" + sp.Name + "(std::string key" +
                           (sp.DimensionType == Space.DimensionType_.ANY ? ",std::string name_, int dimension_" :
                           ",std::string name_") + @"){
        " + sp.Name + @"* s = new " + sp.Name + @"(" + (sp.DimensionType == Space.DimensionType_.ANY ? "name_, dimension_"
                                                    : "name_") + @");
        s->addFrame(new domain::" + sp.Name + @"Frame(""Standard"", s, nullptr));
        this->" + sp.Name + @"_vec.push_back(s);
        this->Space_vec.push_back(s);
        this->Space_map[key] = s;
    
        return s;
    };";/*
                var addFrame = @"
    void addFrame(Frame* frame);";*/


                    var getSVec = "//std::vector<" + sp.Name + "*> &Domain::get" + sp.Name + "Spaces() { return " + sp.Name + "_vec; }";

                    file += "\n" + mkSpace + "\n\n" + getSVec + "\n";
                }
                    


                var mkFrame = sp.Name + "Frame* Domain::mk" + sp.Name + "Frame(std::string name, domain::" + sp.Name + @"* space, domain::" + sp.Name + @"Frame* parent){
    " + sp.Name + @"Frame* child = new domain::" + sp.Name + @"Frame(name, space, parent);
    space->addFrame(child);
    return child;
}
            ";
                file += "\n" + mkFrame + "\n";



                var addFrame = "void " + sp.Name + "::addFrame(" + sp.Name + @"Frame* frame){
    ((Space*)this)->addFrame(frame);
}";

                file += "\n" + addFrame + "\n";
                /*
                foreach (var spObj in sp.Category.Objects)
                {
                    var mkWithSpace = "";

                    if (spObj.IsTransform)
                    {
                        mkWithSpace = @"
template <class ValueType, int ValueCount>
" +
                            sp.Name + spObj.Name + "<ValueType,ValueCount>* Domain::mk" + sp.Name + spObj.Name + @"(MapSpace* sp, ValueType* values[ValueCount]){
    " + sp.Name + spObj.Name + @"<ValueType,ValueCount>* dom_ = new " + sp.Name + spObj.Name + @"<ValueType,ValueCount>(sp, {});
    dom_->setValues(values);
    //this->" + sp.Name + spObj.Name + @"_vec.push_back(dom_);
    int i = 0;
    for(auto val : values){
        dom_->setValue(values[i],i++);
    }
    return dom_;
}
                ";

                    }
                    else
                    {
                        mkWithSpace = @"
template <class ValueType, int ValueCount>
" + sp.Name + spObj.Name + "<ValueType,ValueCount>* Domain::mk" + sp.Name + spObj.Name + "(" + sp.Name + @"* sp, ValueType* values[ValueCount]){
    " + sp.Name + spObj.Name + @"<ValueType,ValueCount>* dom_ = new " + sp.Name + spObj.Name + @"<ValueType,ValueCount>(sp, {});
    //dom_->setValues(values);
    //this->" + sp.Name + spObj.Name + @"_vec.push_back(dom_);
    int i = 0;
    for(auto val : values){
        dom_->setValue(values[i],i++);
    }

    return dom_;
}
                ";
                    }

                    var mkSansSpace = @"
template <class ValueType, int ValueCount>
" +
                            sp.Name + spObj.Name + "<ValueType,ValueCount>* Domain::mk" + sp.Name + spObj.Name + @"(){
    " + sp.Name + spObj.Name + @"<ValueType,ValueCount>* dom_ = new " + sp.Name + spObj.Name + @"<ValueType,ValueCount>({});
    //this->" + sp.Name + spObj.Name + @"_vec.push_back(dom_);
    /*int i = 0;
    for(auto val : values){
        dom_->setValue(values[i],i++);
    } 
    return dom_;
}";

                    file += "\n" + mkWithSpace + "\n" + mkSansSpace + "\n" ;
                    if (spObj.HasFrame)
                    {
                        var setFrame = @"
template <class ValueType, int ValueCount>
" + "void " + sp.Name + spObj.Name + "<ValueType,ValueCount>::setFrame(" + sp.Name + @"Frame* frame){
    this->frame_ = frame;
};";
                        file += "\n" + setFrame;
                    }
                }*/
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
#include <memory>

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

            file += "\nclass Space;\nclass DerivedSpace;\nclass MapSpace;\nclass Frame;\nclass DomainObject;\nclass DomainContainer;\ntemplate<typename ValueType,int ValueCount>\nclass ValueObject;\n";

            foreach (var sp in ParsePeirce.Instance.Spaces)
            {
                file += "\nclass " + sp.Name + ";\n";

                file += "\nclass " + sp.Name + "Frame;\n";

                foreach (var spObj in sp.Category.Objects)
                {
                    file += "\ntemplate<typename ValueType,int ValueCount>\nclass " + sp.Name +  spObj.Name + ";\n";
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
                //var hasName = sp.MaskContains(Space.FieldType.Name);
                //var hasDim = sp.MaskContains(Space.FieldType.Dimension);
                if (sp.IsDerived)
                {
                    var mkSpace = sp.Name + "* mk" + sp.Name + "(std::string key, std::string name_,Space* base1, Space* base2" + ");";
                    var getSVec = "std::vector<" + sp.Name + "*> &get" + sp.Name + "Spaces() { return " + sp.Name + "_vec; }";

                    file += "\n\t" + mkSpace + "\n\t" + getSVec + "\n";
                }
                else
                {
                    var mkSpace = sp.Name + "* mk" + sp.Name + "(std::string key " +
                           (sp.DimensionType == Space.DimensionType_.ANY ? ",std::string name_, int dimension_" :
                                ",std::string name_") + ");";
                    var getSVec = "std::vector<" + sp.Name + "*> &get" + sp.Name + "Spaces() { return " + sp.Name + "_vec; }";

                    file += "\n\t" + mkSpace + "\n\t" + getSVec + "\n";
                }

                var mkFrame = sp.Name + "Frame* mk" + sp.Name + "Frame(std::string name," + (@" domain::" + sp.Name + @"* space") + @", domain::" + sp.Name + @"Frame* parent);";

                file += "\n\t" + mkFrame;

                foreach (var spObj in sp.Category.Objects)
                {
                    var mkWithSpace = "";

                    if (spObj.IsTransform)
                    {
                        mkWithSpace = @"
template <class ValueType, int ValueCount>
" +
                            sp.Name + spObj.Name + "<ValueType,ValueCount>* mk" + sp.Name + spObj.Name + @"(MapSpace* sp, std::shared_ptr<ValueType> values[ValueCount]){
    " + sp.Name + spObj.Name + @"<ValueType,ValueCount>* dom_ = new " + sp.Name + spObj.Name + @"<ValueType,ValueCount>(sp, {});
    //((ValueObject<ValueType,ValueCount>)(dom_))->setValues(values);
    //this->" + sp.Name + spObj.Name + @"_vec.push_back(dom_);
    for(int i = 0; i < ValueCount;i++){
        dom_->setValue(values[i],i);
    }
    return dom_;
}
                ";

                    }
                    else
                    {
                        mkWithSpace = @"
template <class ValueType, int ValueCount>
" + sp.Name + spObj.Name + "<ValueType,ValueCount>* mk" + sp.Name + spObj.Name + "(" + sp.Name + @"* sp, std::shared_ptr<ValueType> values[ValueCount]){
    " + sp.Name + spObj.Name + @"<ValueType,ValueCount>* dom_ = new " + sp.Name + spObj.Name + @"<ValueType,ValueCount>(sp, {});
    //dom_->setValues(values);
    //this->" + sp.Name + spObj.Name + @"_vec.push_back(dom_);
    for(int i = 0; i < ValueCount;i++){
        dom_->setValue(values[i],i);
    }

    return dom_;
}
                ";
                    }

                    var mkSansSpace = @"
template <class ValueType, int ValueCount>
" +
                            sp.Name + spObj.Name + "<ValueType,ValueCount>* mk" + sp.Name + spObj.Name + @"(){
    " + sp.Name + spObj.Name + @"<ValueType,ValueCount>* dom_ = new " + sp.Name + spObj.Name + @"<ValueType,ValueCount>({});
    //this->" + sp.Name + spObj.Name + @"_vec.push_back(dom_);
    /*int i = 0;
    for(auto val : values){
        dom_->setValue(values[i],i++);
    } */  
    return dom_;
}";

                    file += "\n" + mkWithSpace + "\n" + mkSansSpace + "\n";
                    /*if (spObj.HasFrame)
                    {
                        var setFrame = @"
template <class ValueType, int ValueCount>
" + "void " + sp.Name + spObj.Name + "<ValueType,ValueCount>::setFrame(" + sp.Name + @"Frame* frame){
    this->frame_ = frame;
};";
                        file += "\n" + setFrame;
                    }*/
                }
                /*
                foreach (var spObj in sp.Category.Objects)
                {
                    var mkWithSpace = "";
                    if (spObj.IsTransform)
                    {
                        mkWithSpace = @"
template <class ValueType, int ValueCount>
" +
                            sp.Name + spObj.Name + "<ValueType,ValueCount> * mk" + sp.Name + spObj.Name + "(MapSpace* sp,ValueType* values[ValueCount]);";

                    }
                    else
                    {
                        mkWithSpace = @"
template <class ValueType, int ValueCount>
" +
                            sp.Name + spObj.Name + "<ValueType,ValueCount> * mk" + sp.Name + spObj.Name + "(" + sp.Name + "* sp, ValueType* values[ValueCount]);";
                    }
                    var mkSansSpace = @"
template <class ValueType, int ValueCount>
" +
                            sp.Name + spObj.Name + "<ValueType,ValueCount> * mk" + sp.Name + spObj.Name + "();";
                    // var getVec = "std::vector<" + sp.Name + spObj.Name + "*> &get" + sp.Name + spObj.Name + "s() { return " + sp.Name + spObj.Name + "_vec; }";

                    file += "\n\t" + mkWithSpace + "\n\t" + mkSansSpace + "\n\t";// + getVec + "\n";
                }
                */

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
                   // var getVec = "std::vector<" + sp.Name + spObj.Name + "*>" + sp.Name + spObj.Name + "_vec;";

                   // file += "\n\t" + getVec;
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
    Space(string name, int dimension) : name_(name), dimension_(dimension) {};
    virtual ~Space(){};
	virtual std::string toString() const {
		return ""Not implemented""; 
	}
    virtual std::string getName() const {
        return this->name_;
    }
    int getDimension() const {
        return this->dimension_;
    }

    std::vector<Frame*> getFrames() const { return this->frames_; };
    void addFrame(Frame* frame);

protected:
    std::vector<Frame*> frames_;
    std::string name_;
    int dimension_;

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

class DerivedSpace : public Space {
public:
    DerivedSpace() {};
    DerivedSpace(string name, Space* base1, Space* base2) :  Space(name, base1->getDimension()*base2->getDimension()), base_1(base1), base_2(base2) {
        
    }

    Space* getBase1() const {
        return this->base_1;
    }

    Space* getBase2() const {
        return this->base_2;
    }

    virtual ~DerivedSpace(){};
	virtual std::string toString() const override {
		return ""Not implemented""; 
	}

protected:
    Space* base_1;
    Space* base_2;
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


template <typename ValueType, int ValueCount>
class ValueObject : public DomainObject {
public:
    // ValueCoords() : DomainObject() {};

    ValueObject() : DomainObject(){
        for(int i = 0; i<ValueCount;i++){
            this->values_[i] = nullptr;
        }
    };

    ValueObject(std::initializer_list<DomainObject*> args) :  DomainObject(args) {
        for(int i = 0; i<ValueCount;i++){
            this->values_[i] = nullptr;
        }

    }
    ValueObject(std::vector<DomainObject*> operands) : DomainObject(operands) {
        for(int i = 0; i<ValueCount;i++){
            this->values_[i] = nullptr;
        }

    }

    ~ValueObject() {
        for(auto v : this->values_){
           // delete v;
        }
    }

    ValueObject(ValueType* values...) : DomainObject() {
        int i = 0;
        for(auto val : {values}){
            if(i == ValueCount)
                throw ""Out of Range"";
            this->values_[i++] = std::make_shared<ValueType>(*val);

        }
    }

    ValueObject(std::initializer_list<DomainObject*> args, ValueType* values...) : ValueObject(values), DomainObject(args)
    {
    }

    ValueObject(std::vector<DomainObject*> operands, ValueType* values...) : ValueObject(values), DomainObject(operands)
    {
        int i = 0;
        for (auto val : { values}){
            if (i == ValueCount)
                throw ""Out of Range"";
            this->values_[i++] = std::make_shared<ValueType>(*val);

        }
    }

    virtual std::string toString() override {
        std::string ret = ""Value=<"";
    int i = 1;
        for(auto val : this->values_){
        ret += (val ? std::to_string(*val) : ""UNK"") + (i++ == ValueCount ? """" : "","");
    }
        return ret + "">"";
    }

    ValueObject(ValueType* values[ValueCount]) : DomainObject(), values_(values) { };

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
    protected:
    std::shared_ptr<ValueType> values_[ValueCount];
};

";


            foreach (var sp in ParsePeirce.Instance.Spaces)
            {
                //var hasName = sp.MaskContains(Space.FieldType.Name);
                //var hasDim = sp.MaskContains(Space.FieldType.Dimension);
                if (sp.IsDerived)
                {

                    var spclass = "\n\nclass " + sp.Name + @" : public DerivedSpace {
    public:
	    " + @"
        " + sp.Name + @"(std::string name, Space* base1, Space* base2) : DerivedSpace(name, base1, base2) {};
        void addFrame(" + sp.Name + @"Frame* frame);
	    std::string toString() const override {
		    return ""@@" + sp.Name + @"  "" " + "+ getName() " + @"  + ""("" + this->base_1->getName() + "","" + this->base_2->getName() + "")"";" + @" 
	    }

    private:
    };";
                    file += spclass;
                }
                else
                {
                    var spclass = "\n\nclass " + sp.Name + @" : public Space {
    public:
	    " + sp.Name + (sp.DimensionType == Space.DimensionType_.Fixed ? @"(std::string name) : Space(name, " + sp.FixedDimension + @") {};" : "")
        + (sp.DimensionType == Space.DimensionType_.ANY ? @"(std::string name, int dimension) : Space(name, dimension) {};" : "") + @"
	    " + "std::string getName() const override { return name_; }; " + @"
	    " + @"
        void addFrame(" + sp.Name + @"Frame* frame);
	    std::string toString() const override {
		    return ""@@" + sp.Name + @"  "" " + "+ getName() " + @"  + ""(""" + (sp.DimensionType == Space.DimensionType_.ANY ? "+ std::to_string(getDimension())" : "") + @" + "")"";" + @" 
	    }

    private:
    };";
                    file += spclass;
                }
                
                var spframeclass = "\n\nclass " + sp.Name + @"Frame : public Frame {
public:
	" + sp.Name + @"Frame(std::string name,  " + sp.Name + @"* space, " + sp.Name + @"Frame* parent) : Frame(name, space, parent) {};
	std::string toString() const override {
        std::string parentName = ((" + sp.Name + @"*)this->space_)->getName();
		return ""@@" + sp.Name + @"Frame  "" + this->getName() + ""("" + parentName + (this->parent_? "","" + parentName + ""."" + this->parent_->getName() : """") + "")"";
	}

private:
};";

                file += "\n" + spframeclass + "\n";
                
                foreach (var spObj in sp.Category.Objects)
                {
                    var spobjclass = @"

template <class ValueType, int ValueCount>
class " + sp.Name +  spObj.Name + @" : public ValueObject<ValueType,ValueCount> {
public:
    " + sp.Name + spObj.Name + @"(" + (spObj.IsTransform ? "MapSpace" : sp.Name) + @"* s, std::initializer_list<DomainObject*> args) : 
			ValueObject<ValueType,ValueCount>::ValueObject(args), space_(s)  {}
    " + sp.Name + spObj.Name + @"(std::initializer_list<DomainObject*> args ) :
	 		ValueObject<ValueType,ValueCount>::ValueObject(args) {}
	virtual ~" + sp.Name + spObj.Name + @"(){}
    std::string toString() override {
        return ""@@" + sp.Name + spObj.Name 
        + @"("" + " + @"(space_?space_->getName():""Missing Space"")" 
        + "+\",\"+ValueObject<ValueType,ValueCount>::toString()" 
        + (spObj.HasFrame ? "+\",\"+(frame_?frame_->getName():\"\")" : "") + @" + "")"";
    }
    
    " + (spObj.HasFrame ? (sp.Name + @"Frame* getFrame() const { return this->frame_; };") : "") + @"
    " + (spObj.HasFrame ? (@"void setFrame(" + sp.Name + @"Frame* frame){
            this->frame_ = frame;
        };") : "") + @"
private:
    " + (!spObj.IsMap && !spObj.IsTransform ? sp.Name + @"* space_;" : "") + @" 
    " + (spObj.HasFrame?(sp.Name + @"Frame* frame_;"):"") + @"
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

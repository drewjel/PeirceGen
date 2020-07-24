import ..physics.physlib

noncomputable theory





abbreviation RealScalarDefault : RealScalar := RealScalar.mk 0
--for peirce:
def Geometric3ScalarDefault (sp : PhysSpace physicalDimension.distance 3) :  Geometric3Scalar := Geometric3Scalar.mk 0 sp
def Velocity3ScalarDefault (sp : PhysSpace velocity 3) :  Velocity3Scalar := Velocity3Scalar.mk 0 sp
def TimeScalarDefault (sp : PhysSpace physicalDimension.time 1) :  TimeScalar := TimeScalar.mk 0 sp





abbreviation NatDefault := (0 : ℕ)
abbreviation RationalDefault := (0 : ℚ)





--7/14 why is aff_vec in this file? - needs to go

def Geometric3VectorDefault : (PhysSpace physicalDimension.distance 3) → Geometric3Vector
| p := Geometric3Vector.mk p (aff_vec.mk [0,0,0,0] rfl rfl) p.std_frame

def Velocity3VectorDefault : (PhysSpace velocity 3) → Velocity3Vector
| p := Velocity3Vector.mk p (aff_vec.mk [0,0,0,0] rfl rfl) p.std_frame

def TimeVectorDefault : (PhysSpace physicalDimension.time 1) → TimeVector
| p := TimeVector.mk p (aff_vec.mk [0,0] rfl rfl) p.std_frame


def Geometric3PointDefault : (PhysSpace physicalDimension.distance 3) → Geometric3PointStruct
| p := Geometric3Point p 0 0 0
--abbreviation Velocity3Point_zero := Velocity3PointStruct.mk vel (aff_pt.mk [1,0,0,0] rfl rfl) std 
def TimePointDefault : (PhysSpace physicalDimension.time 1) → TimePointStruct
| p := TimePoint p 0



inductive RealScalarVar : Type 
| mk : ℕ → RealScalarVar
inductive Geometric3ScalarVar : Type 
| mk : ℕ → Geometric3ScalarVar
inductive TimeScalarVar : Type 
| mk : ℕ → TimeScalarVar
inductive Velocity3ScalarVar : Type 
| mk : ℕ → Velocity3ScalarVar
inductive Geometric3VectorVar : Type 
| mk : ℕ → Geometric3VectorVar
inductive TimeVectorVar : Type 
| mk : ℕ → TimeVectorVar
inductive Velocity3VectorVar : Type 
| mk : ℕ → Velocity3VectorVar
inductive Geometric3PointVar : Type 
| mk : ℕ → Geometric3PointVar
inductive TimePointVar : Type 
| mk : ℕ → TimePointVar
inductive Velocity3PointVar : Type 
| mk : ℕ → Velocity3PointVar



notation !n := RealScalarVar.mk n
notation !n := Geometric3ScalarVar.mk n
notation !n := TimeScalarVar.mk n
notation !n := Velocity3ScalarVar.mk n
notation !n := Geometric3VectorVar.mk n
notation !n := TimeVectorVar.mk n
notation !n := Velocity3VectorVar.mk n
notation !n := Geometric3PointVar.mk n
notation !n := TimePointVar.mk n


--7/12 Andrew - NO, WE DONT WANT THIS!!!
--structure Velocity3PointVar : Type :=
--	mk :: (n : ℕ)
/-
GeometricLit.mk Geometric3Vector → Geom3Expresison
if passing in 3 "Geometric3ScalarExpressions"

Geometriclit.mk Geometric3Vector.mk EVAL(SCALAEXP1) EVAL(SCALAR....


-/


mutual inductive RealScalarExpression, Geometric3ScalarExpression, 
	TimeScalarExpression, Velocity3ScalarExpression, Geometric3VectorExpression, 
	Velocity3VectorExpression, TimeVectorExpression, Geometric3PointExpression, 
	/-Velocity3PointExpression,-/ TimePointExpression
with RealScalarExpression : Type
| RealLitScalar : RealScalar → RealScalarExpression
| RealVarScalar : RealScalarVar → RealScalarExpression
| RealAddScalarScalar : RealScalarExpression → RealScalarExpression → RealScalarExpression
| RealSubScalarScalar : RealScalarExpression → RealScalarExpression → RealScalarExpression
| RealMulScalarScalar : RealScalarExpression → RealScalarExpression → RealScalarExpression
| RealDivScalarScalar : RealScalarExpression → RealScalarExpression → RealScalarExpression
| RealNegScalar : RealScalarExpression → RealScalarExpression
| RealInvScalar : RealScalarExpression → RealScalarExpression
| RealParenScalar : RealScalarExpression → RealScalarExpression
with Geometric3ScalarExpression : Type
| GeometricLitScalar : Geometric3Scalar → Geometric3ScalarExpression
| GeometricVarScalar : Geometric3ScalarVar → Geometric3ScalarExpression
| GeometricAddScalarScalar : Geometric3ScalarExpression → Geometric3ScalarExpression → Geometric3ScalarExpression
| GeometricSubScalarScalar : Geometric3ScalarExpression → Geometric3ScalarExpression → Geometric3ScalarExpression
| GeometricMulScalarScalar : Geometric3ScalarExpression → Geometric3ScalarExpression → Geometric3ScalarExpression
| GeometricDivScalarScalar : Geometric3ScalarExpression → Geometric3ScalarExpression → Geometric3ScalarExpression
| GeometricNegScalar : Geometric3ScalarExpression → Geometric3ScalarExpression
| GeometricInvScalar : Geometric3ScalarExpression → Geometric3ScalarExpression
| GeometricParenScalar : Geometric3ScalarExpression → Geometric3ScalarExpression
| GeometricNormVector : Geometric3VectorExpression → Geometric3ScalarExpression
with TimeScalarExpression : Type
| TimeLitScalar : TimeScalar → TimeScalarExpression
| TimeVarScalar : TimeScalarVar → TimeScalarExpression
| TimeAddScalarScalar : TimeScalarExpression → TimeScalarExpression → TimeScalarExpression
| TimeSubScalarScalar : TimeScalarExpression → TimeScalarExpression → TimeScalarExpression
| TimeMulScalarScalar : TimeScalarExpression → TimeScalarExpression → TimeScalarExpression
| TimeDivScalarScalar : TimeScalarExpression → TimeScalarExpression → TimeScalarExpression
| TimeNegScalar : TimeScalarExpression → TimeScalarExpression
| TimeInvScalar : TimeScalarExpression → TimeScalarExpression
| TimeParenScalar : TimeScalarExpression → TimeScalarExpression
| TimeNormVector : TimeVectorExpression → TimeScalarExpression
with Velocity3ScalarExpression : Type
| VelocityLitScalar : Velocity3Scalar → Velocity3ScalarExpression 
| VelocityVarScalar : Velocity3ScalarVar → Velocity3ScalarExpression
| VelocityAddScalarScalar : Velocity3ScalarExpression → Velocity3ScalarExpression → Velocity3ScalarExpression 
| VelocitySubScalarScalar : Velocity3ScalarExpression → Velocity3ScalarExpression → Velocity3ScalarExpression
| VelocityMulScalarScalar : Velocity3ScalarExpression → Velocity3ScalarExpression → Velocity3ScalarExpression
| VelocityDivScalarScalar : Velocity3ScalarExpression → Velocity3ScalarExpression → Velocity3ScalarExpression
| VelocityNegScalar : Velocity3ScalarExpression → Velocity3ScalarExpression
| VelocityInvScalar : Velocity3ScalarExpression → Velocity3ScalarExpression
| VelocityParenScalar : Velocity3ScalarExpression → Velocity3ScalarExpression
| VelocityNormVector : Velocity3VectorExpression → Velocity3ScalarExpression
with Geometric3VectorExpression : Type
| Geometric3LitVector : Geometric3Vector → Geometric3VectorExpression
| Geometric3VarVector : Geometric3VectorVar → Geometric3VectorExpression
| Geometric3AddVectorVector : Geometric3VectorExpression → Geometric3VectorExpression → Geometric3VectorExpression
| Geometric3NegVector : Geometric3VectorExpression → Geometric3VectorExpression
| Geometric3MulScalarVector : RealScalarExpression → Geometric3VectorExpression → Geometric3VectorExpression
| Geometric3MulVectorScalar : Geometric3VectorExpression → RealScalarExpression → Geometric3VectorExpression
| Geometric3SubPointPoint : Geometric3PointExpression → Geometric3PointExpression → Geometric3VectorExpression
| Geometric3ParenVector : Geometric3VectorExpression → Geometric3VectorExpression
| Geometric3CoordVector : Geometric3ScalarExpression → Geometric3ScalarExpression → Geometric3ScalarExpression → Geometric3VectorExpression
with Velocity3VectorExpression : Type
| Velocity3LitVector : Velocity3Vector → Velocity3VectorExpression
| Velocity3VarVector : Velocity3VectorVar → Velocity3VectorExpression
| Velocity3AddVectorVector : Velocity3VectorExpression → Velocity3VectorExpression → Velocity3VectorExpression
| Velocity3NegVector : Velocity3VectorExpression → Velocity3VectorExpression
| Velocity3MulScalarVector : RealScalarExpression → Velocity3VectorExpression → Velocity3VectorExpression
| Velocity3MulVectorScalar : Velocity3VectorExpression → RealScalarExpression → Velocity3VectorExpression
| Velocity3ParenVector : Velocity3VectorExpression  → Velocity3VectorExpression
| Velocity3CoordVector : Velocity3ScalarExpression → Velocity3ScalarExpression → Velocity3ScalarExpression → Velocity3VectorExpression
with TimeVectorExpression : Type
| TimeLitVector : TimeVector → TimeVectorExpression
| TimeVarVector : TimeVectorVar → TimeVectorExpression
| TimeAddVectorVector : TimeVectorExpression → TimeVectorExpression → TimeVectorExpression
| TimeNegVector : TimeVectorExpression → TimeVectorExpression
| TimeMulScalarVector : RealScalarExpression → TimeVectorExpression → TimeVectorExpression
| TimeMulVectorScalar : TimeVectorExpression → RealScalarExpression → TimeVectorExpression 
| TimeSubPointPoint : TimePointExpression → TimePointExpression → TimeVectorExpression
| TimeParenVector : TimeVectorExpression → TimeVectorExpression
| TimeCoordVector : TimeScalarExpression → TimeVectorExpression
with Geometric3PointExpression : Type
| Geometric3LitPoint : Geometric3PointStruct → Geometric3PointExpression
| Geometric3VarPoint : Geometric3PointVar → Geometric3PointExpression
| Geometric3SubVectorVector : Geometric3VectorExpression → Geometric3VectorExpression → Geometric3PointExpression
| Geometric3NegPoint : Geometric3PointExpression → Geometric3PointExpression
| Geometric3AddPointVector : Geometric3PointExpression → Geometric3VectorExpression → Geometric3PointExpression
| Geometric3AddVectorPoint : Geometric3VectorExpression → Geometric3PointExpression → Geometric3PointExpression
| Geometric3ScalarPoint : RealScalarExpression → Geometric3PointExpression → Geometric3PointExpression
| Geometric3PointScalar : Geometric3PointExpression → RealScalarExpression → Geometric3PointExpression
| Geometric3ParenPoint : Geometric3PointExpression → Geometric3PointExpression
| Geometric3CoordPoint : Geometric3ScalarExpression → Geometric3ScalarExpression → Geometric3ScalarExpression → Geometric3PointExpression
/-with Velocity3PointExpression : Type
| Velocity3LitPoint : Velocity3PointStruct → Velocity3PointExpression
| Velocity3SubVectorVector : Velocity3VectorExpression → Velocity3VectorExpression → Velocity3PointExpression
| Velocity3NegPoint : Velocity3PointExpression → Velocity3PointExpression
| Velocity3AddVectorPoint : Velocity3VectorExpression → Velocity3PointExpression → Velocity3PointExpression
| Velocity3AddPointVector : Velocity3PointExpression → Velocity3VectorExpression → Velocity3PointExpression
| Velocity3ScalarPoint : RealScalarExpression → Velocity3PointExpression → Velocity3PointExpression
| Velocity3PointScalar : Velocity3PointExpression → RealScalarExpression → Velocity3PointExpression
| Velocity3ParenPoint : Velocity3PointExpression → Velocity3PointExpression-/
with TimePointExpression : Type
| TimeLitPoint : TimePointStruct → TimePointExpression
| TimeVarPoint : TimePointVar → TimePointExpression
| TimeSubVectorVector : TimeVectorExpression → TimeVectorExpression → TimePointExpression
| TimeNegPoint : TimePointExpression → TimePointExpression
| TimeAddPointVector : TimePointExpression → TimeVectorExpression → TimePointExpression
| TimeAddVectorPoint : TimeVectorExpression → TimePointExpression → TimePointExpression
| TimeScalarPoint : RealScalarExpression → TimePointExpression → TimePointExpression
| TimePointScalar : TimePointExpression → RealScalarExpression → TimePointExpression
| TimeParenPoint : TimePointExpression → TimePointExpression
| TimeCoordPoint : TimeScalarExpression → TimePointExpression



/-
VARIABLE UNKNOWN

CASE 1: I PROVIDE A TYPE FOR VARIABLE -> REAL SCALAR VARIABLE X
RealScalarExpression.RealScalarVar 4 --{4}

CASE 2: I DONT PROVIDE A TYPE FOR VARIABLE
{}


-/


--RealScalarExpression Notations
notation %e := RealScalarExpression.RealVarScalar e
instance : has_add RealScalarExpression := ⟨RealScalarExpression.RealAddScalarScalar⟩ 
notation e1 - e2 := RealScalarExpression.RealSubScalarScalar e1 e2
notation e1 ⬝ e2 := RealScalarExpression.RealMulScalarScalar e1 e2
notation e1 / e2 := RealScalarExpression.RealDivScalarScalar e1 e2
instance : has_neg RealScalarExpression := ⟨RealScalarExpression.RealNegScalar⟩ 
notation e⁻¹ := RealScalarExpression.RealInvScalar e
notation $e := RealScalarExpression.RealParenScalar e
notation ⬝e := RealScalarExpression.RealLitScalar e

--Geometric3ScalarExpression Notations
notation %e := Geometric3ScalarExpression.GeometricVarScalar e
instance : has_add Geometric3ScalarExpression := ⟨Geometric3ScalarExpression.GeometricAddScalarScalar⟩ 
notation e1 - e2 := Geometric3ScalarExpression.GeometricSubScalarScalar e1 e2
notation e1 ⬝ e2 := Geometric3ScalarExpression.GeometricMulScalarScalar e1 e2
notation e1 / e2 := Geometric3ScalarExpression.GeometricDivScalarScalar e1 e2
instance : has_neg Geometric3ScalarExpression := ⟨Geometric3ScalarExpression.GeometricNegScalar⟩  
notation e⁻¹ := Geometric3ScalarExpression.GeometricInvScalar e
notation $e := Geometric3ScalarExpression.GeometricParenScalar e
notation |e| := Geometric3ScalarExpression.GeometricNormVector e
notation ⬝e := Geometric3ScalarExpression.GeometricLitScalar e

def x : Geometric3Scalar := Geometric3Scalar.mk 0 (PhysSpace.mk affine_frame.std)
#check %x

--TimeScalarExpression Notations
notation %e := TimeScalarExpression.TimeVarScalar  
instance : has_add TimeScalarExpression := ⟨TimeScalarExpression.TimeAddScalarScalar⟩ 
notation e1 - e2 := TimeScalarExpression.TimeSubScalarScalar e1 e2
notation e1 ⬝ e2 := TimeScalarExpression.TimeMulScalarScalar e1 e2
notation e1 / e2 := TimeScalarExpression.TimeDivScalarScalar e1 e2
instance : has_neg TimeScalarExpression := ⟨TimeScalarExpression.TimeNegScalar⟩ 
notation e⁻¹ := TimeScalarExpression.TimeInvScalar e
notation $e := TimeScalarExpression.TimeParenScalar e
notation |e| := TimeScalarExpression.TimeNormVector e
notation ⬝e := TimeScalarExpression.TimeLitScalar e


def y : TimeScalar := TimeScalar.mk 0 (PhysSpace.mk affine_frame.std)
#check %y
--Velocity3ScalarExpression Notations
notation %e := Velocity3ScalarExpression.VelocityVarScalar e
instance : has_add Velocity3ScalarExpression := ⟨Velocity3ScalarExpression.VelocityAddScalarScalar⟩ 
notation e1 - e2 := Velocity3ScalarExpression.VelocitySubScalarScalar e1 e2
notation e1 ⬝ e2 := Velocity3ScalarExpression.VelocityMulScalarScalar e1 e2
notation e1 / e2 := Velocity3ScalarExpression.VelocityDivScalarScalar e1 e2
instance : has_neg Velocity3ScalarExpression := ⟨Velocity3ScalarExpression.VelocityNegScalar⟩ 
notation e⁻¹ := Velocity3ScalarExpression.VelocityInvScalar e
notation $e := Velocity3ScalarExpression.VelocityParenScalar e
notation |e| := Velocity3ScalarExpression.VelocityNormVector e
notation ⬝e := Velocity3ScalarExpression.VelocityLitScalar e


--Gemoetric3Vector Notations
notation %e := Geometric3VectorExpression.Geometric3VarVector e
instance : has_add Geometric3VectorExpression := ⟨Geometric3VectorExpression.Geometric3AddVectorVector⟩ 
instance : has_neg Geometric3VectorExpression := ⟨Geometric3VectorExpression.Geometric3NegVector⟩ 
notation c ⬝ e := Geometric3VectorExpression.Geometric3MulScalarVector c e
notation e ⬝ c := Geometric3VectorExpression.Geometric3MulVectorScalar e c
notation e1 - e2 := Geometric3VectorExpression.Geometric3SubPointPoint e1 e2
notation $e := Geometric3VectorExpression.Geometric3ParenVector e
notation ~e := Geometric3VectorExpression.Geometric3LitVector e
notation e1⬝e2⬝e3 := Geometric3VectorExpression.Geometric3CoordVector e1 e2 e3


/-
def X : Geometric3ScalarExpression := Geometric3ScalarExpression.GeometricLitScalar (Geometric3Scalar.mk 0 meters)
def Y : Geometric3ScalarExpression := Geometric3ScalarExpression.GeometricLitScalar (Geometric3Scalar.mk 0 meters)
def Z : Geometric3ScalarExpression := Geometric3ScalarExpression.GeometricLitScalar (Geometric3Scalar.mk 0 meters)
def R : RealScalarExpression := RealScalarExpression.RealLitScalar RealScalarDefault
def geomexpr : Geometric3VectorExpression := Geometric3VectorExpression.Geometric3CoordVector X Y Z
def geom2expr : Geometric3VectorExpression := X⬝Y⬝Z
#check Geometric3VectorExpression.Geometric3CoordVector X Y Z
#check  R ⬝ geomexpr
-/


--Velocity3Vector Notations
notation %e := Velocity3VectorExpression.Velocity3VarVector e
instance : has_add Velocity3VectorExpression := ⟨Velocity3VectorExpression.Velocity3AddVectorVector⟩ 
instance : has_neg Velocity3VectorExpression := ⟨Velocity3VectorExpression.Velocity3NegVector⟩ 
notation c ⬝ e := Velocity3VectorExpression.Velocity3MulScalarVector c e
notation e ⬝ c := Velocity3VectorExpression.Velocity3MulVectorScalar e c
notation e1 - e2 := Velocity3VectorExpression.Velocity3SubPointPoint e1 e2
notation $e := Velocity3VectorExpression.Velocity3ParenVector e
notation ~e := Velocity3VectorExpression.Velocity3LitVector e
notation e1⬝e2⬝e3 := Velocity3VectorExpression.Velocity3CoordVector e1 e2 e3

--https://www.caam.rice.edu/~heinken/latex/symbols.pdf

def vel_var : Velocity3VectorVar := Velocity3VectorVar.mk 0 

def help : Velocity3VectorExpression := ~vel_var


def helper : Velocity3VectorExpression → bool
| (Velocity3VectorExpression.Velocity3VarVector v) := tt
| _ := ff

#check ~vel_var

#check ~vel_var



--TimeVector Notations
notation %e := TimeVectorExpression.TimeVarVector e
instance : has_add TimeVectorExpression := ⟨TimeVectorExpression.TimeAddVectorVector⟩ 
instance : has_neg TimeVectorExpression := ⟨TimeVectorExpression.TimeNegVector⟩ 
notation c⬝e := TimeVectorExpression.TimeMulScalarVector c e
notation e⬝c := TimeVectorExpression.TimeMulVectorScalar e c
notation e1-e2 := TimeVectorExpression.TimeSubPointPoint e1 e2
notation $e := TimeVectorExpression.TimeParenVector e
notation ~e := TimeVectorExpression.TimeLitVector e
notation ⬝e := TimeVectorExpression.TimeCoordVector e


--Geometric3Point Notations
notation %e := Geometric3PointExpression.Geometric3VarPoint e
notation e1 - e2 := Geometric3PointExpression.Geometric3SubVectorVector e1 e2
instance : has_neg Geometric3PointExpression := ⟨Geometric3PointExpression.Geometric3NegPoint⟩
instance : has_trans Geometric3VectorExpression Geometric3PointExpression  := ⟨Geometric3PointExpression.Geometric3AddVectorPoint⟩
notation c • e := Geometric3PointExpression.Geometric3ScalarPoint c e
notation e • c := Geometric3PointExpression.Geometric3PointScalar e c
notation $e := Geometric3PointExpression.Geometric3ParenPoint e
notation ~e := Geometric3PointExpression.Geometric3LitPoint e
notation e1⬝e2⬝e3 := Geometric3PointExpression.Geometric3CoordPoint e1 e2 e3

--Andrew : Remove this
--Velocity3Point Notations
/-notation e1 - e2 := Velocity3PointExpression.Velocity3SubVectorVector e1 e2
notation -e := Velocity3PointExpression.Velocity3NegPoint e
notation e1 + e2 := Velocity3PointExpression.Velocity3AddVectorPoint e1 e2
notation e1 + e2 := Velocity3PointExpression.Velocity3AddPointVector e1 e2
notation c * e := Velocity3PointExpression.Velocity3ScalarPoint c e
notation e * c := Velocity3PointExpression.Velocity3PointScalar e c
notation (e := Velocity3PointExpression.Velocity3ParenPoint e
-/


--TimePoint Notations
notation %e := TimePointExpression.TimeVarPoint e
notation e1 ⊹ e2 := TimePointExpression.TimeAddPointVector e1 e2
instance : has_trans TimeVectorExpression TimePointExpression  := ⟨TimePointExpression.TimeAddVectorPoint⟩
notation $e := TimePointExpression.TimeParenPoint e
notation e1 - e2 := TimePointExpression.TimeSubVectorVector e1 e2
instance : has_neg TimePointExpression := ⟨TimePointExpression.TimeNegPoint⟩ 
notation c • e := TimePointExpression.TimeScalarPoint c e
notation e • c := TimePointExpression.TimePointScalar e c
notation ~e := TimePointExpression.TimeLitPoint e
notation ⬝e := TimePointExpression.TimeCoordPoint e






abbreviation RealScalarInterp := RealScalarVar → RealScalar

abbreviation Geometric3ScalarInterp := Geometric3ScalarVar → Geometric3Scalar
abbreviation TimeScalarInterp := TimeScalarVar → TimeScalar
abbreviation Velocity3ScalarInterp := Velocity3ScalarVar → Velocity3Scalar

abbreviation Geometric3VectorInterp := Geometric3VectorVar → Geometric3Vector
abbreviation TimeVectorInterp := TimeVectorVar → TimeVector
abbreviation Velocity3VectorInterp := Velocity3VectorVar → Velocity3Vector

abbreviation Geometric3PointInterp := Geometric3PointVar → Geometric3PointStruct
abbreviation TimePointInterp := TimePointVar → TimePointStruct
--abbreviation Velocity3PointInterp := Velocity3PointVar → Velocity3PointStruct


def DefaultRealScalarInterp : RealScalarInterp := λ scalar, RealScalarDefault
def DefaultGeometric3ScalarInterp : Geometric3ScalarInterp := λ scalar, Geometric3ScalarDefault
def DefaultTimeScalarInterp : TimeScalarInterp := λ scalar, TimeScalarDefault
def DefaultVelocity3ScalarInterp : Velocity3ScalarInterp := λ scalar, Velocity3ScalarDefault
def DefaultGeometric3VectorInterp : Geometric3VectorInterp := λ vector, Geometric3VectorDefault geom3
def DefaultTimeVectorInterp : TimeVectorInterp := λ vector, TimeVectorDefault time
def DefaultVelocity3VectorInterp : Velocity3VectorInterp := λ vector, Velocity3VectorDefault vel
def DefaultGeometric3PointInterp : Geometric3PointInterp := λ point, Geometric3PointDefault geom3
def DefaultTimePointInterp : TimePointInterp := λ point, TimePointDefault time
--def DefaultVelocity3PointInterp : Velocity3PointInterp := λ point, Velocity3Point_zero

inductive RealScalarCommand : Type
| Assignment : RealScalarVar → RealScalarExpression → RealScalarCommand

notation v = e := RealScalarCommand.Assignment v e

inductive Geometric3ScalarCommand : Type
| Assignment : Geometric3ScalarVar → Geometric3ScalarExpression → Geometric3ScalarCommand

notation v = e := Geometric3ScalarCommand.Assignment v e

inductive TimeScalarCommand : Type
| Assignment : TimeScalarVar → TimeScalarExpression → TimeScalarCommand

notation v = e := TimeScalarCommand.Assignment v e

inductive Velocity3ScalarCommand : Type
| Assignment : Velocity3ScalarVar → Velocity3ScalarExpression → Velocity3ScalarCommand

notation v = e := Velocity3ScalarCommand.Assignment v e

inductive Geometric3VectorCommand : Type
| Assignment : Geometric3VectorVar → Geometric3VectorExpression → Geometric3VectorCommand

notation v = e := Geometric3VectorCommand.Assignment v e

inductive TimeVectorCommand : Type
| Assignment : TimeVectorVar → TimeVectorExpression → TimeVectorCommand

notation v = e := TimeVectorCommand.Assignment v e

inductive Velocity3VectorCommand : Type
| Assignment : Velocity3VectorVar → Velocity3VectorExpression → Velocity3VectorCommand

notation v = e := Velocity3VectorCommand.Assignment v e

inductive Geometric3PointCommand : Type
| Assignment : Geometric3PointVar → Geometric3PointExpression → Geometric3PointCommand

notation v = e := Geometric3PointCommand.Assignment v e

inductive TimePointCommand : Type
| Assignment : TimePointVar → TimePointExpression → TimePointCommand

notation v = e := TimePointCommand.Assignment v e

def realScalarEval : RealScalarExpression → RealScalarInterp → ℝ  
| (RealScalarExpression.RealLitScalar r) i := r.val
| (RealScalarExpression.RealVarScalar v) i := (i v).val
| (RealScalarExpression.RealAddScalarScalar e1 e2) i := (realScalarEval e1 i) + (realScalarEval e2 i)
| (RealScalarExpression.RealSubScalarScalar e1 e2) i := (realScalarEval e1 i) - (realScalarEval e2 i)
| (RealScalarExpression.RealMulScalarScalar e1 e2) i := (realScalarEval e1 i) * (realScalarEval e2 i)
| (RealScalarExpression.RealNegScalar e) i := -1 * (realScalarEval e i)
| (RealScalarExpression.RealParenScalar e) i := realScalarEval e i
| (RealScalarExpression.RealDivScalarScalar e1 e2) i := RealScalarDefault.val
| (RealScalarExpression.RealInvScalar e) i := RealScalarDefault.val


def Geometric3ScalarEval : Geometric3ScalarExpression → Geometric3ScalarInterp → ℝ 
| (Geometric3ScalarExpression.GeometricLitScalar r) i := r.val
| (Geometric3ScalarExpression.GeometricVarScalar v) i := (i v).val
| (Geometric3ScalarExpression.GeometricAddScalarScalar e1 e2) i := (Geometric3ScalarEval e1 i) + (Geometric3ScalarEval e2 i)
| (Geometric3ScalarExpression.GeometricSubScalarScalar e1 e2) i := (Geometric3ScalarEval e1 i) - (Geometric3ScalarEval e2 i)
| (Geometric3ScalarExpression.GeometricMulScalarScalar e1 e2) i := (Geometric3ScalarEval e1 i) * (Geometric3ScalarEval e2 i)
| (Geometric3ScalarExpression.GeometricNegScalar e) i := -1 * (Geometric3ScalarEval e i)
| (Geometric3ScalarExpression.GeometricParenScalar e) i := Geometric3ScalarEval e i
| (Geometric3ScalarExpression.GeometricDivScalarScalar e1 e2) i := Geometric3ScalarDefault.val
| (Geometric3ScalarExpression.GeometricInvScalar e) i := Geometric3ScalarDefault.val
| (Geometric3ScalarExpression.GeometricNormVector v) i := Geometric3ScalarDefault.val


def timeScalarEval : TimeScalarExpression → TimeScalarInterp → ℝ 
| (TimeScalarExpression.TimeLitScalar r) i := r.val
| (TimeScalarExpression.TimeVarScalar v) i := (i v).val
| (TimeScalarExpression.TimeAddScalarScalar e1 e2) i := (timeScalarEval e1 i) + (timeScalarEval e2 i)
| (TimeScalarExpression.TimeSubScalarScalar e1 e2) i := (timeScalarEval e1 i) - (timeScalarEval e2 i)
| (TimeScalarExpression.TimeMulScalarScalar e1 e2) i := (timeScalarEval e1 i) * (timeScalarEval e2 i)
| (TimeScalarExpression.TimeNegScalar e) i := -1 * (timeScalarEval e i)
| (TimeScalarExpression.TimeParenScalar e) i := timeScalarEval e i
| (TimeScalarExpression.TimeDivScalarScalar e1 e2) i := TimeScalarDefault.val
| (TimeScalarExpression.TimeInvScalar e) i := TimeScalarDefault.val
| (TimeScalarExpression.TimeNormVector v) i := TimeScalarDefault.val


def Velocity3ScalarEval : Velocity3ScalarExpression → Velocity3ScalarInterp → ℝ 
| (Velocity3ScalarExpression.VelocityLitScalar r) i := r.val
| (Velocity3ScalarExpression.VelocityVarScalar v) i := (i v).val
| (Velocity3ScalarExpression.VelocityAddScalarScalar e1 e2) i := (Velocity3ScalarEval e1 i) + (Velocity3ScalarEval e2 i)
| (Velocity3ScalarExpression.VelocitySubScalarScalar e1 e2) i := (Velocity3ScalarEval e1 i) - (Velocity3ScalarEval e2 i)
| (Velocity3ScalarExpression.VelocityMulScalarScalar e1 e2) i := (Velocity3ScalarEval e1 i) * (Velocity3ScalarEval e2 i)
| (Velocity3ScalarExpression.VelocityNegScalar e) i := -1 * (Velocity3ScalarEval e i)
| (Velocity3ScalarExpression.VelocityParenScalar e) i := Velocity3ScalarEval e i
| (Velocity3ScalarExpression.VelocityDivScalarScalar e1 e2) i := Velocity3ScalarDefault.val
| (Velocity3ScalarExpression.VelocityInvScalar e) i := Velocity3ScalarDefault.val
| (Velocity3ScalarExpression.VelocityNormVector v) i := Velocity3ScalarDefault.val




def geometric3VectorEval : Geometric3VectorExpression → Geometric3VectorInterp → Geometric3Vector
| (Geometric3VectorExpression.Geometric3LitVector vec) i := vec
| (Geometric3VectorExpression.Geometric3VarVector v) i := i v
| (Geometric3VectorExpression.Geometric3AddVectorVector v1 v2) i := Geometric3VectorDefault geom3
| (Geometric3VectorExpression.Geometric3NegVector v) i := Geometric3VectorDefault geom3
| (Geometric3VectorExpression.Geometric3MulScalarVector c v) i := Geometric3VectorDefault geom3
| (Geometric3VectorExpression.Geometric3MulVectorScalar v c) i := Geometric3VectorDefault geom3
| (Geometric3VectorExpression.Geometric3SubPointPoint p1 p2) i := Geometric3VectorDefault geom3
| (Geometric3VectorExpression.Geometric3ParenVector v) i := Geometric3VectorDefault geom3
| (Geometric3VectorExpression.Geometric3CoordVector x y z) i := Geometric3VectorDefault geom3






def velocity3VectorEval : Velocity3VectorExpression → Velocity3VectorInterp → Velocity3Vector
| _ _ := Velocity3VectorDefault vel

def timeVectorEval : TimeVectorExpression → TimeVectorInterp → TimeVector
| _ _ := TimeVectorDefault time

def geometric3PointEval : Geometric3PointExpression → Geometric3PointInterp → Geometric3PointStruct
| _ _ := Geometric3PointDefault geom3


def timePointEval : TimePointExpression → TimePointInterp → TimePointStruct
| _ _ := TimePointDefault time

/-
| (RealScalarExpression.RealLitScalar r) i := r
| (RealScalarExpression.RealVarScalar v) i := i v
| (RealScalarExpression.RealAddScalarScalar e1 e2) i := realScalarEval e1 i + realScalarEval e2 i
| (RealScalarExpression.RealSubScalarScalar e1 e2) i := realScalarEval e1 i - realScalarEval e2 i
| (RealScalarExpression.RealMulScalarScalar e1 e2) i := realScalarEval e1 i * realScalarEval e2 i
| (RealScalarExpression.RealDivScalarScalar e1 e2) i := realScalarEval e1 i / realScalarEval e2 i
| (RealScalarExpression.RealNegScalar e) i := -1 * realScalarEval e i
| (RealScalarExpression.RealInvScalar e) i := 1 / realScalarEval e i
| (RealScalarExpression.RealParenScalar e) i := realScalarEval e i
-/

/-
important for static analysis!!

100 scalar variables

nontrivial : map them individually to a value that makes sense, can change at different points in  program

start of program : interp mapping 1

end of program : interp mapping n

default : map them all to 0

-/
/-
REAL PROGRAM : 100 variables

95 "UNKNOWN"
~ loose bound on "5" of them


CTRLH ℝ → ℚ 
-/







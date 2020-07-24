import data.real.basic
import ..affine.aff_coord_space
 

noncomputable theory

/-
physicalDimension
time and distance are base types
can create derived types with the inverse and multiplication operations
-/
inductive physicalDimension : Type
| time
| distance
| multiply : (physicalDimension → physicalDimension → physicalDimension)
| inverse : (physicalDimension → physicalDimension)
--define velocity to be physical dimension of (distance / time)
def velocity : physicalDimension :=
    physicalDimension.multiply physicalDimension.distance (physicalDimension.inverse physicalDimension.time)

inductive phys_unit (d : physicalDimension) : Type
| std : phys_unit
| mk : ℝ → phys_unit

def meters : phys_unit physicalDimension.distance := phys_unit.std
def kilometers : phys_unit physicalDimension.distance := phys_unit.mk 0.001

--take affine_frame to be a type 
--since we want some notion of a frame attached to any points/vectors we define


inductive affine_frame (dimension : ℕ): Type
| std : affine_frame
| mk : (aff_vec ℝ dimension) → (aff_vec ℝ dimension) → (aff_vec ℝ dimension) → (aff_pt ℝ dimension) → affine_frame




/-
phys_space%

torsor field vector

can construct any physical space for a given dimension and physical dimension
i.e. 3D Geometric space (distance as its physical dimension)
    1D time space (time as its physical dimension)
-/

--def mk_real_affine_space (dimension : ℕ) : Type := 
--    affine_space (aff_pt ℝ 3) ℝ (aff_vec ℝ 3)

--def p : _ := mk_real_affine_space 3

structure PhysSpace (d : physicalDimension) (dimension : ℕ) : Type :=
    mk :: (std_frame : affine_frame dimension) 


--take std to be some affine frame



structure Geometric3PointStruct : Type :=
    mk :: (sp : PhysSpace physicalDimension.distance 3) (pt : aff_pt ℝ 3) (B : affine_frame 3)

structure TimePointStruct : Type :=
    mk :: (sp : PhysSpace physicalDimension.time 1) (pt : aff_pt ℝ 1) (B : affine_frame 1)

structure Velocity3PointStruct : Type :=
    mk :: (sp : PhysSpace velocity 3) (pt : aff_pt ℝ 3) (B : affine_frame 3)

--expression.add (x : TimeVector y : GeometricVector)



structure Geometric3Vector : Type :=
    mk :: (sp : PhysSpace physicalDimension.distance 3) (vec : aff_vec ℝ 3) (B : affine_frame 3)



/-
def GeomAdd : GeometricVector → GeometricVector → GeometricVector
| (GeometricVector.mk a x b) (GeometricVector.mk a y b) := GeometricVector.mk a (x+y) b
| _ _ := sorry --should throw an error

instance : has_add GeometricVector := ⟨GeomAdd⟩

-/


structure TimeVector : Type :=
    mk :: (sp : PhysSpace physicalDimension.time 1) (vec : aff_vec ℝ 1) (B : affine_frame 1)




structure Velocity3Vector : Type :=
    mk :: (sp : PhysSpace velocity 3) (vec : aff_vec ℝ 3) (B : affine_frame 3)


/-
Norm Function:
takes in some 1-dimensional Time vector and returns its magnitude (i.e. first element in the list)
-/
--def magnitude : TimeVector → ℝ
--| _ := 0


/-
Example function for creating GeometricPoint from 3 reals and a physical space
-/
def Geometric3Point (p : PhysSpace physicalDimension.distance 3) (x y z : ℝ) : Geometric3PointStruct :=
    Geometric3PointStruct.mk p (aff_pt.mk [1,x,y,z] rfl rfl) p.std_frame

def TimePoint (p : PhysSpace physicalDimension.time 1) (t : ℝ) : TimePointStruct :=
    TimePointStruct.mk p (aff_pt.mk [1, t] rfl rfl) p.std_frame

def Velocity3Point (p : PhysSpace velocity 3) (x y z : ℝ) : Velocity3PointStruct :=
    Velocity3PointStruct.mk p (aff_pt.mk [1,x,y,z] rfl rfl) p.std_frame
--def example_point := GeometricPoint geom3 1 1 1
--def example_time := TimePoint time 10


--def geom3 : PhysSpace physicalDimension.distance 3 := PhysSpace.mk affine_frame.std

--def vel : PhysSpace velocity 3 := PhysSpace.mk affine_frame.std

--def time : PhysSpace physicalDimension.time 1 := PhysSpace.mk affine_frame.std




def EuclideanGeometry (name : string) (n : nat): PhysSpace physicalDimension.distance 3 :=
    PhysSpace.mk affine_frame.std

def ClassicalTime (name : string) : PhysSpace physicalDimension.time 1 :=
    PhysSpace.mk affine_frame.std

def ClassicalVelocity (name : string) (n : nat) : PhysSpace velocity 3 :=
    PhysSpace.mk affine_frame.std


--def vel2 := ClassicalVelocity "hi" 3


--def 



structure Geometric3Scalar  : Type :=
mk :: (val : ℝ) (sp : PhysSpace physicalDimension.distance 3)

structure TimeScalar : Type :=
mk :: (val : ℝ) (sp : PhysSpace physicalDimension.time 1)
    
structure Velocity3Scalar : Type :=
mk :: (val : ℝ) (sp : PhysSpace velocity 3)

structure RealScalar : Type :=
mk :: (val : ℝ)
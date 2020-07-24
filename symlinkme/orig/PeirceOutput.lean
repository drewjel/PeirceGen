import .physlang2

noncomputable theory



def worldGeometryvar : EuclideanGeometry3SpaceVar := (!19)
def worldGeometrysp := (EuclideanGeometry3SpaceExpression.EuclideanGeometry3Literal ( BuildEuclideanGeometrySpace "worldGeometry" 3))
def worldGeometry := PhysGlobalCommand.GlobalSpace (⊢worldGeometryvar) (⊢worldGeometrysp)


def worldTimevar : ClassicalTimeSpaceVar := (!20)
def worldTimesp :=  (ClassicalTimeSpaceExpression.ClassicalTimeLiteral ( BuildClassicalTimeSpace "worldTime" ))
def worldTime := PhysGlobalCommand.GlobalSpace (⊢worldTimevar) (⊢worldTimesp)


def worldVelocityvar : ClassicalVelocity3SpaceVar := (!21)
def worldVelocitysp := (ClassicalVelocity3SpaceExpression.ClassicalVelocity3Literal ( BuildClassicalVelocitySpace "worldVelocity" 3))
def worldVelocity := PhysGlobalCommand.GlobalSpace (⊢worldVelocityvar) (⊢worldVelocitysp)


def worldGeometry.Standardvar : EuclideanGeometry3FrameVar := (!22)
def worldGeometry.Standardfr := EuclideanGeometry3FrameExpression.FrameLiteral ( GetEuclideanGeometryStandardFrame (EvalEuclideanGeometry3SpaceExpression worldGeometrysp))
def worldGeometry.Standard := PhysGlobalCommand.GlobalFrame (⊢worldGeometry.Standardvar) (⊢worldGeometry.Standardfr)


def worldTime.Standardvar : ClassicalTimeFrameVar := (!23)
def worldTime.Standardfr := ClassicalTimeFrameExpression.FrameLiteral ( GetClassicalTimeStandardFrame (EvalClassicalTimeSpaceExpression worldTimesp))
def worldTime.Standard := PhysGlobalCommand.GlobalFrame (⊢worldTime.Standardvar) (⊢worldTime.Standardfr)


def worldVelocity.Standardvar : ClassicalVelocity3FrameVar := (!24)
def worldVelocity.Standardfr := ClassicalVelocity3FrameExpression.FrameLiteral ( GetClassicalVelocityStandardFrame (EvalClassicalVelocity3SpaceExpression worldVelocitysp))
def worldVelocity.Standard := PhysGlobalCommand.GlobalFrame (⊢worldVelocity.Standardvar) (⊢worldVelocity.Standardfr)





def INDEX4.REAL3.VAR.IDENTtf.start.point : _ := !25


def INDEX7.REAL1.EXPR.B.L105C36.E.L105C36 : _ :=  ⬝. 

def INDEX8.REAL1.EXPR.B.L105C40.E.L105C40 : _ :=  ⬝. 

def INDEX9.REAL1.EXPR.B.L105C44.E.L105C44 : _ :=  ⬝. 
def INDEX19.REAL3.EXPR.B.L105C26.E.L105C46 : _ := (INDEX7.REAL1.EXPR.B.L105C36.E.L105C36)⬝(INDEX8.REAL1.EXPR.B.L105C40.E.L105C40)⬝(INDEX9.REAL1.EXPR.B.L105C44.E.L105C44)
def STMT.B.L104C5.E.L105C47 : _ := (⊢(INDEX4.REAL3.VAR.IDENTtf.start.point))=(⊢(INDEX19.REAL3.EXPR.B.L105C26.E.L105C46))


def INDEX5.REAL3.VAR.IDENTtf.end.point : _ := !26


def INDEX13.REAL1.EXPR.B.L107C34.E.L107C34 : EuclideanGeometry3ScalarExpression  :=  ⬝(EuclideanGeometry3ScalarDefault (EvalEuclideanGeometry3SpaceExpression worldGeometrysp)) 

def INDEX14.REAL1.EXPR.B.L107C38.E.L107C39 : _ :=  ⬝. 

def INDEX15.REAL1.EXPR.B.L107C42.E.L107C42 : _ :=  ⬝. 
def INDEX21.REAL3.EXPR.B.L107C24.E.L107C44 : _ := (INDEX13.REAL1.EXPR.B.L107C34.E.L107C34)⬝(INDEX14.REAL1.EXPR.B.L107C38.E.L107C39)⬝(INDEX15.REAL1.EXPR.B.L107C42.E.L107C42)
def INDEX1.STMT.B.L106C5.E.L107C45 : _ := (⊢(INDEX5.REAL3.VAR.IDENTtf.end.point))=(⊢(INDEX21.REAL3.EXPR.B.L107C24.E.L107C44))


def INDEX6.REAL3.VAR.IDENTtf.displacement : _ := !27


def INDEX1.REAL3.EXPRtf.end.point.B.L109C35.E.L109C35 : _ := %(INDEX5.REAL3.VAR.IDENTtf.end.point)

def INDEX2.REAL3.EXPRtf.start.point.B.L109C50.E.L109C50 : _ := %(INDEX4.REAL3.VAR.IDENTtf.start.point)
def INDEX3.REAL3.EXPR.B.L109C35.E.L109C50 : _ := (INDEX1.REAL3.EXPRtf.end.point.B.L109C35.E.L109C35)-(INDEX2.REAL3.EXPRtf.start.point.B.L109C50.E.L109C50)
def INDEX2037539186.STMT.B.L109C5.E.L109C64 : _ := (⊢(INDEX6.REAL3.VAR.IDENTtf.displacement))=(⊢(INDEX3.REAL3.EXPR.B.L109C35.E.L109C50))


def INDEX117954160.STMTCOMMAND.B.L97C32.E.L110C1 : PhysCommand := PhysCommand.Seq (INDEX2037539186.STMT.B.L109C5.E.L109C64::INDEX1.STMT.B.L106C5.E.L107C45::STMT.B.L104C5.E.L105C47::[])

def INDEX117749040.GLOBALSTMTCOMMAND.B.L97C1.E.L110C1 : PhysGlobalCommand := PhysGlobalCommand.Main INDEX117954160.STMTCOMMAND.B.L97C32.E.L110C1


def INDEX113864224.PROGRAMCOMMAND.B.L0C0.E.L0C0globalseq : PhysGlobalCommand := PhysGlobalCommand.Seq (worldVelocity.Standard::worldTime.Standard::worldGeometry.Standard::worldVelocity::worldTime::worldGeometry::INDEX117749040.GLOBALSTMTCOMMAND.B.L97C1.E.L110C1::[])
def INDEX113864224.PROGRAMCOMMAND.B.L0C0.E.L0C0 : PhysProgram := PhysProgram.Program INDEX113864224.PROGRAMCOMMAND.B.L0C0.E.L0C0globalseq


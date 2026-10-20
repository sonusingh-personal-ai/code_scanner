namespace CodeScanner.Domain.Enums;

public enum OfficeMemberType
{
    VisualBy = 1,
    TestedBy = 2,
    ProcEng = 3,
    ProgDisNo = 4
}

public enum ProductionLineType
{
    Card = 1,
    Assembly = 2
}

public enum QcStage
{
    Testing = 1,
    Qc = 2,
    Oqc = 3
}

public enum TestStatus
{
    Pass = 1,
    Fail = 2,
    Pending = 3
}

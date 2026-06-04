using System;

[Serializable]
public class MeasurementComparisonResult
{
    public string oldSessionId;
    public string newSessionId;

    public string oldDateTime;
    public string newDateTime;

    public float shoulderDelta;
    public float chestDelta;
    public float waistDelta;
    public float hipDelta;
    public float armDelta;
    public float legDelta;
}
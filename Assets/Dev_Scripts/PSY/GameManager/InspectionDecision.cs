public struct InspectionDecision
{
    public bool shouldApprove;
    public string reason;

    public InspectionDecision(bool shouldApprove, string reason)
    {
        this.shouldApprove = shouldApprove;
        this.reason = reason;
    }
}

namespace ServerMedical.Models.Request.Campaign;

public sealed class RequestCreateCampaign
{
    public string Title { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public List<RequestCampaignAssignment> StaffAssignments { get; set; } = new();
}

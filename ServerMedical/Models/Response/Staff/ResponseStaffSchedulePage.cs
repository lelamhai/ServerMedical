namespace ServerMedical.Models.Response.Staff;

public sealed class ResponseStaffSchedulePage
{
    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalItems { get; init; }

    public int TotalPages { get; init; }

    public List<ResponseStaffSchedule> Items { get; init; } = new();
}

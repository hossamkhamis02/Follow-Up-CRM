namespace FollowUpCrm.Shared.Results;

public record PagedResponse<T>(List<T> Items, int TotalCount, int Page, int PageSize);
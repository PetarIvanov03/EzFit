namespace EzFit.Services.Interfaces
{
    // Single seam for "who is the current user" until real auth lands —
    // everything else should depend on this instead of a hardcoded id.
    public interface ICurrentUserProvider
    {
        int UserId { get; }
    }
}

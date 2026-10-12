namespace AppPortal.Models;

public class HostRef
{
    public LiteDB.ObjectId HostId { get; set; }
    public bool IsPreferred { get; set; }
}

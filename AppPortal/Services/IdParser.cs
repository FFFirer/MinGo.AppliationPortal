using LiteDB;

namespace AppPortal.Services;

public static class IdParser
{
    public static bool TryParse(string? id, out ObjectId oid)
    {
        oid = ObjectId.Empty;
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        try
        {
            oid = new ObjectId(id);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

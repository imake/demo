
public class BaseJsonProto
{
    public string type;
}

public class BaseC2SJsonProto : BaseJsonProto
{

}

public class BaseS2CJsonProto : BaseJsonProto
{
    public string err;

    public string mJson;

    public void SetJson(string rawJson)
    {
        mJson = rawJson;
    }

    public string GetJson()
    {
        return mJson;
    }
}

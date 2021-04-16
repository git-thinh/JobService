
public class oResponse
{
    public bool Ok { set; get; }
    public string Message { set; get; }
    public object Data { set; get; }
    public object Error { set; get; }

    public oResponse()
    {
        this.Ok = false;
        this.Message = string.Empty;
    }

    public oResponse(bool ok, object data, string message)
    {
        this.Ok = ok;
        this.Message = message;
        this.Data = data;
    }
}

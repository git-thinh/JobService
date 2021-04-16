using Autofac;
using System.Net.Http;

public interface IAppJob
{
    IContainer Container { set; get; }
    void EventSend(string text);
    void EventRegister(object client);

    bool ApiCheckLogin(HttpRequestMessage request);
}
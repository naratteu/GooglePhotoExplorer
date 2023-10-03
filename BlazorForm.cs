using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace GooglePhotoExplorer;

file static class Util
{
    public static T As<T>(this T t, Action<T> a) { a(t); return t; }
}

public class BlazorForm<T> : BlazorForm where T : ComponentBase
{
    public BlazorForm() : base(typeof(T)) { }
    public object? this[Expression<Func<T, object>> selectMember]
    {
        init => base[selectMember.Body switch
        {
            UnaryExpression { Operand: MemberExpression { Member.Name: string key } } => key, //박싱이 발생할경우
            MemberExpression { Member.Name: string key } => key,
            _ => throw new("Expression Err")
        }] = value;
    }
}
public class BlazorForm : Form
{
    public BlazorForm(Type t) => Controls.Add(new BlazorWebView
    {
        Dock = DockStyle.Fill,
        HostPage = "wwwroot/index.html",
        RootComponents = { new("#app", t, parameters) },
        Services = Services,
    });
    readonly Dictionary<string, object?> parameters = new();
    public object? this[string key] { init => parameters[key] = value; }
    static readonly ServiceProvider Services = new ServiceCollection().As(s =>
    {
        s.AddBlazorWebViewDeveloperTools(); //F12누르면 개발자도구
        s.AddWindowsFormsBlazorWebView();
        s.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
        s.AddGooglePhotos();
        s.AddSingleton(_ => Cache.Inst.Result);
    }).BuildServiceProvider();
}
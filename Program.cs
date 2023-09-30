using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;

await Task.CompletedTask.ContinueWith(_ => Application.Run(new Form
{
    Text = nameof(GooglePhotoExplorer),
    Width = 800,
    Height = 600,
    Controls =
    {
        new BlazorWebView
        {
            Dock = DockStyle.Fill,
            HostPage = "wwwroot/index.html",
            Services = new ServiceCollection().As(s =>
            {
                s.AddBlazorWebViewDeveloperTools(); //F12누르면 개발자도구
                s.AddWindowsFormsBlazorWebView();
                s.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
                s.AddGooglePhotos();
            }).BuildServiceProvider(),
            RootComponents = { new("#app", typeof(GooglePhotoExplorer.GatherPage), null) },
        }
    }
}), new System.Threading.Tasks.Schedulers.StaTaskScheduler(1));

file static class Util
{
    public static T As<T>(this T t, Action<T> a) { a(t); return t; }
}
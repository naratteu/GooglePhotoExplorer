using GooglePhotoExplorer;

static class Pragram
{
    [STAThread]
    static void Main() => Application.Run(new BlazorForm<GatherPage>
    {
        Text = nameof(GooglePhotoExplorer),
        Size = new(800, 600),
    });
}
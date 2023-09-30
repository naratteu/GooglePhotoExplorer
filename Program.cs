using GooglePhotoExplorer;

await Task.CompletedTask.ContinueWith(_ => Application.Run(new BlazorForm<GatherPage>
{
    Text = nameof(GooglePhotoExplorer),
    Size = new(800, 600),
}), new System.Threading.Tasks.Schedulers.StaTaskScheduler(1));
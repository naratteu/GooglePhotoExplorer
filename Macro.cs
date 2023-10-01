using CasCap.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using Keys = OpenQA.Selenium.Keys;

namespace GooglePhotoExplorer;

public class Macro(Func<IWebDriver> getDriver)
{
    public static readonly Macro Inst = new(()=> new EdgeDriver(@"Q:\OneDrive\SWHOME", options: new() { DebuggerAddress = "localhost:9223" }));

    public async Task AppendDecs(string text, IEnumerable<MediaItem> items, Func<MediaItem, Task> postInput)
    {
        using IWebDriver cd = getDriver();
        try
        {
            cd.SwitchTo().NewWindow(WindowType.Tab);

            foreach (var mi in items)
            {
                await Task.Delay(3000);
                cd.Url = mi.productUrl;
                await Task.Delay(3000);
            re: try
                { cd.FindElement(By.CssSelector("textarea[aria-label='설명']")).SendKeys(text + Keys.Escape); }
                catch
                { cd.FindElement(By.CssSelector("button[aria-label='정보 열기']")).Click(); goto re; }
                finally
                { await Task.Delay(3000); }
                await postInput(mi);
            }
        }
        finally
        {
            cd.Quit();
        }
    }
}

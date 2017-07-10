using System;
using System.Threading.Tasks;

using WebSharpJs.Electron;
using WebSharpJs.Script;

//namespace MainWindow
//{
    public class Startup
    {

        static WebSharpJs.NodeJS.Console console;

        static App app;
        static BrowserWindow mainWindow;
        static int windowId = 0;

        /// <summary>
        /// Default entry into managed code.
        /// </summary>
        /// <param name="__dirname">The directory name of the current module. This the same as the path.dirname() of the __filename.</param>
        /// <returns></returns>
        public async Task<object> Invoke(string __dirname)
        {
            if (console == null)
                console = await WebSharpJs.NodeJS.Console.Instance();

            try
            {
                app = await App.Instance();

                // We use app.IsReady instead of listening for the 'ready'event.
                // By the time we get here from the main.js module the 'ready' event has
                // already fired.
                if (await app.IsReady())
                {
                    windowId = await CreateWindow(__dirname);
                    var webContents = await mainWindow.GetWebContents();

                    // Capture all external link clicks and open them in the
                    // default browser instead of inline in our application.
                    await webContents.On("will-navigate",
                        new ScriptObjectCallback<Event,string>(
                            async (navResult) =>
                            {
                                var result = navResult.CallbackState as object[];
                                var ev = result[0] as Event;
                                var externalURL = result[1].ToString();
                                ev.PreventDefault();
                                System.Diagnostics.Process.Start(externalURL);
                            }
                        )
                    
                    );
                }


                await console.Log($"Loading: file://{__dirname}/index.html");
            }
            catch (Exception exc) { await console.Log($"extension exception:  {exc.Message}"); }

            return windowId;


        }

        async Task<int> CreateWindow (string __dirname)
        {
            // Create the browser window.
            mainWindow = await BrowserWindow.Create(new BrowserWindowOptions { UseContentSize = true, IconPath = $"{__dirname}/assets/icons/appicon.ico"});

            // and load the index.html of the app.
            await mainWindow.LoadURL($"file://{__dirname}/index.html");

            // Open the DevTools
            //await mainWindow.GetWebContents().ContinueWith(
            //        (t) => { t.Result?.OpenDevTools(DevToolsMode.Bottom); }
            //);

            return await mainWindow.GetId();

        }
    }
//}

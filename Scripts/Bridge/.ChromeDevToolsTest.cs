using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using MasterDevs.ChromeDevTools;
using MasterDevs.ChromeDevTools.Protocol.Chrome.DOM;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Page;
using EnableCommand = MasterDevs.ChromeDevTools.Protocol.Chrome.Page.EnableCommand;

namespace UnityJS {


public class ChromeDevToolsTest : MonoBehaviour
{


    public ChromeSession chromeSession;
    const int ViewPortWidth = 1440;
    const int ViewPortHeight = 900;


    private void Awake()
    {
        Debug.Log("ChromeDevToolsTest:Awake");

        Task.Run(async () =>
        {
            // synchronization
            //var screenshotDone = new ManualResetEventSlim();

            // STEP 1 - Run Chrome
            string chromePath = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
            var chromeProcessFactory = new ChromeProcessFactory(new StubbornDirectoryCleaner(), chromePath);
            using (var chromeProcess = chromeProcessFactory.Create(9222, true))
            {
                // STEP 2 - Create a debugging session
                var sessionInfo = (await chromeProcess.GetSessionInfo()).LastOrDefault();
                var chromeSessionFactory = new ChromeSessionFactory();
                var chromeSession = chromeSessionFactory.Create(sessionInfo.WebSocketDebuggerUrl);

                // STEP 3 - Send a command
                //
                // Here we are sending a commands to tell chrome to set the viewport size 
                // and navigate to the specified URL
                await chromeSession.SendAsync(new SetDeviceMetricsOverrideCommand
                {
                    Width = ViewPortWidth,
                    Height = ViewPortHeight,
                    Scale = 1
                });

                var navigateResponse = await chromeSession.SendAsync(new NavigateCommand
                {
                    Url = "http://www.google.com"
                });
                Debug.Log("NavigateResponse: " + navigateResponse.Id);

                // STEP 4 - Register for events (in this case, "Page" domain events)
                // send an command to tell chrome to send us all Page events
                // but we only subscribe to certain events in this session
                var pageEnableResult = await chromeSession.SendAsync<EnableCommand>();
                Debug.Log("PageEnable: " + pageEnableResult.Id);

                chromeSession.Subscribe<LoadEventFiredEvent>(loadEventFired =>
                {
                    // we cannot block in event handler, hence the task
                    Task.Run(async () =>
                    {
                        Debug.Log("LoadEventFiredEvent: " + loadEventFired.Timestamp);

                        var documentNodeId = (await chromeSession.SendAsync(new GetDocumentCommand())).Result.Root.NodeId;
                        var bodyNodeId =
                            (await chromeSession.SendAsync(new QuerySelectorCommand
                            {
                                NodeId = documentNodeId,
                                Selector = "body"
                            })).Result.NodeId;
                        var height = (await chromeSession.SendAsync(new GetBoxModelCommand {NodeId = bodyNodeId})).Result.Model.Height;

                        await chromeSession.SendAsync(new SetDeviceMetricsOverrideCommand
                        {
                            Width = ViewPortWidth,
                            Height = height,
                            Scale = 1
                        });

                        Debug.Log("Taking screenshot");
                        var screenshot = await chromeSession.SendAsync(new CaptureScreenshotCommand {Format = "png"});

                        var data = System.Convert.FromBase64String(screenshot.Result.Data);
                        File.WriteAllBytes("output.png", data);
                        Debug.Log("Screenshot stored");

                        // tell the main thread we are done
                        //screenshotDone.Set();
                    });
                });

                // wait for screenshoting thread to (start and) finish
                //screenshotDone.Wait();

                Debug.Log("Exiting ..");
            }
        }).Wait();

    }


}


}

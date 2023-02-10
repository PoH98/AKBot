using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zeraniumu;

namespace AKBot.Scripts
{
    internal class BotScript: IScript
    {
        private EmulatorController controller;
        private Task task;
        private ILog log;
        public BotScript(ILog logger)
        {
            log = logger;
            controller = new EmulatorController(logger);
        }

        public void Run()
        {
            controller.PrepairOCR("eng");
            controller.StartEmulator();
            controller.ConnectEmulator();
            controller.ResizeToPreferedSize();
            controller.KillGame("kr.co.angames.astrokings.google.android");
            bool inSpace = false;
            do
            {
                List<Point> found;
                IImageData capture;
                while (!controller.GameIsForeground("kr.co.angames.astrokings.google.android"))
                {
                    inSpace = false;
                    log.WriteLog("Starting Astroking", Color.Lime);
                    controller.StartGame("kr.co.angames.astrokings.google.android", "com.google.firebase.MessagingUnityPlayerActivity");
                    Delay.Wait(1000);
                    log.WriteLog("Awaiting mainscreen...", Color.Lime);
                    Delay.Wait(1000);
                    do
                    {
                        capture = controller.Screenshot();
                        found = capture.FindImage("Images//homeBase.png", true, 0.75);
                        Delay.Wait(1000);
                    }
                    while (found.Count < 1);
                    log.WriteLog("Base located");
                    Delay.Wait(2000);
                }
                if (!inSpace)
                {
                    //going to space
                    controller.Tap(new Point(824, 650));
                    Delay.Wait(3000);
                    inSpace = true;
                }
                if (inSpace)
                {
                    //check for free fleets
                    capture = controller.Screenshot();
                    found = capture.FindImage("Images//FreeFleet.png", true, 0.7);
                    if(found.Count > 0)
                    {
                        log.WriteLog("Starts finding pirates", Color.Cyan);
                        controller.Tap(found.First());
                        Delay.Wait(500);
                        //search
                        capture = controller.Screenshot();
                        found = capture.FindImage("Images//PirateSelected.png", true, 0.75);
                        if(found.Count > 0)
                        {
                            log.WriteLog("Pirate search already selected", Color.LightCyan);
                        }
                        else
                        {
                            do
                            {
                                Delay.Wait(1000);
                                capture = controller.Screenshot();
                                found = capture.FindImage("Images//Pirate.png", true, 0.75);
                            }
                            while(found.Count == 0);
                            controller.Tap(found.First());
                            log.WriteLog("Selecting Pirate for searching", Color.LightCyan);
                        }
                        Delay.Wait(800);
                        //detect current lv
                        capture = controller.Screenshot();
                        capture.SaveFile("debug.png");
                        var crop = capture.Crop(new Rectangle(598, 618, 55, 12));
                        crop.SaveFile("ocr.png");
                        var text = crop.OCR(controller);
                        log.WriteLog("Detected currently selected " + text);
                        found = capture.FindImage("Images//Search.png", true, 0.8);
                        controller.Tap(found.First());
                        Delay.Wait(800);
                        controller.Tap(new Point(430, 338));
                        Delay.Wait(800);
                        controller.Tap(new Point(587, 641));
                        Delay.Wait(800);
                        controller.Tap(new Point(769, 655));
                        Delay.Wait(2000);
                    }
                    else
                    {
                        log.WriteLog("Unable to detect free fleets, maybe all fleets are busy...", Color.Cyan);
                        Delay.Wait(5000);
                    }
                }
            }
            while (true);
        }

        public void StopEmulator()
        {
            controller.StopEmulator();
        }
    }
}

﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using AttachToChrome.Tests.AttachToChrome;
using System.Diagnostics;
using AttachToChrome.Tests.IntelliTect;
using System.Threading.Tasks;

namespace AttachToChrome.Tests
{
    [TestClass]
    public class AttachToChromeTest
    {
        [TestInitialize()]
        public void MyTestInitialize()
        {
            Process[] wad = Process.GetProcessesByName("WinAppDriver");
            if (wad.Length < 1)
            {
                Process.Start(@"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe");
            }
            // Check for an existing instance of our WPF app and kill if needed
            // Since we can get into a bad state where chromedriver launches, but does not successfully attach,
            // Find and kill any chromedriver processes, too.
            // Note: this won't work if running tests in parallel on the same machine.
            KillProcessesByName("AttachToChrome", "chromedriver");

            Application.LaunchApplication();
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            Application.CloseApplication();
            // Because we attached, the browser itself won't close on normal dispose, so explicitely call close even when we ordinarily wouldn't want to.
            if(Page.Driver != null)
            {
                Page.Driver.Close();
                Page.Driver.Dispose();
            }
            // Note: this won't work if running tests in parallel on the same machine.
            KillProcessesByName("AttachToChrome", "chromedriver");
        }


        [TestMethod]
        public void LaunchChromeAndAttach()
        {
            // Open WPF application, make sure a button is present, then click it to launch Chrome
            Assert.IsTrue(Window.LaunchBrowserButton.Displayed, 
                "Expected button never appears.");
            Window.LaunchBrowserButton.Click();

            // Attach to new Chrome instance
            Page.AttachToChrome();

            // Verify Chrome launched to the correct page
            Assert.AreEqual("https://intellitect.com/blog/", Page.Driver.Url);
            Assert.IsTrue(Page.BlogList.Displayed);
            Assert.IsTrue(Page.BlogHeadings.Count > 0);
        }

        private Window Window => new Window();
        private PageUnderTest Page { get; } = new PageUnderTest();

        private void KillProcessesByName(params string[] namesOfProcessesToKill)
        {
            foreach(var name in namesOfProcessesToKill)
            {
                Process[] existingProcesses = Process.GetProcessesByName(name);
                if (existingProcesses.Length > 0)
                {
                    foreach (var p in existingProcesses)
                    {
                        p.Kill();
                    }
                }
            }
        }
    }
}

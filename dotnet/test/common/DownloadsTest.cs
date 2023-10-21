using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Environment;
using System.Collections.Generic;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace OpenQA.Selenium
{
    [TestFixture]
    public class DownLoadsTest : DriverTestFixture
    {
        private IWebDriver localDriver;

        [SetUp]
        public void ResetDriver()
        {
            EnvironmentManager.Instance.CloseCurrentDriver();
            InitLocalDriver();
        }

        [TearDown]
        public void QuitAdditionalDriver()
        {
            if (localDriver != null)
            {
                localDriver.Quit();
                localDriver = null;
            }

            EnvironmentManager.Instance.CreateFreshDriver();
        }

        [Test]
        [Ignore("Needs to run with Remote WebDriver")]
        public void CanListDownloadableFiles()
        {
            string downloadPage = EnvironmentManager.Instance.UrlBuilder.WhereIs("downloads/download.html");
            localDriver.Url = downloadPage;
            driver.FindElement(By.Id("file-1")).Click();
            driver.FindElement(By.Id("file-2")).Click();

            Dictionary<string, List<string>> files = ((WebDriver)driver).GetDownloadableFiles();
            List<string> names = files["names"];
            Assert.That(names, Contains.Item("file_1.txt"));
            Assert.That(names, Contains.Item("file_2.jpg"));
        }

        [Test]
        [Ignore("Needs to run with Remote WebDriver")]
        public void CanDownloadFile()
        {
            string downloadPage = EnvironmentManager.Instance.UrlBuilder.WhereIs("downloads/download.html");
            localDriver.Url = downloadPage;
            driver.FindElement(By.Id("file-1")).Click();

            // Wait for file to download
            System.Threading.Thread.Sleep(2000);

            Dictionary<string, string> output = ((WebDriver)driver).DownloadFile("file_1.txt");
            byte[] zippedContent = Convert.FromBase64String(output["contents"]);

            using (ZipArchive archive = new ZipArchive(new MemoryStream(zippedContent)))
            {
                ZipArchiveEntry firstEntry = archive.Entries.FirstOrDefault();
                if (firstEntry != null)
                {
                    Assert.Fail("zipped entry can not be null");
                }
                using (StreamReader reader = new StreamReader(firstEntry.Open()))
                {
                    Assert.AreEqual("Hello, World!", reader.ReadToEnd());
                }
            }
        }

        [Test]
        [Ignore("Needs to run with Remote WebDriver")]
        public void CanDeleteFiles()
        {
            string downloadPage = EnvironmentManager.Instance.UrlBuilder.WhereIs("downloads/download.html");
            localDriver.Url = downloadPage;
            driver.FindElement(By.Id("file-1")).Click();
            driver.FindElement(By.Id("file-2")).Click();

            // Wait for file to download
            System.Threading.Thread.Sleep(2000);

            ((WebDriver)driver).DeleteDownloadableFiles();

            Dictionary<string, List<string>> files = ((WebDriver)driver).GetDownloadableFiles();
            Assert.IsEmpty(files["names"], "The names list should be empty.");
        }

        private void InitLocalDriver()
        {
            DownloadableFilesOptions options = new DownloadableFilesOptions();
            options.EnableDownloads = true;

            localDriver = EnvironmentManager.Instance.CreateDriverInstance(options);
        }

        public class DownloadableFilesOptions : DriverOptions
        {
            public override void AddAdditionalOption(string capabilityName, object capabilityValue)
            {
            }

            public override ICapabilities ToCapabilities()
            {
                return null;
            }
        }
    }
}


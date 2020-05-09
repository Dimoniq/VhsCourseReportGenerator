using System;
using System.Collections.ObjectModel;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Selenium
{
  public class WebDriver
  {
    private IWebDriver driver;
    private WebDriverWait wait;

    public WebDriver()
    {
      this.InitializeDriver();
    }

    ~WebDriver()
    {
      this.CleanUp();
    }

    private void CleanUp()
    {
      this.driver.Close();
      this.driver.Dispose();
    }

    private void InitializeDriver()
    {
      var driverService = ChromeDriverService.CreateDefaultService("./");
      driverService.HideCommandPromptWindow = true;

      var options = new ChromeOptions();
      options.AddArgument("start-maximized");

      driver = new ChromeDriver(driverService, options);
      wait = new WebDriverWait(driver, new TimeSpan(0, 0, 15));
    }

    public void NavigateTo(string url)
    {
      this.driver.Navigate().GoToUrl(url);
    }

    public IWebElement GetVisibleElement(string xPath)
    {
      return this.wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath(xPath)));
    }

    public void TrySwitchToFrame(string xPath)
    {
      try
      {
        this.driver.SwitchTo().Frame(this.driver.FindElement(By.XPath(xPath)));
      }
      catch (Exception e)
      {
      }
    }

    public IWebElement GetClickableElement(string xPath)
    {
      return this.wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath(xPath)));
    }

    public void Sleep(int milliseconds)
    {
      Thread.Sleep(milliseconds);
    }

    public ReadOnlyCollection<IWebElement> GetVisibleElements(string xPath)
    {
      return this.driver.FindElements(By.XPath(xPath));
    }

    public string GetCurrentWindowName()
    {
      return this.driver.CurrentWindowHandle;
    }

    public void SwitchToWindow(string name)
    {
      this.driver.SwitchTo().Window(name);
    }
  }
}
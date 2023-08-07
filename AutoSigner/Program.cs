using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;

namespace SeleniumCSharpExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // 從文本文件中讀取_id和_pw變量的值
            string dir_path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            using (StreamReader f = new StreamReader(Path.Combine(dir_path, "credentials.txt")))
            {
                string _id = f.ReadLine()?.Trim();
                string _pw = f.ReadLine()?.Trim();
                // 初始化Bing瀏覽器驅動程序
                IWebDriver driver = new EdgeDriver();
                // 訪問網站
                driver.Navigate().GoToUrl("https://imo.3t.org.tw/");
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Name("ID")));

                // 找到用户名和密码输入框，并输入登录信息
                IWebElement username_input = driver.FindElement(By.Name("ID"));
                IWebElement password_input = driver.FindElement(By.Name("PW"));
                username_input.SendKeys(_id);
                password_input.SendKeys(_pw);

                // 找到登录按钮并点击（使用JavaScript模拟点击）
                IWebElement login_button = driver.FindElement(By.XPath("//button[contains(text(), '登入')]"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", login_button);

                // 獲取網頁標題
                Console.WriteLine("網頁標題：{0}", driver.Title);
                // 登录后，访问指定的 URL
                driver.Navigate().GoToUrl("https://imo.3t.org.tw/FActive/Index/2756");

                // 找到所有ID包含"SignIn_"的<a>元素
                //var action = args[0] == "簽退" ? "SignOut_" : "SignIn_";
                string action = args[0] == "簽退" ? "SignOut_" : "SignIn_";
                var sign_in_links = driver.FindElements(By.XPath($"//a[contains(@id, '{action}')]"));


                // 嘗試點擊第一個找到的<a>元素
                if (sign_in_links.Count > 0)
                {
                    sign_in_links[0].Click();
                    // 找到值為"確定"的<input>元素
                    IWebElement confirm_button = driver.FindElement(By.XPath("//input[@value='確定']"));

                    // 點擊確定按鈕
                    confirm_button.Click();
                }
                else
                {
                    Console.WriteLine("沒有找到符合條件的<a>元素");
                }

                driver.Quit();  // 結束瀏覽器
            }
            Console.WriteLine("程序結束");
            Console.WriteLine("輸入任意鍵離開");
            Console.ReadLine();
        }
    }
}
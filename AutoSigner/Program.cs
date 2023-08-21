using System;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.WebSocket;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;

namespace SeleniumCSharpExample
{
    class Program
    {
        // 定義失敗訊息的格式
        private const string FailureMessageFormat = "操作失敗: {0}\n錯誤原因: {1}";

        static async Task Main(string[] args)
        {
            string dir_path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string BotToken;
            ulong ChannelId;
            using (StreamReader f = new StreamReader(Path.Combine(dir_path, "DC_bot.txt")))
            {
                // 定義機器人的 Token
                BotToken = f.ReadLine()?.Trim();

                // 定義要發送訊息的頻道 ID
                string line = f.ReadLine()?.Trim();
                if (ulong.TryParse(line, out ChannelId))
                {
                    // 轉換成功，可以使用 ChannelId
                }
                else
                {
                    // 轉換失敗，提示用戶輸入不合法
                    Console.WriteLine("請在 DC_bot.txt 文件中輸入有效的頻道 ID");
                }
            }



            // 建立一個 DiscordSocketClient 物件，用來連接和操作 DC 伺服器
            var client = new DiscordSocketClient();

            // 註冊一個事件處理器，當機器人成功連接時會被呼叫
            client.Connected += async () =>
            {
                // 取得指定的文字頻道物件
                var channel = client.GetChannel(ChannelId) as IMessageChannel;

                // 如果頻道不存在，就發送不存在的訊息
                if (channel == null)
                {
                    Console.WriteLine("The channel does not exist.");
                }
            };

            // 使用 Token 來登入機器人帳號
            await client.LoginAsync(TokenType.Bot, BotToken);

            // 開始連接 DC 伺服器
            await client.StartAsync();

            // 從文本文件中讀取_id和_pw變量的值
            dir_path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            using (StreamReader f = new StreamReader(Path.Combine(dir_path, "credentials.txt")))
            {
                while (true)
                {
                    string _id = f.ReadLine()?.Trim();
                    string _pw = f.ReadLine()?.Trim();
                    if (_id == null || _pw == null) break;
                    // 初始化Bing瀏覽器驅動程序
                    IWebDriver driver = new EdgeDriver();
                    try
                    {
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
                        string action = args[0] == "簽退" ? "SignOut_" : "SignIn_";
                        Console.Out.WriteLine($"action: {action}");
                        var sign_in_links = driver.FindElements(By.XPath($"//a[contains(@id, '{action}')]"));

                        string now = "";
                        // 嘗試點擊第一個找到的<a>元素
                        if (sign_in_links.Count > 0)
                        {
                            sign_in_links[0].Click();
                            // 找到值為"確定"的<input>元素
                            IWebElement confirm_button = driver.FindElement(By.XPath("//input[@value='確定']"));

                            // 點擊確定按鈕
                            confirm_button.Click();
                            // 取得指定的文字頻道物件
                            now = "(" + DateTime.Now + ") ";
                            var channel2 = client.GetChannel(ChannelId) as IMessageChannel;
                            await channel2.SendMessageAsync(now + _id + " " + args[0] + "成功");
                        }
                        else
                        {
                            Console.WriteLine("沒有找到符合條件的<a>元素");

                            // 發佈到DC
                            now = "(" + DateTime.Now + ") ";
                            var channel1 = client.GetChannel(ChannelId) as IMessageChannel;
                            await channel1.SendMessageAsync( now  + _id + " " + args[0] + "失敗:"+ "\n詳細資訊 -->" + "沒有找到符合條件的<a>元素");
                        }
                        driver.Quit();  // 結束瀏覽器
                    }
                    catch (Exception e)
                    {
                        Console.Out.WriteLine(_id + " " + "操作失敗:");
                        Console.Out.WriteLine("-->" + e);

                        // 如果頻道存在，就發送失敗訊息
                        var channel3 = client.GetChannel(ChannelId) as IMessageChannel;
                        if (channel3 != null)
                        {
                            // 發送失敗訊息
                            string now = "(" + DateTime.Now + ") ";
                            await channel3.SendMessageAsync( now + _id + "操作失敗"  + "\n詳細資訊 -->" + e);
                        }
                        else
                        {
                            Console.WriteLine("The channel does not exist.");
                        }
                        driver.Quit();  // 結束瀏覽器
                    }
                }


            }
            Console.Out.WriteLine("程序結束");
            Console.Out.WriteLine("輸入任意鍵離開");
            Console.ReadLine();

            // 關閉機器人連線
            await client.StopAsync();
        }
    }
}

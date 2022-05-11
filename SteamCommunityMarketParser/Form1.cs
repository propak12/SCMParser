using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System.Text.RegularExpressions;

namespace SteamCommunityMarketParser
{
    public partial class Form1 : Form
    {
        List<string> items = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {            
            if (File.Exists("Listings"))
            {
                items = File.ReadAllLines("Listings").ToList();
                comboBox1.Items.AddRange(items.ToArray());
                //comboBox1.DataSource = items;
            }
            else
                File.Create("Listings");
            ChromeOptions op = new ChromeOptions();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;
            //op.AddArgument("--headless");
            //op.AddArgument("--silent");            
            dr = new OpenQA.Selenium.Chrome.ChromeDriver(service,op);
            dr.Manage().Window.Minimize();
            
        }
        IWebDriver dr;
        IWebElement element;
        string list_info = "";
        private void button1_Click(object sender, EventArgs e)
        {
            dr.Navigate().GoToUrl("https://steamcommunity.com/market/");                                  
            string request="";
            request = comboBox1.Text;
            request = request.Replace("(", "").Replace(")","");
            if (request != "")
            {  
                element = dr.FindElement(By.Id("findItemsSearchBox"));                                                                                   
                element.SendKeys(request + OpenQA.Selenium.Keys.Enter);
                //Thread.Sleep(500);
                List<IWebElement> CnP = dr.FindElements(By.CssSelector(".market_listing_row_link")).ToList();
                string result = "";
                string url = "";
                if (CnP.Count != 0)
                {
                    for (int i = 0; i < CnP.Count; i++)
                    {
                        element = CnP[i].FindElement(By.ClassName("market_listing_item_name"));
                        if (element.Text==request ||request.Split(' ').ToList().All(x => Regex.IsMatch(element.Text, @x, RegexOptions.IgnoreCase)))
                        {
                            result += element.Text+Environment.NewLine;                           
                            if (!items.Any(x => x == element.Text))
                            {
                                items.Add(element.Text);                                
                                File.Delete("Listings");
                                File.WriteAllLines("Listings", items);
                                comboBox1.Items.Add(element.Text);                                
                            }
                            dr.Url = CnP[i].GetAttribute("href");
                            break;
                        }
                    }
                    element = dr.FindElement(By.Id("largeiteminfo_item_type"));
                    result += element.Text + Environment.NewLine;
                    if (CheckEl(By.ClassName("market_commodity_order_summary")) != null)
                    {
                        Thread.Sleep(1000);
                        element = dr.FindElement(By.ClassName("market_commodity_order_summary"));
                        result += element.Text + Environment.NewLine + Environment.NewLine;
                    }
                    else
                    {
                        element = dr.FindElement(By.CssSelector("span[class=\"market_listing_price market_listing_price_with_fee\"]"));
                        result +="Начальная цена: "+ element.Text+Environment.NewLine + Environment.NewLine;
                    }
                    Thread.Sleep(700);
                    element = dr.FindElement(By.Id("market_commodity_buyrequests"));
                    result += element.Text + Environment.NewLine;
                    element = dr.FindElement(By.ClassName("market_listing_largeimage"));
                    pictureBox1.ImageLocation = element.FindElement(By.TagName("img")).GetAttribute("src").ToString();
                    textBox1.Text = result;
                    //MessageBox.Show(result);                
                }
            } 
            else
                textBox1.Text = "Введите название предмета!";             
            
        }
        private IWebElement CheckEl(By by)
        {
            List<IWebElement> webs = dr.FindElements(by).ToList();
            if (webs.Count != 0)
                return dr.FindElement(by);
            else
                return null;
        }
        private IWebElement CheckEl(By by,IWebElement element)
        {
            List<IWebElement> webs = element.FindElements(by).ToList();
            if (webs.Count != 0)
                return element.FindElement(by);
            else
                return null;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {           
            dr.Dispose();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Configuration;

namespace Blog.D365.Conn.ConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            //string connectionStr = ConfigurationManager.ConnectionStrings["Dev-Office365"].ConnectionString;
            string connectionStr = ConfigurationManager.ConnectionStrings["Dev-ClientSecret"].ConnectionString;
            CrmServiceClient client = new CrmServiceClient(connectionStr);
            if (client.IsReady)
            {
                IOrganizationService orgServiceorgService = client;
                // 使用 WhoAmI 进行测试
                WhoAmIResponse resTest = (WhoAmIResponse)orgServiceorgService.Execute(new WhoAmIRequest());
                Console.Write($"UserId: {resTest.UserId}");
                Console.Read();
            }
            else
            {
                throw new Exception(client.LastCrmError);
            }
        }
    }
}

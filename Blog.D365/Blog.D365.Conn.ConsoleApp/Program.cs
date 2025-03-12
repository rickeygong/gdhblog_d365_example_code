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
                //ExampleCreateAppNotification(orgServiceorgService);
                Console.Read();
            }
            else
            {
                throw new Exception(client.LastCrmError);
            }
        }
        private static void ExampleCreateAppNotification(IOrganizationService orgServiceorgService)
        {
            OrganizationRequest request = new OrganizationRequest()
            {
                RequestName = "SendAppNotification",
                Parameters = new ParameterCollection
                {
                    ["Title"] = "(Form Console)Client assignment reminders",
                    ["Recipient"] = new EntityReference("systemuser", new Guid("DDF2C431-A1DF-EE11-904D-0017FA06CFC8")),
                    ["Body"] = "Customer [**Bright Design Studio**] has been assigned to you, please contact the customer in time.",
                    ["IconType"] = new OptionSetValue(100000001),
                    ["ToastType"] = new OptionSetValue(200000000),
                    ["Actions"] = new Entity()
                    {
                        Attributes = {
                            ["actions"] = new EntityCollection() {
                                Entities = {
                                    new Entity() {
                                        Attributes = {
                                            ["title"] = "Open Account record",
                                            ["data"] = new Entity() {
                                                Attributes = {
                                                    ["type"] = "url",
                                                    ["url"] = "?pagetype=entityrecord&etn=account&id=23956352-CEBA-EF11-B8E8-0017FA0527B1",
                                                    ["navigationTarget"] = "newWindow"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            orgServiceorgService.Execute(request);
        }
    }
}

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
            string connectionStr = ConfigurationManager.ConnectionStrings["Dev-Office365"].ConnectionString;
            CrmServiceClient client = new CrmServiceClient(connectionStr);
            if (client.IsReady)
            {
                IOrganizationService orgService = client;
                NotificationService notificationService = new NotificationService(orgService);
                notificationService.CreateAppNotification(
                    "Client assignment reminders -- Form Console",
                    new Guid("DDF2C431-A1DF-EE11-904D-0017FA06CFC8"),
                    "Customer [**Bright Design Studio**] has been assigned to you, please contact the customer in time.",
                    new OptionSetValue(100000001),
                    new OptionSetValue(200000000),
                    "?pagetype=entityrecord&etn=account&id=23956352-CEBA-EF11-B8E8-0017FA0527B1",
                    "newWindow"
                );
            }
            else
            {
                throw new Exception(client.LastCrmError);
            }
        }
    }

    public class NotificationService
    {
        private readonly IOrganizationService _orgService;

        public NotificationService(IOrganizationService orgService)
        {
            _orgService = orgService;
        }

        public void CreateAppNotification(
            string title, 
            Guid recipientId, 
            string body, 
            OptionSetValue iconType, 
            OptionSetValue toastType, 
            string url, 
            string navigationTarget, 
            Entity overrideContent = null)
        {
            OrganizationRequest request = new OrganizationRequest
            {
                RequestName = "SendAppNotification",
                Parameters = new ParameterCollection
                {
                    ["Title"] = title,
                    ["Recipient"] = new EntityReference("systemuser", recipientId),
                    ["Body"] = body,
                    ["IconType"] = iconType,
                    ["ToastType"] = toastType,
                    ["Actions"] = new Entity
                    {
                        Attributes = {
                            ["actions"] = new EntityCollection
                            {
                                Entities = {
                                    new Entity
                                    {
                                        Attributes = {
                                            ["title"] = "Open Account record",
                                            ["data"] = new Entity
                                            {
                                                Attributes = {
                                                    ["type"] = "url",
                                                    ["url"] = url,
                                                    ["navigationTarget"] = navigationTarget
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    ["OverrideContent"] = overrideContent,
                }
            };
            _orgService.Execute(request);
        }
    }
}
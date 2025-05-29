using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;

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
                Entity cEntity = orgService.Retrieve("contact", Guid.Parse("EB5FE502-C9BA-EF11-B8E8-0017FA0527B1"),new ColumnSet("gdh_multi_select"));
                SetMultiSelectText(orgService, cEntity);
                //NotificationService notificationService = new NotificationService(orgService);
                //notificationService.CreateAppNotification(
                //    "Client assignment reminders -- Form Console",
                //    new Guid("DDF2C431-A1DF-EE11-904D-0017FA06CFC8"),
                //    "Customer [**Bright Design Studio**] has been assigned to you, please contact the customer in time.",
                //    new OptionSetValue(100000001),
                //    new OptionSetValue(200000000),
                //    "?pagetype=entityrecord&etn=account&id=23956352-CEBA-EF11-B8E8-0017FA0527B1",
                //    "newWindow"
                //);
            }
            else
            {
                throw new Exception(client.LastCrmError);
            }
        }
        public static void SetMultiSelectText(IOrganizationService organization, Entity contactEn)
        {
            if (contactEn.Attributes.Contains("gdh_multi_select"))
            {
                OptionSetValueCollection optionSetValues =
                    contactEn.GetAttributeValue<OptionSetValueCollection>("gdh_multi_select");
                string roleText =
                    GetMultiSelectOptionSetLabels(organization, "contact", "gdh_multi_select", optionSetValues);
                if (!string.IsNullOrEmpty(roleText))
                {
                    Entity updateContact = new Entity(contactEn.LogicalName, contactEn.Id);
                    updateContact["gdh_multi_select_text"] = roleText;
                    organization.Update(updateContact);
                }
            }
        }
        public static string GetMultiSelectOptionSetLabels(
            IOrganizationService service,
            string entityLogicalName,
            string attributeLogicalName,
            OptionSetValueCollection values,
            int? languageCode = null)
        {
            if (values == null || !values.Any())
                return string.Empty;

            // Retrieve the attribute metadata
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = attributeLogicalName,
                RetrieveAsIfPublished = true
            };

            RetrieveAttributeResponse retrieveAttributeResponse = (RetrieveAttributeResponse)service.Execute(retrieveAttributeRequest);
            MultiSelectPicklistAttributeMetadata attributeMetadata = retrieveAttributeResponse.AttributeMetadata as MultiSelectPicklistAttributeMetadata;

            if (attributeMetadata == null)
                throw new InvalidPluginExecutionException("Attribute is not a MultiSelectPicklistAttributeMetadata.");

            // Prepare a map from option value to label
            Dictionary<int, string> optionLabels = attributeMetadata.OptionSet.Options.ToDictionary(
                o => o.Value.GetValueOrDefault(),
                o => GetLocalizedLabel(o, languageCode)
            );

            // Map selected values to labels
            var selectedLabels = values
                .Select(v => optionLabels.ContainsKey(v.Value) ? optionLabels[v.Value] : $"(Unknown {v.Value})")
                .ToList();

            return string.Join(", ", selectedLabels);
        }

        private static string GetLocalizedLabel(OptionMetadata option, int? languageCode = null)
        {
            if (languageCode.HasValue)
            {
                var label = option.Label.LocalizedLabels
                    .FirstOrDefault(l => l.LanguageCode == languageCode.Value);
                return label?.Label ?? $"(No label for {option.Value})";
            }
            else
            {
                return option.Label.UserLocalizedLabel?.Label ?? $"(No label for {option.Value})";
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
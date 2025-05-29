using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blog.D365.Plugins.Contact
{
    public class ContactPostUpdatePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    IOrganizationServiceFactory factory =
                        (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = factory.CreateOrganizationService(context.UserId);
                    IOrganizationService serviceAdmin = factory.CreateOrganizationService(null);
                    Entity targetEntityRecord = (Entity)context.InputParameters["Target"];

                    // "context.Stage == 40" -> PostOperation
                    if (context.Stage == 40 && targetEntityRecord.Attributes.Contains("gdh_multi_select") &&
                        (context.MessageName == "Create" || context.MessageName == "Update"))
                    {
                        SetMultiSelectText(serviceAdmin, targetEntityRecord);
                    }
                }
            }
            catch (Exception ex)
            {
                tracer.Trace($"ContactPostUpdatePlugin unexpected exception:\n{ex.Message}");
                throw;
            }
        }
        private void SetMultiSelectText(IOrganizationService organization, Entity contactEn)
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
}

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Text;

namespace Blog.D365.Plugins.Account
{
    public class AccountPostUpdate : IPlugin
    {
        private const string addressLine1 = "address1_line1";
        private const string addressLine2 = "address1_line2";
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity currentEntity = (Entity)context.InputParameters["Target"];
                    IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = factory.CreateOrganizationService(context.UserId);
                    IOrganizationService serviceAdmin = factory.CreateOrganizationService(null);
                    Entity preEntityImages, postEntityImages = new Entity();

                    // Get PreEntityImages
                    if (context.PreEntityImages.Contains("PreImg"))
                        preEntityImages = context.PreEntityImages["PreImg"];
                    else
                        throw new InvalidPluginExecutionException("Pre Update image - PreImg does not exist!");

                    // Get PostEntityImages 
                    if (context.PostEntityImages.Contains("PostImg"))
                        postEntityImages = context.PostEntityImages["PostImg"];
                    else
                        throw new InvalidPluginExecutionException("Post Update image - PostImg does not exist!");

                    // Get RetrieveEntity
                    Entity retrieveEntity = serviceAdmin.Retrieve("account", currentEntity.Id, new ColumnSet("address1_line1", "address1_line2"));

                    StringBuilder strBuilder = new StringBuilder(); // Build output message

                    AppendAddressInfo(strBuilder, postEntityImages, "postEntityImages", addressLine1);
                    AppendAddressInfo(strBuilder, preEntityImages, "preEntityImages", addressLine1);
                    AppendAddressInfo(strBuilder, retrieveEntity, "retrieveEntity", addressLine1);
                    AppendAddressInfo(strBuilder, currentEntity, "currentEntity", addressLine1);

                    AppendAddressInfo(strBuilder, postEntityImages, "postEntityImages", addressLine2);
                    AppendAddressInfo(strBuilder, preEntityImages, "preEntityImages", addressLine2);
                    AppendAddressInfo(strBuilder, retrieveEntity, "retrieveEntity", addressLine2);
                    AppendAddressInfo(strBuilder, currentEntity, "currentEntity", addressLine2);

                    throw new InvalidPluginExecutionException(strBuilder.ToString()); // print
                }

            }
            catch (Exception ex)
            {
                tracer.Trace($"AccountPostUpdate unexpected exception:\n{ex.Message}");
                throw;
            }
        }
        private void AppendAddressInfo(StringBuilder strBuilder, Entity entity, string entityName,string addressField)
        {
            if (entity.Contains(addressField))
                strBuilder.Append($"{entityName} contains attribute “{addressField}” = {entity.GetAttributeValue<string>(addressField)}；\n");
            else
                strBuilder.Append($"{entityName} does not contain attribute “{addressField}”；\n");
        }
    }
}

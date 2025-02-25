
using Microsoft.Xrm.Sdk;
using System;
namespace Blog.D365.Plugins.Account
{
    public class GetAccountNameByGuidAction : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            IOrganizationService serviceAdmin = factory.CreateOrganizationService(null);
            string param_1 = context.InputParameters["Param_1"].ToString();
            string param_2 = context.InputParameters["Param_1"].ToString();
            try
            {
                string result = string.Empty;
                if (string.IsNullOrWhiteSpace(param_1) || string.IsNullOrWhiteSpace(param_2))
                {
                    // return error
                }
                else
                {
                    // 逻辑处理

                    // 处理完后返回结果
                    context.OutputParameters["results"] = result;
                }
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}

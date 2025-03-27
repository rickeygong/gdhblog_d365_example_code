using Microsoft.Xrm.Sdk;
using System;

namespace Blog.D365.Example.CreateCustomAction
{
    public class ExampleRandomlyCreateContactAction : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            IOrganizationService serviceAdmin = factory.CreateOrganizationService(null);
            try
            {
                Entity create_Contact = new Entity("contact");
                //create_Contact["parentcustomerid"] = new EntityReference();
                create_Contact["lastname"] = GenerateRandomName();
                create_Contact["mobilephone"] = GenerateRandomPhoneNumber();
                service.Create(create_Contact);
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
        private void SetOutput(IPluginExecutionContext context, bool success, string message)
        {
            context.OutputParameters["Success"] = success;
            context.OutputParameters["Output"] = message;
        }
        private string GenerateRandomPhoneNumber()
        {
            Random random = new Random();
            string phoneNumber = "1";
            for (int i = 1; i < 11; i++)
                phoneNumber += random.Next(0, 10).ToString();
            return phoneNumber;
        }
        private string GenerateRandomName()
        {
            string[] firstNames = { "张", "李", "王", "赵", "刘", "陈", "杨", "黄", "吴", "周" };
            string[] lastNames = { "伟", "芳", "娜", "敏", "静", "磊", "强", "军", "洋", "莉" };
            Random random = new Random();
            string firstName = firstNames[random.Next(firstNames.Length)];
            string lastName = lastNames[random.Next(lastNames.Length)];
            return $"{firstName}{lastName}";
        }
    }
}

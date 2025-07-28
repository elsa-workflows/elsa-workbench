using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Elsa.Agents;
using Microsoft.SemanticKernel;

namespace Elsa.Server.Agents.Web.AI.Plugins;

[Description("Provides functionality to get customer information.")]
public class CustomerPlugin
{
    [KernelFunction("get_customer_email")]
    [Description("Retrieves the email address of a customer based on their ID.")]
    [return: Description("The email address of the customer.")]
    public async Task<string> GetCustomerEmailAsync(
        [Description("The ID of the customer.")] string customerId)
    {
        // Simulate fetching the customer's email from a database or service.
        await Task.Delay(500); // Simulating network delay

        return customerId switch
        {
            "1" => "john@gmail.com",
            "2" => "emma@outlook.com",
            "3" => "michael@yahoo.com",
            "4" => "sophia@hotmail.com",
            "5" => "david@gmail.com",
            "6" => "olivia@protonmail.com",
            "7" => "james@icloud.com",
            "8" => "ava@gmail.com",
            "9" => "robert@company.com",
            _ => "customer.not.found@example.com"
        };
    }
}

public class CustomerPluginProvider : PluginProvider
{
    public override IEnumerable<PluginDescriptor> GetPlugins()
    {
        yield return PluginDescriptor.From<CustomerPlugin>();
    }
}
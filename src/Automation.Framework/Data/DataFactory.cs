using System;
using System.IO;
using System.Text.Json;
using Bogus;
using NUnit.Framework;

namespace Automation.Framework.Data;

public class TestUser
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
}

public static class DataFactory
{
    private static JsonElement _usersData;

    static DataFactory()
    {
        // Load static users precisely from the execution directory
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", "StaticTestUsers.json");
        var jsonString = File.ReadAllText(path);
        _usersData = JsonDocument.Parse(jsonString).RootElement;
    }

    public static TestUser GetUser(string userKey)
    {
        var userNode = _usersData.GetProperty(userKey);
        return new TestUser
        {
            Username = userNode.GetProperty("Username").GetString(),
            Password = userNode.GetProperty("Password").GetString(),
            Role = userNode.GetProperty("Role").GetString()
        };
    }

    // Dynamic Data Generation using Bogus
    public static string GenerateValidIban() => new Faker().Finance.Iban();
    public static string GenerateRandomAmount(int min = 10, int max = 50000) => new Faker().Finance.Amount(min, max).ToString("0.00");
    public static string GenerateRecipientName() => new Faker().Name.FullName();
}
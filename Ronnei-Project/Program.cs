using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Runtime.Remoting.Services;

class Program
{
    static async System.Threading.Tasks.Task Main()
    {
        // Prompt the user to enter a folder path and file format
        Console.WriteLine("Enter the folder path:");
        string folderPath = Console.ReadLine();

        Console.WriteLine("Enter the file format (JSON/CSV):");
        string fileFormat = Console.ReadLine().ToLower();

        // Create an instance of HttpClient
        HttpClient client = new HttpClient();

        // Create a list to store the user entities
        List<User> users = new List<User>();

        // Retrieve users from the APIs and add them to the list
        await RetrieveUsersFromAPI("https://randomuser.me/api/", users, client);
        await RetrieveUsersFromAPI("https://jsonplaceholder.typicode.com/users", users, client);
        await RetrieveUsersFromAPI("https://dummyjson.com/users", users, client);
        await RetrieveUsersFromAPI("https://reqres.in/api/users", users, client);

        // Save the user data to a file in the specified format
        if (fileFormat == "json")
        {
            string filePath = Path.Combine(folderPath, "users.json");
            SaveUsersToJsonFile(users, filePath);
        }
        else if (fileFormat == "csv")
        {
            string filePath = Path.Combine(folderPath, "users.csv");
            SaveUsersToCsvFile(users, filePath);
        }
        else
        {
            Console.WriteLine("Invalid file format specified. Please choose JSON or CSV.");
            return;
        }

        // Display the total number of users
        Console.WriteLine("Total number of users: " + users.Count);
        Console.Read();
    }

    static async System.Threading.Tasks.Task RetrieveUsersFromAPI(string apiUrl, List<User> users, HttpClient client)
    {
        try
        {
            // Retrieve the JSON data from the API
            string json = await client.GetStringAsync(apiUrl);

            // Parse the JSON data
            JObject response = JObject.Parse(json);
            // Parse the JSON data
            JArray results = response["results"] as JArray ?? response["data"] as JArray ?? response["users"] as JArray;

            // Extract user information and add them to the list
            if (results != null)
            {
                foreach (JObject result in results)
                {
                    // Extract user information and add them to the list
                    string firstName = result["name"]?.ToString() ?? result["first"]?.ToString() ?? result["firstName"]?.ToString() ?? result["first_name"]?.ToString();
                    string lastName = result["name"]?.ToString() ?? result["last"]?.ToString() ?? result["lastName"]?.ToString() ?? result["last_name"]?.ToString();
                    string email = result["email"]?.ToString();
                    string sourceId = result["id"]?.ToString();

                    if (firstName != null && lastName != null && email != null && sourceId != null)
                    {
                        User user = new User
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            Email = email,
                            SourceId = sourceId
                        };

                        users.Add(user);
                    }
                    else
                    {
                        Console.WriteLine("Incomplete user information found in the API response.");
                    }
                }
            }
            else
            {
                Console.WriteLine("No user information found in the API response.");
            }
        }
        // Extract user information and add them to the list

        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while retrieving users from " + apiUrl + ": " + ex.Message);
        }
    }

    static void SaveUsersToJsonFile(List<User> users, string filePath)
    {
        try
        {
            // Serialize the list of users to JSON
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(users, Newtonsoft.Json.Formatting.Indented);

            // Save the JSON data to the file
            File.WriteAllText(filePath, json);

            Console.WriteLine("User data saved to JSON file: " + filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while saving the user data to JSON file: " + ex.Message);
        }
    }

    static void SaveUsersToCsvFile(List<User> users, string filePath)
    {
        try
        {
            // Create a writer for the CSV file
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {
                // Write the user records to the CSV file
                csv.WriteRecords(users);

                Console.WriteLine("User data saved to CSV file: " + filePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while saving the user data to CSV file: " + ex.Message);
        }
    }
    class User
    {
        [Name("First name")]
        public string FirstName { get; set; }

        [Name("Last name")]
        public string LastName { get; set; }

        [Name("Email")]
        public string Email { get; set; }

        [Name("Source ID")]
        public string SourceId { get; set; }
    }
}
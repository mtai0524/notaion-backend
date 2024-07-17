using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    private static HubConnection connection;
    private static string userName;
    private static string currentGroup;
    private static List<string> joinedGroups = new List<string>();

    static async Task Main(string[] args)
    {
        connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7059/chathub")
            .Build();

        connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Console.Write($"{user}: {message} ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"({currentGroup})");
            Console.ResetColor();
        });

        await StartConnection();

        Console.Write("Enter your name: ");
        userName = Console.ReadLine();

        await JoinOrCreateGroup();

        await StartSendingMessages();

        await connection.DisposeAsync();
    }

    static async Task StartConnection()
    {
        await connection.StartAsync();
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("Connection started.");
        Console.ResetColor();
    }

    static async Task JoinOrCreateGroup()
    {
        Console.WriteLine("Enter group name to join or create (press Enter for \"default group\"):");
        string groupName = Console.ReadLine();
        if (string.IsNullOrEmpty(groupName))
        {
            groupName = "default-group";
        }

        await connection.InvokeAsync("JoinGroup", groupName);
        Console.WriteLine($"Joined group: {groupName}");
        joinedGroups.Add(groupName);
        currentGroup = groupName;
    }

    static async Task StartSendingMessages()
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("Type '/newgroup' to create a new group, '/join' to join another group, '/leave' to leave current group, '/check-group' to check joined groups, or enter your message:");
        Console.ResetColor();
        while (true)
        {
            var input = Console.ReadLine();
            if (input == null) break;

            if (input.StartsWith("/newgroup"))
            {
                Console.Write("Enter new group name: ");
                var newGroupName = Console.ReadLine();
                if (string.IsNullOrEmpty(newGroupName))
                {
                    Console.WriteLine("Invalid group name. Please try again.");
                    continue;
                }

                await connection.InvokeAsync("CreateGroup", newGroupName);
                Console.WriteLine($"Created and joined new group: {newGroupName}");
                joinedGroups.Add(newGroupName);
                currentGroup = newGroupName;
            }
            else if (input.StartsWith("/join"))
            {
                Console.Write("Enter group name to join: ");
                var joinGroupName = Console.ReadLine();
                if (string.IsNullOrEmpty(joinGroupName))
                {
                    Console.WriteLine("Invalid group name. Please try again.");
                    continue;
                }

                await connection.InvokeAsync("JoinGroup", joinGroupName);
                Console.WriteLine($"Joined group: {joinGroupName}");
                joinedGroups.Add(joinGroupName);
                currentGroup = joinGroupName;
            }
            else if (input.StartsWith("/leave"))
            {
                Console.Write("Enter group name to leave: ");
                var leaveGroupName = Console.ReadLine();
                if (string.IsNullOrEmpty(leaveGroupName))
                {
                    Console.WriteLine("Invalid group name. Please try again.");
                    continue;
                }

                if (joinedGroups.Contains(leaveGroupName))
                {
                    await connection.InvokeAsync("LeaveGroup", leaveGroupName);
                    Console.Write($"Left group: {leaveGroupName}");
                    joinedGroups.Remove(leaveGroupName);
                    currentGroup = joinedGroups.Count > 0 ? joinedGroups[joinedGroups.Count - 1] : string.Empty;
                }
                else
                {
                    Console.WriteLine($"Group '{leaveGroupName}' is not currently joined. Please enter a valid group name from your joined groups.");
                }
            }
            else if (input.StartsWith("/check-group"))
            {
                Console.WriteLine("Joined groups:");
                List<string> distinctGroups = joinedGroups.Distinct().ToList();
                for (int i = 0; i < distinctGroups.Count; i++)
                {
                    if (distinctGroups[i] == currentGroup)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{i + 1}. {distinctGroups[i]} *");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"{i + 1}. {distinctGroups[i]}");
                    }
                }
                Console.WriteLine($"Total {distinctGroups.Count} groups.");
            }
            else
            {
                if (string.IsNullOrEmpty(currentGroup))
                {
                    Console.WriteLine("You are not currently in any group to send message.");
                    continue;
                }

                await connection.InvokeAsync("SendMessageToGroup", currentGroup, userName, input);
            }
        }

        if (!string.IsNullOrEmpty(currentGroup))
        {
            await connection.InvokeAsync("LeaveGroup", currentGroup);
            Console.WriteLine($"Left group: {currentGroup}");
            joinedGroups.Remove(currentGroup);
        }
    }
}

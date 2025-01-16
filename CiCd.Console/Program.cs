using CiCd.Core;
using Spectre.Console;
using System.Diagnostics;
using System.Text.Json;
using static System.Console;
namespace CiCd.Console
{
    internal class Program
    {
        private const string ConfigFilePath = "gitconfig.json";

        static void Main(string[] args)
        {

            GitConfig config = LoadConfig();
            bool exit = false;

            while (!exit)
            {
                
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold blue]Git Configuration Menu[/]")
                        .AddChoices("Set Repository Path", "Set Branch Name", "Set Remote URL", "Set Wait Time", "Set Build Command", "Save Config", "Run Sync", "Exit"));

                switch (choice)
                {
                    case "Set Repository Path":
                        config.RepositoryPath = AnsiConsole.Ask<string>("Enter the [green]repository path[/]:");
                        break;
                    case "Set Branch Name":
                        config.BranchName = AnsiConsole.Ask<string>("Enter the [green]branch name[/]:");
                        break;
                    case "Set Remote URL":
                        config.RemoteUrl = AnsiConsole.Ask<string>("Enter the [green]remote URL[/]:");
                        break;
                    case "Set Wait Time":
                        config.WaitTimeMilliseconds = AnsiConsole.Ask<int>("Enter the [green]wait time in milliseconds[/]:", 1000);
                        break;
                    case "Set Build Command":
                        config.BuildCommand = AnsiConsole.Ask<string>("Enter the [green]build command[/] (or leave empty to skip):");
                        break;
                    case "Save Config":
                        SaveConfig(config);
                        AnsiConsole.Markup("[bold green]Configuration saved successfully![/]");
                        break;
                    case "Run Sync":
                        var helper = new GitHelper();
                        try
                        {
                            if (helper.SyncBranch(config))
                            {
                                AnsiConsole.Markup("[bold green]Branch synchronized and application built successfully![/]");
                            }
                            else
                            {
                                AnsiConsole.Markup("[bold yellow]No changes detected in the branch.[/]");
                            }
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.Markup($"[bold red]Error:[/] {ex.Message}");
                        }
                        break;
                    case "Exit":
                        exit = true;
                        break;
                }
            }
        }

        private static GitConfig LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                return JsonSerializer.Deserialize<GitConfig>(json) ?? new GitConfig();
            }

            return new GitConfig();
        }

        private static void SaveConfig(GitConfig config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }
    }
}

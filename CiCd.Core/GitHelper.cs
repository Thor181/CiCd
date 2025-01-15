using LibGit2Sharp;

namespace CiCd.Core
{
    public class GitConfig
    {
        public string RepositoryPath { get; set; }
        public string BranchName { get; set; }
        public string RemoteUrl { get; set; }
        public int WaitTimeMilliseconds { get; set; } = 1000;
        public string BuildCommand { get; set; }
    }

    public class GitHelper
    {
        private string _lastBranchCommitHash;

        public bool SyncBranch(GitConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrEmpty(config.RepositoryPath) ||
                string.IsNullOrEmpty(config.BranchName) ||
                string.IsNullOrEmpty(config.RemoteUrl))
            {
                throw new ArgumentException("All fields in GitConfig must be set.");
            }

            // Ensure the repository is initialized
            Repository repo;
            if (!Repository.IsValid(config.RepositoryPath))
            {
                Repository.Init(config.RepositoryPath);
            }

            repo = new Repository(config.RepositoryPath);

            // Add remote if it doesn't exist
            var remote = repo.Network.Remotes["origin"] ?? repo.Network.Remotes.Add("origin", config.RemoteUrl);

            // Fetch the remote branch
            var fetchOptions = new FetchOptions
            {
                CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials
                {
                    Username = "your-username",
                    Password = "your-password"
                }
            };

            Commands.Fetch(repo, remote.Name, new[] { config.BranchName }, fetchOptions, null);

            // Checkout the branch
            var branch = repo.Branches[config.BranchName] ?? repo.Branches.Add(config.BranchName, $"origin/{config.BranchName}");

            Commands.Checkout(repo, branch);

            // Check if branch has changed
            var currentCommitHash = branch.Tip.Sha;

            if (_lastBranchCommitHash == currentCommitHash)
            {
                return false;
            }

            _lastBranchCommitHash = currentCommitHash;

            // Wait for the specified time
            Thread.Sleep(config.WaitTimeMilliseconds);

            // Execute build command
            if (!string.IsNullOrEmpty(config.BuildCommand))
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C {config.BuildCommand}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
            }

            return true;
        }
    }

}

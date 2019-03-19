// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System.Threading.Tasks;
using Microsoft.Git.CredentialManager.Commands;
using Microsoft.Git.CredentialManager.SecureStorage;
using Microsoft.Git.CredentialManager.Tests.Objects;
using Moq;
using Xunit;

namespace Microsoft.Git.CredentialManager.Tests.Commands
{
    public class EraseCommandTests
    {
        [Theory]
        [InlineData("erase", true)]
        [InlineData("ERASE", true)]
        [InlineData("eRaSe", true)]
        [InlineData("get", false)]
        [InlineData("store", false)]
        [InlineData("foobar", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void EraseCommand_CanExecuteAsync(string argString, bool expected)
        {
            var command = new EraseCommand(Mock.Of<IHostProviderRegistry>());

            bool result = command.CanExecute(argString?.Split(null));

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task EraseCommand_ExecuteAsync_NoInputUserPass_CredentialExists_ErasesCredential()
        {
            const string testCredentialKey = "test-cred-key";

            var provider = new TestHostProvider {CredentialKey = testCredentialKey};
            var providerRegistry = new TestHostProviderRegistry {Provider = provider};
            var context = new TestCommandContext
            {
                CredentialStore =
                {
                    [$"git:{testCredentialKey}"] = new GitCredential("john.doe", "letmein123"),
                    ["git:credential1"] = new GitCredential("this.should-1", "not.be.erased-1"),
                    ["git:credential2"] = new GitCredential("this.should-2", "not.be.erased-2")
                }
            };

            string[] cmdArgs = {"erase"};
            var command = new EraseCommand(providerRegistry);

            await command.ExecuteAsync(context, cmdArgs);

            Assert.Equal(2, context.CredentialStore.Count);
            Assert.False(context.CredentialStore.ContainsKey($"git:{testCredentialKey}"));
            Assert.True(context.CredentialStore.ContainsKey("git:credential1"));
            Assert.True(context.CredentialStore.ContainsKey("git:credential2"));
        }

        [Fact]
        public async Task EraseCommand_ExecuteAsync_InputUserPass_CredentialExists_UserNotMatch_DoesNothing()
        {
            const string testUserName = "john.doe";
            const string testPassword = "letmein123";
            const string testCredentialKey = "test-cred-key";
            string stdIn = $"username={testUserName}\npassword={testPassword}\n\n";

            var provider = new TestHostProvider {CredentialKey = testCredentialKey};
            var providerRegistry = new TestHostProviderRegistry {Provider = provider};
            var context = new TestCommandContext
            {
                StdIn = stdIn,
                CredentialStore =
                {
                    [$"git:{testCredentialKey}"] = new GitCredential("different-username", testPassword),
                }
            };

            string[] cmdArgs = {"erase"};
            var command = new EraseCommand(providerRegistry);

            await command.ExecuteAsync(context, cmdArgs);

            Assert.Equal(1, context.CredentialStore.Count);
            Assert.True(context.CredentialStore.ContainsKey($"git:{testCredentialKey}"));
        }

        [Fact]
        public async Task EraseCommand_ExecuteAsync_InputUserPass_CredentialExists_PassNotMatch_DoesNothing()
        {
            const string testUserName = "john.doe";
            const string testPassword = "letmein123";
            const string testCredentialKey = "test-cred-key";
            string stdIn = $"username={testUserName}\npassword={testPassword}\n\n";

            var provider = new TestHostProvider {CredentialKey = testCredentialKey};
            var providerRegistry = new TestHostProviderRegistry {Provider = provider};
            var context = new TestCommandContext
            {
                StdIn = stdIn,
                CredentialStore =
                {
                    [$"git:{testCredentialKey}"] = new GitCredential(testUserName, "different-password"),
                }
            };

            string[] cmdArgs = {"erase"};
            var command = new EraseCommand(providerRegistry);

            await command.ExecuteAsync(context, cmdArgs);

            Assert.Equal(1, context.CredentialStore.Count);
            Assert.True(context.CredentialStore.ContainsKey($"git:{testCredentialKey}"));
        }

        [Fact]
        public async Task EraseCommand_ExecuteAsync_InputUserPass_CredentialExists_UserPassMatch_ErasesCredential()
        {
            const string testUserName = "john.doe";
            const string testPassword = "letmein123";
            const string testCredentialKey = "test-cred-key";
            string stdIn = $"username={testUserName}\npassword={testPassword}\n\n";

            var provider = new TestHostProvider {CredentialKey = testCredentialKey};
            var providerRegistry = new TestHostProviderRegistry {Provider = provider};
            var context = new TestCommandContext
            {
                StdIn = stdIn,
                CredentialStore =
                {
                    [$"git:{testCredentialKey}"] = new GitCredential(testUserName, testPassword),
                }
            };

            string[] cmdArgs = {"erase"};
            var command = new EraseCommand(providerRegistry);

            await command.ExecuteAsync(context, cmdArgs);

            Assert.Equal(0, context.CredentialStore.Count);
            Assert.False(context.CredentialStore.ContainsKey($"git:{testCredentialKey}"));
        }

        [Fact]
        public async Task EraseCommand_ExecuteAsync_NoCredential_DoesNothing()
        {
            const string testCredentialKey = "test-cred-key";

            var provider = new TestHostProvider {CredentialKey = testCredentialKey};
            var providerRegistry = new TestHostProviderRegistry {Provider = provider};
            var context = new TestCommandContext
            {
                CredentialStore =
                {
                    ["git:credential1"] = new GitCredential("this.should-1", "not.be.erased-1"),
                    ["git:credential2"] = new GitCredential("this.should-2", "not.be.erased-2")
                }
            };

            string[] cmdArgs = {"erase"};
            var command = new EraseCommand(providerRegistry);

            await command.ExecuteAsync(context, cmdArgs);

            Assert.Equal(2, context.CredentialStore.Count);
            Assert.True(context.CredentialStore.ContainsKey("git:credential1"));
            Assert.True(context.CredentialStore.ContainsKey("git:credential2"));
        }
    }
}
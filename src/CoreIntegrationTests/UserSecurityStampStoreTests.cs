﻿namespace IntegrationTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity.DocumentDB;
    using NUnit.Framework;

    [TestFixture]
    public class UserSecurityStampStoreTests : UserIntegrationTestsBase
    {
        [Test]
        public async Task Create_NewUser_HasSecurityStamp()
        {
            var manager = GetUserManager();
            var user = new IdentityUser { UserName = "bob" };

            await manager.CreateAsync(user);

            var savedUser = Client.CreateDocumentQuery<IdentityUser>(Users.DocumentsLink).AsEnumerable().FirstOrDefault();
            Expect(savedUser.SecurityStamp, Is.Not.Null);
        }

        [Test]
        public async Task GetSecurityStamp_NewUser_ReturnsStamp()
        {
            var manager = GetUserManager();
            var user = new IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);

            var stamp = await manager.GetSecurityStampAsync(user);

            Expect(stamp, Is.Not.Null);
        }
    }
}
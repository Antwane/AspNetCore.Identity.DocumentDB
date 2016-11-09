﻿// ReSharper disable once CheckNamespace - Common convention to locate extensions in Microsoft namespaces for simplifying autocompletion as a consumer.

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Linq;
    using AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.DocumentDB;
    using Azure.Documents;
    using Azure.Documents.Client;

    public static class DocumentDBIdentityBuilderExtensions
	{
		/// <summary>
		///     This method only registers mongo stores, you also need to call AddIdentity.
		///     Consider using AddIdentityWithMongoStores.
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="connectionString">Must contain the database name</param>
		public static IdentityBuilder RegisterDocumentDBStores<TUser, TRole>(this IdentityBuilder builder, DocumentClient documentClient, string databaseLink)
			where TRole : IdentityRole
			where TUser : IdentityUser
		{
			if (documentClient == null)
			{
				throw new ArgumentException("You must reference an initialized DocumentClient");
			}

            // create collections if not exist.
            var users = documentClient.CreateDocumentCollectionQuery(databaseLink).Where(c => c.Id.Equals("users")).AsEnumerable().FirstOrDefault()
                ?? documentClient.CreateDocumentCollectionAsync(databaseLink, new DocumentCollection { Id = "users" }).Result;
            var roles = documentClient.CreateDocumentCollectionQuery(databaseLink).Where(c => c.Id.Equals("roles")).AsEnumerable().FirstOrDefault()
                ?? documentClient.CreateDocumentCollectionAsync(databaseLink, new DocumentCollection { Id = "roles" }).Result;

            return builder.RegisterDocumentDBStores<TUser, TRole>(
                documentClient,
                p => users,
                p => roles);
        }

		/// <summary>
		///     If you want control over creating the users and roles collections, use this overload.
		///     This method only registers mongo stores, you also need to call AddIdentity.
		/// </summary>
		/// <typeparam name="TUser"></typeparam>
		/// <typeparam name="TRole"></typeparam>
		/// <param name="builder"></param>
		/// <param name="usersCollectionFactory"></param>
		/// <param name="rolesCollectionFactory"></param>
		public static IdentityBuilder RegisterDocumentDBStores<TUser, TRole>(this IdentityBuilder builder,
            DocumentClient documentClient,
            Func<IServiceProvider, DocumentCollection> usersCollectionFactory,
			Func<IServiceProvider, DocumentCollection> rolesCollectionFactory)
			where TRole : IdentityRole
			where TUser : IdentityUser
		{
			if (typeof(TUser) != builder.UserType)
			{
				var message = "User type passed to RegisterMongoStores must match user type passed to AddIdentity. "
				              + $"You passed {builder.UserType} to AddIdentity and {typeof(TUser)} to RegisterMongoStores, "
				              + "these do not match.";
				throw new ArgumentException(message);
			}
			if (typeof(TRole) != builder.RoleType)
			{
				var message = "Role type passed to RegisterMongoStores must match role type passed to AddIdentity. "
				              + $"You passed {builder.RoleType} to AddIdentity and {typeof(TRole)} to RegisterMongoStores, "
				              + "these do not match.";
				throw new ArgumentException(message);
			}
			builder.Services.AddSingleton<IUserStore<TUser>>(p => new UserStore<TUser>(documentClient, usersCollectionFactory(p)));
			builder.Services.AddSingleton<IRoleStore<TRole>>(p => new RoleStore<TRole>(documentClient, rolesCollectionFactory(p)));
			return builder;
		}

		/// <summary>
		///     This method registers identity services and MongoDB stores using the IdentityUser and IdentityRole types.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="connectionString">Connection string must contain the database name</param>
		public static IdentityBuilder AddIdentityWithDocumentDBStores(this IServiceCollection services, DocumentClient documentClient, string databaseLink)
		{
			return services.AddIdentityWithDocumentDBStoresUsingCustomTypes<IdentityUser, IdentityRole>(documentClient, databaseLink);
		}

		/// <summary>
		///     This method allows you to customize the user and role type when registering identity services
		///     and MongoDB stores.
		/// </summary>
		/// <typeparam name="TUser"></typeparam>
		/// <typeparam name="TRole"></typeparam>
		/// <param name="services"></param>
		/// <param name="connectionString">Connection string must contain the database name</param>
		public static IdentityBuilder AddIdentityWithDocumentDBStoresUsingCustomTypes<TUser, TRole>(this IServiceCollection services, DocumentClient documentClient, string databaseLink)
			where TUser : IdentityUser
			where TRole : IdentityRole
		{
			return services.AddIdentity<TUser, TRole>()
				.RegisterDocumentDBStores<TUser, TRole>(documentClient, databaseLink);
		}
	}
}
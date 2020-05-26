using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Banjo.CLI.Services;

namespace Banjo.CLI.TestSupport.ApiModel
{
    public static partial class ApiCalls
    {
        private static class Clients
        {
            private static readonly Func<Auth0InMemoryStore, IDictionary<string, object>, object, object> CreateNewClientAction = (store, routeValues, input) =>
            {
                var newClient = Reflectorisor.CopyMembers<ClientCreateRequest, Client>((ClientCreateRequest) input);
                if (store.Clients.Any(x => x.Name == newClient.Name))
                {
                    throw new ArgumentException($"Cannot create a new Client with the same name: {newClient.Name}");
                }

                newClient.ClientId = Guid.NewGuid().ToString();
                store.Clients.Add(newClient);
                return newClient;
            };

            private static readonly Func<Auth0InMemoryStore, IDictionary<string, object>, object, object> UpdateClientAction = (store, routeValues, input) =>
            {
                var updatedClient = Reflectorisor.CopyMembers<ClientUpdateRequest, Client>((ClientUpdateRequest) input);
                for (var i = 0; i < store.Clients.Count; i++)
                {
                    if (store.Clients[i].ClientId != routeValues["id"].ToString()) continue;
                    updatedClient.ClientId = routeValues["id"].ToString();
                    store.Clients[i] = updatedClient;
                    return updatedClient;
                }

                throw new ArgumentException($"Could not find an existing client with id: {routeValues["id"]}");
            };

            private static readonly Func<Auth0InMemoryStore, object> GetAllClientsAction = store => new PagedList<Client>(store.Clients);
            private static readonly Func<Auth0InMemoryStore, IDictionary<string, object>, object> GetClientAction = (store, routeValues) => new PagedList<Client>(store.Clients.Where(x => string.Equals(x.ClientId, routeValues["id"].ToString())));

            public static readonly ApiCall GetAll = new ApiCall(HttpMethod.Get, "clients", GetAllClientsAction);
            public static readonly ApiCall Get = new ApiCall(HttpMethod.Get, @"clients/{id}", GetClientAction);
            public static readonly ApiCall Create = new ApiCall(HttpMethod.Post, "clients", CreateNewClientAction);
            public static readonly ApiCall Update = new ApiCall(HttpMethod.Patch, "clients/{id}", UpdateClientAction);
        }
    }
}
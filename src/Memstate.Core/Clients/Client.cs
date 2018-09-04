﻿using System;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public static class Client
    {
        /// <summary>
        /// Return a remote or local client depending on the configuration
        /// </summary>
        /// <returns>The for.</returns>
        /// <typeparam name="TModel">The 1st type parameter.</typeparam>
        public static async Task<Client<TModel>> For<TModel>() where TModel: class
        {
            var cfg = Config.Current;
            var settings = cfg.GetSettings<ClientSettings>();

            if(settings.IsLocal)
            {
                var engine = await Engine.For<TModel>();
                return new LocalClient<TModel>(engine);
            }
            else
            {
                var client = new RemoteClient<TModel>();
                await client.Connect();
                return client;
            }
        }
    }

    public abstract class Client<TModel>
    {

        public abstract Task Execute(Command<TModel> command);

        public abstract Task<TResult> Execute<TResult>(Command<TModel, TResult> command);

        public abstract Task<TResult> Execute<TResult>(Query<TModel, TResult> query);

        internal abstract Task<object> ExecuteUntyped(Query query);

        /// <summary>
        /// Add or replace the domain event handler for a specific type,
        /// The type needs to match exactly, inheritance is not supported.
        /// </summary>
        /// <returns>A task which completes when the subscription has been acknowledged</returns>
        /// <param name="handler"></param>
        /// <param name="filter">An optional filter to apply</param>
        /// <typeparam name="T">The type of event to subscribe to</typeparam>
        public abstract Task Subscribe<T>(Action<T> handler, IEventFilter filter = null) where T : Event;

        /// <summary>
        /// Remove the subscription for a given type
        /// </summary>
        /// <returns>A task which completes when the unsubscribe has been acknowledged</returns>
        /// <typeparam name="T">The 1st type parameter</typeparam>
        public abstract Task Unsubscribe<T>() where T : Event;
    }
}
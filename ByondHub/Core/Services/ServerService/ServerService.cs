﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ByondHub.Core.Configuration;
using ByondHub.Core.Services.ServerService.Models;
using ByondHub.Shared.Updates;
using ByondHub.Shared.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ByondHub.Core.Services.ServerService
{
    public class ServerService
    {
        private readonly Dictionary<string, ServerContext> _servers;
        private readonly ILogger<ServerService> _logger;

        public ServerService(IConfiguration config, ILogger<ServerService> logger)
        {
            _servers = new Dictionary<string, ServerContext>();
            _logger = logger;

            var builds = config.GetSection("Hub").GetSection("Builds").Get<BuildModel[]>();
            var updater = new ServerUpdater(config, logger);
            foreach (var build in builds)
            {
                _servers.Add(build.Id,
                    new ServerContext(new ServerInstance(build, config["Hub:DreamDaemonPath"], updater)));
            }
        }

        public ServerStartStopResult Start(string serverId, int port)
        {
            try
            {
                var server = _servers[serverId];

                _logger.LogInformation($"Starting server with id {serverId}, port: {port}");
                return server.Start(port);
            }
            catch (KeyNotFoundException)
            {
                return new ServerStartStopResult {Error = true, Id = serverId, ErrorMessage = "Server not found."};
            }
            catch (Exception e)
            {
                return new ServerStartStopResult()
                {
                    Error = true,
                    Id = serverId,
                    ErrorMessage = $"Got following exception: {e.Message}"
                };
            }


        }

        public ServerStartStopResult Stop(string serverId)
        {
            try
            {
                var server = _servers[serverId];
                _logger.LogInformation($"Killing server with id {serverId}.");
                return server.Stop();
            }
            catch (KeyNotFoundException)
            {
                return new ServerStartStopResult {Error = true, Id = serverId, ErrorMessage = "Server not found."};
            }
            catch (Exception e)
            {
                return new ServerStartStopResult
                {
                    Error = true,
                    Id = serverId,
                    ErrorMessage = $"Got following exception: {e.Message}"
                };
            }
        }

        public UpdateResult Update(UpdateRequest request)
        {
            try
            {
                var server = _servers[request.Id];
                return server.Update(request);
            }
            catch (KeyNotFoundException)
            {
                return new UpdateResult() {Error = true, Id = request.Id, ErrorMessage = "Server not found."};
            }
            catch (Exception e)
            {
                return new UpdateResult()
                {
                    Error = true,
                    Id = request.Id,
                    ErrorMessage = $"Got exception: {e.Message}"
                };
            }

        }
    }
}

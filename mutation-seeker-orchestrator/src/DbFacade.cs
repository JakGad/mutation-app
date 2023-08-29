﻿using Amazon.Util;
using CommunicationTypes;
using MongoDB.Bson;
using MongoDB.Driver;
using mutation_seeker_orchestrator.src.dataStructures;
using mutation_seeker_orchestrator.src.environment;
using mutation_seeker_orchestrator.src.Monitoring;
using ZstdSharp.Unsafe;
using EnvironmentVariables = mutation_seeker_orchestrator.src.environment.EnvironmentVariables;

namespace mutation_seeker_orchestrator.src
{
    public static class DbFacade
    {
        private static ILogger _logger = Logger.GetLogger();
        private static EnvironmentVariables _envVariables = EnvironmentVariables.GetEnvs();

        private static IMongoCollection<RepoResult> _collection =
            new MongoClient(_envVariables.MongoConnectionString).GetDatabase(_envVariables.DatabaseName).GetCollection<RepoResult>(_envVariables.CollectionName);
        
        public static async void SaveResult(RepoCompartisonResultDTO result)
        {
            try
            {
                await _collection.InsertOneAsync(new RepoResult(result));
            }
            catch (Exception error)
            {
                _logger.LogError("Duplicated key for @{url}. @{error}", result.RepoUrl, error);
            }
        }

        public static async Task<HashSet<string>> GetReposUrl()
        {
            var filter = Builders<RepoResult>.Filter.Empty;
            var urlProjection = Builders<RepoResult>.Projection.Include(elem => elem.RepoUrl);
            if (filter == null || urlProjection == null) throw new Exception("cannot get filter or projection");
            
            var options = new FindOptions<RepoResult> 
            {
                Projection = urlProjection,
            };

            var idsEnumerable = await (await _collection.FindAsync<RepoResult>(filter, options)).ToListAsync();

            return new HashSet<string>(idsEnumerable.Select(elem => elem.RepoUrl));
        }
    }
}

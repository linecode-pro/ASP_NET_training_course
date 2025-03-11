﻿using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.DataAccess.Repositories
{
    public class MongoRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IOptions<MongoDatabaseSettings> mongoDatabaseSettings)
        {
            var mongoClient = new MongoClient(
             mongoDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                mongoDatabaseSettings.Value.DatabaseName);

            _collection = mongoDatabase.GetCollection<T>(typeof(T).Name);
        }

        public async Task AddAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task DeleteAsync(T entity)
        {
            await _collection.DeleteOneAsync(e => e.Id == entity.Id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return (await _collection.FindAsync(_ => true)).ToEnumerable();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return (await _collection.FindAsync(e => e.Id == id)).FirstOrDefault();
        }

        public async Task<T> GetFirstWhere(Expression<Func<T, bool>> predicate)
        {
            return (await _collection.FindAsync<T>(predicate)).FirstOrDefault();
        }

        public async Task<IEnumerable<T>> GetRangeByIdsAsync(List<Guid> ids)
        {
            return (await _collection.FindAsync(e => ids.Contains(e.Id))).ToEnumerable();
        }

        public async Task<IEnumerable<T>> GetWhere(Expression<Func<T, bool>> predicate)
        {
            return (await _collection.FindAsync(predicate)).ToEnumerable();
        }

        public async Task UpdateAsync(T entity)
        {
            await _collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);
        }
    }
}

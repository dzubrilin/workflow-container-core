using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.Runtime;
using Diadem.Workflow.Provider.MongoDb.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Diadem.Workflow.Provider.MongoDb
{
    public class MongoDbWorkflowStore : IWorkflowStore
    {
        private static readonly bool IsWarningEnabled = Log.IsEnabled(LogEventLevel.Warning);
        
        public MongoDbWorkflowStore(IMongoDatabase mongoDatabase)
        {
            MongoDatabase = mongoDatabase;
        }

        private IMongoDatabase MongoDatabase { get; }

        public Task ArchiveWorkflowInstance(IWorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetWorkflowConfiguration(Guid id, CancellationToken cancellationToken = default)
        {
            var mongoCollection = MongoDatabase.GetCollection<WorkflowConfigurationDto>(Constants.WorkflowConfigurationCollectionName);
            var findAsync = mongoCollection.FindAsync(dto => dto.Id == id.ToIdString(), cancellationToken: cancellationToken);
            return findAsync.ContinueWith(find => find.Result.FirstOrDefault()?.Content, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public async Task SaveWorkflowConfigurationContent(Guid id, string classCode, string content,
            CancellationToken cancellationToken = default)
        {
            var mongoCollection = MongoDatabase.GetCollection<WorkflowConfigurationDto>(Constants.WorkflowConfigurationCollectionName);
            var workflowConfigurationDto = mongoCollection.Find(dto => dto.Id == id.ToIdString()).FirstOrDefault();

            if (null != workflowConfigurationDto)
            {
                workflowConfigurationDto.Content = content;
                await UpdateAsync(mongoCollection, workflowConfigurationDto, dto => dto.Id == id.ToIdString(), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                workflowConfigurationDto = new WorkflowConfigurationDto
                {
                    Id = id.ToIdString(),
                    Content = content,
                    Created = DateTime.UtcNow,
                    WorkflowClassId = classCode
                };

                await InsertAsync(mongoCollection, workflowConfigurationDto, cancellationToken).ConfigureAwait(false);
            }
        }

        public Task<IWorkflowInstance> GetWorkflowInstance(string entityType, string entityId, CancellationToken cancellationToken = default)
        {
            var mongoCollection = MongoDatabase.GetCollection<WorkflowInstanceDto>(Constants.WorkflowInstanceCollectionName);
            var findAsync = mongoCollection.FindAsync(dto => dto.EntityType == entityType && dto.EntityId == entityId, cancellationToken: cancellationToken);

            return findAsync.ContinueWith(find =>
            {
                var workflowInstanceDto = find.Result.FirstOrDefault();
                return null == workflowInstanceDto ? default : MaterializeWorkflowInstance(workflowInstanceDto);
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public Task<IWorkflowInstance> GetWorkflowInstance(Guid id, CancellationToken cancellationToken = default)
        {
            var mongoCollection = MongoDatabase.GetCollection<WorkflowInstanceDto>(Constants.WorkflowInstanceCollectionName);
            var findAsync = mongoCollection.FindAsync(dto => dto.Id == id.ToIdString(), cancellationToken: cancellationToken);

            return findAsync.ContinueWith(find =>
            {
                var workflowInstanceDto = find.Result.FirstOrDefault();
                return null == workflowInstanceDto ? default : MaterializeWorkflowInstance(workflowInstanceDto);
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public Task<IList<IWorkflowInstanceStateLog>> GetWorkflowInstanceStateLog(Guid id, CancellationToken cancellationToken = default)
        {
            var mongoCollection = MongoDatabase.GetCollection<WorkflowInstanceLogDto>(Constants.WorkflowInstanceLogCollectionName);
            var findAsync = mongoCollection
                .Find(dto => dto.WorkflowInstanceId == id.ToIdString())
                .Project(dto => new { dto.WorkflowInstanceStateLog })
                .FirstOrDefaultAsync(cancellationToken);

            return findAsync.ContinueWith(find =>
            {
                if (null == findAsync.Result)
                {
                    return new List<IWorkflowInstanceStateLog>();
                }

                IList<IWorkflowInstanceStateLog> workflowInstanceStateLogList = new List<IWorkflowInstanceStateLog>();
                foreach (var workflowInstanceStateLog in findAsync.Result.WorkflowInstanceStateLog)
                {
                    workflowInstanceStateLogList.Add(new WorkflowInstanceStateLog(id, workflowInstanceStateLog.StateCode,
                        workflowInstanceStateLog.Started, workflowInstanceStateLog.Duration));
                }

                return workflowInstanceStateLogList;
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public Task<IWorkflowMessageState> GetWorkflowMessageState(Guid id, CancellationToken cancellationToken = default)
        {
            var mongoCollection = MongoDatabase.GetCollection<WorkflowMessageStateDto>(Constants.WorkflowMessageStateCollectionName);
            var findAsync = mongoCollection.FindAsync(dto => dto.WorkflowMessageId == id.ToIdString(), cancellationToken: cancellationToken);

            return findAsync.ContinueWith(find =>
            {
                var workflowMessageStateDto = find.Result.FirstOrDefault();
                return null == workflowMessageStateDto ? default(IWorkflowMessageState) : new WorkflowMessageState(workflowMessageStateDto.JsonState);
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public Task<IEnumerable<IWorkflowInstance>> GetNestedWorkflowInstances(Guid parentId, CancellationToken cancellationToken = default)
        {
            var mongoCollection = MongoDatabase.GetCollection<WorkflowInstanceDto>(Constants.WorkflowInstanceCollectionName);
            var findAsync = mongoCollection.FindAsync(
                dto => dto.Id != parentId.ToIdString() && dto.ParentWorkflowInstanceId == parentId.ToIdString(),
                cancellationToken: cancellationToken);

            return findAsync.ContinueWith(find => MaterializeWorkflowInstances(find.Result.ToList()), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public Task<IEnumerable<IWorkflowInstance>> GetNestedWorkflowInstances(Guid parentId, Guid workflowId,
            CancellationToken cancellationToken = default)
        {
            var mongoCollection = MongoDatabase.GetCollection<WorkflowInstanceDto>(Constants.WorkflowInstanceCollectionName);
            var findAsync = mongoCollection.FindAsync(
                dto => dto.ParentWorkflowInstanceId == parentId.ToIdString() && dto.WorkflowId == workflowId.ToIdString(),
                cancellationToken: cancellationToken);

            return findAsync.ContinueWith(find => MaterializeWorkflowInstances(find.Result.ToList()), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public async Task SaveWorkflowInstance(IWorkflowInstance workflowInstance, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNotNull(workflowInstance, nameof(workflowInstance));

            var workflowInstanceIdString = workflowInstance.Id.ToIdString();
            var mongoCollection = MongoDatabase.GetCollection<WorkflowInstanceDto>(Constants.WorkflowInstanceCollectionName);
            var workflowInstanceDto = mongoCollection.Find(x => x.Id == workflowInstanceIdString).FirstOrDefault();

            if (null == workflowInstanceDto)
            {
                workflowInstanceDto = new WorkflowInstanceDto
                {
                    Id = workflowInstance.Id.ToIdString(),
                    Created = new BsonDateTime(workflowInstance.Created),
                    CurrentStateCode = workflowInstance.CurrentStateCode,
                    CurrentStateProgress = workflowInstance.CurrentStateProgress,
                    EntityId = workflowInstance.EntityId,
                    EntityType = workflowInstance.EntityType,
                    ParentWorkflowInstanceId = workflowInstance.ParentWorkflowInstanceId.ToIdString(),
                    RootWorkflowInstanceId = workflowInstance.RootWorkflowInstanceId.ToIdString(),
                    WorkflowId = workflowInstance.WorkflowId.ToIdString(),
                    Lock = new WorkflowInstanceLockDto
                    {
                        LockOwner = workflowInstance.Lock.LockOwner.ToIdString(),
                        LockMode = workflowInstance.Lock.LockMode,
                        LockedAt = workflowInstance.Lock.LockedAt,
                        LockedUntil = workflowInstance.Lock.LockedUntil
                    }
                };

                await InsertAsync(mongoCollection, workflowInstanceDto, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // make sure not to update the whole entity to avoid conflicting updates on [Lock] sub-object
                var updateDefinition = Builders<WorkflowInstanceDto>.Update
                    .Set(wi => wi.CurrentStateCode, workflowInstance.CurrentStateCode)
                    .Set(wi => wi.CurrentStateProgress, workflowInstance.CurrentStateProgress)
                    .Set(wi => wi.EntityId, workflowInstance.EntityId)
                    .Set(wi => wi.EntityType, workflowInstance.EntityType)
                    .Set(wi => wi.ParentWorkflowInstanceId, workflowInstance.ParentWorkflowInstanceId.ToIdString())
                    .Set(wi => wi.RootWorkflowInstanceId, workflowInstance.RootWorkflowInstanceId.ToIdString())
                    .Set(wi => wi.WorkflowId, workflowInstance.WorkflowId.ToIdString());

                await mongoCollection.UpdateOneAsync(wi => wi.Id == workflowInstanceIdString, updateDefinition, null, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task SaveWorkflowMessageState(IWorkflowMessage workflowMessage, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNotNull(workflowMessage, nameof(workflowMessage));

            var mongoCollection = MongoDatabase.GetCollection<WorkflowMessageStateDto>(Constants.WorkflowMessageStateCollectionName);
            var existingWorkflowMessageStateDto = mongoCollection.Find(x => x.WorkflowMessageId == workflowMessage.WorkflowMessageId.ToIdString())
                .FirstOrDefault();

            if (null != existingWorkflowMessageStateDto)
            {
                throw new Exception($"State for workflow message [{workflowMessage.WorkflowMessageId:D}] has already been saved");
            }

            var jsonContent = workflowMessage.State.JsonState;
            var workflowMessageStateDto = new WorkflowMessageStateDto
            {
                WorkflowMessageId = workflowMessage.WorkflowMessageId.ToIdString(),
                WorkflowInstanceId = workflowMessage.WorkflowInstanceId?.ToIdString(),
                Created = DateTime.UtcNow,
                JsonState = jsonContent
            };

            await InsertAsync(mongoCollection, workflowMessageStateDto, cancellationToken).ConfigureAwait(false);
        }

        public async Task SaveWorkflowInstanceActivityLog(IWorkflowInstanceActivityLog workflowInstanceActivityLog, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNotNull(workflowInstanceActivityLog, nameof(workflowInstanceActivityLog));

            var workflowInstanceIdString = workflowInstanceActivityLog.WorkflowInstanceId.ToIdString();
            var workflowInstanceActivityLogDto = new WorkflowInstanceActivityLogDto(
                workflowInstanceActivityLog.ActivityCode,
                workflowInstanceActivityLog.Started,
                workflowInstanceActivityLog.Duration,
                workflowInstanceActivityLog.TryCount);

            var mongoCollection = MongoDatabase.GetCollection<WorkflowInstanceLogDto>(Constants.WorkflowInstanceLogCollectionName);
            var workflowInstanceIdExists = await mongoCollection.Find(x => x.WorkflowInstanceId == workflowInstanceIdString).AnyAsync(cancellationToken);

            if (!workflowInstanceIdExists)
            {
                var workflowInstanceLogDto = new WorkflowInstanceLogDto
                {
                    WorkflowInstanceId = workflowInstanceIdString,
                    WorkflowInstanceActivityLog = new List<WorkflowInstanceActivityLogDto> { workflowInstanceActivityLogDto }
                };

                await InsertAsync(mongoCollection, workflowInstanceLogDto, cancellationToken).ConfigureAwait(false);
                return;
            }

            var updateDefinition = Builders<WorkflowInstanceLogDto>.Update.Push(log => log.WorkflowInstanceActivityLog, workflowInstanceActivityLogDto);
            var updateResult = await mongoCollection.UpdateOneAsync(log => log.WorkflowInstanceId == workflowInstanceIdString,
                updateDefinition, new UpdateOptions() { IsUpsert = true }, cancellationToken).ConfigureAwait(false);

            if (!updateResult.IsAcknowledged && IsWarningEnabled)
            {
                Log.Warning("Saving activity log record failed for {workflowInstanceId}", workflowInstanceActivityLog.WorkflowInstanceId);
            }
        }

        public async Task SaveWorkflowInstanceEventLog(IWorkflowInstanceEventLog workflowInstanceEventLog, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNotNull(workflowInstanceEventLog, nameof(workflowInstanceEventLog));

            var workflowInstanceIdString = workflowInstanceEventLog.WorkflowInstanceId.ToIdString();
            var workflowInstanceEventLogDto = new WorkflowInstanceEventLogDto(
                workflowInstanceEventLog.EventCode,
                workflowInstanceEventLog.Started,
                workflowInstanceEventLog.Duration);

            var mongoCollection = MongoDatabase.GetCollection<WorkflowInstanceLogDto>(Constants.WorkflowInstanceLogCollectionName);
            var existingWorkflowInstanceLogDto = mongoCollection.Find(x => x.WorkflowInstanceId == workflowInstanceIdString)
                .FirstOrDefault();

            if (null == existingWorkflowInstanceLogDto)
            {
                existingWorkflowInstanceLogDto = new WorkflowInstanceLogDto
                {
                    WorkflowInstanceId = workflowInstanceIdString,
                    WorkflowInstanceEventLog = new List<WorkflowInstanceEventLogDto> { workflowInstanceEventLogDto }
                };

                await InsertAsync(mongoCollection, existingWorkflowInstanceLogDto, cancellationToken).ConfigureAwait(false);
                return;
            }

            var updateDefinition = Builders<WorkflowInstanceLogDto>.Update.Push(log => log.WorkflowInstanceEventLog, workflowInstanceEventLogDto);
            var updateResult = await mongoCollection.UpdateOneAsync(log => log.WorkflowInstanceId == workflowInstanceIdString,
                updateDefinition, new UpdateOptions() { IsUpsert = true }, cancellationToken).ConfigureAwait(false);

            if (!updateResult.IsAcknowledged && IsWarningEnabled)
            {
                Log.Warning("Saving event log record failed for {workflowInstanceId}", workflowInstanceEventLog.WorkflowInstanceId);
            }
        }

        public async Task SaveWorkflowInstanceMessageLog(IWorkflowInstanceMessageLog workflowInstanceMessageLog, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNotNull(workflowInstanceMessageLog, nameof(workflowInstanceMessageLog));

            var workflowMessageIdString = workflowInstanceMessageLog.WorkflowMessageId.ToIdString();
            var workflowInstanceIdString = workflowInstanceMessageLog.WorkflowInstanceId.ToIdString();
            var workflowInstanceMessageLogDto = new WorkflowInstanceMessageLogDto(
                workflowInstanceIdString,
                workflowMessageIdString,
                workflowInstanceMessageLog.Type,
                workflowInstanceMessageLog.Started,
                workflowInstanceMessageLog.Duration);

            var mongoCollection = MongoDatabase.GetCollection<WorkflowInstanceLogDto>(Constants.WorkflowInstanceLogCollectionName);
            var existingWorkflowInstanceLogDto = mongoCollection.Find(x => x.WorkflowInstanceId == workflowInstanceIdString)
                .FirstOrDefault();

            if (null == existingWorkflowInstanceLogDto)
            {
                existingWorkflowInstanceLogDto = new WorkflowInstanceLogDto
                {
                    WorkflowInstanceId = workflowInstanceIdString,
                    WorkflowInstanceMessageLog = new List<WorkflowInstanceMessageLogDto> { workflowInstanceMessageLogDto }
                };

                await InsertAsync(mongoCollection, existingWorkflowInstanceLogDto, cancellationToken).ConfigureAwait(false);
                return;
            }

            var updateDefinition = Builders<WorkflowInstanceLogDto>.Update.Push(log => log.WorkflowInstanceMessageLog, workflowInstanceMessageLogDto);
            var updateResult = await mongoCollection.UpdateOneAsync(log => log.WorkflowInstanceId == workflowInstanceIdString,
                updateDefinition, new UpdateOptions() { IsUpsert = true }, cancellationToken).ConfigureAwait(false);

            if (!updateResult.IsAcknowledged && IsWarningEnabled)
            {
                Log.Warning("Saving message log record failed for {workflowInstanceId}", workflowInstanceMessageLog.WorkflowInstanceId);
            }
        }

        public async Task SaveWorkflowInstanceStateLog(IWorkflowInstanceStateLog workflowInstanceStateLog, CancellationToken cancellationToken = default)
        {
            Guard.ArgumentNotNull(workflowInstanceStateLog, nameof(workflowInstanceStateLog));

            var workflowInstanceIdString = workflowInstanceStateLog.WorkflowInstanceId.ToIdString();
            var workflowInstanceStateLogDto = new WorkflowInstanceStateLogDto(
                workflowInstanceStateLog.StateCode,
                workflowInstanceStateLog.Started,
                workflowInstanceStateLog.Duration);

            var mongoCollection = MongoDatabase.GetCollection<WorkflowInstanceLogDto>(Constants.WorkflowInstanceLogCollectionName);
            var existingWorkflowInstanceLogDto = mongoCollection.Find(x => x.WorkflowInstanceId == workflowInstanceIdString)
                .FirstOrDefault();

            if (null == existingWorkflowInstanceLogDto)
            {
                existingWorkflowInstanceLogDto = new WorkflowInstanceLogDto
                {
                    WorkflowInstanceId = workflowInstanceIdString,
                    WorkflowInstanceStateLog = new List<WorkflowInstanceStateLogDto> { workflowInstanceStateLogDto }
                };

                await InsertAsync(mongoCollection, existingWorkflowInstanceLogDto, cancellationToken).ConfigureAwait(false);
                return;
            }

            var updateDefinition = Builders<WorkflowInstanceLogDto>.Update.Push(log => log.WorkflowInstanceStateLog, workflowInstanceStateLogDto);
            var updateResult = await mongoCollection.UpdateOneAsync(log => log.WorkflowInstanceId == workflowInstanceIdString,
                updateDefinition, new UpdateOptions() { IsUpsert = true }, cancellationToken).ConfigureAwait(false);

            if (!updateResult.IsAcknowledged && IsWarningEnabled)
            {
                Log.Warning("Saving activity log record failed for {workflowInstanceId}", workflowInstanceStateLog.WorkflowInstanceId);
            }
        }

        public async Task<bool> TryLockWorkflowInstance(Guid ownerId, Guid workflowInstanceId, DateTime lockedAt, DateTime lockedUntil, CancellationToken cancellationToken = default)
        {
            var mongoCollection = MongoDatabase.GetCollection<WorkflowInstanceDto>(Constants.WorkflowInstanceCollectionName);

            var lockOwnerString = ownerId.ToIdString();
            var workflowInstanceIdString = workflowInstanceId.ToIdString();
            var updateDefinition = Builders<WorkflowInstanceDto>.Update.Set(wi => wi.Lock, new WorkflowInstanceLockDto(lockOwnerString, WorkflowInstanceLockMode.Locked, lockedAt, lockedUntil));

            var updateResult = await mongoCollection.UpdateOneAsync(
                wi => wi.Id == workflowInstanceIdString && ((wi.Lock.LockOwner == lockOwnerString || wi.Lock.LockMode != WorkflowInstanceLockMode.Locked || wi.Lock.LockedUntil < lockedAt)),
                updateDefinition, cancellationToken: cancellationToken);

            Log.Verbose("Lock of {workflowInstanceId} by {workflowEngineId} has been taken=[{lockTaken}], count=[{counter}]",
                workflowInstanceIdString, lockOwnerString, updateResult.IsAcknowledged && updateResult.ModifiedCount > 0, updateResult.ModifiedCount);
            
//            var tmpWorkflowInstance = await GetWorkflowInstance(workflowInstanceId, cancellationToken).ConfigureAwait(false);
//            Log.Verbose("[Lock] {workflowInstanceId}, {rootWorkflowInstanceId}, lock=[{owner}, {mode}]",
//                    tmpWorkflowInstance.Id, tmpWorkflowInstance.RootWorkflowInstanceId, tmpWorkflowInstance.Lock.LockOwner, tmpWorkflowInstance.Lock.LockMode));

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        public async Task<bool> TryUnlockWorkflowInstance(Guid ownerId, Guid workflowInstanceId, DateTime lockedAt, CancellationToken cancellationToken = default)
        {
            var mongoCollection = MongoDatabase.GetCollection<WorkflowInstanceDto>(Constants.WorkflowInstanceCollectionName);

            var lockOwnerString = ownerId.ToIdString();
            var workflowInstanceIdString = workflowInstanceId.ToIdString();
            var updateDefinition = Builders<WorkflowInstanceDto>.Update.Set(wi => wi.Lock, new WorkflowInstanceLockDto(lockOwnerString, WorkflowInstanceLockMode.Unlocked, lockedAt, lockedAt));

            var updateResult = await mongoCollection.UpdateOneAsync(
                wi => wi.Id == workflowInstanceIdString && (wi.Lock.LockOwner == lockOwnerString && (wi.Lock.LockMode == WorkflowInstanceLockMode.Locked || wi.Lock.LockedUntil < lockedAt)),
                updateDefinition, cancellationToken: cancellationToken);

            Log.Verbose("[Un]Lock of {workflowInstanceId} by {workflowEngineId} has been removed=[{lockRemoved}], count=[{counter}]",
                workflowInstanceIdString, lockOwnerString, updateResult.IsAcknowledged && updateResult.ModifiedCount > 0, updateResult.ModifiedCount);

//                var tmpWorkflowInstance = await GetWorkflowInstance(workflowInstanceId, cancellationToken).ConfigureAwait(false);
//                Logger.Trace(string.Format(CultureInfo.InvariantCulture, "[Lock] wiID=[{0}], rwID=[{1}], lock=[owner={2}, mode={3}]",
//                    tmpWorkflowInstance.Id, tmpWorkflowInstance.RootWorkflowInstanceId, tmpWorkflowInstance.Lock.LockOwner, tmpWorkflowInstance.Lock.LockMode));

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        // TODO: for unit tests only
        public void Clear()
        {
            var workflowInstanceCollection = MongoDatabase.GetCollection<WorkflowInstanceDto>(Constants.WorkflowInstanceCollectionName);
            workflowInstanceCollection.DeleteMany(dto => true);

            var workflowMessageStateDtoCollection = MongoDatabase.GetCollection<WorkflowMessageStateDto>(Constants.WorkflowMessageStateCollectionName);
            workflowMessageStateDtoCollection.DeleteMany(dto => true);
        }

        private static IEnumerable<IWorkflowInstance> MaterializeWorkflowInstances(List<WorkflowInstanceDto> workflowInstanceDtoList)
        {
            if (null == workflowInstanceDtoList || !workflowInstanceDtoList.Any())
            {
                return Enumerable.Empty<WorkflowInstance>();
            }

            var workflowInstanceList = new List<IWorkflowInstance>();
            foreach (var workflowInstanceDto in workflowInstanceDtoList)
            {
                var workflowInstance = MaterializeWorkflowInstance(workflowInstanceDto);
                workflowInstanceList.Add(workflowInstance);
            }

            return workflowInstanceList;
        }

        private static IWorkflowInstance MaterializeWorkflowInstance(WorkflowInstanceDto workflowInstanceDto)
        {
            var workflowInstanceLock = new WorkflowInstanceLock(
                workflowInstanceDto.Lock.LockOwner.ToIdGuid(),
                workflowInstanceDto.Lock.LockMode,
                workflowInstanceDto.Lock.LockedAt.ToUniversalTime(),
                workflowInstanceDto.Lock.LockedUntil.ToUniversalTime());

            return new WorkflowInstance(
                workflowInstanceDto.WorkflowId.ToIdGuid(),
                workflowInstanceDto.Id.ToIdGuid(),
                workflowInstanceDto.RootWorkflowInstanceId.ToIdGuid(),
                workflowInstanceDto.ParentWorkflowInstanceId.ToIdGuid(),
                workflowInstanceDto.EntityType,
                workflowInstanceDto.EntityId,
                workflowInstanceLock)
            {
                Created = workflowInstanceDto.Created.ToUniversalTime(),
                CurrentStateCode = workflowInstanceDto.CurrentStateCode,
                CurrentStateProgress = workflowInstanceDto.CurrentStateProgress,
                EntityId = workflowInstanceDto.EntityId,
                EntityType = workflowInstanceDto.EntityType,
                WorkflowId = workflowInstanceDto.WorkflowId.ToIdGuid()
            };
        }

        private static async Task InsertAsync<T>(IMongoCollection<T> mongoCollection, T record, CancellationToken cancellationToken)
        {
            await mongoCollection.InsertOneAsync(record, cancellationToken: cancellationToken);
        }

        private static async Task UpdateAsync<T>(IMongoCollection<T> collection, T record, Expression<Func<T, bool>> filter, CancellationToken cancellationToken)
        {
            await collection.ReplaceOneAsync<T>(filter, record, new ReplaceOptions {IsUpsert = true}, cancellationToken).ConfigureAwait(false);
        }

        private static class Constants
        {
            public const string WorkflowClassCollectionName = "workflowClass";

            public const string WorkflowConfigurationCollectionName = "workflowConfiguration";

            public const string WorkflowInstanceCollectionName = "workflowInstance";

            public const string WorkflowInstanceLogCollectionName = "workflowInstanceLog";

            public const string WorkflowMessageStateCollectionName = "workflowMessageState";
        }
    }
}
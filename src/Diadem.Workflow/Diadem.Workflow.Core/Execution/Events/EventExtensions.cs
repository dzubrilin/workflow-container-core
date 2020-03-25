using System;
using Diadem.Core;
using Diadem.Core.DomainModel;
using Diadem.Workflow.Core.Common;

namespace Diadem.Workflow.Core.Execution.Events
{
    internal static class EventExtensions
    {
        internal static IEntity GetDomainEntityOrNull(this IEvent @event)
        {
            Guard.ArgumentNotNull(@event, nameof(@event));
            return @event is IDomainEntityEvent domainEvent ? domainEvent.Entity : null;
        }

        internal static string GetDomainEntityTypeOrNull(this IEvent @event)
        {
            Guard.ArgumentNotNull(@event, nameof(@event));
            return @event is IDomainEvent domainEvent ? domainEvent.EntityType : null;
        }

        internal static string GetDomainEntityIdOrNull(this IEvent @event)
        {
            Guard.ArgumentNotNull(@event, nameof(@event));
            return @event is IDomainEvent domainEvent ? domainEvent.EntityId : null;
        }

        internal static JsonState GetEventStateOrDefault(this IEvent @event)
        {
            Guard.ArgumentNotNull(@event, nameof(@event));
            return @event is IStatefulEvent statefulEvent ? statefulEvent.State : new JsonState();
        }

        internal static Guid GetParentWorkflowInstanceIdOrUseCurrent(this IEvent @event, Guid currentInstanceId)
        {
            Guard.ArgumentNotNull(@event, nameof(@event));
            return @event is ParentToNestedEntityEvent nestedEvent ? nestedEvent.ParentWorkflowInstanceId : currentInstanceId;
        }

        internal static Guid GetRootWorkflowInstanceIdOrUseCurrent(this IEvent @event, Guid currentInstanceId)
        {
            Guard.ArgumentNotNull(@event, nameof(@event));
            return @event is ParentToNestedEntityEvent nestedEvent ? nestedEvent.RootWorkflowInstanceId : currentInstanceId;
        }

        internal static bool IsHierarchicalEvent(this IEvent @event)
        {
            return @event is IHierarchicalEvent;
        }
    }
}
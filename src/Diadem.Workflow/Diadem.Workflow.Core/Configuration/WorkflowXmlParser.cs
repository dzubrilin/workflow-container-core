using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Diadem.Core;
using Serilog;

namespace Diadem.Workflow.Core.Configuration
{
    public sealed class WorkflowXmlParser : IWorkflowParser
    {
        public WorkflowConfiguration Parse(string content)
        {
            try
            {
                using (var textReader = new StringReader(content))
                using (var xmlReader = XmlReader.Create(textReader))
                {
                    var xDocument = XDocument.Load(xmlReader);
                    Guard.ArgumentNotNull(xDocument.Root, "document");

                    var idString = ParseAttributeString(xDocument.Root, "id");
                    var @class = ParseAttributeString(xDocument.Root, "class");
                    var code = ParseAttributeString(xDocument.Root, "code");
                    var name = ParseAttributeString(xDocument.Root, "name");
                    var versionString = ParseAttributeString(xDocument.Root, "version");

                    if (!Guid.TryParse(idString, out var id))
                    {
                        throw new Exception($"Can not convert [{idString}] to a Guid");
                    }

                    if (!Version.TryParse(versionString, out var version))
                    {
                        throw new Exception($"Can not convert [{versionString}] to a version");
                    }

                    Log.Verbose("Starting reading workflow [{code}], [{name}], [{version}] from XML", code, name, version);
                    var workflowConfiguration = new WorkflowConfiguration(id, @class, code, name, version);

                    var events = xDocument.Root.Element("events");
                    if (events != null)
                    {
                        foreach (var eventElement in events.Elements("event"))
                        {
                            var @event = ParseEvent(eventElement);
                            workflowConfiguration.Events.Add(@event);
                        }
                    }

                    var states = xDocument.Root.Element("states");
                    if (null != states)
                    {
                        foreach (var stateElement in states.Elements("state"))
                        {
                            var state = ParseState(stateElement);
                            workflowConfiguration.States.Add(state);
                        }
                    }

                    Log.Verbose("Workflow [{code}], [{name}], [{version}] has been read from XML", code, name, version);
                    WorkflowConfigurationValidator.ValidateWorkflowConfiguration(workflowConfiguration);
                    return workflowConfiguration;
                }
            }
            catch (WorkflowConfigurationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WorkflowConfigurationException("An error has occurred during reading XML workflow", ex);
            }
        }

        private static EventConfiguration ParseEvent(XElement element)
        {
            var code = ParseAttributeString(element, "code");
            var parameters = element.Attribute("parameters")?.Value;

            var typeString = element.Attribute("type")?.Value;
            if (string.IsNullOrEmpty(typeString) || !Enum.TryParse(typeString, true, out EventTypeConfiguration eventTypeConfiguration))
            {
                throw new Exception($"Cannot convert [{typeString}] [state={code}] into valid event type");
            }

            return new EventConfiguration(eventTypeConfiguration, code, parameters);
        }

        private static StateConfiguration ParseState(XElement element)
        {
            var code = ParseAttributeString(element, "code");

            var typeString = element.Attribute("type")?.Value;
            if (string.IsNullOrEmpty(typeString) || !Enum.TryParse(typeString, true, out StateTypeConfiguration typeConfiguration))
            {
                throw new Exception($"Cannot convert [{typeString}] [state={code}] into valid state type");
            }

            var state = new StateConfiguration(code, typeConfiguration);
            var activities = element.Element("activities");
            if (null != activities)
            {
                foreach (var activityElement in activities.Elements("activity"))
                {
                    var activity = ParseStateActivity(activityElement);
                    state.Activities.Add(activity);
                }
            }

            var events = element.Element("events");
            if (null != events)
            {
                foreach (var eventElement in events.Elements("event"))
                {
                    var @event = ParseStateEvent(eventElement);
                    state.Events.Add(@event);
                }
            }

            var transitions = element.Element("transitions");
            if (null != transitions)
            {
                foreach (var transitionElement in transitions.Elements("transition"))
                {
                    var transition = ParseStateTransition(transitionElement);
                    state.Transitions.Add(transition);
                }
            }

            return state;
        }

        private static ActivityConfiguration ParseStateActivity(XElement element)
        {
            var code = ParseAttributeString(element, "code");
            var script = element.Attribute("script")?.Value;
            var parameters = element.Attribute("parameters")?.Value;

            var scriptTypeString = element.Attribute("scriptType")?.Value;
            var scriptTypeConfiguration = ScriptTypeConfiguration.Undefined;
            if (!string.IsNullOrEmpty(scriptTypeString) && !Enum.TryParse(scriptTypeString, true, out scriptTypeConfiguration))
            {
                throw new Exception($"Cannot convert [{scriptTypeString}] into valid script type");
            }

            var typeString = element.Attribute("type")?.Value;
            if (string.IsNullOrEmpty(typeString) || !Enum.TryParse(typeString, true, out ActivityTypeConfiguration activityTypeConfiguration))
            {
                throw new Exception($"Cannot convert [{typeString}] into valid activity type");
            }

            var retryPolicyElement = element.Element("retryPolicy");
            var activityRetryPolicy = null != retryPolicyElement
                ? ParseActivityRetryPolicy(retryPolicyElement)
                : new ActivityRetryPolicyDefaultConfiguration();

            return new ActivityConfiguration(activityTypeConfiguration, code, parameters, scriptTypeConfiguration, script, activityRetryPolicy);
        }

        private static ActivityRetryPolicyConfiguration ParseActivityRetryPolicy(XElement element)
        {
            var count = ParseAttributeInt32(element, "count");
            var delay = ParseAttributeInt32(element, "delay");
            var onFailureTransitionToUse = ParseAttributeString(element, "onFailureTransitionToUse", true);
            return new ActivityRetryPolicyConfiguration(count, delay, onFailureTransitionToUse);
        }

        private static StateEventConfiguration ParseStateEvent(XElement element)
        {
            var code = element.Attribute("code")?.Value;
            var handlerCode = element.Attribute("handlerCode")?.Value;
            var script = element.Attribute("script")?.Value;

            var scriptTypeString = element.Attribute("scriptType")?.Value;
            var scriptTypeConfiguration = ScriptTypeConfiguration.Undefined;
            if (!string.IsNullOrEmpty(scriptTypeString) && !Enum.TryParse(scriptTypeString, true, out scriptTypeConfiguration))
            {
                throw new Exception($"Cannot convert [{scriptTypeString}] into valid script type");
            }

            return new StateEventConfiguration(code, handlerCode, scriptTypeConfiguration, script);
        }

        private static TransitionConfiguration ParseStateTransition(XElement element)
        {
            var code = element.Attribute("code")?.Value;
            var script = element.Attribute("script")?.Value;
            var parameters = element.Attribute("parameters")?.Value;
            var moveToState = ParseAttributeString(element, "moveToState");

            var scriptTypeString = element.Attribute("scriptType")?.Value;
            var scriptTypeConfiguration = ScriptTypeConfiguration.Undefined;
            if (!string.IsNullOrEmpty(scriptTypeString) && !Enum.TryParse(scriptTypeString, true, out scriptTypeConfiguration))
            {
                throw new Exception($"Cannot convert [{scriptTypeString}] into valid script type");
            }

            var typeString = element.Attribute("type")?.Value;
            if (string.IsNullOrEmpty(typeString) || !Enum.TryParse(typeString, true, out TransitionTypeConfiguration typeConfiguration))
            {
                throw new Exception($"Cannot convert [{typeString}] into valid transition type");
            }

            int? delay = null;
            var delayString = element.Attribute("delay")?.Value;
            if (!string.IsNullOrEmpty(delayString) && int.TryParse(delayString, out var tmp))
            {
                delay = tmp;
            }

            return new TransitionConfiguration(typeConfiguration, code, parameters, moveToState, scriptTypeConfiguration, script, delay);
        }

        private static string ParseAttributeString(XElement element, string attribute, bool allowNull = false)
        {
            var attributeString = element.Attribute(attribute)?.Value;
            if (!allowNull)
            {
                Guard.ArgumentNotNullOrEmpty(attributeString, attribute);
            }
            
            return attributeString;
        }

        private static int ParseAttributeInt32(XElement element, string attribute)
        {
            var attributeString = element.Attribute(attribute)?.Value;
            Guard.ArgumentNotNullOrEmpty(attributeString, attribute);

            if (!int.TryParse(attributeString, out var result))
            {
                throw new Exception($"Cannot convert to integer attribute value [{attribute}]");
            }

            return result;
        }
    }
}
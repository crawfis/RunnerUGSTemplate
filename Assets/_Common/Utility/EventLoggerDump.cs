using CrawfisSoftware.Events;

using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

using static UnityEngine.Rendering.GPUSort;

namespace CrawfisSoftware.Utility.Testing
{
    internal class EventLoggerDump : MonoBehaviour
    {
        private StringBuilder _sb = new StringBuilder();
        private void Awake()
        {
            EventsPublisher.Instance.SubscribeToEvent("QuitRequested", DumpLogs);
            EventsPublisher.Instance.SubscribeToAllEvents(LogEvent);
            _sb.AppendLine("Sequence of events:");
        }

        private void OnDestroy()
        {
            EventsPublisher.Instance.UnsubscribeToAllEvents(LogEvent);
            EventsPublisher.Instance.UnsubscribeToEvent("QuitRequested", DumpLogs);
        }

        private void LogEvent(string eventName, object arg2, object arg3)
        {
            _sb.AppendLine(eventName);
        }

        private void DumpLogs(string eventName, object sender, object data)
        {
            Debug.Log(_sb.ToString());
        }
    }
}
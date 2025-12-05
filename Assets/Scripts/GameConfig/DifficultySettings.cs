using CrawfisSoftware.Events;

using System;
using System.Collections.Generic;

namespace CrawfisSoftware.TempleRun.GameConfig
{
    [Serializable]
    public class DifficultySettings
    {
        private List<DifficultyConfig> _configs;

        public List<DifficultyConfig> Configs
        {
            get
            {
                return _configs;
            }
            set { 
                _configs = value;
                EventsPublisherGameFlow.Instance.PublishEvent(GameFlowEvents.DifficultySettingsChanged, this, _configs);
            }
        }
    }
}
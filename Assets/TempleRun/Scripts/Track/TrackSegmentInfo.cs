using System;

namespace CrawfisSoftware.TempleRun
{
    [Serializable]
    public struct TrackSegmentInfo
    {
        public string SegmentId;
        public string TurnDirection;
        public int TurnDirectionValue;
        public float Length;

        public TrackSegmentInfo(string segmentId, string turnDirection, int turnDirectionValue, float length)
        {
            SegmentId = segmentId;
            TurnDirection = turnDirection;
            TurnDirectionValue = turnDirectionValue;
            Length = length;
        }
    }
}

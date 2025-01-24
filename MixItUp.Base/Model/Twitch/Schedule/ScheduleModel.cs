using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Schedule
{
    /// <summary>
    /// Information about a broadcaster's schedule
    /// </summary>
    public class ScheduleModel
    {
        /// <summary>
        /// The ID of the broadcaster.
        /// </summary>
        public string broadcaster_id { get; set; }
        /// <summary>
        /// The name of the broadcaster.
        /// </summary>
        public string broadcaster_name { get; set; }
        /// <summary>
        /// Login of the broadcaster.
        /// </summary>
        public string broadcaster_login { get; set; }
        /// <summary>
        /// If Vacation Mode is enabled, this includes start and end dates for the vacation.If Vacation Mode is disabled, value is set to null.
        /// </summary>
        public ScheduleVacationModel vacation { get; set; }
        /// <summary>
        /// Scheduled broadcasts for this stream schedule.
        /// </summary>
        public List<ScheduleSegmentModel> segments { get; set; } = new List<ScheduleSegmentModel>();
    }
}

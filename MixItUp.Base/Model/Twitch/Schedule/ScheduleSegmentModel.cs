namespace MixItUp.Base.Model.Twitch.Schedule
{
    /// <summary>
    /// Information for a segment of a broadcaster's schedule
    /// </summary>
    public class ScheduleSegmentModel
    {
        /// <summary>
        /// The ID for the scheduled broadcast.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Scheduled start time for the scheduled broadcast in RFC3339 format.
        /// </summary>
        public string start_time { get; set; }
        /// <summary>
        /// Scheduled end time for the scheduled broadcast in RFC3339 format.
        /// </summary>
        public string end_time { get; set; }
        /// <summary>
        /// Title for the scheduled broadcast
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Used with recurring scheduled broadcasts. Specifies the date of the next recurring broadcast in RFC3339 format if one or more specific broadcasts have been deleted in the series. Set to null otherwise.
        /// </summary>
        public string canceled_until { get; set; }
        /// <summary>
        /// The category for the scheduled broadcast. Set to null if no category has been specified.
        /// </summary>
        public ScheduleSegmentCategoryModel category { get; set; }
        /// <summary>
        /// Indicates if the scheduled broadcast is recurring weekly.
        /// </summary>
        public bool is_recurring { get; set; }
    }
}

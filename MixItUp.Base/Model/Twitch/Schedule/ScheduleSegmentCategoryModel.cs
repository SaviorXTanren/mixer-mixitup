namespace MixItUp.Base.Model.Twitch.Schedule
{
    /// <summary>
    /// The game/category for a broadcaster's schedule segment
    /// </summary>
    public class ScheduleSegmentCategoryModel
    {
        /// <summary>
        /// Game/category ID.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Game/category name.
        /// </summary>
        public string name { get; set; }
    }
}

namespace MixItUp.Base.Model.Twitch.Schedule
{
    /// <summary>
    /// Infomration for a broadcater's scheduled vacation
    /// </summary>
    public class ScheduleVacationModel
    {
        /// <summary>
        /// Start time for vacation specified in RFC3339 format.
        /// </summary>
        public string start_time { get; set; }
        /// <summary>
        /// End time for vacation specified in RFC3339 format.
        /// </summary>
        public string end_time { get; set; }
    }
}

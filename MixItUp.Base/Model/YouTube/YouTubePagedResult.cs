using System.Collections.Generic;

namespace MixItUp.Base.Model.YouTube
{
    /// <summary>
    /// Information about the specific page of results.
    /// </summary>
    public class YouTubePagedResultInfo
    {
        /// <summary>
        /// The total results.
        /// </summary>
        public int totalResults { get; set; }
        /// <summary>
        /// The results for this page.
        /// </summary>
        public int resultsPerPage { get; set; }
    }

    /// <summary>
    /// A paged result of YouTube resources.
    /// </summary>
    public class YouTubePagedResult<T> : YouTubeModelBase
    {
        /// <summary>
        /// The token for the next page results.
        /// </summary>
        public string nextPageToken { get; set; }
        /// <summary>
        /// The token for the previous page results.
        /// </summary>
        public string prevPageToken { get; set; }
        /// <summary>
        /// Information about the page.
        /// </summary>
        public YouTubePagedResultInfo pageInfo { get; set; }
        /// <summary>
        /// The result items.
        /// </summary>
        public List<T> items { get; set; }
    }
}

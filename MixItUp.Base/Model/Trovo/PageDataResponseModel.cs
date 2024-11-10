namespace MixItUp.Base.Model.Trovo
{
    /// <summary>
    /// A paged response.
    /// </summary>
    public abstract class PageDataResponseModel
    {
        /// <summary>
        /// The page instance token.
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// The total number of pages.
        /// </summary>
        public int total_page { get; set; }
        /// <summary>
        /// The total number of pages.
        /// </summary>
        public int total { get; set; }
        /// <summary>
        /// The current page number.
        /// </summary>
        public int cursor { get; set; }

        /// <summary>
        /// Gets the count of items associated with the response.
        /// </summary>
        /// <returns>The count of items associated with the response</returns>
        public abstract int GetItemCount();
    }
}

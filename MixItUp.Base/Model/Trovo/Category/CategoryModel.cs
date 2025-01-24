using System.Collections.Generic;

namespace MixItUp.Base.Model.Trovo.Category
{
    /// <summary>
    /// Information about a category.
    /// </summary>
    public class CategoryModel
    {
        /// <summary>
        /// The ID of the category.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// The full name of the category.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The short name of the category.
        /// </summary>
        public string short_name { get; set; }

        /// <summary>
        /// The url of the category's icon.
        /// </summary>
        public string icon_url { get; set; }

        /// <summary>
        /// The description of the category.
        /// </summary>
        public string desc { get; set; }
    }

    public class CategoriesModel
    {
        public List<CategoryModel> category_info { get; set; }
    }
}

using MixItUp.Base.Model.Trovo.Category;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo.API
{
    /// <summary>
    /// The APIs for category-based services.
    /// </summary>
    public class CategoryService : TrovoServiceBase
    {
        private class CategoryWrapperModel
        {
            public List<CategoryModel> category_info { get; set; }
        }

        /// <summary>
        /// Creates an instance of the CategoryService.
        /// </summary>
        /// <param name="connection">The Trovo connection to use</param>
        public CategoryService(TrovoConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the top categories.
        /// </summary>
        /// <returns>The top categories</returns>
        public async Task<IEnumerable<CategoryModel>> GetTopCategories()
        {
            CategoryWrapperModel categories = await this.GetAsync<CategoryWrapperModel>("categorys/top");
            if (categories != null)
            {
                return categories.category_info;
            }
            return null;
        }

        /// <summary>
        /// Searches for categories matching the specified query.
        /// </summary>
        /// <param name="query">The query to search for</param>
        /// <param name="maxResults">The maximum number of results. Will be either that amount or slightly more</param>
        /// <returns>The matching categories</returns>
        public async Task<IEnumerable<CategoryModel>> SearchCategories(string query, int maxResults = 1)
        {
            Validator.ValidateString(query, "query");

            JObject jobj = new JObject();
            jobj["query"] = query;
            jobj["limit"] = maxResults;

            CategoryWrapperModel categories = await this.PostAsync<CategoryWrapperModel>("searchcategory", AdvancedHttpClient.CreateContentFromObject(jobj));
            if (categories != null)
            {
                return categories.category_info;
            }
            return null;
        }
    }
}

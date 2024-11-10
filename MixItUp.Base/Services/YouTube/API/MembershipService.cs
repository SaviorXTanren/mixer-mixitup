using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.YouTube.API
{
    /// <summary>
    /// The APIs for Membership-based services.
    /// </summary>
    public class MembershipService : YouTubeServiceBase
    {
        /// <summary>
        /// Creates an instance of the MembershipService.
        /// </summary>
        /// <param name="connection">The YouTube connection to use</param>
        public MembershipService(YouTubeConnection connection) : base(connection) { }

        /// <summary>
        /// Gets the membership levels defined for the currently authenticated account.
        /// </summary>
        /// <returns>The membership levels</returns>
        public async Task<IEnumerable<MembershipsLevel>> GetMyMembershipLevels()
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                MembershipsLevelsResource.ListRequest request = this.connection.GoogleYouTubeService.MembershipsLevels.List("id,snippet");
                LogRequest(request);

                MembershipsLevelListResponse response = await request.ExecuteAsync();
                LogResponse(request, response);
                return response.Items;
            });
        }

        /// <summary>
        /// Gets the members for the currently authenticated account.
        /// </summary>
        /// <param name="maxResults">The maximum results to return</param>
        /// <returns>The list of members</returns>
        public async Task<IEnumerable<Member>> GetMembers(int maxResults = 1)
        {
            return await this.GetMembersInternal(maxResults: maxResults);
        }

        /// <summary>
        /// Gets the member information for the specified channel to the currently authenticated channel.
        /// </summary>
        /// <param name="memberChannelId">The channel to check if they are a member</param>
        /// <returns>The membership, if it exists</returns>
        public async Task<Member> CheckIfMember(string memberChannelId)
        {
            var memberships = await this.GetMembersInternal(filterToSpecificMemberChannelId: memberChannelId, maxResults: 1);
            if (memberships != null)
            {
                return memberships.FirstOrDefault();
            }
            return null;
        }

        internal async Task<IEnumerable<Member>> GetMembersInternal(bool onlyUpdates = false, string filterToSpecificLevel = null, string filterToSpecificMemberChannelId = null, int maxResults = 1)
        {
            return await this.YouTubeServiceWrapper(async () =>
            {
                List<Member> results = new List<Member>();
                string pageToken = null;
                do
                {
                    MembersResource.ListRequest request = this.connection.GoogleYouTubeService.Members.List("snippet");
                    if (onlyUpdates)
                    {
                        request.Mode = MembersResource.ListRequest.ModeEnum.Updates;
                    }
                    if (!string.IsNullOrEmpty(filterToSpecificLevel))
                    {
                        request.HasAccessToLevel = filterToSpecificLevel;
                    }
                    if (!string.IsNullOrEmpty(filterToSpecificMemberChannelId))
                    {
                        request.FilterByMemberChannelId = filterToSpecificMemberChannelId;
                    }

                    request.MaxResults = Math.Min(maxResults, 50);
                    request.PageToken = pageToken;
                    LogRequest(request);

                    MemberListResponse response = await request.ExecuteAsync();
                    LogResponse(request, response);
                    results.AddRange(response.Items);
                    maxResults -= response.Items.Count;
                    pageToken = response.NextPageToken;

                } while (maxResults > 0 && !string.IsNullOrEmpty(pageToken));
                return results;
            });
        }
    }
}

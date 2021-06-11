using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Store;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class CommunityCommandsService
    {
        private List<CommunityCommandDetailsModel> commandCache = new List<CommunityCommandDetailsModel>();

        public CommunityCommandsService()
        {
            foreach (string name in HitmanGameCommandModel.DefaultWords)
            {
                this.commandCache.Add(this.GenerateTestCommand(name));
            }
        }

        public async Task<IEnumerable<CommunityCommandCategoryModel>> GetHomeCategories()
        {
            await Task.Delay(1000);

            List<CommunityCommandCategoryModel> categories = new List<CommunityCommandCategoryModel>();

            categories.Add(new CommunityCommandCategoryModel()
            {
                Name = "Top Commands",
                Description = "The best of the best from the community, find it all here!",
                Commands = new List<CommunityCommandModel>(this.commandCache.Skip(0).Take(10))
            });

            categories.Add(new CommunityCommandCategoryModel()
            {
                Name = "Latest Stuff",
                Description = "Want to see what's up-and-coming in the community? Look no further!",
                Commands = new List<CommunityCommandModel>(this.commandCache.Skip(10).Take(10))
            });

            categories.Add(new CommunityCommandCategoryModel()
            {
                Name = "Only The Cool Kids",
                Description = "The coolest stuff to try out; only for the hip kids.",
                Commands = new List<CommunityCommandModel>(this.commandCache.Skip(20).Take(10))
            });

            categories.Add(new CommunityCommandCategoryModel()
            {
                Name = "Lights, Camera, Action!",
                Description = "Bring your stream to a whole new level with all types of stream automation!",
                Commands = new List<CommunityCommandModel>(this.commandCache.Skip(30).Take(10))
            });

            categories.Add(new CommunityCommandCategoryModel()
            {
                Name = "Spooky Times!",
                Description = "Get ready to be scared, these commands will keep you on the edge of our seat!",
                Commands = new List<CommunityCommandModel>(this.commandCache.Skip(40).Take(10))
            });

            return categories;
        }

        public async Task<IEnumerable<CommunityCommandModel>> SearchCommands(string searchText)
        {
            await Task.Delay(1000);

            return this.commandCache.Where(c => c.Name.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) ||
                c.Description.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) ||
                c.Tags.Any(t => string.Equals(EnumLocalizationHelper.GetLocalizedName(t), searchText, StringComparison.InvariantCultureIgnoreCase)));
        }

        public async Task<CommunityCommandDetailsModel> GetCommandDetails(Guid id)
        {
            await Task.Delay(1000);

            return this.commandCache.FirstOrDefault(c => c.ID.Equals(id));
        }

        public async Task<CommunityCommandDetailsModel> AddCommand(CommunityCommandDetailsModel command)
        {
            await Task.Delay(1000);

            command.ID = Guid.NewGuid();
            this.commandCache.Add(command);
            return command;
        }

        public async Task<CommunityCommandDetailsModel> UpdateCommand(CommunityCommandDetailsModel command)
        {
            await Task.Delay(1000);

            CommunityCommandDetailsModel existingCommand = this.commandCache.FirstOrDefault(c => c.ID.Equals(command.ID));
            if (existingCommand != null)
            {
                existingCommand.Name = command.Name;
                existingCommand.Description = command.Description;
                existingCommand.ImageURL = command.ImageURL;
                existingCommand.Tags = command.Tags;
                existingCommand.Username = command.Username;
                existingCommand.UserAvatarURL = command.UserAvatarURL;
                existingCommand.Data = command.Data;

                return existingCommand;
            }
            else
            {
                return await this.AddCommand(command);
            }
        }

        public async Task DeleteCommand(Guid id)
        {
            await Task.Delay(1000);

            CommunityCommandDetailsModel existingCommand = this.commandCache.FirstOrDefault(c => c.ID.Equals(id));
            if (existingCommand != null)
            {
                this.commandCache.Remove(existingCommand);
            }
        }

        public async Task ReportCommand(Guid id, string report)
        {
            await Task.Delay(1000);

            CommunityCommandDetailsModel existingCommand = this.commandCache.FirstOrDefault(c => c.ID.Equals(id));
            if (existingCommand != null)
            {
                // File report
            }
        }

        public async Task<IEnumerable<CommunityCommandDetailsModel>> GetMyCommands()
        {
            await Task.Delay(1000);

            return this.commandCache.ToList();
        }

        public async Task<CommunityCommandReviewModel> AddReview(CommunityCommandReviewModel review)
        {
            await Task.Delay(1000);

            CommunityCommandDetailsModel command = this.commandCache.FirstOrDefault(c => c.ID.Equals(review.CommandID));
            if (command != null)
            {
                review.ID = Guid.NewGuid();
                review.Username = "Joe Smoe";
                review.UserAvatarURL = "https://static-cdn.jtvnw.net/jtv_user_pictures/45182012-95d6-4704-9863-82ff3fbaf48e-profile_image-70x70.png";
                review.DateTime = DateTimeOffset.Now;

                command.Reviews.Add(review);

                command.AverageRating = command.Reviews.Average(r => r.Rating);

                return review;
            }
            return null;
        }

        public async Task DownloadCommand(Guid id)
        {
            await Task.Delay(1000);

            CommunityCommandDetailsModel command = this.commandCache.FirstOrDefault(c => c.ID.Equals(id));
            if (command != null)
            {
                command.Downloads++;
            }
        }

        private CommunityCommandDetailsModel GenerateTestCommand(string name)
        {
            CommunityCommandDetailsModel storeCommand = new CommunityCommandDetailsModel()
            {
                ID = Guid.NewGuid(),
                Name = name,
                Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Quis viverra nibh cras pulvinar mattis. At elementum eu facilisis sed odio morbi quis commodo. Malesuada fames ac turpis egestas. In pellentesque massa placerat duis ultricies. Porttitor massa id neque aliquam vestibulum. Lorem ipsum dolor sit amet consectetur adipiscing elit. Arcu non odio euismod lacinia at quis. Nunc mattis enim ut tellus elementum sagittis. Feugiat in fermentum posuere urna nec tincidunt praesent semper feugiat.",
                ImageURL = "https://appsgeyser.com/img/store_icon.png",
                Username = "Joe Smoe",
                UserAvatarURL = "https://static-cdn.jtvnw.net/jtv_user_pictures/45182012-95d6-4704-9863-82ff3fbaf48e-profile_image-70x70.png",
                Downloads = 1234,
            };

            foreach (CommunityCommandTagEnum tag in EnumHelper.GetEnumList<CommunityCommandTagEnum>().Shuffle().Take(5))
            {
                storeCommand.Tags.Add(tag);
            }

            for (int i = 1; i <= 5; i++)
            {
                storeCommand.Reviews.Add(new CommunityCommandReviewModel()
                {
                    ID = Guid.NewGuid(),
                    CommandID = storeCommand.ID,
                    Username = "Joe Smoe",
                    UserAvatarURL = "https://static-cdn.jtvnw.net/jtv_user_pictures/45182012-95d6-4704-9863-82ff3fbaf48e-profile_image-70x70.png",
                    Rating = i,
                    Review = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Quis viverra nibh cras pulvinar mattis. At elementum eu facilisis sed odio morbi quis commodo. Malesuada fames ac turpis egestas. In pellentesque massa placerat duis ultricies. Porttitor massa id neque aliquam vestibulum. Lorem ipsum dolor sit amet consectetur adipiscing elit. Arcu non odio euismod lacinia at quis. Nunc mattis enim ut tellus elementum sagittis. Feugiat in fermentum posuere urna nec tincidunt praesent semper feugiat.",
                    DateTime = DateTimeOffset.Now
                });
            }
            storeCommand.AverageRating = storeCommand.Reviews.Average(r => r.Rating);

            ChatCommandModel command = new ChatCommandModel(storeCommand.Name, new HashSet<string>() { "test" });
            command.Actions.Add(new ChatActionModel("Hello World!"));
            storeCommand.SetCommand(command);

            return storeCommand;
        }
    }
}

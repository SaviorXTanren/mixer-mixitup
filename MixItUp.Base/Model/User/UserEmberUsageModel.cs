using MixItUp.Base.ViewModel.User;

namespace MixItUp.Base.Model.User
{
    public class UserEmberUsageModel
    {
        public UserViewModel User { get; set; }

        public int Amount { get; set; }

        public string Message { get; set; }

        public UserEmberUsageModel(UserViewModel user, int amount, string message)
        {
            this.User = user;
            this.Amount = amount;
            this.Message = message;
        }
    }
}

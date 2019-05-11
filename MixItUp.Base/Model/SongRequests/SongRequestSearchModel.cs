using System.Collections.Generic;

namespace MixItUp.Base.Model.SongRequests
{
    public class SongRequestSearchModel
    {
        public SongRequestModel SongRequest { get; set; }

        public bool MultipleResults { get; set; }

        public SongRequestServiceTypeEnum Type { get; set; }
        public List<string> ErrorMessages { get; set; }

        public SongRequestSearchModel(SongRequestModel songRequest)
        {
            this.SongRequest = songRequest;
            this.Type = this.SongRequest.Type;
        }

        public SongRequestSearchModel(SongRequestServiceTypeEnum type, string errorMessage, bool multipleResults = false) : this(type, new string[] { errorMessage }, multipleResults) { }

        public SongRequestSearchModel(SongRequestServiceTypeEnum type, IEnumerable<string> errorMessages, bool multipleResults = false)
        {
            this.Type = type;
            this.ErrorMessages = new List<string>(errorMessages);
            this.MultipleResults = multipleResults;
        }

        public bool FoundSingleResult { get { return this.SongRequest != null; } }
    }
}

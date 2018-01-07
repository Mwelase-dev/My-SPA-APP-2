using System;
using System.Collections.Generic;

namespace Intranet.Models
{
    public class SuggestionModel : BaseModel
    {
        public virtual Guid     StaffId           { get; set; }
        public virtual Guid     SuggestionId      { get; set; }
        public virtual String   SuggestionSubject { get; set; }
        public virtual DateTime SuggestionDate    { get; set; }
        public virtual String   SuggestionText    { get; set; }

        public virtual ICollection<StaffSuggestionVotesModel> Votes { get; set; }

        public virtual List<StaffSuggestionFollower> SuggestionFollowers { get; set; }
        public virtual StaffModel Staff { get; set; }
        public virtual string SuggestionStatus { get; set; }
        public virtual string SuggestionResponse { get; set; }

        //Css
        public string SuggestionResponseClass
        {
            get
            {
                switch (SuggestionResponse)
                {
                    case "No response yet!":
                        return "no";

                    case "*":
                        return "yes";

                    default:
                        return "yes";
                }
            }
        }

    }

    public class StaffSuggestionVotesModel
    {
        public virtual Guid SuggestionID { get; set; }
        public virtual Guid StaffID { get; set; }
        public virtual string StaffComments { get; set; }
        public virtual DateTime StaffVoteDate { get; set; }
        public virtual int VoteType { get; set; }
        public virtual string RecordStatus { get; set; }

        //public virtual StaffModel StaffMember { get; set; }
        public virtual SuggestionModel Suggestion { get; set; }

    }

    public class StaffSuggestionFollower
    {
        public virtual Guid SuggestionID { get; set; }
        public virtual Guid StaffID { get; set; }

       public virtual SuggestionModel Suggestion { get; set; } //you are not following the law
    }

}

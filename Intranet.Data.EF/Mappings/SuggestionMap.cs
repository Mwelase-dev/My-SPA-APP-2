using System.Data.Entity.ModelConfiguration;
using Intranet.Models;

namespace Intranet.Data.EF.Mappings
{
    public class SuggestionMap : EntityTypeConfiguration<SuggestionModel>
    {
        public SuggestionMap()
        {
            ToTable("tblStaffMembersSuggestions");
            HasKey(x => x.SuggestionId);
            Property(x => x.StaffId          ).HasColumnName("StaffID");
            Property(x => x.SuggestionSubject).HasColumnName("SuggestionSubject");
            Property(x => x.SuggestionText   ).HasColumnName("Suggestion"       );
            Property(x => x.SuggestionDate   ).HasColumnName("SuggestionDate"   );
            Property(x => x.RecordStatus     ).HasColumnName("RecordStatus"     );
            Property(x => x.SuggestionStatus).HasColumnName("SuggestionStatus");
            Property(x => x.SuggestionResponse).HasColumnName("SuggestionResponse");

            HasMany(x => x.SuggestionFollowers)
                .WithRequired(m => m.Suggestion)
                .HasForeignKey(m => m.SuggestionID);

            HasMany(m => m.Votes)
              .WithRequired(m => m.Suggestion)
              .HasForeignKey(m => m.SuggestionID);

            HasRequired(m => m.Staff)
                .WithMany(m => m.Suggestions)
                .HasForeignKey(m => m.StaffId);
        }
    }

    public class StaffSuggestionVotesModelMap:EntityTypeConfiguration<StaffSuggestionVotesModel>
    {
        public StaffSuggestionVotesModelMap()
        {
            ToTable("tblStaffMembersSuggestionVotes");
            HasKey(x => new {x.SuggestionID, x.StaffID});

            Property(x => x.SuggestionID).HasColumnName("SuggestionID");
            Property(x => x.StaffID).HasColumnName("StaffID");
            Property(x => x.StaffComments).HasColumnName("StaffComments");
            Property(x => x.StaffVoteDate).HasColumnName("StaffVoteDate");
            Property(x => x.VoteType).HasColumnName("VoteType");
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");

           // HasRequired(m => m.Suggestion)
             //   .WithMany(m =>m.Votes)
               // .HasForeignKey(m =>m);
          //  HasRequired(m => m.StaffMember);

        }
    }
    public class StaffSuggestionFollowerMap : EntityTypeConfiguration<StaffSuggestionFollower>
    {
        public StaffSuggestionFollowerMap()
        {
            ToTable("tblSuggestionFollowers");
            HasKey(x => new {x.SuggestionID, x.StaffID});
            Property(x => x.SuggestionID).HasColumnName("SuggestionID");
            Property(x => x.StaffID).HasColumnName("StaffID");
        }
    }
}

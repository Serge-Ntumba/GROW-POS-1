using System.Collections.Generic;

namespace GROW_CRM.Models
{
    public class DietaryRestriction
    {
        //Constructor
        public DietaryRestriction()
        {
            DietaryRestrictionMembers = new HashSet<DietaryRestrictionMember>();
        }

        //Fields
        public int ID { get; set; }

        public string Restriction { get; set; }

        //Foreign Keys
        public int HealthIssueTypeID { get; set; }

        public HealthIssueType HealthIssueType { get; set; }

        //O:M Relationships
        public ICollection<DietaryRestrictionMember> DietaryRestrictionMembers { get; set; }
    }
}
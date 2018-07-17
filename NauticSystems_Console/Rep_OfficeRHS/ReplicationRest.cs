using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NauticSystemsConsole
{
    public class ReplicationRestHeaderFull
    {
        public ReplicationRestHeader RepRestHeader { get; set; }
        public List<ReplicationRestCrewMember> entries { get; set; }
    }

    public class ReplicationRestHeader
    {
        public int Id { get; set; }
        public string RepRef { get; set; }
        public string Created { get; set; }
        public string Status { get; set; }
        public string Updated { get; set; }
        public int Imo { get; set; }
    }

    public class ReplicationRestCrewMember 
    {
        public int ForeignID { get; set; }
        public string FirstName { get; set; }
        public string MiddleNames { get; set; }
        public string LastName { get; set; }
        public string Rank { get; set; }
        public string SignOn { get; set; }
        public string PlannedSignOff { get; set; }
        public string ActualSignOff { get; set; }
        public Boolean Active{ get; set; }

        public string Citizenship { get; set; }
        public string Passport { get; set; }
        public string PassportExpiry { get; set; }
        
        public List<ReplicationRestCrewWatch> Watches { get; set; }
        public List<ReplicationRestMasterComment> MasterComments { get; set; }
        public List<ReplicationRestNonConformance> NonConformances { get; set; }

        public List<ReplicationRestCrewDuty> RestCrewDutyLog { get; set; }

        public List<ReplicationRestCrewComment> CrewComments { get; set; }
        

    }

    public class ReplicationRestCrewWatch
    {

        public int ForeignID { get; set; }
        public string Created { get; set; }
        public Nullable<int> ReplacedBy { get; set; }
        public string Deleted { get; set; }
        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public int Type { get; set; }


    }

    public class ReplicationRestCrewComment : ReplicationRestMasterComment
    {
       

    }

    public class ReplicationRestMasterComment
    {
        //public bool AtPort { get; set; }

        public String Day { get; set; }
        public String Text { get; set; }
        public String Created { get; set; }
        public String Updated { get; set; }
        public String Deleted { get; set; }

    }

    public class ReplicationRestNonConformance
    {

        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public int Type { get; set; }
        public String Created { get; set; }
        public String Updated { get; set; }
        public String Deleted { get; set; }

    }

    public class ReplicationRestCrewDuty
    {
        public String SignedOn { get; set; }
        public String SignedOff { get; set; }
        public String Created { get; set; }
        public String Updated { get; set; }
    }

    public class ReplicationShipTimes
    {
        public string TookEffect { get; set; }
        public float Offset { get; set; }
    }

}
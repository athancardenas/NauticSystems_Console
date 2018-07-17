using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NauticSystemsConsole
{
    public class class_operationlist : class_rmslist
    {
        public int iID { get; set; }
        public string strCode { get; set; }
        public string strName { get; set; }
        private string _strShipTypeIds { get; set; }
        public string strShipTypeIds
        {
            get
            {
                return _strShipTypeIds;
            }
            set
            {
                _strShipTypeIds = value;
                ShipTypes = value.Split(',').Select(Int32.Parse).ToList();
            }
        }
        public List<int> ShipTypes { get; set; }
        public string strShipType { get; set; }
        public int iShipDept { get; set; }
        public string strShipDept { get; set; }
        public bool bHasChildren { get; set; }
    }




    public class class_tasklist : class_rmslist
    {
        public int iID { get; set; }
        public string strName { get; set; }
        public string strCode { get; set; }
        public string strOpsGroupCode { get; set; }
        public string strOpsGroup { get; set; }
        public bool bIsCritical { get; set; }
        public int iTaskIndex { get; set; }
        public bool bHasChildren { get; set; }
        public bool bFromShore { get; set; }
        public int iNSID { get; set; }

    }

    public class class_hazardlist : class_rmslist
    {
        public int iID { get; set; }
        public string strName { get; set; }
        public int iTypeId { get; set; }
        public string strType { get; set; }
        public bool bHasChildren { get; set; }
        public bool bFromShore { get; set; }
        public int iNSID { get; set; }
    }



    public class class_ralist : class_rmslist
    {
        public int iID { get; set; }
        public string strOpsGroupCode { get; set; }
        public string strOpsGroup { get; set; }
        public int iTaskID { get; set; }
        public string strTaskCode { get; set; }
        public string strTask { get; set; }
        public string strCode { get; set; }
        public string strStatus { get; set; }
        public int iIssueNo { get; set; }
        public int iRevNo { get; set; }
        public string strVersion { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime ApprovedDate { get; set; }
        public bool bIsApproved { get; set; }
        public string strPreparedBy { get; set; }
        public string strModifiedBy { get; set; }
        public string strApprovedBy { get; set; }
        public bool bFromShore { get; set; }
        public bool bHasChildren { get; set; }
        public int iNSID { get; set; }

        public List<class_rahazardlist> Hazards { get; set; }
        public List<int> Checklists { get; set; }
        public List<int> WorkPermits { get; set; }
    }




    public class class_raattachmentlist
    {
        public List<int> id { get; set; }
        public List<class_typelist> items { get; set; }
    }




    public class class_typelist
    {
        public int iID { get; set; }
        public string sType { get; set; }
    }


    public class class_rahazardlist
    {
        public class_rahazardlist()
        {
            RiskList = new List<class_rahazardrisklist>();
        }
        public int iID { get; set; }
        public int iRAID { get; set; }
        public int iHazardID { get; set; }
        public string strHazard { get; set; }
        public string strConsequence { get; set; }
        public string strProcRef { get; set; }
        public string strSafetyPrec { get; set; }
        public string strAddMeasures { get; set; }
        public string strByWhom { get; set; }
        public string ByWhen { get; set; }
        public string bFromShore { get; set; }

        public List<class_rahazardrisklist> RiskList { get; set; }
    }

    public class class_rahazardrisklist
    {
        public int iID { get; set; }
        public int iCategoryID { get; set; }
        public string strCategory { get; set; }
        public int iRiskIndex { get; set; }
        public int iSeverity { get; set; }
        public int iFrequency { get; set; }
        public string strValue { get; set; }
    }

    public abstract class class_rmslist { }

    public class class_rmsRepFull
    {
        public class_RepHeader RepHeader { get; set; }
        public List<class_rmsRepItem> entries { get; set; }
        public class_rmsRepFull()
        {
            entries = new List<class_rmsRepItem>();
        }
    }

    public class class_rmsRepItem
    {
        private rmsItemType _type;
        public rmsItemType type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                strType = value.ToString();
            }
        }
        public string strType { get; set; }
        public class_rmslist RepItemDetails { get; set; }
    }

    public class class_workplanlist : class_rmslist
    {
        public int iID { get; set; }
        public string strJob { get; set; }
        public DateTime JobDate { get; set; }
        public string strPreparedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public bool bIsApproved { get; set; }

        public string strApprovedBy { get; set; }

        public List<string> RACodes { get; set; }

        public List<CrewMember> Crew { get; set; }
        public List<class_rahazardlist> Hazards { get; set; }
        public List<int> Checklists { get; set; }
        public List<int> WorkPermits { get; set; }

        private new int iNSID { get; set; }
    }





    public class CrewMember
    {
        public int ID { get; set; }
        public Guid Guid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Rank { get; set; }

        public bool Active { get; set; }

    }

    public class Rank
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public string Code { get; set; }

    }


    public enum rmsItemType
    {
        OperationsGroup,
        Task,
        Hazard,
        ChecklistProcs,
        WorkPermit,
        RASheet,
        WorkPlan
    }

    public class HazardIdShipOffice
    {
        public int HazShip { get; set; }
        public int HazOffice { get; set; }
    }

    public class TaskListIdShipOffice
    {
        public int TaskListShip { get; set; }
        public int TaskListOffice { get; set; }
    }

    public class HazardId
    {
        public int Id { get; set; }

    }

    public class RmaAcknowledgement
    {
        public string repRef { get; set; }
    }


    public class class_checklistlist : class_rmslist
    {
        public int iID { get; set; }
        public string strName { get; set; }
        public string strCode { get; set; }
        public int iShipType { get; set; }
        public string strShipType { get; set; }
        public string strDocUrl { get; set; }
        public string strDocName { get; set; }
        public string strDocVersion { get; set; }
        public string strVersion { get; set; }
        public int iMajorVersion { get; set; }
        public int iMinorVersion { get; set; }
        public bool bHasChildren { get; set; }
        public int iShipIMO { get; set; }
        public string strShip { get; set; }
        public bool bIsApproved { get; set; }
        public bool bIsCurrentVersion { get; set; }

        public List<int> Tasks { get; set; }

        public DateTime UpdateDate { get; set; }
        public string KMSRepRef { get; set; }
    }

    public class class_workpermitlist : class_rmslist
    {
        public int iID { get; set; }
        public string strName { get; set; }
        public string strCode { get; set; }
        public string strDocUrl { get; set; }
        public string strDocName { get; set; }
        public string strDocVersion { get; set; }
        public string KMSRepRef { get; set; }
    }

    public class WorkPlanRAs
    {
        public int NMRAId { get; set; }
        public string RACode { get; set; }
    }



    /*  Acknowledgement from Office to Shipt */
        
    public class RMM_Acknowledgement
    {
        public string repref { get; set; }
        public List<RMM_RA_AckIds> RMM_RA_Ids { get; set; }
        public List<RMM_WP_AckIds> RMM_WP_Ids { get; set; } // This is for Work Plan Acknowledgement reciept
        public List<RMM_HazardsId> Hazard_Ids { get; set; }
        public List<RMM_TasksId> Task_Ids { get; set; }
        public List<RMM_CheckListsProcsId> Checklist_Ids { get; set; }


    }

    public class RMM_HazardsId
    {
        public int haz_NSID { get; set; }
        public int haz_NMID { get; set; }
    }

    public class RMM_TasksId
    {
        public int tas_NSID { get; set; }
        public int tas_NMID { get; set; }
    }

    public class RMM_RA_AckIds
    {
        public int RA_nmId { get; set; }
        public int RA_nsId { get; set; }

    }

    public class RMM_WP_AckIds
    {
        public int WP_nmId { get; set; }
        public int WP_nsId { get; set; }

    }

    public class RMM_CheckListsProcsId
    {
        public int chk_NSID { get; set; }
        public int chk_NMID { get; set; }
    }

}

using System.Runtime.Serialization;

namespace QuantConnect.TDAmeritrade.Domain.Enums
{
    public enum OrderStatusType
    {
        NoValue = 0,
        [EnumMember(Value = "AWAITING_PARENT_ORDER")]
        AwaitingParentOrder = 1,
        [EnumMember(Value = "AWAITING_CONDITION")]
        AwaitingCondition = 2,
        [EnumMember(Value = "AWAITING_MANUAL_REVIEW")]
        AwaitingManualReview = 3,
        [EnumMember(Value = "ACCEPTED")]
        Accepted = 4,
        [EnumMember(Value = "AWAITING_UR_OUT")]
        AwaitingurOut = 5,
        [EnumMember(Value = "PENDING_ACTIVATION")]
        PendingActivation = 6,
        [EnumMember(Value = "QUEUED")]
        Queued = 7,
        [EnumMember(Value = "WORKING")]
        Working = 8,
        [EnumMember(Value = "REJECTED")]
        Rejected = 9,
        [EnumMember(Value = "PENDING_CANCEL")]
        PendingCancel = 10,
        [EnumMember(Value = "CANCELED")]
        Canceled = 11,
        [EnumMember(Value = "PENDING_REPLACE")]
        PendingReplace = 12,
        [EnumMember(Value = "REPLACED")]
        Replaced = 13,
        [EnumMember(Value = "FILLED")]
        Filled = 14,
        [EnumMember(Value = "EXPIRED")]
        Expired = 15,
    }
}

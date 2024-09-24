using Dim.Entities.Enums;

namespace Dim.Entities.Extensions;

public static class ProcessStepTypeIdExtensions
{
    public static ProcessStepTypeId GetRetriggerStep(this ProcessStepTypeId processStepTypeId, ProcessTypeId processTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.CREATE_WALLET when processTypeId is ProcessTypeId.SETUP_DIM => ProcessStepTypeId.RETRIGGER_CREATE_WALLET,
            ProcessStepTypeId.CHECK_OPERATION when processTypeId is ProcessTypeId.SETUP_DIM => ProcessStepTypeId.RETRIGGER_CHECK_OPERATION,
            ProcessStepTypeId.GET_COMPANY when processTypeId is ProcessTypeId.SETUP_DIM => ProcessStepTypeId.RETRIGGER_GET_COMPANY,
            ProcessStepTypeId.GET_DID_DOCUMENT when processTypeId is ProcessTypeId.SETUP_DIM => ProcessStepTypeId.RETRIGGER_GET_DID_DOCUMENT,
            ProcessStepTypeId.CREATE_STATUS_LIST when processTypeId is ProcessTypeId.SETUP_DIM => ProcessStepTypeId.RETRIGGER_CREATE_STATUS_LIST,
            ProcessStepTypeId.SEND_CALLBACK when processTypeId is ProcessTypeId.SETUP_DIM => ProcessStepTypeId.RETRIGGER_SEND_CALLBACK,
            ProcessStepTypeId.CREATE_TECHNICAL_USER when processTypeId is ProcessTypeId.TECHNICAL_USER => ProcessStepTypeId.RETRIGGER_CREATE_TECHNICAL_USER,
            ProcessStepTypeId.GET_TECHNICAL_USER_DATA when processTypeId is ProcessTypeId.TECHNICAL_USER => ProcessStepTypeId.RETRIGGER_GET_TECHNICAL_USER_DATA,
            ProcessStepTypeId.GET_TECHNICAL_USER_SERVICE_KEY when processTypeId is ProcessTypeId.TECHNICAL_USER => ProcessStepTypeId.RETRIGGER_GET_TECHNICAL_USER_SERVICE_KEY,
            ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK when processTypeId is ProcessTypeId.TECHNICAL_USER => ProcessStepTypeId.RETRIGGER_SEND_TECHNICAL_USER_CREATION_CALLBACK,
            ProcessStepTypeId.DELETE_TECHNICAL_USER when processTypeId is ProcessTypeId.TECHNICAL_USER => ProcessStepTypeId.RETRIGGER_DELETE_TECHNICAL_USER,
            ProcessStepTypeId.SEND_TECHNICAL_USER_DELETION_CALLBACK when processTypeId is ProcessTypeId.TECHNICAL_USER => ProcessStepTypeId.RETRIGGER_SEND_TECHNICAL_USER_DELETION_CALLBACK,
            _ => throw new ArgumentOutOfRangeException(nameof(processStepTypeId), processStepTypeId, null)
        };

    public static ProcessStepTypeId GetStepForRetrigger(this ProcessStepTypeId processStepTypeId, ProcessTypeId processTypeId) =>
         processStepTypeId switch
         {
             ProcessStepTypeId.RETRIGGER_CREATE_WALLET when processTypeId is ProcessTypeId.SETUP_DIM => ProcessStepTypeId.CREATE_WALLET,
             ProcessStepTypeId.RETRIGGER_CHECK_OPERATION when processTypeId is ProcessTypeId.SETUP_DIM => ProcessStepTypeId.CHECK_OPERATION,
             ProcessStepTypeId.RETRIGGER_GET_COMPANY when processTypeId is ProcessTypeId.SETUP_DIM => ProcessStepTypeId.GET_COMPANY,
             ProcessStepTypeId.RETRIGGER_GET_DID_DOCUMENT when processTypeId is ProcessTypeId.SETUP_DIM => ProcessStepTypeId.GET_DID_DOCUMENT,
             ProcessStepTypeId.RETRIGGER_CREATE_STATUS_LIST when processTypeId is ProcessTypeId.SETUP_DIM => ProcessStepTypeId.CREATE_STATUS_LIST,
             ProcessStepTypeId.RETRIGGER_SEND_CALLBACK when processTypeId is ProcessTypeId.SETUP_DIM => ProcessStepTypeId.SEND_CALLBACK,
             ProcessStepTypeId.RETRIGGER_CREATE_TECHNICAL_USER when processTypeId is ProcessTypeId.TECHNICAL_USER => ProcessStepTypeId.CREATE_TECHNICAL_USER,
             ProcessStepTypeId.RETRIGGER_GET_TECHNICAL_USER_DATA when processTypeId is ProcessTypeId.TECHNICAL_USER => ProcessStepTypeId.GET_TECHNICAL_USER_DATA,
             ProcessStepTypeId.RETRIGGER_GET_TECHNICAL_USER_SERVICE_KEY when processTypeId is ProcessTypeId.TECHNICAL_USER => ProcessStepTypeId.GET_TECHNICAL_USER_SERVICE_KEY,
             ProcessStepTypeId.RETRIGGER_SEND_TECHNICAL_USER_CREATION_CALLBACK when processTypeId is ProcessTypeId.TECHNICAL_USER => ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK,
             ProcessStepTypeId.RETRIGGER_DELETE_TECHNICAL_USER when processTypeId is ProcessTypeId.TECHNICAL_USER => ProcessStepTypeId.DELETE_TECHNICAL_USER,
             ProcessStepTypeId.RETRIGGER_SEND_TECHNICAL_USER_DELETION_CALLBACK when processTypeId is ProcessTypeId.TECHNICAL_USER => ProcessStepTypeId.SEND_TECHNICAL_USER_DELETION_CALLBACK,
             _ => throw new ArgumentOutOfRangeException(nameof(processStepTypeId), processStepTypeId, null)
         };
}

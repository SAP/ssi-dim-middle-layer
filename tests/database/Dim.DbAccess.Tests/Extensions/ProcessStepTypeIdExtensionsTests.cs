using Dim.Entities.Enums;
using Dim.Entities.Extensions;
using FluentAssertions;
using Xunit;

namespace Dim.DbAccess.Tests.Extensions;

public class ProcessStepTypeIdExtensionsTests
{
    [Theory]
    [InlineData(ProcessStepTypeId.CREATE_WALLET, ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_CREATE_WALLET)]
    [InlineData(ProcessStepTypeId.CHECK_OPERATION, ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_CHECK_OPERATION)]
    [InlineData(ProcessStepTypeId.GET_COMPANY, ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_GET_COMPANY)]
    [InlineData(ProcessStepTypeId.GET_DID_DOCUMENT, ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_GET_DID_DOCUMENT)]
    [InlineData(ProcessStepTypeId.CREATE_STATUS_LIST, ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_CREATE_STATUS_LIST)]
    [InlineData(ProcessStepTypeId.SEND_CALLBACK, ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_SEND_CALLBACK)]
    [InlineData(ProcessStepTypeId.CREATE_TECHNICAL_USER, ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_CREATE_TECHNICAL_USER)]
    [InlineData(ProcessStepTypeId.GET_TECHNICAL_USER_DATA, ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_GET_TECHNICAL_USER_DATA)]
    [InlineData(ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK, ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_SEND_TECHNICAL_USER_CREATION_CALLBACK)]
    [InlineData(ProcessStepTypeId.DELETE_TECHNICAL_USER, ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_DELETE_TECHNICAL_USER)]
    [InlineData(ProcessStepTypeId.SEND_TECHNICAL_USER_DELETION_CALLBACK, ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_SEND_TECHNICAL_USER_DELETION_CALLBACK)]
    public void GetRetriggerStep(ProcessStepTypeId processStepTypeId, ProcessTypeId processTypeId, ProcessStepTypeId expected)
    {
        // Act
        var typeId = processStepTypeId.GetRetriggerStep(processTypeId);

        // Assert
        typeId.Should().Be(expected);
    }

    [Theory]
    [InlineData(ProcessStepTypeId.CREATE_WALLET, ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_CREATE_WALLET)]
    [InlineData(ProcessStepTypeId.CHECK_OPERATION, ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_CHECK_OPERATION)]
    [InlineData(ProcessStepTypeId.GET_COMPANY, ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_GET_COMPANY)]
    [InlineData(ProcessStepTypeId.GET_DID_DOCUMENT, ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_GET_DID_DOCUMENT)]
    [InlineData(ProcessStepTypeId.CREATE_STATUS_LIST, ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_CREATE_STATUS_LIST)]
    [InlineData(ProcessStepTypeId.SEND_CALLBACK, ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_SEND_CALLBACK)]
    [InlineData(ProcessStepTypeId.CREATE_TECHNICAL_USER, ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_CREATE_TECHNICAL_USER)]
    [InlineData(ProcessStepTypeId.GET_TECHNICAL_USER_DATA, ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_GET_TECHNICAL_USER_DATA)]
    [InlineData(ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK, ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_SEND_TECHNICAL_USER_CREATION_CALLBACK)]
    [InlineData(ProcessStepTypeId.DELETE_TECHNICAL_USER, ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_DELETE_TECHNICAL_USER)]
    [InlineData(ProcessStepTypeId.SEND_TECHNICAL_USER_DELETION_CALLBACK, ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_SEND_TECHNICAL_USER_DELETION_CALLBACK)]
    public void GetStepForRetrigger(ProcessStepTypeId expected, ProcessTypeId processTypeId, ProcessStepTypeId processStepTypeId)
    {
        // Act
        var typeId = processStepTypeId.GetStepForRetrigger(processTypeId);

        // Assert
        typeId.Should().Be(expected);
    }
}

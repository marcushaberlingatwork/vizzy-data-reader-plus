// See https://aka.ms/new-console-template for more information

using System.Net;

public class UserLoginRequestDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}
public class DataWrapper<x>
{
    public x Data { get; set; }
    public HttpStatusCode Code { get; set; }
    public string Message { get; set; }
    public bool IsGood { get => Code < HttpStatusCode.Ambiguous; }
}

public class AccessTokenInfo
{
    public string AccessToken { get; set; }
    public string TokenType { get; set; } // "bearer"
    public ulong ExpiresIn { get; set; } // Seconds
    public string RefreshToken { get; set; } //Currently Unused
}

public class DeviceDto
{
    public string Id { get; set; }

    /// <summary>
    /// The id of the model this device is associated with
    /// </summary>
    public string DeviceDefId { get; set; }
    public string OriginDeviceDefId { get; set; }

    /// <summary>
    /// The unique vizzy id for this device
    /// </summary>
    public string VizzyId { get; set; }

    /// <summary>
    /// The account this device is registered with
    /// </summary>
    public string UserId { get; set; }

    public string OrgId { get; set; }

    /// <summary>
    /// If this device is a type of device that relies on a gateway
    /// to communicate, then the gateway id is the id of the vizzy 
    /// device used to communicate with the vizzy backend
    /// </summary>
    public string GatewayId { get; set; }

    /// <summary>
    /// A customizable name for the device
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Any notes to associate with this device
    /// </summary>
    public string Notes { get; set; }

    /// <summary>
    /// The date the device was created in the system
    /// aka the date the device was provisioned
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// The date the devices was registered and associated with an account
    /// </summary>
    public DateTime? RegisteredOn { get; set; }

    /// <summary>
    /// How frequently (in minutes) the device is expected to check in
    /// </summary>
    public int CheckinInterval { get; set; }

    public bool AllowOnlyWhitelist { get; set; }

    public string FirmwareVersion { get; set; }


    /// <summary>
    /// Not mapped, only used to hold related resources in memory not stored with the document
    /// in the database
    /// </summary>
    public IList<DeviceResourceDto> Resources { get; set; }


    public string OrgCustomerId { get; set; }
    /// <summary>
    /// This is not a real device resource. Which is why it isnt in the list, not mapped to the model database object.
    /// </summary>
}

public class DeviceResourceDto
{
    public string Id { get; set; }
    public string DeviceId { get; set; }
    public string UserId { get; set; }
    public string OrgId { get; set; }
    public string IoId { get; set; }
    public DeviceResourceType Type { get; set; }
    public DeviceResourceDataType DataType { get; set; }
    public string Name { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public double LastValue { get; set; }
    public string StringValue { get; set; }
    public bool UserVisible { get; set; }
}
public class DeviceResourceDefDto
{
    public string Id { get; set; }
    public string IoId { get; set; }
    public string Group { get; set; }
    public string DefaultName { get; set; }
    public string Description { get; set; }
    public bool LogHistory { get; set; }
    public DeviceResourceType Type { get; set; }
    public DeviceResourceDataType DataType { get; set; }
    public double DefaultValue { get; set; }
    public string DefaultStringValue { get; set; }
    public string MapBooleanTrue { get; set; }
    public string MapBooleanFalse { get; set; }
    public string Measurement { get; set; }
    public string DisplayMeasurement { get; set; }
    /// <summary>
    /// Indicates if the resource is visible to end users in the UI 
    /// </summary>
    public bool UserVisible { get; set; }
    public string VisibleScript { get; set; }
    public string LatchScript { get; set; }
    public int Order { get; set; }
}
public class DeviceDefDto
{
    public string Id { get; set; }
    public IList<DeviceResourceDefDto> ResourceDefs { get; set; } = new List<DeviceResourceDefDto>();
}
public enum DeviceResourceType
{
    None = 0,
    Input = 1,
    Output = 2,
    Variable = 3,
    VirtualInput = 4,
}
public enum DeviceResourceDataType
{
    None = 0,
    Boolean = 1,
    Number = 2,
    String = 3,
    NullZeroNumber = 4, //A value of zero is equivalent to false or null
    Mask = 5, //A list of strings used for configurations
    Enum = 6 // An integer mapped to strings in the Mask Fields
}

public class  SelectDevice
{
    public string SelectIndex { get; set; }
    public string Name { get; set; }
    public string VizzyId { get; set; }
    public DeviceDto Device { get; set; }

}

public class  SelectedResource
{
    public string SelectIndex { get; set; }
    public string Name { get; set; }
    public DeviceResourceDto Resource { get; set; }
}

public class EventHistoryResultDto
{
    public string ResourceId { get; set; }
    public DeviceResourceEventDto[] EventHistory { get; set; }
    public DeviceResourceEventDto EndEvent { get; set; }
    public string PageToken { get; set; }

}

public class DeviceResourceEventDto
{
    public string ResourceId { get; set; }
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
}
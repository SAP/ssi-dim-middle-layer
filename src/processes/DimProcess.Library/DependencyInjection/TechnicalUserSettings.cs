using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace DimProcess.Library.DependencyInjection;

public class TechnicalUserSettings
{
    [Required]
    public int EncryptionConfigIndex { get; set; }

    [Required]
    [DistinctValues("x => x.Index")]
    public IEnumerable<EncryptionModeConfig> EncryptionConfigs { get; set; } = null!;
}

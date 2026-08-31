using AutoMapper;
using Marketplace.Application.Common.Mappings;

namespace Marketplace.Application.Tests.TestHelpers;

internal static class MappingHelper
{
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
            cfg.AddProfile<MappingProfile>());

        return config.CreateMapper();
    }
}

using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options.Interfaces;

namespace ATMS.Infrastructure.Options;

public class ImagesOptions : IOptions
{
    public string ImagesRootPath { get; set; }
    public string BaseImageUrl { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ImagesRootPath))
        {
            throw new ConfigurationException(ConfigurationErrorType.Images_RootPathNotFound, "Images: 'RootPath' not found .");
        }

        if (string.IsNullOrWhiteSpace(BaseImageUrl))
        {
            throw new ConfigurationException(ConfigurationErrorType.Images_BaseUrlNotFound, "Images: 'BaseImageUrl' not found .");
        }
    }
}

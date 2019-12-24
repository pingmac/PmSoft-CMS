using Microsoft.Extensions.Options;

namespace PmSoft.DBContext
{
    public class PetaPocoDbContextOptions 
    {
        public string ConnectionString { get; set; }
        public string ProviderName { get; set; }
    }
}

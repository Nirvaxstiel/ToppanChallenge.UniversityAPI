namespace UniversityAPI.Utility.Helpers.Filters
{
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        private int pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => pageSize;
            set => pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
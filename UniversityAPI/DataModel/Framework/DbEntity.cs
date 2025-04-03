namespace UniversityAPI.DataModel.Framework
{
    public class DbEntity
    {
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public DateTime? DeletedAt { get; set; }
    }
}

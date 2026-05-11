namespace OutsourcingApplication.Services.Interfaces
{
    public interface ITagService
    {
        void SaveTagRelations(int targetId, List<int>? tagIds, byte targetType);
        List<string> GetTagNames(int targetId, byte targetType);
        List<int> GetTagIds(int targetId, byte targetType);
        void SyncSkillsString(int targetId, byte targetType);
    }
}

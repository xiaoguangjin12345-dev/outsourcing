using OutsourcingApplication.DTOs;
using OutsourcingApplication.Models;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Services
{
    public class TagService : ITagService
    {
        private readonly OutsourcingDbContext _context;
        public TagService(OutsourcingDbContext context) => _context = context;

        // 保存标签
        public void SaveTagRelations(int targetId, List<int>? tagIds, byte targetType)
        {
            // 清理旧数据
            var old = _context.TagRelations.Where(r => r.TargetID == targetId && r.TargetType == targetType);
            if (old.Any())
            {
                _context.TagRelations.RemoveRange(old);
            }

            // 插入新数据
            if (tagIds != null && tagIds.Any())
            {
                var relations = tagIds.Select(id => new TagRelation
                {
                    TagID = id,
                    TargetID = targetId,
                    TargetType = targetType
                });
                _context.TagRelations.AddRange(relations);
            }
        }

        // 获取标签名称列表：直接返回字符串数组
        public List<string> GetTagNames(int targetId, byte targetType)
        {
            return (from r in _context.TagRelations
                    join d in _context.DictTags on r.TagID equals d.TagID
                    where r.TargetID == targetId && r.TargetType == targetType
                    select d.TagName).ToList();
        }

        // 获取标签id列表：直接返回id数组
        public List<int> GetTagIds(int targetId, byte targetType)
        {
            return _context.TagRelations
                .Where(r => r.TargetID == targetId && r.TargetType == targetType)
                .Select(r => r.TagID)
                .ToList();
        }

        // 根据标签关联信息，更新主表（User/Task）的Skills字符串字段
        public void SyncSkillsString(int targetId, byte targetType)
        {
            // 获取该目标当前关联的所有标签名称
            var names = (from r in _context.TagRelations
                         join d in _context.DictTags on r.TagID equals d.TagID
                         where r.TargetID == targetId && r.TargetType == targetType
                         select d.TagName).ToList();

            // 拼装字符串
            string skillsString = names.Any() ? string.Join(", ", names) : string.Empty;

            // 根据类型更新对应的实体
            if (targetType == 1) // 用户
            {
                var user = _context.Users.Find(targetId);
                if (user != null) user.Skills = skillsString;
            }
            else if (targetType == 2) // 任务
            {
                var task = _context.Tasks.Find(targetId);
                if (task != null) task.RequiredSkills = skillsString;
            }
        }
    }
}

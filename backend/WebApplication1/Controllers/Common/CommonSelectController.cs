using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OutsourcingApplication.DTOs.Common;
using OutsourcingApplication.Models;
using OutsourcingApplication.Models.Common;
using OutsourcingApplication.Services.Interfaces;

namespace OutsourcingApplication.Controllers.Common
{
    [Route("api/common")]
    [ApiController]
    public class CommonSelectController : BaseController
    {
        private readonly OutsourcingDbContext _context;
        public CommonSelectController(OutsourcingDbContext context)
        {
            _context = context;
        }
        // 获取通用分类字典
        [HttpGet("{type}/options")]
        public ApiResponse<List<SelectOptionDto>> GetCategories([FromRoute] string type)
        {
            var list = new List<SelectOptionDto>();

            switch (type)
            {
                // 用户角色
                case "user-role":
                    list.Add(new SelectOptionDto("1", "PMO"));
                    list.Add(new SelectOptionDto("2", "项目经理"));
                    list.Add(new SelectOptionDto("3", "开发人员"));
                    list.Add(new SelectOptionDto("4", "系统管理员"));
                    break;

                // 用户状态
                case "user-status":
                    list.Add(new SelectOptionDto("1", "待验证"));
                    list.Add(new SelectOptionDto("2", "已验证"));
                    list.Add(new SelectOptionDto("3", "未通过"));
                    break;

                // 项目状态
                case "project-status":
                    list.Add(new SelectOptionDto("1", "待审核"));
                    list.Add(new SelectOptionDto("2", "待修改"));
                    list.Add(new SelectOptionDto("3", "进行中"));
                    list.Add(new SelectOptionDto("4", "待结项"));
                    list.Add(new SelectOptionDto("5", "已归档"));
                    break;

                // 任务状态
                case "task-status":
                    list.Add(new SelectOptionDto("1", "待分配"));
                    list.Add(new SelectOptionDto("2", "进行中"));
                    list.Add(new SelectOptionDto("3", "待验收"));
                    list.Add(new SelectOptionDto("4", "已完成"));
                    break;

                // 任务申请状态
                case "app-status":
                    list.Add(new SelectOptionDto("1", "待处理"));
                    list.Add(new SelectOptionDto("2", "已同意"));
                    list.Add(new SelectOptionDto("3", "已失效"));
                    break;

                // 工时日志状态
                case "log-status":
                    list.Add(new SelectOptionDto("1", "可修改"));
                    list.Add(new SelectOptionDto("2", "只读"));
                    break;

                // 任务评审
                case "review-result":
                    list.Add(new SelectOptionDto("1", "待评审"));
                    list.Add(new SelectOptionDto("2", "通过"));
                    list.Add(new SelectOptionDto("3", "返工"));
                    break;

                // 消息通知类型
                case "notice-type":
                    list.Add(new SelectOptionDto("1", "系统通知"));
                    list.Add(new SelectOptionDto("2", "审核通知"));
                    list.Add(new SelectOptionDto("3", "申请通知"));
                    list.Add(new SelectOptionDto("4", "工时预警"));
                    list.Add(new SelectOptionDto("5", "验收通知"));
                    list.Add(new SelectOptionDto("6", "其他"));
                    break;

                // 消息通知状态
                case "notice-status":
                    list.Add(new SelectOptionDto("1", "未读"));
                    list.Add(new SelectOptionDto("2", "已读"));
                    list.Add(new SelectOptionDto("3", "已删除"));
                    break;

                // 绩效评价类型
                case "perf-type":
                    list.Add(new SelectOptionDto("1", "项目绩效"));
                    list.Add(new SelectOptionDto("2", "任务绩效"));
                    break;

                // 绩效状态
                case "pref-status":
                    list.Add(new SelectOptionDto("1", "未发布"));
                    list.Add(new SelectOptionDto("2", "已发布"));
                    break;
                
                // 技能标签
                case "tags":
                    // 查出所有可选标签
                    var tags = _context.DictTags
                        .Select(t => new SelectOptionDto(t.TagID.ToString(), t.TagName))
                        .ToList();
                    return ApiResponse<List<SelectOptionDto>>.Success(tags);

                default:
                    return ApiResponse<List<SelectOptionDto>>.Fail(404, "请求的字典类别不存在");
            }
            return ApiResponse<List<SelectOptionDto>>.Success(list);
        }
    }
}